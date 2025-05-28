using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Core.Time;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AshesOfTheEarth.Gameplay.Lighting
{
    public class LightSystem : ITimeObserver
    {
        private readonly EntityManager _entityManager;
        private readonly TimeManager _timeManager;
        private List<Entity> _lightEmitters;
        private Random _random = new Random();

        public float GlobalAmbientLight { get; private set; } = 1.0f;

        public LightSystem()
        {
            _entityManager = ServiceLocator.Get<EntityManager>();
            _timeManager = ServiceLocator.Get<TimeManager>();
            _lightEmitters = new List<Entity>();

            if (_timeManager != null)
            {
                _timeManager.Subscribe(this);
                UpdateGlobalAmbientLight();
            }
        }

        public void Update(GameTime gameTime)
        {
            _lightEmitters = _entityManager.GetAllEntitiesWithComponents<LightEmitterComponent, TransformComponent>().ToList();

            foreach (var emitterEntity in _lightEmitters)
            {
                var lightComp = emitterEntity.GetComponent<LightEmitterComponent>();
                if (lightComp.IsActive && lightComp.FlickerIntensity > 0)
                {
                    lightComp.CurrentFlickerOffset = (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * lightComp.FlickerSpeed + emitterEntity.Id) * lightComp.FlickerIntensity);
                }
                else
                {
                    lightComp.CurrentFlickerOffset = 0f;
                }
            }
        }

        public IEnumerable<Entity> GetActiveLightEmitters()
        {
            return _lightEmitters.Where(e => e.GetComponent<LightEmitterComponent>().IsActive);
        }

        private void UpdateGlobalAmbientLight()
        {
            if (_timeManager == null) return;
            GlobalAmbientLight = _timeManager.GetDaylightFactor();
        }

        public float GetEffectiveLightAtPosition(Vector2 worldPosition)
        {
            float baseLight = GlobalAmbientLight;
            float maxEmitterLight = 0f;

            foreach (var emitterEntity in GetActiveLightEmitters())
            {
                var lightComp = emitterEntity.GetComponent<LightEmitterComponent>();
                var transformComp = emitterEntity.GetComponent<TransformComponent>();

                float distanceSq = Vector2.DistanceSquared(transformComp.Position, worldPosition);
                float radiusSq = lightComp.LightRadius * lightComp.LightRadius;

                if (distanceSq < radiusSq)
                {
                    float distanceFactor = 1.0f - (float)Math.Sqrt(distanceSq) / lightComp.LightRadius;
                    float currentIntensity = lightComp.LightIntensity * distanceFactor * distanceFactor;
                    currentIntensity += lightComp.CurrentFlickerOffset;
                    currentIntensity = MathHelper.Clamp(currentIntensity, 0f, 1f);

                    if (currentIntensity > maxEmitterLight)
                    {
                        maxEmitterLight = currentIntensity;
                    }
                }
            }
            return MathHelper.Clamp(baseLight + maxEmitterLight, 0f, 1f);
        }

        public bool IsPositionInDarkness(Vector2 worldPosition, float darknessThreshold = 0.15f)
        {
            return GetEffectiveLightAtPosition(worldPosition) < darknessThreshold;
        }

        public void OnTimeChanged(TimeManager timeManager)
        {
            UpdateGlobalAmbientLight();
        }

        public void OnDayPhaseChanged(DayPhase newPhase)
        {
            UpdateGlobalAmbientLight();
        }

        public void OnHourElapsed(int hour) { }
    }
}