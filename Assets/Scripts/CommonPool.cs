using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Survivor
{
    public class AnimatedSpritePoolData
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
        public static void Init(AnimatedSpritePoolData poolData, int maxItems)
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

        public static void ShowPoolItem(AnimatedSpritePoolData poolData, Vector2 position, float angle, int poolIndex)
        {
            poolData.LiveIdxs[poolData.LiveCount++] = poolIndex;

            poolData.Pool[poolIndex].transform.localPosition = position;
            poolData.Pool[poolIndex].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, angle);
            poolData.Pool[poolIndex].gameObject.SetActive(true);

            // Debug.Log("ShowPoolItem " + poolData.Pool[poolIndex].name + " poolIndex " + poolIndex);
        }

        public static void Clear(AnimatedSpritePoolData poolData)
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

        public static int GetFreePoolIndex(AnimatedSpritePoolData m_poolData, Balance balance, string spriteName, string spritePath, Transform spriteParent, int spriteType)
        {
            int poolIndex = TryGetUnusedPoolItem(m_poolData, balance, spriteType);

            if (poolIndex == -1)
            {
                poolIndex = GetNewPoolItemIndex(m_poolData, spriteType);
                m_poolData.Pool[poolIndex] = AssetManager.Instance.GetAnimatedSprite(spriteName, spritePath, spriteParent);
                m_poolData.Pool[poolIndex].name = spriteName + " " + poolIndex.ToString();
            }

            CommonVisual.InitSpriteFrameData(ref m_poolData.m_spriteAnimationData[poolIndex], m_poolData.Pool[poolIndex]);

            return poolIndex;
        }

        public static void HidePoolItem(AnimatedSpritePoolData poolData, int poolIndex)
        {
            poolData.Used[poolIndex] = false;
            poolData.Pool[poolIndex].gameObject.SetActive(false);
            // Debug.Log("HidePoolItem " + poolData.Pool[poolIndex].name + " poolIndex " + poolIndex);

            int count = 0;
            for (int i = 0; i < poolData.LiveCount; i++)
                if (poolData.LiveIdxs[i] != poolIndex)
                    poolData.LiveIdxs[count++] = poolData.LiveIdxs[i];
            poolData.LiveCount = count;
        }

        public static int TryGetUnusedPoolItem(AnimatedSpritePoolData poolData, Balance balance, int type)
        {
            for (int i = 0; i < poolData.Count; i++)
                if (!poolData.Used[i] && poolData.Type[i] == type)
                {
                    poolData.Used[i] = true;
                    return i;
                }

            return -1;
        }

        public static int GetNewPoolItemIndex(AnimatedSpritePoolData poolData, int type)
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

        public static void Tick(AnimatedSpritePoolData poolData, float dt)
        {
            for (int i = 0; i < poolData.LiveCount; i++)
            {
                int index = poolData.LiveIdxs[i];
                CommonVisual.TryChangeSpriteFrame(ref poolData.m_spriteAnimationData[index], poolData.Pool[index]);
            }
        }
    }
}