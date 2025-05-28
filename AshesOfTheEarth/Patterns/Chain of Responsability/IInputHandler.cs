using AshesOfTheEarth.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.Core.Input.Handlers
{
    public interface IInputHandler
    {
        void SetNext(IInputHandler nextHandler);
        bool HandleRequest(GameTime gameTime, InputManager inputManager, Entity playerEntity); // Returnează true dacă a gestionat
    }
}
