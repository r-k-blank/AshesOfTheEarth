using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using AshesOfTheEarth.World;
using Microsoft.Xna.Framework;
using System.Linq;

namespace AshesOfTheEarth.Gameplay.Placement
{
    public class PlacementValidator : IPlacementValidator
    {
        private readonly WorldManager _worldManager;
        private readonly EntityManager _entityManager;
        private readonly Core.Validation.IPositionValidator _positionValidator;

        public PlacementValidator()
        {
            _worldManager = ServiceLocator.Get<WorldManager>();
            _entityManager = ServiceLocator.Get<EntityManager>();
            _positionValidator = ServiceLocator.Get<Core.Validation.IPositionValidator>();
        }

        public ItemData GetItemDataForPlacement(ItemType itemType)
        {
            return ItemRegistry.GetData(itemType);
        }

        public bool IsPlacementValid(ItemType itemToPlace, Vector2 worldPosition, Entity placer)
        {
            ItemData itemData = GetItemDataForPlacement(itemToPlace);
            if (itemData == null || itemData.Category != ItemCategory.Placeable || string.IsNullOrEmpty(itemData.EntityToPlaceTag))
            {
                return false;
            }

            if (_worldManager.TileMap == null) return false;

            int tileX = (int)(worldPosition.X / _worldManager.TileMap.TileWidth);
            int tileY = (int)(worldPosition.Y / _worldManager.TileMap.TileHeight);

            Tile mapTile = _worldManager.TileMap.GetTile(tileX, tileY);
            if (!mapTile.IsWalkable) // Condiție 1
            {
                System.Diagnostics.Debug.WriteLine($"[PlacementValidator] Invalid: Tile ({tileX},{tileY}) not walkable for {itemToPlace}. TileType: {mapTile.Type}");
                return false;
            }

            Entity templateEntity = CreateTemplateEntity(itemData, worldPosition);
            if (templateEntity == null) // Condiție 2
            {
                System.Diagnostics.Debug.WriteLine($"[PlacementValidator] Invalid: Template entity for {itemToPlace} is null.");
                return false;
            }

            if (!_positionValidator.IsPositionSafe(templateEntity)) // Condiție 3
            {
                System.Diagnostics.Debug.WriteLine($"[PlacementValidator] Invalid: Position for {itemToPlace} at {worldPosition} not safe (collision).");
                return false;
            }
            System.Diagnostics.Debug.WriteLine($"[PlacementValidator] Valid: Position for {itemToPlace} at {worldPosition} IS safe.");
            return true;


            return true;
        }

        private Entity CreateTemplateEntity(ItemData itemData, Vector2 position)
        {

            if (itemData.EntityToPlaceTag == "Campfire")
            {
                var factory = ServiceLocator.Get<Entities.Factories.CampfireFactory>();
                return factory?.CreateEntity(position);
            }
            return null;
        }
    }
}