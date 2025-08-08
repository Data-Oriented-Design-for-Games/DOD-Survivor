using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Survivor
{
    public class TireTrackPool
    {
        int m_maxCount;
        GameObject[] m_poolGO;
        SpriteRenderer[] m_poolSR;
        float[] m_poolAlpha;
        Color[] m_poolColor;
        int m_index;

        Transform spriteParent;

        GameData gameData;

        public void Init(GameData gameData, int maxCount, Transform spriteParent, string tireMarkName)
        {
            m_maxCount = maxCount;
            m_poolGO = new GameObject[maxCount];
            m_poolSR = new SpriteRenderer[maxCount];
            m_poolAlpha = new float[maxCount];
            m_poolColor = new Color[maxCount];
            for (int i = 0; i < maxCount; i++)
            {
                m_poolGO[i] = AssetManager.Instance.GetTireMark(tireMarkName, spriteParent);
                m_poolSR[i] = m_poolGO[i].GetComponentInChildren<SpriteRenderer>();
            }

            Clear();

            m_index = 0;
            this.gameData = gameData;
            this.spriteParent = spriteParent;
        }

        public void Clear()
        {
            for (int i = 0; i < m_maxCount; i++)
            {
                m_poolAlpha[i] = 0.0f;
                m_poolColor[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                m_poolSR[i].color = m_poolColor[m_index];
            }
        }

        public void ShowTireTrack(Vector2 position, Color color)
        {
            m_poolGO[m_index].transform.localPosition = position;
            m_poolColor[m_index] = color;
            m_poolSR[m_index].color = m_poolColor[m_index];
            m_index = (m_index + 1) % m_maxCount;
        }

        public void Tick()
        {
            for (int i = 0; i < m_maxCount; i++)
            {
                m_poolGO[i].transform.localPosition = gameData.SkidMarkPos[i];
                m_poolSR[i].color = gameData.SkidMarkColor[i];
            }
        }
    }
}