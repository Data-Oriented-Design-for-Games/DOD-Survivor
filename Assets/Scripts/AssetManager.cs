using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CommonTools;
using UnityEngine;
using UnityEditor;

namespace Survivor
{
    public class AssetManager : Singleton<AssetManager>
    {
        public static bool UseAssetBundles = true;

        [SerializeField] GameObject m_UIInGame = null;
        [SerializeField] GameObject m_UIMainMenu = null;
        [SerializeField] GameObject m_UIGameOver = null;
        [SerializeField] GameObject m_UIPauseMenu = null;

        [SerializeField] MainMenuVisual m_MainMenuVisual = null;
        [SerializeField] GameOverVisual m_GameOverVisual = null;
        [SerializeField] PauseMenuVisual m_PauseMenuVisual = null;

        AssetBundle m_commonBundle;

        public void LoadCommonAssetBundle()
        {
#if UNITY_EDITOR
            if (UseAssetBundles)
                m_commonBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "AssetBundles/common"));
#else
            m_commonBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "AssetBundles/common"));
#endif
        }

        public void UnloadCommonAssetBundle()
        {
            if (m_commonBundle != null)
                m_commonBundle.Unload(true);
        }

        public Sprite loadSprite(AssetBundle assetBundle, string spriteName, string localPath)
        {
            Sprite sprite = null;
#if UNITY_EDITOR
            if (UseAssetBundles)
                sprite = assetBundle.LoadAsset<Sprite>(spriteName);
            else
                sprite = (Sprite)AssetDatabase.LoadAssetAtPath(localPath, typeof(Sprite));
#else
            sprite = assetBundle.LoadAsset<Sprite>(spriteName);
#endif
            return sprite;
        }

        GameObject loadGameObject(AssetBundle assetBundle, string objName, string localPath)
        {
            //Debug.Log("loadGameObject objName " + objName + " localPath " + localPath);

            GameObject go = null;
#if UNITY_EDITOR
            if (UseAssetBundles)
                go = assetBundle.LoadAsset<GameObject>(objName);
            else
                go = (GameObject)AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject));
#else
            go = assetBundle.LoadAsset<GameObject>(objName);
#endif
            return go;
        }

        public Player GetPlayer(string playerName, Transform spriteParent)
        {
            return Instantiate(loadGameObject(m_commonBundle, playerName, "Assets/Prefabs/Common/Players/" + playerName + ".prefab"), spriteParent).GetComponent<Player>();
        }

        public Car GetCar(string carName, Transform spriteParent)
        {
            return Instantiate(loadGameObject(m_commonBundle, carName, "Assets/Prefabs/Common/Cars/" + carName + ".prefab"), spriteParent).GetComponent<Car>();
        }

        public AnimatedSprite GetAnimatedSprite(string spriteName, string spritePath, Transform spriteParent)
        {
            return Instantiate(loadGameObject(m_commonBundle, spriteName, "Assets/Prefabs/Common/" + spritePath + spriteName + ".prefab"), spriteParent).GetComponent<AnimatedSprite>();
        }

        public EnemySprite GetEnemy(string enemyName, Transform parent)
        {
            return Instantiate(loadGameObject(m_commonBundle, enemyName, "Assets/Prefabs/Common/Enemies" + enemyName + ".prefab"), parent).GetComponent<EnemySprite>();
        }

        public GameObject GetTireMark(string tireMarkName, Transform parent)
        {
            return Instantiate(loadGameObject(m_commonBundle, tireMarkName, "Assets/Prefabs/Common/" + tireMarkName + ".prefab"), parent);
        }

        public GameObject GetInGameUI()
        {
            return Instantiate(m_UIInGame);
        }

        public GameObject GetMainMenuUI()
        {
            return Instantiate(m_UIMainMenu);
        }

        public GameObject GetGameOverUI()
        {
            return Instantiate(m_UIGameOver);
        }

        public GameObject GetPauseMenuUI()
        {
            return Instantiate(m_UIPauseMenu);
        }

        public MainMenuVisual GetMainMenuVisual()
        {
            return Instantiate(m_MainMenuVisual);
        }

        public GameOverVisual GetGameOverVisual()
        {
            return Instantiate(m_GameOverVisual);
        }

        public PauseMenuVisual GetPauseMenuVisual()
        {
            return Instantiate(m_PauseMenuVisual);
        }
    }
}