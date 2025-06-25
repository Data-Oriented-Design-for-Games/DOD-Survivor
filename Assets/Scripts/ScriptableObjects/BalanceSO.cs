using UnityEngine;

namespace Survivor
{
    [CreateAssetMenu(fileName = "BalanceSO", menuName = "DOD/BalanceSO", order = 1)]
    public class BalanceSO : ScriptableObject
    {
        [Header("Enemies")]
        public int MaxEnemies;
        public float SpawnRadius;

        [Header("Player")]
        public float MinCollisionDistance;
        public int MaxPlayerWeapons;

        [Header("Weapons")]
        public int MaxWeapons;
        public int MaxParticles;
    }
}