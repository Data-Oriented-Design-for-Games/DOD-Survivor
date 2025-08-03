using UnityEngine;

namespace Survivor
{
    [CreateAssetMenu(fileName = "CarSO", menuName = "DOD/CarSO", order = 1)]
    public class CarSO : ScriptableObject
    {
        [HideInInspector] public int ID;
        public Car Car;
        public int HP;
        public float Velocity;
        public float Acceleration;
    }
}