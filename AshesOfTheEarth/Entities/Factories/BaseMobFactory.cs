using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using AshesOfTheEarth.Graphics.Animation;
using System.Collections.Generic;
using System;

namespace AshesOfTheEarth.Entities.Factories.Mobs
{
    public abstract class BaseMobFactory : IEntityFactory
    {
        protected ContentManager _content;

        protected BaseMobFactory(ContentManager content)
        {
            _content = content;
        }

        public abstract Entity CreateEntity(Vector2 position);

        protected List<AnimationFrame> CreateMobFrames(SpriteSheet sheet, int startIndex, int count, float durationPerFrame)
        {
            var frames = new List<AnimationFrame>(count);
            for (int i = 0; i < count; i++)
            {
                if (startIndex + i < sheet.TotalFrames)
                {
                    frames.Add(new AnimationFrame(startIndex + i, durationPerFrame));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Warning: Frame index {startIndex + i} out of bounds for mob spritesheet (Total: {sheet.TotalFrames}).");
                    if (sheet.TotalFrames > 0 && frames.Count < count)
                        frames.Add(new AnimationFrame(0, durationPerFrame));
                    else if (frames.Count < count)
                        frames.Add(new AnimationFrame(0, durationPerFrame));
                }
            }
            if (count > 0 && frames.Count == 0 && sheet.TotalFrames > 0)
            {
                frames.Add(new AnimationFrame(0, durationPerFrame));
            }
            else if (count > 0 && frames.Count == 0)
            {
                frames.Add(new AnimationFrame(0, 1f));
            }
            return frames;
        }
    }
}