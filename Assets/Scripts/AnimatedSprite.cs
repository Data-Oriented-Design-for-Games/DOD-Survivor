using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedSprite : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] Sprites;
    public float FrameTime;

    public void SetSpriteFrame(int index)
    {
        spriteRenderer.sprite = Sprites[index];
    }
}
