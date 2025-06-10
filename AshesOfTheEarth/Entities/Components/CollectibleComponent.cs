using AshesOfTheEarth.Gameplay.Items;

namespace AshesOfTheEarth.Entities.Components
{
    public class CollectibleComponent : IComponent
    {
        public ItemType ItemToCollect { get; private set; }
        public int Quantity { get; private set; }

        public CollectibleComponent(ItemType itemType, int quantity)
        {
            ItemToCollect = itemType;
            Quantity = quantity;
        }

        public void Initialize(ItemType itemType, int quantity)
        {
            ItemToCollect = itemType;
            Quantity = quantity;
        }
    }
}