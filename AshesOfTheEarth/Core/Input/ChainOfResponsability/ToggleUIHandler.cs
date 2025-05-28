using AshesOfTheEarth.Core.Input.Command;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using AshesOfTheEarth.Core.Services;

namespace AshesOfTheEarth.Core.Input.ChainOfResponsibility
{
    public class ToggleUIHandler : AbstractInputHandler
    {
        public override ICommand ProcessInput(InputManager inputManager, Entity playerEntity, GameTime gameTime, UIManager uiManager)
        {
            if (inputManager.IsKeyPressed(Keys.Escape))
            {
                if (uiManager.IsInventoryVisible())
                {
                    uiManager.ToggleInventoryScreen();
                    return IdleCommand.Instance;
                }
                else if (uiManager.IsCraftingScreenVisible())
                {
                    uiManager.ToggleCraftingScreen();
                    return IdleCommand.Instance;
                }
                else
                {
                    var playingState = ServiceLocator.Get<PlayingState>();
                    playingState?.SaveGameAndReturnToMenu();
                    return NullCommand.Instance;
                }
            }

            if (inputManager.IsKeyPressed(Keys.I) || inputManager.IsKeyPressed(Keys.Tab))
            {
                uiManager.ToggleInventoryScreen();
                System.Diagnostics.Debug.WriteLine($"ToggleUIHandler: Inventory toggled. Visible: {uiManager.IsInventoryVisible()}");
                return IdleCommand.Instance;
            }

            if (inputManager.IsKeyPressed(Keys.C))
            {
                uiManager.ToggleCraftingScreen();
                System.Diagnostics.Debug.WriteLine($"ToggleUIHandler: Crafting toggled. Visible: {uiManager.IsCraftingScreenVisible()}");
                return IdleCommand.Instance;
            }

            if (inputManager.IsKeyPressed(Keys.F5))
            {
                var playingState = ServiceLocator.Get<PlayingState>();
                playingState?.TriggerSaveGame();
                return IdleCommand.Instance;
            }

            return base.ProcessInput(inputManager, playerEntity, gameTime, uiManager);
        }
    }
}