using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor
{
    public static class CommonVisual
    {
        public static void AnimateSprite(float dt, ref SpriteAnimationData spriteAnimationData)
        {
            spriteAnimationData.FrameTimeLeft -= dt;
            if (spriteAnimationData.FrameTimeLeft <= 0.0f)
            {
                spriteAnimationData.FrameIndex = (spriteAnimationData.FrameIndex + 1) % spriteAnimationData.NumFrames;
                spriteAnimationData.FrameTimeLeft += spriteAnimationData.FrameTime;
                spriteAnimationData.FrameChanged = true;
            }
        }

        public static void TryChangeSpriteFrame(ref SpriteAnimationData spriteAnimationData, AnimatedSprite animatedSprite)
        {
            if (spriteAnimationData.FrameChanged)
            {
                animatedSprite.SetSpriteFrame(spriteAnimationData.FrameIndex);
                spriteAnimationData.FrameChanged = false;
            }
        }

        public static void InitSpriteFrameData(ref SpriteAnimationData spriteAnimationData, AnimatedSprite animatedSprite)
        {
            spriteAnimationData.FrameIndex = 0;
            spriteAnimationData.FrameTimeLeft = animatedSprite.FrameTime;
            spriteAnimationData.FrameTime = animatedSprite.FrameTime;
            spriteAnimationData.NumFrames = animatedSprite.Sprites.Length;
            spriteAnimationData.FrameChanged = false;
            animatedSprite.SetSpriteFrame(spriteAnimationData.FrameIndex);
        }

        public static void InitPlayerFrameData(PlayerAnimationData playerAnimationData, Player player)
        {
            playerAnimationData.SpriteAnimationData.FrameIndex = 0;
            playerAnimationData.SpriteAnimationData.FrameTimeLeft = player.FrameTime;
            playerAnimationData.SpriteAnimationData.FrameTime = player.FrameTime;
            playerAnimationData.SpriteAnimationData.NumFrames = player.NumAnimFrames;

            playerAnimationData.DirectionIndex = 0;
            playerAnimationData.NumDirections = player.NumDirections;
            playerAnimationData.Angle = 360.0f / (float)playerAnimationData.NumDirections;
        }

        public static string GetTimeElapsedString(float time)
        {
            string timeString = "";
            int m = Mathf.FloorToInt(time / 60.0f);
            int s = Mathf.FloorToInt(time - m * 60.0f);
            if (m >= 10)
                timeString += m;
            else
                timeString += "0" + m;
            timeString += ":";
            if (s >= 10)
                timeString += s;
            else
                timeString += "0" + s;

            return timeString;
        }
    }
}