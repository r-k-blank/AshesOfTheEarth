using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using AshesOfTheEarth.Core.Services; // Pentru a accesa ContentManager

namespace AshesOfTheEarth.Core
{
    public class GameStateManager
    {
        public IGameState CurrentState { get; private set; }
        private IGameState _nextState = null;

        public void ChangeState(IGameState newState)
        {
          
            _nextState = newState;
        }

      
        public void Update(GameTime gameTime)
        {
           
            if (_nextState != null)
            {
                CurrentState?.UnloadContent(); 
                CurrentState = _nextState;
                _nextState = null;

              
                var content = ServiceLocator.Get<ContentManager>();
                CurrentState?.LoadContent(content);
                //System.Diagnostics.Debug.WriteLine($"GameState changed to: {CurrentState?.GetType().Name}");
            }

            
            CurrentState?.Update(gameTime);
        }

        
        public void Draw(GameTime gameTime)
        {
            CurrentState?.Draw(gameTime);
        }
    }
}