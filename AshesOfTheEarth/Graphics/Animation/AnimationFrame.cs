using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace AshesOfTheEarth.Graphics.Animation
{
    public class AnimationFrame
    {
        public int FrameIndex { get; private set; }
        public float Duration { get; private set; } // Seconds

        public AnimationFrame(int frameIndex, float duration)
        {
            FrameIndex = frameIndex;
            Duration = duration > 0 ? duration : 0.01f; // Asigură o durată minimă pozitivă
        }
    }
}
