using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor
{
    public class EnemyPool
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

        public void ShowEnemy(int weaponType, Vector2 position)
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
                m_poolData.Pool[index] = AssetManager.Instance.GetEnemy(name, spriteParent);
            }

            CommonVisual.InitSpriteFrameData(ref m_poolData.m_spriteAnimationData[index], m_poolData.Pool[index]);

            return index;
        }

        public void Tick(float dt)
        {
            CommonPool.Tick(m_poolData, dt);
        }
    }
}