using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Patterns.Flyweight
{
    public class TreeTypeData
    {
        public string Name { get; }
        public Texture2D Texture { get; }
        public Rectangle SourceRect { get; }
        public float MaxHealth { get; }

        public TreeTypeData(string name, Texture2D texture, Rectangle sourceRect, float maxHealth)
        {
            Name = name;
            Texture = texture;
            SourceRect = sourceRect;
            MaxHealth = maxHealth;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            if (Texture != null)
            {
                Vector2 origin = new Vector2(SourceRect.Width / 2f, SourceRect.Height);
                spriteBatch.Draw(Texture, position, SourceRect, Color.White, 0f, origin, 1.0f, SpriteEffects.None, 0.4f);
            }
        }
    }
}