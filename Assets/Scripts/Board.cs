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
        GameObject m_player;
        public Transform SpriteParent;

        GameObject[] m_enemyPool;
        Camera m_mainCamera;
        Vector2 m_mouseDownPos;

        BoardGUI m_boardGUI;

        public GameObject InputCircleOut;
        public GameObject InputCircleIn;

        GameData gameData;
        MetaData metaData;
        Balance balance;

        public void Init(MetaData metaData, GameData gameData, Balance balance, Camera mainCamera)
        {
            m_mainCamera = mainCamera;

            this.metaData = metaData;
            this.gameData = gameData;
            this.balance = balance;

            m_player = AssetManager.Instance.GetPlayerGameObject(SpriteParent);
            m_player.transform.localPosition = Vector2.zero;

            m_enemyPool = new GameObject[balance.NumEnemies];
            for (int i = 0; i < balance.NumEnemies; i++)
            {
                m_enemyPool[i] = AssetManager.Instance.GetEnemyGameObject(SpriteParent);
                m_enemyPool[i].SetActive(false);
            }

            m_boardGUI = new BoardGUI();
            m_boardGUI.UI = AssetManager.Instance.GetInGameUI();

            GUIRef guiRef = m_boardGUI.UI.GetComponent<GUIRef>();
            m_boardGUI.GameTimeText = guiRef.GetTextGUI("GameTime");
            guiRef.GetButton("Pause").onClick.AddListener(pauseGame);

            m_player.SetActive(false);
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
                m_enemyPool[i].SetActive(true);
            }
            m_player.SetActive(true);

            m_boardGUI.UI.SetActive(true);
        }

        public void Hide()
        {
            for (int i = 0; i < balance.NumEnemies; i++)
                m_enemyPool[i].SetActive(false);
            m_player.SetActive(false);

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

            for (int i = 0; i < balance.NumEnemies; i++)
                m_enemyPool[i].transform.localPosition = gameData.EnemyPosition[i];

            m_boardGUI.GameTimeText.text = CommonVisual.GetTimeElapsedString(gameData.GameTime);

            if (isGameOver)
                gameOver();
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