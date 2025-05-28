using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Entities.Mobs.AI;
using AshesOfTheEarth.Graphics.Animation;
using AshesOfTheEarth.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using AshesOfTheEarth.Core.Mediator;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Core.Time;
using AshesOfTheEarth.Entities;


namespace AshesOfTheEarth.Gameplay.Systems
{
    public class AISystem
    {
        private EntityManager _entityManager;
        private WorldManager _worldManager;
        private Random _random = new Random();
        private Entity _player;
        private IGameplayMediator _gameplayMediator;
        private TimeManager _timeManager;

        private List<Entity> _solidNonPlayerEntitiesCache = new List<Entity>();

        public AISystem(EntityManager entityManager, WorldManager worldManager)
        {
            _entityManager = entityManager;
            _worldManager = worldManager;
            _gameplayMediator = ServiceLocator.Get<IGameplayMediator>();
            _timeManager = ServiceLocator.Get<TimeManager>();
        }

        private void EnsurePlayerReference()
        {
            if (_player == null ||
                !_player.HasComponent<PlayerControllerComponent>() || // Ar trebui să fie ok
                _player.GetComponent<HealthComponent>()?.IsDead == true)
            {
                _player = _entityManager.GetAllEntities().FirstOrDefault(e =>
                e.HasComponent<PlayerControllerComponent>() && // <-- SCHIMBAT
                e.GetComponent<HealthComponent>()?.IsDead == false);
            }
        }
        private void EnsureMediatorAndTimemanager() // Poți crea o metodă helper dacă o folosești des
        {
            if (_gameplayMediator == null)
            {
                _gameplayMediator = ServiceLocator.Get<IGameplayMediator>();
                if (_gameplayMediator == null)
                {
                    System.Diagnostics.Debug.WriteLine("CRITICAL ERROR: GameplayMediator could not be retrieved from ServiceLocator in AISystem.");
                    // Poți arunca o excepție aici sau gestiona eroarea altfel,
                    // dar jocul probabil nu va funcționa corect fără mediator.
                }
            }
            if (_timeManager == null) // Asigură-te și de TimeManager dacă e folosit în apropiere
            {
                _timeManager = ServiceLocator.Get<TimeManager>();
                if (_timeManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("CRITICAL ERROR: TimeManager could not be retrieved from ServiceLocator in AISystem.");
                }
            }
        }
        public void InvalidatePlayerReference()
        {
            this._player = null;
            System.Diagnostics.Debug.WriteLine("[AISystem] Player reference invalidated. Will re-ensure on next update.");
        }
        public void Update(GameTime gameTime)
        {
            EnsurePlayerReference();
            var playerTransform = _player?.GetComponent<TransformComponent>();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _solidNonPlayerEntitiesCache = _entityManager.GetAllEntities()
                .Where(e => e.Tag != "Player" &&
                            e.HasComponent<ColliderComponent>() &&
                            e.GetComponent<ColliderComponent>().IsSolid)
                .ToList();

            var mobs = _entityManager.GetAllEntitiesWithComponents<AIComponent, TransformComponent, MobStatsComponent, AnimationComponent, HealthComponent, SpriteComponent>().ToList();

            foreach (var mob in mobs)
            {
                var ai = mob.GetComponent<AIComponent>();
                var transform = mob.GetComponent<TransformComponent>();
                var stats = mob.GetComponent<MobStatsComponent>();
                var anim = mob.GetComponent<AnimationComponent>();
                var health = mob.GetComponent<HealthComponent>();
                var sprite = mob.GetComponent<SpriteComponent>();

                ai.StateTimer += deltaTime;
                if (ai.AttackIntervalTimer > 0) ai.AttackIntervalTimer -= deltaTime;
                if (ai.ActionCooldown > 0) ai.ActionCooldown -= deltaTime;
                if (ai.AttackActionTimer > 0) ai.AttackActionTimer -= deltaTime;

                if (health.IsDead)
                {
                    HandleDeathState(mob, ai, anim, stats);
                    continue;
                }

                UpdateFacingDirection(mob, ai, transform, sprite, playerTransform);

                if (playerTransform != null && _player?.GetComponent<HealthComponent>()?.IsDead == false &&
                    ai.CurrentState != AIState.Chasing && ai.CurrentState != AIState.Attacking &&
                    ai.CurrentState != AIState.Fleeing && ai.CurrentState != AIState.Hurt &&
                    ai.CurrentState != AIState.SpecialAction)
                {
                    float distToPlayerSq = Vector2.DistanceSquared(transform.Position, playerTransform.Position);
                    bool isAggressive = !(mob.Tag.Contains("Deer") || mob.Tag.Contains("Rabbit"));

                    if (distToPlayerSq < stats.AggroRange * stats.AggroRange)
                    {
                        if (isAggressive)
                        {
                            ai.Target = _player;
                            ai.CurrentState = AIState.Chasing;
                            ai.StateTimer = 0f;
                            ai.CurrentChaseTimeWithoutAttack = 0f;
                        }
                        else if (ai.CurrentState != AIState.Fleeing)
                        {
                            ai.Target = _player;
                            ai.CurrentState = AIState.Fleeing;
                            ai.StateTimer = 0f;
                        }
                    }
                }

                switch (ai.CurrentState)
                {
                    case AIState.Idle:
                        HandleIdleState(mob, ai, anim);
                        break;
                    case AIState.Patrolling:
                        HandlePatrollingState(mob, ai, transform, anim, stats, deltaTime);
                        break;
                    case AIState.Chasing:
                        HandleChasingState(mob, ai, transform, anim, stats, playerTransform, deltaTime);
                        break;
                    case AIState.Attacking:
                        HandleAttackingState(mob, ai, anim, stats, playerTransform);
                        break;
                    case AIState.ReturningToPatrol:
                        HandleReturningToPatrolState(mob, ai, transform, anim, stats, deltaTime);
                        break;
                    case AIState.Fleeing:
                        HandleFleeingState(mob, ai, transform, anim, stats, playerTransform, deltaTime);
                        break;
                    case AIState.Hurt:
                        if (anim.Controller.AnimationFinished || ai.StateTimer > GetHurtAnimationDuration(anim))
                        {
                            if (ai.Target != null && !(mob.Tag.Contains("Deer") || mob.Tag.Contains("Rabbit")))
                            {
                                ai.CurrentState = AIState.Chasing;
                            }
                            else
                            {
                                ai.CurrentState = AIState.Idle;
                            }
                            ai.StateTimer = 0f;
                        }
                        break;
                    case AIState.SpecialAction:
                        if (anim.Controller.AnimationFinished || ai.StateTimer > 2.0f)
                        {
                            ai.CurrentState = AIState.Idle;
                            ai.StateTimer = 0f;
                        }
                        break;
                }
            }
        }

        private void HandleDeathState(Entity mob, AIComponent ai, AnimationComponent anim, MobStatsComponent stats)
        {
            EnsureMediatorAndTimemanager();
            if (ai.CurrentState != AIState.Dead)
            {
                ai.CurrentState = AIState.Dead;
                anim.PlayAnimation("Dead");
                var collider = mob.GetComponent<ColliderComponent>();
                if (collider != null) collider.IsSolid = false;
                ai.StateTimer = 0f;

                 _gameplayMediator.Notify(this, GameplayEvent.EntityDied, mob, mob.GetComponent<HealthComponent>());
            }

            if (anim.Controller.CurrentAnimationData?.Name == "Dead" && anim.Controller.AnimationFinished && !anim.Controller.IsPausedOnFrame)
            {
                anim.Controller.PauseOnCurrentFrame();
            }
        }

        private float GetHurtAnimationDuration(AnimationComponent anim)
        {
            var currentAnimData = anim.Controller.CurrentAnimationData;
            if (currentAnimData != null && currentAnimData.Name.Contains("Hurt"))
            {
                return currentAnimData.TotalDuration();
            }
            if (currentAnimData != null && currentAnimData.Name.Contains("Fall")) return 1.2f;
            return 0.4f;
        }

        private void UpdateFacingDirection(Entity mob, AIComponent ai, TransformComponent transform, SpriteComponent sprite, TransformComponent playerTransform)
        {
            Vector2 directionToConsider = Vector2.Zero;

            if (ai.Target != null && (ai.CurrentState == AIState.Chasing || ai.CurrentState == AIState.Attacking))
            {
                if (playerTransform != null)
                {
                    directionToConsider = playerTransform.Position - transform.Position;
                }
            }
            else if (ai.CurrentState == AIState.Patrolling || ai.CurrentState == AIState.ReturningToPatrol)
            {
                directionToConsider = ai.PatrolTargetPosition - transform.Position;
            }
            else if (ai.Target != null && ai.CurrentState == AIState.Fleeing && playerTransform != null)
            {
                directionToConsider = transform.Position - playerTransform.Position;
            }

            if (directionToConsider.LengthSquared() > 0.01f)
            {
                ai.FacingDirection = Vector2.Normalize(directionToConsider);
            }

            if (Math.Abs(ai.FacingDirection.X) > 0.05f)
            {
                sprite.Effects = ai.FacingDirection.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }
        }

        private void HandleIdleState(Entity mob, AIComponent ai, AnimationComponent anim)
        {
            string idleAnimName = "Idle";
            anim.PlayAnimation(idleAnimName);

            if (ai.CurrentIdleTime <= 0f)
            {
                ai.CurrentIdleTime = _random.Next((int)(ai.MinIdleTime * 100), (int)(ai.MaxIdleTime * 100)) / 100f;
                ai.StateTimer = 0f;
            }

            if (ai.StateTimer >= ai.CurrentIdleTime)
            {
                ai.CurrentState = AIState.Patrolling;
                ai.StateTimer = 0f;
                ai.CurrentIdleTime = 0f;
                SetNewPatrolTarget(ai);
            }
        }

        private string GetAppropriateRunAnimationName(Entity mob, AnimationComponent anim)
        {
            if (mob.Tag.Contains("Minotaur"))
            {
                return anim.Animations.ContainsKey("Run") ? "Run" : "Walk";
            }
            return anim.Animations.ContainsKey("Run") ? "Run" : "Walk";
        }

        private void HandlePatrollingState(Entity mob, AIComponent ai, TransformComponent transform, AnimationComponent anim, MobStatsComponent stats, float deltaTime)
        {
            anim.PlayAnimation(GetAppropriateRunAnimationName(mob, anim));
            MoveTowards(transform, ai.PatrolTargetPosition, stats.MovementSpeed * deltaTime, mob, _solidNonPlayerEntitiesCache);

            if (Vector2.DistanceSquared(transform.Position, ai.PatrolTargetPosition) < 30f * 30f)
            {
                ai.CurrentState = AIState.Idle;
                ai.StateTimer = 0f;
            }
        }

        private void HandleChasingState(Entity mob, AIComponent ai, TransformComponent transform, AnimationComponent anim, MobStatsComponent stats, TransformComponent playerTransform, float deltaTime)
        {
            if (ai.Target == null || playerTransform == null || _player?.GetComponent<HealthComponent>()?.IsDead == true)
            {
                ai.Target = null;
                ai.CurrentState = AIState.ReturningToPatrol;
                ai.StateTimer = 0f;
                ai.LastKnownTargetPosition = transform.Position;
                return;
            }

            ai.LastKnownTargetPosition = playerTransform.Position;
            float distToPlayerSq = Vector2.DistanceSquared(transform.Position, playerTransform.Position);

            if (distToPlayerSq < stats.AttackRange * stats.AttackRange &&
                ai.AttackIntervalTimer <= 0f &&
                IsPlayerInAttackCone(transform, playerTransform, ai.FacingDirection, stats.AttackConeAngleDegrees))
            {
                ai.CurrentState = AIState.Attacking;
                ai.StateTimer = 0f;
                ai.DamageAppliedThisAttackCycle = false;

                AnimationData attackAnimData = GetBestAttackAnimation(anim, mob.Tag.Contains("Minotaur"));
                if (attackAnimData != null)
                {
                    ai.AttackActionTimer = attackAnimData.TotalDuration();
                }
                else
                {
                    ai.AttackActionTimer = 0.8f;
                    System.Diagnostics.Debug.WriteLine($"Warning: No attack animation found for {mob.Tag}. Using default attack duration.");
                }
                return;
            }

            if (distToPlayerSq > (stats.AggroRange + 80f) * (stats.AggroRange + 80f))
            {
                ai.CurrentState = AIState.ReturningToPatrol;
                ai.StateTimer = 0f;
            }
            else
            {
                anim.PlayAnimation(GetAppropriateRunAnimationName(mob, anim));
                MoveTowards(transform, playerTransform.Position, stats.MovementSpeed * stats.RunSpeedMultiplier * deltaTime, mob, _solidNonPlayerEntitiesCache);

                ai.CurrentChaseTimeWithoutAttack += deltaTime;
                if (ai.CurrentChaseTimeWithoutAttack > ai.MaxChaseTimeWithoutAttack)
                {
                    ai.CurrentState = AIState.ReturningToPatrol;
                    ai.StateTimer = 0f;
                }
            }
        }

        private AnimationData GetBestAttackAnimation(AnimationComponent animComp, bool isMinotaur = false)
        {
            string[] preferredAttackAnims;
            if (isMinotaur)
            {
                preferredAttackAnims = new[] { "Attack" };
            }
            else
            {
                preferredAttackAnims = new[] { "Attack_1", "Attack_2", "Attack_3", "Run_Attack", "Attack" };
            }

            foreach (var animName in preferredAttackAnims)
            {
                if (animComp.Animations.TryGetValue(animName, out var animData))
                {
                    return animData;
                }
            }
            return null;
        }

        private void HandleAttackingState(Entity mob, AIComponent ai, AnimationComponent anim, MobStatsComponent stats, TransformComponent playerTransform)
        {
            if (ai.Target == null || playerTransform == null || _player?.GetComponent<HealthComponent>()?.IsDead == true)
            {
                ai.CurrentState = AIState.Chasing;
                ai.StateTimer = 0f;
                ai.AttackIntervalTimer = 0.2f;
                return;
            }

            AnimationData attackAnimData = GetBestAttackAnimation(anim, mob.Tag.Contains("Minotaur"));
            if (attackAnimData == null)
            {
                ai.CurrentState = AIState.Chasing;
                ai.StateTimer = 0f;
                ai.AttackIntervalTimer = 0.5f;
                return;
            }

            anim.PlayAnimation(attackAnimData.Name);

            if (ai.AttackActionTimer <= 0f)
            {
                ai.CurrentState = AIState.Chasing;
                ai.StateTimer = 0f;
                ai.AttackIntervalTimer = 1f / stats.AttackSpeed;
                ai.DamageAppliedThisAttackCycle = false; // --- Resetează aici ---
                ai.CurrentChaseTimeWithoutAttack = 0f;
            }
        }

        private void HandleReturningToPatrolState(Entity mob, AIComponent ai, TransformComponent transform, AnimationComponent anim, MobStatsComponent stats, float deltaTime)
        {
            anim.PlayAnimation("Walk");
            MoveTowards(transform, ai.LastKnownTargetPosition, stats.MovementSpeed * deltaTime, mob, _solidNonPlayerEntitiesCache);

            if (Vector2.DistanceSquared(transform.Position, ai.LastKnownTargetPosition) < 30f * 30f || ai.StateTimer > 10f)
            {
                ai.Target = null;
                ai.CurrentState = AIState.Patrolling;
                SetNewPatrolTarget(ai);
                ai.StateTimer = 0f;
            }
        }

        private void HandleFleeingState(Entity mob, AIComponent ai, TransformComponent transform, AnimationComponent anim, MobStatsComponent stats, TransformComponent playerTransform, float deltaTime)
        {
            if (ai.Target == null || playerTransform == null)
            {
                ai.Target = null;
                ai.CurrentState = AIState.Idle;
                ai.StateTimer = 0f;
                return;
            }

            float distToPlayerSq = Vector2.DistanceSquared(transform.Position, playerTransform.Position);
            if (distToPlayerSq > (stats.AggroRange + 120f) * (stats.AggroRange + 120f) || ai.StateTimer > 8f)
            {
                ai.Target = null;
                ai.CurrentState = AIState.Patrolling;
                SetNewPatrolTarget(ai);
                ai.StateTimer = 0f;
                return;
            }

            anim.PlayAnimation(GetAppropriateRunAnimationName(mob, anim));
            Vector2 fleeDirection = Vector2.Normalize(transform.Position - playerTransform.Position);
            Vector2 fleeTarget = transform.Position + fleeDirection * 150f;
            MoveTowards(transform, fleeTarget, stats.MovementSpeed * stats.RunSpeedMultiplier * 1.8f * deltaTime, mob, _solidNonPlayerEntitiesCache);
        }

        private void SetNewPatrolTarget(AIComponent ai)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float radius = (float)(_random.NextDouble() * ai.MaxPatrolRadius);
            Vector2 newPatrolPos = ai.SpawnPosition + new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);

            if (_worldManager?.TileMap != null)
            {
                newPatrolPos.X = MathHelper.Clamp(newPatrolPos.X, 32, _worldManager.TileMap.WidthInPixels - 32);
                newPatrolPos.Y = MathHelper.Clamp(newPatrolPos.Y, 32, _worldManager.TileMap.HeightInPixels - 32);
            }
            ai.PatrolTargetPosition = newPatrolPos;
        }

        private void MoveTowards(TransformComponent transform, Vector2 target, float maxDistanceThisFrame, Entity selfEntity, List<Entity> allSolidEntitiesInProximity)
        {
            if (maxDistanceThisFrame <= 0) return;
            Vector2 direction = target - transform.Position;
            if (direction.LengthSquared() < 1.0f)
            {
                transform.Position = target;
                return;
            }

            float distanceToTarget = direction.Length();
            direction.Normalize();

            Vector2 moveAmount = direction * Math.Min(maxDistanceThisFrame, distanceToTarget);
            Vector2 nextPosition = transform.Position + moveAmount;

            var selfCollider = selfEntity.GetComponent<ColliderComponent>();

            if (_worldManager == null || _worldManager.TileMap == null)
            {
                transform.Position = nextPosition;
                return;
            }

            if (_worldManager.IsPositionWalkable(nextPosition))
            {
                if (CanMobMoveTo(selfEntity, selfCollider, nextPosition, allSolidEntitiesInProximity))
                {
                    transform.Position = nextPosition;
                }
                else
                {
                    Vector2 slidePositionX = new Vector2(nextPosition.X, transform.Position.Y);
                    if (Math.Abs(moveAmount.X) > 0.01f && _worldManager.IsPositionWalkable(slidePositionX) && CanMobMoveTo(selfEntity, selfCollider, slidePositionX, allSolidEntitiesInProximity))
                    {
                        transform.Position = slidePositionX;
                    }
                    else
                    {
                        Vector2 slidePositionY = new Vector2(transform.Position.X, nextPosition.Y);
                        if (Math.Abs(moveAmount.Y) > 0.01f && _worldManager.IsPositionWalkable(slidePositionY) && CanMobMoveTo(selfEntity, selfCollider, slidePositionY, allSolidEntitiesInProximity))
                        {
                            transform.Position = slidePositionY;
                        }
                    }
                }
            }
        }

        private bool CanMobMoveTo(Entity selfEntity, ColliderComponent selfCollider, Vector2 targetPosition, List<Entity> solidEntitiesInProximity)
        {
            if (selfCollider == null || !selfCollider.IsSolid) return true;

            Rectangle futureSelfBounds = new Rectangle(
                (int)(targetPosition.X + selfCollider.Offset.X - selfCollider.Bounds.Width / 2f),
                (int)(targetPosition.Y + selfCollider.Offset.Y - selfCollider.Bounds.Height / 2f),
                selfCollider.Bounds.Width,
                selfCollider.Bounds.Height
            );

            foreach (var otherEntity in solidEntitiesInProximity)
            {
                if (otherEntity == selfEntity) continue;

                var otherCollider = otherEntity.GetComponent<ColliderComponent>();
                var otherTransform = otherEntity.GetComponent<TransformComponent>();

                if (otherCollider != null && otherCollider.IsSolid && otherTransform != null)
                {
                    if (futureSelfBounds.Intersects(otherCollider.GetWorldBounds(otherTransform)))
                    {
                        return false;
                    }
                }
            }
            if (_player != null && _player != selfEntity)
            {
                var playerCollider = _player.GetComponent<ColliderComponent>();
                var playerTransform = _player.GetComponent<TransformComponent>();
                if (playerCollider != null && playerCollider.IsSolid && playerTransform != null)
                {
                    if (futureSelfBounds.Intersects(playerCollider.GetWorldBounds(playerTransform)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool IsPlayerInAttackCone(TransformComponent mobTransform, TransformComponent playerTransform, Vector2 mobFacingDirection, float coneAngleDegrees)
        {
            if (playerTransform == null) return false;
            Vector2 directionToPlayer = playerTransform.Position - mobTransform.Position;

            if (directionToPlayer.LengthSquared() == 0) return true;

            directionToPlayer.Normalize();

            float dotProduct = Vector2.Dot(mobFacingDirection, directionToPlayer);
            dotProduct = MathHelper.Clamp(dotProduct, -1.0f, 1.0f);
            float angleToPlayerRadians = (float)Math.Acos(dotProduct);

            float coneAngleRadians = MathHelper.ToRadians(coneAngleDegrees);

            return angleToPlayerRadians <= coneAngleRadians / 2.0f;
        }
    }
}