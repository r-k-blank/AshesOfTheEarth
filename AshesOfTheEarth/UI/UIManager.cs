using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.Core.Time;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using System.Linq;
using AshesOfTheEarth.Core.Mediator;
using AshesOfTheEarth.Gameplay.Crafting;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using AshesOfTheEarth.Gameplay.Placement;
using AshesOfTheEarth.Core.Input;
using AshesOfTheEarth.Graphics;

namespace AshesOfTheEarth.UI
{
    public class UIManager : ITimeObserver
    {
        private ContentManager _content;
        private GraphicsDevice _graphicsDevice;
        private HUD _hud;
        private InventoryScreen _inventoryScreen;
        private CraftingScreen _craftingScreen;
        private bool _isHudVisible = false;
        private IGameplayMediator _gameplayMediator;
        private IPlacementValidator _placementValidator;
        private Texture2D _pixelTexture;

        public UIManager()
        {
            _content = ServiceLocator.Get<ContentManager>();
            _graphicsDevice = ServiceLocator.Get<GraphicsDevice>();
            _gameplayMediator = ServiceLocator.Get<IGameplayMediator>();
            _placementValidator = ServiceLocator.Get<IPlacementValidator>();

            _pixelTexture = ServiceLocator.Get<Texture2D>();
            if (_pixelTexture == null)
            {
                _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
                ServiceLocator.Register<Texture2D>(_pixelTexture);
            }
        }

        public void LoadContent(ContentManager content)
        {
            _content = content;
            if (_pixelTexture == null)
            {
                _pixelTexture = ServiceLocator.Get<Texture2D>();
                if (_pixelTexture == null)
                {
                    _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                    _pixelTexture.SetData(new[] { Color.White });
                }
            }


            Entity player = ServiceLocator.Get<EntityManager>()?.GetAllEntities().FirstOrDefault(e => e.HasComponent<PlayerControllerComponent>());
            CraftingSystem craftingSystem = ServiceLocator.Get<CraftingSystem>();

            if (player != null && _graphicsDevice != null)
            {
                if (_inventoryScreen == null)
                {
                    _inventoryScreen = new InventoryScreen(_content, _graphicsDevice, player);
                }
                if (_craftingScreen == null && craftingSystem != null)
                {
                    _craftingScreen = new CraftingScreen(_content, _graphicsDevice, player, craftingSystem);
                }
            }
        }

        public void LoadHUD()
        {
            if (_hud == null) _hud = new HUD(_content, _graphicsDevice);
            _isHudVisible = true;
        }
        public void HideHUD() { _isHudVisible = false; }

        public InventoryScreen GetInventoryScreen() => _inventoryScreen;


        public void ToggleInventoryScreen()
        {
            Entity player = ServiceLocator.Get<EntityManager>().GetAllEntities().FirstOrDefault(e => e.HasComponent<PlayerControllerComponent>());
            if (player == null) return;

            if (_inventoryScreen == null)
            {
                _inventoryScreen = new InventoryScreen(_content, _graphicsDevice, player);
            }

            if (_inventoryScreen.IsVisible && _craftingScreen != null && _craftingScreen.IsVisible)
            {
                _craftingScreen.SetVisible(false);
            }

            bool currentVisibility = _inventoryScreen.IsVisible;
            _inventoryScreen.SetVisible(!currentVisibility);
            if (_inventoryScreen.IsVisible) _inventoryScreen.RefreshInventoryLinks();
        }

        public bool IsInventoryVisible()
        {
            return _inventoryScreen != null && _inventoryScreen.IsVisible;
        }

        public void ToggleCraftingScreen()
        {
            Entity player = ServiceLocator.Get<EntityManager>().GetAllEntities().FirstOrDefault(e => e.HasComponent<PlayerControllerComponent>());
            CraftingSystem craftingSystem = ServiceLocator.Get<CraftingSystem>();
            if (player == null || craftingSystem == null) return;

            if (_craftingScreen == null)
            {
                _craftingScreen = new CraftingScreen(_content, _graphicsDevice, player, craftingSystem);
            }

            var playerController = player.GetComponent<PlayerControllerComponent>();
            if (playerController != null && playerController.IsInPlacementMode)
            {
                playerController.IsInPlacementMode = false;
            }

            if (_craftingScreen.IsVisible && _inventoryScreen != null && _inventoryScreen.IsVisible)
            {
                _inventoryScreen.SetVisible(false);
            }
            _craftingScreen.SetVisible(!_craftingScreen.IsVisible);
        }

        public bool IsCraftingScreenVisible()
        {
            return _craftingScreen != null && _craftingScreen.IsVisible;
        }

        public void Update(GameTime gameTime)
        {
            if (_isHudVisible && _hud != null)
            {
                _hud.Update(gameTime);
            }

            if (IsInventoryVisible())
            {
                _inventoryScreen?.Update(gameTime);
            }
            if (IsCraftingScreenVisible())
            {
                _craftingScreen?.Update(gameTime);
            }
        }

        private void DrawPlacementPreview(SpriteBatch spriteBatch)
        {
            Entity player = ServiceLocator.Get<EntityManager>()?.GetAllEntities().FirstOrDefault(e => e.HasComponent<PlayerControllerComponent>());
            var playerController = player?.GetComponent<PlayerControllerComponent>();

            if (playerController == null || !playerController.IsInPlacementMode || playerController.CurrentPlacingItemType == ItemType.None)
            {
                return;
            }
            if (_placementValidator == null)
            {
                return;
            }

            ItemData itemData = _placementValidator.GetItemDataForPlacement(playerController.CurrentPlacingItemType);
            if (itemData == null)
            {
                return;
            }

            if (itemData.Icon == null)
            {
                return;
            }

            Texture2D previewTexture = itemData.Icon;
            float baseWidth = itemData.Icon.Width;
            float baseHeight = itemData.Icon.Height;
            float finalPreviewScale = 1.0f;

            if (itemData.EntityToPlaceTag == "Campfire")
            {
                var campfireFactory = ServiceLocator.Get<Entities.Factories.CampfireFactory>();
                var tempCampfireEntity = campfireFactory?.CreateEntity(Vector2.Zero);
                var animComp = tempCampfireEntity?.GetComponent<AnimationComponent>();
                if (animComp?.SpriteSheet != null)
                {
                    previewTexture = animComp.SpriteSheet.Texture;
                    baseWidth = animComp.SpriteSheet.FrameWidth;
                    baseHeight = animComp.SpriteSheet.FrameHeight;
                }
                finalPreviewScale = tempCampfireEntity?.GetComponent<TransformComponent>()?.Scale.X ?? 3f;
            }

            float actualWidth = baseWidth * finalPreviewScale;
            float actualHeight = baseHeight * finalPreviewScale;

            Vector2 previewWorldPos = playerController.PlacementPreviewPosition;
            Color tint = playerController.IsCurrentPlacementValid ? Color.Green * 0.5f : Color.Red * 0.5f;
            var camera = ServiceLocator.Get<Camera>();
            Vector2 screenPosForDraw = camera.WorldToScreen(previewWorldPos).ToVector2();

            Rectangle sourceRectForPreview = new Rectangle(0, 0, (int)baseWidth, (int)baseHeight);
            if (itemData.EntityToPlaceTag == "Campfire" && previewTexture.GetHashCode() != itemData.Icon.GetHashCode())
            {
                var tempCampfireEntity = ServiceLocator.Get<Entities.Factories.CampfireFactory>()?.CreateEntity(Vector2.Zero);
                var animComp = tempCampfireEntity?.GetComponent<AnimationComponent>();
                if (animComp?.SpriteSheet != null)
                {
                    sourceRectForPreview = new Rectangle(0, 0, animComp.SpriteSheet.FrameWidth, animComp.SpriteSheet.FrameHeight);
                }
            }


            spriteBatch.Draw(previewTexture,
                             new Rectangle((int)(screenPosForDraw.X - actualWidth / 2), (int)(screenPosForDraw.Y - actualHeight / 2), (int)actualWidth, (int)actualHeight),
                             sourceRectForPreview,
                             tint,
                             0f,
                             Vector2.Zero,
                             SpriteEffects.None,
                             0.9f);


            if (Utils.Settings.DebugShowColliders && itemData.EntityToPlaceTag == "Campfire")
            {
                var factory = ServiceLocator.Get<Entities.Factories.CampfireFactory>();
                if (factory != null)
                {
                    Entity template = factory.CreateEntity(playerController.PlacementPreviewPosition);
                    var collider = template?.GetComponent<ColliderComponent>();
                    var transform = template?.GetComponent<TransformComponent>();
                    if (collider != null && transform != null && _pixelTexture != null)
                    {
                        Rectangle worldBounds = collider.GetWorldBounds(transform);
                        Rectangle screenBounds = new Rectangle(
                            camera.WorldToScreen(new Vector2(worldBounds.Left, worldBounds.Top)).X,
                            camera.WorldToScreen(new Vector2(worldBounds.Left, worldBounds.Top)).Y,
                            (int)(worldBounds.Width * camera.Zoom),
                            (int)(worldBounds.Height * camera.Zoom)
                            );

                        spriteBatch.Draw(_pixelTexture, new Rectangle(screenBounds.X, screenBounds.Y, screenBounds.Width, 1), Color.Magenta);
                        spriteBatch.Draw(_pixelTexture, new Rectangle(screenBounds.X, screenBounds.Bottom - 1, screenBounds.Width, 1), Color.Magenta);
                        spriteBatch.Draw(_pixelTexture, new Rectangle(screenBounds.X, screenBounds.Y, 1, screenBounds.Height), Color.Magenta);
                        spriteBatch.Draw(_pixelTexture, new Rectangle(screenBounds.Right - 1, screenBounds.Y, 1, screenBounds.Height), Color.Magenta);
                    }
                }
            }
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            DrawPlacementPreview(spriteBatch);

            if (_isHudVisible && _hud != null)
            {
                _hud.Draw(spriteBatch);
            }

            if (IsInventoryVisible())
            {
                _inventoryScreen?.Draw(spriteBatch);
            }
            if (IsCraftingScreenVisible())
            {
                _craftingScreen?.Draw(spriteBatch);
            }
        }

        public void OnTimeChanged(TimeManager timeManager) { _hud?.UpdateTimeDisplay(timeManager.GetFormattedTime(), timeManager.DayNumber); }
        public void OnDayPhaseChanged(DayPhase newPhase) { _hud?.UpdateDayPhaseDisplay(newPhase); }
        public void OnHourElapsed(int hour) { }
        public void UnloadContent()
        {
            _hud?.Dispose();
            _hud = null;
            _inventoryScreen?.Dispose();
            _inventoryScreen = null;
            _craftingScreen?.Dispose();
            _craftingScreen = null;
        }
    }
}