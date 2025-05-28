using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities.Factories;
using System.Collections.Generic;

namespace AshesOfTheEarth.Gameplay.Systems
{
    public class DropGenerationSystem
    {
        private EntityManager _entityManager;
        private CollectibleFactory _collectibleFactory;
        private Random _random = new Random();

        public DropGenerationSystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
            _collectibleFactory = ServiceLocator.Get<CollectibleFactory>();
        }

        public void GenerateDrops(Entity deceasedEntity, Vector2 dropPosition)
        {
            var lootTable = deceasedEntity.GetComponent<LootTableComponent>();
            var resourceSource = deceasedEntity.GetComponent<ResourceSourceComponent>(); // Keep for compatibility or general resources

            List<LootDropInfo> dropsToProcess = new List<LootDropInfo>();

            if (lootTable != null && lootTable.PossibleDrops.Any())
            {
                dropsToProcess.AddRange(lootTable.PossibleDrops);
            }
            else if (resourceSource != null && resourceSource.PossibleDrops.Any()) // Fallback or for non-mob resources
            {
                // Convert DropChance to LootDropInfo for consistent processing
                foreach (var rcDrop in resourceSource.PossibleDrops)
                {
                    dropsToProcess.Add(new LootDropInfo(rcDrop.Item, rcDrop.MinAmount, rcDrop.MaxAmount, rcDrop.Chance));
                }
            }

            if (dropsToProcess.Any())
            {
                System.Diagnostics.Debug.WriteLine($"Generating drops for: {deceasedEntity.Tag} (ID: {deceasedEntity.Id}) which has {dropsToProcess.Count} potential drop types.");
                foreach (var dropInfo in dropsToProcess)
                {
                    if (_random.NextDouble() < dropInfo.Chance)
                    {
                        int amountToDrop = _random.Next(dropInfo.MinAmount, dropInfo.MaxAmount + 1);
                        if (amountToDrop > 0)
                        {
                            // Spawn item on the map
                            if (_collectibleFactory != null)
                            {
                                // Add small random offset to drop position to avoid perfect stacking
                                Vector2 offset = new Vector2((float)(_random.NextDouble() * 20 - 10), (float)(_random.NextDouble() * 20 - 10));
                                Entity collectible = _collectibleFactory.CreateCollectible(dropPosition + offset, dropInfo.Item, amountToDrop);
                                if (collectible != null)
                                {
                                    _entityManager.AddEntity(collectible);
                                    System.Diagnostics.Debug.WriteLine($"Entity {deceasedEntity.Tag} dropped {amountToDrop} x {dropInfo.Item} at {dropPosition + offset}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"CollectibleFactory not available. Entity {deceasedEntity.Tag} would drop {amountToDrop} x {dropInfo.Item} at {dropPosition}");
                            }
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No drops defined (LootTable or ResourceSource) for {deceasedEntity.Tag} (ID: {deceasedEntity.Id}).");
            }
        }
    }
}