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
                m_poolGO[i] = AssetManager.Instance.GetSkidMark(tireMarkName, spriteParent);
                m_poolGO[i].name = "SkidMark " + i.ToString();
                m_poolSR[i] = m_poolGO[i].GetComponentInChildren<SpriteRenderer>();
            }

            Clear();

            this.gameData = gameData;
            this.spriteParent = spriteParent;
        }

        public void Clear()
        {
            for (int i = 0; i < m_maxCount; i++)
            {
                m_poolAlpha[i] = 0.0f;
                m_poolColor[i] = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                m_poolSR[i].color = m_poolColor[i];
            }
        }

        public void ShowSkidMark(Vector2 position, Color color, float angle, int index)
        {
            // Debug.Log("ShowSkidMark " + m_poolGO[index].name + " angle " + angle);

            m_poolGO[index].transform.localPosition = position;
            m_poolColor[index] = color;
            m_poolSR[index].color = m_poolColor[index];

            m_poolGO[index].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, angle);
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