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

        public Vector2[] WeaponPosition;
        public Vector2[] WeaponDirection;
        public int[] WeaponType;
        public float[] WeaponAngle;
        public int AliveWeaponCount;
        public int DeadWeaponCount;
        public int[] AliveWeaponIdx;
        public int[] DeadWeaponIdx;
        public int[] WeaponTargetIdx;
        public Vector2[] WeaponTargetPos;

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