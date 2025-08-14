using System;
using UnityEngine;
using CommonTools;

namespace Survivor
{
    public class Game : Singleton<Game>
    {
        public Board Board;
        public Camera MainCamera;

        MainMenuVisual m_mainMenuVisual;
        GameOverVisual m_gameOverVisual;
        PauseMenuVisual m_pauseMenuVisual;

        GameData m_gameData = new GameData();
        MetaData m_metaData = new MetaData();
        Balance m_balance = new Balance();

        int m_screenShotIdx = 0;

        protected override void Awake()
        {
            base.Awake();

            Application.targetFrameRate = 60;
        }

        // Start is called before the first frame update
        void Start()
        {
            AssetManager.Instance.LoadCommonAssetBundle();

            m_mainMenuVisual = AssetManager.Instance.GetMainMenuVisual();
            m_gameOverVisual = AssetManager.Instance.GetGameOverVisual();
            m_pauseMenuVisual = AssetManager.Instance.GetPauseMenuVisual();

            m_mainMenuVisual.Init(m_metaData);
            m_gameOverVisual.Init(m_metaData, m_gameData);
            m_pauseMenuVisual.Init();

            m_balance.LoadBalance();
            Logic.AllocateGameData(m_gameData, m_balance);

            MetaDataIO.Load(m_metaData);
            Logic.Init(m_metaData);

            Board.Init(m_metaData, m_gameData, m_balance, MainCamera);

            SetMenuState(MENU_STATE.MAIN_MENU);
        }

        public void SetMenuState(MENU_STATE newMenuState)
        {
            MENU_STATE oldMenuState = m_metaData.MenuState;
            Logic.SetMenuState(m_metaData, newMenuState);
            if (oldMenuState == MENU_STATE.MAIN_MENU)
            {
                m_mainMenuVisual.Hide();
            }
            else if (oldMenuState == MENU_STATE.GAME_OVER)
            {
                Board.Hide();
                m_gameOverVisual.Hide();
            }
            else if (oldMenuState == MENU_STATE.IN_GAME && newMenuState == MENU_STATE.PAUSE_MENU)
            {
                Board.Hide();
            }
            else if (oldMenuState == MENU_STATE.PAUSE_MENU)
            {
                m_pauseMenuVisual.Hide();
            }

            if (newMenuState == MENU_STATE.MAIN_MENU)
            {
                m_mainMenuVisual.Show();
            }
            else if (newMenuState == MENU_STATE.GAME_OVER)
            {
                m_gameOverVisual.Show();
            }
            else if (newMenuState == MENU_STATE.PAUSE_MENU)
            {
                m_pauseMenuVisual.Show();
            }
            else if (newMenuState == MENU_STATE.IN_GAME)
            {
                Board.Show();
            }
        }

        public void StartGame()
        {
            Board.StartGame();
            SetMenuState(MENU_STATE.IN_GAME);
        }

        public void ContinueGame()
        {
            GameDataIO.Load(m_gameData);
            SetMenuState(MENU_STATE.IN_GAME);
        }

        public float sqrMagnitude = 0.0f;
        public void RunTest()
        {
            int numFrames = 600;

            Logic.StartGame(m_gameData, m_balance);
            float dt = 1.0f / 60.0f;

            bool isGameOver;
            Span<int> spawnedEnemyIdxs = stackalloc int[m_balance.MaxEnemies];
            int spawnedEnemyCount;
            Span<int> deadEnemyIdxs = stackalloc int[m_balance.MaxEnemies];
            int deadEnemyCount;
            Span<int> dyingEnemyIdxs = stackalloc int[m_balance.MaxEnemies];
            int dyingEnemyCount;
            Span<int> firedAmmoIdxs = stackalloc int[m_balance.MaxAmmo];
            int firedAmmoCount;
            Span<int> deadAmmoIdxs = stackalloc int[m_balance.MaxAmmo];
            int deadAmmoCount;
            Span<int> xpPlacedIdxs = stackalloc int[m_balance.MaxXP];
            int xpPlacedCount;

            int xpPickedUpMax = 10;
            Span<int> xpPickedUpIdxs = stackalloc int[xpPickedUpMax];
            int xpPickedUpCount;

            m_gameData.PlayerTargetDirection = new Vector2(0.0f, 1.0f);

            double time = Time.realtimeSinceStartupAsDouble;
            double mapTime = 0.0f;
            for (int i = 0; i < numFrames; i++)
            {
                Logic.Tick(
                    m_metaData,
                    m_gameData,
                    m_balance,
                    dt,
                    spawnedEnemyIdxs,
                    out spawnedEnemyCount,
                    deadEnemyIdxs,
                    out deadEnemyCount,
                    dyingEnemyIdxs,
                    out dyingEnemyCount,
                    firedAmmoIdxs,
                    out firedAmmoCount,
                    deadAmmoIdxs,
                    out deadAmmoCount,
                    xpPlacedIdxs,
                    out xpPlacedCount,
                    xpPickedUpIdxs,
                    out xpPickedUpCount,
                    xpPickedUpMax,
                    out isGameOver);

                double t = Time.realtimeSinceStartupAsDouble;
                Logic.AddAllEnemiesToMap(m_gameData, m_balance);
                mapTime += Time.realtimeSinceStartupAsDouble - t;
            }

            Debug.Log(numFrames.ToString() + " frames test time " + (Time.realtimeSinceStartupAsDouble - time).ToString());
            Debug.Log(numFrames.ToString() + " map time " + mapTime.ToString());
        }

        // Update is called once per frame
        void Update()
        {
            if (m_metaData.MenuState == MENU_STATE.IN_GAME)
                Board.Tick(Time.deltaTime);

            if (Input.GetKeyUp("s"))
                captureScreenshot();
        }
        void captureScreenshot()
        {
            ScreenCapture.CaptureScreenshot("screenshot" + (m_screenShotIdx++) + ".png");
        }
    }
}