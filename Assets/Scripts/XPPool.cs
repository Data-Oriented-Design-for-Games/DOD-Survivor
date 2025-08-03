using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor
{
    public class XPPool
    {
        AnimatedSpritePoolData m_poolData;

        int[] m_poolIndexToXPIndex;
        int[] m_xpIndexToPoolIndex;

        GameData gameData;
        Balance balance;
        Transform spriteParent;

        public void Init(GameData gameData, Balance balance, Transform spriteParent)
        {
            this.gameData = gameData;
            this.balance = balance;
            this.spriteParent = spriteParent;

            m_poolData = new AnimatedSpritePoolData();
            CommonPool.Init(m_poolData, balance.MaxXP);
            m_poolIndexToXPIndex = new int[balance.MaxXP];
            m_xpIndexToPoolIndex = new int[balance.MaxXP];
        }

        public void Clear()
        {
            CommonPool.Clear(m_poolData);
        }

        public void ShowXP(int enemyIndex, int spriteType, Vector2 position)
        {
            int poolIndex = getFreePoolIndex(spriteType);
            m_poolIndexToXPIndex[poolIndex] = enemyIndex;
            m_xpIndexToPoolIndex[enemyIndex] = poolIndex;

            // Debug.Log("ShowEnemy enemyIndex " + enemyIndex + " poolIndex " + poolIndex);

            CommonPool.ShowPoolItem(m_poolData, position, 0.0f, poolIndex);
        }

        public void HideXP(int enemyIndex)
        {
            int poolIndex = m_xpIndexToPoolIndex[enemyIndex];
            // Debug.Log("HideEnemy enemyIndex " + enemyIndex + " poolIndex " + poolIndex);
            CommonPool.HidePoolItem(m_poolData, poolIndex);
        }

        int getFreePoolIndex(int spriteType)
        {
            string spriteName = balance.XPName;
            int poolIndex = CommonPool.GetFreePoolIndex(m_poolData, balance, spriteName, "XP/", spriteParent, spriteType);

            m_poolData.m_spriteAnimationData[poolIndex].FrameIndex = Mathf.FloorToInt(Random.value * m_poolData.m_spriteAnimationData[poolIndex].NumFrames);

            return poolIndex;
        }

        public void Tick(float dt)
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
                int xpIndex = m_poolIndexToXPIndex[poolIndex];
                m_poolData.Pool[poolIndex].transform.localPosition = new Vector3(gameData.XPPosition[xpIndex].x, gameData.XPPosition[xpIndex].y, -5.0f);
            }
        }
    }
}