using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;
using AshesOfTheEarth.Core.Input.Command.Templates;
using AshesOfTheEarth.Core.Services;

namespace AshesOfTheEarth.Core.Input.Command
{
    public class ExitPlacementModeCommand : AbstractGameCommand
    {
        public static readonly ExitPlacementModeCommand Instance = new ExitPlacementModeCommand();
        private ExitPlacementModeCommand() { }

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
            if (playerController != null)
            {
                playerController.IsInPlacementMode = false;
                playerController.CurrentPlacingItemType = Gameplay.Items.ItemType.None;
                System.Diagnostics.Debug.WriteLine("Exited placement mode.");
            }
        }
        protected override void OnActionSuccess(Entity entity, GameTime gameTime) { }
        protected override void OnGameplayConditionFailed(Entity entity, GameTime gameTime) { }
    }
}