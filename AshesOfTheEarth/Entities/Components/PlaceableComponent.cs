using AshesOfTheEarth.Gameplay.Items;

namespace AshesOfTheEarth.Entities.Components
{
    public class PlaceableComponent : IComponent
    {
        public ItemType OriginalItemType { get; }
        public ulong PlacedByEntityId { get; }

        public PlaceableComponent(ItemType originalItemType, ulong placerId)
        {
            OriginalItemType = originalItemType;
            PlacedByEntityId = placerId;
        }
    }
}