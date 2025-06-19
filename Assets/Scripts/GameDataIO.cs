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

                bw.Write(balance.NumEnemies);
                for (int i = 0; i < balance.NumEnemies; i++)
                {
                    bw.Write(gameData.EnemyPosition[i].x);
                    bw.Write(gameData.EnemyPosition[i].y);
                }
                for (int i = 0; i < balance.NumEnemies; i++)
                {
                    bw.Write(gameData.EnemyDirection[i].x);
                    bw.Write(gameData.EnemyDirection[i].y);
                }

                bw.Write(gameData.PlayerDirection.x);
                bw.Write(gameData.PlayerDirection.y);

                bw.Write(balance.NumAmmo);
                for (int i = 0; i < balance.NumAmmo; i++)
                {
                    bw.Write(gameData.AmmoPosition[i].x);
                    bw.Write(gameData.AmmoPosition[i].y);
                }
                for (int i = 0; i < balance.NumAmmo; i++)
                {
                    bw.Write(gameData.AmmoDirection[i].x);
                    bw.Write(gameData.AmmoDirection[i].y);
                }
                bw.Write(gameData.AliveAmmoCount);
                bw.Write(gameData.DeadAmmoCount);
                for (int i = 0; i < balance.NumAmmo; i++)
                    bw.Write(gameData.AliveAmmoIdx[i]);
                for (int i = 0; i < balance.NumAmmo; i++)
                    bw.Write(gameData.DeadAmmoIdx[i]);
                bw.Write(gameData.FiringRateTimer);

                bw.Write(gameData.GameTime);
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

                    int numAmmo = br.ReadInt32();
                    for (int i = 0; i < numAmmo; i++)
                    {
                        gameData.AmmoPosition[i].x = br.ReadSingle();
                        gameData.AmmoPosition[i].y = br.ReadSingle();
                    }
                    for (int i = 0; i < numAmmo; i++)
                    {
                        gameData.AmmoDirection[i].x = br.ReadSingle();
                        gameData.AmmoDirection[i].y = br.ReadSingle();
                    }
                    gameData.AliveAmmoCount = br.ReadInt32();
                    gameData.DeadAmmoCount = br.ReadInt32();
                    for (int i = 0; i < numAmmo; i++)
                        gameData.AliveAmmoIdx[i] = br.ReadInt32();
                    for (int i = 0; i < numAmmo; i++)
                        gameData.DeadAmmoIdx[i] = br.ReadInt32();
                    gameData.FiringRateTimer = br.ReadInt32();

                    gameData.GameTime = br.ReadSingle();
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