using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace AshesOfTheEarth.Graphics.Animation
{
    public class SpriteSheet
    {
        public Texture2D Texture { get; private set; }
        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }
        private readonly int _columns;
        private readonly int _rows;
        private readonly Rectangle[] _sourceRectangles;

        public SpriteSheet(Texture2D texture, int frameWidth, int frameHeight)
        {
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            FrameWidth = frameWidth > 0 ? frameWidth : throw new ArgumentOutOfRangeException(nameof(frameWidth), "Frame width must be positive.");
            FrameHeight = frameHeight > 0 ? frameHeight : throw new ArgumentOutOfRangeException(nameof(frameHeight), "Frame height must be positive.");


            if (texture.Width % frameWidth != 0 || texture.Height % frameHeight != 0)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Texture dimensions ({texture.Width}x{texture.Height}) are not perfectly divisible by frame size ({frameWidth}x{frameHeight}).");
            }


            _columns = Texture.Width / FrameWidth;
            _rows = Texture.Height / FrameHeight;
            _sourceRectangles = new Rectangle[_columns * _rows];

            for (int y = 0; y < _rows; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    int index = y * _columns + x;
                    _sourceRectangles[index] = new Rectangle(x * FrameWidth, y * FrameHeight, FrameWidth, FrameHeight);
                }
            }
        }

        public Rectangle GetSourceRectangle(int frameIndex)
        {
            if (frameIndex < 0 || frameIndex >= _sourceRectangles.Length)
            {
                System.Diagnostics.Debug.WriteLine($"Error: Invalid frame index {frameIndex} requested for spritesheet with {TotalFrames} frames.");
                // Returnează primul cadru sau un dreptunghi gol ca fallback
                return _sourceRectangles.Length > 0 ? _sourceRectangles[0] : Rectangle.Empty;
            }
            return _sourceRectangles[frameIndex];
        }

        public int TotalFrames => _sourceRectangles.Length;
    }
}
