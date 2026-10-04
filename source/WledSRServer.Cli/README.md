# WledSRServer.Cli

Headless Linux console build of the audio → FFT/beat → WLED UDP sync pipeline. It shares
its audio-processing and networking code with the Windows GUI app (`WledSRServer.csproj`)
via linked source files, but is its own plain `net10.0` project because the Windows project
is hardcoded to `net10.0-windows7.0`/`win-x64` and can never produce a Linux binary.

This exists mainly to run this app's audio processing side by side with a real WLED
device, both fed the same signal (this machine's line-out, cabled into WLED's line-in),
so the two implementations' output can be compared.

## How it captures audio on Linux

There's no WASAPI/loopback equivalent on Linux, so capture shells out to `parec`
(PulseAudio's client tools, also served by PipeWire's `pipewire-pulse` compatibility
layer) against a monitor source - i.e. it taps an **output**, the same way the Windows
app's default "Loopback (system output)" does. It does not capture from real inputs
(mics, line-in ADCs); that's what the real WLED device is doing on its own line-in, not
something this app needs to do too.

Requires `pactl`/`parec` on the host (Debian/Ubuntu: `pulseaudio-utils`).

## Building and running

### With the devcontainer (no local .NET SDK needed)

```bash
docker build -t wled-sr-proto -f .devcontainer/Dockerfile .

docker run --rm --network host \
  -v "$(pwd)/source:/workspace/source" \
  -v "${XDG_RUNTIME_DIR}/pulse:/run/pulse-host" \
  -e PULSE_SERVER=unix:/run/pulse-host/native \
  -w /workspace/source/WledSRServer.Cli \
  wled-sr-proto \
  dotnet run -c Release -- --target-ip=192.168.1.50
```

- Mounting `${XDG_RUNTIME_DIR}/pulse` and setting `PULSE_SERVER` gives the container
  access to the host's real PulseAudio/PipeWire socket - without this, `parec` has
  nothing to connect to.
- `--network host` is needed for `parec`'s device negotiation and, more importantly,
  because WLED devices are only reachable if the container shares the host's network
  (the default Docker bridge network won't route broadcast/multicast/UDP to your LAN).
- Alternatively, open this repo in VS Code and use "Reopen in Container" - the mount and
  `PULSE_SERVER` env var are already set up in `.devcontainer/devcontainer.json`.
  The socket is mounted at a fixed path, so it doesn't depend on your UID.

### With a local .NET 10 SDK

```bash
dotnet run --project source/WledSRServer.Cli -- --target-ip=192.168.1.50
```

## CLI options

All options are `--key=value`. Anything not passed keeps its default (see
`Properties/Settings.settings` in the main project) - defaults are **not persisted**
between runs; each invocation starts fresh plus whatever flags you pass.

| Option | Effect |
|---|---|
| `--help` / `-h` | Prints the option list and exits. |
| `--list-devices` | Lists outputs that can be tapped (see below), then exits without capturing. |
| `--device=<id>` | Which output's monitor to capture. Omit for the default output (`@DEFAULT_MONITOR@`, follows the system default, picked up when capture restarts). Use an id from `--list-devices` to pin a specific output. |
| `--target-ip=<ip[,ip...]>` | Send packets directly to these IP(s) instead of broadcasting. |
| `--broadcast-ip=<ip[,ip...]>` | Send packets to these broadcast address(es) (subnet broadcast mode). |
| `--udp-port=<port>` | UDP port (must match the WLED module's configured SR port; default 11988). |
| `--fft-low=<hz>` / `--fft-high=<hz>` | FFT bucket frequency range (default 40-10000Hz, matching WLED's usual range). |

With no `--target-ip`/`--broadcast-ip`, it falls back to the default send mode
(broadcast on the whole LAN).

Press Ctrl+C to stop; it shuts down the capture and network threads cleanly.

### Listing devices

```
$ dotnet run --project source/WledSRServer.Cli -- --list-devices
Available audio devices (use with --device=<id>):
  (default)                                                    Loopback (system output, follows the default on capture restart)
  alsa_output.pci-0000_00_1f.3.analog-stereo.monitor            Monitor of Built-in Audio Analog Stereo
  alsa_output.usb-Some_USB_DAC.analog-stereo.monitor             Monitor of Some USB DAC
```

Only lists monitor sources (things you can tap as an output), never mics/line-in inputs -
if your line-out to WLED isn't the system default output, use the matching id here as
`--device=<id>`.

## Known gaps

- No live default-output-change notification on Linux (WASAPI gives the Windows app this
  for free via `AudioDeviceEventWatcher`); if the default output changes mid-run, restart
  the process to pick it up.
- Settings aren't persisted to a config file from the CLI - every run is driven purely by
  its command-line flags plus built-in defaults.
