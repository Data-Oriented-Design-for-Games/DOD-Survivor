using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

        public int MaxItems;

        public SpriteAnimationData[] m_spriteAnimationData;
    }

    public static class CommonPool
    {
        public static void Init(PoolData poolData, int maxItems)
        {
            poolData.MaxItems = maxItems;
            poolData.Pool = new AnimatedSprite[maxItems];
            poolData.Used = new bool[maxItems];
            poolData.Type = new int[maxItems];
            for (int i = 0; i < maxItems; i++)
                poolData.Type[i] = -1;
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

        public static void Clear(PoolData poolData)
        {
            for (int i = 0; i < poolData.LiveCount; i++)
            {
                int index = poolData.LiveIdxs[i];
                poolData.Pool[index].gameObject.SetActive(false);
            }

            for (int i = 0; i < poolData.Count; i++)
            {
                poolData.Used[i] = false;
                GameObject.Destroy(poolData.Pool[i]);
                poolData.Type[i] = -1;
            }
            poolData.Count = 0;
            poolData.LiveCount = 0;
        }

        public static void HidePoolItem(PoolData poolData, int poolIndex)
        {
            poolData.Used[poolIndex] = false;
            poolData.Pool[poolIndex].gameObject.SetActive(false);
        }

        public static int TryGetUnusedPoolItem(PoolData poolData, Balance balance, int type)
        {
            for (int i = 0; i < poolData.Count; i++)
                if (!poolData.Used[i] && poolData.Type[i] == type)
                {
                    poolData.Used[i] = true;
                    return i;
                }

            return -1;
        }

        public static int GetNewPoolItemIndex(PoolData poolData, int type)
        {
            if (poolData.Count == poolData.MaxItems)
                Debug.LogError("Pool out of space! Allocated " + poolData.MaxItems + " items!");

            int index = poolData.Count;
            // no free existing particle found, allocate a new one
            poolData.Used[index] = true;
            poolData.Type[index] = type;
            poolData.Count++;
            return index;
        }

        public static void Tick(PoolData poolData, float dt)
        {
            for (int i = 0; i < poolData.LiveCount; i++)
            {
                int index = poolData.LiveIdxs[i];
                CommonVisual.TryChangeSpriteFrame(ref poolData.m_spriteAnimationData[index], poolData.Pool[index]);
            }
        }
    }
}