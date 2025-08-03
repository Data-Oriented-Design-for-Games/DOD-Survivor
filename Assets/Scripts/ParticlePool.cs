using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor
{
    public class ParticlePool
    {
        AnimatedSpritePoolData m_poolData;

        GameData gameData;
        Balance balance;
        Transform spriteParent;

        public void Init(GameData gameData, Balance balance, Transform spriteParent)
        {
            this.gameData = gameData;
            this.balance = balance;
            this.spriteParent = spriteParent;

            m_poolData = new AnimatedSpritePoolData();
            CommonPool.Init(m_poolData, balance.MaxParticles);
        }

        public void Clear()
        {
            CommonPool.Clear(m_poolData);
        }

        public void ShowParticle(int weaponType, Vector2 position, float angle, string name)
        {
            int index = getFreePoolIndex(weaponType, name);

            CommonPool.ShowPoolItem(m_poolData, position, angle, index);
        }

        int getFreePoolIndex(int spriteType, string name)
        {
            int poolIndex = CommonPool.GetFreePoolIndex(m_poolData, balance, name, "Particles/", spriteParent, spriteType);

            return poolIndex;
        }

        public void Tick(float dt)
        {
            int count = 0;
            for (int i = 0; i < m_poolData.LiveCount; i++)
            {
                int index = m_poolData.LiveIdxs[i];
                CommonVisual.AnimateSprite(dt, ref m_poolData.m_spriteAnimationData[index]);
                if (m_poolData.m_spriteAnimationData[index].FrameChanged && m_poolData.m_spriteAnimationData[index].FrameIndex == 0)
                {
                    m_poolData.Pool[index].gameObject.SetActive(false);
                    m_poolData.Used[index] = false;
                }
                else
                    m_poolData.LiveIdxs[count++] = index;
            }
            m_poolData.LiveCount = count;

            for (int i = 0; i < m_poolData.LiveCount; i++)
            {
                int index = m_poolData.LiveIdxs[i];
                Vector2 position = m_poolData.Pool[index].transform.localPosition;
                m_poolData.Pool[index].transform.localPosition = position -= gameData.PlayerDelta;
            }
            CommonPool.Tick(m_poolData, dt);
        }
    }
}