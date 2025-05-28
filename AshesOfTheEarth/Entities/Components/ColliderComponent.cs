using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Entities.Components
{
    public class ColliderComponent : IComponent
    {
        public Rectangle Bounds { get; set; } // Relativ la Offset și poziția entității
        public Vector2 Offset { get; set; }   // Offset de la Transform.Position la centrul Bounds
        public bool IsSolid { get; set; }     // NOU: Indică dacă acest colider blochează mișcarea

        public ColliderComponent(Rectangle relativeBounds, Vector2? offset = null, bool isSolid = true)
        {
            Bounds = relativeBounds;
            Offset = offset ?? Vector2.Zero;
            IsSolid = isSolid; // Default la solid
        }

        // Calculează dreptunghiul absolut în lume
        public Rectangle GetWorldBounds(TransformComponent transform)
        {
            if (transform == null) return Rectangle.Empty;
            // Presupunem că Transform.Position este centrul sprite-ului,
            // iar Offset este de la acel centru la centrul DREPTUNGHIULUI de coliziune.
            // Bounds.X și Bounds.Y sunt apoi relative la acest centru al coliderului (de obicei 0,0 dacă Bounds e centrat).
            return new Rectangle(
                (int)(transform.Position.X + Offset.X - Bounds.Width / 2f),
                (int)(transform.Position.Y + Offset.Y - Bounds.Height / 2f),
                Bounds.Width,
                Bounds.Height
            );
        }

        public bool Intersects(TransformComponent Transform, ColliderComponent otherCollider, TransformComponent otherTransform)
        {
            if (otherCollider == null || Transform == null || otherTransform == null) return false;
            return GetWorldBounds(Transform).Intersects(otherCollider.GetWorldBounds(otherTransform));
        }
    }
}