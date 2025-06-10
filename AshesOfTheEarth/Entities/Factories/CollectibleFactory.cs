using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Utils;
using System;
using AshesOfTheEarth.Entities;

namespace AshesOfTheEarth.Entities.Factories
{
    public class CollectibleFactory : IEntityFactory
    {
        private ObjectPool<Entity> _collectiblePool;
        private EntityManager _entityManager;
        private const int DEFAULT_POOL_SIZE = 50;
        private const int MAX_POOL_SIZE = 200;

        public CollectibleFactory()
        {
        }

        public void InitializePool(int initialSize = DEFAULT_POOL_SIZE, int maxSize = MAX_POOL_SIZE)
        {
            if (_collectiblePool != null) return;
            _entityManager = ServiceLocator.Get<EntityManager>();
            if (_entityManager == null)
            {
                throw new InvalidOperationException("EntityManager not found in ServiceLocator. CollectibleFactory cannot initialize pool.");
            }

            Func<Entity> factoryFunc = CreatePooledCollectibleTemplate;
            Action<Entity> resetAction = ResetCollectible;
            Action<Entity> returnAction = DeactivateAndStoreCollectible;
            _collectiblePool = new ObjectPool<Entity>(factoryFunc, resetAction, returnAction, initialSize, maxSize);
        }

        private Entity CreatePooledCollectibleTemplate()
        {
            Entity collectible = new Entity("PooledCollectible");
            collectible.AddComponent(new TransformComponent());
            collectible.AddComponent(new SpriteComponent());
            collectible.AddComponent(new ColliderComponent(new Rectangle(0, 0, 16, 16), Vector2.Zero, false));
            collectible.AddComponent(new CollectibleComponent(ItemType.None, 0));
            collectible.IsActive = false;
            return collectible;
        }

        private void ResetCollectible(Entity entity)
        {
            entity.IsActive = true;
            var spriteComp = entity.GetComponent<SpriteComponent>();
            if (spriteComp != null)
            {
                spriteComp.Color = Color.White;
                spriteComp.Effects = SpriteEffects.None;
            }
            var transform = entity.GetComponent<TransformComponent>();
            if (transform != null)
            {
                transform.Rotation = 0f;
            }
        }

        private void DeactivateAndStoreCollectible(Entity entity)
        {
            entity.IsActive = false;
            if (_entityManager.GetEntity(entity.Id) != null)
            {
                _entityManager.RemoveEntity(entity);
            }
        }

        public Entity CreateEntity(Vector2 position)
        {
            return CreateCollectible(position, ItemType.WoodLog, 1);
        }

        public Entity CreateCollectible(Vector2 position, ItemType itemType, int quantity)
        {
            if (_collectiblePool == null)
            {
                InitializePool();
            }

            ItemData itemData = ItemRegistry.GetData(itemType);
            if (itemData == null || itemData.Icon == null)
            {
                return null;
            }

            Entity collectible = _collectiblePool.Get();
            if (collectible == null)
            {
                return null;
            }

            collectible.Tag = $"Collectible_{itemType}";

            var transform = collectible.GetComponent<TransformComponent>();
            transform.Position = position;
            float targetDisplayWidthInWorld = Settings.WorldTileWidth * 0.6f;
            float scaleFactor = 1.0f;

            if (itemData.Icon.Width > 0)
            {
                scaleFactor = targetDisplayWidthInWorld / itemData.Icon.Width;
            }
            else if (itemData.Icon.Height > 0)
            {
                float targetDisplayHeightInWorld = Settings.WorldTileHeight * 0.6f;
                scaleFactor = targetDisplayHeightInWorld / itemData.Icon.Height;
            }
            scaleFactor = MathHelper.Clamp(scaleFactor, 0.1f, 3.0f);
            transform.Scale = Vector2.One * scaleFactor;

            var spriteComp = collectible.GetComponent<SpriteComponent>();
            spriteComp.Texture = itemData.Icon;
            spriteComp.Origin = new Vector2(itemData.Icon.Width / 2f, itemData.Icon.Height / 2f);
            spriteComp.LayerDepth = 0.45f + (position.Y / (Settings.DefaultWorldHeight * Settings.WorldTileHeight * 2f));

            var collectibleComp = collectible.GetComponent<CollectibleComponent>();
            collectibleComp.Initialize(itemType, quantity);

            var colliderComp = collectible.GetComponent<ColliderComponent>();
            float scaledIconWidth = itemData.Icon.Width * transform.Scale.X;
            float scaledIconHeight = itemData.Icon.Height * transform.Scale.Y;
            float colliderEffectiveSize = Math.Max(scaledIconWidth, scaledIconHeight) * 0.95f;
            colliderComp.Bounds = new Rectangle(0, 0, (int)colliderEffectiveSize, (int)colliderEffectiveSize);
            colliderComp.Offset = Vector2.Zero;
            colliderComp.IsSolid = false;

            ResetCollectible(collectible);

            return collectible;
        }

        public void ReturnCollectibleToPool(Entity collectibleEntity)
        {
            if (collectibleEntity == null || _collectiblePool == null) return;
            _collectiblePool.Return(collectibleEntity);
        }
    }
}