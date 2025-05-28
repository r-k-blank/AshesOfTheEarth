using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AshesOfTheEarth.UI.Widgets
{
    public class ProgressBar
    {
        public Rectangle Bounds { get; set; }
        public Color BackgroundColor { get; set; } = Color.Gray * 0.8f;
        public Color ForegroundColor { get; set; } = Color.Green;
        public Color BorderColor { get; set; } = Color.Black;
        public int BorderThickness { get; set; } = 1;

        private float _currentValue; // Valoare curentă (ex: 0-100)
        private float _maxValue;     // Valoare maximă

        public ProgressBar(Rectangle bounds, float maxValue = 100f, float initialValue = 100f)
        {
            Bounds = bounds;
            _maxValue = maxValue > 0 ? maxValue : 1;
            SetValue(initialValue);
        }

        public void SetValue(float value)
        {
            _currentValue = MathHelper.Clamp(value, 0, _maxValue);
        }
        public void SetPercentage(float percentage) // Valoare între 0.0 și 1.0
        {
            SetValue(percentage * _maxValue);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D pixelTexture) // Necesită o textură 1x1 albă
        {
            if (pixelTexture == null) return;

            // 1. Desenează fundalul
            spriteBatch.Draw(pixelTexture, Bounds, BackgroundColor);

            // 2. Calculează și desenează bara de progres
            float percentage = (_maxValue > 0) ? (_currentValue / _maxValue) : 0f;
            int foregroundWidth = (int)(Bounds.Width * percentage);
            if (foregroundWidth > 0)
            {
                Rectangle foregroundRect = new Rectangle(Bounds.X, Bounds.Y, foregroundWidth, Bounds.Height);
                spriteBatch.Draw(pixelTexture, foregroundRect, ForegroundColor);
            }

            // 3. Desenează bordura (opțional)
            if (BorderThickness > 0)
            {
                // Top
                spriteBatch.Draw(pixelTexture, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, BorderThickness), BorderColor);
                // Bottom
                spriteBatch.Draw(pixelTexture, new Rectangle(Bounds.Left, Bounds.Bottom - BorderThickness, Bounds.Width, BorderThickness), BorderColor);
                // Left
                spriteBatch.Draw(pixelTexture, new Rectangle(Bounds.Left, Bounds.Top, BorderThickness, Bounds.Height), BorderColor);
                // Right
                spriteBatch.Draw(pixelTexture, new Rectangle(Bounds.Right - BorderThickness, Bounds.Top, BorderThickness, Bounds.Height), BorderColor);
            }
        }
    }
}