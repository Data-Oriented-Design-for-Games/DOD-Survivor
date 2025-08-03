using UnityEngine;

namespace Survivor
{
    public enum AMMO_TARGET { ENEMY, DIRECTION, ORBIT };

    [CreateAssetMenu(fileName = "WeaponSO", menuName = "DOD/WeaponSO", order = 1)]
    public class WeaponSO : ScriptableObject
    {
        [HideInInspector] public int ID;
        public AnimatedSprite AnimatedSprite;
        public AnimatedSprite ExplosionSprite;
        public AnimatedSprite TrailSprite;
        public float FiringRate;
        public float Velocity;
        public float AngularVelocity;
        public float TriggerRadius;
        public float ExplosionRadius;
        public int NumProjectiles;
        public AMMO_TARGET WeaponTarget;

        // not yet implemented
        public int Damage;

    }
}