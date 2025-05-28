using AshesOfTheEarth.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using AshesOfTheEarth.Core.Input;

namespace AshesOfTheEarth.Core
{
    public class MainMenuState : IGameState
    {
        private SpriteFont _font;
        private List<string> _menuItems;
        private int _selectedItemIndex;
        private Vector2 _menuPosition;
        private Color _normalColor = Color.White;
        private Color _selectedColor = Color.Yellow;
        private Texture2D _backgroundTexture;

        private SaveLoadManager _saveLoadManager;
        private bool _canContinue;

        public void LoadContent(ContentManager content)
        {
            try
            {
                _font = content.Load<SpriteFont>("Fonts/DefaultFont");
                _backgroundTexture = content.Load<Texture2D>("menu_background");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading main menu font or background: {ex.Message}");
            }

            _saveLoadManager = ServiceLocator.Get<SaveLoadManager>();
            _canContinue = _saveLoadManager.DoesSaveExist();

            _menuItems = new List<string>();
            if (_canContinue)
            {
                _menuItems.Add("Continue");
            }
            _menuItems.Add("New Game"); // Mutat sub Continue dacă există
            _menuItems.Add("Exit");

            _selectedItemIndex = 0;

            var graphicsDevice = ServiceLocator.Get<GraphicsDevice>();
            if (_font != null) // Verifică dacă fontul s-a încărcat
            {
                float totalMenuHeight = _menuItems.Count * _font.LineSpacing;
                _menuPosition = new Vector2(
                    graphicsDevice.Viewport.Width / 2f,
                    (graphicsDevice.Viewport.Height - totalMenuHeight) / 2f
                );
            }
            else // Poziție fallback dacă fontul lipsește
            {
                _menuPosition = new Vector2(graphicsDevice.Viewport.Width / 2f, graphicsDevice.Viewport.Height / 2f - 50);
            }
        }

        public void UnloadContent()
        {
            _font = null;
            _backgroundTexture = null;
        }

        public void Update(GameTime gameTime)
        {
            var inputManager = ServiceLocator.Get<InputManager>();

            if (inputManager.IsKeyPressed(Keys.Up))
            {
                _selectedItemIndex--;
                if (_selectedItemIndex < 0)
                {
                    _selectedItemIndex = _menuItems.Count - 1;
                }
            }
            else if (inputManager.IsKeyPressed(Keys.Down))
            {
                _selectedItemIndex++;
                if (_selectedItemIndex >= _menuItems.Count)
                {
                    _selectedItemIndex = 0;
                }
            }
            else if (inputManager.IsKeyPressed(Keys.Enter) || inputManager.IsKeyPressed(Keys.Space))
            {
                SelectItem(_menuItems[_selectedItemIndex]);
            }

            if (_font != null && inputManager.IsLeftMouseButtonPressed())
            {
                Point mousePos = inputManager.MousePosition;
                for (int i = 0; i < _menuItems.Count; i++)
                {
                    Vector2 itemSize = _font.MeasureString(_menuItems[i]);
                    Vector2 itemPos = _menuPosition + new Vector2(-itemSize.X / 2f, i * _font.LineSpacing);
                    Rectangle itemBounds = new Rectangle((int)itemPos.X, (int)itemPos.Y, (int)itemSize.X, (int)itemSize.Y);
                    if (itemBounds.Contains(mousePos))
                    {
                        SelectItem(_menuItems[i]);
                        break;
                    }
                }
            }
        }

        private void SelectItem(string selectedItem)
        {
            var gameStateManager = ServiceLocator.Get<GameStateManager>();
            switch (selectedItem)
            {
                case "New Game":
                    gameStateManager.ChangeState(new CharacterSelectionState()); // Schimbă aici
                    break;
                case "Continue":
                    if (_canContinue)
                    {
                        GameStateMemento memento = _saveLoadManager.LoadGame();
                        if (memento != null)
                        {
                            // Preluăm tipul de personaj din memento
                            gameStateManager.ChangeState(new PlayingState(memento,
                                             memento.PlayerState?.SelectedCharacterSpriteSheetPath,
                                             memento.PlayerState?.SelectedCharacterName,
                                             memento.PlayerState?.SelectedCharacterAnimationType, // Asigură-te că AnimationType e salvat/restaurat
                                             memento.PlayerState?.SelectedCharacterFrameWidth,    // Pasează din memento
                                             memento.PlayerState?.SelectedCharacterFrameHeight));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Failed to load save game. Starting Character Selection.");
                            gameStateManager.ChangeState(new CharacterSelectionState());
                        }
                    }
                    break;
                case "Exit":
                    ServiceLocator.Get<Game>().Exit();
                    break;
            }
        }

        public void Draw(GameTime gameTime)
        {
            var spriteBatch = ServiceLocator.Get<SpriteBatch>();
            var graphicsDevice = ServiceLocator.Get<GraphicsDevice>();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if (_backgroundTexture != null)
            {
                spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);
            }
            else
            {
                graphicsDevice.Clear(Color.Black);
            }


            if (_font != null)
            {
                for (int i = 0; i < _menuItems.Count; i++)
                {
                    Color color = (i == _selectedItemIndex) ? _selectedColor : _normalColor;
                    string text = _menuItems[i];
                    if (text == "Continue" && !_canContinue) color = Color.Gray;

                    Vector2 textSize = _font.MeasureString(text);
                    Vector2 position = _menuPosition + new Vector2(-textSize.X / 2f, i * _font.LineSpacing);
                    spriteBatch.DrawString(_font, text, position, color);
                }
            }
            else
            {
                // Fallback dacă fontul nu s-a încărcat
                // Încearcă să obții fontul din SpriteFontReference sau desenează text simplu
                var fallbackFont = SpriteFontReference.DefaultFont;
                if (fallbackFont != null)
                {
                    spriteBatch.DrawString(fallbackFont, "MENU (Font Missing)", _menuPosition, Color.Red);
                    for (int i = 0; i < _menuItems.Count; i++) // Afișează itemele chiar și cu font de fallback
                    {
                        Color color = (i == _selectedItemIndex) ? _selectedColor : _normalColor;
                        string text = _menuItems[i];
                        if (text == "Continue" && !_canContinue) color = Color.Gray;
                        Vector2 textSize = fallbackFont.MeasureString(text);
                        Vector2 position = _menuPosition + new Vector2(-textSize.X / 2f, (i * fallbackFont.LineSpacing) + 30);
                        spriteBatch.DrawString(fallbackFont, text, position, color);
                    }
                }
                else
                {
                    // Dacă nici fallback-ul nu merge, nu putem desena text.
                }
            }
            spriteBatch.End();
        }
    }

    internal static class SpriteFontReference
    {
        private static SpriteFont _defaultFont;
        public static SpriteFont DefaultFont
        {
            get
            {
                if (_defaultFont == null)
                {
                    try
                    {
                        var content = ServiceLocator.Get<ContentManager>();
                        _defaultFont = content.Load<SpriteFont>("Fonts/DefaultFont");
                    }
                    catch { }
                }
                return _defaultFont;
            }
        }
    }
}