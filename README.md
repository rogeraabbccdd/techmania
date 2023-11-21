# TECHMANIA
An open source rhythm game, written in Unity, playable with or without a touchscreen.

[Download for Windows](https://github.com/techmania-team/techmania/releases)

[Download for iOS/iPadOS](https://apps.apple.com/us/app/techmania/id1581524513)

[Trailer](https://www.youtube.com/watch?v=MtkxhEmCWwU)

[Discord](https://discord.gg/K4Nf7AnAZt)

[Official website](https://techmania-team.herokuapp.com/)

## Licensing
All code and assets are released under the [MIT License](LICENSE), with the following exceptions:
* Sound effects in [TECHMANIA/Assets/Sfx](TECHMANIA/Assets/Sfx) and [TECHMANIA/Assets/UI/SFX](TECHMANIA/Assets/UI/SFX) are acquired from external resources, which use different licenses. Refer to [TECHMANIA/Assets/Sfx/Attributions.md](TECHMANIA/Assets/Sfx/Attributions.md) and [TECHMANIA/Assets/UI/SFX/Attributions.md](TECHMANIA/Assets/UI/SFX/Attributions.md) for details. Please note that some licenses prohibit commercial use.
* Some included tracks in the releases are under separate licenses:

|Track name|License|
|-|-|
|f for fun|[CC BY-NC-ND 4.0](https://creativecommons.org/licenses/by-nc-nd/4.0/)|
|Yin-Yang Specialist (MUG ver)|[CC BY-NC-NA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/)|
|v (Game Mix)|[CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/)|
|TAKE OUT|[CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/)|
|Run 4 Cover|[CC BY-NC-ND 4.0](https://creativecommons.org/licenses/by-nc-nd/4.0/)|
|Ash Barrens|[CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/)|

## Roadmap and progress
Refer to the [Kanban](https://github.com/techmania-team/techmania/projects/1).

## Manual and documentation
Refer to the [documentation website](https://techmania-team.github.io/techmania-docs/).

## Platform
The game is designed for Windows PCs, with the Touch control scheme requiring a touchscreen monitor. Patterns using other control schemes are playable with a mouse and keyboard.

We also provide technical support for iOS/iPadOS, in that we are able to respond to bug reports and some feature requests. However please be aware of the following:

- The game's UI is designed for PC and may be difficult to navigate on a phone;
- The game's difficulty is tuned for PC and may prove too difficult for mobile players;
- Some features in the included editor require mouse and keyboard, and therefore are unavailble to mobile users.

[Builds for other platforms](#platform-specific-forks) also exist, but are not officially supported at the moment.

## Content policy
Per the MIT license, you are free to produce any Content with TECHMANIA, including but not limited to screenshots, videos and livestreams. Attributions are appreciated, but not required. However, please keep the following in mind:
* If your Content features 3rd party music, it may be subject to copyright claims and/or takedowns. You may not hold TECHMANIA responsible for the resulting losses.
* If your Content is publicly available and features any unofficial [skin](https://github.com/techmania-team/techmania-docs/blob/main/English/Skins.md) or theme, you must clearly state so in the description of your Content, to avoid potential confusion.
* If your Content is commercial, additional limitations apply:
  * Your Content cannot feature any [official tracks](#licensing).
  * Your Content cannot feature the [Fever sound effect](TECHMANIA/Assets/UI/SFX/Fever.wav), which is a part of the default theme. You can produce Content from a different theme.

## Feedback
For technical issues, read the [contribution guidelines](CONTRIBUTING.md), then submit them to [Issues](https://github.com/techmania-team/techmania/issues).

For general discussions, head to [Discord](https://discord.gg/K4Nf7AnAZt).

## Making your own builds
It's not clearly documented whether we can distribute FMOD for Unity in an open source project, so to be on the safe side, we DO NOT INCLUDE FMOD for Unity in this repo, and ask you to acquire your own FMOD license and plugin. They are [free](https://fmod.com/licensing) for developers with less than $200k revenue per year.
* Go to [fmod.com](http://fmod.com) and register an account, if you don't already have one.
* Go to [Download](https://fmod.com/download#fmodforunity) and download "FMOD for Unity". You should receive a file named `fmodstudio20219.unitypackage`.

To build TECHMANIA:
* Install Unity, making sure your Unity version matches this project's. Check the project's Unity version at [ProjectVersion.txt](TECHMANIA/ProjectSettings/ProjectVersion.txt).
* Clone this repo, then open it from Unity. You may need to open it in safe mode, as the code will not compile without FMOD.
* In the menu, click Assets - Import Package - Custom Package, then choose the `fmodstudio20219.unitypackage` you downloaded earlier.
* At any point if the FMOD Setup Wizard popups up, you can simply dismiss it.
* Close and reopen the project, it should now be buildable. Go to File - Build Settings.
* Choose your target platform, then build.

Please note that the default skins are not part of the project either, so you'll need to copy the `Skins` folder from an official release into your build folder, in order for your build to be playable. Alternatively, set up [streaming assets](#on-streaming-assets) in your local clone.

If the build fails or produces a platform-specific bug, you can submit an issue, but we do not guarantee support.

## On streaming assets
In PC builds we release the base game and resources (official tracks and skins) separately so PC players don't need to redownload resources when updating. On mobile builds, however, it's more beneficial to include the resources in the release so the installation process is easier. To achieve this, we take advantage of [streaming assets](https://docs.unity3d.com/Manual/StreamingAssets.html).

In order to keep this repo focused, the streaming assets folder (`Assets/StreamingAssets`) is ignored in `.gitignore`. To set up streaming assets in your local clone:
* Create the directory `Assets/StreamingAssets`.
* Download `Skins_and_Tracks.zip` from an official release.
* Extract everything in `Skins_and_Tracks.zip` into `Assets/StreamingAssets`.

If done correctly, you should see official tracks and skins in the game even when they are not in the build folder.

## Platform-specific forks
* rogeraabbccdd's iOS & Android builds: https://github.com/rogeraabbccdd/techmania/releases
* MoonLight's Android builds: https://github.com/yyj01004/techmania/releases
* fhalfkg's macOS builds: https://github.com/fhalfkg/techmania/releases
* samnyan's Android build on 0.2: https://github.com/samnyan/techmania/releases
