using UnityEngine;

namespace Survivor
{
    public class WeaponPool
    {
        PoolData m_poolData;

        public int[] PoolIndexToWeaponIndex;
        public int[] WeaponIndexToPoolIndex;

        Balance balance;
        Transform spriteParent;

        public void Init(Balance balance, Transform spriteParent)
        {
            this.balance = balance;
            this.spriteParent = spriteParent;

            m_poolData = new PoolData();
            CommonPool.Init(m_poolData, balance.MaxAmmo * 10);
            PoolIndexToWeaponIndex = new int[balance.MaxAmmo * 10];
            WeaponIndexToPoolIndex = new int[balance.MaxAmmo];
        }

        public void Clear()
        {
            CommonPool.Clear(m_poolData);
        }

        public void ShowWeapon(int weaponIndex, int spriteType, Vector2 position)
        {
            int poolIndex = getFreePoolIndex(spriteType);
            PoolIndexToWeaponIndex[poolIndex] = weaponIndex;
            WeaponIndexToPoolIndex[weaponIndex] = poolIndex;

            CommonPool.ShowPoolItem(m_poolData, position, poolIndex);
        }

        public void HideWeapon(int weaponIndex)
        {
            int poolIndex = WeaponIndexToPoolIndex[weaponIndex];
            CommonPool.HidePoolItem(m_poolData, poolIndex);
        }

        int getFreePoolIndex(int spriteType)
        {
            int poolIndex = CommonPool.TryGetUnusedPoolItem(m_poolData, balance, spriteType);

            if (poolIndex == -1)
            {
                poolIndex = CommonPool.GetNewPoolItemIndex(m_poolData, spriteType);

                string name = balance.WeaponBalance.SpriteName[spriteType];
                m_poolData.Pool[poolIndex] = AssetManager.Instance.GetWeapon(name, spriteParent);
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
                int ammoIndex = PoolIndexToWeaponIndex[poolIndex];
                m_poolData.Pool[poolIndex].transform.localPosition = gameData.AmmoPosition[ammoIndex];
                float angle = Vector2.SignedAngle(Vector2.up, gameData.AmmoDirection[ammoIndex]);
                m_poolData.Pool[poolIndex].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, angle);
            }
        }
    }
}