using UnityEngine;
using System;
using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.Burst.Intrinsics;

namespace Survivor
{
    public static class Logic
    {
        public static void AllocateGameData(GameData gameData, Balance balance)
        {
            gameData.EnemyPosition = new Vector2[balance.MaxEnemies];
            gameData.EnemyDirection = new Vector2[balance.MaxEnemies];
            gameData.EnemyType = new int[balance.MaxEnemies];
            gameData.AliveEnemyIdxs = new int[balance.MaxEnemies];
            gameData.DeadEnemyIdxs = new int[balance.MaxEnemies];

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
        }

        public static void Init(MetaData metaData)
        {
            metaData.MenuState = MENU_STATE.NONE;
        }

        public static void StartGame(GameData gameData, Balance balance)
        {
            gameData.InGame = true;

            gameData.GameTime = 0.0f;

            gameData.PlayerDirection = Vector2.zero;
            gameData.LastPlayerDirection = Vector2.right;

            gameData.AliveEnemyCount = 0;
            gameData.DeadEnemyCount = balance.MaxEnemies;
            for (int i = 0; i < balance.MaxEnemies; i++)
                gameData.DeadEnemyIdxs[i] = (balance.MaxEnemies - 1) - i;

            gameData.AliveAmmoCount = 0;
            gameData.DeadAmmoCount = balance.MaxAmmo;
            for (int i = 0; i < balance.MaxAmmo; i++)
                gameData.DeadAmmoIdx[i] = (balance.MaxAmmo - 1) - i;

            for (int i = 0; i < balance.MaxAmmo; i++)
                gameData.AmmoTargetIdx[i] = -1;

            gameData.PlayerType = 0;

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

            // TEST - no way to assign them in game yet
            gameData.PlayerWeaponType[0] = 0;
            gameData.PlayerWeaponType[1] = 1;
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
                                    int enemyIdx = GetClosestEnemyToPlayerIdxNotUsed(gameData);
                                    if (enemyIdx > -1)
                                    {
                                        gameData.AmmoDirection[ammoIndex] = gameData.LastPlayerDirection.sqrMagnitude > 0.0f ? gameData.LastPlayerDirection : Vector2.up;
                                        gameData.AmmoTargetIdx[ammoIndex] = enemyIdx;
                                        gameData.AmmoTargetPos[ammoIndex] = gameData.EnemyPosition[enemyIdx] + balance.SpawnRadius * gameData.AmmoDirection[ammoIndex] * balance.WeaponBalance.DontRemoveOnHit[playerWeaponType];
                                        gameData.AmmoRotationT[ammoIndex] = 0.0f;
                                    }
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

            Debug.Log(closestEnemyIdx + " closestEnemyIdx ");
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
            Span<int> firedWeaponIdxs,
            out int firedWeaponCount,
            Span<int> deadWeaponIdxs,
            out int deadWeaponCount,
            out bool gameOver)
        {
            gameData.GameTime += dt;

            deadEnemyCount = 0;
            spawnedEnemyCount = 0;
            firedWeaponCount = 0;
            deadWeaponCount = 0;

            // Weapon
            tryFireWeapon(gameData, balance, dt, firedWeaponIdxs, ref firedWeaponCount);

            moveAmmo(gameData, balance, dt);

            checkAmmoOutOfBounds(gameData, balance, deadWeaponIdxs, ref deadWeaponCount);

            checkAmmoEnemyCollision(gameData, balance, deadEnemyIdxs, ref deadEnemyCount, deadWeaponIdxs, ref deadWeaponCount);

            checkAmmoReachedDestination(gameData, balance, deadEnemyIdxs, ref deadEnemyCount, deadWeaponIdxs, ref deadWeaponCount);

            // enemies
            trySpawnEnemy(gameData, balance, dt, spawnedEnemyIdxs, ref spawnedEnemyCount);

            moveEnemies(gameData, balance, dt);

            checkEnemyOutOfBounds(gameData, balance, deadEnemyIdxs, ref deadEnemyCount);

            doEnemyToEnemyCollision(gameData, balance);

            // player
            movePlayer(gameData, balance, dt);

            gameOver = checkGameOver(metaData, gameData, balance);
        }

        static void trySpawnEnemy(GameData gameData, Balance balance, float dt, Span<int> spawnedEnemyIdxs, ref int spawnedEnemyCount)
        {
            for (int waveIdx = 0; waveIdx < balance.LevelBalance[gameData.Level].TotalWaves; waveIdx++)
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

        static void removeEnemy(GameData gameData, Balance balance, int enemyIdx, Span<int> deadEnemyIdxs, ref int deadEnemyCount)
        {
            RemoveIndexFromArray(gameData.AliveEnemyIdxs, ref gameData.AliveEnemyCount, enemyIdx);
            gameData.DeadEnemyIdxs[gameData.DeadEnemyCount++] = enemyIdx;

            removeEnemyTargetIdxFromWeapons(gameData, balance, enemyIdx);
            deadEnemyIdxs[deadEnemyCount++] = enemyIdx;
        }

        static void moveEnemies(GameData gameData, Balance balance, float dt)
        {
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIndex = gameData.AliveEnemyIdxs[i];
                Vector2 dir = -gameData.EnemyPosition[enemyIndex].normalized;
                gameData.EnemyPosition[enemyIndex] = gameData.EnemyPosition[enemyIndex] + dir * balance.EnemyBalance.Velocity[gameData.EnemyType[enemyIndex]] * dt;
            }
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
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIdx = gameData.AliveEnemyIdxs[i];
                if (gameData.EnemyPosition[enemyIdx].sqrMagnitude > distanceSqr)
                    if (!tryRespawnEnemy(gameData, balance, enemyIdx))
                        removeEnemy(gameData, balance, enemyIdx, deadEnemyIdxs, ref deadEnemyCount);
            }
        }

        static void movePlayer(GameData gameData, Balance balance, float dt)
        {
            Vector2 playerPosition = gameData.PlayerDirection * balance.PlayerBalance.PlayerBalanceData[gameData.PlayerType].Velocity * dt;

            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIdx = gameData.AliveEnemyIdxs[i];
                gameData.EnemyPosition[enemyIdx] -= playerPosition;
            }

            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                gameData.AmmoPosition[ammoIndex] -= playerPosition;
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

        static void checkAmmoEnemyCollision(GameData gameData, Balance balance, Span<int> deadEnemyIdxs, ref int deadEnemyCount, Span<int> deadWeaponIdxs, ref int deadWeaponCount)
        {
            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                float distanceSqr = balance.WeaponBalance.TriggerRadius[gameData.AmmoType[ammoIndex]] * balance.WeaponBalance.TriggerRadius[gameData.AmmoType[ammoIndex]];
                for (int j = 0; j < gameData.AliveEnemyCount; j++)
                {
                    int enemyIndex = gameData.AliveEnemyIdxs[j];
                    Vector2 diff = gameData.EnemyPosition[enemyIndex] - gameData.AmmoPosition[ammoIndex];
                    if (diff.sqrMagnitude <= distanceSqr)
                    {
                        // bullet impacted enemy

                        removeEnemy(gameData, balance, enemyIndex, deadEnemyIdxs, ref deadEnemyCount);

                        int ammoType = gameData.AmmoType[ammoIndex];
                        if (balance.WeaponBalance.ExplosionRadius[ammoType] > 0)
                            checkWeaponEnemyExplosion(gameData, balance, ammoType, gameData.AmmoPosition[ammoIndex], deadEnemyIdxs, ref deadEnemyCount);

                        // remove weapon
                        if (balance.WeaponBalance.DontRemoveOnHit[gameData.AmmoType[ammoIndex]] <= 0.0f)
                        {
                            deadWeaponIdxs[deadWeaponCount++] = ammoIndex;
                            RemoveIndexFromArray(gameData.AliveAmmoIdx, ref gameData.AliveAmmoCount, ammoIndex);
                            gameData.DeadAmmoIdx[gameData.DeadAmmoCount++] = ammoIndex;
                        }

                        break;
                    }
                }
            }
        }

        static void checkWeaponEnemyExplosion(GameData gameData, Balance balance, int ammoType, Vector2 explosionPos, Span<int> deadEnemyIdxs, ref int deadEnemyCount)
        {
            float explosionRadius = balance.WeaponBalance.ExplosionRadius[ammoType];
            float squareRadius = explosionRadius * explosionRadius;
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIdx = gameData.AliveEnemyIdxs[i];
                if ((gameData.EnemyPosition[enemyIdx] - explosionPos).sqrMagnitude < squareRadius)
                {
                    removeEnemy(gameData, balance, enemyIdx, deadEnemyIdxs, ref deadEnemyCount);
                }
            }
        }

        static void removeEnemyTargetIdxFromWeapons(GameData gameData, Balance balance, int enemyIdx)
        {
            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                if (gameData.AmmoTargetIdx[ammoIndex] == enemyIdx)
                {
                    gameData.AmmoTargetIdx[ammoIndex] = -1;
                    gameData.AmmoTargetPos[ammoIndex] = gameData.AmmoPosition[ammoIndex] + gameData.AmmoDirection[ammoIndex] * balance.SpawnRadius;
                    Debug.Log("ammo set new target position. gameData.AmmoDirection[" + ammoIndex + "] " + gameData.AmmoDirection[ammoIndex] + " gameData.AmmoTargetPos[" + ammoIndex + "] " + gameData.AmmoTargetPos[ammoIndex]);
                }
            }
        }

        static void checkAmmoOutOfBounds(GameData gameData, Balance balance, Span<int> deadWeaponIdxs, ref int deadWeaponCount)
        {
            float distanceSqr = balance.SpawnRadius * balance.SpawnRadius * 1.1f;
            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                if (gameData.AmmoPosition[ammoIndex].sqrMagnitude > distanceSqr)
                {
                    deadWeaponIdxs[deadWeaponCount++] = ammoIndex;
                    RemoveIndexFromArray(gameData.AliveAmmoIdx, ref gameData.AliveAmmoCount, ammoIndex);
                    gameData.DeadAmmoIdx[gameData.DeadAmmoCount++] = ammoIndex;
                }
            }
        }

        static void checkAmmoReachedDestination(GameData gameData, Balance balance, Span<int> deadEnemyIdxs, ref int deadEnemyCount, Span<int> deadWeaponIdxs, ref int deadWeaponCount)
        {
            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                int ammoType = gameData.AmmoType[ammoIndex];
                float radius = balance.WeaponBalance.TriggerRadius[ammoType];
                if ((gameData.AmmoPosition[ammoIndex] - gameData.AmmoTargetPos[ammoIndex]).sqrMagnitude < (radius * radius))
                {
                    checkWeaponEnemyExplosion(gameData, balance, gameData.AmmoType[ammoIndex], gameData.AmmoPosition[ammoIndex], deadEnemyIdxs, ref deadEnemyCount);

                    deadWeaponIdxs[deadWeaponCount++] = ammoIndex;
                    RemoveIndexFromArray(gameData.AliveAmmoIdx, ref gameData.AliveAmmoCount, ammoIndex);
                    gameData.DeadAmmoIdx[gameData.DeadAmmoCount++] = ammoIndex;
                }
            }
        }

        public static void RemoveIndexFromArray(int[] array, ref int arrayCount, int index)
        {
            int count = 0;
            for (int i = 0; i < arrayCount; i++)
                if (array[i] != index)
                    array[count++] = array[i];
            arrayCount = count;
        }


        public static void MouseMove(GameData gameData, Vector2 mouseDownPos, Vector2 mouseCurrentPos)
        {
            gameData.LastPlayerDirection = gameData.PlayerDirection.sqrMagnitude > 0.0f ? gameData.PlayerDirection : gameData.LastPlayerDirection;
            gameData.PlayerDirection = (mouseCurrentPos - mouseDownPos).normalized;
        }

        public static void MouseUp(GameData gameData)
        {
            gameData.LastPlayerDirection = gameData.PlayerDirection.sqrMagnitude > 0.0f ? gameData.PlayerDirection : gameData.LastPlayerDirection;
            gameData.PlayerDirection = Vector2.zero;
        }

        static bool checkGameOver(MetaData metaData, GameData gameData, Balance balance)
        {
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIdx = gameData.AliveEnemyIdxs[i];
                if (gameData.EnemyPosition[enemyIdx].magnitude < balance.MinCollisionDistance)
                {
                    if (gameData.GameTime > metaData.BestTime)
                        metaData.BestTime = gameData.GameTime;

                    gameData.InGame = false;
                    return true;
                }
            }
            return false;
        }

        public static void SetMenuState(MetaData metaData, MENU_STATE newMenuState)
        {
            metaData.MenuState = newMenuState;
        }
    }
}