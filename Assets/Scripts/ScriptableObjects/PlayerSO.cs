using UnityEngine;

namespace Survivor
{
    [CreateAssetMenu(fileName = "PlayerSO", menuName = "DOD/PlayerSO", order = 1)]
    public class PlayerSO : ScriptableObject
    {
        [HideInInspector] public int ID;
        public AnimatedSprite AnimatedSprite;
        public float HP;
        public float Velocity;
        public WeaponSO Weapon;
    }
}