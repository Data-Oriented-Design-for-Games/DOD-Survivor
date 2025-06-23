using UnityEngine;
using System.IO;

namespace Survivor
{
    public class GameData
    {
        public bool InGame;

        public Vector2[] EnemyPosition;
        public Vector2[] EnemyDirection;

        public Vector2 PlayerDirection;
        public Vector2 LastPlayerDirection;

        public Vector2[] WeaponPosition;
        public Vector2[] WeaponDirection;
        public float[] WeaponAngle;
        public int AliveWeaponCount;
        public int DeadWeaponCount;
        public int[] AliveWeaponIdx;
        public int[] DeadWeaponIdx;
        public float[] FiringRateTimer;
        public int[] WeaponTargetIdx;
        public Vector2[] WeaponTargetPos;


        public int PlayerType;
        public int EnemyType;
        public int WeaponType;

        public float GameTime;
    }
}