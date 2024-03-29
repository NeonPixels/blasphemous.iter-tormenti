<p align="center">  
  <h1 align="center">Iter Tormenti</h1><!-- <img src="logo.png"> -->
  <h2 align="center">-Path of Torment-</h2>

[![en](https://img.shields.io/badge/lang-en-red.svg)](https://github.com/NeonPixels/blasphemous.iter-tormenti/blob/main/README.md)
[![es](https://img.shields.io/badge/lang-es-yellow.svg)](https://github.com/NeonPixels/blasphemous.iter-tormenti/blob/main/README.es.md)

</p>

---

<p align="center">
  <img src="https://img.shields.io/github/v/release/NeonPixels/blasphemous.iter-tormenti">
  <img src="https://img.shields.io/github/last-commit/NeonPixels/blasphemous.iter-tormenti?color=important">
  <img src="https://img.shields.io/github/downloads/NeonPixels/blasphemous.iter-tormenti/total?color=success">
</p>

---

## Table of Contents

- [Links](https://github.com/NeonPixels/blasphemous.iter-tormenti#links)
- [Installation](https://github.com/NeonPixels/blasphemous.iter-tormenti#installation)<!-- - [Available commands](https://github.com/NeonPixels/blasphemous.iter-tormenti#available-commands) -->
- [Mod info](https://github.com/NeonPixels/blasphemous.iter-tormenti#mod-info)
  - [Current features](https://github.com/NeonPixels/blasphemous.iter-tormenti#current-features)
  - [Planned features](https://github.com/NeonPixels/blasphemous.iter-tormenti#planned-features)
  - [Important notes](https://github.com/NeonPixels/blasphemous.iter-tormenti#important-notes)
- [Credits](https://github.com/NeonPixels/blasphemous.iter-tormenti#credits)

---

## Links

- Discord: [Blasphemous General Server](https://discord.gg/Blasphemous)

<!-- [![how-to](https://img.shields.io/badge/how--to-use-blue.svg)](https://github.com/NeonPixels/blasphemous.iter-tormenti/blob/master/HOW-TO.md) -->
---

## Installation

Mod Installer:
- The mod can be installed via the [Blasphemous Modding Installer](https://github.com/BrandenEK/Blasphemous.Modding.Installer)

Manual installation:
1. Check the requirements for the latest release of the mod from the [Releases](https://github.com/NeonPixels/blasphemous.iter-tormenti/releases) page
2. Download the required release of the [Modding API](https://github.com/BrandenEK/Blasphemous-Modding-API/releases)
3. Follow the instructions there on how to install the API, take note of the location of the `Modding` folder
4. Download the required release of the [Penitence Framework](https://github.com/BrandenEK/Blasphemous.Framework.Penitence/releases)
5. Extract the contents of the `PenitenceFramework.zip` file into the `Modding` folder
6. Download the required release of the [Level Framework](https://github.com/BrandenEK/Blasphemous.Framework.Levels/releases)
7. Extract the contents of the `LevelFramework.zip` file into the `Modding` folder
8. Download the latest release of the mod from the [Releases](https://github.com/NeonPixels/blasphemous.iter-tormenti/releases) page
9. Extract the contents of the `IterTormenti.zip` file into the `Modding` folder

Manual removal:
Remove the following files and folders from the `Modding` folder:
- `plugins\IterTormenti.dll`
- `data\Iter Tormenti\`
- `levels\Iter Tormenti\`
- `localitazion\Iter Tomenti.txt`

<b>Note:</b> When manually updating the mod to a new version, it is recommended to manually remove the previous mod files first, as files that might have been removed from the mod release won't be removed automatically.

---

<!--
## Available commands
- Press the `backslash` key to open the debug console
- Type the desired command followed by the parameters all separated by a single space

| Command | Parameters | Description |
| ------- | ----------- | ------- |
| `itertormenti help` | none | List all available commands |

---
-->
## Mod info

Iter Tormenti (Path of Torment) is a modification (mod) for [Blasphemous](https://thegamekitchen.com/blasphemous/), meant for those who want to experience all that the game has to offer in a single playthrough, without needing to ascend and restart in True Torment mode.

### Current features

- Empty Save Slots can be ascended, thus beginning the game directly in True Torment mode, granting access to the Amanecidas questline, as well as the Penitence altar. Beware, as this mode presents a considerable challenge.
- At the Penitece Altar, one of the following new penitences can be selected:
  - `Penitence of the Bleeding Faith`: Combines the effects and rewards of the Bleeding Heart and the Unwavering Faith penitences.
  - `Penitence of the Guilty Heart`: Combines the effects and rewards of the Bleeding Heart and the True Guilt penitences.
  - `Penitence of the Unwavering Guilt`: Combines the effects and rewards of the Unwavering Faith and the True Guilt penitences.
  - `Penitence of the Path of Torment`: Combines the effects and rewards of all three of the basic penitences.
- Completing a combined penitence will also award completion of the penitences involved.
- The Petrified Bell can now be obtained in a non-ascended playthrough, thus enabling the Amanecidas questline. Beware, as the Amanecidas are balanced for True Torment mode, and will pose a significant challenge in a non-Ascended game.

<b>Note:</b> The current location of the Petrified Bell is a placeholder. It will be moved to a different location upon final mod release.

### Planned features

- Ability to, when in possesion of the Incomplete Scapular, choose to fight Esdras.

### Important notes

- Only works on the most current game version: `4.0.67`

## Credits

- Coding help & inspiration - [BrandenEK](https://github.com/BrandenEK)
