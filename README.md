# StashMan

A PoE stash management plugin (scaffold). This plugin demonstrates:

- **Stash & Item Models** (under `Models/`)  
- **Managers** (under `Managers/`) for CRUD on tabs and items  
- **Event System** (under `Events/`) for hooking into tab/item changes  
- **LostAndFound** or "Recycle Bin" approach  
- **TaskRunner** (in `Compartments/`) for async or background tasks  
- **PriceCheckService** (in `Services/`) as a stub for external price lookups  
- **UI** (in `UI/`) with a sample ImGui panel  

## Quick Start

1. **Clone/Copy** the repo into your ExileCore plugin folder.
2. **Build** the solution in Visual Studio or using your favorite .NET build tool.
3. **Run PoE** with the Loader, or place the compiled plugin into the correct folder.
4. **Enable** the "StashMan" plugin in the Loader/Interface if needed.
5. **Use** your stash in Hideout or Town. The plugin will (in theory) read tabs/items and raise events.

## Usage & Extension

- **Register Event Handlers**: In `StashManCore.Initialise()`, call `LoggingEventHandler.Register()` and `PriceEventHandler.Register()` to enable them.
- **Customize**: Modify `StashUpdater` to read actual game data from PoE memory (via ExileCore2 or your chosen approach).
- **Lost & Found**: If an item is removed, it sits in a dictionary for a set time. If re-added with the same `UniqueHash`, it’s “restored,” preserving its old data.


