using Microsoft.Xna.Framework;
using System;

namespace AshesOfTheEarth.Graphics.Animation
{
    public class AnimationController
    {
        private AnimationData _currentAnimation;
        private float _timer;
        private int _currentFrameIndex;
        private bool _isPlaying;
        private bool _animationFinished;

        public int CurrentSpriteSheetFrameIndex
        {
            get
            {
                if (_currentAnimation == null || _currentAnimation.Frames.Count == 0) return 0;
                int safeIndex = Math.Clamp(_currentFrameIndex, 0, _currentAnimation.Frames.Count - 1);
                return _currentAnimation.Frames[safeIndex].FrameIndex;
            }
        }

        // NOU: Pentru a ști pe ce frame din secvența animației suntem (0, 1, 2...)
        public int CurrentFrameIndexWithinAnimation => _currentFrameIndex;


        public bool IsPlaying => _isPlaying && !IsPausedOnFrame; // Modificat
        public bool AnimationFinished => _animationFinished;
        public string CurrentAnimationName => _currentAnimation?.Name;

        public AnimationData CurrentAnimationData => _currentAnimation; // NOU

        public bool IsPausedOnFrame { get; private set; } = false; // NOU


        public void Play(AnimationData animation)
        {
            if (animation == null)
            {
                System.Diagnostics.Debug.WriteLine("Warning: Attempted to play a null animation.");
                Stop();
                return;
            }

            if (_currentAnimation == animation && _isPlaying && (animation.IsLooping || !_animationFinished))
            {
                return;
            }

            _currentAnimation = animation;
            _currentFrameIndex = 0;
            _timer = 0f;
            _isPlaying = (_currentAnimation.Frames.Count > 0);
            _animationFinished = false;
            IsPausedOnFrame = false; // Resetează la play
            //System.Diagnostics.Debug.WriteLine($"[AnimCtrl] Play: STARTED '{animation.Name}'. Duration: {animation.TotalDuration()}. Frames: {animation.Frames.Count}. IsLooping: {animation.IsLooping}");

            if (!_isPlaying && _currentAnimation.Frames.Count > 0)
            {
                //System.Diagnostics.Debug.WriteLine($"Warning: Animation '{_currentAnimation.Name}' has frames but failed to start playing.");
            }
        }

        public void Stop()
        {
            _isPlaying = false;
            _animationFinished = true;
            IsPausedOnFrame = false;
        }

        public void PauseOnCurrentFrame()
        {
            if (_isPlaying || (!_isPlaying && !_animationFinished)) // Poate fi pauzată dacă rulează sau dacă s-a oprit dar nu e finished (caz rar)
            {
                IsPausedOnFrame = true;
                _isPlaying = false; // Oprește procesarea în Update
            }
        }

        public void Resume()
        {
            if (IsPausedOnFrame)
            {
                IsPausedOnFrame = false;
                if (!_animationFinished) // Reluăm doar dacă animația nu era deja marcată ca terminată
                {
                    _isPlaying = true;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!_isPlaying || IsPausedOnFrame || _currentAnimation == null || _currentAnimation.Frames.Count == 0 || _animationFinished)
            {
                return;
            }

            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            float currentFrameDuration = _currentAnimation.Frames[_currentFrameIndex].Duration;

            if (currentFrameDuration <= 0) currentFrameDuration = 0.01f;

            while (_timer >= currentFrameDuration)
            {
                _timer -= currentFrameDuration;
                _currentFrameIndex++;

                if (_currentFrameIndex >= _currentAnimation.Frames.Count)
                {
                    if (_currentAnimation.IsLooping)
                    {
                        _currentFrameIndex = 0;
                    }
                    else
                    {
                        _currentFrameIndex = _currentAnimation.Frames.Count - 1;
                        _isPlaying = false;
                        _animationFinished = true;
                        break;
                    }
                }

                if (_currentFrameIndex < _currentAnimation.Frames.Count)
                {
                    currentFrameDuration = _currentAnimation.Frames[_currentFrameIndex].Duration;
                    if (currentFrameDuration <= 0) currentFrameDuration = 0.01f;
                }
                else
                {
                    currentFrameDuration = _currentAnimation.Frames[0].Duration;
                    if (currentFrameDuration <= 0) currentFrameDuration = 0.01f;
                }
            }
        }
    }
}     