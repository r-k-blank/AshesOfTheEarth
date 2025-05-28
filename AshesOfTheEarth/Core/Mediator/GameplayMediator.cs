using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Systems;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;
using AshesOfTheEarth.Entities.Mobs.AI;
using System;
using AshesOfTheEarth.Gameplay;
using AshesOfTheEarth.Core.Time;


namespace AshesOfTheEarth.Core.Mediator
{
    public class GameplayMediator : IGameplayMediator
    {
        private CombatSystem _combatSystem;
        private HarvestingSystem _harvestingSystem;
        private UIManager _uiManager;
        private EntityManager _entityManager;
        private DropGenerationSystem _dropGenerationSystem;
        private TimeManager _timeManager;

        public GameplayMediator()
        {
        }

        private void EnsureSystemsInitialized()
        {
            if (_combatSystem == null) _combatSystem = ServiceLocator.Get<CombatSystem>();
            if (_harvestingSystem == null) _harvestingSystem = ServiceLocator.Get<HarvestingSystem>();
            if (_uiManager == null) _uiManager = ServiceLocator.Get<UIManager>();
            if (_entityManager == null) _entityManager = ServiceLocator.Get<EntityManager>();
            if (_dropGenerationSystem == null) _dropGenerationSystem = ServiceLocator.Get<DropGenerationSystem>();
            if (_timeManager == null) _timeManager = ServiceLocator.Get<TimeManager>();
        }

        public void Notify(object sender, GameplayEvent eventType, Entity actor, object payload = null, GameTime gameTime = null)
        {
            EnsureSystemsInitialized();

            switch (eventType)
            {
                case GameplayEvent.PlayerAttackAttempt:
                    _combatSystem?.ProcessPlayerAttack(actor, gameTime, payload);
                    break;

                case GameplayEvent.PlayerInteractAttempt:
                    _harvestingSystem?.ProcessInteractionAttempt(actor, gameTime);
                    break;

                case GameplayEvent.InventoryOpened:
                    break;

                case GameplayEvent.InventoryClosed:
                    break;

                case GameplayEvent.EntityDied:
                    if (actor != null && payload is HealthComponent healthComp && healthComp.IsDead)
                    {
                        if (actor.Tag == "Player")
                        {
                            System.Diagnostics.Debug.WriteLine("Mediator: Player has died. Game Over sequence should start.");
                        }
                        else
                        {
                            var mobStats = actor.GetComponent<MobStatsComponent>();
                            float lingerDuration = mobStats?.DeathLingerDuration ?? 1.8f;

                            if (_timeManager != null)
                            {
                                _timeManager.SetTimeout(() => {
                                    var currentActorState = _entityManager.GetEntity(actor.Id);
                                    if (currentActorState != null && currentActorState.GetComponent<HealthComponent>()?.IsDead == true)
                                    {
                                        var dropPosition = currentActorState.GetComponent<TransformComponent>()?.Position ?? Vector2.Zero;
                                        _dropGenerationSystem?.GenerateDrops(currentActorState, dropPosition);
                                        _entityManager.RemoveEntity(currentActorState);
                                    }
                                }, TimeSpan.FromSeconds(lingerDuration));
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("Mediator Error: TimeManager not available for EntityDied timeout.");
                                var dropPosition = actor.GetComponent<TransformComponent>()?.Position ?? Vector2.Zero;
                                _dropGenerationSystem?.GenerateDrops(actor, dropPosition);
                                _entityManager.RemoveEntity(actor);
                            }
                        }
                    }
                    else if (actor != null && payload is HealthComponent hc && !hc.IsDead)
                    {
                    }
                    else
                    {
                    }
                    break;

                case GameplayEvent.PlayerDamaged:
                    if (actor != null && payload is float damageAmount && actor.Tag == "Player")
                    {
                        System.Diagnostics.Debug.WriteLine($"Mediator: Player {actor.Tag} took {damageAmount} damage.");
                    }
                    break;

                case GameplayEvent.EntityDamaged:
                    if (actor != null && payload is float dmgAmount && actor.Tag != "Player")
                    {
                        System.Diagnostics.Debug.WriteLine($"Mediator: Entity {actor.Tag} took {dmgAmount} damage.");
                    }
                    break;
            }
        }
    }
}