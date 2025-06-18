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

        public Vector2 BoardBounds;

        public float GameTime;
    }
}