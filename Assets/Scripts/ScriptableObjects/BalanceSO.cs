using UnityEngine;

namespace Survivor
{
    [CreateAssetMenu(fileName = "BalanceSO", menuName = "DOD/BalanceSO", order = 1)]
    public class BalanceSO : ScriptableObject
    {
        [Header("Enemies")]
        public int NumEnemies;
        public float EnemyVelocity;
        public float EnemyRadius;
        public float SpawnRadius;

        [Header("Player")]
        public float PlayerVelocity;
        public float MinCollisionDistance;

        [Header("Weapons")]
        public int NumAmmo;
        public float FiringRate;
        public float AmmoVelocity;
        public float AmmoRadius;
    }
}