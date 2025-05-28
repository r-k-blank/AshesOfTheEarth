using AshesOfTheEarth.Entities;
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Core.Mediator
{
    public interface IGameplayMediator
    {
        void Notify(object sender, GameplayEvent eventType, Entity actor, object payload = null, GameTime gameTime = null);
    }
}