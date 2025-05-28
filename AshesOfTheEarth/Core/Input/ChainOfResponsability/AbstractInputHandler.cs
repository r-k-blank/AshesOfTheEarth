using AshesOfTheEarth.Core.Input.Command;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.UI; // Adăugat UIManager
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Core.Input.ChainOfResponsibility
{
    public abstract class AbstractInputHandler : IInputHandler
    {
        private IInputHandler _nextHandler;

        public IInputHandler SetNext(IInputHandler handler)
        {
            this._nextHandler = handler;
            return handler;
        }

        public virtual ICommand ProcessInput(InputManager inputManager, Entity playerEntity, GameTime gameTime, UIManager uiManager)
        {
            if (this._nextHandler != null)
            {
                return this._nextHandler.ProcessInput(inputManager, playerEntity, gameTime, uiManager);
            }
            else
            {
                return NullCommand.Instance;
            }
        }
    }
}