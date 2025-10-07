# Unity Inventory & Item Management System

## Overview

This project is a comprehensive inventory and item management system for Unity, designed for RPGs, adventure, and story-driven games. It features a multi-page inventory, item database, player stat integration, debug console, and modular architecture for easy expansion. The system is written in C# (targeting .NET Framework 4.7.1, C# 9.0).

---

## Features

- **Multi-Page Inventory:**  
  Supports multiple inventory pages (consumables, weapons, armor, key items), each with its own grid and UI.
- **Item Database:**  
  Centralized `ItemDB` for item definitions, stack limits, icons, and destination logic.
- **Player Stat Integration:**  
  Equipping weapons/armor dynamically modifies player stats (health, speed, jump, attack, defense).
- **Inventory Operations:**  
  Add, remove, flood, set, and drop items with stack management and validation.
- **UI Integration:**  
  Uses TextMeshPro for inventory display and menu navigation.
- **Debug Console:**  
  In-game console for executing debug commands, including inventory manipulation.
- **Dialogue & Game State Awareness:**  
  Inventory and menu logic respect dialogue state and game pause.
- **Extensible Architecture:**  
  Modular classes (`InventoryManager`, `InventoryPage`, `ItemDB`, etc.) for easy feature addition.

---  

## Controls

| Action                | Key                |  
|-----------------------|--------------------|  
| Move / Menu Navigation| Arrow Keys         |  
| Confirm Menu Option   | Z                  |  
| Toggle Inventory      | E                  |  
| Toggle Console (Debug)| Tab (Debug Mode)   |  
| Pause Game            | Escape             |  

- **Console Commands:**  
  Type `.help` in the console for a list of commands.    
  For item-related commands, ask for item IDs as needed.

---  

## Code Highlights

### InventoryManager

- **Central Controller:**  
  Handles all inventory logic, including page switching, item operations, and stat updates.
- **Static API:**  
  Most inventory operations are static, allowing easy access from anywhere in the codebase.
- **Validation:**  
  Extensive checks for item existence, stack limits, and valid destinations.
- **UI Updates:**  
  Dynamically updates TextMeshPro UI elements and inventory panels.

### InventoryPage

- **Page Types:**  
  Enum-based system for different inventory categories.
- **Grid Management:**  
  Each page manages its own grid of items and equipped slots.
- **Sprite Updates:**    
  Handles updating item icons and equipped visuals.

### ItemDB

- **Centralized Data:**  
  All item data (names, icons, stack sizes, etc.) is managed here.
- **Destination Logic:**  
  Determines which inventory page an item belongs to.

### Player Integration

- **Stat Modifiers:**  
  Equipping items directly affects player stats.
- **Sprite Updates:**    
  Armor changes update the player's visual appearance.

### Debug & Console

- **DebugManager:**  
  Logs inventory actions and errors for easier troubleshooting.
- **Console Commands:**    
  Allows runtime manipulation of inventory and game state.

---

## Example: Adding an Item
// Add 3 potions (itemID = 1) to the player's inventory  
`InventoryManager.Add(1, 3);`


## Example: Removing an Item
// Remove 1 sword (itemID = 2) from the player's inventory  
`InventoryManager.Remove(2, 1);`


## Example: Dropping an Item
// Drop the item at index 0 from the weapons page
`InventoryManager.DropItem(0, InventoryPage.PageTypes.weapons);`


---

## Extending the System

- **Add New Item Types:**  
  Update `ItemDB` and extend `InventoryPage.PageTypes`.
- **Custom Inventory Pages:**  
  Add new `InventoryPage` instances and update UI as needed.
- **New Console Commands:**  
  Implement in `DebugManager` or the console handler.

---


## Getting Started

### Prerequisites

* [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework) 9.0 or higher installed

### Running the Application

Navigate to the [releases pages](https://github.com/titansrule3035/solo-comp-sci-portfolio/releases), find the release for this project, and run the `Inventory System test 2.exe` file. 

### **Debug Console:**
Enable debug mode to use the in-game console.

---

## Notes

**Item IDs:**  
Each item is referenced by a unique integer ID.

| Item ID | Item Name      |  
  |:-------:|----------------|  
|    0    | Empty          |  
|  12003  | Diamond Helmet |  
|  12002  | Gold Helmet    |  
|  12001  | Iron Helmet    |  
|  12004  | Ultra Helmet   |
|    1    | Apple          |
|    2    | Banana         |
|    3    | Grapes         |
|    4    | Lemon          |
|    5    | Lime           |
|    6    | Orange         |
|    7    | Pear           |
|    8    | Pineapple      |
| 192003  | Diamond Sword  |
| 192002  | Gold Sword     |
| 192001  | Iron Sword     |
| 192004  | Ultra Sword    |  

- **Stack Limits:**  
  Each item has a maximum stack size, enforced by the system.
- **Dialogue & Pause:**  
  Inventory and menu actions are disabled during dialogue or when the game is paused.

---

## Credits

- **TextMeshPro:**  
  For advanced UI text rendering.
- **Unity Engine:**  
  Core game engine and editor.

---

## License

This project is provided as-is for educational and prototyping purposes.  
For commercial use, please ensure compliance with all third-party asset licenses.