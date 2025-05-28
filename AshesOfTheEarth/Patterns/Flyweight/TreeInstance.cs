using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.Patterns.Flyweight;
using System; // Added for ArgumentNullException

namespace AshesOfTheEarth.Patterns.Flyweight
{
    public class TreeInstance
    {
        public TreeFlyweight Flyweight { get; private set; }
        public Vector2 Position { get; set; }
        public float CurrentHealth { get; private set; }
        public bool IsDestroyed => CurrentHealth <= 0;

        public TreeInstance(TreeFlyweight flyweight, Vector2 position)
        {
            if (flyweight == null)
                throw new ArgumentNullException(nameof(flyweight), "Flyweight cannot be null for a TreeInstance.");

            this.Flyweight = flyweight;
            this.Position = position;
            this.CurrentHealth = flyweight.GetMaxHealth();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Flyweight.Draw(spriteBatch, Position, CurrentHealth);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0 || IsDestroyed) return;

            CurrentHealth -= amount;
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }
            System.Diagnostics.Debug.WriteLine($"Tree '{Flyweight.GetName()}' at {Position} took {amount} damage. Health: {CurrentHealth}/{Flyweight.GetMaxHealth()}");
        }
    }
}