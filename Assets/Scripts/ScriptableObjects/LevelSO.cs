using System;
using UnityEngine;

namespace Survivor
{
    [Serializable]
    public struct WaveInfo
    {
        public EnemySO EnemySO;
        public int NumEnemies;
        public float StartTime;
        public float EndTime;
        public float SpawnDelay;
        public float GroupDelay;
    }

    [CreateAssetMenu(fileName = "LevelSO", menuName = "DOD/LevelSO", order = 1)]
    public class LevelSO : ScriptableObject
    {
        public WaveInfo[] WaveInfo;
    }
}