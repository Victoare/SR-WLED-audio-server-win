FROM mcr.microsoft.com/dotnet/sdk:8.0

# pulseaudio-utils provides `parec`/`pactl`, used to prototype loopback
# capture against the host's PulseAudio/PipeWire-pulse socket.
RUN apt-get update \
    && apt-get install -y --no-install-recommends pulseaudio-utils \
    && rm -rf /var/lib/apt/lists/*
