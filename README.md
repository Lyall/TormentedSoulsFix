# NOTICE
## This fix has been archived as it no longer works on the latest update.<br />I recommend using this much better fix [avaiable here](https://github.com/p1xel8ted/UltrawideFixes/releases/tag/TormentedSouls) by [p1xel8ted](https://github.com/p1xel8ted).

# Tormented Souls Fix
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/W7W01UAI9)</br>
[![Github All Releases](https://img.shields.io/github/downloads/Lyall/TormentedSoulsFix/total.svg)](https://github.com/Lyall/TormentedSoulsFix/releases)

This BepInEx plugin for the game Tormented Souls adds support for:
- Playing the game in any ultrawide aspect ratio such as 21:9, 32:9 or even higher.
- Setting an arbitrary resolution.
- Increasing the FOV.
- Vert+ FOV at aspect ratios narrower than 16:9 (such as the Steam Deck).
- Skipping the intro logos.
- Increasing the physics update rate.

## Installation
- Grab the latest release of TormentedSoulsFix from [here.](https://github.com/Lyall/TormentedSoulsFix/releases)
- Extract the contents of the release zip in to the game directory.<br />(e.g. "**steamapps\common\Tormented Souls**" for Steam.
- Run the game once to generate a config file located at **GameDirectory\BepinEx\config\TormentedSoulsFix.cfg**

### Linux
- If you are running Linux (for example with the Steam Deck) then the game needs to have it's launch option changed to load BepInEx.
- You can do this by going to the game properties in Steam and finding "LAUNCH OPTIONS".
- Make sure the launch option is set to: ```WINEDLLOVERRIDES="winhttp=n,b" %command%```

| ![steam launch options](https://user-images.githubusercontent.com/695941/179568974-6697bfcf-b67d-441c-9707-88cd3c72a104.jpeg) |
|:--:|
| Steam launch options. |

## Configuration
- See the generated config file to adjust various aspects of the plugin.

## Known Issues
Please report any issues you see and I'll do my best to fix them.

## Screenshots
| ![ezgif-2-36a8ff90e6](https://user-images.githubusercontent.com/695941/183559690-e0c2dbbe-9c6e-4ab5-852a-fcd46723bf25.gif) |
|:--:|
| Ultrawide pillarbox removal. | 

| ![ezgif-2-95901ccc9c](https://user-images.githubusercontent.com/695941/183560702-573c9980-fb0c-4a01-99f0-0c95959e50ba.gif) |
|:--:|
| Vert+ FOV at narrower than 16:9. | 

## Credits
- [BepinEx](https://github.com/BepInEx/BepInEx) is licensed under the GNU Lesser General Public License v2.1.
