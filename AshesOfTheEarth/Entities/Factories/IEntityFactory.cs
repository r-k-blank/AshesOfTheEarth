using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Entities.Factories
{
    public interface IEntityFactory
    {
        Entity CreateEntity(Vector2 position);
    }
}