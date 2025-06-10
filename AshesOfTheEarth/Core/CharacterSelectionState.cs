using AshesOfTheEarth.Core.Input;
using AshesOfTheEarth.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using AshesOfTheEarth.Graphics.Animation;

namespace AshesOfTheEarth.Core
{
    public class CharacterSelectionState : IGameState
    {
        private SpriteFont _fontTitle;
        private SpriteFont _fontDescription;
        private List<CharacterSelectionData> _characters;
        private List<SpriteSheet> _characterSpriteSheets;
        private int _selectedCharacterIndex = 0;
        private Texture2D _backgroundTexture;
        private Texture2D _slotTexture;
        private Texture2D _highlightTexture;

        private const int PREVIEW_SIZE = 128;
        private const int SLOT_SIZE = 150;
        private const int SLOT_PADDING = 20;

        public void LoadContent(ContentManager content)
        {
            try
            {
                _fontTitle = content.Load<SpriteFont>("Fonts/DefaultFont");
                _fontDescription = content.Load<SpriteFont>("Fonts/DefaultFont");
                _backgroundTexture = content.Load<Texture2D>("menu_background");
                _slotTexture = new Texture2D(ServiceLocator.Get<GraphicsDevice>(), 1, 1);
                _slotTexture.SetData(new[] { new Color(50, 50, 50, 150) });
                _highlightTexture = new Texture2D(ServiceLocator.Get<GraphicsDevice>(), 1, 1);
                _highlightTexture.SetData(new[] { new Color(80, 80, 30, 200) });
            }
            catch (System.Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine($"Error loading character selection assets: {ex.Message}");
            }

            _characters = new List<CharacterSelectionData>
            {
                new CharacterSelectionData("Gotoku", "Un razboinic agil cu miini puternice.", "Sprites/Player/Gotoku_spritelist", PlayerAnimationSetType.Gotoku, 128, 128),
                new CharacterSelectionData("Kunoichi", "Fata japoneza, I like it.", "Sprites/Player/Kunoichi_spritelist", PlayerAnimationSetType.Kunoichi, 128, 140),
                new CharacterSelectionData("Ninja", "Ninja, de asta nu ati vazut.", "Sprites/Player/Ninja_Peasant_spritelist", PlayerAnimationSetType.Ninja, 96, 96),
                new CharacterSelectionData("Vampire", "Vampir feroce.", "Sprites/Player/Vampire_Girl_Spritelist", PlayerAnimationSetType.Vampire, 128, 128)
            };
            _characterSpriteSheets = new List<SpriteSheet>(); // NOU
            foreach (var characterData in _characters)
            {
                try
                {
                    Texture2D sheetTexture = content.Load<Texture2D>(characterData.SpriteSheetPath);
                    _characterSpriteSheets.Add(new SpriteSheet(sheetTexture, characterData.FrameWidth, characterData.FrameHeight));
                    //System.Diagnostics.Debug.WriteLine($"Loaded spritesheet for {characterData.Name}");
                }
                catch (System.Exception ex)
                {
                    //System.Diagnostics.Debug.WriteLine($"Error loading spritesheet for {characterData.Name} from {characterData.SpriteSheetPath}: {ex.Message}");
                    _characterSpriteSheets.Add(null); // Adaugă null dacă nu se poate încărca, pentru a menține indexarea
                }
            }
        }

        public void UnloadContent()
        {
            _fontTitle = null;
            _fontDescription = null;
            _backgroundTexture = null;
            _slotTexture?.Dispose();
            _highlightTexture?.Dispose();
            // Nu mai avem _character.PreviewImage?.Dispose();
            // Texturile din SpriteSheet sunt gestionate de ContentManager
            _characterSpriteSheets?.Clear(); // Eliberează lista, dar nu texturile
        }

        public void Update(GameTime gameTime)
        {
            var inputManager = ServiceLocator.Get<InputManager>();

            if (inputManager.IsKeyPressed(Keys.Right) || inputManager.IsKeyPressed(Keys.D))
            {
                _selectedCharacterIndex++;
                if (_selectedCharacterIndex >= _characters.Count) _selectedCharacterIndex = 0;
            }
            else if (inputManager.IsKeyPressed(Keys.Left) || inputManager.IsKeyPressed(Keys.A))
            {
                _selectedCharacterIndex--;
                if (_selectedCharacterIndex < 0) _selectedCharacterIndex = _characters.Count - 1;
            }
            else if (inputManager.IsKeyPressed(Keys.Enter) || inputManager.IsKeyPressed(Keys.Space))
            {
                StartNewGameWithSelectedCharacter();
            }
            else if (inputManager.IsKeyPressed(Keys.Escape))
            {
                ServiceLocator.Get<GameStateManager>().ChangeState(new MainMenuState());
            }

            if (inputManager.IsLeftMouseButtonPressed())
            {
                Point mousePos = inputManager.MousePosition;
                var graphicsDevice = ServiceLocator.Get<GraphicsDevice>();
                int totalWidthOfSlots = _characters.Count * SLOT_SIZE + (_characters.Count - 1) * SLOT_PADDING;
                int startX = (graphicsDevice.Viewport.Width - totalWidthOfSlots) / 2;
                int slotY = graphicsDevice.Viewport.Height / 2 - SLOT_SIZE / 2 - 100;

                for (int i = 0; i < _characters.Count; i++)
                {
                    Rectangle slotRect = new Rectangle(startX + i * (SLOT_SIZE + SLOT_PADDING), slotY, SLOT_SIZE, SLOT_SIZE);
                    if (slotRect.Contains(mousePos))
                    {
                        _selectedCharacterIndex = i;
                        StartNewGameWithSelectedCharacter();
                        break;
                    }
                }
            }
        }

        private void StartNewGameWithSelectedCharacter()
        {
            if (_characters.Any() && _selectedCharacterIndex >= 0 && _selectedCharacterIndex < _characters.Count)
            {
                CharacterSelectionData selected = _characters[_selectedCharacterIndex];
                ServiceLocator.Get<GameStateManager>().ChangeState(new PlayingState(null, selected.SpriteSheetPath, selected.Name, selected.AnimationType, selected.FrameWidth, selected.FrameHeight));
            }
        }

        public void Draw(GameTime gameTime)
        {
            var spriteBatch = ServiceLocator.Get<SpriteBatch>();
            var graphicsDevice = ServiceLocator.Get<GraphicsDevice>();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if (_backgroundTexture != null)
            {
                spriteBatch.Draw(_backgroundTexture, graphicsDevice.Viewport.Bounds, Color.White);
            }
            else
            {
                graphicsDevice.Clear(Color.Black);
            }

            if (_fontTitle == null || _fontDescription == null)
            {
                spriteBatch.End();
                return;
            }

            string title = "Select Your Hero";
            Vector2 titleSize = _fontTitle.MeasureString(title);
            spriteBatch.DrawString(_fontTitle, title, new Vector2((graphicsDevice.Viewport.Width - titleSize.X) / 2, 50), Color.Yellow);

            if (!_characters.Any() || _characterSpriteSheets == null || _characterSpriteSheets.Count != _characters.Count)
            {
                spriteBatch.DrawString(_fontDescription, "No characters available or error loading previews.", new Vector2(100, 200), Color.Red);
                spriteBatch.End();
                return;
            }

            int totalWidthOfSlots = _characters.Count * SLOT_SIZE + (_characters.Count - 1) * SLOT_PADDING;
            int startX = (graphicsDevice.Viewport.Width - totalWidthOfSlots) / 2;
            int slotY = graphicsDevice.Viewport.Height / 2 - SLOT_SIZE / 2 - 100; // Mutat mai sus puțin

            for (int i = 0; i < _characters.Count; i++)
            {
                CharacterSelectionData character = _characters[i];
                SpriteSheet currentSheet = _characterSpriteSheets[i]; // NOU
                Rectangle slotRect = new Rectangle(startX + i * (SLOT_SIZE + SLOT_PADDING), slotY, SLOT_SIZE, SLOT_SIZE);

                Texture2D currentSlotBg = (i == _selectedCharacterIndex && _highlightTexture != null) ? _highlightTexture : _slotTexture;
                if (currentSlotBg != null) spriteBatch.Draw(currentSlotBg, slotRect, Color.White);

                if (currentSheet != null) // NOU: Verifică dacă spritesheet-ul s-a încărcat
                {
                    Rectangle sourceRect = currentSheet.GetSourceRectangle(0); // Ia primul frame (index 0)

                    // Calculează cum să încapă frame-ul în PREVIEW_SIZE, păstrând proporțiile
                    float scale;
                    if (sourceRect.Width > sourceRect.Height)
                        scale = (float)PREVIEW_SIZE / sourceRect.Width;
                    else
                        scale = (float)PREVIEW_SIZE / sourceRect.Height;

                    int RENDER_PREVIEW_SIZE_W = (int)(sourceRect.Width * scale);
                    int RENDER_PREVIEW_SIZE_H = (int)(sourceRect.Height * scale);

                    Rectangle previewDestRect = new Rectangle(
                        slotRect.X + (SLOT_SIZE - RENDER_PREVIEW_SIZE_W) / 2,
                        slotRect.Y + (SLOT_SIZE - RENDER_PREVIEW_SIZE_H) / 2, // Centrat și pe Y
                        RENDER_PREVIEW_SIZE_W,
                        RENDER_PREVIEW_SIZE_H);

                    spriteBatch.Draw(currentSheet.Texture, previewDestRect, sourceRect, Color.White);
                }
                else // Fallback dacă spritesheet-ul nu s-a încărcat
                {
                    Vector2 nameSize = _fontDescription.MeasureString(character.Name.Substring(0, System.Math.Min(character.Name.Length, 3)) + ".."); // prescurtat
                    spriteBatch.DrawString(_fontDescription, character.Name.Substring(0, System.Math.Min(character.Name.Length, 3)) + "..",
                                           new Vector2(slotRect.X + (SLOT_SIZE - nameSize.X) / 2, slotRect.Y + (SLOT_SIZE - nameSize.Y) / 2),
                                           Color.LightCoral);
                }
            }

            // Afișează numele și descrierea personajului selectat sub sloturi
            if (_selectedCharacterIndex >= 0 && _selectedCharacterIndex < _characters.Count)
            {
                CharacterSelectionData currentSelected = _characters[_selectedCharacterIndex];
                Vector2 nameSizeSelected = _fontTitle.MeasureString(currentSelected.Name);
                float nameYPos = slotY + SLOT_SIZE + SLOT_PADDING + 20;
                spriteBatch.DrawString(_fontTitle, currentSelected.Name,
                                    new Vector2((graphicsDevice.Viewport.Width - nameSizeSelected.X) / 2, nameYPos),
                                    Color.Orange);

                Vector2 descSize = _fontDescription.MeasureString(currentSelected.Description);
                float descYPos = nameYPos + _fontTitle.LineSpacing + 5;
                spriteBatch.DrawString(_fontDescription, currentSelected.Description,
                                    new Vector2((graphicsDevice.Viewport.Width - descSize.X) / 2, descYPos),
                                    Color.LightGray);
            }


            string controls = "Use Left/Right Arrows or A/D to Select. Enter/Space to Start. Esc to Go Back.";
            Vector2 controlsSize = _fontDescription.MeasureString(controls);
            spriteBatch.DrawString(_fontDescription, controls, new Vector2((graphicsDevice.Viewport.Width - controlsSize.X) / 2, graphicsDevice.Viewport.Height - 50), Color.White);

            spriteBatch.End();
        }
    }
}