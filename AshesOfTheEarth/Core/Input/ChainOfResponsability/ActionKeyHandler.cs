using AshesOfTheEarth.Core.Input.Command;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;

namespace AshesOfTheEarth.Core.Input.ChainOfResponsibility
{
    public class ActionKeyHandler : AbstractInputHandler
    {
        public override ICommand ProcessInput(InputManager inputManager, Entity playerEntity, GameTime gameTime, UIManager uiManager)
        {
            var playerController = playerEntity?.GetComponent<PlayerControllerComponent>();
            if (playerController != null && playerController.IsInPlacementMode)
            {
                return base.ProcessInput(inputManager, playerEntity, gameTime, uiManager);
            }

            if (uiManager.IsInventoryVisible() || uiManager.IsCraftingScreenVisible())
            {
                return base.ProcessInput(inputManager, playerEntity, gameTime, uiManager);
            }

            if (inputManager.IsKeyPressed(Keys.Space))
            {
                return AttackCommand.Instance;
            }

            if (inputManager.IsKeyPressed(Keys.E))
            {
                return InteractCommand.Instance;
            }

            //if (inputManager.IsKeyPressed(Keys.P)) // Tasta P pentru a intra în modul de plasare Campfire (test)
            //{
            //    var inventory = playerEntity?.GetComponent<InventoryComponent>();
            //    if (inventory != null && inventory.HasItem(ItemType.Campfire, 1))
            //    {
            //        return new EnterPlacementModeCommand(ItemType.Campfire);
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine("ActionKeyHandler: Cannot enter placement mode for Campfire, item not in inventory.");
            //        return NullCommand.Instance;
            //    }
            //}

            return base.ProcessInput(inputManager, playerEntity, gameTime, uiManager);
        }
    }
}