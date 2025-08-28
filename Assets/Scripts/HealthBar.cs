using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Sprite[] Sprites;
    public SpriteRenderer SpriteRenderer;

    public void Show(float value)
    {
        int index = Mathf.FloorToInt(value * Sprites.Length);
        if (index > Sprites.Length)
            index = Sprites.Length - 1;
        SpriteRenderer.sprite = Sprites[index];
    }
}
