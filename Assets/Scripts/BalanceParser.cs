using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


namespace Survivor
{
    public class BalanceParser : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("DOD/Balance/Parse Local")]
        public static void ParseLocal()
        {
            Debug.Log("Parse balance started!");

            if (!Directory.Exists(Application.persistentDataPath + "/Resources"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Resources");

            AssignIDs();
            byte[] array = parse();
            // save array
            string path = "Assets/Resources/balance.bytes";
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            using (FileStream fs = File.Create(path))
            using (BinaryWriter bw = new BinaryWriter(fs))
                bw.Write(array);

            Debug.Log("Parse balance finished!");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void AssignIDs()
        {
            List<Object> objects = new List<Object>();
            int numObjects;

            objects.Clear();
            AddObjectsFromDirectory("Assets/Data/Weapons", objects, typeof(WeaponSO));
            numObjects = objects.Count;
            for (int i = 0; i < numObjects; i++)
            {
                WeaponSO weapon = (WeaponSO)objects[i];
                weapon.ID = i;
                EditorUtility.SetDirty(weapon);
            }

            objects.Clear();
            AddObjectsFromDirectory("Assets/Data/Enemies", objects, typeof(EnemySO));
            numObjects = objects.Count;
            for (int i = 0; i < numObjects; i++)
            {
                EnemySO enemy = (EnemySO)objects[i];
                enemy.ID = i;
                EditorUtility.SetDirty(enemy);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static byte[] parse()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    int version = 1;
                    bw.Write(version);

                    BalanceSO balanceSO = (BalanceSO)AssetDatabase.LoadAssetAtPath("Assets/Data/Balance.asset", typeof(BalanceSO));

                    bw.Write(balanceSO.MaxEnemies);
                    bw.Write(balanceSO.SpawnRadius);

                    bw.Write(balanceSO.MaxPlayerWeapons);

                    bw.Write(balanceSO.MaxSkidMarks);
                    bw.Write(balanceSO.SkidMark.name);
                    bw.Write(balanceSO.SkidMarkColor.r);
                    bw.Write(balanceSO.SkidMarkColor.g);
                    bw.Write(balanceSO.SkidMarkColor.b);

                    bw.Write(balanceSO.MaxAmmo);
                    bw.Write(balanceSO.MaxParticles);

                    bw.Write(balanceSO.MaxXP);
                    bw.Write(balanceSO.XPAnimatedSprite.name);

                    List<Object> objects = new List<Object>();

                    objects.Clear();
                    AddObjectsFromDirectory("Assets/Data/Levels", objects, typeof(LevelSO));
                    int numLevels = objects.Count;
                    Debug.Log("numLevels " + numLevels);
                    bw.Write(numLevels);
                    for (int levelIdx = 0; levelIdx < numLevels; levelIdx++)
                    {
                        LevelSO levelSO = (LevelSO)objects[levelIdx];
                        int numWaves = levelSO.WaveInfo.Length;
                        bw.Write(numWaves);
                        for (int waveIdx = 0; waveIdx < numWaves; waveIdx++)
                        {
                            bw.Write(levelSO.WaveInfo[waveIdx].EnemySO.ID);
                            bw.Write(levelSO.WaveInfo[waveIdx].NumEnemies);
                            bw.Write(levelSO.WaveInfo[waveIdx].StartTime);
                            bw.Write(levelSO.WaveInfo[waveIdx].EndTime);
                            bw.Write(levelSO.WaveInfo[waveIdx].SpawnDelay);
                            bw.Write(levelSO.WaveInfo[waveIdx].GroupDelay);
                        }
                    }

                    objects.Clear();
                    AddObjectsFromDirectory("Assets/Data/Players", objects, typeof(PlayerSO));
                    int numPlayers = objects.Count;
                    Debug.Log("numPlayers " + numPlayers);
                    bw.Write(numPlayers);
                    for (int playerIdx = 0; playerIdx < numPlayers; playerIdx++)
                    {
                        PlayerSO playerSO = (PlayerSO)objects[playerIdx];
                        bw.Write(playerSO.Player.name);

                        bw.Write(playerSO.HP);
                        bw.Write(playerSO.Velocity);
                        bw.Write(playerSO.Weapon.ID);
                    }

                    objects.Clear();
                    AddObjectsFromDirectory("Assets/Data/Cars", objects, typeof(CarSO));
                    int numCars = objects.Count;
                    Debug.Log("numCars " + numCars);
                    bw.Write(numCars);
                    for (int carIdx = 0; carIdx < numCars; carIdx++)
                    {
                        CarSO carSO = (CarSO)objects[carIdx];
                        bw.Write(carSO.Car.name);
                        bw.Write(carSO.HP);
                        bw.Write(carSO.Velocity);
                        bw.Write(carSO.Acceleration);

                        int numCarFrames = carSO.Car.CarFrameInfo.Length;
                        bw.Write(numCarFrames);
                        bw.Write(360.0f / (float)numCarFrames);
                        for (int frameIdx = 0; frameIdx < numCarFrames; frameIdx++)
                        {
                            int numCircleCollisions = carSO.Car.CarFrameInfo[frameIdx].CollisionCircles.Length;
                            bw.Write(numCircleCollisions);
                            for (int circleIdx = 0; circleIdx < numCircleCollisions; circleIdx++)
                            {
                                bw.Write(carSO.Car.CarFrameInfo[frameIdx].CollisionCircles[circleIdx].transform.localPosition.x);
                                bw.Write(carSO.Car.CarFrameInfo[frameIdx].CollisionCircles[circleIdx].transform.localPosition.y);
                                bw.Write(carSO.Car.CarFrameInfo[frameIdx].CollisionCircles[circleIdx].transform.localScale.x / 2.0f);
                            }

                            for (int tireIdx = 0; tireIdx < 4; tireIdx++)
                            {
                                bw.Write(carSO.Car.CarFrameInfo[frameIdx].Tires[tireIdx].transform.localPosition.x);
                                bw.Write(carSO.Car.CarFrameInfo[frameIdx].Tires[tireIdx].transform.localPosition.y);
                            }
                        }
                    }

                    Dictionary<string, int> objectNameToType = new Dictionary<string, int>();
                    int spriteNameCounter = 0;

                    objects.Clear();
                    AddObjectsFromDirectory("Assets/Data/Weapons", objects, typeof(WeaponSO));
                    int numWeapons = objects.Count;
                    Debug.Log("numWeapons " + numWeapons);
                    bw.Write(numWeapons);
                    objectNameToType.Clear();
                    spriteNameCounter = 0;
                    for (int weaponIdx = 0; weaponIdx < numWeapons; weaponIdx++)
                    {
                        WeaponSO weaponSO = (WeaponSO)objects[weaponIdx];
                        bw.Write(weaponSO.ID);

                        bw.Write(weaponSO.AnimatedSprite.name);
                        if (!objectNameToType.ContainsKey(weaponSO.AnimatedSprite.name))
                            objectNameToType[weaponSO.AnimatedSprite.name] = spriteNameCounter++;
                        bw.Write(objectNameToType[weaponSO.AnimatedSprite.name]);

                        string explosionName = (weaponSO.ExplosionSprite == null) ? "" : weaponSO.ExplosionSprite.name;
                        bw.Write(explosionName);
                        string trailName = (weaponSO.TrailSprite == null) ? "" : weaponSO.TrailSprite.name;
                        bw.Write(trailName);
                        bw.Write(weaponSO.FiringRate);
                        bw.Write(weaponSO.Velocity);
                        bw.Write(weaponSO.AngularVelocity);
                        bw.Write(weaponSO.TriggerRadius);

                        bw.Write(weaponSO.ExplosionRadius);
                        bw.Write(weaponSO.NumProjectiles);
                        bw.Write((byte)weaponSO.WeaponTarget);
                        bw.Write(weaponSO.Damage);
                    }

                    objects.Clear();
                    AddObjectsFromDirectory("Assets/Data/Enemies", objects, typeof(EnemySO));
                    int numEnemies = objects.Count;
                    Debug.Log("numEnemies " + numEnemies);
                    bw.Write(numEnemies);
                    objectNameToType.Clear();
                    spriteNameCounter = 0;
                    for (int enemyIdx = 0; enemyIdx < numEnemies; enemyIdx++)
                    {
                        EnemySO enemySO = (EnemySO)objects[enemyIdx];
                        bw.Write(enemySO.ID);

                        bw.Write(enemySO.WalkingAnimation.name);
                        if (!objectNameToType.ContainsKey(enemySO.WalkingAnimation.name))
                            objectNameToType[enemySO.WalkingAnimation.name] = spriteNameCounter++;
                        bw.Write(objectNameToType[enemySO.WalkingAnimation.name]);
                        bw.Write(enemySO.DeathAnimation.name);
                        bw.Write(enemySO.DeathAnimation.FrameTime * enemySO.DeathAnimation.Sprites.Length);
                        bw.Write(enemySO.DyingColor.r);
                        bw.Write(enemySO.DyingColor.g);
                        bw.Write(enemySO.DyingColor.b);

                        bw.Write(enemySO.Velocity);
                        bw.Write(enemySO.Radius);
                        bw.Write(enemySO.HP);
                        bw.Write(enemySO.XP);
                        bw.Write(enemySO.ImpactSlowdown);
                    }

                    int magic = 123456789;
                    bw.Write(magic);
                    return stream.ToArray();
                }
            }
        }

        public static void AddObjectsFromDirectory(string path, List<Object> items, System.Type type)
        {
            if (Directory.Exists(path))
            {
                string[] assets = Directory.GetFiles(path);
                foreach (string assetPath in assets)
                    if (assetPath.Contains(".asset") && !assetPath.Contains(".meta"))
                        items.Add(AssetDatabase.LoadAssetAtPath(assetPath, type));

                string[] directories = Directory.GetDirectories(path);
                foreach (string directory in directories)
                    if (Directory.Exists(directory))
                        AddObjectsFromDirectory(directory, items, type);
            }
        }

#endif
    }
}
