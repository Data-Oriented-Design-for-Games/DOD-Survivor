using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor
{
    public class EnemyPool
    {
        PoolData m_poolData;

        public int[] PoolIndexToEnemyIndex;
        public int[] EnemyIndexToPoolIndex;

        Balance balance;
        Transform spriteParent;

        public void Init(Balance balance, Transform spriteParent)
        {
            this.balance = balance;
            this.spriteParent = spriteParent;

            m_poolData = new PoolData();
            CommonPool.Init(m_poolData, balance.MaxEnemies * 10);
            PoolIndexToEnemyIndex = new int[balance.MaxEnemies * 10];
            EnemyIndexToPoolIndex = new int[balance.MaxEnemies];
        }

        public void Clear()
        {
            CommonPool.Clear(m_poolData);
        }

        public void ShowEnemy(int enemyIndex, int spriteType, Vector2 position)
        {
            int poolIndex = getFreePoolIndex(spriteType);
            PoolIndexToEnemyIndex[poolIndex] = enemyIndex;
            EnemyIndexToPoolIndex[enemyIndex] = poolIndex;

            CommonPool.ShowPoolItem(m_poolData, position, poolIndex);
        }

        public void HideEnemy(int enemyIndex)
        {
            int poolIndex = EnemyIndexToPoolIndex[enemyIndex];
            CommonPool.HidePoolItem(m_poolData, poolIndex);
        }

        int getFreePoolIndex(int spriteType)
        {
            int poolIndex = CommonPool.TryGetUnusedPoolItem(m_poolData, balance, spriteType);

            if (poolIndex == -1)
            {
                poolIndex = CommonPool.GetNewPoolItemIndex(m_poolData, spriteType);

                string name = balance.EnemyBalance.SpriteName[spriteType];
                m_poolData.Pool[poolIndex] = AssetManager.Instance.GetEnemy(name, spriteParent);
            }

            CommonVisual.InitSpriteFrameData(ref m_poolData.m_spriteAnimationData[poolIndex], m_poolData.Pool[poolIndex]);
            m_poolData.m_spriteAnimationData[poolIndex].FrameIndex = Mathf.FloorToInt(Random.value * m_poolData.m_spriteAnimationData[poolIndex].NumFrames);

            return poolIndex;
        }

        public void Tick(GameData gameData, float dt)
        {
            for (int i = 0; i < m_poolData.LiveCount; i++)
            {
                int poolIndex = m_poolData.LiveIdxs[i];
                CommonVisual.AnimateSprite(dt, ref m_poolData.m_spriteAnimationData[poolIndex]);
            }

            CommonPool.Tick(m_poolData, dt);

            for (int i = 0; i < m_poolData.LiveCount; i++)
            {
                int poolIndex = m_poolData.LiveIdxs[i];
                int enemyIndex = PoolIndexToEnemyIndex[poolIndex];
                m_poolData.Pool[poolIndex].transform.localPosition = gameData.EnemyPosition[enemyIndex];
                float scaleX = m_poolData.Pool[poolIndex].transform.localPosition.x < 0.0f ? 1.0f : -1.0f;
                m_poolData.Pool[poolIndex].transform.localScale = new Vector3(scaleX, 1.0f, 1.0f);
            }
        }
    }
}