using AshesOfTheEarth.Core.Input;
using AshesOfTheEarth.Core.Services;
using Microsoft.Xna.Framework;
using AshesOfTheEarth.Graphics.Animation;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.World;
using System;
using System.Linq;
using AshesOfTheEarth.UI;
using AshesOfTheEarth.Core.Mediator;
using AshesOfTheEarth.Core.Time;
using System.Collections.Generic;
using AshesOfTheEarth.Gameplay.Items;
using AshesOfTheEarth.Gameplay.Lighting;

namespace AshesOfTheEarth.Entities.Components
{
    public class PlayerControllerComponent : IComponent
    {
        public float WalkSpeed { get; set; } = 120f;
        public float RunSpeedOffset { get; set; } = 80f;

        private InputManager _inputManager;
        private WorldManager _worldManager;
        private EntityManager _entityManager;
        private UIManager _uiManager;
        private IGameplayMediator _gameplayMediator;
        private TimeManager _timeManager;

        private Entity _playerEntity;
        private LightEmitterComponent _torchLightComponent;


        public bool IsAttacking { get; set; } = false;
        public float AttackCooldownTimer { get; set; } = 0f;
        public const float ATTACK_COOLDOWN_VALUE = 0.4f;

        public float InteractCooldownTimer { get; set; } = 0f;
        public const float INTERACT_COOLDOWN_VALUE = 0.3f;

        public bool IsInPlacementMode { get; set; } = false;
        public ItemType CurrentPlacingItemType { get; set; } = ItemType.None;
        public Vector2 PlacementPreviewPosition { get; set; }
        public bool IsCurrentPlacementValid { get; set; } = false;


        public PlayerControllerComponent()
        {
        }

        public void Initialize(Entity playerEntity)
        {
            _playerEntity = playerEntity;
            _inputManager = ServiceLocator.Get<InputManager>();
            _worldManager = ServiceLocator.Get<WorldManager>();
            _entityManager = ServiceLocator.Get<EntityManager>();
            _uiManager = ServiceLocator.Get<UIManager>();
            _gameplayMediator = ServiceLocator.Get<IGameplayMediator>();
            _timeManager = ServiceLocator.Get<TimeManager>();

            _torchLightComponent = new LightEmitterComponent(radius: 180f, intensity: 0.6f, color: new Color(255, 220, 150), isActive: false, flickerIntensity: 0.05f, flickerSpeed: 7f);
            _playerEntity.AddComponent(_torchLightComponent);
        }

        public void Update(GameTime gameTime)
        {
            if (_playerEntity == null)
            {
                _playerEntity = _entityManager.GetEntityByTag("Player");
                if (_playerEntity == null) return;
                Initialize(_playerEntity);
            }
            if (_inputManager == null) _inputManager = ServiceLocator.Get<InputManager>();


            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (AttackCooldownTimer > 0) AttackCooldownTimer -= deltaTime;
            if (InteractCooldownTimer > 0) InteractCooldownTimer -= deltaTime;

            UpdateTorchStatus();

            var command = _inputManager.HandleInputForPlayer(_playerEntity, gameTime);
            command?.Execute(_playerEntity, gameTime);
        }

        private void UpdateTorchStatus()
        {
            if (_playerEntity == null || _torchLightComponent == null) return;

            var inventory = _playerEntity.GetComponent<InventoryComponent>();
            if (inventory != null && inventory.HasItem(ItemType.Torch))
            {
                _torchLightComponent.IsActive = true;
            }
            else
            {
                _torchLightComponent.IsActive = false;
            }
        }


        public void InitiateAttack(GameTime gameTime)
        {
            if (_playerEntity == null) return;

            var animationComp = _playerEntity.GetComponent<AnimationComponent>();
            var sprite = _playerEntity.GetComponent<SpriteComponent>();
            var playerTransform = _playerEntity.GetComponent<TransformComponent>();

            if (animationComp == null || sprite == null || playerTransform == null)
            {
                IsAttacking = false;
                return;
            }

            IsAttacking = true;

            string attackAnimDirection = GetFacingDirectionFromAnimation(animationComp.Controller.CurrentAnimationName, sprite.Effects);
            string attackAnimName = "Attack_" + attackAnimDirection;

            if (animationComp.Animations.TryGetValue(attackAnimName, out AnimationData attackAnimData))
            {
                animationComp.PlayAnimation(attackAnimName);

                if (attackAnimDirection == "Left") sprite.Effects = SpriteEffects.FlipHorizontally;
                else if (attackAnimDirection == "Right") sprite.Effects = SpriteEffects.None;

                float attackDuration = AnimationDataExtensions.TotalDuration(attackAnimData);
                float hitMomentDelay = attackDuration * 0.4f;

                var currentMediator = _gameplayMediator;
                var entityToAttackWith = _playerEntity;

                _timeManager.SetTimeout(() =>
                {
                    if (entityToAttackWith != null && entityToAttackWith.GetComponent<PlayerControllerComponent>()?.IsAttacking == true)
                    {
                        var attackDetails = new Dictionary<string, object>
                        {
                            { "AttackDirection", attackAnimDirection },
                            { "AttackRange", 95f }
                        };
                        currentMediator?.Notify(this, GameplayEvent.PlayerAttackAttempt, entityToAttackWith, attackDetails, gameTime);
                    }
                }, TimeSpan.FromSeconds(hitMomentDelay));

                _timeManager.SetTimeout(() =>
                {
                    if (entityToAttackWith != null)
                    {
                        var controller = entityToAttackWith.GetComponent<PlayerControllerComponent>();
                        if (controller != null) controller.IsAttacking = false;
                    }
                }, TimeSpan.FromSeconds(attackDuration));
            }
            else
            {
                IsAttacking = false;
            }
        }

        public void InitiateInteraction(GameTime gameTime)
        {
            if (_playerEntity == null) return;
            _gameplayMediator?.Notify(this, GameplayEvent.PlayerInteractAttempt, _playerEntity, null, gameTime);
            InteractCooldownTimer = INTERACT_COOLDOWN_VALUE;
        }

        public bool CanMoveTo(Entity entity, Vector2 targetPosition)
        {
            var playerTransform = entity.GetComponent<TransformComponent>();
            var playerCollider = entity.GetComponent<ColliderComponent>();

            if (playerCollider == null || playerTransform == null || _worldManager == null)
            {
                return _worldManager?.IsPositionWalkable(targetPosition) ?? true;
            }

            if (!_worldManager.IsPositionWalkable(targetPosition))
            {
                return false;
            }

            Rectangle futurePlayerBounds = new Rectangle(
                (int)(targetPosition.X + playerCollider.Offset.X - playerCollider.Bounds.Width / 2f),
                (int)(targetPosition.Y + playerCollider.Offset.Y - playerCollider.Bounds.Height / 2f),
                playerCollider.Bounds.Width,
                playerCollider.Bounds.Height
            );

            var nearbySolidEntities = _entityManager.GetAllEntitiesWithComponents<TransformComponent, ColliderComponent>()
                                                 .Where(e => e != entity && e.GetComponent<ColliderComponent>().IsSolid);

            foreach (var solidEntity in nearbySolidEntities)
            {
                var solidTransform = solidEntity.GetComponent<TransformComponent>();
                var solidCollider = solidEntity.GetComponent<ColliderComponent>();
                if (futurePlayerBounds.Intersects(solidCollider.GetWorldBounds(solidTransform)))
                {
                    return false;
                }
            }
            return true;
        }

        public static string GetFacingDirectionFromAnimation(string animName, SpriteEffects currentEffects)
        {
            if (string.IsNullOrEmpty(animName)) return "Down";

            if (animName.Contains("_Up")) return "Up";
            if (animName.Contains("_Down")) return "Down";
            if (animName.Contains("_Left")) return "Left";
            if (animName.Contains("_Right")) return "Right";

            if (animName.Contains("Walk_") || animName.Contains("Run_") || animName.Contains("Idle_") || animName.Contains("Attack_"))
            {
                if (currentEffects == SpriteEffects.FlipHorizontally) return "Left";
                return "Right";
            }
            return "Down";
        }
    }
}