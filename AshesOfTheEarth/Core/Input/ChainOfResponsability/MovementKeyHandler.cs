using AshesOfTheEarth.Core.Input.Command;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Core.Input.ChainOfResponsibility
{
    public class MovementKeyHandler : AbstractInputHandler
    {
        public override ICommand ProcessInput(InputManager inputManager, Entity playerEntity, GameTime gameTime, UIManager uiManager)
        {
            if (uiManager.IsInventoryVisible())
            {
                return base.ProcessInput(inputManager, playerEntity, gameTime, uiManager);
            }

            Vector2 moveDir = inputManager.GetCurrentMovementDirection();
            if (moveDir != Vector2.Zero)
            {
                return new MoveCommand(moveDir);
            }

            return base.ProcessInput(inputManager, playerEntity, gameTime, uiManager);
        }
    }
}