using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Entities.Factories;
using AshesOfTheEarth.Gameplay.Items;
using AshesOfTheEarth.Gameplay.Placement;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;
using AshesOfTheEarth.Core.Input.Command.Templates;

namespace AshesOfTheEarth.Core.Input.Command
{
    public class ExecutePlacementCommand : AbstractGameCommand
    {
        public static readonly ExecutePlacementCommand Instance = new ExecutePlacementCommand();
        private ExecutePlacementCommand() { }

        protected override bool AllowsExecutionInInventory() => true;
        protected override bool AllowsExecutionDuringPlacementMode() => true;


        protected override bool CanExecuteGameplayConditions(Entity entity, UIManager uiManager, GameTime gameTime)
        {
            var playerController = entity.GetComponent<PlayerControllerComponent>();
            return playerController != null &&
                   playerController.IsInPlacementMode &&
                   playerController.IsCurrentPlacementValid;
        }

        protected override void PerformAction(Entity entity, GameTime gameTime)
        {
            var playerController = entity.GetComponent<PlayerControllerComponent>();
            var inventory = entity.GetComponent<InventoryComponent>();
            var entityManager = ServiceLocator.Get<EntityManager>();
            var placementValidator = ServiceLocator.Get<IPlacementValidator>();


            if (playerController == null || inventory == null || entityManager == null || placementValidator == null) return;

            ItemData itemDataToPlace = placementValidator.GetItemDataForPlacement(playerController.CurrentPlacingItemType);
            if (itemDataToPlace == null || !inventory.HasItem(playerController.CurrentPlacingItemType, 1))
            {
                System.Diagnostics.Debug.WriteLine($"ExecutePlacementCommand: Cannot place {playerController.CurrentPlacingItemType}, item data missing or not in inventory.");
                playerController.IsInPlacementMode = false;
                return;
            }

            Entity placedEntity = null;
            if (itemDataToPlace.EntityToPlaceTag == "Campfire")
            {
                var factory = ServiceLocator.Get<CampfireFactory>();
                placedEntity = factory?.CreateEntity(playerController.PlacementPreviewPosition);
            }


            if (placedEntity != null)
            {
                var placeableComp = placedEntity.GetComponent<PlaceableComponent>() ?? new PlaceableComponent(playerController.CurrentPlacingItemType, entity.Id);
                if (!placedEntity.HasComponent<PlaceableComponent>())
                {
                    placedEntity.AddComponent(placeableComp);
                }

                entityManager.AddEntity(placedEntity);
                inventory.RemoveItem(playerController.CurrentPlacingItemType, 1);
                System.Diagnostics.Debug.WriteLine($"Placed {itemDataToPlace.Name} at {playerController.PlacementPreviewPosition}");

                if (!inventory.HasItem(playerController.CurrentPlacingItemType, 1))
                {
                    playerController.IsInPlacementMode = false;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ExecutePlacementCommand: Failed to create entity for {itemDataToPlace.EntityToPlaceTag}.");
                playerController.IsInPlacementMode = false;
            }
        }
        protected override void OnGameplayConditionFailed(Entity entity, GameTime gameTime)
        {
            System.Diagnostics.Debug.WriteLine($"ExecutePlacementCommand: Gameplay conditions failed (not in placement mode or placement invalid).");
        }
    }
}