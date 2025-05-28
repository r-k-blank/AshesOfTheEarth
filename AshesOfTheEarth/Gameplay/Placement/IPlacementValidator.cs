using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Gameplay.Placement
{
    public interface IPlacementValidator
    {
        bool IsPlacementValid(ItemType itemToPlace, Vector2 worldPosition, Entity placer);
        ItemData GetItemDataForPlacement(ItemType itemType);
    }
}