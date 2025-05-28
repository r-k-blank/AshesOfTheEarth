using AshesOfTheEarth.Gameplay.Items;

namespace AshesOfTheEarth.Entities.Components
{
    public class CollectibleComponent : IComponent
    {
        public ItemType ItemToCollect { get; }
        public int Quantity { get; }

        public CollectibleComponent(ItemType itemType, int quantity)
        {
            ItemToCollect = itemType;
            Quantity = quantity;
        }
    }
}