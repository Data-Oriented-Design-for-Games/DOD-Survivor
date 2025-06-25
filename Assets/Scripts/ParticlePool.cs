using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor
{
    public class ParticlePool
    {
        PoolData m_poolData;

        Balance balance;
        Transform spriteParent;

        public void Init(Balance balance, Transform spriteParent)
        {
            this.balance = balance;
            this.spriteParent = spriteParent;

            m_poolData = new PoolData();
            CommonPool.Init(m_poolData, balance.MaxParticles);
        }

        public void Clear()
        {
            CommonPool.Clear(m_poolData);
        }

        public void ShowParticle(int weaponType, Vector2 position)
        {
            int index = getFreePoolIndex(weaponType);

            CommonPool.ShowPoolItem(m_poolData, position, index);
        }

        int getFreePoolIndex(int weaponType)
        {
            int index = CommonPool.TryGetUnusedPoolItem(m_poolData, balance, weaponType);

            if (index == -1)
            {
                index = CommonPool.GetNewPoolItemIndex(m_poolData, weaponType);

                string name = balance.WeaponBalance.ExplosionName[weaponType];
                m_poolData.Pool[index] = AssetManager.Instance.GetParticle(name, spriteParent);
            }

            CommonVisual.InitSpriteFrameData(ref m_poolData.m_spriteAnimationData[index], m_poolData.Pool[index]);

            return index;
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

            CommonPool.Tick(m_poolData, dt);
        }
    }
}