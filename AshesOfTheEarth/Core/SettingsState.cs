// Core/SettingsState.cs
using AshesOfTheEarth.Core.Input;
using AshesOfTheEarth.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using AshesOfTheEarth.Utils; // Pentru Settings
using System;
using System.Linq;

namespace AshesOfTheEarth.Core
{
    public class SettingsState : IGameState
    {
        private SpriteFont _font;
        private List<string> _menuItemKeys;
        private Dictionary<string, Func<string>> _menuItemTextGetters;
        private Dictionary<string, bool> _menuItemHasLeftRightOptions; // Nou: Indică dacă itemul are opțiuni stânga/dreapta
        private int _selectedItemIndex;
        private Vector2 _menuPosition;
        private Color _normalColor = Color.White;
        private Color _selectedColor = Color.Yellow;
        private Color _disabledColor = Color.Gray;
        private Texture2D _backgroundTexture;

        // Chei pentru itemele de meniu
        private const string KEY_SHOW_COLLIDERS = "ShowColliders";
        private const string KEY_WORLD_SIZE = "WorldSize";
        private const string KEY_DIFFICULTY = "Difficulty";
        // private const string KEY_USE_CUSTOM_SEED = "UseCustomSeed"; // Opțional
        // private const string KEY_WORLD_SEED_VALUE = "WorldSeedValue"; // Opțional, necesită input text
        private const string KEY_BACK = "Back";

        public void LoadContent(ContentManager content)
        {
            try
            {
                _font = content.Load<SpriteFont>("Fonts/DefaultFont");
                _backgroundTexture = content.Load<Texture2D>("Sprites/UI/menu_background");
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debug.WriteLine($"Error loading settings state assets: {ex.Message}");
                if (_font == null && SpriteFontReference.DefaultFont != null) _font = SpriteFontReference.DefaultFont;
                if (_backgroundTexture == null && ServiceLocator.Get<GraphicsDevice>() != null)
                {
                    _backgroundTexture = new Texture2D(ServiceLocator.Get<GraphicsDevice>(), 1, 1);
                    _backgroundTexture.SetData(new[] { Color.DarkSlateGray });
                }
            }

            _menuItemKeys = new List<string>
            {
                KEY_SHOW_COLLIDERS,
                KEY_WORLD_SIZE,
                KEY_DIFFICULTY,
                // KEY_USE_CUSTOM_SEED, // Opțional
                // KEY_WORLD_SEED_VALUE, // Opțional
                KEY_BACK
            };

            _menuItemTextGetters = new Dictionary<string, Func<string>>
            {
                { KEY_SHOW_COLLIDERS, () => $"Show Colliders: {(Settings.DebugShowColliders ? "ON" : "OFF")}" },
                { KEY_WORLD_SIZE, () => $"World Size: {Settings.SelectedWorldSize}" },
                { KEY_DIFFICULTY, () => $"Difficulty: {Settings.SelectedDifficulty}" },
                // { KEY_USE_CUSTOM_SEED, () => $"Use Custom Seed: {(Settings.UseCustomSeed ? "ON" : "OFF")}" }, // Opțional
                // { KEY_WORLD_SEED_VALUE, () => Settings.UseCustomSeed ? $"World Seed: {Settings.WorldSeed}" : "World Seed: Random" }, // Opțional
                { KEY_BACK, () => "Back to Main Menu" }
            };

            _menuItemHasLeftRightOptions = new Dictionary<string, bool>
            {
                { KEY_SHOW_COLLIDERS, true }, // ON/OFF poate fi schimbat și cu stânga/dreapta
                { KEY_WORLD_SIZE, true },
                { KEY_DIFFICULTY, true },
                // { KEY_USE_CUSTOM_SEED, true }, // Opțional
                // { KEY_WORLD_SEED_VALUE, Settings.UseCustomSeed }, // Doar dacă e custom seed // Opțional
                { KEY_BACK, false }
            };


            _selectedItemIndex = 0;
            UpdateMenuPosition();
        }

        private void UpdateMenuPosition()
        {
            var graphicsDevice = ServiceLocator.Get<GraphicsDevice>();
            if (graphicsDevice != null && _font != null)
            {
                float totalMenuHeight = 0;
                if (_menuItemKeys.Count > 0)
                {
                    totalMenuHeight = _menuItemKeys.Count * _font.LineSpacing;
                }

                _menuPosition = new Vector2(
                    graphicsDevice.Viewport.Width / 2f,
                    (graphicsDevice.Viewport.Height - totalMenuHeight) / 2f
                );
            }
            else if (graphicsDevice != null)
            {
                _menuPosition = new Vector2(graphicsDevice.Viewport.Width / 2f, graphicsDevice.Viewport.Height / 2f - (_menuItemKeys.Count * 15f));
            }
        }


        public void UnloadContent()
        {
            _font = null; // Gestionat de ContentManager
            _backgroundTexture = null; // Gestionat de ContentManager (dacă nu e fallback)
        }

        public void Update(GameTime gameTime)
        {
            var inputManager = ServiceLocator.Get<InputManager>();
            if (inputManager == null) return;

            if (inputManager.IsKeyPressed(Keys.Up) || inputManager.IsKeyPressed(Keys.W))
            {
                _selectedItemIndex--;
                if (_selectedItemIndex < 0) _selectedItemIndex = _menuItemKeys.Count - 1;
            }
            else if (inputManager.IsKeyPressed(Keys.Down) || inputManager.IsKeyPressed(Keys.S))
            {
                _selectedItemIndex++;
                if (_selectedItemIndex >= _menuItemKeys.Count) _selectedItemIndex = 0;
            }

            string currentSelectedKey = _menuItemKeys[_selectedItemIndex];

            // Acțiune principală (Enter/Space) sau click stânga
            if (inputManager.IsKeyPressed(Keys.Enter) || inputManager.IsKeyPressed(Keys.Space))
            {
                HandleSelection(currentSelectedKey, primaryAction: true);
            }
            else if (inputManager.IsLeftMouseButtonPressed() && _font != null)
            {
                Point mousePos = inputManager.MousePosition;
                for (int i = 0; i < _menuItemKeys.Count; i++)
                {
                    string itemText = _menuItemTextGetters[_menuItemKeys[i]]();
                    // Adaugă indicatoare dacă e cazul, pentru calculul corect al bounds
                    if (_menuItemHasLeftRightOptions.TryGetValue(_menuItemKeys[i], out bool hasLR) && hasLR && i == _selectedItemIndex)
                    {
                        itemText = "< " + itemText + " >";
                    }
                    Vector2 itemSize = _font.MeasureString(itemText);
                    Vector2 itemPos = _menuPosition + new Vector2(-itemSize.X / 2f, i * _font.LineSpacing);
                    Rectangle itemBounds = new Rectangle((int)itemPos.X, (int)itemPos.Y, (int)itemSize.X, (int)itemSize.Y);

                    if (itemBounds.Contains(mousePos))
                    {
                        if (_selectedItemIndex == i) // Click pe itemul deja selectat
                        {
                            HandleSelection(_menuItemKeys[i], primaryAction: true, mouseClick: true);
                        }
                        else // Click pe un item nou
                        {
                            _selectedItemIndex = i;
                            // Nu declanșa acțiunea imediat la selectarea cu mouse-ul, doar la click pe cel selectat
                        }
                        break;
                    }
                }
            }


            // Navigare Stânga/Dreapta pentru opțiunile care o permit
            if (_menuItemHasLeftRightOptions.TryGetValue(currentSelectedKey, out bool canChangeValue) && canChangeValue)
            {
                if (inputManager.IsKeyPressed(Keys.Left) || inputManager.IsKeyPressed(Keys.A))
                {
                    HandleSelection(currentSelectedKey, changeValueDirection: -1);
                }
                else if (inputManager.IsKeyPressed(Keys.Right) || inputManager.IsKeyPressed(Keys.D))
                {
                    HandleSelection(currentSelectedKey, changeValueDirection: 1);
                }
            }

            if (inputManager.IsKeyPressed(Keys.Escape))
            {
                GoBackToMainMenu();
            }
            _menuItemHasLeftRightOptions[KEY_SHOW_COLLIDERS] = true;
            _menuItemHasLeftRightOptions[KEY_WORLD_SIZE] = true;
            _menuItemHasLeftRightOptions[KEY_DIFFICULTY] = true;
            // _menuItemHasLeftRightOptions[KEY_USE_CUSTOM_SEED] = true; // Opțional
            // _menuItemHasLeftRightOptions[KEY_WORLD_SEED_VALUE] = Settings.UseCustomSeed; // Se actualizează dinamic // Opțional

        }

        private void HandleSelection(string selectedKey, bool primaryAction = false, int changeValueDirection = 0, bool mouseClick = false)
        {
            switch (selectedKey)
            {
                case KEY_SHOW_COLLIDERS:
                    if (primaryAction || changeValueDirection != 0) // Schimbă la Enter/Space/Click SAU Stânga/Dreapta
                    {
                        Settings.DebugShowColliders = !Settings.DebugShowColliders;
                    }
                    break;

                case KEY_WORLD_SIZE:
                    if (primaryAction && mouseClick) changeValueDirection = 1; // La click, trece la următoarea
                    if (changeValueDirection != 0)
                    {
                        Settings.WorldSizeOption currentSize = Settings.SelectedWorldSize;
                        Settings.SelectedWorldSize = CycleEnumOption(currentSize, changeValueDirection);
                    }
                    break;

                case KEY_DIFFICULTY:
                    if (primaryAction && mouseClick) changeValueDirection = 1; // La click, trece la următoarea
                    if (changeValueDirection != 0)
                    {
                        Settings.DifficultyOption currentDifficulty = Settings.SelectedDifficulty;
                        Settings.SelectedDifficulty = CycleEnumOption(currentDifficulty, changeValueDirection);
                    }
                    break;

                /* // Opțional: Seed
                case KEY_USE_CUSTOM_SEED:
                    if (primaryAction || changeValueDirection != 0)
                    {
                        Settings.UseCustomSeed = !Settings.UseCustomSeed;
                        // Actualizează dacă WorldSeedValue poate fi schimbat
                        _menuItemHasLeftRightOptions[KEY_WORLD_SEED_VALUE] = Settings.UseCustomSeed;
                        if (!Settings.UseCustomSeed) Settings.WorldSeed = 0; // Reset la Random
                    }
                    break;
                case KEY_WORLD_SEED_VALUE:
                    if (Settings.UseCustomSeed && changeValueDirection != 0)
                    {
                        // Aici ar fi input text, dar pentru simplitate, putem face +/- 1
                        // Sau dacă ai o listă de seed-uri predefinite.
                        // Pentru moment, lăsăm fără funcționalitate de schimbare directă a valorii.
                        // Poți afișa un mesaj "Enter seed on new game screen"
                        System.Diagnostics.Debug.WriteLine("Seed value modification via arrows not implemented. Set via code or future UI.");
                    }
                    break;
                */

                case KEY_BACK:
                    if (primaryAction)
                    {
                        GoBackToMainMenu();
                    }
                    break;
            }
        }

        private TEnum CycleEnumOption<TEnum>(TEnum currentOption, int direction) where TEnum : struct, Enum
        {
            var enumValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();
            int currentIndex = Array.IndexOf(enumValues, currentOption);
            int newIndex = currentIndex + direction;

            if (newIndex < 0) newIndex = enumValues.Length - 1;
            else if (newIndex >= enumValues.Length) newIndex = 0;

            return enumValues[newIndex]; // Returnează noua valoare
        }


        private void GoBackToMainMenu()
        {
            ServiceLocator.Get<GameStateManager>().ChangeState(new MainMenuState());
        }

        public void Draw(GameTime gameTime)
        {
            var spriteBatch = ServiceLocator.Get<SpriteBatch>();
            var graphicsDevice = ServiceLocator.Get<GraphicsDevice>();

            if (spriteBatch == null || graphicsDevice == null) return;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            if (_backgroundTexture != null)
            {
                spriteBatch.Draw(_backgroundTexture, graphicsDevice.Viewport.Bounds, Color.White);
            }
            else
            {
                graphicsDevice.Clear(Color.CornflowerBlue);
            }

            if (_font != null)
            {
                string title = "Game Settings";
                Vector2 titleSize = _font.MeasureString(title);
                Vector2 titlePosition = new Vector2(
                    (graphicsDevice.Viewport.Width - titleSize.X) / 2,
                    _menuPosition.Y - _font.LineSpacing * 2.5f
                );
                spriteBatch.DrawString(_font, title, titlePosition, Color.Gold);

                for (int i = 0; i < _menuItemKeys.Count; i++)
                {
                    string key = _menuItemKeys[i];
                    string textToDraw = _menuItemTextGetters[key]();
                    Color color = (i == _selectedItemIndex) ? _selectedColor : _normalColor;

                    bool isSeedValueAndNotCustom = key == "WorldSeedValue" /* && !Settings.UseCustomSeed */; // Opțional
                    if (isSeedValueAndNotCustom)
                    {
                        color = _disabledColor; // Dezactivează vizual dacă nu e custom seed
                    }

                    // Adaugă indicatoare stânga/dreapta pentru opțiunile modificabile
                    if (_menuItemHasLeftRightOptions.TryGetValue(key, out bool hasOptions) && hasOptions)
                    {
                        if (i == _selectedItemIndex && (!isSeedValueAndNotCustom)) // Doar dacă e selectat și activat
                        {
                            textToDraw = "< " + textToDraw + " >";
                        }
                    }

                    Vector2 textSize = _font.MeasureString(textToDraw);
                    Vector2 position = _menuPosition + new Vector2(-textSize.X / 2f, i * _font.LineSpacing);
                    spriteBatch.DrawString(_font, textToDraw, position, color);
                }
            }
            spriteBatch.End();
        }
    }
}