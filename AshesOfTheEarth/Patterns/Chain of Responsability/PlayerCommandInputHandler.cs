using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using Microsoft.Xna.Framework;
namespace AshesOfTheEarth.Core.Input.Handlers
{
    public class PlayerCommandInputHandler : BaseInputHandler
    {
        public override bool HandleRequest(GameTime gameTime, InputManager inputManager, Entity playerEntity)
        {
            // Logica ta existentă din InputManager.HandleInput() și PlayerControllerComponent.Update()
            // care transformă input-ul în comenzi și le execută.
            var command = inputManager.HandleInputForPlayer(playerEntity, gameTime); // Ia comanda de bază
            command?.Execute(playerEntity, gameTime);
            System.Diagnostics.Debug.WriteLine($"PlayerCommandInputHandler executed command: {command?.GetType().Name}");
            return true; // Presupunem că acest handler gestionează mereu input-ul pentru player
        }
    }
}


// --- Clientul (în PlayingState.Update, de exemplu) ---
// Ar trebui să construiești lanțul o singură dată
/*
private IInputHandler _inputChainRoot;

// În InitializeState() din PlayingState:
var uiHandler = new UIInputHandler();
var playerHandler = new PlayerCommandInputHandler();
uiHandler.SetNext(playerHandler);
_inputChainRoot = uiHandler;

// În Update() din PlayingState (în loc de apelul direct la _entityManager.Update care declanșează PlayerController):
if (!_uiManager.IsInventoryVisible()) // Sau lasă lanțul să decidă
{
    // Găsește player-ul
    Entity player = _entityManager.GetAllEntities().FirstOrDefault(e => e.Tag == "Player");
    if (player != null)
    {
        _inputChainRoot.HandleRequest(gameTime, inputManager, player);
    }
}
// Apoi _entityManager.Update(gameTime) ar trebui să ruleze componentele care nu sunt legate de input-ul direct al player-ului
// sau PlayerControllerComponent.Update ar trebui să fie mai suplu și să nu mai preia input direct.
*/