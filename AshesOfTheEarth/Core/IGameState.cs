using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AshesOfTheEarth.Core
{
    public interface IGameState
    {
        void LoadContent(ContentManager content);
        void UnloadContent(); 
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime); 
    }
}