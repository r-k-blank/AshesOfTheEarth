using AshesOfTheEarth.Gameplay.Crafting;
using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace AshesOfTheEarth.UI
{
    public class CraftingRecipeWidget
    {
        public Recipe Recipe { get; }
        public Rectangle Bounds { get; set; }
        private SpriteFont _font;
        private Texture2D _backgroundTexture;
        private Texture2D _highlightTexture;

        public bool IsHovered { get; private set; }
        public bool CanBeCrafted { get; set; } // Set by CraftingScreen

        public CraftingRecipeWidget(Recipe recipe, Rectangle bounds, SpriteFont font, Texture2D background, Texture2D highlight)
        {
            Recipe = recipe;
            Bounds = bounds;
            _font = font;
            _backgroundTexture = background;
            _highlightTexture = highlight;
        }

        public void Update(Point mousePosition)
        {
            IsHovered = Bounds.Contains(mousePosition);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D currentBg = (IsHovered && _highlightTexture != null) ? _highlightTexture : _backgroundTexture;
            if (currentBg != null)
            {
                spriteBatch.Draw(currentBg, Bounds, CanBeCrafted ? Color.White : Color.DarkGray * 0.7f);
            }

            if (Recipe.OutputItemData?.Icon != null && _font != null)
            {
                int iconSize = (int)(Bounds.Height * 0.8f);
                Rectangle iconRect = new Rectangle(Bounds.X + 5, Bounds.Y + (Bounds.Height - iconSize) / 2, iconSize, iconSize);
                spriteBatch.Draw(Recipe.OutputItemData.Icon, iconRect, Color.White);

                string recipeName = $"{Recipe.OutputItemData.Name} (x{Recipe.OutputQuantity})";
                Vector2 textPosition = new Vector2(iconRect.Right + 10, Bounds.Y + (Bounds.Height - _font.LineSpacing) / 2f);
                spriteBatch.DrawString(_font, recipeName, textPosition, CanBeCrafted ? Color.LawnGreen : Color.LightGray);
            }
            else if (_font != null)
            {
                spriteBatch.DrawString(_font, Recipe.RecipeId, new Vector2(Bounds.X + 5, Bounds.Y + 5), Color.Red);
            }
        }
    }
}