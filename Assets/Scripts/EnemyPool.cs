using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor
{
    public class EnemyPool
    {
        AnimatedSpritePoolData m_poolData;

        int[] m_poolIndexToEnemyIndex;
        int[] m_enemyIndexToPoolIndex;

        Balance balance;
        Transform spriteParent;

        public void Init(Balance balance, Transform spriteParent)
        {
            this.balance = balance;
            this.spriteParent = spriteParent;

            m_poolData = new AnimatedSpritePoolData();
            CommonPool.Init(m_poolData, balance.MaxEnemies * 10);
            m_poolIndexToEnemyIndex = new int[balance.MaxEnemies * 10];
            m_enemyIndexToPoolIndex = new int[balance.MaxEnemies];
        }

        public void Clear()
        {
            CommonPool.Clear(m_poolData);
        }

        public void ShowEnemy(int enemyIndex, int spriteType, Vector2 position)
        {
            int poolIndex = getFreePoolIndex(spriteType);
            m_poolIndexToEnemyIndex[poolIndex] = enemyIndex;
            m_enemyIndexToPoolIndex[enemyIndex] = poolIndex;

            // Debug.Log("ShowEnemy enemyIndex " + enemyIndex + " poolIndex " + poolIndex);

            CommonPool.ShowPoolItem(m_poolData, position, 0.0f, poolIndex);
        }

        public void HideEnemy(int enemyIndex)
        {
            int poolIndex = m_enemyIndexToPoolIndex[enemyIndex];
            // Debug.Log("HideEnemy enemyIndex " + enemyIndex + " poolIndex " + poolIndex);
            CommonPool.HidePoolItem(m_poolData, poolIndex);
        }

        int getFreePoolIndex(int spriteType)
        {
            string spriteName = balance.EnemyBalance.SpriteName[spriteType];
            int poolIndex = CommonPool.GetFreePoolIndex(m_poolData, balance, spriteName, "Enemies/", spriteParent, spriteType);

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
                int enemyIndex = m_poolIndexToEnemyIndex[poolIndex];
                m_poolData.Pool[poolIndex].transform.localPosition = new Vector3(gameData.EnemyPosition[enemyIndex].x, gameData.EnemyPosition[enemyIndex].y, -5.0f);

                // float enemyAngle = Vector2.SignedAngle(Vector2.up, -gameData.EnemyPosition[enemyIndex].normalized);
                m_poolData.Pool[poolIndex].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, gameData.EnemyRotation[enemyIndex]);
            }
        }
    }
}