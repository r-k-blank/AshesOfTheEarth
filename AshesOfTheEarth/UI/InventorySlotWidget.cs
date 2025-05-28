using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Core.Input;
using AshesOfTheEarth.Entities.Components;

namespace AshesOfTheEarth.UI
{
    public class InventorySlotWidget
    {
        public Rectangle Bounds { get; set; }
        private ItemStack _currentItemStack;

        private Texture2D _slotTexture;
        private Texture2D _highlightTexture;
        private Texture2D _selectedTexture;
        private SpriteFont _font;

        public bool IsHovered { get; private set; }
        public bool IsVisuallySelected { get; set; } = false;
        public bool IsRightClicked { get; private set; }

        public bool IsEmpty => _currentItemStack == null || _currentItemStack.Type == ItemType.None || _currentItemStack.Quantity <= 0;
        public ItemData CurrentItemData => _currentItemStack?.Data;
        public ItemType CurrentItemType => _currentItemStack?.Type ?? ItemType.None;


        public InventorySlotWidget(Rectangle bounds, Texture2D slotTexture, Texture2D highlightTexture, Texture2D selectedTexture, SpriteFont font)
        {
            Bounds = bounds;
            _slotTexture = slotTexture;
            _highlightTexture = highlightTexture;
            _selectedTexture = selectedTexture;
            _font = font;
            _currentItemStack = new ItemStack(ItemType.None, 0);
        }

        public void LinkItemStack(ItemStack itemStackFromInventory)
        {
            _currentItemStack = itemStackFromInventory;
        }

        public void Update(Point mousePosition, InputManager inputManager)
        {
            IsHovered = Bounds.Contains(mousePosition);
            IsRightClicked = false;

            if (IsHovered && inputManager.IsRightMouseButtonPressed())
            {
                IsRightClicked = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Texture2D textureToDraw = _slotTexture;
            if (IsVisuallySelected && _selectedTexture != null)
            {
                textureToDraw = _selectedTexture;
            }
            else if (IsHovered && _highlightTexture != null)
            {
                textureToDraw = _highlightTexture;
            }

            if (textureToDraw != null)
            {
                spriteBatch.Draw(textureToDraw, Bounds, Color.White);
            }
            else
            {
                Texture2D pixel = ServiceLocator.Get<Texture2D>();
                if (pixel == null)
                {
                    pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                    pixel.SetData(new[] { Color.Magenta });
                }
                spriteBatch.Draw(pixel, Bounds, Color.DarkSlateGray);
            }


            if (!IsEmpty && CurrentItemData != null && CurrentItemData.Icon != null)
            {
                float iconScale = System.Math.Min(
                    (float)Bounds.Width * 0.8f / CurrentItemData.Icon.Width,
                    (float)Bounds.Height * 0.8f / CurrentItemData.Icon.Height
                );

                int iconWidth = (int)(CurrentItemData.Icon.Width * iconScale);
                int iconHeight = (int)(CurrentItemData.Icon.Height * iconScale);

                Rectangle iconRect = new Rectangle(
                    Bounds.X + (Bounds.Width - iconWidth) / 2,
                    Bounds.Y + (Bounds.Height - iconHeight) / 2,
                    iconWidth,
                    iconHeight
                );
                spriteBatch.Draw(CurrentItemData.Icon, iconRect, Color.White);

                if (_currentItemStack.Quantity > 1 && _font != null)
                {
                    string quantityText = _currentItemStack.Quantity.ToString();
                    Vector2 textSize = _font.MeasureString(quantityText);
                    Vector2 textPosition = new Vector2(
                        Bounds.Right - textSize.X - 5,
                        Bounds.Bottom - textSize.Y - 3
                    );
                    spriteBatch.DrawString(_font, quantityText, textPosition + Vector2.One, Color.Black * 0.7f);
                    spriteBatch.DrawString(_font, quantityText, textPosition, Color.White);
                }
            }
        }
    }
}