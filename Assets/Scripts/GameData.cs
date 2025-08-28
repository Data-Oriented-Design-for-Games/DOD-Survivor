using System;
using UnityEngine;

namespace Survivor
{
    public class GameData
    {
        public bool InGame;

        public Vector2[] EnemyPosition;
        public float[] EnemyRotation;
        public float[] EnemyVelocity;

        public Vector2[] EnemyPushStartPos;
        public Vector2[] EnemyPushEndPos;
        public float[] EnemyPushValue;
        public float[] EnemyPushRotation;

        public int[] EnemyType;
        public int[] AliveEnemyIdxs;
        public int AliveEnemyCount;
        public int[] DyingEnemyIdxs;
        public float[] DyingEnemyTimer;
        public int DyingEnemyCount;
        public int[] DeadEnemyIdxs;
        public int DeadEnemyCount;
        public int[] EnemyMapIdxs;
        public int[] EnemyMapCount;

        public Vector2 PlayerDirection;
        public Vector2 PlayerDelta;
        public float PlayerVelocity;

        // car
        public int CarSlideIndex;
        public Vector2 CarSlideDirection;
        public Vector2 CarTargetDirection;
        public float CarRotationAngle;
        public bool InCar;
        public float CarVelocity;

        // car tread mark
        public int CurrentSkidMarkIndex;
        public int LastSkidMarkIndex;
        public Vector2[] SkidMarkPos;
        public float[] SkidMarkAngle;
        public Color[] SkidMarkColor;
        public Color[] CurrentSkidMarkColor;

        public Vector2[] AmmoPosition;
        public Vector2[] AmmoDirection;
        public int[] AmmoType;
        public float[] AmmoRotationT;
        public int AliveAmmoCount;
        public int DeadAmmoCount;
        public int[] AliveAmmoIdx;
        public int[] DeadAmmoIdx;
        public int[] AmmoTargetIdx;
        public Vector2[] AmmoTargetPos;

        // weapons
        public int[] PlayerWeaponType;
        public float[] PlayerWeaponFiringRateTimer;

        // experience
        public float XP;
        public float XPPickupRange;
        public Vector2[] XPPosition;
        public float[] XPValue;
        public int XPCount;
        public int[] XPUsedIdxs;
        public int XPUsedCount;
        public int[] XPUnusedIdxs;
        public int XPUnusedCount;
        // xp pickup
        public int[] XPPickupIdxs;
        public float[] XPPickupTimer;
        public int XPPickupCount;
        // xp map
        public int[] XPMapIdxs;
        public int[] XPMapCount;
        public int[] XPMapOutOfBoundsIdxs;
        public int XPMapOutOfBoundsCount;

        // map
        public int[] MapSpiralIndices;

        // enemy waves
        public int Level;
        public int Wave;
        public int[] WaveEnemyCount;
        public float[] SpawnTime;
        public float[] GroupTime;

        public int HeroType;
        public int CarType;

        public float GameTime;

        // stats
        public int StatsEnemiesKilled;

        // performance stast
        public int NumEnemyCollisionsChecks;
    }
}