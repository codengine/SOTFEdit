# Changelog

## v0.11.10

### Features

- Add Large Log Holder
- Add Deer Hide Rug

## v0.11.9

### Bugfixes

- Fixed that multiplayer saves where not detected anymore

## v0.11.8

### Bugfixes

- Fix bug at saving ("Additional text encountered after finished reading JSON")
    - The previous version might have corrupted your savegame. In that case, you have to restore the original savegame
      from a backup

## v0.11.7

### Features

- Added new trap: Radio Alert Trap

### Bugfixes

- Added support for changes in Savegame System (Savegames are now compressed by the game)

## v0.11.6

### Features

- Added new enemy: Mutant Virginia

### Bugfixes

- Fixed crash because of strange actors without a position on the map

## v0.11.5

Update 08 Changes

### Features

- Added new enemy: Armsy
- Added new structure: Hokey Pokey Trap
- Added new item: Printed GPS Case

## v0.11.4

### Bugfixes

- Fixed maximum rifle ammo in inventory
- Added rifle ammo to storage selection

## v0.11.3

### Features

- Added support for new Boss Mutant (Spawn & NPCs)

### Bugfixes

- Fixed issue with cloned world items being displayed as unknown

## v0.11.2

This release reflects the changes introduced in Update 07.

### Features

- Added support for new items (rifle, rifle ammo, head trophies, personal note a-c)
- Added support for golf carts at map and world item cloner/teleporter (teleportation only)
- Added teleport object-to-player for world items (glider, knightV, golf cart)
- Added map POIs and screenshots for rifle and personal note a-c
- Added map POIs and screenshots for new lakes (LakeEA - LakeEG)
- Added map POIs and screenshots for new ponds (LakePFA - LakePFD)
- Added support for new stone holder (Structures and Storages)
- Added support for new head trophy holder (Structures and Storages)

### Improvements

- Updated both maps (bright, dark) to reflect the newly added lakes, ponds and walkways
- Updates icons of structures (map)
- Hard Survival now defaults to "inventory pause on" (aligns with changes in Update 07)

## v0.11.1

Due to a change in the data model for log, stick, bone and stone holders, older savegames might be incompatible.
Please save the game first using the current game version before you use this version of the editor.

### Features

- Added new items: Stone Fireplace Blueprint, Stone (the bigger ones used for constructions)
- Added support for Hard Survival mode (you can switch around freely if you wish)
- Added support for changes to the Advanced Log Sled
- Added Button for storages to apply the current item setup to all storages of the same type

### Improvements

- Renamed "Log Sled" to "Advanced Log Sled"
- Change data model of log, stick, bone and stone holders to a simplified one

### Fixes

- Fix issue if a game setting bool value is null

## v0.11.0

The biggest change by far in this release is the integration directly into the game and displaying the position of the
player, Kelvin and Virginia and in Multiplayer-Games - the position of the other players.

Moreover, this also allows the teleportation to any point where we have exact coordinates (x,y,z) ingame without having
to leave the game.

All you need is a plugin. You can find the instructions at "Companion -> Setup".

### Features

Companion (only works while connected)

- Added integration with a game mod that allows player, follower and multiplayer live tracking on the map
- Added live teleportation for the player, Kelvin and Virginia without having to leave the game to any point with exact
  coordinates (x,y,z)
- Added Custom POIs with Screenshots that you can create and sync to SOTFEdit via an ingame menu
- If the connection to the companion is established, there will be a "plug" icon at the titlebar of the map

Map

- Updated map with new ponds and lakes
- Added a darker version of the regular map, with more details
- The selection of POI types will now be saved and restored when you close and reopen the map
- Added option to follow the player while connected to the Companion (-> titlebar of the map)
- Added option to keep the map at the foreground so that it can be used like an overlay (-> titlebar of the map)
- Added Slider at the bottom to control the zoom level

Other

- Added new option at "Game Setup" which controls if the game pauses while in the inventory
- Ziplines can now be extended indefinitely and at any distance by clicking one anchor point and then "Add new from
  here"

### Bugfixes

- Fixed critical bug where ziplines were accidentally removed from the map if structures were modified
- Complete rewrite of the "Game Setup" tab, which should now work reliably
    - One major change is that if you switch the Game Mode, the "invalid" settings are removed. So, "Custom" to "
      Peaceful" will also disable enemy spawning for example.
- Rewrote Zipline Management so that it now works reliably
- Fixed the tool to "Lit Fires" so that fires will now really burn
- Fixed teleportation to positions on the raft

### Improvements

- Changed and added new icons to menu items
- To identify the rotation of the players and followers while connected, the icons now include a small arrow
- Removed "Hard Survival" and "Creative" Mode for now as it has not been implemented yet
- The map can now be opened independently of the main window, saving and editing is disabled though while the map is
  open
- The map will now zoom and scroll faster
- The map is not maximized by default anymore
    - If you notice a decrease in performance in games while the map is open, just shrink and maximize the map
- Removed some buttons from the top left of the map to have less distraction and more room

## v0.10.4

## Features

- Update 05 support
- Updated Cooking Pot (it can be stored on shelves)
- Added Space Suit
- Added new outfits for Kelvin and Virginia
- Added POIs and Screenshots for the new Cooking Pots and Space Suit
- Added support for Log Sleds and Basic Log Sleds at Structure and Storage Tab

## v0.10.3

### Bugfixes

- Fixed player area detection

## v0.10.2

### Features

- Added a couple hundred new teleport locations
- Added hundreds new/fixed item locations
- Added over one hundred new screenshots
- All POIs are now assigned to the correct area
- Added human-readable Actor States (e.g. Sleeping, Hiding in Bushes, Despawned etc.)
- Added a new splash screen while loading and about window (hope you like it :))
- Added full-text search on the map

### Improvements

- Lakes will now only show the most important ones
- Images are now bundled, which should help with performance especially on slower hard drives
- Assigned correct items for some documents/papers

### Bugfixes

- Fix teleportation at a couple of places
- Replaced icons for a few weapons on the map with better visible ones

## v0.10.1

### Bugfixes

- Fix issue with default actor item

## v0.10.0

### Features

- Brand new interactive map
- Spawn enemies/actors using the map
- Display zip lines
- Teleport the player/followers to caves, bunkers, enemies, zip lines
- Show information about the most important POIs
- Filter items and bunkers if you have the required items to access or if you have already collected everything

### Improvements

- Some areas have had their performance improved

## v0.9.5

### Features

- Added editing of already finished structures (unfinish, almost finish, remove)
- Regrow trees now as a percent selector, which lets you decide how many trees you want to regrow

### Improvements

- Items in storages should now keep their modules and attachments
- Added grouping and scroll bars to structure list
- Added count of stumps/gone/half-chopped to "Regrow Trees" tool

### Fixes

- Fixed error when items with special modules/attachments were stored at storages

## v0.9.4

### Features

- Added some more items and icons
- Ability to modify items of Kelvin (right now Tarp only)

### Improvements

- Player and follower page now have scrollbars on smaller window sizes
- Player armor ordering is now done properly. There are no more gaps

### Fixes

- Fixed issue that dried/cooked meat, fish etc. was not selectable at inventory and storage tab

## v0.9.3

### Features

- Update 04 improvements
- Added support for cooked/spoiled/dried arms and legs in inventory and storages
    - At inventory, it will add cooked arms/legs by default. Detailed editing may be added in the future
- Added new action camera
- Added support for savegame naming
- Added icons for almost all items

### Improvements

- Influences can now be added on demand if missing
- All sliders for all influence stats are now always visible, independent if present in the savegame or not
- Teleporting Player, Followers and NPCs will now consider the area mask
    - Previously, teleporting to locations below or back to the surface was not successful
    - "Move to Player" will now be disabled if the player is not at the surface
- Unassigned Items at Inventory Page will now only contain items that can be added to the inventory
- Optimize resource bundling

### Fixes

- Fixed stats resetting if the player did not have a value currently set for a given stat
- Fixed editing of player stats
- Fixed editing of rest buff (maximum value here is 1, else the game will overwrite this)

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
