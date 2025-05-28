using System;
using System.Collections.Generic;
using System.Linq;

namespace AshesOfTheEarth.Graphics.Animation
{
    public class AnimationData
    {
        public string Name { get; private set; }
        public List<AnimationFrame> Frames { get; private set; }
        public bool IsLooping { get; private set; }
        public float TotalDuration { get; private set; }

        public AnimationData(string name, List<AnimationFrame> frames, bool isLooping)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Frames = frames ?? throw new ArgumentNullException(nameof(frames));
            if (Frames.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: AnimationData '{name}' created with zero frames.");
            }
            IsLooping = isLooping;
            TotalDuration = Frames.Sum(f => f.Duration > 0 ? f.Duration : 0.01f);
        }

    }
}