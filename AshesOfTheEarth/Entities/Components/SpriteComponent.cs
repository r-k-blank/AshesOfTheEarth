using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.Graphics.Animation; // Necesar pentru SpriteSheet

namespace AshesOfTheEarth.Entities.Components
{
    public class SpriteComponent : IComponent
    {
        public Texture2D Texture { get; set; } // Poate fi null dacă folosim doar spritesheet
        public Color Color { get; set; } = Color.White;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;
        public float LayerDepth { get; set; } = 0.5f; // Default layer (0=sus, 1=jos)
        public Vector2 Origin { get; set; } // Punctul de pivot

        // Constructor simplu
        public SpriteComponent(Texture2D texture = null)
        {
            Texture = texture;
            if (Texture != null)
            {
                // Setează originea default în centru (poate fi suprascrisă)
                Origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
            }
            else
            {
                Origin = Vector2.Zero; // Va fi setat de AnimationComponent dacă există
            }
        }

        // Metoda Draw actualizată
        public void Draw(SpriteBatch spriteBatch, TransformComponent transform, AnimationComponent animation = null)
        {
            if (transform == null) return; // Necesară poziția

            Texture2D texToDraw = null;
            Rectangle sourceRect = Rectangle.Empty;
            Vector2 drawOrigin = Origin; // Folosește originea setată, o suprascriem dacă avem animație

            if (animation?.SpriteSheet != null && animation.Controller.IsPlaying)
            {
                // Prioritizează animația dacă există și rulează
                texToDraw = animation.SpriteSheet.Texture;
                sourceRect = animation.CurrentSourceRectangle;
                // Setează originea la centrul frame-ului din spritesheet
                drawOrigin = new Vector2(animation.SpriteSheet.FrameWidth / 2f, animation.SpriteSheet.FrameHeight / 2f);
            }
            else if (Texture != null)
            {
                // Folosește textura statică dacă nu e animație sau animația nu rulează
                texToDraw = Texture;
                sourceRect = new Rectangle(0, 0, Texture.Width, Texture.Height); // Tot dreptunghiul
                                                                                 // Asigură-te că originea a fost setată corect în constructor sau manual
                if (Origin == Vector2.Zero) drawOrigin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
            }

            // Desenează doar dacă avem ce și unde
            if (texToDraw != null && sourceRect.Width > 0 && sourceRect.Height > 0)
            {
                spriteBatch.Draw(
                    texture: texToDraw,
                    position: transform.Position,
                    sourceRectangle: sourceRect,
                    color: Color,
                    rotation: transform.Rotation,
                    origin: drawOrigin, // Folosește originea calculată
                    scale: transform.Scale,
                    effects: Effects,
                    layerDepth: LayerDepth
                );
            }
        }
    }
}