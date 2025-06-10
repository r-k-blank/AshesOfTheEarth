using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.World;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.UI;
using AshesOfTheEarth.Utils;
using AshesOfTheEarth.Gameplay.Lighting;
using System;
using Microsoft.Xna.Framework.Content;
using System.Linq;

namespace AshesOfTheEarth.Graphics
{
    public class Renderer
    {
        private Camera _camera;
        private SpriteBatch _spriteBatch;
        private BasicEffect _basicEffect;
        private GraphicsDevice _graphicsDevice;

        private bool _isDrawing = false;
        private Texture2D _pixelTexture;
        private Texture2D _lightMaskTexture;
        private LightSystem _lightSystem;

        public void Initialize(Camera camera)
        {
            _camera = camera;
            _spriteBatch = ServiceLocator.Get<SpriteBatch>();
            _graphicsDevice = ServiceLocator.Get<GraphicsDevice>();
            _lightSystem = ServiceLocator.Get<LightSystem>();

            _pixelTexture = ServiceLocator.Get<Texture2D>();
            if (_pixelTexture == null)
            {
                _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });
            }

            try
            {
                _lightMaskTexture = ServiceLocator.Get<ContentManager>().Load<Texture2D>("Sprites/UI/light_mask_radial");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Could not load 'Sprites/UI/light_mask_radial'. Creating procedural fallback. Error: {ex.Message}");
                int maskSize = 128;
                _lightMaskTexture = new Texture2D(_graphicsDevice, maskSize, maskSize);
                Color[] data = new Color[maskSize * maskSize];
                Vector2 center = new Vector2(maskSize / 2f - 0.5f, maskSize / 2f - 0.5f);
                float maxDist = maskSize / 2f;

                for (int y_coord = 0; y_coord < maskSize; y_coord++)
                {
                    for (int x_coord = 0; x_coord < maskSize; x_coord++)
                    {
                        float distance = Vector2.Distance(new Vector2(x_coord, y_coord), center);
                        float t = MathHelper.Clamp(distance / maxDist, 0f, 1f);
                        float alpha = MathHelper.SmoothStep(1.0f, 0.0f, t);
                        data[y_coord * maskSize + x_coord] = new Color(Color.White, alpha);
                    }
                }
                _lightMaskTexture.SetData(data);
            }

            _basicEffect = new BasicEffect(_graphicsDevice);
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.TextureEnabled = true;
        }

        public void BeginDraw(Matrix? transformMatrix = null)
        {
            if (_isDrawing)
            {
                return;
            }
            Matrix matrix = transformMatrix ?? Matrix.Identity;
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, matrix);
            _isDrawing = true;
        }

        public void EndDraw()
        {
            if (!_isDrawing)
            {
                return;
            }
            _spriteBatch.End();
            _isDrawing = false;
        }

        public void DrawWorld(WorldManager worldManager, SpriteBatch spriteBatch)
        {
            worldManager?.Draw(spriteBatch, _camera);
        }

        public void DrawEntities(EntityManager entityManager, SpriteBatch spriteBatch)
        {
            var renderableEntities = entityManager.GetAllEntitiesWithComponents<TransformComponent, SpriteComponent>();
            var sortedEntities = renderableEntities.OrderBy(e => e.GetComponent<TransformComponent>().Position.Y).ToList();

            foreach (var entity in sortedEntities) // Desenează în ordinea sortată
            {
                var transform = entity.GetComponent<TransformComponent>();
                var sprite = entity.GetComponent<SpriteComponent>();
                var animation = entity.GetComponent<AnimationComponent>();
                sprite.Draw(spriteBatch, transform, animation);
            }
        }

        public void DrawColliders(EntityManager entityManager, SpriteBatch spriteBatch)
        {
            if (!Settings.DebugShowColliders || _pixelTexture == null) return;
            var collidableEntities = entityManager.GetAllEntitiesWithComponents<TransformComponent, ColliderComponent>();
            foreach (var entity in collidableEntities)
            {
                var transform = entity.GetComponent<TransformComponent>();
                var collider = entity.GetComponent<ColliderComponent>();
                Rectangle worldBounds = collider.GetWorldBounds(transform);
                Color colliderColor = collider.IsSolid ? Color.Red : Color.Yellow;
                spriteBatch.Draw(_pixelTexture, new Rectangle(worldBounds.X, worldBounds.Y, worldBounds.Width, 1), colliderColor);
                spriteBatch.Draw(_pixelTexture, new Rectangle(worldBounds.X, worldBounds.Bottom - 1, worldBounds.Width, 1), colliderColor);
                spriteBatch.Draw(_pixelTexture, new Rectangle(worldBounds.X, worldBounds.Y, 1, worldBounds.Height), colliderColor);
                spriteBatch.Draw(_pixelTexture, new Rectangle(worldBounds.Right - 1, worldBounds.Y, 1, worldBounds.Height), colliderColor);
                spriteBatch.Draw(_pixelTexture, new Rectangle((int)transform.Position.X - 2, (int)transform.Position.Y - 2, 4, 4), Color.Cyan);
                Vector2 colliderCenter = transform.Position + collider.Offset;
                spriteBatch.Draw(_pixelTexture, new Rectangle((int)colliderCenter.X - 1, (int)colliderCenter.Y - 1, 2, 2), Color.Magenta);
            }
        }

        public void DrawLightingEffects(SpriteBatch spriteBatch, Camera camera)
        {
            if (_lightSystem == null || _pixelTexture == null || _lightMaskTexture == null) return;

            float globalAmbient = _lightSystem.GlobalAmbientLight;

            if (globalAmbient < 0.99f) // Aplică efecte doar dacă nu e zi plină
            {
                Color darknessColor = Color.Black * (1.0f - globalAmbient);
                spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, camera.Viewport.Width, camera.Viewport.Height), null, darknessColor, 0f, Vector2.Zero, SpriteEffects.None, 0.99f);

                var currentBlendState = _graphicsDevice.BlendState;
                _graphicsDevice.BlendState = BlendState.Additive;

                foreach (var emitterEntity in _lightSystem.GetActiveLightEmitters())
                {
                    var lightComp = emitterEntity.GetComponent<LightEmitterComponent>();
                    if (!lightComp.IsActive) continue;

                    var transformComp = emitterEntity.GetComponent<TransformComponent>();
                    Vector2 lightScreenPos = camera.WorldToScreen(transformComp.Position).ToVector2();
                    float scaledDiameter = lightComp.LightRadius * 2f * camera.Zoom;

                    float baseIntensity = lightComp.LightIntensity + lightComp.CurrentFlickerOffset;
                    baseIntensity = MathHelper.Clamp(baseIntensity, 0f, 1f);

                    float lightVisibilityFactor = MathHelper.Clamp(1.0f - (globalAmbient / 0.8f), 0f, 1f);
                    float finalIntensity = baseIntensity * lightVisibilityFactor;


                    Color finalLightColor = lightComp.LightColor * finalIntensity;

                    spriteBatch.Draw(
                        _lightMaskTexture,
                        new Rectangle((int)(lightScreenPos.X), (int)(lightScreenPos.Y), (int)scaledDiameter, (int)scaledDiameter),
                        null,
                        finalLightColor,
                        0f,
                        new Vector2(_lightMaskTexture.Width / 2f, _lightMaskTexture.Height / 2f),
                        SpriteEffects.None,
                        1f
                    );
                }
                _graphicsDevice.BlendState = currentBlendState;
            }
        }

        public void DrawUI(UIManager uiManager, SpriteBatch spriteBatch)
        {
            uiManager?.Draw(spriteBatch);
        }
    }
}