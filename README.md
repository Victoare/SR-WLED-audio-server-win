# SoundReactive WLED audio streamer for windows

Feed windows audio output (in a processed form) to sound reactive wled instances without huge and complicated apps. Not targeting vast configuration options, just the bare minimum to achieve this:

[![Watch the video](https://img.youtube.com/vi/uMCMXIF_DOo/hqdefault.jpg)](https://www.youtube.com/embed/uMCMXIF_DOo)

Right now this is merely a proof of concept running in the console.
The final form for this project would be an easily installable windows service.

## Libraries
- https://github.com/naudio/NAudio - for capturing the audio
- https://github.com/swharden/FftSharp - for the blazing fast FFT

## Links
Some usefull links:

**SoundReactive installers:**
- https://github.com/MoonModules/WLED/releases 
- https://wled-install.github.io/ 

**WLED-sync**
- https://github.com/netmindz/WLED-sync
- https://mm.kno.wled.ge/WLEDSR/UDP-Sound-Sync/

**Usefull info**
- https://github.com/zak-45/WLEDAudioSync-Chataigne-Module/blob/main/WLEDAudioSync.js - packet format (missing padding bytes)
