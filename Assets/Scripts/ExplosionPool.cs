using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor
{
    public class ExplosionPool
    {
        AnimatedSprite[] m_pool;
        bool[] m_used;
        int[] m_type;
        int m_count;

        int[] m_liveIdxs;
        int m_liveCount;

        SpriteAnimationData[] m_spriteAnimationData;

        Balance balance;
        Transform spriteParent;

        public void Init(Balance balance, Transform spriteParent)
        {
            this.balance = balance;
            this.spriteParent = spriteParent;
            m_pool = new AnimatedSprite[balance.MaxParticles];
            m_used = new bool[balance.MaxParticles];
            m_type = new int[balance.MaxParticles];
            m_count = 0;

            m_spriteAnimationData = new SpriteAnimationData[balance.MaxParticles];
            m_liveIdxs = new int[balance.MaxParticles];
            m_liveCount = 0;
        }

        public void ShowExplosion(int weaponType, Vector2 position)
        {
            int index = getFreePoolIndex(weaponType);

            m_liveIdxs[m_liveCount++] = index;

            m_pool[index].transform.localPosition = position;
            m_pool[index].gameObject.SetActive(true);
        }

        int getFreePoolIndex(int weaponType)
        {
            int index = -1;
            for (int i = 0; i < m_count; i++)
                if (!m_used[i] && m_type[i] == weaponType)
                {
                    index = i;
                    m_used[i] = true;
                }

            if (index == -1)
            {
                if (this.balance.MaxParticles == m_count)
                {
                    Debug.LogError("Particle pool out of space! Allocated " + balance.MaxParticles);
                }

                index = m_count;
                string name = balance.WeaponBalance.ExplosionName[weaponType];
                // no free existing particle found, allocate a new one
                m_pool[index] = AssetManager.Instance.GetParticle(name, spriteParent);
                m_used[index] = true;
                m_type[index] = weaponType;


                m_count++;
            }

            CommonVisual.InitSpriteFrameData(ref m_spriteAnimationData[index], m_pool[index]);

            return index;
        }

        public void Tick(float dt)
        {
            int count = 0;
            for (int i = 0; i < m_liveCount; i++)
            {
                int index = m_liveIdxs[i];
                CommonVisual.AnimateSprite(dt, ref m_spriteAnimationData[index]);
                if (m_spriteAnimationData[index].FrameChanged && m_spriteAnimationData[index].FrameIndex == 0)
                {
                    // remove frame
                    m_pool[index].gameObject.SetActive(false);
                    m_used[index] = false;
                }
                else
                    m_liveIdxs[count++] = index;
            }
            m_liveCount = count;

            for (int i = 0; i < m_liveCount; i++)
            {
                int index = m_liveIdxs[i];
                CommonVisual.TryChangeSpriteFrame(ref m_spriteAnimationData[index], m_pool[index]);
            }
        }
    }
}