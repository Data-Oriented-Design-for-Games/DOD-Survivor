using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor
{
    public class DyingEnemyPool
    {
        AnimatedSpritePoolData m_poolData;

        public int[] PoolIndexToEnemyIndex;
        public int[] EnemyIndexToPoolIndex;

        Balance balance;
        GameData gameData;
        Transform spriteParent;

        public void Init(GameData gameData, Balance balance, Transform spriteParent)
        {
            this.balance = balance;
            this.gameData = gameData;
            this.spriteParent = spriteParent;

            m_poolData = new AnimatedSpritePoolData();
            CommonPool.Init(m_poolData, balance.MaxEnemies * 10);
            PoolIndexToEnemyIndex = new int[balance.MaxEnemies * 10];
            EnemyIndexToPoolIndex = new int[balance.MaxEnemies];
        }

        public void Clear()
        {
            CommonPool.Clear(m_poolData);
        }

        public void ShowDyingEnemy(int enemyIndex, int spriteType, Vector2 position)
        {
            int poolIndex = getFreePoolIndex(spriteType);
            PoolIndexToEnemyIndex[poolIndex] = enemyIndex;
            EnemyIndexToPoolIndex[enemyIndex] = poolIndex;

            Debug.Log("ShowDyingEnemy enemyIndex " + enemyIndex + " poolIndex " + poolIndex);

            CommonPool.ShowPoolItem(m_poolData, position, 0.0f, poolIndex);
        }

        public void HideDyingEnemy(int enemyIndex)
        {

            int poolIndex = EnemyIndexToPoolIndex[enemyIndex];

            Debug.Log("HideDyingEnemy enemyIndex " + enemyIndex + " poolIndex " + poolIndex);

            CommonPool.HidePoolItem(m_poolData, poolIndex);
        }

        int getFreePoolIndex(int spriteType)
        {
            string spriteName = balance.EnemyBalance.DyingName[spriteType];
            int poolIndex = CommonPool.GetFreePoolIndex(m_poolData, balance, spriteName, "Enemies/", spriteParent, spriteType);

            return poolIndex;
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < m_poolData.LiveCount; i++)
            {
                int poolIndex = m_poolData.LiveIdxs[i];
                if (m_poolData.m_spriteAnimationData[poolIndex].FrameIndex < m_poolData.m_spriteAnimationData[poolIndex].NumFrames - 1)
                    CommonVisual.AnimateSprite(dt, ref m_poolData.m_spriteAnimationData[poolIndex]);
            }

            CommonPool.Tick(m_poolData, dt);

            for (int i = 0; i < m_poolData.LiveCount; i++)
            {
                int poolIndex = m_poolData.LiveIdxs[i];
                int enemyIndex = PoolIndexToEnemyIndex[poolIndex];
                m_poolData.Pool[poolIndex].transform.localPosition = new Vector3(gameData.EnemyPosition[enemyIndex].x, gameData.EnemyPosition[enemyIndex].y, -11.0f);
            }
        }
    }
}