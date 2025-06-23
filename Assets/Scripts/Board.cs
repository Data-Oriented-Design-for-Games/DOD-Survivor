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
        AnimatedSprite[] m_enemyPool;
        AnimatedSprite[] m_weaponPool;

        ExplosionPool m_explosionPool = new ExplosionPool();

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
            m_enemyPool = new AnimatedSprite[balance.MaxEnemies];
            for (int i = 0; i < balance.MaxEnemies; i++)
            {
                m_enemyPool[i] = AssetManager.Instance.GetEnemy(balance.EnemyBalance.SpriteName[gameData.EnemyType], SpriteParent);
                m_enemyPool[i].gameObject.SetActive(false);
            }

            m_visualBoardData.EnemySpriteAnimData = new SpriteAnimationData[balance.MaxEnemies];
            for (int i = 0; i < balance.MaxEnemies; i++)
                CommonVisual.InitSpriteFrameData(ref m_visualBoardData.EnemySpriteAnimData[i], m_enemyPool[i]);

            // ammo
            m_weaponPool = new AnimatedSprite[balance.MaxWeapons];
            for (int i = 0; i < balance.MaxWeapons; i++)
            {
                m_weaponPool[i] = AssetManager.Instance.GetWeapon(balance.WeaponBalance.SpriteName[gameData.WeaponType], SpriteParent);
                m_weaponPool[i].gameObject.SetActive(false);
            }

            m_visualBoardData.WeaponSpriteAnimData = new SpriteAnimationData[balance.MaxWeapons];
            for (int i = 0; i < balance.MaxWeapons; i++)
                CommonVisual.InitSpriteFrameData(ref m_visualBoardData.WeaponSpriteAnimData[i], m_weaponPool[i]);
                
            // particles
            m_explosionPool.Init(balance, SpriteParent);

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
            for (int i = 0; i < balance.MaxEnemies; i++)
            {
                m_enemyPool[i].transform.localPosition = gameData.EnemyPosition[i];
                m_enemyPool[i].gameObject.SetActive(true);
                m_visualBoardData.EnemySpriteAnimData[i].FrameIndex = Mathf.FloorToInt(UnityEngine.Random.value * m_visualBoardData.EnemySpriteAnimData[i].NumFrames);
                m_enemyPool[i].SetSpriteFrame(m_visualBoardData.EnemySpriteAnimData[i].FrameIndex);
            }

            for (int i = 0; i < balance.MaxWeapons; i++)
            {
                m_weaponPool[i].gameObject.SetActive(false);
                m_visualBoardData.WeaponSpriteAnimData[i].FrameIndex = Mathf.FloorToInt(UnityEngine.Random.value * m_visualBoardData.WeaponSpriteAnimData[i].NumFrames);
                m_weaponPool[i].SetSpriteFrame(m_visualBoardData.WeaponSpriteAnimData[i].FrameIndex);
            }

            m_player.gameObject.SetActive(true);

            m_boardGUI.UI.SetActive(true);
        }

        public void Hide()
        {
            for (int i = 0; i < balance.MaxEnemies; i++)
                m_enemyPool[i].gameObject.SetActive(false);
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
            Span<int> firedWeaponIdxs = stackalloc int[balance.MaxWeapons];
            int firedWeaponCount;
            Span<int> deadWeaponIdxs = stackalloc int[balance.MaxEnemies];
            int deadWeaponCount;
            Logic.Tick(metaData, gameData, balance, dt, firedWeaponIdxs, out firedWeaponCount, deadWeaponIdxs, out deadWeaponCount, out isGameOver);

            updateVisuals(firedWeaponIdxs, firedWeaponCount, deadWeaponIdxs, deadWeaponCount, dt);

            if (isGameOver)
                gameOver();
        }

        private void updateVisuals(
            Span<int> firedWeaponIdxs,
            int firedWeaponCount,
            Span<int> deadWeaponIdsx,
            int deadWeaponCount,
            float dt)
        {
            // player
            CommonVisual.AnimateSprite(dt, ref m_visualBoardData.PlayerSpriteAnimData);
            CommonVisual.TryChangeSpriteFrame(ref m_visualBoardData.PlayerSpriteAnimData, m_player);

            float playerScaleX = gameData.PlayerDirection.x > 0.0f ? 1.0f : -1.0f;
            m_player.transform.localScale = new Vector3(playerScaleX, 1.0f, 1.0f);

            // enemies
            for (int i = 0; i < balance.MaxEnemies; i++)
            {
                m_enemyPool[i].transform.localPosition = gameData.EnemyPosition[i];
                float scaleX = m_enemyPool[i].transform.localPosition.x < 0.0f ? 1.0f : -1.0f;
                m_enemyPool[i].transform.localScale = new Vector3(scaleX, 1.0f, 1.0f);
            }

            for (int i = 0; i < balance.MaxEnemies; i++)
                CommonVisual.AnimateSprite(dt, ref m_visualBoardData.EnemySpriteAnimData[i]);
            for (int i = 0; i < balance.MaxEnemies; i++)
                CommonVisual.TryChangeSpriteFrame(ref m_visualBoardData.EnemySpriteAnimData[i], m_enemyPool[i]);

            // weapons
            for (int i = 0; i < firedWeaponCount; i++)
            {
                int weaponIdx = firedWeaponIdxs[i];
                m_weaponPool[weaponIdx].gameObject.SetActive(true);
            }
            for (int i = 0; i < deadWeaponCount; i++)
            {
                int weaponIdx = deadWeaponIdsx[i];
                if (balance.WeaponBalance.ExplosionRadius[gameData.WeaponType] > 0.0f)
                {
                    m_explosionPool.ShowExplosion(gameData.WeaponType, m_weaponPool[weaponIdx].transform.localPosition);
                }
                m_weaponPool[weaponIdx].gameObject.SetActive(false);
            }
            m_explosionPool.Tick(dt);

            for (int i = 0; i < gameData.AliveWeaponCount; i++)
            {
                int weaponIdx = gameData.AliveWeaponIdx[i];
                CommonVisual.AnimateSprite(dt, ref m_visualBoardData.WeaponSpriteAnimData[weaponIdx]);
            }
            for (int i = 0; i < gameData.AliveWeaponCount; i++)
            {
                int weaponIdx = gameData.AliveWeaponIdx[i];
                m_weaponPool[weaponIdx].transform.localPosition = gameData.WeaponPosition[weaponIdx];
                m_weaponPool[weaponIdx].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, gameData.WeaponAngle[weaponIdx]);

                CommonVisual.TryChangeSpriteFrame(ref m_visualBoardData.WeaponSpriteAnimData[weaponIdx], m_weaponPool[weaponIdx]);
            }

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
                Span<int> firedWeaponIdxs = stackalloc int[balance.MaxWeapons];
                int firedWeaponCount = 0;

                Logic.TestFireWeapon(gameData, balance, (1.0f / 60.0f), firedWeaponIdxs, ref firedWeaponCount);
                for (int i = 0; i < firedWeaponCount; i++)
                {
                    int weaponIdx = firedWeaponIdxs[i];
                    m_weaponPool[weaponIdx].gameObject.SetActive(true);
                    int enemyIdx = gameData.WeaponTargetIdx[weaponIdx];
                    Debug.DrawLine(m_weaponPool[weaponIdx].transform.position, m_enemyPool[enemyIdx].transform.position, Color.white, 1.0f);
                }
            }
            if (Input.GetKeyUp(KeyCode.D))
            {
                for (int i = 0; i < gameData.AliveWeaponCount; i++)
                {
                    int weaponIdx = gameData.AliveWeaponIdx[i];
                    int enemyIdx = gameData.WeaponTargetIdx[weaponIdx];
                    Debug.Log("enemyIdx " + enemyIdx);
                    Debug.DrawLine(m_weaponPool[weaponIdx].transform.position, SpriteParent.TransformPoint(gameData.WeaponTargetPos[weaponIdx]), Color.white, 1.0f);
                    Debug.Log("draw line!");
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