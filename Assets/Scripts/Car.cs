using UnityEngine;

namespace Survivor
{
    public class Car : MonoBehaviour
    {
        public CarFrameInfo[] CarFrameInfo;

        public void UpdateFrame(PlayerAnimationData playerAnimationData)
        {
            for (int i = 0; i < CarFrameInfo.Length; i++)
            {
                bool activeState = (i == playerAnimationData.DirectionIndex);
                bool changeActive = CarFrameInfo[i].gameObject.activeSelf != activeState;
                if (changeActive)
                    CarFrameInfo[i].gameObject.SetActive(activeState);
            }
        }
    }
}