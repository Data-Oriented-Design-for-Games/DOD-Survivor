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
        public static bool UseAssetBundles = false;

        [SerializeField] AnimatedSprite m_playerPrefab;
        [SerializeField] AnimatedSprite m_enemyPrefab;
        [SerializeField] AnimatedSprite m_ammoPrefab;

        [SerializeField] GameObject m_UIInGame;
        [SerializeField] GameObject m_UIMainMenu;
        [SerializeField] GameObject m_UIGameOver;
        [SerializeField] GameObject m_UIPauseMenu;

        [SerializeField] MainMenuVisual m_MainMenuVisual;
        [SerializeField] GameOverVisual m_GameOverVisual;
        [SerializeField] PauseMenuVisual m_PauseMenuVisual;

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

        GameObject loadGameObject(AssetBundle assetBundle, string objName, string localPath)
        {
            // Debug.Log("loadGameObject objName " + objName + " localPath " + localPath);

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

        public AnimatedSprite GetPlayer(string playerName, Transform spriteParent)
        {
            return Instantiate(loadGameObject(m_commonBundle, playerName, "Assets/Prefabs/Common/Players/" + playerName + ".prefab"), spriteParent).GetComponent<AnimatedSprite>();
        }

        public AnimatedSprite GetEnemy(string enemyName, Transform spriteParent)
        {
            return Instantiate(loadGameObject(m_commonBundle, enemyName, "Assets/Prefabs/Common/Enemies/" + enemyName + ".prefab"), spriteParent).GetComponent<AnimatedSprite>();
        }

        public AnimatedSprite GetWeapon(string weaponName, Transform spriteParent)
        {
            return Instantiate(loadGameObject(m_commonBundle, weaponName, "Assets/Prefabs/Common/Weapons/" + weaponName + ".prefab"), spriteParent).GetComponent<AnimatedSprite>();
        }

        public AnimatedSprite GetParticle(string particleName, Transform spriteParent)
        {
            return Instantiate(loadGameObject(m_commonBundle, particleName, "Assets/Prefabs/Common/Particles/" + particleName + ".prefab"), spriteParent).GetComponent<AnimatedSprite>();
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