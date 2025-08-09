using UnityEngine;
using System;
using Unity.Burst.Intrinsics;

namespace Survivor
{
    public static class Logic
    {
        public static void AllocateGameData(GameData gameData, Balance balance)
        {
            gameData.EnemyPosition = new Vector2[balance.MaxEnemies];
            gameData.EnemyRotation = new float[balance.MaxEnemies];

            gameData.EnemyPushStartPos = new Vector2[balance.MaxEnemies];
            gameData.EnemyPushEndPos = new Vector2[balance.MaxEnemies];
            gameData.EnemyPushValue = new float[balance.MaxEnemies];
            gameData.EnemyPushRotation = new float[balance.MaxEnemies];

            gameData.EnemyType = new int[balance.MaxEnemies];
            gameData.AliveEnemyIdxs = new int[balance.MaxEnemies];
            gameData.DeadEnemyIdxs = new int[balance.MaxEnemies];
            gameData.DyingEnemyIdxs = new int[balance.MaxEnemies];
            gameData.DyingEnemyTimer = new float[balance.MaxEnemies];

            gameData.AmmoPosition = new Vector2[balance.MaxAmmo];
            gameData.AmmoDirection = new Vector2[balance.MaxAmmo];
            gameData.AmmoType = new int[balance.MaxAmmo];
            gameData.AmmoRotationT = new float[balance.MaxAmmo];
            gameData.AliveAmmoIdx = new int[balance.MaxAmmo];
            gameData.DeadAmmoIdx = new int[balance.MaxAmmo];
            gameData.AmmoTargetIdx = new int[balance.MaxAmmo];
            gameData.AmmoTargetPos = new Vector2[balance.MaxAmmo];
            gameData.PlayerWeaponFiringRateTimer = new float[balance.MaxAmmo];

            gameData.PlayerWeaponType = new int[balance.MaxPlayerWeapons];

            gameData.XPPosition = new Vector2[balance.MaxXP];
            gameData.XPValue = new float[balance.MaxXP];
            gameData.XPUsedIdxs = new int[balance.MaxXP];
            gameData.XPUnusedIdxs = new int[balance.MaxXP];

            gameData.SkidMarkPos = new Vector2[4 * balance.MaxSkidMarks];
            gameData.SkidMarkAngle = new float[balance.MaxSkidMarks];
            gameData.SkidMarkColor = new Color[4 * balance.MaxSkidMarks];
            gameData.CurrentSkidMarkColor = new Color[4];
        }

        public static void Init(MetaData metaData)
        {
            metaData.MenuState = MENU_STATE.NONE;
        }

        public static void StartGame(GameData gameData, Balance balance)
        {
            gameData.InGame = false;

            gameData.GameTime = 0.0f;
            gameData.XP = 0.0f;

            gameData.HeroType = 0;
            gameData.CarType = 0;

            gameData.InCar = true;
            gameData.PlayerDirection = gameData.InCar ? Vector2.right : Vector2.zero;
            gameData.PlayerTargetDirection = gameData.InCar ? Vector2.right : Vector2.zero;
            gameData.CarSlideDirection = gameData.PlayerDirection;
            gameData.CarRotationAngle = 0.0f;
            gameData.CarVelocity = 0.0f;

            gameData.AliveEnemyCount = 0;
            gameData.DeadEnemyCount = balance.MaxEnemies;
            for (int i = 0; i < balance.MaxEnemies; i++)
                gameData.DeadEnemyIdxs[i] = (balance.MaxEnemies - 1) - i;
            gameData.DyingEnemyCount = 0;

            gameData.AliveAmmoCount = 0;
            gameData.DeadAmmoCount = balance.MaxAmmo;
            for (int i = 0; i < balance.MaxAmmo; i++)
                gameData.DeadAmmoIdx[i] = (balance.MaxAmmo - 1) - i;

            for (int i = 0; i < balance.MaxAmmo; i++)
                gameData.AmmoTargetIdx[i] = -1;


            for (int i = 0; i < balance.MaxPlayerWeapons; i++)
                gameData.PlayerWeaponType[i] = -1;

            gameData.Level = 0;
            gameData.Wave = 0;
            gameData.WaveEnemyCount = new int[balance.LevelBalance[gameData.Level].TotalWaves];
            gameData.SpawnTime = new float[balance.LevelBalance[gameData.Level].TotalWaves];
            gameData.GroupTime = new float[balance.LevelBalance[gameData.Level].TotalWaves];
            for (int i = 0; i < gameData.WaveEnemyCount.Length; i++)
            {
                gameData.WaveEnemyCount[i] = 0;
                gameData.SpawnTime[i] = 0.0f;
                gameData.GroupTime[i] = 0.0f;
            }

            gameData.XPUsedCount = 0;
            gameData.XPUnusedCount = balance.MaxXP;
            for (int i = 0; i < balance.MaxXP; i++)
                gameData.XPUnusedIdxs[i] = (balance.MaxXP - 1) - i;

            gameData.StatsEnemiesKilled = 0;

            gameData.CurrentSkidMarkIndex = 0;
            for (int i = 0; i < 4; i++)
                gameData.CurrentSkidMarkColor[i] = balance.SkidMarkColor;

            // TEST - no way to assign them in game yet
            // gameData.PlayerWeaponType[0] = 0;
            // gameData.PlayerWeaponType[1] = 1;
        }

        static int spawnEnemy(GameData gameData, Balance balance, int enemyType)
        {
            Vector2 direction = gameData.PlayerDirection;
            float angle = UnityEngine.Random.value * 180.0f - 90.0f;
            if (direction.magnitude == 0.0f)
            {
                direction = new Vector2(0.0f, 1.0f);
                angle = UnityEngine.Random.value * 360.0f;
            }
            direction = RotateVector(direction, angle);
            int enemyIndex = gameData.DeadEnemyIdxs[--gameData.DeadEnemyCount];
            gameData.AliveEnemyIdxs[gameData.AliveEnemyCount++] = enemyIndex;
            gameData.EnemyPosition[enemyIndex] = direction.normalized * balance.SpawnRadius;
            gameData.EnemyType[enemyIndex] = enemyType;
            gameData.EnemyRotation[enemyIndex] = 0.0f;
            gameData.EnemyPushValue[enemyIndex] = 0.0f;

            return enemyIndex;
        }

        private const double DegToRad = Math.PI / 180.0d;
        private const double RadToDeg = 180.0d / Math.PI;

        public static Vector2 RotateVector(Vector2 a, double degrees)
        {
            double radians = degrees * DegToRad;
            double ca = Math.Cos(radians);
            double sa = Math.Sin(radians);
            a.x = (float)(ca * a.x - sa * a.y);
            a.y = (float)(sa * a.x + ca * a.y);
            return a;
        }

        public static void tryFireWeapon(GameData gameData, Balance balance, float dt, Span<int> firedWeaponIdxs, ref int fireWeaponCount)
        {
            for (int playerWeaponIndex = 0; playerWeaponIndex < balance.MaxPlayerWeapons; playerWeaponIndex++)
            {
                if (gameData.PlayerWeaponType[playerWeaponIndex] > -1)
                {
                    int playerWeaponType = gameData.PlayerWeaponType[playerWeaponIndex];
                    gameData.PlayerWeaponFiringRateTimer[playerWeaponIndex] -= dt;
                    if (gameData.AliveEnemyCount > 0 && gameData.PlayerWeaponFiringRateTimer[playerWeaponIndex] < 0.0f && gameData.DeadAmmoCount > 0)
                    {
                        // get free Weapon
                        gameData.PlayerWeaponFiringRateTimer[playerWeaponIndex] += balance.WeaponBalance.FiringRate[playerWeaponType];

                        for (int projectileIndex = 0; projectileIndex < balance.WeaponBalance.NumProjectiles[playerWeaponType]; projectileIndex++)
                        {
                            if (gameData.DeadAmmoCount > 0)
                            {
                                int ammoIndex = gameData.DeadAmmoIdx[--gameData.DeadAmmoCount];
                                gameData.AliveAmmoIdx[gameData.AliveAmmoCount++] = ammoIndex;
                                firedWeaponIdxs[fireWeaponCount++] = ammoIndex;
                                gameData.AmmoType[ammoIndex] = playerWeaponType;

                                gameData.AmmoPosition[ammoIndex] = Vector2.zero;

                                // get enemy to fire at
                                // int enemyIdx = GetClosestEnemyToPlayerIdx(gameData);
                                if (balance.WeaponBalance.AmmoTarget[playerWeaponType] == AMMO_TARGET.ENEMY)
                                {
                                    gameData.AmmoDirection[ammoIndex] = gameData.PlayerDirection.sqrMagnitude > 0.0f ? gameData.PlayerDirection : Vector2.up;
                                    gameData.AmmoRotationT[ammoIndex] = 0.0f;

                                    int enemyIdx = GetClosestEnemyToPlayerIdxNotUsed(gameData);
                                    gameData.AmmoTargetIdx[ammoIndex] = enemyIdx;
                                    if (enemyIdx > -1)
                                        gameData.AmmoTargetPos[ammoIndex] = gameData.EnemyPosition[enemyIdx] + balance.SpawnRadius * gameData.AmmoDirection[ammoIndex];
                                    else
                                        gameData.AmmoTargetPos[ammoIndex] = gameData.AmmoDirection[ammoIndex] * balance.SpawnRadius;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void TestFireWeapon(GameData gameData, Balance balance, float dt, Span<int> firedWeaponIdxs, ref int fireWeaponCount)
        {
            int playerWeaponIndex = 0; // Test fire the first weapon
            gameData.PlayerWeaponFiringRateTimer[playerWeaponIndex] = 0;

            tryFireWeapon(gameData, balance, dt, firedWeaponIdxs, ref fireWeaponCount);
        }

        public static int GetClosestEnemyToPlayerIdxNotUsed(GameData gameData)
        {
            // change to insertion sort, and return closest untargeted value
            int closestEnemyIdx = -1;
            float closestDistanceSqr = float.MaxValue;
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIndex = gameData.AliveEnemyIdxs[i];
                float distanceSqr = gameData.EnemyPosition[enemyIndex].sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    bool enemyTargeted = false;
                    for (int ammoIndex = 0; ammoIndex < gameData.AliveAmmoCount; ammoIndex++)
                        if (gameData.AmmoTargetIdx[ammoIndex] == enemyIndex)
                            enemyTargeted = true;

                    if (!enemyTargeted)
                    {
                        closestEnemyIdx = enemyIndex;
                        closestDistanceSqr = distanceSqr;
                    }
                }
            }

            // Debug.Log(closestEnemyIdx + " closestEnemyIdx ");
            return closestEnemyIdx;
        }

        public static int GetClosestEnemyToPlayerIdx(GameData gameData)
        {
            // change to insertion sort, and return closest untargeted value

            int closestEnemyIdx = -1;
            float closestDistanceSqr = float.MaxValue;
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIndex = gameData.AliveEnemyIdxs[i];
                float distanceSqr = gameData.EnemyPosition[enemyIndex].sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestEnemyIdx = enemyIndex;
                    closestDistanceSqr = distanceSqr;
                }
            }

            return closestEnemyIdx;
        }

        public static void Tick(
            MetaData metaData,
            GameData gameData,
            Balance balance,
            float dt,
            Span<int> spawnedEnemyIdxs,
            out int spawnedEnemyCount,
            Span<int> deadEnemyIdxs,
            out int deadEnemyCount,
            Span<int> dyingEnemyIdxs,
            out int dyingEnemyCount,
            Span<int> firedWeaponIdxs,
            out int firedWeaponCount,
            Span<int> deadAmmoIdxs,
            out int deadAmmoCount,
            Span<int> xpPlacedIdxs,
            out int xpPlacedCount,
            Span<int> xpPickedUpIdxs,
            out int xpPickedUpCount,
            int xpPickedUpMax,
            out bool gameOver)
        {
            gameData.GameTime += dt;

            deadEnemyCount = 0;
            dyingEnemyCount = 0;
            spawnedEnemyCount = 0;
            firedWeaponCount = 0;
            deadAmmoCount = 0;
            xpPlacedCount = 0;
            xpPickedUpCount = 0;

            if (gameData.InCar)
                updateTireTracks(gameData, balance, dt);

            // Weapon
            tryFireWeapon(gameData, balance, dt, firedWeaponIdxs, ref firedWeaponCount);

            moveAmmo(gameData, balance, dt);

            checkAmmoOutOfBounds(gameData, balance, deadAmmoIdxs, ref deadAmmoCount);

            checkAmmoEnemyCollision(gameData, balance, dyingEnemyIdxs, ref dyingEnemyCount, deadAmmoIdxs, ref deadAmmoCount);

            checkAmmoReachedDestination(gameData, balance, dyingEnemyIdxs, ref dyingEnemyCount, deadAmmoIdxs, ref deadAmmoCount);

            // enemies
            timeOutDyingEnemies(gameData, balance, deadEnemyIdxs, ref deadEnemyCount, xpPlacedIdxs, ref xpPlacedCount, dt);

            trySpawnEnemy(gameData, balance, dt, spawnedEnemyIdxs, ref spawnedEnemyCount);

            moveEnemies(gameData, balance, dt);

            moveDyingEnemies(gameData, balance, dt);

            checkEnemyOutOfBounds(gameData, balance, deadEnemyIdxs, ref deadEnemyCount);

            checkDyingEnemyOutOfBounds(gameData, balance);

            doEnemyToEnemyCollision(gameData, balance);

            doCarEnemyCollision(metaData, gameData, balance, dyingEnemyIdxs, ref dyingEnemyCount);

            // player
            if (gameData.InCar)
            {
                // updateTireTracks(gameData, balance, dt);
                moveCar(gameData, balance, dt);
            }
            else
                movePlayer(gameData, balance, dt);
            pickupXP(gameData, xpPickedUpIdxs, ref xpPickedUpCount, xpPickedUpMax);


            gameOver = false;//checkGameOver(metaData, gameData, balance);
        }

        private static void pickupXP(GameData gameData, Span<int> xpPickedUpIdxs, ref int xpPickedUpCount, int xpPickedUpMax)
        {
            int count = 0;
            for (int i = 0; i < gameData.XPUsedCount; i++)
            {
                int xpIndex = gameData.XPUsedIdxs[i];
                if (xpPickedUpCount < xpPickedUpMax && gameData.XPPosition[xpIndex].sqrMagnitude < 0.1f)
                {
                    gameData.XP += gameData.XPValue[xpIndex];
                    // pickup XP;
                    xpPickedUpIdxs[xpPickedUpCount++] = xpIndex;
                    gameData.XPUnusedIdxs[gameData.XPUnusedCount++] = xpIndex;
                }
                else
                {
                    gameData.XPUsedIdxs[count++] = gameData.XPUsedIdxs[i];
                }
            }
            gameData.XPUsedCount = count;
        }

        static void trySpawnEnemy(GameData gameData, Balance balance, float dt, Span<int> spawnedEnemyIdxs, ref int spawnedEnemyCount)
        {
            for (int waveIdx = 0; waveIdx < balance.LevelBalance[gameData.Level].TotalWaves; waveIdx++)
            {
                if (gameData.DeadEnemyCount > 0)
                {
                    int enemyType = balance.LevelBalance[gameData.Level].EnemyType[waveIdx];
                    if (gameData.GameTime >= balance.LevelBalance[gameData.Level].StartTime[waveIdx] &&
                        gameData.GameTime < balance.LevelBalance[gameData.Level].EndTime[waveIdx])
                    {
                        if (gameData.WaveEnemyCount[waveIdx] < balance.LevelBalance[gameData.Level].NumEnemies[waveIdx])
                        {
                            gameData.SpawnTime[waveIdx] += dt;
                            if (gameData.SpawnTime[waveIdx] >= balance.LevelBalance[gameData.Level].SpawnDelay[waveIdx])
                            {
                                gameData.SpawnTime[waveIdx] -= balance.LevelBalance[gameData.Level].SpawnDelay[waveIdx];

                                // need to spawn this enemy
                                int enemyIndex = spawnEnemy(gameData, balance, enemyType);
                                spawnedEnemyIdxs[spawnedEnemyCount++] = enemyIndex;
                                gameData.WaveEnemyCount[waveIdx]++;
                            }
                        }
                    }
                }
            }
        }

        static bool tryRespawnEnemy(GameData gameData, Balance balance, int enemyIndex)
        {
            // check if enemy is still in wave
            for (int waveIdx = 0; waveIdx < balance.LevelBalance[gameData.Level].TotalWaves; waveIdx++)
            {
                // is this enemy type part of this wave?
                if (gameData.EnemyType[enemyIndex] == balance.LevelBalance[gameData.Level].EnemyType[waveIdx])
                {
                    // is this wave active?
                    if (gameData.GameTime >= balance.LevelBalance[gameData.Level].StartTime[waveIdx] &&
                            gameData.GameTime < balance.LevelBalance[gameData.Level].EndTime[waveIdx])
                    {
                        Vector2 direction = gameData.PlayerDirection;
                        float angle = UnityEngine.Random.value * 180.0f - 90.0f;
                        if (direction.magnitude == 0.0f)
                        {
                            direction = new Vector2(0.0f, 1.0f);
                            angle = UnityEngine.Random.value * 360.0f;
                        }
                        direction = RotateVector(direction, angle);
                        gameData.EnemyPosition[enemyIndex] = direction.normalized * balance.SpawnRadius;

                        return true;
                    }
                }
            }

            return false;
        }

        static void markEnemyDead(GameData gameData, Balance balance, int enemyIdx, Span<int> deadEnemyIdxs, ref int deadEnemyCount)
        {
            gameData.DeadEnemyIdxs[gameData.DeadEnemyCount++] = enemyIdx;

            changeEnemyTargetIdxFromWeapons(gameData, balance, enemyIdx);
            deadEnemyIdxs[deadEnemyCount++] = enemyIdx;
        }

        static void markEnemyDyingCheckForDuplicate(
            GameData gameData,
            Balance balance,
            int enemyIndex,
            Span<int> dyingEnemyIdxs,
            ref int dyingEnemyCount)
        {
            bool enemyAlreadyDying = false;
            for (int i = 0; i < gameData.DyingEnemyCount; i++)
                if (gameData.DyingEnemyIdxs[i] == enemyIndex)
                {
                    enemyAlreadyDying = true;
                    break;
                }

            if (!enemyAlreadyDying)
            {
                int enemyType = gameData.EnemyType[enemyIndex];
                gameData.DyingEnemyIdxs[gameData.DyingEnemyCount] = enemyIndex;
                gameData.DyingEnemyTimer[gameData.DyingEnemyCount] = balance.EnemyBalance.DyingTime[enemyType];
                gameData.DyingEnemyCount++;
                dyingEnemyIdxs[dyingEnemyCount++] = enemyIndex;
                gameData.StatsEnemiesKilled++;
            }

            changeEnemyTargetIdxFromWeapons(gameData, balance, enemyIndex);

            // Debug.Log("enemyDying " + enemyIndex);
        }

        private static int dropXP(GameData gameData, Balance balance, int enemyIndex, Span<int> xpPlacedIdxs, ref int xpPlacedCount)
        {
            if (gameData.XPUnusedCount > 0)
            {
                int xpIndex = gameData.XPUnusedIdxs[--gameData.XPUnusedCount];
                gameData.XPUsedIdxs[gameData.XPUsedCount++] = xpIndex;
                gameData.XPPosition[xpIndex] = gameData.EnemyPosition[enemyIndex];
                gameData.XPValue[xpIndex] = balance.EnemyBalance.XP[gameData.EnemyType[enemyIndex]];
                xpPlacedIdxs[xpPlacedCount++] = xpIndex;
            }
            else
            {
                int xpIndex = combineClosestXPReturnNewUnusedIndex(gameData);
                gameData.XPValue[xpIndex] = balance.EnemyBalance.XP[gameData.EnemyType[enemyIndex]];
                gameData.XPPosition[xpIndex] = gameData.EnemyPosition[enemyIndex];
            }

            return xpPlacedCount;
        }

        // static void combineClosesXP(GameData gameData)
        // {
        //     int count = 0;
        //     for (int i = 0; i < gameData.XPUsedCount; i++)
        //     {
        //         int xpIndex1 = gameData.XPUsedIdxs[i];
        //         bool removed = false;
        //         for (int j = i + 1; j < gameData.XPUsedCount; j++)
        //         {
        //             int xpIndex2 = gameData.XPUsedIdxs[j];
        //             if ((gameData.XPPosition[xpIndex1] - gameData.XPPosition[xpIndex2]).sqrMagnitude < 0.1f)
        //             {
        //                 gameData.XPValue[xpIndex2] += gameData.XPValue[xpIndex1];
        //                 gameData.XPUnusedIdxs[gameData.XPUnusedCount++] = xpIndex1;
        //                 removed = true;
        //                 break;
        //             }
        //         }
        //         if (!removed)
        //             gameData.XPUsedIdxs[count++] = xpIndex1;
        //     }
        //     gameData.XPUsedCount = count;
        // }

        static int combineClosestXPReturnNewUnusedIndex(GameData gameData)
        {
            int closestIndex1 = 0;
            int closestIndex2 = 1;
            float closestDistanceSqr = float.MaxValue;

            for (int i = 0; i < gameData.XPUsedCount; i++)
            {
                int xpIndex1 = gameData.XPUsedIdxs[i];
                for (int j = i + 1; j < gameData.XPUsedCount; j++)
                {
                    int xpIndex2 = gameData.XPUsedIdxs[j];

                    Vector2 diff = gameData.XPPosition[xpIndex1] - gameData.XPPosition[xpIndex2];
                    float distanceSqr = diff.sqrMagnitude;
                    if (distanceSqr < closestDistanceSqr)
                    {
                        closestIndex1 = xpIndex1;
                        closestIndex2 = xpIndex2;
                        closestDistanceSqr = distanceSqr;
                    }
                }
            }
            gameData.XPValue[closestIndex1] += gameData.XPValue[closestIndex2];
            gameData.XPValue[closestIndex2] = 0.0f;
            return closestIndex2;
        }

        static int getClosestXPIndexForEnemy(GameData gameData, int enemyIndex)
        {
            int closestIndex = -1;
            float closestDistanceSqr = float.MaxValue;
            Vector2 enemyPosition = gameData.EnemyPosition[enemyIndex];
            for (int i = 0; i < gameData.XPUsedCount; i++)
            {
                int xpIndex = gameData.XPUsedIdxs[i];
                Vector2 xpPosition = gameData.XPPosition[xpIndex];
                float distanceSqr = (xpPosition - enemyPosition).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestIndex = xpIndex;
                }
            }
            return closestIndex;
        }

        static void timeOutDyingEnemies(
            GameData gameData,
            Balance balance,
            Span<int> deadEnemyIdxs,
            ref int deadEnemyCount,
            Span<int> xpPlacedIdxs,
            ref int xpPlacedCount,
            float dt)
        {
            int count = 0;
            for (int i = 0; i < gameData.DyingEnemyCount; i++)
            {
                int enemyIndex = gameData.DyingEnemyIdxs[i];
                gameData.DyingEnemyTimer[i] -= dt;
                if (gameData.DyingEnemyTimer[i] <= 0.0f)
                {
                    markEnemyDead(gameData, balance, enemyIndex, deadEnemyIdxs, ref deadEnemyCount);

                    //dropXP(gameData, balance, enemyIndex, xpPlacedIdxs, ref xpPlacedCount);
                }
                else
                {
                    gameData.DyingEnemyIdxs[count] = enemyIndex;
                    gameData.DyingEnemyTimer[count] = gameData.DyingEnemyTimer[i];
                    count++;
                }
            }
            gameData.DyingEnemyCount = count;
        }

        static void moveEnemies(GameData gameData, Balance balance, float dt)
        {
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIndex = gameData.AliveEnemyIdxs[i];

                if (gameData.EnemyPushValue[enemyIndex] > 0.0f)
                    moveAndPushEnemy(gameData, dt, enemyIndex);
                else
                {
                    Vector2 dir = -gameData.EnemyPosition[enemyIndex].normalized;
                    gameData.EnemyPosition[enemyIndex] += dir * balance.EnemyBalance.Velocity[gameData.EnemyType[enemyIndex]] * dt;
                    if (gameData.EnemyRotation[enemyIndex] > 0.0f)
                    {
                        gameData.EnemyRotation[enemyIndex] -= dt * 360.0f;
                        if (gameData.EnemyRotation[enemyIndex] <= 0.0f)
                            gameData.EnemyRotation[enemyIndex] = 0.0f;
                    }
                    if (gameData.EnemyRotation[enemyIndex] < 0.0f)
                    {
                        gameData.EnemyRotation[enemyIndex] += dt * 360.0f;
                        if (gameData.EnemyRotation[enemyIndex] >= 0.0f)
                            gameData.EnemyRotation[enemyIndex] = 0.0f;
                    }
                }
            }
        }

        static void moveDyingEnemies(GameData gameData, Balance balance, float dt)
        {
            for (int i = 0; i < gameData.DyingEnemyCount; i++)
            {
                int enemyIndex = gameData.DyingEnemyIdxs[i];
                moveAndPushEnemy(gameData, dt, enemyIndex);
            }
        }

        private static void moveAndPushEnemy(GameData gameData, float dt, int enemyIndex)
        {
            gameData.EnemyPushValue[enemyIndex] -= dt;
            float value = 1.0f - (gameData.EnemyPushValue[enemyIndex] * gameData.EnemyPushValue[enemyIndex] * gameData.EnemyPushValue[enemyIndex]);
            gameData.EnemyPosition[enemyIndex] = (gameData.EnemyPushEndPos[enemyIndex] - gameData.EnemyPushStartPos[enemyIndex]) * value + gameData.EnemyPushStartPos[enemyIndex];
            gameData.EnemyRotation[enemyIndex] += gameData.EnemyPushRotation[enemyIndex] * value * dt;
            if (gameData.EnemyRotation[enemyIndex] > 180.0f)
                gameData.EnemyRotation[enemyIndex] -= 360.0f;
            if (gameData.EnemyRotation[enemyIndex] < -180.0f)
                gameData.EnemyRotation[enemyIndex] += 360.0f;
        }

        static void doEnemyToEnemyCollision(GameData gameData, Balance balance)
        {
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIdx1 = gameData.AliveEnemyIdxs[i];
                for (int j = i + 1; j < gameData.AliveEnemyCount; j++)
                {
                    int enemyIdx2 = gameData.AliveEnemyIdxs[j];

                    float diameter = balance.EnemyBalance.Radius[gameData.EnemyType[enemyIdx1]] + balance.EnemyBalance.Radius[gameData.EnemyType[enemyIdx2]];
                    float diameterSqr = diameter * diameter;

                    Vector2 diff = gameData.EnemyPosition[enemyIdx1] - gameData.EnemyPosition[enemyIdx2];
                    if (diff.sqrMagnitude <= diameterSqr)
                    {
                        Vector2 diffNormalized = diff.normalized;
                        Vector2 midPoint = (gameData.EnemyPosition[enemyIdx1] + gameData.EnemyPosition[enemyIdx2]) / 2.0f;
                        gameData.EnemyPosition[enemyIdx1] = midPoint + diffNormalized * balance.EnemyBalance.Radius[gameData.EnemyType[enemyIdx1]];
                        gameData.EnemyPosition[enemyIdx2] = midPoint - diffNormalized * balance.EnemyBalance.Radius[gameData.EnemyType[enemyIdx2]];
                    }
                }
            }
        }

        static void checkEnemyOutOfBounds(GameData gameData, Balance balance, Span<int> deadEnemyIdxs, ref int deadEnemyCount)
        {
            float distanceSqr = balance.SpawnRadius * balance.SpawnRadius * 1.1f;
            int count = 0;
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIdx = gameData.AliveEnemyIdxs[i];
                if (gameData.EnemyPosition[enemyIdx].sqrMagnitude > distanceSqr)
                {
                    if (!tryRespawnEnemy(gameData, balance, enemyIdx))
                        markEnemyDead(gameData, balance, enemyIdx, deadEnemyIdxs, ref deadEnemyCount);
                    else
                        gameData.AliveEnemyIdxs[count++] = enemyIdx;
                }
                else
                    gameData.AliveEnemyIdxs[count++] = enemyIdx;
            }
            gameData.AliveEnemyCount = count;
        }

        static void checkDyingEnemyOutOfBounds(GameData gameData, Balance balance)
        {
            float distanceSqr = balance.SpawnRadius * balance.SpawnRadius * 1.1f;
            for (int deIdx = 0; deIdx < gameData.DyingEnemyCount; deIdx++)
            {
                int enemyIdx = gameData.DyingEnemyIdxs[deIdx];
                if (gameData.EnemyPosition[enemyIdx].sqrMagnitude > distanceSqr)
                    gameData.DyingEnemyTimer[deIdx] = 0.0f;
            }
        }

        static void doCarEnemyCollision(
            MetaData metaData,
            GameData gameData,
            Balance balance,
            Span<int> dyingEnemyIdxs,
            ref int dyingEnemyCount)
        {
            Span<float> collisionRadiusSqr = stackalloc float[balance.CarBalance[gameData.CarType].CollisionCircles[gameData.CarSlideIndex].Length];
            for (int c = 0; c < balance.CarBalance[gameData.CarType].CollisionCircles[gameData.CarSlideIndex].Length; c++)
            {
                float collisionRadius = balance.CarBalance[gameData.CarType].CollisionRadius[gameData.CarSlideIndex][c];
                collisionRadiusSqr[c] = collisionRadius * collisionRadius;
            }

            int enemyCount = 0;
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIdx = gameData.AliveEnemyIdxs[i];
                if (doEnemyCircleCollision(gameData, balance, collisionRadiusSqr, enemyIdx))
                {
                    int enemyType = gameData.EnemyType[enemyIdx];
                    gameData.CarVelocity *= balance.EnemyBalance.ImpactSlowdown[enemyType];
                    if (gameData.CarVelocity < balance.CarBalance[gameData.CarType].Velocity * 0.67f)
                        gameData.CarVelocity = balance.CarBalance[gameData.CarType].Velocity * 0.67f;

                    markEnemyDyingCheckForDuplicate(gameData, balance, enemyIdx, dyingEnemyIdxs, ref dyingEnemyCount);

                    int closestTireIdx = 0;
                    float closestDistanceSqr = (gameData.EnemyPosition[enemyIdx] - balance.CarBalance[gameData.CarType].Tires[gameData.CarSlideIndex][0]).sqrMagnitude;
                    for (int tireIdx = 1; tireIdx < 4; tireIdx++)
                    {
                        float distanceSqr = (gameData.EnemyPosition[enemyIdx] - balance.CarBalance[gameData.CarType].Tires[gameData.CarSlideIndex][tireIdx]).sqrMagnitude;
                        if (distanceSqr < closestDistanceSqr)
                        {
                            closestTireIdx = tireIdx;
                            closestDistanceSqr = distanceSqr;
                        }
                    }
                    gameData.CurrentSkidMarkColor[closestTireIdx] = balance.EnemyBalance.DyingColor[enemyType];

                    float offsetAngle = UnityEngine.Random.value * 10.0f - 5.0f;
                    gameData.PlayerDirection = RotateVector(gameData.PlayerDirection, offsetAngle);
                }
                else
                    gameData.AliveEnemyIdxs[enemyCount++] = enemyIdx;
            }
            gameData.AliveEnemyCount = enemyCount;

            // for (int i = 0; i < 4; i++)
            //     gameData.CurrentSkidMarkColor[i].a = 1.0f; //0.25f;

            for (int i = 0; i < gameData.DyingEnemyCount; i++)
            {
                int enemyIdx = gameData.DyingEnemyIdxs[i];
                if (doEnemyCircleCollision(gameData, balance, collisionRadiusSqr, enemyIdx))
                {
                    // do nothing
                }
            }
        }

        private static bool doEnemyCircleCollision(GameData gameData, Balance balance, Span<float> collisionRadiusSqr, int enemyIdx)
        {
            bool collisionHappened = false;
            for (int c = 0; c < balance.CarBalance[gameData.CarType].CollisionCircles[gameData.CarSlideIndex].Length; c++)
            {
                Vector2 collisionCircle = balance.CarBalance[gameData.CarType].CollisionCircles[gameData.CarSlideIndex][c];
                float collisionRadius = balance.CarBalance[gameData.CarType].CollisionRadius[gameData.CarSlideIndex][c];
                Vector2 diff = gameData.EnemyPosition[enemyIdx] - collisionCircle;
                if (diff.sqrMagnitude <= collisionRadiusSqr[c])
                {
                    Vector2 diffNormalized = diff.normalized;
                    Vector2 pushStartPos = collisionCircle + diffNormalized * collisionRadius;
                    float randomValue = UnityEngine.Random.value * 0.4f + 0.2f;
                    Vector2 pushEndPos = diffNormalized + gameData.CarSlideDirection * gameData.CarVelocity * randomValue;
                    pushEnemy(gameData, enemyIdx, pushStartPos, pushEndPos);

                    collisionHappened = true;
                }
            }
            return collisionHappened;
        }

        private static void pushEnemy(GameData gameData, int enemyIdx, Vector2 pushStartPos, Vector2 pushEndPos)
        {
            gameData.EnemyPosition[enemyIdx] = pushStartPos;
            gameData.EnemyPushStartPos[enemyIdx] = pushStartPos;
            gameData.EnemyPushEndPos[enemyIdx] = pushEndPos;
            gameData.EnemyPushValue[enemyIdx] = 1.0f;
            gameData.EnemyPushRotation[enemyIdx] = UnityEngine.Random.value * 720.0f - 360.0f;
        }

        static void movePlayer(GameData gameData, Balance balance, float dt)
        {
            gameData.PlayerDirection = gameData.PlayerTargetDirection;
            gameData.PlayerDelta = gameData.PlayerDirection * balance.HeroBalance.HeroBalanceData[gameData.HeroType].Velocity * dt;

            moveObjectsAroundPlayer(gameData, dt);
        }

        static void moveCar(GameData gameData, Balance balance, float dt)
        {
            // gameData.PlayerDirection = RotateVector(gameData.PlayerDirection, gameData.CarRotationAngle * dt);

            float angle = Vector2.Angle(gameData.PlayerDirection, gameData.PlayerTargetDirection);
            Vector2 lerpVector = gameData.PlayerDirection;
            float velocityMultiplier = 1.0f;
            if (angle > 0.0f)
            {
                float angleSize = Mathf.Abs(angle / 180.0f);
                float angleVelocity = 180.0f * angleSize * dt * 4.0f; // how fast are we moving towards the angle
                float t = angleVelocity / angle;
                if (t > 1.0f)
                    t = 1.0f;

                lerpVector = Vector2.Lerp(gameData.PlayerDirection, gameData.PlayerTargetDirection, t);
                // velocityMultiplier = lerpVector.magnitude;
                // Debug.Log("moveCar()");
                // Debug.Log("gameData.PlayerDirection " + gameData.PlayerDirection + " gameData.PlayerTargetDirection " + gameData.PlayerTargetDirection + " t " + t);
                // Debug.Log("angleVelocity " + angleVelocity + " angle " + angle + " t " + t);
                // Debug.Log("lerpVector " + lerpVector.ToString() + " t " + t + " velocityMultiplier " + velocityMultiplier + " lerpVector.normalized " + lerpVector.normalized.ToString());
            }

            gameData.PlayerDirection = lerpVector.normalized;
            gameData.CarSlideDirection = (gameData.PlayerDirection + gameData.PlayerTargetDirection) / 2.0f;

            if ((gameData.CarSlideDirection - gameData.PlayerDirection).sqrMagnitude > 0.1f)
                for (int tireIdx = 0; tireIdx < 4; tireIdx++)
                    gameData.CurrentSkidMarkColor[tireIdx].a = 1.0f;

            float playerAngle = Vector2.Angle(Vector2.up, gameData.CarSlideDirection);
            int slideIndex = Mathf.RoundToInt(playerAngle / balance.CarBalance[gameData.CarType].AngleDelta);
            if (slideIndex > balance.CarBalance[gameData.CarType].NumCarFrames - 1)
                slideIndex = balance.CarBalance[gameData.CarType].NumCarFrames - 1;
            gameData.CarSlideIndex = slideIndex;

            gameData.CarVelocity += balance.CarBalance[gameData.CarType].Acceleration * dt;
            if (gameData.CarVelocity > balance.CarBalance[gameData.CarType].Velocity)
                gameData.CarVelocity = balance.CarBalance[gameData.CarType].Velocity;

            gameData.PlayerDelta = gameData.PlayerDirection * gameData.CarVelocity * dt * velocityMultiplier;
            moveObjectsAroundPlayer(gameData, dt);

            placeSkidMark(gameData, balance);
        }

        private static void placeSkidMark(GameData gameData, Balance balance)
        {
            for (int tireIdx = 0; tireIdx < 4; tireIdx++)
            {
                Vector2 currentTirePos = balance.CarBalance[gameData.CarType].Tires[gameData.CarSlideIndex][tireIdx];
                currentTirePos.x *= gameData.CarSlideDirection.x > 0.0f ? 1.0f : -1.0f;
                // currentTirePos.x += UnityEngine.Random.value * 0.05f - 0.025f;
                // currentTirePos.y += UnityEngine.Random.value * 0.05f - 0.025f;

                int index = gameData.CurrentSkidMarkIndex + tireIdx * balance.MaxSkidMarks;
                gameData.SkidMarkPos[index] = currentTirePos;
                gameData.SkidMarkColor[index] = gameData.CurrentSkidMarkColor[tireIdx];
            }
            gameData.SkidMarkAngle[gameData.CurrentSkidMarkIndex] = Vector2.SignedAngle(Vector2.up, gameData.CarSlideDirection);

            gameData.LastSkidMarkIndex = gameData.CurrentSkidMarkIndex;
            gameData.CurrentSkidMarkIndex = (gameData.CurrentSkidMarkIndex + 1) % balance.MaxSkidMarks;
        }

        static void updateTireTracks(GameData gameData, Balance balance, float dt)
        {
            for (int i = 0; i < 4 * balance.MaxSkidMarks; i++)
            {
                gameData.SkidMarkColor[i].a *= 1.0f - ((0.2f * dt) * (0.2f * dt));
                gameData.SkidMarkPos[i] -= gameData.PlayerDelta;
            }

            for (int i = 0; i < 4; i++)
            {
                Color colorDiff = balance.SkidMarkColor - gameData.CurrentSkidMarkColor[i];
                gameData.CurrentSkidMarkColor[i] += colorDiff * 0.25f;
            }
        }

        static void moveObjectsAroundPlayer(GameData gameData, float dt)
        {
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIdx = gameData.AliveEnemyIdxs[i];
                gameData.EnemyPosition[enemyIdx] -= gameData.PlayerDelta;
                gameData.EnemyPushStartPos[enemyIdx] -= gameData.PlayerDelta;
                gameData.EnemyPushEndPos[enemyIdx] -= gameData.PlayerDelta;
            }

            for (int i = 0; i < gameData.DyingEnemyCount; i++)
            {
                int enemyIdx = gameData.DyingEnemyIdxs[i];
                gameData.EnemyPosition[enemyIdx] -= gameData.PlayerDelta;
                gameData.EnemyPushStartPos[enemyIdx] -= gameData.PlayerDelta;
                gameData.EnemyPushEndPos[enemyIdx] -= gameData.PlayerDelta;
            }

            for (int i = 0; i < gameData.AliveEnemyCount; i++)
                for (int j = 0; j < gameData.DyingEnemyCount; j++)
                    if (gameData.AliveEnemyIdxs[i] == gameData.DyingEnemyIdxs[j])
                        Debug.LogError("alive enemy " + i + " and dying enemy " + j + " have the same idxs");

            for (int i = 0; i < gameData.DyingEnemyCount; i++)
                for (int j = i + 1; j < gameData.DyingEnemyCount; j++)
                    if (gameData.DyingEnemyIdxs[i] == gameData.DyingEnemyIdxs[j])
                        Debug.LogError("dying enemy " + i + " and " + j + " have the same idxs " + gameData.DyingEnemyIdxs[i]);


            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                gameData.AmmoPosition[ammoIndex] -= gameData.PlayerDelta;
            }

            for (int i = 0; i < gameData.XPUsedCount; i++)
            {
                int xpIndex = gameData.XPUsedIdxs[i];
                gameData.XPPosition[xpIndex] -= gameData.PlayerDelta;
            }
        }

        static void moveAmmo(GameData gameData, Balance balance, float dt)
        {
            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];

                int enemyIndex = gameData.AmmoTargetIdx[ammoIndex];
                if (enemyIndex > -1)
                    gameData.AmmoTargetPos[ammoIndex] = gameData.EnemyPosition[enemyIndex];

                gameData.AmmoRotationT[ammoIndex] += balance.WeaponBalance.AngularVelocity[gameData.AmmoType[ammoIndex]] * dt;
                if (gameData.AmmoRotationT[ammoIndex] > 1.0f)
                    gameData.AmmoRotationT[ammoIndex] = 1.0f;

                gameData.AmmoDirection[ammoIndex] = Vector2.Lerp(gameData.AmmoDirection[ammoIndex], gameData.AmmoTargetPos[ammoIndex] - gameData.AmmoPosition[ammoIndex], gameData.AmmoRotationT[ammoIndex]).normalized;

                gameData.AmmoPosition[ammoIndex] = gameData.AmmoPosition[ammoIndex] + gameData.AmmoDirection[ammoIndex] * balance.WeaponBalance.Velocity[gameData.AmmoType[ammoIndex]] * dt;
            }
        }

        static void checkAmmoEnemyCollision(
            GameData gameData,
            Balance balance,
            Span<int> dyingEnemyIdxs,
            ref int dyingEnemyCount,
            Span<int> deadAmmoIdxs,
            ref int deadAmmoCount)
        {
            int ammoCount = 0;
            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                bool ammoRemoved = false;
                float distanceSqr = balance.WeaponBalance.TriggerRadius[gameData.AmmoType[ammoIndex]] * balance.WeaponBalance.TriggerRadius[gameData.AmmoType[ammoIndex]];
                int enemyCount = 0;
                for (int j = 0; j < gameData.AliveEnemyCount; j++)
                {
                    int enemyIndex = gameData.AliveEnemyIdxs[j];
                    Vector2 diff = gameData.EnemyPosition[enemyIndex] - gameData.AmmoPosition[ammoIndex];
                    if (diff.sqrMagnitude <= distanceSqr)
                    {
                        // bullet impacted enemy
                        Vector2 pushStartPos = gameData.EnemyPosition[enemyIndex];
                        Vector2 pushEndPos = gameData.EnemyPosition[enemyIndex] + gameData.AmmoDirection[ammoIndex];
                        pushEnemy(gameData, enemyIndex, pushStartPos, pushEndPos);

                        markEnemyDyingCheckForDuplicate(gameData, balance, enemyIndex, dyingEnemyIdxs, ref dyingEnemyCount);

                        // remove weapon
                        ammoRemoved = true;
                    }
                    else
                        gameData.AliveEnemyIdxs[enemyCount++] = enemyIndex;
                }
                gameData.AliveEnemyCount = enemyCount;

                if (ammoRemoved)
                {
                    int ammoType = gameData.AmmoType[ammoIndex];
                    if (balance.WeaponBalance.ExplosionRadius[ammoType] > 0)
                        checkWeaponEnemyExplosion(gameData, balance, ammoType, gameData.AmmoPosition[ammoIndex], dyingEnemyIdxs, ref dyingEnemyCount);

                    deadAmmoIdxs[deadAmmoCount++] = ammoIndex;
                    gameData.DeadAmmoIdx[gameData.DeadAmmoCount++] = ammoIndex;
                }
                else
                    gameData.AliveAmmoIdx[ammoCount++] = ammoIndex;

            }
            gameData.AliveAmmoCount = ammoCount;
        }

        static void checkWeaponEnemyExplosion(
            GameData gameData,
            Balance balance,
            int ammoType,
            Vector2 explosionPos,
            Span<int> dyingEnemyIdxs,
            ref int dyingEnemyCount)
        {
            float explosionRadius = balance.WeaponBalance.ExplosionRadius[ammoType];
            float squareRadius = explosionRadius * explosionRadius;
            int enemyCount = 0;
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIndex = gameData.AliveEnemyIdxs[i];
                if ((gameData.EnemyPosition[enemyIndex] - explosionPos).sqrMagnitude < squareRadius)
                {
                    Vector2 pushStartPos = gameData.EnemyPosition[enemyIndex];
                    Vector2 pushEndPos = gameData.EnemyPosition[enemyIndex] + (gameData.EnemyPosition[enemyIndex] - explosionPos).normalized;
                    pushEnemy(gameData, enemyIndex, pushStartPos, pushEndPos);

                    markEnemyDyingCheckForDuplicate(gameData, balance, enemyIndex, dyingEnemyIdxs, ref dyingEnemyCount);
                }
                else
                    gameData.AliveEnemyIdxs[enemyCount++] = enemyIndex;
            }
            gameData.AliveEnemyCount = enemyCount;
        }

        static void changeEnemyTargetIdxFromWeapons(GameData gameData, Balance balance, int enemyIdx)
        {
            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                if (gameData.AmmoTargetIdx[ammoIndex] == enemyIdx)
                {
                    gameData.AmmoTargetIdx[ammoIndex] = GetClosestEnemyToPlayerIdxNotUsed(gameData);


                    // gameData.AmmoTargetIdx[ammoIndex] = -1;
                    // gameData.AmmoTargetPos[ammoIndex] = gameData.AmmoPosition[ammoIndex] + gameData.AmmoDirection[ammoIndex] * balance.SpawnRadius;
                    //Debug.Log("ammo set new target position. gameData.AmmoDirection[" + ammoIndex + "] " + gameData.AmmoDirection[ammoIndex] + " gameData.AmmoTargetPos[" + ammoIndex + "] " + gameData.AmmoTargetPos[ammoIndex]);
                }
            }
        }

        static void checkAmmoOutOfBounds(GameData gameData, Balance balance, Span<int> deadAmmoIdxs, ref int deadAmmoCount)
        {
            float distanceSqr = balance.SpawnRadius * balance.SpawnRadius * 1.1f;
            int ammoCount = 0;
            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                if (gameData.AmmoPosition[ammoIndex].sqrMagnitude > distanceSqr)
                {
                    deadAmmoIdxs[deadAmmoCount++] = ammoIndex;
                    gameData.DeadAmmoIdx[gameData.DeadAmmoCount++] = ammoIndex;
                }
                else
                    gameData.AliveAmmoIdx[ammoCount++] = ammoIndex;
            }
            gameData.AliveAmmoCount = ammoCount;
        }

        static void checkAmmoReachedDestination(GameData gameData, Balance balance, Span<int> dyingEnemyIdxs, ref int dyingEnemyCount, Span<int> deadAmmoIdxs, ref int deadAmmoCount)
        {
            int ammoCount = 0;
            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                int ammoType = gameData.AmmoType[ammoIndex];
                float radius = balance.WeaponBalance.TriggerRadius[ammoType];
                if ((gameData.AmmoPosition[ammoIndex] - gameData.AmmoTargetPos[ammoIndex]).sqrMagnitude < (radius * radius))
                {
                    checkWeaponEnemyExplosion(gameData, balance, gameData.AmmoType[ammoIndex], gameData.AmmoPosition[ammoIndex], dyingEnemyIdxs, ref dyingEnemyCount);

                    deadAmmoIdxs[deadAmmoCount++] = ammoIndex;
                    gameData.DeadAmmoIdx[gameData.DeadAmmoCount++] = ammoIndex;
                }
                else
                    gameData.AliveAmmoIdx[ammoCount++] = ammoIndex;
            }
            gameData.AliveAmmoCount = ammoCount;
        }

        public static void MouseMovePlayer(GameData gameData, Vector2 mouseDownPos, Vector2 mouseCurrentPos)
        {
            Vector2 newDirection = (mouseCurrentPos - mouseDownPos).normalized;
            gameData.PlayerDirection = gameData.PlayerTargetDirection = newDirection;
        }

        public static void MouseMoveCar(GameData gameData, Vector2 mouseDownPos, Vector2 mouseCurrentPos)
        {
            Vector2 newDirection = (mouseCurrentPos - mouseDownPos).normalized;
            gameData.PlayerTargetDirection = newDirection;
        }


        public static void MouseUpPlayer(GameData gameData)
        {
            gameData.PlayerDirection = gameData.PlayerTargetDirection = Vector2.zero;
        }

        public static void MouseUpCar(GameData gameData)
        {
            // do nothing
            // gameData.CarRotationAngle = 0.0f;            
        }

        public static void SetMenuState(MetaData metaData, MENU_STATE newMenuState)
        {
            metaData.MenuState = newMenuState;
        }
    }
}