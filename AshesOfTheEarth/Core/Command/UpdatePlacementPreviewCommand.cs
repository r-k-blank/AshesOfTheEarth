using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Placement;
using AshesOfTheEarth.Graphics;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;
using AshesOfTheEarth.Core.Input.Command.Templates;

namespace AshesOfTheEarth.Core.Input.Command
{
    public class UpdatePlacementPreviewCommand : AbstractGameCommand
    {
        public static readonly UpdatePlacementPreviewCommand Instance = new UpdatePlacementPreviewCommand();
        private UpdatePlacementPreviewCommand() { }

        protected override bool AllowsExecutionInInventory() => true;
        protected override bool AllowsExecutionDuringPlacementMode() => true;

        protected override bool CanExecuteGameplayConditions(Entity entity, UIManager uiManager, GameTime gameTime)
        {
            var playerController = entity.GetComponent<PlayerControllerComponent>();
            return playerController != null && playerController.IsInPlacementMode;
        }

        protected override void PerformAction(Entity entity, GameTime gameTime)
        {
            var playerController = entity.GetComponent<PlayerControllerComponent>();
            var inputManager = ServiceLocator.Get<InputManager>();
            var camera = ServiceLocator.Get<Camera>();
            var placementValidator = ServiceLocator.Get<IPlacementValidator>();

            if (playerController == null || inputManager == null || camera == null || placementValidator == null) return;

            playerController.PlacementPreviewPosition = camera.ScreenToWorld(inputManager.MousePosition);


            playerController.IsCurrentPlacementValid = placementValidator.IsPlacementValid(
                playerController.CurrentPlacingItemType,
                playerController.PlacementPreviewPosition,
                entity
            );
        }

        protected override void OnActionSuccess(Entity entity, GameTime gameTime) { }
        protected override void OnGameplayConditionFailed(Entity entity, GameTime gameTime) { }
        protected override void OnPreConditionFailed(Entity entity, GameTime gameTime) { }
    }
}