# SOTFEdit - Sons of The Forest Savegame Editor

![Screenshot](https://abload.de/img/sotfeditfadso.jpg)

[![Build](https://github.com/codengine/SOTFEdit/actions/workflows/build.yaml/badge.svg)](https://github.com/codengine/SOTFEdit/actions/workflows/build.yaml)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/codengine/SOTFEdit)](https://github.com/codengine/SOTFEdit/releases)
[![GitHub all releases](https://img.shields.io/github/downloads/codengine/SOTFEdit/total)](https://github.com/codengine/SOTFEdit/releases)
![GitHub](https://img.shields.io/github/license/codengine/SOTFEdit)

A savegame editor for "Sons of The Forest".

[![Discord](https://abload.de/img/discordc7csi.png)](https://discord.gg/867UDYvvqE)

- [SOTFEdit - Sons of The Forest Savegame Editor](#sotfedit---sons-of-the-forest-savegame-editor)
    - [Disclaimer](#disclaimer)
    - [Features](#features)
    - [Download](#download)
    - [Requirements](#requirements)
    - [Usage](#usage)
    - [Inventory](#inventory)
    - [Storage](#storage)
    - [Armor](#armor)
    - [NPCs](#npcs)
    - [Structures](#structures)
    - [Weather](#weather)
    - [Reviving](#reviving)
    - [Spawning](#spawning)
    - [Troubleshooting](#troubleshooting)
    - [Contributing](#contributing)
    - [Final Words](#final-words)
    - [Links and Credits](#links-and-credits)

## Disclaimer

This project is in no way or form associated with the developers of the game. It is just a fan project, nothing more,
nothing less.

## Features

- Edit Player Stats (Strength, MaxHealth, CurrentHealth, Fullness etc.)
- Move Player to Virginia & Kelvin
- Edit Game Setup (Game Mode, Spawn Rate, Survival Damage etc.)
- Edit Inventory (Add/Remove items, change quantities)
- Edit Armor Data (Add Armor Pieces, change durability)
- Edit Weather Data (Weather, Seasons...)
- Edit Game State Data (Playtime, Doors open/closed, Bunkers opened/closed)
- Edit Storage Data (Unlimited Logs, sticks etc.)
- Edit Influences of Players towards Kelvin and Virginia
- Edit NPCs
- Edit Structures / Blueprints
- Theme Support
- Ignite and refuel all fires as well as lowering their fuel drain rate
- Reset Structural Damage
- Teleport to and clone World Objects like Glider and Knight V
- Reset consumed items
- Modify Virginias equipped items
- Change Virginias and Kelvins outfits
- Spawn an army of Virginias and Kelvins
- Several experiments
- Revive Virginia & Kelvin
- Set stats for Virginia & Kelvin
- Move Kelvin & Virginia to Player or each other
- Regrow Trees selectively (All, Removed, Half-Chopped, Stumps)
- Reset containers, crates and pickups in caves and open world
- Backup changed files automatically
- ... more features are planned

## Download

- You can find the newest version at the [Releases page](https://github.com/codengine/SOTFEdit/releases)

## Requirements

- Windows 7+ (I guess...)
- [.net 6.0+ Runtime](https://dotnet.microsoft.com/en-us/download/dotnet)

## Usage

- Start the application using SOTFEdit.exe
- Click on "File" -> "Open Savegame"
- Select your savegame
- Edit
- Save with "File" -> "Save"

## Inventory

- In order to add or remove items, click on the add or remove buttons
- You can use the other buttons to modify the current count in a convenient way

## Storage

- Only items that are supported by the storages are available. Solar Panels for example can not be stored there
- The editor will obey the maximum number of items per slot

## Armor

Armor protects you from most hazards. However you are still going to drown and die from fall damage.

## NPCs

The game groups some enemies into "families". Most likely they won't attack each other and raid the player.

One word about modifying NPCs: Some enemies are in caves/bunkers and have a different GraphMask.
When this option is selected (Only in the same area as this actor) only enemies in the same "phase" are edited.

So, if you just want to remove babies in caves, select this option. If you remove all of them, just deselect it.

Spawners: I haven't fully investigated how the game works with those spawners. When you delete them the game will
re-create them
on the next game save. So this would be one way to increase the number of enemies.

## Structures

This tool lets you "almost" finish blueprints, set them to "unfinished" or remove them.
Why "almost"? Because it is too complex to convert blueprints to final buildings. So this tool will change the
blueprints to require one last item until a blueprint is finished.

The cool thing about this is that you can use it in order to have an easy source for logs for example. Just set some
shelters, change them to "almost finished", take them down
and all logs will drop to the ground.

Another cool thing is that you can use it to build stuff in caves or bunkers. The process is the same: Just set the
blueprints, mark them as "almost finished", take them down and
use the resources to build new stuff.

## Weather

There is one very important thing. If you only change the season, it will be reverted immediately when the game
progresses.
To fix that, you also have to adjust the played time at "Game State". It is calculated based on the length of the
season. Here is an example:

- Starting Season: Spring
- Season Length: Long
- Played Time: 31 Days

The in-game season will be winter, because:

- Day 0-9 = Spring
- Day 10-19 = Summer
- Day 20-29 = Autumn
- Day 30-39 = Winter

So if you want to change the weather to Summer, you need to adjust the playtime days to something between 10 and 19.

Here is a list of number of days per season per season length setting:

- Short: ???
- Default (non-custom games): 5 days
- Long: 10 days
- Realistic: 90 days

## Reviving

If you want to revive either or both followers, there are some things to consider:

- If the body is completely missing, the follower will spawn at the players location
- Make sure to be outside of buildings, else you might glitch into the building sometimes
- The followers will be at maximum stats and should be friendly towards players
- They will have the items that you have selected before spawning
- Virginia is shy as a lamb when she was revived. I haven't tested it thoroughly but to me it appears that you have to
  regain her trust, like in the beginning
  of a session. I'm still trying to figure out which setting determines her trust.

## Spawning

This feature is quite experimental and allows the duplication of Kelvin and Virginia. It turns out that the game can not
spawn, for some reason, more than 5
Virginias. If the value is higher, they would spawn somewhere unreachable.

Kelvin was tested successfully with 20, so this is also the maximum now.

If you want to exceed this maximum, you would have to save, fire up the tool, spawn and load the game again.

```
Be careful: This feature will most likely kill your performance and may corrupt your savegame.
Make sure to enable backups!  
```

## Troubleshooting

One of the items in inventory is listed as "Unknown"?

- Please report the ItemId so that I can add it to the list of known items

My game does not work anymore?

- If you have selected to create backups before saving, you can just delete the old files and restore the files that are
  suffixed with ".bak*".

I get errors and the application does strange things

- Please upload any logs to https://pastebin.com and create an issue

I can not change "IsRobbyDead" or "IsVirginiaDead"

- In order to revive both there is a special button at "Followers" that does the job

## Contributing

Feel free to report any unknown items or any feature requests. PRs are also welcome.

## Final Words

Big thanks to [Gronkh](https://gronkh.tv) for your many years of "Influenz". Especially without your "The Forest"
streams I would have never known anything
about that game.

## Links and Credits

- [Kleine Axt icon by Icons8](https://icons8.com/icon/81685/kleine-axt)
