using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Entities.Components
{
    public class TransformComponent : IComponent
    {
        public Vector2 Position { get; set; } = Vector2.Zero;
        public float Rotation { get; set; } = 0f; // În radiani
        public Vector2 Scale { get; set; } = Vector2.One;
    }
}