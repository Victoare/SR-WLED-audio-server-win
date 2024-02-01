# SoundReactive WLED audio streamer for windows

## Synopsis

Feed windows audio output (in a processed form) to sound reactive wled instances without huge and complicated apps. Not targeting vast configuration options, just the bare minimum to achieve this:

[![Watch the video](https://img.youtube.com/vi/uMCMXIF_DOo/hqdefault.jpg)](https://www.youtube.com/embed/uMCMXIF_DOo)

## The app itself

![Main form](\assets\Screenshots\SysTray.png)
Probably, for most of the time you will see this little guy next to the clock telling you the server is running. From here you can open the main form or stop and exit the server.
By default the main form will open when the app starts.

![Main form](\assets\Screenshots\Screenshot_3.png)
This is the main form where you can check if the capture is working (bars-a-jumpin' for the music you listen and the capture box is green) and set some values.

If you one of the lucky few, who waited for this exact moment with well prepared WLED instances, non problematic network setup and music bangin' from your PC, then it will probably work out of the box. 
But otherwise, fear not, because the **Settings** button will open up the bottom part (it is hidden by default).

For most convenient usage you can set to **Start with windows** and the app will start after login. Mind you, to set this, the app may require elevated permissions (it will ask if needed)
You can check **Start without GUI** and the app will only appear in the system tray.

**FFT range** will set the frequency range for the processing. By default is 20-20000hz but you can widen or tighten the range.

**SR Port** is the same port you have to set up in your WLED configuration page. By default this is 11988.

In WLED you have to enable the AudioReactive module, the sync interface and set the Mode to Receive. Open the Info window and check if it is turned on (the power icon should be green). If everything is working, then you will see there: "*Audio Source : UDP sound sync - receiving*" and "*UDP Sound Sync : receive mode v2*"
If you only see "*Audio Source : UDP sound sync - idle*" it means, the wled is waiting for the audio data.

If the bars are jumping the *Packet per second* shows a nonzero value, WLED is properly configured but still not dancing then try to set the **Local IP** to the IP you machine have in the network. If you start to type, it will autosuggest IPs it found on the network adapters.

If the app started with the main form open then when you close the form with the "X" you will receive a notification telling you that the app will run in the background.

**Stop server and exit** is a mysterious button. :D

That's all folks, I hope it will run smoothly :)

## "Install"

Download the latest version from [releases](/releases) and put it somewhere on your machine. If you set it to auto run and move the exe somewhere else, you have to re-enable it.
The app is a portable one, without any fancy installer, but it needs .NET 8.0 runtime. If you don't have it already installed then at the start you will be prompted to do so (giving you the link to the .NET installer)

## Libraries / assets
- https://github.com/naudio/NAudio - for capturing the audio
- https://github.com/swharden/FftSharp - for the blazing fast FFT
- https://github.com/Aircoookie/Akemi - Icon for the app

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
