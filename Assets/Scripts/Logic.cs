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

            gameData.WeaponPosition = new Vector2[balance.MaxWeapons];
            gameData.WeaponDirection = new Vector2[balance.MaxWeapons];
            gameData.WeaponType = new int[balance.MaxWeapons];
            gameData.WeaponAngle = new float[balance.MaxWeapons];
            gameData.AliveWeaponIdx = new int[balance.MaxWeapons];
            gameData.DeadWeaponIdx = new int[balance.MaxWeapons];
            gameData.WeaponTargetIdx = new int[balance.MaxWeapons];
            gameData.WeaponTargetPos = new Vector2[balance.MaxWeapons];
            gameData.PlayerWeaponFiringRateTimer = new float[balance.MaxWeapons];

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

            gameData.AliveWeaponCount = 0;
            gameData.DeadWeaponCount = balance.MaxWeapons;
            for (int i = 0; i < balance.MaxWeapons; i++)
                gameData.DeadWeaponIdx[i] = (balance.MaxWeapons - 1) - i;

            for (int i = 0; i < balance.MaxWeapons; i++)
                gameData.WeaponTargetIdx[i] = -1;

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

            // TEST
            gameData.PlayerWeaponType[0] = 1;
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
                    if (gameData.AliveEnemyCount > 0 && gameData.PlayerWeaponFiringRateTimer[playerWeaponIndex] < 0.0f && gameData.DeadWeaponCount > 0)
                    {
                        // get free Weapon
                        gameData.PlayerWeaponFiringRateTimer[playerWeaponIndex] += balance.WeaponBalance.FiringRate[playerWeaponType];
                        int weaponIndex = gameData.DeadWeaponIdx[--gameData.DeadWeaponCount];
                        gameData.AliveWeaponIdx[gameData.AliveWeaponCount++] = weaponIndex;
                        firedWeaponIdxs[fireWeaponCount++] = weaponIndex;
                        gameData.WeaponType[weaponIndex] = playerWeaponType;

                        gameData.WeaponPosition[weaponIndex] = Vector2.zero;

                        // get enemy to fire at
                        // int enemyIdx = GetClosestEnemyToPlayerIdx(gameData);
                        int enemyIdx = GetClosestEnemyToPlayerIdxNotUsed(gameData);
                        if (enemyIdx > -1)
                        {
                            gameData.WeaponDirection[weaponIndex] = gameData.LastPlayerDirection;
                            gameData.WeaponTargetIdx[weaponIndex] = enemyIdx;
                            gameData.WeaponTargetPos[weaponIndex] = gameData.EnemyPosition[enemyIdx] + balance.SpawnRadius * gameData.EnemyDirection[enemyIdx] * balance.WeaponBalance.DontRemoveOnHit[playerWeaponType];
                            gameData.WeaponAngle[weaponIndex] = Vector2.SignedAngle(Vector2.up, gameData.LastPlayerDirection);
                        }

                        Debug.Log("Weapon added " + weaponIndex + " gameData.DeadWeaponCount " + gameData.DeadWeaponCount + " gameData.AliveWeaponCount " + gameData.AliveWeaponCount);
                    }
                }
            }
        }

        public static void TestFireWeapon(GameData gameData, Balance balance, float dt, Span<int> firedWeaponIdxs, ref int fireWeaponCount)
        {
            int playerWeaponIndex = 0; // TODO
            gameData.PlayerWeaponFiringRateTimer[playerWeaponIndex] = 0;

            tryFireWeapon(gameData, balance, dt, firedWeaponIdxs, ref fireWeaponCount);
        }

        public static int GetClosestEnemyToPlayerIdxNotUsed(GameData gameData)
        {
            // change to insertion sort, and return closest untargeted value
            int closestEnemyIdx = -1;
            float closestDistanceSqr = float.MaxValue;
            Debug.Log("gameData.AliveEnemyCount " + gameData.AliveEnemyCount);
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIndex = gameData.AliveEnemyIdxs[i];
                float distanceSqr = gameData.EnemyPosition[enemyIndex].sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    bool enemyTargeted = false;
                    for (int weaponIndex = 0; weaponIndex < gameData.AliveWeaponCount; weaponIndex++)
                        if (gameData.WeaponTargetIdx[weaponIndex] == enemyIndex)
                            enemyTargeted = true;

                    if (!enemyTargeted)
                    {
                        closestEnemyIdx = enemyIndex;
                        closestDistanceSqr = distanceSqr;
                        Debug.Log(i + " closestEnemyIdx " + closestEnemyIdx + " closestDistanceSqr " + closestDistanceSqr);
                    }
                }
            }

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

            moveWeapons(gameData, balance, dt);

            checkWeaponOutOfBounds(gameData, balance, deadWeaponIdxs, ref deadWeaponCount);

            checkWeaponEnemyCollision(gameData, balance, deadEnemyIdxs, ref deadEnemyCount, deadWeaponIdxs, ref deadWeaponCount);

            checkWeaponReachedDestination(gameData, balance, deadEnemyIdxs, ref deadEnemyCount, deadWeaponIdxs, ref deadWeaponCount);

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

        // TODO
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

        static void removeEnemy(GameData gameData, int enemyIdx, Span<int> deadEnemyIdxs, ref int deadEnemyCount)
        {
            RemoveIndexFromArray(gameData.AliveEnemyIdxs, ref gameData.AliveEnemyCount, enemyIdx);
            gameData.DeadEnemyIdxs[gameData.DeadEnemyCount++] = enemyIdx;

            removeEnemyTargetIdxFromWeapons(gameData, enemyIdx);
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
                        removeEnemy(gameData, enemyIdx, deadEnemyIdxs, ref deadEnemyCount);
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

            for (int i = 0; i < gameData.AliveWeaponCount; i++)
            {
                int weaponIdx = gameData.AliveWeaponIdx[i];
                gameData.WeaponPosition[weaponIdx] -= playerPosition;
            }
        }

        static void moveWeapons(GameData gameData, Balance balance, float dt)
        {
            for (int i = 0; i < gameData.AliveWeaponCount; i++)
            {
                int weaponIndex = gameData.AliveWeaponIdx[i];

                int enemyIndex = gameData.WeaponTargetIdx[weaponIndex];
                if (enemyIndex > -1)
                    gameData.WeaponTargetPos[weaponIndex] = gameData.EnemyPosition[enemyIndex];

                float targetAngle = Vector2.SignedAngle(Vector2.up, gameData.WeaponTargetPos[weaponIndex] - gameData.WeaponPosition[weaponIndex]);
                float angleOffset = targetAngle - gameData.WeaponAngle[weaponIndex];

                if (angleOffset < -180.0f)
                    angleOffset += 360.0f;
                else if (angleOffset > 180.0f)
                    angleOffset -= 360.0f;

                float angleDelta = balance.WeaponBalance.AngularVelocity[gameData.WeaponType[weaponIndex]] * dt;

                // sharp turns in case destination is at a sharp angle
                if (Mathf.Abs(angleOffset) >= 45.0f)
                    angleDelta *= Mathf.Abs(angleOffset) / 45.0f;

                // Debug.Log("weaponIdx " + weaponIdx + " targetAngle " + targetAngle + " angleOffset " + angleOffset + " angleDelta " + angleDelta);
                if (angleDelta > Mathf.Abs(angleOffset))
                {
                    gameData.WeaponAngle[weaponIndex] = targetAngle;
                    gameData.WeaponDirection[weaponIndex] = (gameData.WeaponTargetPos[weaponIndex] - gameData.WeaponPosition[weaponIndex]).normalized;
                }
                else
                {
                    if (angleOffset < 0.0f)
                        gameData.WeaponAngle[weaponIndex] -= angleDelta;
                    else if (angleOffset > 0.0f)
                        gameData.WeaponAngle[weaponIndex] += angleDelta;

                    gameData.WeaponDirection[weaponIndex] = RotateVector(Vector2.up, gameData.WeaponAngle[weaponIndex]).normalized;
                }

                gameData.WeaponPosition[weaponIndex] = gameData.WeaponPosition[weaponIndex] + gameData.WeaponDirection[weaponIndex] * balance.WeaponBalance.Velocity[gameData.WeaponType[weaponIndex]] * dt;
            }
        }

        static void checkWeaponEnemyCollision(GameData gameData, Balance balance, Span<int> deadEnemyIdxs, ref int deadEnemyCount, Span<int> deadWeaponIdxs, ref int deadWeaponCount)
        {
            for (int i = 0; i < gameData.AliveWeaponCount; i++)
            {
                int weaponIdx = gameData.AliveWeaponIdx[i];
                float distanceSqr = balance.WeaponBalance.TriggerRadius[gameData.WeaponType[weaponIdx]] * balance.WeaponBalance.TriggerRadius[gameData.WeaponType[weaponIdx]];
                for (int j = 0; j < gameData.AliveEnemyCount; j++)
                {
                    int enemyIndex = gameData.AliveEnemyIdxs[j];
                    Vector2 diff = gameData.EnemyPosition[enemyIndex] - gameData.WeaponPosition[weaponIdx];
                    if (diff.sqrMagnitude <= distanceSqr)
                    {
                        // bullet impacted enemy

                        removeEnemy(gameData, enemyIndex, deadEnemyIdxs, ref deadEnemyCount);

                        checkWeaponEnemyExplosion(gameData, balance, gameData.WeaponType[weaponIdx], gameData.WeaponPosition[weaponIdx], deadEnemyIdxs, ref deadEnemyCount);

                        // remove weapon
                        if (balance.WeaponBalance.DontRemoveOnHit[gameData.WeaponType[weaponIdx]] <= 0.0f)
                        {
                            deadWeaponIdxs[deadWeaponCount++] = weaponIdx;
                            RemoveIndexFromArray(gameData.AliveWeaponIdx, ref gameData.AliveWeaponCount, weaponIdx);
                            gameData.DeadWeaponIdx[gameData.DeadWeaponCount++] = weaponIdx;
                        }

                        Debug.Log("Weapon removed " + weaponIdx + " gameData.DeadWeaponCount " + gameData.DeadWeaponCount + " gameData.AliveWeaponCount " + gameData.AliveWeaponCount);
                        break;
                    }
                }
            }
        }

        static void checkWeaponEnemyExplosion(GameData gameData, Balance balance, int weaponType, Vector2 explosionPos, Span<int> deadEnemyIdxs, ref int deadEnemyCount)
        {
            float explosionRadius = balance.WeaponBalance.ExplosionRadius[weaponType];
            float squareRadius = explosionRadius * explosionRadius;
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIdx = gameData.AliveEnemyIdxs[i];
                if ((gameData.EnemyPosition[enemyIdx] - explosionPos).sqrMagnitude < squareRadius)
                {
                    removeEnemy(gameData, enemyIdx, deadEnemyIdxs, ref deadEnemyCount);
                }
            }
        }

        static void removeEnemyTargetIdxFromWeapons(GameData gameData, int enemyIdx)
        {
            for (int i = 0; i < gameData.AliveWeaponCount; i++)
            {
                int weaponIdx = gameData.AliveWeaponIdx[i];
                if (gameData.WeaponTargetIdx[weaponIdx] == enemyIdx)
                {
                    gameData.WeaponTargetIdx[weaponIdx] = -1;
                    gameData.WeaponTargetPos[weaponIdx] = gameData.EnemyPosition[enemyIdx];
                }
            }
        }

        static void checkWeaponOutOfBounds(GameData gameData, Balance balance, Span<int> deadWeaponIdxs, ref int deadWeaponCount)
        {
            float distanceSqr = balance.SpawnRadius * balance.SpawnRadius * 1.1f;
            for (int i = 0; i < gameData.AliveWeaponCount; i++)
            {
                int weaponIdx = gameData.AliveWeaponIdx[i];
                if (gameData.WeaponPosition[weaponIdx].sqrMagnitude > distanceSqr)
                {
                    deadWeaponIdxs[deadWeaponCount++] = weaponIdx;
                    RemoveIndexFromArray(gameData.AliveWeaponIdx, ref gameData.AliveWeaponCount, weaponIdx);
                    gameData.DeadWeaponIdx[gameData.DeadWeaponCount++] = weaponIdx;

                    Debug.Log("Weapon out of bounds " + weaponIdx + " gameData.DeadWeaponCount " + gameData.DeadWeaponCount + " gameData.AliveWeaponCount " + gameData.AliveWeaponCount);
                }
            }
        }

        static void checkWeaponReachedDestination(GameData gameData, Balance balance, Span<int> deadEnemyIdxs, ref int deadEnemyCount, Span<int> deadWeaponIdxs, ref int deadWeaponCount)
        {
            for (int i = 0; i < gameData.AliveWeaponCount; i++)
            {
                int weaponIndex = gameData.AliveWeaponIdx[i];
                if ((gameData.WeaponPosition[weaponIndex] - gameData.WeaponTargetPos[weaponIndex]).sqrMagnitude < 0.1f)
                {
                    checkWeaponEnemyExplosion(gameData, balance, gameData.WeaponType[weaponIndex], gameData.WeaponPosition[weaponIndex], deadEnemyIdxs, ref deadEnemyCount);

                    deadWeaponIdxs[deadWeaponCount++] = weaponIndex;
                    RemoveIndexFromArray(gameData.AliveWeaponIdx, ref gameData.AliveWeaponCount, weaponIndex);
                    gameData.DeadWeaponIdx[gameData.DeadWeaponCount++] = weaponIndex;

                    Debug.Log("Weapon reached destination " + weaponIndex + " gameData.DeadWeaponCount " + gameData.DeadWeaponCount + " gameData.AliveWeaponCount " + gameData.AliveWeaponCount);
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
            gameData.LastPlayerDirection = gameData.PlayerDirection;
            gameData.PlayerDirection = (mouseCurrentPos - mouseDownPos).normalized;
        }

        public static void MouseUp(GameData gameData)
        {
            gameData.LastPlayerDirection = gameData.PlayerDirection;
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