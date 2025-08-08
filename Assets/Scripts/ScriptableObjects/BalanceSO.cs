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
        public int MaxPlayerWeapons;

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