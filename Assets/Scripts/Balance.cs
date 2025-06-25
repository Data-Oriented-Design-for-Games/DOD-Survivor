using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;

namespace Survivor
{
    public class LevelBalance
    {
        public int TotalWaves;
        public int[] EnemyType;
        public int[] NumEnemies;
        public float[] StartTime;
        public float[] EndTime;
        public float[] SpawnDelay;
        public float[] GroupDelay;
    }

    public class EnemyBalance
    {
        public int[] EnemyType;
        public string[] SpriteName;
        public int[] SpriteType;
        public float[] Velocity;
        public float[] Radius;
        public float[] HP;
    }

    public class WeaponBalance
    {
        public int[] WeaponType;
        public string[] SpriteName;
        public string[] ExplosionName;
        public int[] SpriteType;
        public string[] TrailName;
        public float[] FiringRate;
        public float[] Velocity;
        public float[] AngularVelocity;
        public float[] TriggerRadius;
        public AMMO_TARGET[] AmmoTarget;
        public float[] ExplosionRadius;
        public int[] Damage;
        public float[] DontRemoveOnHit;

        Dictionary<string, int> SpriteNameToIndex;
        string[] SpriteIndexToName;
    }

    public struct PlayerBalanceData
    {
        public string SpriteName;
        public int HP;
        public float Velocity;
        public int WeaponID;
    }

    public class PlayerBalance
    {
        public PlayerBalanceData[] PlayerBalanceData;
    }

    [Serializable]
    public class Balance
    {
        [Header("Enemies")]
        public int MaxEnemies;
        public float SpawnRadius;

        [Header("Player")]
        public float MinCollisionDistance;
        public int MaxPlayerWeapons;

        [Header("Weapons")]
        public int MaxAmmo;
        public int MaxParticles;

        public LevelBalance[] LevelBalance;
        public PlayerBalance PlayerBalance = new PlayerBalance();
        public WeaponBalance WeaponBalance = new WeaponBalance();
        public EnemyBalance EnemyBalance = new EnemyBalance();

        public void LoadBalance()
        {
            TextAsset asset = Resources.Load("balance") as TextAsset;
            LoadBalance(asset.bytes);
        }

        public void LoadBalance(byte[] array)
        {
            Stream s = new MemoryStream(array);
            using (BinaryReader br = new BinaryReader(s))
            {
                int version = br.ReadInt32();

                MaxEnemies = br.ReadInt32();
                SpawnRadius = br.ReadSingle();

                MinCollisionDistance = br.ReadSingle();
                MaxPlayerWeapons = br.ReadInt32();

                MaxAmmo = br.ReadInt32();
                MaxParticles = br.ReadInt32();

                int numLevels = br.ReadInt32();
                LevelBalance = new LevelBalance[numLevels];
                for (int levelIdx = 0; levelIdx < numLevels; levelIdx++)
                {
                    LevelBalance[levelIdx] = new LevelBalance();
                    int numWaves = br.ReadInt32();
                    LevelBalance[levelIdx].TotalWaves = numWaves;
                    LevelBalance[levelIdx].EnemyType = new int[numWaves];
                    LevelBalance[levelIdx].NumEnemies = new int[numWaves];
                    LevelBalance[levelIdx].StartTime = new float[numWaves];
                    LevelBalance[levelIdx].EndTime = new float[numWaves];
                    LevelBalance[levelIdx].SpawnDelay = new float[numWaves];
                    LevelBalance[levelIdx].GroupDelay = new float[numWaves];
                    for (int enemyIdx = 0; enemyIdx < numWaves; enemyIdx++)
                    {
                        LevelBalance[levelIdx].EnemyType[enemyIdx] = br.ReadInt32();
                        LevelBalance[levelIdx].NumEnemies[enemyIdx] = br.ReadInt32();
                        LevelBalance[levelIdx].StartTime[enemyIdx] = br.ReadSingle();
                        LevelBalance[levelIdx].EndTime[enemyIdx] = br.ReadSingle();
                        LevelBalance[levelIdx].SpawnDelay[enemyIdx] = br.ReadSingle();
                        LevelBalance[levelIdx].GroupDelay[enemyIdx] = br.ReadSingle();
                    }
                }

                int numPlayers = br.ReadInt32();
                PlayerBalance.PlayerBalanceData = new PlayerBalanceData[numPlayers];
                for (int playerIdx = 0; playerIdx < numPlayers; playerIdx++)
                {
                    PlayerBalance.PlayerBalanceData[playerIdx].SpriteName = br.ReadString();
                    PlayerBalance.PlayerBalanceData[playerIdx].HP = br.ReadInt32();
                    PlayerBalance.PlayerBalanceData[playerIdx].Velocity = br.ReadSingle();
                    PlayerBalance.PlayerBalanceData[playerIdx].WeaponID = br.ReadInt32();
                }

                int numWeapons = br.ReadInt32();
                WeaponBalance.WeaponType = new int[numWeapons];
                WeaponBalance.SpriteName = new string[numWeapons];
                WeaponBalance.SpriteType = new int[numWeapons];
                WeaponBalance.ExplosionName = new string[numWeapons];
                WeaponBalance.TrailName = new string[numWeapons];
                WeaponBalance.FiringRate = new float[numWeapons];
                WeaponBalance.Velocity = new float[numWeapons];
                WeaponBalance.AngularVelocity = new float[numWeapons];
                WeaponBalance.TriggerRadius = new float[numWeapons];
                WeaponBalance.AmmoTarget = new AMMO_TARGET[numWeapons];
                WeaponBalance.ExplosionRadius = new float[numWeapons];
                WeaponBalance.Damage = new int[numWeapons];
                WeaponBalance.DontRemoveOnHit = new float[numWeapons];
                for (int weaponIdx = 0; weaponIdx < numWeapons; weaponIdx++)
                {
                    WeaponBalance.WeaponType[weaponIdx] = br.ReadInt32();
                    WeaponBalance.SpriteName[weaponIdx] = br.ReadString();
                    WeaponBalance.SpriteType[weaponIdx] = br.ReadInt32();
                    WeaponBalance.ExplosionName[weaponIdx] = br.ReadString();
                    WeaponBalance.TrailName[weaponIdx] = br.ReadString();
                    WeaponBalance.FiringRate[weaponIdx] = br.ReadSingle();
                    WeaponBalance.Velocity[weaponIdx] = br.ReadSingle();
                    WeaponBalance.AngularVelocity[weaponIdx] = br.ReadSingle();
                    WeaponBalance.TriggerRadius[weaponIdx] = br.ReadSingle();
                    WeaponBalance.AmmoTarget[weaponIdx] = (AMMO_TARGET)br.ReadByte();
                    WeaponBalance.ExplosionRadius[weaponIdx] = br.ReadSingle();
                    WeaponBalance.Damage[weaponIdx] = br.ReadInt32();
                    WeaponBalance.DontRemoveOnHit[weaponIdx] = br.ReadSingle();
                }

                int numEnemies = br.ReadInt32();
                EnemyBalance.EnemyType = new int[numEnemies];
                EnemyBalance.SpriteName = new string[numEnemies];
                EnemyBalance.SpriteType = new int[numEnemies];
                EnemyBalance.Velocity = new float[numEnemies];
                EnemyBalance.Radius = new float[numEnemies];
                EnemyBalance.HP = new float[numEnemies];
                for (int enemyIdx = 0; enemyIdx < numEnemies; enemyIdx++)
                {
                    EnemyBalance.EnemyType[enemyIdx] = br.ReadInt32();
                    EnemyBalance.SpriteName[enemyIdx] = br.ReadString();
                    EnemyBalance.SpriteType[enemyIdx] = br.ReadInt32();
                    EnemyBalance.Velocity[enemyIdx] = br.ReadSingle();
                    EnemyBalance.Radius[enemyIdx] = br.ReadSingle();
                    EnemyBalance.HP[enemyIdx] = br.ReadSingle();
                }

                int magic = br.ReadInt32();
                Debug.Log(magic);
            }
        }
    }
}