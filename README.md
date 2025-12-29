# SOTFEdit - Sons of The Forest Savegame Editor

![Screenshot](https://github.com/codengine/SOTFEdit/blob/master/SOTFEdit.jpg?raw=true)

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
    - [Map](#map)
    - [Weather](#weather)
    - [Reviving](#reviving)
    - [Spawning](#spawning)
    - [Troubleshooting](#troubleshooting)
    - [Contributing](#contributing)
    - [Final Words](#final-words)
    - [Links and Credits](#credits)
    - [Attributions](#attributions)
    - [Icons](#icons)

## Disclaimer

This project is in no way or form associated with the developers of the game. It is just a non-commercial fan project,
nothing more, nothing less.

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

- Windows 8+
- [.net 8.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Usage

- Start the application using SOTFEdit.exe
- Click on "File" -> "Open Savegame"
- Select your savegame
- Edit
- Save with "File" -> "Save"

## Inventory

- In order to add or remove items, click on the add or remove buttons
- You can use the other buttons to modify the current count in a convenient way
- Items that are (currently) not available for the inventory are hidden, but can easily be enabled in data.json
- When Legs and Arms are added they will default to their cooked variants. The same applies to already existing
  Arms/Legs when you just increase the count

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

If you want to build larger structures like shelters in a cave or bunker, you have to place a small blueprint first (
like a chair), then change its type and save.

## Map

The map combines static information about points-of-interest with dynamic information that are read directly from the
savegame.
Some positions are missing but they may be added in the future.

### Features

- Show/hide information about 3D Printers, Actors/NPCs, Ammunition, Bunkers, Camps, Cannibal Villages, Caves, Crates,
  Doors, Helicopters, Generic Information, Items, Lakes, Laptops, Player, Structures, Supplies and Villages
- Teleport the Player and Followers to Actors/NPCs, the player, Zip Lines, Structures, Caves, Bunkers, Helicopters
- Remove Zip Lines from the map
- Spawn Actors/NPCs at target areas

### Options

You can enable or disable icons using the "Options" button in the top left corner.

Some important notes regarding the filters:

- "Show only uncollected items" will show/hide uncollected items including bunkers and caves where they can be found
- Area - Mainly affects Actors or, in general terms, positions where we have the exact coordinates
- Requirements - Show/Hide caves, bunkers and items which are accessible/inaccessible

### Teleportation

You can only teleport to locations where we have the exact coordinates either from the savegame itself or attached to
the POI.

Kelvin and Virginia do not appear underground, so teleportation for them is disabled if the target location is
underground.

By default an offset is added to the target location. This is done so that you do not spawn inside an enemy which would
catapult you into the sky and most likely kill you.
You can adjust the target location using those numbers. X and Z represent the longitute/latitude and Y represents the
height.

### Spawning

Most enemies can be spawned at all areas. Virginia and Kelvin can only be spawned at the Surface. Please note that if
you spawn too many it may have a severe impact on your performance or even crash your game.

Moreover, the game will not spawn all enemies at the same time. If you spawn 200 enemies the game will create ~25
enemies and when they are killed, after some time, the next ~25 enemies will spawn.

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

The program does not start

- Make sure that .net 8.0 Desktop Runtime is installed. Also make sure to extract all files from the archive if you
  downloaded the zip archive manually. Lastly, check if any antivirus is blocking the editor

Antivirus (Windows Defender for example, Smartscreen) is complaining

- This is due to the fact that it is a self-developed application which is not signed. It's safe to just ignore the
  warning. The code is all hosted on Github.

"Could not load file or assembly"

- Make sure to have .net 8.0 Desktop Runtime installed (either x86 or x64)

My changes are not applied or reverted

- In some cases, the Cloud Saving Feature of Steam overwrites changes done by SOTFEdit. You can fix that if you start
  the Game (not a game session!), edit a savegame and THEN start the game session.

Lakes, rivers etc. are gone

- This happens after teleporting in and out of caves. This should be fixed when you teleport again in and out

## Contributing

Feel free to report any unknown items or any feature requests. PRs are also welcome.

## Final Words

Big thanks to [Gronkh](https://gronkh.tv) for your many years of "Influenz". Especially without your "The Forest"
streams I would have never known anything
about that game.

## Credits

- Translations and Corrections
    - Polski: Mortennif
    - German: Hinterix
- Supporters and Testers: Mortennif, M2THE49, feydrautha01

## Attributions

Icons used for items are property of Endnight Games.

POIs and screenshots originate from https://github.com/lmachens/sons-of-the-forest-map who really did a great job to
collect all data.

## Icons

- [Kleine Axt icon by Icons8](https://icons8.com/icon/81685/kleine-axt)
- [Birth icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/birth)
- [Bat icons created by Vitaly Gorbachev - Flaticon](https://www.flaticon.com/free-icons/bat)
- [Bird icons created by monkik - Flaticon](https://www.flaticon.com/free-icons/bird)
- [Duck icons created by smalllikeart - Flaticon](https://www.flaticon.com/free-icons/duck)
- [Eagle icons created by Flat Icons - Flaticon](https://www.flaticon.com/free-icons/eagle)
- [Fish icons created by VectorPortal - Flaticon](https://www.flaticon.com/free-icons/fish)
- [Bird icons created by Mihimihi - Flaticon](https://www.flaticon.com/free-icons/bird)
- [Letter k icons created by icon_small - Flaticon](https://www.flaticon.com/free-icons/letter-k)
- [Letter v icons created by icon_small - Flaticon](https://www.flaticon.com/free-icons/letter-v)
- [Shaman icons created by Smashicons - Flaticon](https://www.flaticon.com/free-icons/shaman)
- [Moose icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/moose)
- [Orca icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/orca)
- [Question mark icons created by Fathema Khanom - Flaticon](https://www.flaticon.com/free-icons/question-mark)
- [Bunny icons created by Mihimihi - Flaticon](https://www.flaticon.com/free-icons/bunny)
- [Seagull icons created by surang - Flaticon](https://www.flaticon.com/free-icons/seagull)
- [Shark icons created by BZZRINCANTATION - Flaticon](https://www.flaticon.com/free-icons/shark)
- [Squirrel icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/squirrel)
- [Turtle icons created by BZZRINCANTATION - Flaticon](https://www.flaticon.com/free-icons/turtle)
- [Crosshair icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/crosshair)
- [Hunt icons created by Darius Dan - Flaticon](https://www.flaticon.com/free-icons/hunt)
- [Bunker icons created by Smashicons - Flaticon](https://www.flaticon.com/free-icons/bunker)
- [Cave icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/cave)
- [Laptop icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/laptop)
- [Tent icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/tent)
- [Village icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/village)
- [Helicopter icons created by Konkapp - Flaticon](https://www.flaticon.com/free-icons/helicopter)
- [Signaling icons created by Smashicons - Flaticon](https://www.flaticon.com/free-icons/signaling)
- [Hiking icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/hiking)
- [Door icons created by kerismaker - Flaticon](https://www.flaticon.com/free-icons/door)
- [Art and design icons created by Hilmy Abiyyu A. - Flaticon](https://www.flaticon.com/free-icons/art-and-design)
- [Loot icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/loot)
- [Crate icons created by lapiyee - Flaticon](https://www.flaticon.com/free-icons/crate)
- [Shipping and delivery icons created by Ida Desi Mariana - Flaticon](https://www.flaticon.com/free-icons/shipping-and-delivery)
- [Bullet icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/bullet)
- [Native american icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/native-american)
- [Pond icons created by Mihimihi - Flaticon](https://www.flaticon.com/free-icons/pond)
- [Wood icons created by Nikita Golubev - Flaticon](https://www.flaticon.com/free-icons/wood)
- [Shelter icons created by Muhammad_Usman - Flaticon](https://www.flaticon.com/free-icons/shelter)
- [Grow icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/grow)
- [Glider icons created by Pop Vectors - Flaticon](https://www.flaticon.com/free-icons/glider)
- [Monowheel icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/monowheel)
- [Pole dance icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/pole-dance)
- [Golf icons created by Good Ware - Flaticon](https://www.flaticon.com/free-icons/golf)
- [Bookmark icons created by Ian Anandara - Flaticon](https://www.flaticon.com/free-icons/bookmark)
- [Soldier icons created by ppangman - Flaticon](https://www.flaticon.com/free-icons/soldier)
- [Fireplace icons created by Vector Squad - Flaticon](https://www.flaticon.com/free-icons/fireplace)
- [Golf cart icons created by surang - Flaticon](https://www.flaticon.com/free-icons/golf-cart)
- [Gun icons created by Nikita Golubev - Flaticon](https://www.flaticon.com/free-icons/gun)
- [Furnace icons created by amonrat rungreangfangsai - Flaticon](https://www.flaticon.com/free-icons/furnace)
- [Couch icons created by Stockio - Flaticon](https://www.flaticon.com/free-icons/couch)
- [Raccoon icons created by Icongeek26 - Flaticon](https://www.flaticon.com/free-icons/raccoon)
- [Dangerous icons created by Smashicons - Flaticon](https://www.flaticon.com/free-icons/dangerous)
- [Teleportation icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/teleportation)
- [Wood icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/wood)
- [Radio icons created by Eucalyp - Flaticon](https://www.flaticon.com/free-icons/radio)
- [Skunk icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/skunk)
- [Raft icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/raft)
- [Unicycle icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/unicycle)
- [Bird house icons created by Smashicons - Flaticon](https://www.flaticon.com/free-icons/bird-house)
- [Mooring icons created by Freepik - Flaticon](https://www.flaticon.com/free-icons/mooring)
