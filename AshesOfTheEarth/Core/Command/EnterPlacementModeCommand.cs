using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Core.Input.Command
{
    public class EnterPlacementModeCommand : ICommand
    {
        private readonly ItemType _itemToPlace;

        public EnterPlacementModeCommand(ItemType itemToPlace)
        {
            _itemToPlace = itemToPlace;
        }

        public void Execute(Entity entity, GameTime gameTime)
        {
            var playerController = entity.GetComponent<PlayerControllerComponent>();
            var uiManager = ServiceLocator.Get<UIManager>(); // Obține UIManager aici

            if (playerController == null || ItemRegistry.GetData(_itemToPlace)?.Category != ItemCategory.Placeable)
            {
                System.Diagnostics.Debug.WriteLine($"EnterPlacementModeCommand: Failed. PlayerController null or item {_itemToPlace} not placeable.");
                return;
            }

            playerController.IsInPlacementMode = true;
            playerController.CurrentPlacingItemType = _itemToPlace;

            // Asigură-te că UIManager este valid înainte de a-l folosi
            if (uiManager != null)
            {
                if (uiManager.IsInventoryVisible())
                {
                    System.Diagnostics.Debug.WriteLine("[EnterPlacementModeCommand] Inventory is visible, toggling OFF."); // LOG
                    uiManager.ToggleInventoryScreen();
                }
                if (uiManager.IsCraftingScreenVisible())
                {
                    System.Diagnostics.Debug.WriteLine("[EnterPlacementModeCommand] Crafting screen is visible, toggling OFF."); // LOG
                    uiManager.ToggleCraftingScreen();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[EnterPlacementModeCommand] ERROR: uiManager is null, cannot toggle UI screens."); // LOG
            }

            System.Diagnostics.Debug.WriteLine($"Entered placement mode for item: {_itemToPlace}");
        }
    }
}