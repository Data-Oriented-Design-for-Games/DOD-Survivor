using UnityEngine;

namespace Survivor
{
    public class WeaponPool
    {
        AnimatedSpritePoolData m_poolData;

        public int[] PoolIndexToWeaponIndex;
        public int[] WeaponIndexToPoolIndex;

        Balance balance;
        Transform spriteParent;

        public void Init(Balance balance, Transform spriteParent)
        {
            this.balance = balance;
            this.spriteParent = spriteParent;

            m_poolData = new AnimatedSpritePoolData();
            CommonPool.Init(m_poolData, balance.MaxAmmo * 10);
            PoolIndexToWeaponIndex = new int[balance.MaxAmmo * 10];
            WeaponIndexToPoolIndex = new int[balance.MaxAmmo];
        }

        public void Clear()
        {
            CommonPool.Clear(m_poolData);
        }

        public void ShowAmmo(int weaponIndex, int spriteType, Vector2 position)
        {
            int poolIndex = getFreePoolIndex(spriteType);
            PoolIndexToWeaponIndex[poolIndex] = weaponIndex;
            WeaponIndexToPoolIndex[weaponIndex] = poolIndex;

            CommonPool.ShowPoolItem(m_poolData, position, 0.0f, poolIndex);
        }

        public void HideWeapon(int weaponIndex)
        {
            int poolIndex = WeaponIndexToPoolIndex[weaponIndex];
            CommonPool.HidePoolItem(m_poolData, poolIndex);
        }

        int getFreePoolIndex(int spriteType)
        {
            string spriteName = balance.WeaponBalance.SpriteName[spriteType];
            int poolIndex = CommonPool.GetFreePoolIndex(m_poolData, balance, spriteName, "Weapons/", spriteParent, spriteType);

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