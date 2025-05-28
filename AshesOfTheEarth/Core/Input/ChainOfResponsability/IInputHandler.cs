using AshesOfTheEarth.Core.Input.Command;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Core.Input.ChainOfResponsibility
{
    public interface IInputHandler
    {
        IInputHandler SetNext(IInputHandler handler);
        ICommand ProcessInput(InputManager inputManager, Entity playerEntity, GameTime gameTime, UIManager uiManager);
    }
}