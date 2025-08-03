using System.IO;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace Survivor
{
    public static class GameDataIO
    {
        public static void Save(GameData gameData, Balance balance)
        {
            Debug.LogFormat("SaveGame()");

            string fileName = Application.persistentDataPath + "/gamedata.dat";
            using (FileStream fs = File.Create(fileName))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                int version = 1;
                bw.Write(version);

                bw.Write(gameData.InGame);

                bw.Write(balance.MaxEnemies);
                for (int i = 0; i < balance.MaxEnemies; i++)
                {
                    bw.Write(gameData.EnemyPosition[i].x);
                    bw.Write(gameData.EnemyPosition[i].y);
                }
                for (int i = 0; i < balance.MaxEnemies; i++)
                {
                    bw.Write(gameData.EnemyDirection[i].x);
                    bw.Write(gameData.EnemyDirection[i].y);
                }

                bw.Write(gameData.PlayerDirection.x);
                bw.Write(gameData.PlayerDirection.y);
                bw.Write(gameData.PlayerTargetDirection.x);
                bw.Write(gameData.PlayerTargetDirection.y);
                bw.Write(gameData.InCar);

                bw.Write(balance.MaxAmmo);
                for (int i = 0; i < balance.MaxAmmo; i++)
                {
                    bw.Write(gameData.AmmoPosition[i].x);
                    bw.Write(gameData.AmmoPosition[i].y);
                }
                for (int i = 0; i < balance.MaxAmmo; i++)
                {
                    bw.Write(gameData.AmmoDirection[i].x);
                    bw.Write(gameData.AmmoDirection[i].y);
                }
                for (int i = 0; i < balance.MaxAmmo; i++)
                    bw.Write(gameData.AmmoRotationT[i]);

                bw.Write(gameData.AliveAmmoCount);
                bw.Write(gameData.DeadAmmoCount);
                for (int i = 0; i < balance.MaxAmmo; i++)
                    bw.Write(gameData.AliveAmmoIdx[i]);
                for (int i = 0; i < balance.MaxAmmo; i++)
                    bw.Write(gameData.DeadAmmoIdx[i]);
                for (int i = 0; i < balance.MaxAmmo; i++)
                bw.Write(gameData.PlayerWeaponFiringRateTimer[i]);

                bw.Write(gameData.GameTime);
                bw.Write(gameData.XP);
            }
        }

        public static void Load(GameData gameData)
        {
            string fileName = Application.persistentDataPath + "/gamedata.dat";
            if (File.Exists(fileName))
            {
                using (FileStream stream = File.Open(fileName, FileMode.Open))
                using (BinaryReader br = new BinaryReader(stream))
                {
                    int version = br.ReadInt32();

                    gameData.InGame = br.ReadBoolean();

                    int numEnemies = br.ReadInt32();
                    for (int i = 0; i < numEnemies; i++)
                    {
                        gameData.EnemyPosition[i].x = br.ReadSingle();
                        gameData.EnemyPosition[i].y = br.ReadSingle();
                    }
                    for (int i = 0; i < numEnemies; i++)
                    {
                        gameData.EnemyDirection[i].x = br.ReadSingle();
                        gameData.EnemyDirection[i].y = br.ReadSingle();
                    }

                    gameData.PlayerDirection.x = br.ReadSingle();
                    gameData.PlayerDirection.y = br.ReadSingle();
                    gameData.PlayerTargetDirection.x = br.ReadSingle();
                    gameData.PlayerTargetDirection.y = br.ReadSingle();
                    gameData.InCar = br.ReadBoolean();

                    int numWeapons = br.ReadInt32();
                    for (int i = 0; i < numWeapons; i++)
                    {
                        gameData.AmmoPosition[i].x = br.ReadSingle();
                        gameData.AmmoPosition[i].y = br.ReadSingle();
                    }
                    for (int i = 0; i < numWeapons; i++)
                    {
                        gameData.AmmoDirection[i].x = br.ReadSingle();
                        gameData.AmmoDirection[i].y = br.ReadSingle();
                    }
                    for (int i = 0; i < numWeapons; i++)
                        gameData.AmmoRotationT[i] = br.ReadSingle();

                    gameData.AliveAmmoCount = br.ReadInt32();
                    gameData.DeadAmmoCount = br.ReadInt32();
                    for (int i = 0; i < numWeapons; i++)
                        gameData.AliveAmmoIdx[i] = br.ReadInt32();
                    for (int i = 0; i < numWeapons; i++)
                        gameData.DeadAmmoIdx[i] = br.ReadInt32();
                    for (int i = 0; i < numWeapons; i++)
                    gameData.PlayerWeaponFiringRateTimer[i] = br.ReadInt32();

                    gameData.GameTime = br.ReadSingle();
                    gameData.XP = br.ReadSingle();
                }
            }
        }

        public static bool SaveGameExists()
        {
            bool inGame = false;
            string fileName = Application.persistentDataPath + "/gamedata.dat";
            if (File.Exists(fileName))
            {
                using (FileStream stream = File.Open(fileName, FileMode.Open))
                using (BinaryReader br = new BinaryReader(stream))
                {
                    int version = br.ReadInt32();

                    inGame = br.ReadBoolean();
                }
            }
            return inGame;
        }
    }
}