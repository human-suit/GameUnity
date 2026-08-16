# Unstable Experiment — Unity Demo

Playable demo: **Sector A** (5 rooms), WASD movement, doors, lottery combat, Route Map (Tab).

## Requirements

- **Unity 2022.3 LTS** (or newer 2022.3.x)
- Open folder: `Unity/UnstableExperiment`

## First open

1. Unity Hub → **Add** → select `Unity/UnstableExperiment`
2. Menu: **Unstable Experiment → Create Demo Scene** (creates `Assets/Scenes/Main.unity`)
3. Open `Main` scene → **Play**

Or: create empty scene, add empty GameObject, attach `GameBootstrap`, Play.

## Controls

| Key | Action |
|-----|--------|
| WASD | Move |
| E | Door / loot |
| Tab | Route Map (after finding map in Home) |
| Combat | Click ticket → REDEEM, End Turn |

## Demo path (Sector A)

1. **Plaza** — walk, optional fight Subject 03  
2. **Home** (south door) — pick up **Rust Key** + **Sector Map**  
3. **Gate** (west door) — needs key → Sector B hub (stub message)  
4. Alley / Well — optional  

## Data source

Runtime loads from `Assets/Resources/Data/`:

- `rooms_graph.json`
- `tickets.json`
- `unstable_rules.json`

Design docs: repo root `КОНЦЕПЦИЯ-ИГРЫ.md`

## Art

Placeholder procedural tiles. Drop sprites into `Assets/Art/` and extend `ProceduralRoomBuilder` to use them.

Source art: `../../окружение/`, `../../герои/`, `../../demo/`

## Structure

```
Assets/
  Scripts/
    Core/       GameBootstrap, GameState, DataModels
    World/      RoomManager, ProceduralRoomBuilder, Door, Enemy
    Combat/     CombatManager, TicketCombat
    UI/         GameHUD, RouteMapUI
  Resources/Data/
  Scenes/
  Editor/       DemoSceneCreator
```
