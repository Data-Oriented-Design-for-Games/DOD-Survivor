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
        public string[] DyingName;
        public float[] DyingTime;
        public Color[] DyingColor;
        public int[] SpriteType;
        public float[] Velocity;
        public float[] Radius;
        public float[] HP;
        public float[] XP;
        public float[] ImpactSlowdown;
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
        public float[] ExplosionRadius;
        public int[] NumProjectiles;
        public int[] Damage;
        public AMMO_TARGET[] AmmoTarget;

        Dictionary<string, int> SpriteNameToIndex;
        string[] SpriteIndexToName;
    }

    public class CarBalance
    {
        public string CarName;
        public int CarHP;
        public float Velocity;
        public float Acceleration;
        public int NumCarFrames;
        public float AngleDelta;
        public Vector2[][] CollisionCircles;
        public float[][] CollisionRadius;
        public Vector2[][] Tires;        
    }

    public struct HeroBalanceData
    {
        public string heroName;
        public int HP;
        public float Velocity;
        public int WeaponID;
    }

    public class HeroBalance
    {
        public HeroBalanceData[] HeroBalanceData;
    }

    [Serializable]
    public class Balance
    {
        public int MaxEnemies;
        public int MaxEnemiesPerMapSquare;
        public float SpawnRadius;
        public float BoundsRadius;
        public float BoundsRadiusSqr;

        public int MaxPlayerWeapons;

        public int MaxSkidMarks;
        public string SkidMarkName;
        public Color SkidMarkColor;

        public int MaxAmmo;
        public int MaxParticles;

        public int MaxXP;
        public string XPName;

        public LevelBalance[] LevelBalance;
        public HeroBalance HeroBalance = new HeroBalance();
        public CarBalance[] CarBalance;
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
                MaxEnemiesPerMapSquare = br.ReadInt32();
                SpawnRadius = br.ReadSingle();
                BoundsRadius = br.ReadSingle();
                BoundsRadiusSqr = BoundsRadius * BoundsRadius;

                MaxPlayerWeapons = br.ReadInt32();

                MaxSkidMarks = br.ReadInt32();
                SkidMarkName = br.ReadString();
                SkidMarkColor = new Color(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

                MaxAmmo = br.ReadInt32();
                MaxParticles = br.ReadInt32();

                MaxXP = br.ReadInt32();
                XPName = br.ReadString();

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
                HeroBalance.HeroBalanceData = new HeroBalanceData[numPlayers];
                for (int playerIdx = 0; playerIdx < numPlayers; playerIdx++)
                {
                    HeroBalance.HeroBalanceData[playerIdx].heroName = br.ReadString();

                    HeroBalance.HeroBalanceData[playerIdx].HP = br.ReadInt32();
                    HeroBalance.HeroBalanceData[playerIdx].Velocity = br.ReadSingle();
                    HeroBalance.HeroBalanceData[playerIdx].WeaponID = br.ReadInt32();
                }

                int numCars = br.ReadInt32();
                CarBalance = new CarBalance[numCars];
                for (int carIdx = 0; carIdx < numCars; carIdx++)
                {
                    CarBalance[carIdx] = new CarBalance();
                    CarBalance[carIdx].CarName = br.ReadString();
                    CarBalance[carIdx].CarHP = br.ReadInt32();
                    CarBalance[carIdx].Velocity = br.ReadSingle();
                    CarBalance[carIdx].Acceleration = br.ReadSingle();

                    int numCarFrames = br.ReadInt32();
                    CarBalance[carIdx].NumCarFrames = numCarFrames;
                    CarBalance[carIdx].AngleDelta = br.ReadSingle();
                    CarBalance[carIdx].CollisionCircles = new Vector2[numCarFrames][];
                    CarBalance[carIdx].CollisionRadius = new float[numCarFrames][];
                    CarBalance[carIdx].Tires = new Vector2[numCarFrames][];
                    for (int frameIdx = 0; frameIdx < numCarFrames; frameIdx++)
                    {
                        int numCircleCollisions = br.ReadInt32();
                        CarBalance[carIdx].CollisionCircles[frameIdx] = new Vector2[numCircleCollisions];
                        CarBalance[carIdx].CollisionRadius[frameIdx] = new float[numCircleCollisions];
                        for (int circleIdx = 0; circleIdx < numCircleCollisions; circleIdx++)
                        {
                            CarBalance[carIdx].CollisionCircles[frameIdx][circleIdx] = new Vector2(br.ReadSingle(), br.ReadSingle());
                            CarBalance[carIdx].CollisionRadius[frameIdx][circleIdx] = br.ReadSingle();
                        }

                        CarBalance[carIdx].Tires[frameIdx] = new Vector2[4];
                        for (int tireIdx = 0; tireIdx < 4; tireIdx++)
                            CarBalance[carIdx].Tires[frameIdx][tireIdx] = new Vector2(br.ReadSingle(), br.ReadSingle());
                    }
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
                WeaponBalance.ExplosionRadius = new float[numWeapons];
                WeaponBalance.NumProjectiles = new int[numWeapons];
                WeaponBalance.AmmoTarget = new AMMO_TARGET[numWeapons];
                WeaponBalance.Damage = new int[numWeapons];
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
                    WeaponBalance.ExplosionRadius[weaponIdx] = br.ReadSingle();
                    WeaponBalance.NumProjectiles[weaponIdx] = br.ReadInt32();
                    WeaponBalance.AmmoTarget[weaponIdx] = (AMMO_TARGET)br.ReadByte();
                    WeaponBalance.Damage[weaponIdx] = br.ReadInt32();
                }

                int numEnemies = br.ReadInt32();
                EnemyBalance.EnemyType = new int[numEnemies];
                EnemyBalance.SpriteName = new string[numEnemies];
                EnemyBalance.SpriteType = new int[numEnemies];
                EnemyBalance.DyingName = new string[numEnemies];
                EnemyBalance.DyingTime = new float[numEnemies];
                EnemyBalance.DyingColor = new Color[numEnemies];
                EnemyBalance.Velocity = new float[numEnemies];
                EnemyBalance.Radius = new float[numEnemies];
                EnemyBalance.HP = new float[numEnemies];
                EnemyBalance.XP = new float[numEnemies];
                EnemyBalance.ImpactSlowdown = new float[numEnemies];
                for (int enemyIdx = 0; enemyIdx < numEnemies; enemyIdx++)
                {
                    EnemyBalance.EnemyType[enemyIdx] = br.ReadInt32();
                    EnemyBalance.SpriteName[enemyIdx] = br.ReadString();
                    EnemyBalance.SpriteType[enemyIdx] = br.ReadInt32();
                    EnemyBalance.DyingName[enemyIdx] = br.ReadString();
                    EnemyBalance.DyingTime[enemyIdx] = br.ReadSingle();
                    EnemyBalance.DyingColor[enemyIdx] = new Color(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    EnemyBalance.Velocity[enemyIdx] = br.ReadSingle();
                    EnemyBalance.Radius[enemyIdx] = br.ReadSingle();
                    EnemyBalance.HP[enemyIdx] = br.ReadSingle();
                    EnemyBalance.XP[enemyIdx] = br.ReadSingle();
                    EnemyBalance.ImpactSlowdown[enemyIdx] = br.ReadSingle();
                }

                int magic = br.ReadInt32();
                Debug.Log(magic);
            }
        }
    }
}