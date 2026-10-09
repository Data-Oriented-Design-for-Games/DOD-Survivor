# DOD Survivor

Sample project for [*High Performance Unity Game Development (Using data-oriented design)*](https://www.manning.com/books/high-performance-unity-game-development) by Nitzan Wilnai (Manning).

DOD Survivor is a bigger version of the survivor game that the chapter samples build step by step. It keeps the same data-oriented layout and puts a lot more game on top of it: a car you drive through the enemies, animated sprites, weapons, experience (XP) pickups, enemy waves and a collision grid. It is a work in progress, so some features are in the code but switched off. See *Current state* below.

## What it shows

- The same split as the chapter samples, at a larger size. `GameData` holds only arrays and counts, `Balance` holds read-only settings, `Logic` is static functions, and `Board` draws the result.
- Every kind of object is a set of parallel arrays plus index lists: enemies (alive, dying, dead), ammo, XP pickups and skid marks.
- `Logic.Tick` tells `Board` what changed through `Span<int>` lists: spawned, dying and dead enemies, fired and dead ammo, placed and picked-up XP. `Board` only shows or hides the sprites on those lists.
- A grid for collisions. Each tick the enemies are sorted into the squares of a `MapSize` by `MapSize` grid, and only enemies in the same square are checked against each other. XP pickups use a grid too.
- One shared sprite pool (`CommonPool`) behind the enemy, dying enemy, ammo, particle and XP pools.
- All content is data. Enemies, weapons, cars, players and level waves are ScriptableObjects in `Assets/Data`, baked into `Assets/Resources/balance.bytes` by `BalanceParser`.
- The logic runs without drawing anything. **Run Test** on the main menu calls `Game.RunTest`, which runs 600 ticks of `Logic.Tick` in a loop and logs the time to the Console.

## How it relates to the chapter samples

- It is the same game, not a separate one. It uses the same `Survivor` namespace and the same files as [Chapter-11](https://github.com/Data-Oriented-Design-for-Games/Chapter-11): `Game`, `Board`, `Logic`, `GameData`, `Balance`, `BalanceParser`, `AssetManager`, `GameDataIO`, `MetaDataIO`.
- Nothing in the code ties it to one chapter. It is best read after the chapter samples, as a larger example of the same ideas.
- On top of Chapter-11 it adds the car and its skid marks, animated sprites, a dying state for enemies, weapons and ammo, XP drops and pickup, level waves, and the collision grid.
- Enemies spawn from the waves in a `LevelSO`, not from a weighted spawn table.
- The pooling code has moved out of `Board` into its own pool classes.
- Prefabs are loaded by name through `AssetManager`, either straight from `Assets/Prefabs/Common` or from an asset bundle.
- It does not use the Job System, Burst or ECS. For those, see [Chapter-12-Jobs](https://github.com/Data-Oriented-Design-for-Games/Chapter-12-Jobs), [Chapter-12-Jobs-Transforms](https://github.com/Data-Oriented-Design-for-Games/Chapter-12-Jobs-Transforms), [Chapter-12-Burst](https://github.com/Data-Oriented-Design-for-Games/Chapter-12-Burst) and [Chapter-12-ECS](https://github.com/Data-Oriented-Design-for-Games/Chapter-12-ECS).

## Current state

This is how the code stands right now.

- A new game always starts in the car (`InCar` is set to `true` in `Logic.StartGame`). The on-foot code is there, but a new game does not use it.
- The player has no weapon. `Logic.StartGame` empties every weapon slot, and the lines that hand out weapons are commented out. Enemies die when the car hits them.
- Game over is switched off in `Logic.Tick`.
- The in-game UI (timer, stats, pause button) is not shown. The line that turns it on in `Board.Show` is commented out, so you cannot pause or save from inside the game.
- `Board.Show` spawns the level's enemies all at once with `Logic.TestSpanMaxEnemies`, up to `MaxEnemies`.
- `Logic.Tick` keeps three versions of enemy-to-enemy collision side by side, picked by a hard-coded `type` value.

## How the code is organized

Data (`Assets/Scripts`)
- `GameData.cs` — the state of a running game. Arrays and counts for enemies, ammo, XP, skid marks, the grids and the car.
- `Balance.cs` — read-only settings loaded from `balance.bytes`, grouped into `EnemyBalance`, `WeaponBalance`, `CarBalance`, `HeroBalance` and `LevelBalance`.
- `MetaData.cs` — menu state and best time.
- `ScriptableObjects/` — `BalanceSO`, `EnemySO`, `WeaponSO`, `CarSO`, `PlayerSO`, `LevelSO`. These are the assets you edit in `Assets/Data`.

Logic
- `Logic.cs` — every rule of the game as static functions. Start at `Logic.Tick`.
- `GameDataIO.cs`, `MetaDataIO.cs` — save and load to binary files.

The Unity side
- `Game.cs` — the entry point. It owns the data, switches between menu states and calls `Board.Tick` every frame.
- `Board.cs` — reads input, calls `Logic.Tick`, then updates the sprites in `updateVisuals`.
- `CommonPool.cs` — the shared pool code. `EnemyPool.cs`, `DyingEnemyPool.cs`, `WeaponPool.cs`, `ParticlePool.cs` and `XPPool.cs` use it. `TireTrackPool.cs` handles the skid marks.
- `CommonVisual.cs` — sprite animation helpers.
- `AnimatedSprite.cs`, `EnemySprite.cs`, `Player.cs`, `Car.cs`, `CarFrameInfo.cs`, `HealthBar.cs` — small components on the prefabs that hold sprite references.
- `AssetManager.cs` — loads and instantiates prefabs.
- `MainMenuVisual.cs`, `PauseMenuVisual.cs`, `GameOverVisual.cs` — the menus.

Tools
- `BalanceParser.cs` — the editor menu command that writes `balance.bytes`.
- `Assets/Editor/CreateAssetBundles.cs` — the **DOD > Asset Bundles** menu.
- `Assets/Editor/BuildGame.cs` — Android and iOS build commands in the **DOD > Build** menu.

## Running it

1. Open the project in Unity **6000.3.22f1** (Unity 6.3 LTS) or newer.
2. Open `Assets/Scenes/MainGameScene.unity` and press **Play**.
3. Click **New game**. Hold the left mouse button and drag to steer the car. The car speeds up and keeps driving on its own. Drive into enemies to kill them and pick up the XP they drop.
4. There is no game over at the moment. Stop Play mode to end the run. Press **S** to save a screenshot.
5. To change the game data, edit the assets in `Assets/Data`, then run **DOD > Balance > Parse Local** from the menu bar. This rewrites `Assets/Resources/balance.bytes`, which is the file the game reads.

`Board.handleInput` uses the mouse in the Editor and the first touch on a device.

**DOD > Asset Bundles > Use Asset Bundles** is off by default, and the Editor loads prefabs straight from `Assets/Prefabs/Common`. If you turn it on, the game loads them from `Assets/StreamingAssets/AssetBundles/common` instead. Rebuild that bundle for your platform first, for example with **DOD > Asset Bundles > Build AssetBundles OSX** or **Build AssetBundles Windows**.

## More samples

All sample projects for the book: https://github.com/Data-Oriented-Design-for-Games
