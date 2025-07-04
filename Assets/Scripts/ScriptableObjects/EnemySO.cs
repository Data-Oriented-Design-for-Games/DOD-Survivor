using UnityEngine;

namespace Survivor
{
    [CreateAssetMenu(fileName = "EnemySO", menuName = "DOD/EnemySO", order = 1)]
    public class EnemySO : ScriptableObject
    {
        [HideInInspector] public int ID;
        public AnimatedSprite AnimatedSprite;
        public float Velocity;
        public float Radius;
        public float HP;
    }
}