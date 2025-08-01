# Industrial Age - Objects and Furniture (Continued) Mod: GitHub Copilot Instructions

## Mod Overview and Purpose

The "Industrial Age - Objects and Furniture (Continued)" mod updates and expands upon Jecrell's original mod, bringing the aesthetics and functional items of the late 1800s and early 1900s to your RimWorld colony. With additional contributions and enhancements, the mod enriches the RimWorld experience by introducing historically themed furniture and appliances that can be utilized by colonists at earlier technological stages.

## Key Features and Systems

- **Furniture and Appliances**: Adds a selection of historically inspired objects including:
  - Refrigerator: Provides meal storage and a freezing capability.
  - Telegraph: Enables early-stage communication for colonists.
  - Gramophone: Brings music to your colony, enhancing social experiences.
  - Various lighting options: From Candelabras to Edison lamps, designed for both interior and street settings.

- **Resource Integration**: 
  - Wax: Obtained from cooking tables, expanding crafting resources.

- **Compatibilities and Updates**:
  - Incorporates wax mechanics from crec4ets mod.
  - Modifies the royal chandelier to be compatible with throne rooms, courtesy of SoongJr.

## Coding Patterns and Conventions

- The mod primarily adheres to the C# conventions, implementing classes for various building and component behaviors.
- The use of well-structured methods and private function calls to encapsulate complex operations (e.g., `TryResolveNextTrack`, `ResolveTemperature`).

## XML Integration

- XML definitions are crucial for integrating new items and resources within RimWorld. Ensure XML files define properties such as textures, sizes, and interactions for items like the Gramophone, refrigerator, and additional lamps.

## Harmony Patching

- The mod employs Harmony for patching to modify and extend game behavior:
  - Ensure robust application of prefix and postfix patches to guard against mod conflicts.
  - Use Harmony to inject or alter game logic, such as power consumption checks or music playback operations.

## Suggestions for Copilot

### General Code Patterns

- **Method Stubs**: Generate complete method implementations for handling specific game interactions, leveraging method stubs such as `TryResolveNextTrack` or `SwitchTracks`.

- **Inheritance Utilization**: Explore class inheritance (e.g., `Building_Radio` inheriting from `Building_Gramophone`) to reduce code duplication and encapsulate shared logic.

- **Private and Utility Methods**: Focus on creating utility methods for repeated logic and encapsulating complex operations within private methods to enhance readability and maintainability.

### Specific File Suggestions

- **Building_Gramophone.cs**: Implement music track management, including features like playlist creation and track switching, orchestrated through methods like `SwitchTracks`.

- **Building_Refrigerator.cs**: Introduce temperature management logic, leveraging methods like `ResolveTemperature` and power consumption checks via `IsUsingHighPower`.

- **CompHeatPusherRefuelable.cs**: Develop component logic for heat emission in conjunction with fuel states, enhancing realism in heating devices like the Wood Stove Furnace.

- **JobDriver_PlayGramophone.cs**: Automate job functionalities for interacting with the Gramophone, such as queueing tracks and managing use frequency through the `JobDriver` subclasses.

### XML and Harmony Guidelines

- **XML Configuration**: Propose default values for item properties, ensuring compatibility with other mods and smooth integration within the game's ecosystem.

- **Harmony Best Practices**: Offer advice on structuring patch classes and handling potential conflicts, including checking for method signatures and applying conditional patches when necessary.

By following these guidelines and leveraging GitHub Copilot's suggestions, developers can streamline mod updates and feature implementations, maintaining the richness and compatibility of the "Industrial Age - Objects and Furniture (Continued)" mod within the RimWorld community.
