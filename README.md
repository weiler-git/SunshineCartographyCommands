# SunshineCartographyCommands

A client-side Valheim plugin that changes how cartography table data is read and written, with commands for managing map pins.

> [!IMPORTANT]
> Keep a backup of your character before using the plugin, especially after major Valheim updates.
> Changes to Valheim's cartography system can affect how pins are imported, merged, or removed.

## Features

- Shares explored map data and boss locations by default.
- Prevents other players' pins from being automatically removed when reading a cartography table.
- Can optionally include player-created pins when writing to a cartography table.
- Avoids importing duplicate pins.
- Includes slash commands for removing pins from your map.
- Client-side only; no server installation is required.

## Commands

`/removemypins`

Removes all personal pins from your map.

`/removeotherspins`

Removes pins created by other players from your map.

`/removeallpins`

Removes all pins from your map.

`/writepindata`

Allows player-created pins to be included the next time you write to a cartography table.

## Installation

Install using a mod manager, or manually copy the plugin into:

`BepInEx\plugins`

The plugin is client-side only and does not need to be installed on the server.

## How Valheim Normally Handles Cartography Data

### Reading from a cartography table

When you click **Read** on a cartography table, Valheim normally:

- Merges explored map data into your map.
- Removes pins created by other players from your map.
- Imports pins stored on the cartography table.
- Skips pins that are close to existing pins.
- Skips pins originally created by you.

### Writing to a cartography table

When you click **Write**, Valheim normally:

- Reads the cartography table into your map first.
- Merges your explored map with the table data.
- Reads the table data again.
- Merges your map data into the cartography package.
- Adds your pins to the package.
- Sends the result to the cartography table owner/area host over RPC.

## How SunshineCartographyCommands Changes This

### Reading from a cartography table

The plugin:

- Prevents other players' pins from being automatically deleted from your map.
- Imports all pins stored on the cartography table, including pins originally created by you.

Use the slash commands above if you want to remove specific groups of pins from your map.

If you want to completely erase the data stored in a cartography table, the table itself can be rebuilt.

### Writing to a cartography table

The plugin:

- Skips Valheim's normal read-before-write operation.
- Reads the existing cartography table data directly.
- Merges your explored map with the explored map stored in the table.
- Merges existing table pins with your pins.
- Skips duplicate pins.
- Skips normal player-created pins by default.
- Sends the merged result to the cartography table owner/area host over RPC.

Use `/writepindata` to allow player-created pins to be included for the next write.

## Why?

This plugin was originally created for a setup where:

- A public cartography table is available for everyone.
- A designated character adds useful world-location pins to the public map.
- Individual teams maintain their own maps and pins.
- Teams can read public map data without losing their private team pins.
- Players can maintain personal cartography tables for sharing map data between alternate characters.

## Changelog

### 1.1.0

Reworked from the ground up following breaking changes to Valheim's cartography system. The new implementation uses a safer approach intended to reduce the risk of losing pins.

### 1.0.4

Fixed an error when writing to a cartography table more than once.

### 1.0.3

Recompiled for Valheim 0.217.24.

### 1.0.2

Recompiled for the Hildir's Request update.

### 1.0.1

Fixed a null-reference error when updating an empty cartography table.

### 1.0.0

Initial release.
