using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using AshesOfTheEarth.Entities.Factories;
using AshesOfTheEarth.Graphics.Animation;
using AshesOfTheEarth.Core.Mediator;
using System.Collections.Generic;

namespace AshesOfTheEarth.Gameplay.Systems
{
    public class HarvestingSystem
    {
        private EntityManager _entityManager;
        private IGameplayMediator _gameplayMediator;
        private CollectibleFactory _collectibleFactory;
        private Random _random = new Random();

        public HarvestingSystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
            _collectibleFactory = ServiceLocator.Get<CollectibleFactory>();
            if (_collectibleFactory == null)
            {
                _collectibleFactory = new CollectibleFactory();
                ServiceLocator.Register<CollectibleFactory>(_collectibleFactory);
                _collectibleFactory.InitializePool();
            }
        }

        private void EnsureMediator()
        {
            if (_gameplayMediator == null)
                _gameplayMediator = ServiceLocator.Get<IGameplayMediator>();
        }

        public void ProcessInteractionAttempt(Entity playerEntity, GameTime gameTime)
        {
            EnsureMediator();

            var playerTransform = playerEntity.GetComponent<TransformComponent>();
            var playerInventory = playerEntity.GetComponent<InventoryComponent>();

            if (playerTransform == null || playerInventory == null) return;

            float interactionRange = 64f;
            Rectangle interactionArea = new Rectangle(
                (int)(playerTransform.Position.X - interactionRange / 2),
                (int)(playerTransform.Position.Y - interactionRange / 2),
                (int)interactionRange,
                (int)interactionRange
            );

            Entity closestInteractableEntity = null;
            float minDistanceSq = interactionRange * interactionRange;

            var nearbyCollectibles = _entityManager.GetEntitiesInBounds(interactionArea)
                                         .Where(e => e.IsActive && e.HasComponent<CollectibleComponent>());

            foreach (var collectibleEntity in nearbyCollectibles)
            {
                var collectibleTransform = collectibleEntity.GetComponent<TransformComponent>();
                float distSq = Vector2.DistanceSquared(playerTransform.Position, collectibleTransform.Position);

                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                    closestInteractableEntity = collectibleEntity;
                }
            }

            if (closestInteractableEntity != null && closestInteractableEntity.HasComponent<CollectibleComponent>())
            {
                AttemptCollectItem(closestInteractableEntity, playerEntity);
                return;
            }

            minDistanceSq = interactionRange * interactionRange;
            closestInteractableEntity = null;

            var nearbyHarvestables = _entityManager.GetEntitiesInBounds(interactionArea)
                                          .Where(e => e.HasComponent<ResourceSourceComponent>());

            foreach (var resourceEntity in nearbyHarvestables)
            {
                var resourceSource = resourceEntity.GetComponent<ResourceSourceComponent>();
                if (resourceSource.Depleted && !resourceSource.DestroyOnDepleted) continue;
                if (resourceSource.Depleted && resourceSource.DestroyOnDepleted) continue;

                var resourceTransform = resourceEntity.GetComponent<TransformComponent>();
                float distSq = Vector2.DistanceSquared(playerTransform.Position, resourceTransform.Position);

                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                    closestInteractableEntity = resourceEntity;
                }
            }

            if (closestInteractableEntity != null && closestInteractableEntity.HasComponent<ResourceSourceComponent>())
            {
                AttemptHarvest(closestInteractableEntity, playerEntity, gameTime);
            }
        }

        private bool AttemptCollectItem(Entity collectibleEntity, Entity playerEntity)
        {
            var collectibleComp = collectibleEntity.GetComponent<CollectibleComponent>();
            var playerInventory = playerEntity.GetComponent<InventoryComponent>();

            if (collectibleComp == null || playerInventory == null) return false;

            ItemStack itemStack = new ItemStack(collectibleComp.ItemToCollect, collectibleComp.Quantity);
            bool added = playerInventory.AddItem(itemStack.Type, itemStack.Quantity);

            if (added)
            {
                _collectibleFactory.ReturnCollectibleToPool(collectibleEntity);
                EnsureMediator();
                _gameplayMediator.Notify(this, GameplayEvent.ItemCollected, playerEntity, itemStack);
                return true;
            }
            return false;
        }

        private bool AttemptHarvest(Entity resourceEntity, Entity playerEntity, GameTime gameTime)
        {
            var resourceSource = resourceEntity.GetComponent<ResourceSourceComponent>();
            var playerInventory = playerEntity.GetComponent<InventoryComponent>();
            var playerAnimation = playerEntity.GetComponent<AnimationComponent>();
            var playerTransform = playerEntity.GetComponent<TransformComponent>();
            var playerSprite = playerEntity.GetComponent<SpriteComponent>();
            var playerController = playerEntity.GetComponent<PlayerControllerComponent>();
            var playerStats = playerEntity.GetComponent<StatsComponent>();

            if (resourceSource == null || (resourceSource.Depleted && !resourceSource.DestroyOnDepleted) || playerInventory == null || playerAnimation == null || playerTransform == null || playerController == null || playerSprite == null || playerStats == null) return false;
            if (resourceSource.Depleted && resourceSource.DestroyOnDepleted) return false;


            float toolEffectiveness = 0.5f;
            ItemData usedToolData = null;

            if (string.IsNullOrEmpty(resourceSource.RequiredToolCategory) || resourceSource.RequiredToolCategory.Equals("Hand", StringComparison.OrdinalIgnoreCase))
            {
                toolEffectiveness = 1.0f;
            }
            else
            {
                var bestTool = playerInventory.Items
                    .Where(stack => stack.Type != ItemType.None && stack.Data != null && stack.Data.Category == ItemCategory.Tool && stack.Data.ToolType == resourceSource.RequiredToolCategory)
                    .OrderByDescending(stack => stack.Data.ToolEffectiveness)
                    .FirstOrDefault();

                if (bestTool != null)
                {
                    usedToolData = bestTool.Data;
                    toolEffectiveness = usedToolData.ToolEffectiveness;
                }
                else
                {
                    return false;
                }
            }

            if (!playerStats.TryUseStamina(3f * (1f / toolEffectiveness)))
            {
                return false;
            }

            string currentFacing = PlayerControllerComponent.GetFacingDirectionFromAnimation(playerAnimation.Controller.CurrentAnimationName, playerSprite.Effects);
            string harvestAnimName = "Attack_" + currentFacing;
            playerAnimation.PlayAnimation(harvestAnimName);
            playerController.IsAttacking = true;

            float attackDuration = 0.5f;
            if (playerAnimation.Animations.TryGetValue(harvestAnimName, out AnimationData animData))
            {
                attackDuration = Graphics.Animation.AnimationDataExtensions.TotalDuration(animData);
            }

            ServiceLocator.Get<Core.Time.TimeManager>().SetTimeout(() => {
                playerController.IsAttacking = false;
            }, TimeSpan.FromSeconds(attackDuration));


            float damageToResource = 10f * toolEffectiveness;
            if (resourceSource.RequiredToolCategory != null && resourceSource.RequiredToolCategory.Equals("Hand", StringComparison.OrdinalIgnoreCase))
            {
                damageToResource = resourceSource.MaxHealth;
            }

            resourceSource.TakeDamage(damageToResource);

            if (resourceSource.Depleted)
            {
                foreach (var dropInfo in resourceSource.PossibleDrops)
                {
                    if (_random.NextDouble() < dropInfo.Chance)
                    {
                        int amountToDrop = _random.Next(dropInfo.MinAmount, dropInfo.MaxAmount + 1);
                        if (amountToDrop > 0)
                        {
                            ItemStack collectedStack = new ItemStack(dropInfo.Item, amountToDrop);
                            bool added = playerInventory.AddItem(collectedStack.Type, collectedStack.Quantity);
                            if (added)
                            {
                                _gameplayMediator.Notify(this, GameplayEvent.ItemCollected, playerEntity, collectedStack);
                            }
                        }
                    }
                }

                if (resourceSource.DestroyOnDepleted)
                {
                    _entityManager.RemoveEntity(resourceEntity);
                }
                else
                {
                    if (resourceEntity.Tag.StartsWith("Bush_"))
                    {
                        if (System.Enum.TryParse<BushType>(resourceEntity.Tag.Substring("Bush_".Length), out BushType type))
                        {
                            BushFactory.SetBushToHarvestedState(resourceEntity, type);
                        }
                    }
                }
            }
            return true;
        }
        public void Update(GameTime gameTime)
        {
        }
    }
}