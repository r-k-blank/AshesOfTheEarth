using AshesOfTheEarth.Entities; // Necesar dacă comenzile operează pe entități
using Microsoft.Xna.Framework; // Necesar pentru GameTime sau Vector2

namespace AshesOfTheEarth.Core.Input.Command
{
    public interface ICommand
    {
        // Execută comanda. Poate primi entitatea țintă și/sau GameTime.
        // Pentru simplitate, lăsăm Execute fără parametri acum. Logica reală
        // ar fi în constructor sau parametrii Execute.
        void Execute(Entity entity, GameTime gameTime);
    }
}