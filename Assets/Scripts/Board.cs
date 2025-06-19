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

    public class Board : MonoBehaviour
    {
        public Transform SpriteParent;

        AnimatedSprite m_player;
        AnimatedSprite[] m_enemyPool;
        AnimatedSprite[] m_ammoPool;

        Camera m_mainCamera;
        Vector2 m_mouseDownPos;

        BoardGUI m_boardGUI;

        public GameObject InputCircleOut;
        public GameObject InputCircleIn;

        GameData gameData;
        MetaData metaData;
        Balance balance;

        public struct SpriteAnimationData
        {
            public int FrameIndex;
            public int NumFrames;
            public float FrameTimeLeft;
            public float FrameTime;
            public bool FrameChanged;
        }

        public class VisualBoardData
        {
            public SpriteAnimationData PlayerSpriteAnimData;
            public SpriteAnimationData[] EnemySpriteAnimData;
            public SpriteAnimationData[] AmmoSpriteAnimData;
        }

        VisualBoardData m_visualBoardData = new VisualBoardData();

        public void Init(MetaData metaData, GameData gameData, Balance balance, Camera mainCamera)
        {
            m_mainCamera = mainCamera;

            this.metaData = metaData;
            this.gameData = gameData;
            this.balance = balance;

            // player
            m_player = AssetManager.Instance.GetPlayer(SpriteParent);
            m_player.transform.localPosition = Vector2.zero;

            m_visualBoardData.PlayerSpriteAnimData.FrameIndex = 0;
            m_visualBoardData.PlayerSpriteAnimData.FrameTimeLeft = m_player.FrameTime;
            m_visualBoardData.PlayerSpriteAnimData.FrameTime = m_player.FrameTime;
            m_visualBoardData.PlayerSpriteAnimData.NumFrames = m_player.Sprites.Length;

            // enemies
            m_enemyPool = new AnimatedSprite[balance.NumEnemies];
            for (int i = 0; i < balance.NumEnemies; i++)
            {
                m_enemyPool[i] = AssetManager.Instance.GetEnemy(SpriteParent);
                m_enemyPool[i].gameObject.SetActive(false);
            }

            m_visualBoardData.EnemySpriteAnimData = new SpriteAnimationData[balance.NumEnemies];
            for (int i = 0; i < balance.NumEnemies; i++)
            {
                m_visualBoardData.EnemySpriteAnimData[i].FrameIndex = 0;
                m_visualBoardData.EnemySpriteAnimData[i].FrameTimeLeft = m_enemyPool[i].FrameTime;
                m_visualBoardData.EnemySpriteAnimData[i].FrameTime = m_enemyPool[i].FrameTime;
                m_visualBoardData.EnemySpriteAnimData[i].NumFrames = m_enemyPool[i].Sprites.Length;
            }

            // ammo
            m_ammoPool = new AnimatedSprite[balance.NumAmmo];
            for (int i = 0; i < balance.NumAmmo; i++)
            {
                m_ammoPool[i] = AssetManager.Instance.GetAmmo(SpriteParent);
                m_ammoPool[i].gameObject.SetActive(false);
            }

            m_visualBoardData.AmmoSpriteAnimData = new SpriteAnimationData[balance.NumAmmo];
            for (int i = 0; i < balance.NumAmmo; i++)
            {
                m_visualBoardData.AmmoSpriteAnimData[i].FrameIndex = 0;
                m_visualBoardData.AmmoSpriteAnimData[i].FrameTimeLeft = m_ammoPool[i].FrameTime;
                m_visualBoardData.AmmoSpriteAnimData[i].FrameTime = m_ammoPool[i].FrameTime;
                m_visualBoardData.AmmoSpriteAnimData[i].NumFrames = m_ammoPool[i].Sprites.Length;
            }

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
            for (int i = 0; i < balance.NumEnemies; i++)
            {
                m_enemyPool[i].transform.localPosition = gameData.EnemyPosition[i];
                m_enemyPool[i].gameObject.SetActive(true);
                m_visualBoardData.EnemySpriteAnimData[i].FrameIndex = Mathf.FloorToInt(Random.value * m_visualBoardData.EnemySpriteAnimData[i].NumFrames);
            }

            for (int i = 0; i < balance.NumAmmo; i++)
            {
                m_ammoPool[i].gameObject.SetActive(false);
                m_visualBoardData.AmmoSpriteAnimData[i].FrameIndex = Mathf.FloorToInt(Random.value * m_visualBoardData.AmmoSpriteAnimData[i].NumFrames);
            }

            m_player.gameObject.SetActive(true);

            m_boardGUI.UI.SetActive(true);
        }

        public void Hide()
        {
            for (int i = 0; i < balance.NumEnemies; i++)
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
            Logic.Tick(metaData, gameData, balance, dt, out isGameOver);

            updateVisuals(dt);

            if (isGameOver)
                gameOver();
        }

        private void updateVisuals(float dt)
        {
            m_visualBoardData.PlayerSpriteAnimData.FrameTimeLeft -= dt;
            if (m_visualBoardData.PlayerSpriteAnimData.FrameTimeLeft <= 0.0f)
            {
                m_visualBoardData.PlayerSpriteAnimData.FrameIndex = (m_visualBoardData.PlayerSpriteAnimData.FrameIndex + 1) % m_visualBoardData.PlayerSpriteAnimData.NumFrames;
                m_visualBoardData.PlayerSpriteAnimData.FrameTimeLeft += m_visualBoardData.PlayerSpriteAnimData.FrameTime;
                m_player.SetSpriteFrame(m_visualBoardData.PlayerSpriteAnimData.FrameIndex);
            }
            float playerScaleX = gameData.PlayerDirection.x > 0.0f ? 1.0f : -1.0f;
            m_player.transform.localScale = new Vector3(playerScaleX, 1.0f, 1.0f);


            for (int i = 0; i < balance.NumEnemies; i++)
            {
                m_enemyPool[i].transform.localPosition = gameData.EnemyPosition[i];
                float scaleX = m_enemyPool[i].transform.localPosition.x > 0.0f ? 1.0f : -1.0f;
                m_enemyPool[i].transform.localScale = new Vector3(scaleX, 1.0f, 1.0f);
            }

            for (int i = 0; i < balance.NumEnemies; i++)
            {
                m_visualBoardData.EnemySpriteAnimData[i].FrameTimeLeft -= dt;
                if (m_visualBoardData.EnemySpriteAnimData[i].FrameTimeLeft <= 0.0f)
                {
                    m_visualBoardData.EnemySpriteAnimData[i].FrameIndex = (m_visualBoardData.EnemySpriteAnimData[i].FrameIndex + 1) % m_visualBoardData.EnemySpriteAnimData[i].NumFrames;
                    m_visualBoardData.EnemySpriteAnimData[i].FrameTimeLeft += m_visualBoardData.EnemySpriteAnimData[i].FrameTime;
                    m_visualBoardData.EnemySpriteAnimData[i].FrameChanged = true;
                }
            }
            for (int i = 0; i < balance.NumEnemies; i++)
            {
                if (m_visualBoardData.EnemySpriteAnimData[i].FrameChanged)
                {
                    m_enemyPool[i].SetSpriteFrame(m_visualBoardData.EnemySpriteAnimData[i].FrameIndex);
                    m_visualBoardData.EnemySpriteAnimData[i].FrameChanged = false;
                }
            }

            for (int i = 0; i < balance.NumEnemies; i++)
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