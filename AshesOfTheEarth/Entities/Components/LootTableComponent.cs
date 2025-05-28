using AshesOfTheEarth.Gameplay.Items;
using System.Collections.Generic;

namespace AshesOfTheEarth.Entities.Components
{
    public class LootDropInfo
    {
        public ItemType Item { get; }
        public int MinAmount { get; }
        public int MaxAmount { get; }
        public float Chance { get; } // 0.0 to 1.0

        public LootDropInfo(ItemType item, int min, int max, float chance = 1.0f)
        {
            Item = item;
            MinAmount = min;
            MaxAmount = max;
            Chance = System.Math.Clamp(chance, 0.0f, 1.0f);
        }
    }

    public class LootTableComponent : IComponent
    {
        public List<LootDropInfo> PossibleDrops { get; private set; }

        public LootTableComponent(List<LootDropInfo> drops)
        {
            PossibleDrops = drops ?? new List<LootDropInfo>();
        }
    }
}