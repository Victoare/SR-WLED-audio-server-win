FROM mcr.microsoft.com/dotnet/sdk:10.0

# pulseaudio-utils provides `parec`/`pactl`, used by the Linux CLI to capture
# loopback audio from the host's PulseAudio/PipeWire-pulse socket.
RUN apt-get update \
    && apt-get install -y --no-install-recommends pulseaudio-utils \
    && rm -rf /var/lib/apt/lists/*
