using System.Collections;
using System.Collections.Generic;
using Survivor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Sprite[] SpriteFrames;
    public SpriteRenderer SpriteRenderer;
    public int NumDirections;
    [Header("Animation")]
    public int NumAnimFrames = 1;
    public float FrameTime;

    void Awake()
    {
    }

    public void UpdateFrame(PlayerAnimationData playerAnimationData)
    {
        int frameIndex = playerAnimationData.DirectionIndex * playerAnimationData.SpriteAnimationData.NumFrames + playerAnimationData.SpriteAnimationData.FrameIndex;
        SpriteRenderer.sprite = SpriteFrames[frameIndex];
        playerAnimationData.SpriteAnimationData.FrameChanged = false;
    }
}
