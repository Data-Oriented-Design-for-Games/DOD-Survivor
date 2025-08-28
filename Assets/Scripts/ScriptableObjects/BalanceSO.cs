using UnityEngine;

namespace Survivor
{
    [CreateAssetMenu(fileName = "BalanceSO", menuName = "DOD/BalanceSO", order = 1)]
    public class BalanceSO : ScriptableObject
    {
        public float MaxEnemiesPerMapSquare;
        public int MapSize;
        public int MaxXPPerMapSquare;

        [Header("Enemies")]
        public int MaxEnemies;
        public float SpawnRadius;
        public float BoundsRadius;

        [Header("Player")]
        public int MaxPlayerWeapons;
        public float StartingPickupRange;

        [Header("Car")]
        public int MaxSkidMarks;
        public GameObject SkidMark;
        public Color SkidMarkColor;

        [Header("Weapons")]
        public int MaxAmmo;
        public int MaxParticles;

        [Header("XP")]
        public int MaxXP;
        public AnimatedSprite XPAnimatedSprite;
    }
}