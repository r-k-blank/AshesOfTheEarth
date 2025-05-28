using AshesOfTheEarth.Core.Input.Command;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Core.Input.ChainOfResponsibility
{
    public class DefaultInputHandler : AbstractInputHandler
    {
        public override ICommand ProcessInput(InputManager inputManager, Entity playerEntity, GameTime gameTime, UIManager uiManager)
        {
            // Chiar dacă inventarul e deschis, vrem ca player-ul să fie Idle (animație)
            // și nu să primească o comandă Null care ar putea opri animația brusc.
            return IdleCommand.Instance;
        }
    }
}