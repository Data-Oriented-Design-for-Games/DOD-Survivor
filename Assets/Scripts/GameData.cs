using UnityEngine;
using System.IO;

namespace Survivor
{
    public class GameData
    {
        public bool InGame;

        public Vector2[] EnemyPosition;
        public float[] EnemyRotation;

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

        public Vector2 PlayerDirection;
        public Vector2 PlayerDelta;
        public Vector2 PlayerTargetDirection;

        // car
        public int CarSlideIndex;
        public Vector2 CarSlideDirection;
        public float CarRotationAngle;
        public bool InCar;
        public float CarVelocity;

        // car tread mark
        public int CurrentSkidMarkIndex;
        public int LastSkidMarkIndex;
        public Vector2[] SkidMarkPos;
        public Color[] SkidMarkColor;
        public Color CurrentSkidMarkColor;

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
        public Vector2[] XPPosition;
        public float[] XPValue;
        public int[] XPUsedIdxs;
        public int XPUsedCount;
        public int[] XPUnusedIdxs;
        public int XPUnusedCount;
        public int XPCount;

        // enemy waves
        public int Level;
        public int Wave;
        public int[] WaveEnemyCount;
        public float[] SpawnTime;
        public float[] GroupTime;

        public int HeroType;
        public int CarType;

        public float GameTime;
        public float XP;

        // stats
        public int StatsEnemiesKilled;
    }
}