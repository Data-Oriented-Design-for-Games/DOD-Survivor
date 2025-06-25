using UnityEngine;
using System.IO;

namespace Survivor
{
    public class GameData
    {
        public bool InGame;

        public Vector2[] EnemyPosition;
        public Vector2[] EnemyDirection;
        public int[] EnemyType;
        public int[] AliveEnemyIdxs;
        public int AliveEnemyCount;
        public int[] DeadEnemyIdxs;
        public int DeadEnemyCount;

        public Vector2 PlayerDirection;
        public Vector2 LastPlayerDirection;

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

        public int[] PlayerWeaponType;
        public float[] PlayerWeaponFiringRateTimer;

        public int Level;
        public int Wave;
        public int[] WaveEnemyCount;
        public float[] SpawnTime;
        public float[] GroupTime;

        public int PlayerType;

        public float GameTime;
    }
}