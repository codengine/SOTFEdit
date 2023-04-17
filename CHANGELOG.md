# Changelog

## v0.9.2

### Features

- Added multilingual support and translations for german

### Fixes

- Some minor fixes and improvements

## v0.9.1

### Features

- Added feature to change a blueprint's type, which allows the convertion of any blueprint to any other inside bunkers
  and caves

## v0.9.0

### Features

- Added a brand new NPC page
- Added a brand new structures page, which allows you to finish/remove blueprints and as a side effect also lets you
  build inside bunkers and caves!
- Added sliders for Fullness, Hydration and Rest Buffs
- Added ability to change themes
- Improved backup system. You can now create zip files as backups, which is the default, and have more flexibility in
  configuring them
- Added buttons to fill bars for followers
- Added tool to reset consumed items
- Added tool to ignite and refuel all fires as well as lowering their fuel drain rate
- Added tool to reset structural damages
- Added tool to teleport to and clone world objects like Glider and Knight V
- Added a red pin to Coordinates which opens a zoomable map and displays their location
- Added a red pin to Storages which opens a zoomable map and displays their location
- Add in-app viewer for Readme and Changelog
- Display changelog if new version is available
- Added hotkeys to save (CTRL+S) and reload (F5) savegames
- Added Escape as hotkey to close most windows
- Added CTRL+Q as hotkey to close the application
- Added menu option to select the last opened savegame

### Improvements

- Filtering in the inventory panel now happens with a delay, which improves responsiveness
- Replaced normal message boxes with dialogs that are displayed within the application
- Improve loading performance
- Action Buttons on inventory page are now left-aligned
- All modifications (including reviving) are now only saved when you actually save, which removes the need to reload on
  things like reviving!
- Added button on savegame selection to select the default directory
- Improve responsiveness of some tabs which are resource-intense

### Fixes

- Wall Storages now only show 4 slots (although internally it has 5)
- Storage manager will now keep the state of fish and meat (cooked meat will stay cooked, dried fish will stay dried)
- Fixed a bug with player clothing for default outfit

## v0.8.5

### Fixes

- Reverted removal of follower stats. It appears that it depends on your game which stats are set
- Fix reviving of Kelvin and Virginia which is broken since Update 03
- Teleporting should not launch the player into the air anymore

## v0.8.4

### Fixes

- Removed follower stats that were removed in Patch 03
- Removed sliders from influences that were removed in Patch 03
- Fix saving of follower stats and influences

## v0.8.3

### Fixes

- Fixed a couple of cases where the +/- button did not work properly

## v0.8.2

### New Features

- Added support for armor rack
- Added items that were added in patch 3: light bulb, solar panel, night vision goggles
- Added some missing items, like small rocks, some documents and blueprints

### Fixes

- Fixed issue when loading savegames saved with Patch 3

## v0.8.1

### Fixes

- fixed duplicate items in unassigned window on inventory page after reload

## v0.8.0

### New Features

- add Outfit selector on Player page
- armor: add buttons to remove / set to default durability / set to max durability
- armor: add buttons to set all armor items to default/max durability
- inventory: add button to add all items from a category (weapon, ammo etc.)
- inventory: add button to remove/set item count to max (for all and single)
- inventory: add max count for items
- storage: add button to fill all storages
- storage: add max count for items

### Improvements

- inventory: instead of double click, items are now added and removed from inventory using buttons
- inventory: new items are now added with their max value by default
- storage: in the upper "all slots" box, the first available item is now selected by default
- storage: all items for mannequin and scarecrow are now hidden
- teleporting will now add a small offset on the Y-Axis to prevent glitching into the ground
- Some performance improvements

### Fixes

- Golden Armor is now an outfit, removed from Armor Box
- Remove Creepy Skin from Armor selector
- Items that can not be stored in inventory/shelves are now disabled

## v0.7.1

### Improvements

- bug reports now contain the application version (window + clipboard)
- you can now load savegames from any location

### Fixes

- fix follower equipment editing
- savegame selector was not working properly
- removed "creepy skin" from the armor page

## v0.7.0

### New Features

- added settings for enemy spawn
- added setting for consumable effects damage options
- added setting for survival damage option
- added setting for weather change frequency
- added setting to modify the crash location
- added settings for various World Object States, e.g. if Bunker Doors are open, events have happened etc.

### Improvements

- merge game setup, game state and weather into one page
- A window is now displayed on exceptions, giving you an opportunity to easily report the issue

### Fixes

- fixed saving of game settings and game setup
- Minimum Sentiment Value changed from 0 to -100

## v0.6.0

### New Features

- added tools to spawn an army of Kelvins and Virginias (at "Followers")
- improvements for reviving (e.g. they will now get the items and outfits that you have selected)
- added feature in menu to restore from oldest/newest backup

### Improvements

- check for update will now notify only once until the next version is released
- improved loading performance
- some code beautification, cleanups and refactorings

### Fixes

- fixed deletion of backups

## v0.5.2

- reviving a follower should now also work when the body is completely gone
- small improvements to followers inventory editing

## v0.5.1

- added tool at "Game State" to reset containers, crates and pickups in caves and open world

## v0.5.0

- add storage editing (unlimited logs, sticks etc.)
- add selection of Kelvin's and Virginia's outfit
- add editing of Virginias equipped items
- add editing of influences that the player / enemies have to your followers (e.g.: "Player" brings "Fear" to her if the
  value is high)
- add experiment to reset kill statistics
- add experiment to reset number of cut trees
- add experiment so that enemies fear the player (hopefully...)
- add experiment so that enemies have no fear and are very angry (hopefully...)
- add experiment to remove all actors and spawn points except for Kelvin and Virginia
- some cosmetical improvements
- add menu bar
- add option to open the currently selected savegame dir in Explorer
- the window title now changes dynamically after loading
- backup can now be toggled in menu
- add option to delete all *.bak* files in savegame dir
- add links to all sites where the editor is hosted
- add update check (checks automatically, can be turned off)
- move savegame selection to its own window, freeing space
- in the new savegame selection, the current directory is more prominent
- Savegames are now grouped by SinglePlayer, Multiplayer or MP_Client
- A lot of background improvements
- Removed locks for now, as everything does not need to be synchronized right now
- Downgrade MVVM Toolkit to 8.0.0 due to compilation issues

## v0.4.0

- add player tab, allowing editing of player stats as well as positioning
- move armor tab to player tab
- improve performance of savegame loading, also reducing memory consumption

## v0.3.1

- fix logging of exceptions during savegame loading
- fix non-uniqueness of savegame parent directories

## v0.3.0

- replace displaying of save-time instead of last-write-time, resolves #2
- the currently selected savegame now stays selected after saving
- add follower tab
    - allows changing Kelvin and Virginias stats
    - allows moving Kelvin and Virginia to the player or each other

## v0.2.1

- add detailed options to regrow trees instead of reviving all, resolves

## v0.2.0

- misc fixes
- add markers for non-inventory items
- better rendering for numeric columns
- add weather data page
- add game state data page
