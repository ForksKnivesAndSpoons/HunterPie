

<img src="https://cdn.discordapp.com/attachments/402557384209203200/735695965461151894/hunterpie_patreon_banner.png" Width="1200">

[![Discord](https://img.shields.io/discord/678286768046342147?color=7289DA&label=Discord&logo=discord&logoColor=white&style=flat-square)](https://discord.gg/5pdDq4Q)
[![NexusMods](https://img.shields.io/badge/Download-Nexus-white.svg?color=da8e35&style=flat-square&logo=nexusmods&logoColor=white)](https://www.nexusmods.com/monsterhunterworld/mods/2645)
[![Paypal](https://img.shields.io/badge/donate-Paypal-blue.svg?color=62b2fc&style=flat-square&label=Donate)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=F2QA6HEQZ366A&source=url)
[![Patreon](https://img.shields.io/badge/Support-Patreon-blue.svg?color=fc8362&style=flat-square&logo=patreon&logoColor=white)](https://www.patreon.com/HunterPie)

[![GitHub license](https://img.shields.io/github/license/Haato3o/HunterPie?color=c20067&style=flat-square)](https://github.com/Haato3o/HunterPie/blob/master/LICENSE)
[![GitHub stars](https://img.shields.io/github/stars/Haato3o/HunterPie?color=b440de&style=flat-square)](https://github.com/Haato3o/HunterPie/stargazers)


## About this fork

I maintain this fork because I want the automation. I keep this fork up to date with the parent repo by using the awesome github action [tgymnich/fork-sync@v1.2](https://github.com/marketplace/actions/fork-sync). Unfortunately, this will create merge commits that were never in the parent repo. I wouldn't normally care about this, but I'm trying to contribute... So I get around it like so:

- Branch `ci` might get stale, but is where I keep the CI addition at `HEAD`
  - Periodically, I'll run `fix-branches.ps1` to fetch upstream and `git reset --hard FETCH_HEAD`
  - `fix-branches.ps1` then cherry picks the top 2 commits from `ci` and puts them on `master` and `development`. Then I force push...
- Branch `dev` is kept clean. No CI commits. This is also synced by `fix-branches.ps1`.
  - Any contributions I make will branch out from `dev`

Hacky AF... I know. It's because GH actions have to be in the branches that trigger the workflow. Otherwise, I'd just have branch `ci` and keep `master` and `development` in sync via a cron rebase.

The branches are synced every 8 hours starting at 5am EST. This schedule is good enough for me. I have Dependabot keep my dependencies on HunterPie libs up to date.

For now, this is my versioning system:

- Whatever HunterPie version is, I push an annotated tag: `v.MAJOR.MINOR.BUILD-REVISION-rX`. i.e. `1.0.3.995` => `v1.0.3-995-r0`
  - Pushes to `master` will make a release named like this and append `.COMMITS_SINCE_ANNOTATED_TAG`. i.e. `v1.0.3-995-r0.5`
  - Pushes to `development` will do the same as `master`, but further append `-beta`. i.e. `v1.0.3-995-r0.5-beta`
  - `X` is the number of times I manually pushed a tag since HunterPie version was bumped. i.e. `v1.0.3-995-r1`
- Whatever the tag is for the trigger job will be the nuget package version for both HunterPie.Core and HunterPie.UI.

> Note: Until v1.0.4+, I'm pushing tags like `1.0.3.99r5-0` because Dependabot thinks 1.0.3.99r1 > 1.0.3.995. Hyphens don't cause this behavior. I know now for next time...

### TODO

- Don't push nuget package if lib didn't change...


## About HunterPie

HunterPie is a modern and simple to use overlay with support for Discord Rich Presence for Monster Hunter: World.

## How to install

#### Requirements

- [.NET Framework >= 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)

#### Installation

- Download the latest release [here](https://github.com/Haato3o/HunterPie/releases/latest);
- Extract it anywhere you want;
- Open the extracted folder and start the **HunterPie.exe**, it will automatically look for new updates.

#### Uninstallation

- Delete HunterPie folder

## Build instructions

If you want to build HunterPie by yourself, you might need:
- [Python](https://www.python.org/downloads/)
- [NuGet](https://www.nuget.org/downloads)

For the release build:

```bash
nuget restore HunterPie.sln
msbuild HunterPie.sln -property:Configuration=Release
```

The apps will be in _{HunterPie|Update}/bin/Release_

> **ATTENTION:** Don't forget to disable auto-update, otherwise your local build will be overwritten by the files in HunterPie's update server.

## Features

### Core
- Automatic updates
- [Build exporter to Honey Hunters World](https://hunterpie.haato.dev/?p=Integrations/honeyHuntersWorld.md)
- [Decoration & Charms exporter to Honey Hunters World](https://hunterpie.haato.dev/?p=Integrations/honeyHuntersWorld.md)
- [Automatic Player Data Exporter](https://hunterpie.haato.dev/?p=HunterPie/playerDataExporter.md)
- [Discord Rich Presence Support](https://hunterpie.haato.dev/?p=Integrations/discord.md)
- [Plugin Support](https://github.com/Haato3o/HunterPie.Plugins)

### Overlay
- [Monster Widget](https://hunterpie.haato.dev/?p=Overlay/monstersWidget.md)
- [Harvest Box Widget](https://hunterpie.haato.dev/?p=Overlay/harvestBoxWidget.md)
- [Specialized Tools Widget](https://hunterpie.haato.dev/?p=Overlay/specializedToolWidget.md)
- [Abnormalities Tracker Widget](https://hunterpie.haato.dev/?p=Overlay/abnormalitiesWidget.md)
- [Class Helper Widget](https://hunterpie.haato.dev/?p=Overlay/classesWidget.md)
- [Damage Meter Widget](https://hunterpie.haato.dev/?p=Overlay/damageMeterWidget.md)

## Troubleshooting & Bugs

Please, read the [FAQ](https://github.com/Haato3o/HunterPie/wiki/FAQ), if your issue isn't listed there, contact me on [Discord](https://discord.gg/5pdDq4Q).

## Suggestions & PRs

You can use the #suggestions chat in HunterPie Discord server, open a ticket [here](https://github.com/Haato3o/HunterPie/issues) or make your own pull request. I'll gladly read them all.

> **NOTE:** If you're making a pull request, please, point it to the **development** branch, not the master one.