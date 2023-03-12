# SOTFEdit

![Screenshot](https://abload.de/img/sotfeditdrdoh.jpg)

A savegame editor for "Sons of The Forest". 

- [SOTFEdit](#sotfedit)
- [Disclaimer](#disclaimer)
- [Features](#features)
- [Download](#download)
- [Usage](#usage)
- [Hints](#hints)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [Final Words](#final-words)

# Disclaimer

This project is in no way or form associated with the developers of the game. It is just a fan project, nothing more, nothing less.

# Features

- Edit Game Setup (Game Mode, Spawn Rate etc.)
- Edit Inventory (Add/Remove items, change quantities)
- Edit Armor Data (Add Armor Pieces, change durability)
- Revive Virginia & Kelvin
- Regrow Trees
- Backup changed files automatically
- ... more features are planned

# Download
- You can find the newest version at the [Releases page](https://github.com/codengine/SOTFEdit/releases)

# Requirements
- Windows 7+ (I guess...)
- [.net 6.0+ Runtime](https://dotnet.microsoft.com/en-us/download/dotnet)

# Usage

- Start the application using SOTFEdit.exe
- You will get a prompt if the application could not identify your savegame directory, else it will be selected automatically
- You'll get a list of all savegames, ordered by the last write time
- Do some changes and save them using the button at the top right

If you use one of the "Tools" this will trigger a reload of the savegames, which will discard any pending changes. I'd recommend to use the tools after you're done with editing.

# Hints

- There is no sanity check on the values entered at the player's inventory. So something like "100" for backpack will most likely lead to undesired behaviors.

# Troubleshooting

One of the items in inventory is listed as "Unknown"?
- Please report the ItemId so that I can add it to the list of known items

My game does not work anymore?
- If you have selected to create backups before saving, you can just delete the old files and restore the files that are suffixed with ".bak*".

# Contributing

Feel free to report any unknown items or any feature requests. PRs are also welcome.

# Final Words

Big thanks to [Gronkh](https://gronkh.tv) for your many years of "Influenz". Especially without your "The Forest" streams I would have never known anything about that game.

# Links and Credits

- [Kleine Axt icon by Icons8](https://icons8.com/icon/81685/kleine-axt)