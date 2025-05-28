using AshesOfTheEarth.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.Core.Input.Handlers
{
    public abstract class BaseInputHandler : IInputHandler
    {
        protected IInputHandler _nextHandler;

        public void SetNext(IInputHandler nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public virtual bool HandleRequest(GameTime gameTime, InputManager inputManager, Entity playerEntity)
        {
            if (_nextHandler != null)
            {
                return _nextHandler.HandleRequest(gameTime, inputManager, playerEntity);
            }
            return false; // Nu a fost gestionat de nimeni în lanț
        }
    }
}