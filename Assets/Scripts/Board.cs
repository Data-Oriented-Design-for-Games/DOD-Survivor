using System;
using CommonTools;
using TMPro;
using UnityEngine;

namespace Survivor
{
    public class BoardGUI
    {
        public GameObject UI;
        public TextMeshProUGUI GameTimeText;
    }

    public struct SpriteAnimationData
    {
        public int FrameIndex;
        public int NumFrames;
        public float FrameTimeLeft;
        public float FrameTime;
        public bool FrameChanged;
    }


    public class Board : MonoBehaviour
    {
        public Transform SpriteParent;

        AnimatedSprite m_player;

        EnemyPool m_enemyPool = new EnemyPool();
        WeaponPool m_weaponPool = new WeaponPool();
        ParticlePool m_trailPool = new ParticlePool();
        ParticlePool m_explosionPool = new ParticlePool();

        Camera m_mainCamera;
        Vector2 m_mouseDownPos;

        BoardGUI m_boardGUI;

        public GameObject InputCircleOut;
        public GameObject InputCircleIn;

        GameData gameData;
        MetaData metaData;
        Balance balance;

        public class VisualBoardData
        {
            public SpriteAnimationData PlayerSpriteAnimData;
            public SpriteAnimationData[] EnemySpriteAnimData;
            public SpriteAnimationData[] WeaponSpriteAnimData;
        }

        VisualBoardData m_visualBoardData = new VisualBoardData();

        public void Init(MetaData metaData, GameData gameData, Balance balance, Camera mainCamera)
        {
            m_mainCamera = mainCamera;

            this.metaData = metaData;
            this.gameData = gameData;
            this.balance = balance;

            // player
            m_player = AssetManager.Instance.GetPlayer(balance.PlayerBalance.PlayerBalanceData[gameData.PlayerType].SpriteName, SpriteParent);
            m_player.transform.localPosition = Vector2.zero;

            CommonVisual.InitSpriteFrameData(ref m_visualBoardData.PlayerSpriteAnimData, m_player);

            // enemies
            m_enemyPool.Init(balance, SpriteParent);
            m_visualBoardData.EnemySpriteAnimData = new SpriteAnimationData[balance.MaxEnemies];

            // weapons
            m_weaponPool.Init(balance, SpriteParent);
            m_visualBoardData.WeaponSpriteAnimData = new SpriteAnimationData[balance.MaxAmmo];

            // particles
            m_explosionPool.Init(balance, SpriteParent);
            m_trailPool.Init(balance, SpriteParent);

            // GUI
            m_boardGUI = new BoardGUI();
            m_boardGUI.UI = AssetManager.Instance.GetInGameUI();

            GUIRef guiRef = m_boardGUI.UI.GetComponent<GUIRef>();
            m_boardGUI.GameTimeText = guiRef.GetTextGUI("GameTime");
            guiRef.GetButton("Pause").onClick.AddListener(pauseGame);

            m_player.gameObject.SetActive(false);
            InputCircleOut.SetActive(false);

            hideUI();
        }

        public void StartGame()
        {
            Logic.StartGame(gameData, balance);
        }

        public void Show()
        {
            m_player.gameObject.SetActive(true);

            m_boardGUI.UI.SetActive(true);
        }

        public void Hide()
        {
            m_enemyPool.Clear();
            m_weaponPool.Clear();
            m_explosionPool.Clear();
            m_trailPool.Clear();

            m_player.gameObject.SetActive(false);

            hideUI();
        }

        public void hideUI()
        {
            m_boardGUI.UI.SetActive(false);
        }

        public void Tick(float dt)
        {
            handleInput();

            bool isGameOver;
            Span<int> spawnedEnemyIdxs = stackalloc int[balance.MaxEnemies];
            int spawnedEnemyCount;
            Span<int> deadEnemyIdxs = stackalloc int[balance.MaxEnemies];
            int deadEnemyCount;
            Span<int> firedAmmoIdxs = stackalloc int[balance.MaxAmmo];
            int firedAmmoCount;
            Span<int> deadAmmoIdxs = stackalloc int[balance.MaxAmmo];
            int deadAmmoCount;
            Logic.Tick(metaData, gameData, balance, dt, spawnedEnemyIdxs, out spawnedEnemyCount, deadEnemyIdxs, out deadEnemyCount, firedAmmoIdxs, out firedAmmoCount, deadAmmoIdxs, out deadAmmoCount, out isGameOver);

            updateVisuals(spawnedEnemyIdxs, spawnedEnemyCount, deadEnemyIdxs, deadEnemyCount, firedAmmoIdxs, firedAmmoCount, deadAmmoIdxs, deadAmmoCount, dt);

            if (isGameOver)
                gameOver();
        }

        private void updateVisuals(
            Span<int> spawnedEnemyIdxs,
            int spawnedEnemyCount,
            Span<int> deadEnemyIdxs,
            int deadEnemyCount,
            Span<int> firedAmmoIdxs,
            int firedAmmoCount,
            Span<int> deadAmmoIdxs,
            int deadAmmoCount,
            float dt)
        {
            // player
            CommonVisual.AnimateSprite(dt, ref m_visualBoardData.PlayerSpriteAnimData);
            CommonVisual.TryChangeSpriteFrame(ref m_visualBoardData.PlayerSpriteAnimData, m_player);

            float playerScaleX = gameData.PlayerDirection.x > 0.0f ? 1.0f : -1.0f;
            m_player.transform.localScale = new Vector3(playerScaleX, 1.0f, 1.0f);

            // enemies
            for (int i = 0; i < deadEnemyCount; i++)
            {
                int enemyIndex = deadEnemyIdxs[i];
                m_enemyPool.HideEnemy(enemyIndex);
            }
            for (int i = 0; i < spawnedEnemyCount; i++)
            {
                int enemyIndex = spawnedEnemyIdxs[i];
                int spriteType = balance.EnemyBalance.SpriteType[gameData.EnemyType[enemyIndex]];
                m_enemyPool.ShowEnemy(enemyIndex, spriteType, gameData.EnemyPosition[enemyIndex]);
            }
            m_enemyPool.Tick(gameData, dt);

            // weapons
            for (int i = 0; i < firedAmmoCount; i++)
            {
                int ammoIndex = firedAmmoIdxs[i];
                int spriteType = balance.WeaponBalance.SpriteType[gameData.AmmoType[ammoIndex]];
                m_weaponPool.ShowWeapon(ammoIndex, spriteType, gameData.AmmoPosition[ammoIndex]);

            }
            for (int i = 0; i < deadAmmoCount; i++)
            {
                int ammoIndex = deadAmmoIdxs[i];
                m_weaponPool.HideWeapon(ammoIndex);

                if (balance.WeaponBalance.ExplosionName[gameData.AmmoType[ammoIndex]].Length > 0)
                {
                    int spriteType = balance.WeaponBalance.SpriteType[gameData.AmmoType[ammoIndex]];
                    m_explosionPool.ShowParticle(spriteType, gameData.AmmoPosition[ammoIndex], balance.WeaponBalance.ExplosionName[spriteType]);
                }
            }
            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                m_trailPool.ShowParticle(gameData.AmmoType[ammoIndex], gameData.AmmoPosition[ammoIndex], balance.WeaponBalance.TrailName[gameData.AmmoType[ammoIndex]]);
            }
            m_weaponPool.Tick(gameData, dt);
            m_explosionPool.Tick(dt);
            m_trailPool.Tick(dt);

            // ui
            for (int i = 0; i < balance.MaxEnemies; i++)
                m_boardGUI.GameTimeText.text = CommonVisual.GetTimeElapsedString(gameData.GameTime);
        }

        void handleInput()
        {
#if UNITY_EDITOR
            bool mouseDown = Input.GetMouseButtonDown(0);
            bool mouseMove = Input.GetMouseButton(0);
            bool mouseUp = Input.GetMouseButtonUp(0);
            Vector3 mousePosition = Input.mousePosition;
#else
bool mouseDown = (Input.touchCount > 0) && Input.GetTouch(0).phase == TouchPhase.Began;
bool mouseMove = (Input.touchCount > 0) && Input.GetTouch(0).phase == TouchPhase.Moved;
bool mouseUp = (Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled);
Vector3 mousePosition = Vector3.zero;
if (Input.touchCount > 0)
mousePosition = Input.GetTouch(0).position;
#endif
            Vector3 mouseWorldPos = m_mainCamera.ScreenToWorldPoint(mousePosition);
            Vector2 mouseLocalPos = SpriteParent.InverseTransformPoint(mouseWorldPos);

            if (mouseDown)

            {
                InputCircleOut.SetActive(true);
                m_mouseDownPos = mouseLocalPos;
            }

            if (mouseMove)
            {
                InputCircleOut.transform.position = m_mouseDownPos;
                Vector2 diff = (mouseLocalPos - m_mouseDownPos);
                float dist = diff.magnitude;
                if (dist > 1.0f)
                    dist = 1.0f;
                InputCircleIn.transform.localPosition = (mouseLocalPos - m_mouseDownPos).normalized * dist * ((1.0f - InputCircleIn.transform.localScale.x) / 2.0f);
                Logic.MouseMove(gameData, m_mouseDownPos, mouseLocalPos);
            }

            if (mouseUp)
            {
                InputCircleOut.SetActive(false);
                Logic.MouseUp(gameData);
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                Span<int> firedWeaponIdxs = stackalloc int[balance.MaxAmmo];
                int firedWeaponCount = 0;

                Logic.TestFireWeapon(gameData, balance, (1.0f / 60.0f), firedWeaponIdxs, ref firedWeaponCount);
                for (int i = 0; i < firedWeaponCount; i++)
                {
                    int ammoIndex = firedWeaponIdxs[i];
                    int spriteType = balance.WeaponBalance.SpriteType[gameData.AmmoType[ammoIndex]];
                    m_weaponPool.ShowWeapon(ammoIndex, spriteType, gameData.AmmoPosition[ammoIndex]);
                }
            }
        }

        void updateVisuals()
        {

        }

        void gameOver()
        {
            Game.Instance.SetMenuState(MENU_STATE.GAME_OVER);
            MetaDataIO.Save(metaData);
            hideUI();
        }

        void pauseGame()
        {
            Game.Instance.SetMenuState(MENU_STATE.PAUSE_MENU);
            GameDataIO.Save(gameData, balance);
            MetaDataIO.Save(metaData);
        }
    }
}