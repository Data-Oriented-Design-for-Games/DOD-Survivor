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
                bw.Write(gameData.LastPlayerDirection.x);
                bw.Write(gameData.LastPlayerDirection.y);

                bw.Write(balance.MaxWeapons);
                for (int i = 0; i < balance.MaxWeapons; i++)
                {
                    bw.Write(gameData.WeaponPosition[i].x);
                    bw.Write(gameData.WeaponPosition[i].y);
                }
                for (int i = 0; i < balance.MaxWeapons; i++)
                {
                    bw.Write(gameData.WeaponDirection[i].x);
                    bw.Write(gameData.WeaponDirection[i].y);
                }
                for (int i = 0; i < balance.MaxWeapons; i++)
                    bw.Write(gameData.WeaponAngle[i]);

                bw.Write(gameData.AliveWeaponCount);
                bw.Write(gameData.DeadWeaponCount);
                for (int i = 0; i < balance.MaxWeapons; i++)
                    bw.Write(gameData.AliveWeaponIdx[i]);
                for (int i = 0; i < balance.MaxWeapons; i++)
                    bw.Write(gameData.DeadWeaponIdx[i]);
                for (int i = 0; i < balance.MaxWeapons; i++)
                bw.Write(gameData.FiringRateTimer[i]);

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
                    gameData.LastPlayerDirection.x = br.ReadSingle();
                    gameData.LastPlayerDirection.y = br.ReadSingle();

                    int numWeapons = br.ReadInt32();
                    for (int i = 0; i < numWeapons; i++)
                    {
                        gameData.WeaponPosition[i].x = br.ReadSingle();
                        gameData.WeaponPosition[i].y = br.ReadSingle();
                    }
                    for (int i = 0; i < numWeapons; i++)
                    {
                        gameData.WeaponDirection[i].x = br.ReadSingle();
                        gameData.WeaponDirection[i].y = br.ReadSingle();
                    }
                    for (int i = 0; i < numWeapons; i++)
                        gameData.WeaponAngle[i] = br.ReadSingle();

                    gameData.AliveWeaponCount = br.ReadInt32();
                    gameData.DeadWeaponCount = br.ReadInt32();
                    for (int i = 0; i < numWeapons; i++)
                        gameData.AliveWeaponIdx[i] = br.ReadInt32();
                    for (int i = 0; i < numWeapons; i++)
                        gameData.DeadWeaponIdx[i] = br.ReadInt32();
                    for (int i = 0; i < numWeapons; i++)
                    gameData.FiringRateTimer[i] = br.ReadInt32();

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