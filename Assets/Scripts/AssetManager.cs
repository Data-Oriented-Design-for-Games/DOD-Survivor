using System;
using System.Collections;
using System.Collections.Generic;
using CommonTools;
using UnityEngine;

namespace Survivor
{
    public class AssetManager : Singleton<AssetManager>
    {
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

        public AnimatedSprite GetPlayer(Transform enemyParent)
        {
            return Instantiate(m_playerPrefab, enemyParent);
        }

        public AnimatedSprite GetEnemy(Transform enemyParent)
        {
            return Instantiate(m_enemyPrefab, enemyParent);
        }

        public AnimatedSprite GetAmmo(Transform enemyParent)
        {
            return Instantiate(m_ammoPrefab, enemyParent);
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