using AshesOfTheEarth.Core.Input;
using AshesOfTheEarth.Core.Mediator;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Crafting;
using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AshesOfTheEarth.UI
{
    public class CraftingScreen
    {
        public bool IsVisible { get; private set; } = false;

        private Texture2D _backgroundTexture;
        private Texture2D _recipeListBackground;
        private Texture2D _recipeDetailBackground;
        private Texture2D _slotTexture;
        private Texture2D _recipeSlotTexture;
        private Texture2D _recipeSlotHighlightTexture;
        private SpriteFont _font;
        private SpriteFont _smallFont;

        private List<CraftingRecipeWidget> _recipeWidgets;
        private Recipe _selectedRecipe = null;
        private int _hoveredRecipeIndex = -1;

        private Entity _player;
        private InventoryComponent _playerInventory;
        private CraftingSystem _craftingSystem;
        private IGameplayMediator _gameplayMediator;
        private InputManager _inputManager;

        private Rectangle _screenArea;
        private Rectangle _recipeListArea;
        private Rectangle _recipeDetailArea;
        private Rectangle _craftButtonArea;
        private Rectangle _closeButtonArea;

        private bool _canCraftSelected = false;

        public CraftingScreen(ContentManager content, GraphicsDevice graphicsDevice, Entity player, CraftingSystem craftingSystem)
        {
            _player = player;
            _craftingSystem = craftingSystem;
            _inputManager = ServiceLocator.Get<InputManager>();
            _gameplayMediator = ServiceLocator.Get<IGameplayMediator>();

            if (_player != null) _playerInventory = _player.GetComponent<InventoryComponent>();

            if (_playerInventory == null || _craftingSystem == null)
            {
                System.Diagnostics.Debug.WriteLine("CraftingScreen: Critical component (Inventory or CraftingSystem) or player is null.");
                return;
            }

            LoadContent(content, graphicsDevice);
            InitializeLayout(graphicsDevice);
            RefreshRecipeList();
        }

        private void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            try
            {
                _backgroundTexture = content.Load<Texture2D>("Sprites/UI/inventory_background");
                _recipeListBackground = content.Load<Texture2D>("Sprites/UI/inventory_background");
                _recipeDetailBackground = content.Load<Texture2D>("Sprites/UI/inventory_background");
                _slotTexture = content.Load<Texture2D>("Sprites/UI/inventory_slot");
                _recipeSlotTexture = content.Load<Texture2D>("Sprites/UI/inventory_slot");
                _recipeSlotHighlightTexture = content.Load<Texture2D>("Sprites/UI/inventory_slot");
                _font = content.Load<SpriteFont>("Fonts/DefaultFont");
                _smallFont = content.Load<SpriteFont>("Fonts/DefaultFont");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading crafting screen assets: {ex.Message}");
                CreateFallbackTextures(graphicsDevice);
            }
        }

        private void CreateFallbackTextures(GraphicsDevice gd)
        {
            if (_backgroundTexture == null) _backgroundTexture = new Texture2D(gd, 1, 1); _backgroundTexture.SetData(new[] { new Color(30, 30, 30, 220) });
            if (_recipeListBackground == null) _recipeListBackground = new Texture2D(gd, 1, 1); _recipeListBackground.SetData(new[] { new Color(50, 50, 50, 200) });
            if (_recipeDetailBackground == null) _recipeDetailBackground = new Texture2D(gd, 1, 1); _recipeDetailBackground.SetData(new[] { new Color(70, 70, 70, 200) });
            if (_slotTexture == null) _slotTexture = new Texture2D(gd, 1, 1); _slotTexture.SetData(new[] { Color.DarkGray });
            if (_recipeSlotTexture == null) _recipeSlotTexture = new Texture2D(gd, 1, 1); _recipeSlotTexture.SetData(new[] { Color.Gray });
            if (_recipeSlotHighlightTexture == null) _recipeSlotHighlightTexture = new Texture2D(gd, 1, 1); _recipeSlotHighlightTexture.SetData(new[] { Color.LightGray });
            if (_font == null && _smallFont != null) _font = _smallFont;
            if (_smallFont == null && _font != null) _smallFont = _font;
            if (_font == null && _smallFont == null)
            {
                System.Diagnostics.Debug.WriteLine("CraftingScreen: Default and Small fonts are missing. Text will not render.");
            }
        }

        private void InitializeLayout(GraphicsDevice graphicsDevice)
        {
            int screenWidth = (int)(graphicsDevice.Viewport.Width * 0.7f);
            int screenHeight = (int)(graphicsDevice.Viewport.Height * 0.8f);
            _screenArea = new Rectangle(
                (graphicsDevice.Viewport.Width - screenWidth) / 2,
                (graphicsDevice.Viewport.Height - screenHeight) / 2,
                screenWidth, screenHeight);

            int padding = 20;
            int recipeListWidth = (int)(_screenArea.Width * 0.4f) - padding * 2;
            _recipeListArea = new Rectangle(
                _screenArea.X + padding,
                _screenArea.Y + padding + 30,
                recipeListWidth,
                _screenArea.Height - padding * 2 - 30 - 50);

            _recipeDetailArea = new Rectangle(
                _recipeListArea.Right + padding,
                _screenArea.Y + padding + 30,
                _screenArea.Width - _recipeListArea.Width - padding * 3,
                _screenArea.Height - padding * 2 - 30 - 50);

            _craftButtonArea = new Rectangle(
                _recipeDetailArea.X + (_recipeDetailArea.Width - 150) / 2,
                _recipeDetailArea.Bottom - 60,
                150, 40);

            _closeButtonArea = new Rectangle(
                _screenArea.Right - 40 - padding,
                _screenArea.Y + padding / 2,
                40, 25
            );
        }

        private void RefreshRecipeList()
        {
            if (_craftingSystem == null || _playerInventory == null) return;

            var craftableRecipes = _craftingSystem.GetAllRecipes();
            _recipeWidgets = new List<CraftingRecipeWidget>();
            int recipeSlotHeight = 40;
            int currentY = _recipeListArea.Y + 5;

            foreach (var recipe in craftableRecipes)
            {
                Rectangle bounds = new Rectangle(_recipeListArea.X + 5, currentY, _recipeListArea.Width - 10, recipeSlotHeight);
                var widget = new CraftingRecipeWidget(recipe, bounds, _smallFont ?? _font, _recipeSlotTexture, _recipeSlotHighlightTexture);
                widget.CanBeCrafted = _craftingSystem.CanCraft(recipe, _playerInventory);
                _recipeWidgets.Add(widget);
                currentY += recipeSlotHeight + 2;
            }
        }

        public void SetVisible(bool visible)
        {
            if (IsVisible == visible) return;
            IsVisible = visible;
            _selectedRecipe = null;
            _hoveredRecipeIndex = -1;
            if (IsVisible) RefreshRecipeList();

            if (_player != null && _gameplayMediator != null)
            {
                _gameplayMediator.Notify(this, IsVisible ? GameplayEvent.InventoryOpened : GameplayEvent.InventoryClosed, _player);
            }
            System.Diagnostics.Debug.WriteLine($"CraftingScreen.IsVisible set to: {IsVisible}");
        }

        public void Update(GameTime gameTime)
        {
            if (!IsVisible || _inputManager == null || _playerInventory == null || _craftingSystem == null) return;

            Point mousePos = _inputManager.MousePosition;
            _hoveredRecipeIndex = -1;

            for (int i = 0; i < _recipeWidgets.Count; i++)
            {
                _recipeWidgets[i].Update(mousePos);
                if (_recipeWidgets[i].IsHovered)
                {
                    _hoveredRecipeIndex = i;
                    if (_inputManager.IsLeftMouseButtonPressed())
                    {
                        _selectedRecipe = _recipeWidgets[i].Recipe;
                        _canCraftSelected = _craftingSystem.CanCraft(_selectedRecipe, _playerInventory);
                    }
                }
            }

            if (_selectedRecipe != null && _craftButtonArea.Contains(mousePos) && _inputManager.IsLeftMouseButtonPressed())
            {
                if (_canCraftSelected)
                {
                    if (_craftingSystem.TryCraftItem(_selectedRecipe, _playerInventory))
                    {
                        RefreshRecipeList();
                        _canCraftSelected = _craftingSystem.CanCraft(_selectedRecipe, _playerInventory);
                    }
                }
            }

            if (_closeButtonArea.Contains(mousePos) && _inputManager.IsLeftMouseButtonPressed())
            {
                SetVisible(false);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible || _font == null) return;

            Texture2D pixelTexture = ServiceLocator.Get<Texture2D>(); // MODIFICAT: Am eliminat cheia "pixel"

            if (_backgroundTexture != null) spriteBatch.Draw(_backgroundTexture, _screenArea, Color.White);
            else spriteBatch.Draw(pixelTexture, _screenArea, new Color(30, 30, 30, 220));


            string title = "Crafting";
            Vector2 titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title, new Vector2(_screenArea.X + (_screenArea.Width - titleSize.X) / 2, _screenArea.Y + 10), Color.White);

            if (_recipeListBackground != null) spriteBatch.Draw(_recipeListBackground, _recipeListArea, Color.White);
            else spriteBatch.Draw(pixelTexture, _recipeListArea, new Color(50, 50, 50, 200));

            foreach (var widget in _recipeWidgets)
            {
                widget.Draw(spriteBatch);
            }

            if (_recipeDetailBackground != null) spriteBatch.Draw(_recipeDetailBackground, _recipeDetailArea, Color.White);
            else spriteBatch.Draw(pixelTexture, _recipeDetailArea, new Color(70, 70, 70, 200));


            DrawRecipeDetails(spriteBatch);
            DrawCraftButton(spriteBatch);

            Texture2D buttonTex = _slotTexture ?? pixelTexture;
            Color closeButtonColor = _closeButtonArea.Contains(_inputManager.MousePosition) ? Color.LightCoral : Color.IndianRed;
            spriteBatch.Draw(buttonTex, _closeButtonArea, closeButtonColor);
            if (_smallFont != null)
            {
                string closeTxt = "X";
                Vector2 closeTxtSize = _smallFont.MeasureString(closeTxt);
                spriteBatch.DrawString(_smallFont, closeTxt, new Vector2(_closeButtonArea.X + (_closeButtonArea.Width - closeTxtSize.X) / 2, _closeButtonArea.Y + (_closeButtonArea.Height - closeTxtSize.Y) / 2), Color.White);
            }

        }

        private void DrawRecipeDetails(SpriteBatch spriteBatch)
        {
            if (_selectedRecipe == null || _smallFont == null) return;

            int detailPadding = 15;
            int currentY = _recipeDetailArea.Y + detailPadding;

            ItemData outputData = _selectedRecipe.OutputItemData;
            if (outputData != null)
            {
                spriteBatch.DrawString(_font, $"Craft: {outputData.Name} x{_selectedRecipe.OutputQuantity}", new Vector2(_recipeDetailArea.X + detailPadding, currentY), Color.White);
                currentY += _font.LineSpacing + 5;
                if (outputData.Icon != null)
                {
                    Rectangle iconRect = new Rectangle(_recipeDetailArea.X + detailPadding, currentY, 64, 64);
                    spriteBatch.Draw(outputData.Icon, iconRect, Color.White);
                    currentY += 64 + detailPadding;
                }
                else
                {
                    currentY += 20;
                }
            }

            spriteBatch.DrawString(_smallFont, "Ingredients:", new Vector2(_recipeDetailArea.X + detailPadding, currentY), Color.Yellow);
            currentY += _smallFont.LineSpacing + 3;

            foreach (var ingredient in _selectedRecipe.RequiredIngredients)
            {
                ItemData ingData = ItemRegistry.GetData(ingredient.Key);
                int ownedCount = _playerInventory.GetItemCount(ingredient.Key);
                Color textColor = ownedCount >= ingredient.Value ? Color.LightGreen : Color.Salmon;

                string ingredientText = $"{ingData?.Name ?? ingredient.Key.ToString()}: {ownedCount}/{ingredient.Value}";

                Vector2 ingTextPos = new Vector2(_recipeDetailArea.X + detailPadding + 25, currentY);
                if (ingData?.Icon != null)
                {
                    Rectangle ingIconRect = new Rectangle(_recipeDetailArea.X + detailPadding, currentY, 20, 20);
                    spriteBatch.Draw(ingData.Icon, ingIconRect, Color.White);
                }

                spriteBatch.DrawString(_smallFont, ingredientText, ingTextPos, textColor);
                currentY += _smallFont.LineSpacing;
            }
        }

        private void DrawCraftButton(SpriteBatch spriteBatch)
        {
            if (_selectedRecipe == null || _font == null) return;

            Texture2D buttonTexture = _slotTexture ?? ServiceLocator.Get<Texture2D>(); // MODIFICAT
            Color buttonColor;
            string buttonText = "Craft";

            if (_canCraftSelected)
            {
                buttonColor = _craftButtonArea.Contains(_inputManager.MousePosition) ? Color.LimeGreen : Color.Green;
            }
            else
            {
                buttonColor = Color.DarkGray;
                buttonText = "Cannot Craft";
            }

            spriteBatch.Draw(buttonTexture, _craftButtonArea, buttonColor);
            Vector2 textSize = _font.MeasureString(buttonText);
            Vector2 textPos = new Vector2(
                _craftButtonArea.X + (_craftButtonArea.Width - textSize.X) / 2,
                _craftButtonArea.Y + (_craftButtonArea.Height - textSize.Y) / 2);
            spriteBatch.DrawString(_font, buttonText, textPos, Color.White);
        }

        public void Dispose()
        {
        }
    }
}