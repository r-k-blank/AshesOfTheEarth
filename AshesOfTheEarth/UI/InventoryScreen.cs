using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Core.Input;
using AshesOfTheEarth.Core.Mediator;
using AshesOfTheEarth.Core.Input.Command;

namespace AshesOfTheEarth.UI
{
    public class InventoryScreen
    {
        public bool IsVisible { get; private set; } = false;

        private Texture2D _backgroundTexture;
        private Texture2D _slotTexture;
        private Texture2D _slotHighlightTexture;
        private Texture2D _slotSelectedTexture;
        private SpriteFont _font;

        private List<InventorySlotWidget> _slotWidgets;
        private Entity _player;
        private InventoryComponent _playerInventory;
        private IGameplayMediator _gameplayMediator;

        private Rectangle _inventoryArea;
        private int _columns = 5;
        private int _rows;
        private int _slotSize = 64;
        private int _padding = 10;

        private int _selectedSlotIndex = -1;
        private ItemStack _heldItemStack = null;
        private Vector2 _heldItemDrawOffset = new Vector2(-24, -24);

        private InputManager _inputManager;
        private UIManager _uiManager;
        public InventoryScreen(ContentManager content, GraphicsDevice graphicsDevice, Entity player)
        {
            _player = player;
            _inputManager = ServiceLocator.Get<InputManager>();
            _gameplayMediator = ServiceLocator.Get<IGameplayMediator>();
            _uiManager = ServiceLocator.Get<UIManager>();

            if (_player != null)
            {
                _playerInventory = _player.GetComponent<InventoryComponent>();
                if (_playerInventory == null)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            LoadContent(content, graphicsDevice);
            InitializeLayout(graphicsDevice);
        }

        private void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
        {
            try
            {
                _backgroundTexture = content.Load<Texture2D>("Sprites/UI/inventory_background");
                _slotTexture = content.Load<Texture2D>("Sprites/UI/inventory_slot");
                _slotHighlightTexture = content.Load<Texture2D>("Sprites/UI/inventory_slot");
                _slotSelectedTexture = content.Load<Texture2D>("Sprites/UI/inventory_slot");
                _font = content.Load<SpriteFont>("Fonts/DefaultFont");
            }
            catch (System.Exception)
            {
                CreateFallbackTextures(graphicsDevice);
            }
        }

        private void CreateFallbackTextures(GraphicsDevice graphicsDevice)
        {
            Texture2D pixel = ServiceLocator.Get<Texture2D>();
            if (pixel == null)
            {
                pixel = new Texture2D(graphicsDevice, 1, 1);
                pixel.SetData(new[] { Color.Magenta });
            }

            if (_backgroundTexture == null) { _backgroundTexture = pixel; }
            if (_slotTexture == null) { _slotTexture = pixel; }
            if (_slotHighlightTexture == null) { _slotHighlightTexture = pixel; }
            if (_slotSelectedTexture == null) { _slotSelectedTexture = pixel; }
        }


        private void InitializeLayout(GraphicsDevice graphicsDevice)
        {
            if (_playerInventory == null) return;

            _rows = (int)System.Math.Ceiling((double)_playerInventory.Capacity / _columns);
            int totalWidth = (_columns * _slotSize) + ((_columns + 1) * _padding);
            int totalHeight = (_rows * _slotSize) + ((_rows + 1) * _padding);

            _inventoryArea = new Rectangle(
                (graphicsDevice.Viewport.Width - totalWidth) / 2,
                (graphicsDevice.Viewport.Height - totalHeight) / 2,
                totalWidth,
                totalHeight
            );

            _slotWidgets = new List<InventorySlotWidget>();
            for (int i = 0; i < _playerInventory.Capacity; i++)
            {
                int row = i / _columns;
                int col = i % _columns;

                Rectangle slotBounds = new Rectangle(
                    _inventoryArea.X + _padding + col * (_slotSize + _padding),
                    _inventoryArea.Y + _padding + row * (_slotSize + _padding),
                    _slotSize,
                    _slotSize
                );
                var widget = new InventorySlotWidget(slotBounds, _slotTexture, _slotHighlightTexture, _slotSelectedTexture, _font);
                widget.LinkItemStack(_playerInventory.Items[i]);
                _slotWidgets.Add(widget);
            }
        }

        public void SetVisible(bool visible)
        {
            if (IsVisible == visible)
            {
                return;
            }

            IsVisible = visible;
            _selectedSlotIndex = -1;
            _heldItemStack = null;

            if (_player != null && _gameplayMediator != null)
            {
                _gameplayMediator.Notify(this, IsVisible ? GameplayEvent.InventoryOpened : GameplayEvent.InventoryClosed, _player);
            }
        }


        public void RefreshInventoryLinks()
        {
            if (_playerInventory == null || _slotWidgets == null || _playerInventory.Items.Count != _slotWidgets.Count)
            {
                var gd = ServiceLocator.Get<GraphicsDevice>();
                if (gd != null) InitializeLayout(gd); else return;
            }
            for (int i = 0; i < _playerInventory.Capacity && i < _slotWidgets.Count; i++)
            {
                _slotWidgets[i].LinkItemStack(_playerInventory.Items[i]);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!IsVisible || _playerInventory == null || _inputManager == null)
            {
                if (_heldItemStack != null)
                {
                    DropHeldItemBack();
                }
                return;
            }

            Point mousePosition = _inputManager.MousePosition;

            for (int i = 0; i < _slotWidgets.Count; i++)
            {
                _slotWidgets[i].Update(mousePosition, _inputManager);

                if (_slotWidgets[i].IsHovered && _inputManager.IsLeftMouseButtonPressed())
                {
                    HandleSlotClick(i);
                }
                else if (_slotWidgets[i].IsRightClicked)
                {
                    HandleSlotRightClick(i, gameTime);
                }
            }

            if (_heldItemStack != null && _inputManager.IsKeyReleased(Keys.Escape))
            {
                DropHeldItemBack();
            }
        }
        private void HandleSlotRightClick(int clickedSlotIndex, GameTime gameTime)
        {
            if (clickedSlotIndex < 0 || clickedSlotIndex >= _slotWidgets.Count)
            {
                return;
            }

            InventorySlotWidget slotWidget = _slotWidgets[clickedSlotIndex];

            if (!slotWidget.IsEmpty && slotWidget.CurrentItemData != null)
            {
                if (slotWidget.CurrentItemData.Category == ItemCategory.Consumable)
                {
                    var consumeCommand = new ConsumeItemCommand(clickedSlotIndex);
                    consumeCommand.Execute(_player, gameTime);
                }
                else if (slotWidget.CurrentItemData.Category == ItemCategory.Placeable)
                {
                    ItemType itemToPlace = slotWidget.CurrentItemType;
                    var enterPlacementCommand = new EnterPlacementModeCommand(itemToPlace);
                    enterPlacementCommand.Execute(_player, gameTime);
                }
            }
        }
        private void HandleSlotClick(int clickedSlotIndex)
        {
            if (clickedSlotIndex < 0 || clickedSlotIndex >= _playerInventory.Items.Count) return;

            ItemStack clickedStackInInventory = _playerInventory.Items[clickedSlotIndex];

            if (_heldItemStack == null)
            {
                if (clickedStackInInventory.Type != ItemType.None && clickedStackInInventory.Quantity > 0)
                {
                    _heldItemStack = new ItemStack(clickedStackInInventory.Type, clickedStackInInventory.Quantity);
                    _playerInventory.Items[clickedSlotIndex] = new ItemStack(ItemType.None, 0);
                    _selectedSlotIndex = clickedSlotIndex;
                }
            }
            else
            {
                if (clickedStackInInventory.Type == ItemType.None)
                {
                    _playerInventory.Items[clickedSlotIndex] = _heldItemStack;
                    _heldItemStack = null;
                    _selectedSlotIndex = -1;
                }
                else if (clickedStackInInventory.Type == _heldItemStack.Type)
                {
                    ItemData itemData = clickedStackInInventory.Data;
                    if (itemData != null)
                    {
                        int canAddToStack = itemData.MaxStackSize - clickedStackInInventory.Quantity;
                        int amountToMove = System.Math.Min(_heldItemStack.Quantity, canAddToStack);

                        if (amountToMove > 0)
                        {
                            _playerInventory.Items[clickedSlotIndex].Quantity += amountToMove;
                            _heldItemStack.Quantity -= amountToMove;
                        }

                        if (_heldItemStack.Quantity <= 0)
                        {
                            _heldItemStack = null;
                            _selectedSlotIndex = -1;
                        }
                    }
                }
                else
                {
                    ItemStack tempFromSlot = new ItemStack(clickedStackInInventory.Type, clickedStackInInventory.Quantity);
                    _playerInventory.Items[clickedSlotIndex] = _heldItemStack;
                    _heldItemStack = tempFromSlot;
                }
            }
            RefreshInventoryLinks();
        }


        private void DropHeldItemBack()
        {
            if (_heldItemStack != null && _selectedSlotIndex != -1)
            {
                bool addedBack = _playerInventory.AddItem(_heldItemStack.Type, _heldItemStack.Quantity);
            }
            _heldItemStack = null;
            _selectedSlotIndex = -1;
            RefreshInventoryLinks();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;
            if (_playerInventory == null)
            {
                return;
            }

            if (_backgroundTexture != null)
            {
                spriteBatch.Draw(_backgroundTexture, _inventoryArea, Color.White * 0.95f);
            }
            else
            {
                Texture2D pixel = ServiceLocator.Get<Texture2D>();
                if (pixel != null) spriteBatch.Draw(pixel, _inventoryArea, new Color(20, 20, 20, 200));
            }


            if (_slotWidgets != null)
            {
                for (int i = 0; i < _slotWidgets.Count; i++)
                {
                    _slotWidgets[i].IsVisuallySelected = (i == _selectedSlotIndex && _heldItemStack != null);
                    _slotWidgets[i].Draw(spriteBatch);
                }
            }

            if (_heldItemStack != null && _heldItemStack.Data?.Icon != null && _font != null)
            {
                Vector2 mousePos = _inputManager.MousePosition.ToVector2();
                float iconScale = System.Math.Min(
                   (float)_slotSize * 0.8f / _heldItemStack.Data.Icon.Width,
                   (float)_slotSize * 0.8f / _heldItemStack.Data.Icon.Height
                );
                int iconWidth = (int)(_heldItemStack.Data.Icon.Width * iconScale);
                int iconHeight = (int)(_heldItemStack.Data.Icon.Height * iconScale);

                Rectangle iconRect = new Rectangle(
                    (int)(mousePos.X + _heldItemDrawOffset.X),
                    (int)(mousePos.Y + _heldItemDrawOffset.Y),
                    iconWidth,
                    iconHeight
                );
                spriteBatch.Draw(_heldItemStack.Data.Icon, iconRect, Color.White * 0.8f);

                if (_heldItemStack.Quantity > 1 && _font != null)
                {
                    string quantityText = _heldItemStack.Quantity.ToString();
                    Vector2 textSize = _font.MeasureString(quantityText);
                    Vector2 textPosition = new Vector2(
                        iconRect.Right - textSize.X - 2,
                        iconRect.Bottom - textSize.Y - 1
                    );
                    spriteBatch.DrawString(_font, quantityText, textPosition + new Vector2(1, 1), Color.Black * 0.7f);
                    spriteBatch.DrawString(_font, quantityText, textPosition, Color.White);
                }
            }
        }


        public void Dispose()
        {
        }
    }
}