using AshesOfTheEarth.Core.Input.Command;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AshesOfTheEarth.Core.Input.ChainOfResponsibility
{
    public class PlacementModeInputHandler : AbstractInputHandler
    {
        public override ICommand ProcessInput(InputManager inputManager, Entity playerEntity, GameTime gameTime, UIManager uiManager)
        {
            var playerController = playerEntity?.GetComponent<PlayerControllerComponent>();

            if (playerController != null && playerController.IsInPlacementMode)
            {
                if (inputManager.IsLeftMouseButtonPressed())
                {
                    return ExecutePlacementCommand.Instance;
                }
                if (inputManager.IsRightMouseButtonPressed() || inputManager.IsKeyPressed(Keys.Escape))
                {
                    return ExitPlacementModeCommand.Instance;
                }
                return UpdatePlacementPreviewCommand.Instance;
            }
            return base.ProcessInput(inputManager, playerEntity, gameTime, uiManager);
        }
    }
}