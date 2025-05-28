using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Lighting;
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Gameplay.Survival
{
    public class DarknessDamageSystem
    {
        private readonly EntityManager _entityManager;
        private readonly LightSystem _lightSystem;
        private Entity _player;
        private float _timeInDarkness = 0f;
        private const float MAX_TIME_IN_DARKNESS = 5f; // seconds
        private bool _playerFound = false;

        public DarknessDamageSystem()
        {
            _entityManager = ServiceLocator.Get<EntityManager>();
            _lightSystem = ServiceLocator.Get<LightSystem>();
        }

        private void FindPlayer()
        {
            if (!_playerFound || _player == null || _player.GetComponent<HealthComponent>()?.IsDead == true)
            {
                _player = _entityManager.GetEntityByTag("Player");
                _playerFound = _player != null;
                if (!_playerFound)
                {
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            FindPlayer();
            if (!_playerFound || _player == null || _lightSystem == null) return;

            var playerTransform = _player.GetComponent<TransformComponent>();
            var playerHealth = _player.GetComponent<HealthComponent>();

            if (playerTransform == null || playerHealth == null || playerHealth.IsDead) return;

            if (_lightSystem.IsPositionInDarkness(playerTransform.Position))
            {
                _timeInDarkness += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_timeInDarkness >= MAX_TIME_IN_DARKNESS)
                {
                    playerHealth.TakeDamage(playerHealth.MaxHealth + 1);
                }
            }
            else
            {
                _timeInDarkness = 0f;
            }
        }
    }
}