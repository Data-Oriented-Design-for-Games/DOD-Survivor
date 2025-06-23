using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor
{
    public class PoolData
    {
        public AnimatedSprite[] Pool;
        public bool[] Used;
        public int[] Type;
        public int Count;

        public int[] LiveIdxs;
        public int LiveCount;

        public SpriteAnimationData[] m_spriteAnimationData;
    }

    public static class CommonPool
    {
        public static void Init(PoolData poolData, int maxItems)
        {
            poolData.Pool = new AnimatedSprite[maxItems];
            poolData.Used = new bool[maxItems];
            poolData.Type = new int[maxItems];
            poolData.Count = 0;

            poolData.m_spriteAnimationData = new SpriteAnimationData[maxItems];
            poolData.LiveIdxs = new int[maxItems];
            poolData.LiveCount = 0;
        }

        public static void ShowPoolItem(PoolData poolData, Vector2 position, int index)
        {
            poolData.LiveIdxs[poolData.LiveCount++] = index;

            poolData.Pool[index].transform.localPosition = position;
            poolData.Pool[index].gameObject.SetActive(true);

        }

        public static int TryGetUnusedPoolItem(PoolData poolData, Balance balance, int type)
        {
            for (int i = 0; i < poolData.Count; i++)
                if (!poolData.Used[i] && poolData.Type[i] == type)
                {
                    poolData.Used[i] = true;
                    return i;
                }

            if (balance.MaxParticles == poolData.Count)
            {
                Debug.LogError("Particle pool out of space! Allocated " + balance.MaxParticles);
            }

            return -1;
        }

        public static int GetNewPoolItemIndex(PoolData poolData, int type)
        {
            int index = poolData.Count;
            // no free existing particle found, allocate a new one
            poolData.Used[index] = true;
            poolData.Type[index] = type;
            poolData.Count++;
            return index;
        }

        public static void Tick(PoolData poolData, float dt)
        {
            int count = 0;
            for (int i = 0; i < poolData.LiveCount; i++)
            {
                int index = poolData.LiveIdxs[i];
                CommonVisual.AnimateSprite(dt, ref poolData.m_spriteAnimationData[index]);
                if (poolData.m_spriteAnimationData[index].FrameChanged && poolData.m_spriteAnimationData[index].FrameIndex == 0)
                {
                    // remove frame
                    poolData.Pool[index].gameObject.SetActive(false);
                    poolData.Used[index] = false;
                }
                else
                    poolData.LiveIdxs[count++] = index;
            }
            poolData.LiveCount = count;

            for (int i = 0; i < poolData.LiveCount; i++)
            {
                int index = poolData.LiveIdxs[i];
                CommonVisual.TryChangeSpriteFrame(ref poolData.m_spriteAnimationData[index], poolData.Pool[index]);
            }
        }

    }
}