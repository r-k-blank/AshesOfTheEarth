using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Graphics.Animation;
using AshesOfTheEarth.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using AshesOfTheEarth.Core.Mediator;
using System.Linq;
using AshesOfTheEarth.Entities.Mobs.AI;
using AshesOfTheEarth.Utils;
using System.Collections.Generic;
using AshesOfTheEarth.Gameplay.Items;
using AshesOfTheEarth.Graphics;
using AshesOfTheEarth.Gameplay.Systems;

namespace AshesOfTheEarth.Gameplay
{
    public class CombatSystem
    {
        private readonly EntityManager _entityManager;
        private IGameplayMediator _gameplayMediator;
        private Random _random = new Random();
        private Texture2D _pixelTexture;

        public CombatSystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        private void EnsureMediator()
        {
            if (_gameplayMediator == null)
                _gameplayMediator = ServiceLocator.Get<IGameplayMediator>();
            if (_pixelTexture == null)
                _pixelTexture = ServiceLocator.Get<Texture2D>();
        }

        public void ProcessPlayerAttack(Entity playerEntity, GameTime gameTime, object payload = null)
        {
            EnsureMediator();
            var playerTransform = playerEntity.GetComponent<TransformComponent>();
            var playerInventory = playerEntity.GetComponent<InventoryComponent>();
            var animationComp = playerEntity.GetComponent<AnimationComponent>();

            if (playerTransform == null || playerInventory == null || animationComp == null)
            {
                return;
            }

            string attackDirection = "Down";
            float attackRange = 100f;

            if (payload is Dictionary<string, object> attackDetails)
            {
                if (attackDetails.TryGetValue("AttackDirection", out var dirObj) && dirObj is string dirStr)
                    attackDirection = dirStr;
                if (attackDetails.TryGetValue("AttackRange", out var rangeObj) && rangeObj is float rangeVal)
                    attackRange = rangeVal;
            }

            float baseDamage = 5f;
            var equippedItem = playerInventory.Items
                .FirstOrDefault(stack => stack.Type != ItemType.None && stack.Data != null &&
                                        (stack.Data.Category == ItemCategory.Weapon || (stack.Data.Category == ItemCategory.Tool && stack.Data.Damage > 0)));

            if (equippedItem != null && equippedItem.Data.Damage > 0)
            {
                baseDamage = equippedItem.Data.Damage;
            }

            Rectangle playerAttackHitbox = CalculatePlayerAttackHitbox(playerTransform, attackDirection, attackRange, animationComp);

            var potentialTargets = _entityManager.GetEntitiesInBounds(playerAttackHitbox);
            var mobsInRange = potentialTargets.Where(e => e.Tag != "Player" &&
                                                    e.GetComponent<HealthComponent>()?.IsDead == false &&
                                                    e.HasComponent<AIComponent>());


            int targetsHitThisSwing = 0;
            foreach (var mob in mobsInRange)
            {
                var mobCollider = mob.GetComponent<ColliderComponent>();
                var mobTransform = mob.GetComponent<TransformComponent>();

                if (mobCollider.GetWorldBounds(mobTransform).Intersects(playerAttackHitbox))
                {
                    targetsHitThisSwing++;
                    var mobHealth = mob.GetComponent<HealthComponent>();
                    var mobAI = mob.GetComponent<AIComponent>();

                    mobHealth.TakeDamage(baseDamage);

                    _gameplayMediator.Notify(this, GameplayEvent.EntityDamaged, mob, baseDamage, gameTime);

                    if (!mobHealth.IsDead)
                    {
                        mobAI.Target = playerEntity;
                        if (mobAI.CurrentState != AIState.Attacking && mobAI.CurrentState != AIState.SpecialAction && mobAI.CurrentState != AIState.Hurt)
                        {
                            mobAI.CurrentState = AIState.Hurt;
                            mobAI.StateTimer = 0f;
                            string hurtAnimName = "Hurt";
                            if (mobHealth.CurrentHealth < mobHealth.MaxHealth * 0.25f && mob.GetComponent<AnimationComponent>().Animations.ContainsKey("Fall"))
                            {
                                hurtAnimName = "Fall";
                            }
                            mob.GetComponent<AnimationComponent>().PlayAnimation(hurtAnimName);
                        }
                    }
                }
            }
            if (targetsHitThisSwing == 0)
            {
            }
        }

        public void Update(GameTime gameTime)
        {
            EnsureMediator();
            Entity player = _entityManager.GetEntityByTag("Player");
            if (player == null || !player.HasComponent<HealthComponent>() || player.GetComponent<HealthComponent>().IsDead) return;

            var playerCollider = player.GetComponent<ColliderComponent>();
            var playerTransform = player.GetComponent<TransformComponent>();
            var playerHealth = player.GetComponent<HealthComponent>();

            if (playerCollider == null || playerTransform == null || playerHealth == null) return;

            var mobsReadyToAttack = _entityManager.GetAllEntitiesWithComponents<AIComponent, TransformComponent, MobStatsComponent, AnimationComponent, SpriteComponent, HealthComponent>()
                                 .Where(m => m.GetComponent<AIComponent>().CurrentState == AIState.Attacking &&
                                             !m.GetComponent<HealthComponent>().IsDead);

            foreach (var mob in mobsReadyToAttack)
            {
                var mobAI = mob.GetComponent<AIComponent>();
                var mobAnim = mob.GetComponent<AnimationComponent>();
                var mobStats = mob.GetComponent<MobStatsComponent>();
                var mobTransform = mob.GetComponent<TransformComponent>();
                var mobSprite = mob.GetComponent<SpriteComponent>();

                if (mobAnim.Controller.IsPlaying &&
                    mobAnim.Controller.CurrentAnimationData != null &&
                    mobAnim.Controller.CurrentAnimationData.Name.Contains("Attack"))
                {
                    AnimationData currentMobAnimData = mobAnim.Controller.CurrentAnimationData;
                    float animTotalDuration = Graphics.Animation.AnimationDataExtensions.TotalDuration(currentMobAnimData);

                    float hitMomentPercentage = 0.5f;
                    float hitWindowStartOffset = 0.1f;
                    float hitWindowEndOffset = 0.15f;

                    float hitWindowStartTime = Math.Max(0, animTotalDuration * (hitMomentPercentage - hitWindowStartOffset));
                    float hitWindowEndTime = Math.Min(animTotalDuration, animTotalDuration * (hitMomentPercentage + hitWindowEndOffset));

                    if (!mobAI.DamageAppliedThisAttackCycle &&
                        mobAI.StateTimer >= hitWindowStartTime &&
                        mobAI.StateTimer <= hitWindowEndTime)
                    {
                        Rectangle mobAttackBox = CalculateMobAttackHitbox(mobTransform, mobStats, mobSprite, mobAnim);
                        var potentialTargets = _entityManager.GetEntitiesInBounds(mobAttackBox);
                        Entity playerCandidate = potentialTargets.FirstOrDefault(e => e.Id == player.Id);

                        if (playerCandidate != null &&
                            ServiceLocator.Get<AISystem>().IsPlayerInAttackCone(mobTransform, playerTransform, mobAI.FacingDirection, mobStats.AttackConeAngleDegrees) &&
                            playerCollider.GetWorldBounds(playerTransform).Intersects(mobAttackBox))
                        {
                            playerHealth.TakeDamage(mobStats.Damage);
                            _gameplayMediator.Notify(this, GameplayEvent.PlayerDamaged, player, mobStats.Damage, gameTime);
                            mobAI.DamageAppliedThisAttackCycle = true;
                        }
                    }
                }
            }
        }

        private Rectangle CalculatePlayerAttackHitbox(TransformComponent playerTransform, string direction, float range, AnimationComponent playerAnim)
        {
            float baseSpriteHeight = 64f;
            if (playerAnim?.SpriteSheet != null)
            {
                baseSpriteHeight = playerAnim.SpriteSheet.FrameHeight * playerTransform.Scale.Y;
            }

            int hitboxWidth = (int)(range * 0.8f);
            int hitboxHeight = (int)(baseSpriteHeight * 0.7f);
            Point offset = Point.Zero;

            int verticalOffset = -(int)(hitboxHeight * 0.5f) - (int)(baseSpriteHeight * 0.1f);

            switch (direction.ToUpperInvariant())
            {
                case "UP":
                    offset = new Point(-hitboxWidth / 2, -(int)(range * 0.6f) - hitboxHeight / 2);
                    break;
                case "DOWN":
                    offset = new Point(-hitboxWidth / 2, (int)(range * 0.5f) - hitboxHeight / 2);
                    break;
                case "LEFT":
                    offset = new Point(-(int)(range * 0.7f), verticalOffset);
                    break;
                case "RIGHT":
                    offset = new Point((int)(range * 0.5f), verticalOffset);
                    break;
                default:
                    offset = new Point((int)(range * 0.5f), verticalOffset);
                    break;
            }
            return new Rectangle((int)playerTransform.Position.X + offset.X, (int)playerTransform.Position.Y + offset.Y, hitboxWidth, hitboxHeight);
        }

        private Rectangle CalculateMobAttackHitbox(TransformComponent mobTransform, MobStatsComponent mobStats, SpriteComponent mobSprite, AnimationComponent mobAnim)
        {
            float baseSpriteHeight = 64f;
            if (mobAnim?.SpriteSheet != null)
            {
                baseSpriteHeight = mobAnim.SpriteSheet.FrameHeight * mobTransform.Scale.Y;
            }

            int hitboxWidth = (int)(mobStats.AttackRange * 0.7f);
            int hitboxHeight = (int)(baseSpriteHeight * 0.8f);
            Point offset = Point.Zero;

            int verticalOffset = -(int)(hitboxHeight * 0.5f) - (int)(baseSpriteHeight * 0.1f);

            if (mobSprite.Effects == SpriteEffects.FlipHorizontally)
            {
                offset = new Point(-(int)(mobStats.AttackRange * 0.6f), verticalOffset);
            }
            else
            {
                offset = new Point((int)(mobStats.AttackRange * 0.1f), verticalOffset);
            }
            return new Rectangle((int)mobTransform.Position.X + offset.X, (int)mobTransform.Position.Y + offset.Y, hitboxWidth, hitboxHeight);
        }
    }
}