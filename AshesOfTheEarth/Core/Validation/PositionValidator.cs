// Core/Validation/PositionValidator.cs
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.World;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using AshesOfTheEarth.Core.Services;

namespace AshesOfTheEarth.Core.Validation
{
    public class PositionValidator : IPositionValidator
    {
        private WorldManager _worldManager;
        private EntityManager _entityManager;
        private readonly Random _random = new Random();

        private WorldManager WorldManagerInstance => _worldManager ??= ServiceLocator.Get<WorldManager>();
        private EntityManager EntityManagerInstance => _entityManager ??= ServiceLocator.Get<EntityManager>();

        public PositionValidator()
        {
        }

        public bool IsPositionSafe(Entity entityProspect)
        {
            if (entityProspect == null) return false;

            if (WorldManagerInstance == null || EntityManagerInstance == null)
            {
                return false;
            }

            var prospectTransform = entityProspect.GetComponent<TransformComponent>();
            var prospectCollider = entityProspect.GetComponent<ColliderComponent>();

            if (prospectTransform == null) return true;


            if (!WorldManagerInstance.IsPositionWalkable(prospectTransform.Position))
            {
                return false;
            }

            if (prospectCollider == null) return true;


            Rectangle prospectiveWorldBounds = prospectCollider.GetWorldBounds(prospectTransform);

            var solidEntities = EntityManagerInstance.GetAllEntitiesWithComponents<TransformComponent, ColliderComponent>()
                                             .Where(e => e.Id != entityProspect.Id &&
                                                         e.GetComponent<ColliderComponent>().IsSolid);

            foreach (var solidEntity in solidEntities)
            {
                var solidTransform = solidEntity.GetComponent<TransformComponent>();
                var solidCollider = solidEntity.GetComponent<ColliderComponent>();

                if (prospectiveWorldBounds.Intersects(solidCollider.GetWorldBounds(solidTransform)))
                {
                    return false;
                }
            }
            return true;
        }

        public Vector2 FindSafeSpawnPositionNearby(Entity entityProspectTemplate, Vector2 desiredPosition, float searchRadius, int maxAttempts = 20)
        {
            if (entityProspectTemplate == null) return desiredPosition;

            if (WorldManagerInstance == null)
            {
                return desiredPosition;
            }

            var templateTransform = entityProspectTemplate.GetComponent<TransformComponent>();
            var templateCollider = entityProspectTemplate.GetComponent<ColliderComponent>();

            if (templateTransform == null) return desiredPosition;


            Vector2 originalTemplatePos = templateTransform.Position;
            templateTransform.Position = desiredPosition;

            if (IsPositionSafe(entityProspectTemplate))
            {
                templateTransform.Position = originalTemplatePos;
                return desiredPosition;
            }

            for (int i = 0; i < maxAttempts; i++)
            {
                float angle = (float)(_random.NextDouble() * 2 * Math.PI);
                float radiusFactor = (float)_random.NextDouble();
                float currentSearchRadius = searchRadius * radiusFactor * radiusFactor;

                Vector2 candidatePosition = desiredPosition + new Vector2((float)Math.Cos(angle) * currentSearchRadius, (float)Math.Sin(angle) * currentSearchRadius);

                if (WorldManagerInstance.TileMap != null && templateCollider != null)
                {
                    Rectangle worldBoundsForClamp = templateCollider.GetWorldBounds(templateTransform);
                    candidatePosition.X = MathHelper.Clamp(candidatePosition.X, worldBoundsForClamp.Width / 2f, WorldManagerInstance.TileMap.WidthInPixels - worldBoundsForClamp.Width / 2f);
                    candidatePosition.Y = MathHelper.Clamp(candidatePosition.Y, worldBoundsForClamp.Height / 2f, WorldManagerInstance.TileMap.HeightInPixels - worldBoundsForClamp.Height / 2f);
                }
                else if (WorldManagerInstance.TileMap != null)
                {
                    candidatePosition.X = MathHelper.Clamp(candidatePosition.X, 0, WorldManagerInstance.TileMap.WidthInPixels);
                    candidatePosition.Y = MathHelper.Clamp(candidatePosition.Y, 0, WorldManagerInstance.TileMap.HeightInPixels);
                }


                templateTransform.Position = candidatePosition;
                if (IsPositionSafe(entityProspectTemplate))
                {
                    templateTransform.Position = originalTemplatePos;
                    return candidatePosition;
                }
            }

            templateTransform.Position = originalTemplatePos;
            return desiredPosition;
        }
    }
}