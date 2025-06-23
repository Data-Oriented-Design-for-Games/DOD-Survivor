using UnityEngine;
using System;
using System.Security.Cryptography;
using Unity.Mathematics;

namespace Survivor
{
    public static class Logic
    {
        public static void AllocateGameData(GameData gameData, Balance balance)
        {
            gameData.EnemyPosition = new Vector2[balance.MaxEnemies];
            gameData.EnemyDirection = new Vector2[balance.MaxEnemies];

            gameData.WeaponPosition = new Vector2[balance.MaxWeapons];
            gameData.WeaponDirection = new Vector2[balance.MaxWeapons];
            gameData.WeaponAngle = new float[balance.MaxWeapons];
            gameData.AliveWeaponIdx = new int[balance.MaxWeapons];
            gameData.DeadWeaponIdx = new int[balance.MaxWeapons];
            gameData.WeaponTargetIdx = new int[balance.MaxWeapons];
            gameData.WeaponTargetPos = new Vector2[balance.MaxWeapons];
            gameData.FiringRateTimer = new float[balance.MaxWeapons];
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

            for (int i = 0; i < balance.MaxEnemies; i++)
                gameData.EnemyPosition[i] = spawnEnemy(gameData, balance);

            gameData.AliveWeaponCount = 0;
            gameData.DeadWeaponCount = balance.MaxWeapons;
            for (int i = 0; i < balance.MaxWeapons; i++)
                gameData.DeadWeaponIdx[i] = (balance.MaxWeapons - 1) - i;

            for (int i = 0; i < balance.MaxWeapons; i++)
                gameData.WeaponTargetIdx[i] = -1;

            gameData.PlayerType = 0;
            gameData.EnemyType = 0;
            gameData.WeaponType = 1;
        }

        static Vector2 spawnEnemy(GameData gameData, Balance balance)
        {
            Vector2 direction = gameData.PlayerDirection;
            float angle = UnityEngine.Random.value * 180.0f - 90.0f;
            if (direction.magnitude == 0.0f)
            {
                direction = new Vector2(0.0f, 1.0f);
                angle = UnityEngine.Random.value * 360.0f;
            }
            direction = RotateVector(direction, angle);
            return direction.normalized * balance.SpawnRadius;
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
            gameData.FiringRateTimer[gameData.WeaponType] -= dt;
            if (gameData.FiringRateTimer[gameData.WeaponType] < 0.0f && gameData.DeadWeaponCount > 0)
            {
                // get free Weapon
                gameData.FiringRateTimer[gameData.WeaponType] += balance.WeaponBalance.FiringRate[gameData.WeaponType];
                int weaponIdx = gameData.DeadWeaponIdx[--gameData.DeadWeaponCount];
                gameData.AliveWeaponIdx[gameData.AliveWeaponCount++] = weaponIdx;
                firedWeaponIdxs[fireWeaponCount++] = weaponIdx;

                gameData.WeaponPosition[weaponIdx] = Vector2.zero;

                // get enemy to fire at
                int enemyIdx = GetClosestEnemyToPlayerIdx(gameData, balance);
                gameData.WeaponDirection[weaponIdx] = gameData.LastPlayerDirection;
                gameData.WeaponTargetIdx[weaponIdx] = enemyIdx;
                gameData.WeaponTargetPos[weaponIdx] = gameData.EnemyPosition[enemyIdx] + balance.SpawnRadius * gameData.EnemyDirection[enemyIdx] * balance.WeaponBalance.DontRemoveOnHit[gameData.WeaponType];
                gameData.WeaponAngle[weaponIdx] = Vector2.SignedAngle(Vector2.up, gameData.LastPlayerDirection);

                Debug.Log("Weapon added " + weaponIdx + " gameData.DeadWeaponCount " + gameData.DeadWeaponCount + " gameData.AliveWeaponCount " + gameData.AliveWeaponCount);
            }
        }

        public static void TestFireWeapon(GameData gameData, Balance balance, float dt, Span<int> firedWeaponIdxs, ref int fireWeaponCount)
        {
            gameData.FiringRateTimer[gameData.WeaponType] = 0;

            tryFireWeapon(gameData, balance, dt, firedWeaponIdxs, ref fireWeaponCount);
        }

        public static int GetClosestEnemyToPlayerIdx(GameData gameData, Balance balance)
        {
            int enemyIdx = 0;
            float closestDistanceSqr = float.MaxValue;
            for (int i = 0; i < balance.MaxEnemies; i++)
            {
                float distanceSqr = gameData.EnemyPosition[i].sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    enemyIdx = i;
                    closestDistanceSqr = distanceSqr;
                }
            }

            return enemyIdx;
        }

        public static void Tick(
            MetaData metaData,
            GameData gameData,
            Balance balance,
            float dt,
            Span<int> firedWeaponIdxs,
            out int firedWeaponCount,
            Span<int> deadWeaponIdxs,
            out int deadWeaponCount,
            out bool gameOver)
        {
            gameData.GameTime += dt;

            firedWeaponCount = 0;
            deadWeaponCount = 0;

            // Weapon
            tryFireWeapon(gameData, balance, dt, firedWeaponIdxs, ref firedWeaponCount);

            moveWeapons(gameData, balance, dt);

            checkWeaponOutOfBounds(gameData, balance, deadWeaponIdxs, ref deadWeaponCount);

            checkWeaponEnemyCollision(gameData, balance, deadWeaponIdxs, ref deadWeaponCount);

            checkWeaponReachedDestination(gameData, balance, deadWeaponIdxs, ref deadWeaponCount);

            // enemies
            moveEnemies(gameData, balance, dt);

            checkEnemyOutOfBounds(gameData, balance);

            doEnemyToEnemyCollision(gameData, balance);

            // player
            movePlayer(gameData, balance, dt);

            gameOver = checkGameOver(metaData, gameData, balance);
        }

        static void moveEnemies(GameData gameData, Balance balance, float dt)
        {
            for (int i = 0; i < balance.MaxEnemies; i++)
            {
                Vector2 dir = -gameData.EnemyPosition[i].normalized;
                gameData.EnemyPosition[i] = gameData.EnemyPosition[i] + dir * balance.EnemyBalance.Velocity[gameData.EnemyType] * dt;
            }
        }

        static void doEnemyToEnemyCollision(GameData gameData, Balance balance)
        {
            float diameter = balance.EnemyBalance.Radius[gameData.EnemyType] + balance.EnemyBalance.Radius[gameData.EnemyType];
            float diameterSqr = diameter * diameter;
            for (int enemyIdx1 = 0; enemyIdx1 < balance.MaxEnemies; enemyIdx1++)
            {
                for (int enemyIdx2 = enemyIdx1 + 1; enemyIdx2 < balance.MaxEnemies; enemyIdx2++)
                {
                    Vector2 diff = gameData.EnemyPosition[enemyIdx1] - gameData.EnemyPosition[enemyIdx2];
                    if (diff.sqrMagnitude <= diameterSqr)
                    {
                        Vector2 diffNormalized = diff.normalized;
                        Vector2 midPoint = (gameData.EnemyPosition[enemyIdx1] + gameData.EnemyPosition[enemyIdx2]) / 2.0f;
                        gameData.EnemyPosition[enemyIdx1] = midPoint + diffNormalized * balance.EnemyBalance.Radius[gameData.EnemyType];
                        gameData.EnemyPosition[enemyIdx2] = midPoint - diffNormalized * balance.EnemyBalance.Radius[gameData.EnemyType];
                    }
                }
            }
        }

        static void checkEnemyOutOfBounds(GameData gameData, Balance balance)
        {
            float distanceSqr = balance.SpawnRadius * balance.SpawnRadius * 1.1f;
            for (int i = 0; i < balance.MaxEnemies; i++)
                if (gameData.EnemyPosition[i].sqrMagnitude > distanceSqr)
                    gameData.EnemyPosition[i] = spawnEnemy(gameData, balance);
        }

        static void movePlayer(GameData gameData, Balance balance, float dt)
        {
            Vector2 playerPosition = gameData.PlayerDirection * balance.PlayerBalance.PlayerBalanceData[gameData.PlayerType].Velocity * dt;

            for (int i = 0; i < balance.MaxEnemies; i++)
                gameData.EnemyPosition[i] -= playerPosition;

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
                int weaponIdx = gameData.AliveWeaponIdx[i];

                int enemyIdx = gameData.WeaponTargetIdx[weaponIdx];
                if (enemyIdx > -1)
                    gameData.WeaponTargetPos[weaponIdx] = gameData.EnemyPosition[enemyIdx];

                float targetAngle = Vector2.SignedAngle(Vector2.up, gameData.WeaponTargetPos[weaponIdx] - gameData.WeaponPosition[weaponIdx]);
                float angleOffset = targetAngle - gameData.WeaponAngle[weaponIdx];

                if (angleOffset < -180.0f)
                    angleOffset += 360.0f;
                else if (angleOffset > 180.0f)
                    angleOffset -= 360.0f;

                float angleDelta = balance.WeaponBalance.AngularVelocity[gameData.WeaponType] * dt;

                // sharp turns in case destination is at a sharp angle
                if (Mathf.Abs(angleOffset) >= 45.0f)
                    angleDelta *= Mathf.Abs(angleOffset) / 45.0f;

                // Debug.Log("weaponIdx " + weaponIdx + " targetAngle " + targetAngle + " angleOffset " + angleOffset + " angleDelta " + angleDelta);
                if (angleDelta > Mathf.Abs(angleOffset))
                {
                    gameData.WeaponAngle[weaponIdx] = targetAngle;
                    gameData.WeaponDirection[weaponIdx] = (gameData.WeaponTargetPos[weaponIdx] - gameData.WeaponPosition[weaponIdx]).normalized;
                }
                else
                {
                    if (angleOffset < 0.0f)
                        gameData.WeaponAngle[weaponIdx] -= angleDelta;
                    else if (angleOffset > 0.0f)
                        gameData.WeaponAngle[weaponIdx] += angleDelta;

                    gameData.WeaponDirection[weaponIdx] = RotateVector(Vector2.up, gameData.WeaponAngle[weaponIdx]).normalized;
                }

                gameData.WeaponPosition[weaponIdx] = gameData.WeaponPosition[weaponIdx] + gameData.WeaponDirection[weaponIdx] * balance.WeaponBalance.Velocity[gameData.WeaponType] * dt;
            }
        }

        static void checkWeaponEnemyCollision(GameData gameData, Balance balance, Span<int> deadWeaponIdxs, ref int deadWeaponCount)
        {
            float distanceSqr = balance.WeaponBalance.TriggerRadius[gameData.WeaponType] * balance.WeaponBalance.TriggerRadius[gameData.WeaponType];
            for (int i = 0; i < gameData.AliveWeaponCount; i++)
            {
                int weaponIdx = gameData.AliveWeaponIdx[i];
                for (int enemyIdx = 0; enemyIdx < balance.MaxEnemies; enemyIdx++)
                {
                    Vector2 diff = gameData.EnemyPosition[enemyIdx] - gameData.WeaponPosition[weaponIdx];
                    if (diff.sqrMagnitude <= distanceSqr)
                    {
                        // bullet impacted enemy

                        removeEnemy(gameData, balance, enemyIdx);

                        checkWeaponEnemyExplosion(gameData, balance, gameData.WeaponPosition[weaponIdx]);

                        // remove weapon
                        if (balance.WeaponBalance.DontRemoveOnHit[gameData.WeaponType] <= 0.0f)
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

        static void removeEnemy(GameData gameData, Balance balance, int enemyIdx)
        {
            // go through all weapons, remove enemy from targetIdx, set position instead
            removeEnemyTargetIdxFromWeapons(gameData, enemyIdx);

            // respawn enemy
            gameData.EnemyPosition[enemyIdx] = spawnEnemy(gameData, balance);
        }

        static void checkWeaponEnemyExplosion(GameData gameData, Balance balance, Vector2 explosionPos)
        {
            float explosionRadius = balance.WeaponBalance.ExplosionRadius[gameData.WeaponType];
            float squareRadius = explosionRadius * explosionRadius;
            for (int enemyIdx = 0; enemyIdx < balance.MaxEnemies; enemyIdx++)
            {
                if ((gameData.EnemyPosition[enemyIdx] - explosionPos).sqrMagnitude < squareRadius)
                {
                    removeEnemy(gameData, balance, enemyIdx);
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

        static void checkWeaponReachedDestination(GameData gameData, Balance balance, Span<int> deadWeaponIdxs, ref int deadWeaponCount)
        {
            float distanceSqr = balance.SpawnRadius * balance.SpawnRadius * 1.1f;
            for (int i = 0; i < gameData.AliveWeaponCount; i++)
            {
                int weaponIdx = gameData.AliveWeaponIdx[i];
                if ((gameData.WeaponPosition[weaponIdx] - gameData.WeaponTargetPos[weaponIdx]).sqrMagnitude < 0.1f)
                {
                    checkWeaponEnemyExplosion(gameData, balance, gameData.WeaponPosition[weaponIdx]);

                    deadWeaponIdxs[deadWeaponCount++] = weaponIdx;
                    RemoveIndexFromArray(gameData.AliveWeaponIdx, ref gameData.AliveWeaponCount, weaponIdx);
                    gameData.DeadWeaponIdx[gameData.DeadWeaponCount++] = weaponIdx;

                    Debug.Log("Weapon reached destination " + weaponIdx + " gameData.DeadWeaponCount " + gameData.DeadWeaponCount + " gameData.AliveWeaponCount " + gameData.AliveWeaponCount);
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
            for (int i = 0; i < balance.MaxEnemies; i++)
                if (gameData.EnemyPosition[i].magnitude < balance.MinCollisionDistance)
                {
                    if (gameData.GameTime > metaData.BestTime)
                        metaData.BestTime = gameData.GameTime;

                    gameData.InGame = false;
                    return true;
                }
            return false;
        }

        public static void SetMenuState(MetaData metaData, MENU_STATE newMenuState)
        {
            metaData.MenuState = newMenuState;
        }
    }
}