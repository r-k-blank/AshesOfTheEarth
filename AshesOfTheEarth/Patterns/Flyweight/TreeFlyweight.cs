using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AshesOfTheEarth.Patterns.Flyweight
{
    public class TreeFlyweight
    {
        private TreeTypeData _sharedTreeData;

        public TreeFlyweight(TreeTypeData sharedData)
        {
            _sharedTreeData = sharedData;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float currentHealth)
        {
            _sharedTreeData.Draw(spriteBatch, position);

            if (currentHealth < _sharedTreeData.MaxHealth * 0.3f)
            {
                // Logică vizuală opțională bazată pe starea extrinsecă
            }
        }

        public string GetName() => _sharedTreeData.Name;
        public float GetMaxHealth() => _sharedTreeData.MaxHealth;
    }
}
