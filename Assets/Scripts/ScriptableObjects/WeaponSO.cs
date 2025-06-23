using UnityEngine;

namespace Survivor
{
    public enum AMMO_TARGET { DIRECTION, HOMING, POSITION, ORBIT };

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
        public bool RemoveOnHit;
        public float ExplosionRadius;
        public int Damage;

        public AMMO_TARGET WeaponTarget;
    }
}