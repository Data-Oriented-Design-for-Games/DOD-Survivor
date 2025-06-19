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

        public Vector2[] AmmoPosition;
        public Vector2[] AmmoDirection;
        public int AliveAmmoCount;
        public int DeadAmmoCount;
        public int[] AliveAmmoIdx;
        public int[] DeadAmmoIdx;
        public float FiringRateTimer;

        public float GameTime;
    }
}