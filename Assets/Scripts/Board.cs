using System;
using CommonTools;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Survivor
{
    public class BoardGUI
    {
        public GameObject UI;
        public TextMeshProUGUI GameTimeText;
        public TextMeshProUGUI StatsText;
    }

    public struct SpriteAnimationData
    {
        public int FrameIndex;
        public int NumFrames;
        public float FrameTimeLeft;
        public float FrameTime;
        public bool FrameChanged;
    }

    public class PlayerAnimationData
    {
        public SpriteAnimationData SpriteAnimationData;
        public int DirectionIndex;
        public int NumDirections;
        public float Angle;
    }


    public class Board : MonoBehaviour
    {
        public Transform SpriteParent;

        Player m_hero;
        Car m_car;

        EnemyPool m_enemyPool = new EnemyPool();
        WeaponPool m_ammoPool = new WeaponPool();
        ParticlePool m_trailPool = new ParticlePool();
        ParticlePool m_explosionPool = new ParticlePool();
        DyingEnemyPool m_dyingEnemyPool = new DyingEnemyPool();
        XPPool m_xpPool = new XPPool();
        TireTrackPool m_tireTrackPool = new TireTrackPool();

        Camera m_mainCamera;
        Vector2 m_mouseDownPos;

        BoardGUI m_boardGUI;

        public GameObject InputCircleOut;
        public GameObject InputCircleIn;

        GameData gameData;
        MetaData metaData;
        Balance balance;

        PlayerAnimationData m_playerAnimationData;

        public void Init(MetaData metaData, GameData gameData, Balance balance, Camera mainCamera)
        {
            m_mainCamera = mainCamera;

            this.metaData = metaData;
            this.gameData = gameData;
            this.balance = balance;

            // enemies
            m_enemyPool.Init(balance, SpriteParent);

            // weapons
            m_ammoPool.Init(balance, SpriteParent);

            // particles
            m_explosionPool.Init(gameData, balance, SpriteParent);
            m_trailPool.Init(gameData, balance, SpriteParent);
            m_tireTrackPool.Init(gameData, balance.MaxTireTracks * 4, SpriteParent, balance.TireTrackName);

            // dead enemies
            m_dyingEnemyPool.Init(gameData, balance, SpriteParent);

            // xp
            m_xpPool.Init(gameData, balance, SpriteParent);

            // GUI
            m_boardGUI = new BoardGUI();
            m_boardGUI.UI = AssetManager.Instance.GetInGameUI();

            GUIRef guiRef = m_boardGUI.UI.GetComponent<GUIRef>();
            m_boardGUI.GameTimeText = guiRef.GetTextGUI("GameTime");
            m_boardGUI.StatsText = guiRef.GetTextGUI("Stats");
            guiRef.GetButton("Pause").onClick.AddListener(pauseGame);

            InputCircleOut.SetActive(false);

            hideUI();
        }

        public void StartGame()
        {
            Logic.StartGame(gameData, balance);
        }

        public void Show()
        {
            // player
            m_hero = AssetManager.Instance.GetPlayer(balance.HeroBalance.HeroBalanceData[gameData.HeroType].heroName, SpriteParent);
            m_hero.transform.localPosition = new Vector3(0.0f, 0.0f, -10.0f);
            m_playerAnimationData = new PlayerAnimationData();
            CommonVisual.InitPlayerFrameData(m_playerAnimationData, m_hero);
            m_hero.gameObject.SetActive(!gameData.InCar);

            m_car = AssetManager.Instance.GetCar(balance.CarBalance[gameData.CarType].CarName, SpriteParent);
            m_car.transform.localPosition = new Vector3(0.0f, 0.0f, -10.0f);
            m_car.gameObject.SetActive(gameData.InCar);

            m_boardGUI.UI.SetActive(true);
        }

        public void Hide()
        {
            m_enemyPool.Clear();
            m_ammoPool.Clear();
            m_explosionPool.Clear();
            m_trailPool.Clear();
            m_dyingEnemyPool.Clear();
            m_xpPool.Clear();

            m_hero.gameObject.SetActive(false);
            GameObject.Destroy(m_hero);

            m_car.gameObject.SetActive(false);
            GameObject.Destroy(m_car);

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
            Span<int> dyingEnemyIdxs = stackalloc int[balance.MaxEnemies];
            int dyingEnemyCount;
            Span<int> firedAmmoIdxs = stackalloc int[balance.MaxAmmo];
            int firedAmmoCount;
            Span<int> deadAmmoIdxs = stackalloc int[balance.MaxAmmo];
            int deadAmmoCount;
            Span<int> xpPlacedIdxs = stackalloc int[balance.MaxXP];
            int xpPlacedCount;

            int xpPickedUpMax = 10;
            Span<int> xpPickedUpIdxs = stackalloc int[xpPickedUpMax];
            int xpPickedUpCount;

            Logic.Tick(
                metaData,
                gameData,
                balance,
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

            updateVisuals(
                spawnedEnemyIdxs,
                spawnedEnemyCount,
                deadEnemyIdxs,
                deadEnemyCount,
                dyingEnemyIdxs,
                dyingEnemyCount,
                firedAmmoIdxs,
                firedAmmoCount,
                deadAmmoIdxs,
                deadAmmoCount,
                xpPlacedIdxs,
                xpPlacedCount,
                xpPickedUpIdxs,
                xpPickedUpCount,
                dt);

            if (isGameOver)
                gameOver();
        }

        private void updateVisuals(
            Span<int> spawnedEnemyIdxs,
            int spawnedEnemyCount,
            Span<int> deadEnemyIdxs,
            int deadEnemyCount,
            Span<int> dyingEnemyIdxs,
            int dyingEnemyCount,
            Span<int> firedAmmoIdxs,
            int firedAmmoCount,
            Span<int> deadAmmoIdxs,
            int deadAmmoCount,
            Span<int> xpPlacedIdxs,
            int xpPlacedCount,
            Span<int> xpPickedUpIdxs,
            int xpPickedUpCount,
            float dt)
        {
            if (gameData.InCar) // todo needed?
                m_playerAnimationData.DirectionIndex = gameData.CarSlideIndex;


            if (gameData.InCar)
            {
                m_car.UpdateFrame(m_playerAnimationData);

                // skid marks
                for (int tireIdx = 0; tireIdx < 4; tireIdx++)
                {
                    int index = tireIdx * balance.MaxTireTracks + gameData.LastTireMarkIndex;
                    Vector2 tirePos = gameData.TireMarkPos[index];
                    m_tireTrackPool.ShowTireTrack(tirePos, gameData.TireMarkColor[index]);
                }
            }
            else
            {
                CommonVisual.AnimateSprite(dt, ref m_playerAnimationData.SpriteAnimationData);
                if (m_playerAnimationData.SpriteAnimationData.FrameChanged)
                    m_hero.UpdateFrame(m_playerAnimationData);
            }

            float playerScaleX = gameData.CarSlideDirection.x > 0.0f ? 1.0f : -1.0f;
            if (gameData.InCar)
                m_car.transform.localScale = new Vector3(playerScaleX, 1.0f, 1.0f);
            else
                m_hero.transform.localScale = new Vector3(playerScaleX, 1.0f, 1.0f);

            // enemies
            for (int i = 0; i < dyingEnemyCount; i++)
            {
                int enemyIndex = dyingEnemyIdxs[i];
                int spriteType = balance.EnemyBalance.SpriteType[gameData.EnemyType[enemyIndex]];
                m_enemyPool.HideEnemy(enemyIndex);
                m_dyingEnemyPool.ShowDyingEnemy(enemyIndex, spriteType, gameData.EnemyPosition[enemyIndex]);
            }
            for (int i = 0; i < deadEnemyCount; i++)
            {
                int enemyIndex = deadEnemyIdxs[i];
                m_dyingEnemyPool.HideDyingEnemy(enemyIndex);
            }
            for (int i = 0; i < spawnedEnemyCount; i++)
            {
                int enemyIndex = spawnedEnemyIdxs[i];
                int spriteType = balance.EnemyBalance.SpriteType[gameData.EnemyType[enemyIndex]];
                m_enemyPool.ShowEnemy(enemyIndex, spriteType, gameData.EnemyPosition[enemyIndex]);
            }
            m_enemyPool.Tick(gameData, dt);

            // weapons and ammo
            for (int i = 0; i < firedAmmoCount; i++)
            {
                int ammoIndex = firedAmmoIdxs[i];
                int spriteType = balance.WeaponBalance.SpriteType[gameData.AmmoType[ammoIndex]];
                m_ammoPool.ShowAmmo(ammoIndex, spriteType, gameData.AmmoPosition[ammoIndex]);

            }
            for (int i = 0; i < deadAmmoCount; i++)
            {
                int ammoIndex = deadAmmoIdxs[i];
                m_ammoPool.HideWeapon(ammoIndex);

                if (balance.WeaponBalance.ExplosionName[gameData.AmmoType[ammoIndex]].Length > 0)
                {
                    int spriteType = balance.WeaponBalance.SpriteType[gameData.AmmoType[ammoIndex]];
                    m_explosionPool.ShowParticle(spriteType, gameData.AmmoPosition[ammoIndex], 0.0f, balance.WeaponBalance.ExplosionName[spriteType]);
                }
            }
            for (int i = 0; i < gameData.AliveAmmoCount; i++)
            {
                int ammoIndex = gameData.AliveAmmoIdx[i];
                Vector2 position = gameData.AmmoPosition[ammoIndex];
                position.x += UnityEngine.Random.value * 0.1f - 0.05f;
                position.y += UnityEngine.Random.value * 0.1f - 0.05f;
                float angle = UnityEngine.Random.value * 360.0f;

                m_trailPool.ShowParticle(gameData.AmmoType[ammoIndex], gameData.AmmoPosition[ammoIndex], angle, balance.WeaponBalance.TrailName[gameData.AmmoType[ammoIndex]]);
            }

            for (int i = 0; i < xpPlacedCount; i++)
            {
                int xpIndex = xpPlacedIdxs[i];
                m_xpPool.ShowXP(xpIndex, 0, gameData.XPPosition[xpIndex]);
            }
            for (int i = 0; i < xpPickedUpCount; i++)
            {
                int xpIndex = xpPickedUpIdxs[i];
                m_xpPool.HideXP(xpIndex);
            }

            m_ammoPool.Tick(gameData, dt);
            m_explosionPool.Tick(dt);
            m_trailPool.Tick(dt);
            m_dyingEnemyPool.Tick(dt);
            m_xpPool.Tick(dt);
            m_tireTrackPool.Tick();

            // ui
            for (int i = 0; i < balance.MaxEnemies; i++)
                m_boardGUI.GameTimeText.text = CommonVisual.GetTimeElapsedString(gameData.GameTime);

            string statsText = "Enemies alive " + gameData.AliveEnemyCount.ToString("N0") + "\n";
            statsText += "Enemies dying " + gameData.DyingEnemyCount.ToString("N0") + "\n";
            statsText += "Enemies dead " + gameData.StatsEnemiesKilled.ToString("N0");
            m_boardGUI.StatsText.text = statsText;
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
                // if (gameData.InCar)
                //     SteeringWheel.SetActive(true);
                // else
                InputCircleOut.SetActive(true);
                m_mouseDownPos = mouseLocalPos;
            }

            if (mouseMove)
            {

                InputCircleOut.transform.position = m_mouseDownPos;

                Vector2 diff = mouseLocalPos - m_mouseDownPos;
                float magnitude = diff.magnitude;
                if (magnitude > 1.0f)
                    magnitude = 1.0f;
                if (gameData.InCar)
                    Logic.MouseMoveCar(gameData, m_mouseDownPos, mouseLocalPos);
                else
                    Logic.MouseMovePlayer(gameData, m_mouseDownPos, mouseLocalPos);

                InputCircleIn.transform.localPosition = (mouseLocalPos - m_mouseDownPos).normalized * magnitude * ((1.0f - InputCircleIn.transform.localScale.x) / 2.0f);
            }

            if (mouseUp)
            {
                InputCircleOut.SetActive(false);

                if (gameData.InCar)
                    Logic.MouseUpCar(gameData);
                else
                    Logic.MouseUpPlayer(gameData);
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
                    m_ammoPool.ShowAmmo(ammoIndex, spriteType, gameData.AmmoPosition[ammoIndex]);
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