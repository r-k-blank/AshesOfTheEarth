using AshesOfTheEarth.Core.Services; // Pentru UIManager
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;
namespace AshesOfTheEarth.Core.Input.Handlers
{
    public class UIInputHandler : BaseInputHandler
    {
        public override bool HandleRequest(GameTime gameTime, InputManager inputManager, Entity playerEntity)
        {
            var uiManager = ServiceLocator.Get<UIManager>();
            if (uiManager.IsInventoryVisible()) // Sau alt UI activ
            {
                // uiManager.HandleInputForInventory(inputManager); // Logica de input a UI-ului
                System.Diagnostics.Debug.WriteLine("UIInputHandler is attempting to handle input (conceptual)");
                // Dacă UI-ul a consumat input-ul (ex: click pe un buton), returnează true
                // if (uiManager.ConsumedInput()) return true;
                return true; // Momentan, presupunem că UI-ul activ consumă tot input-ul
            }
            return base.HandleRequest(gameTime, inputManager, playerEntity); // Pasează mai departe
        }
    }
}