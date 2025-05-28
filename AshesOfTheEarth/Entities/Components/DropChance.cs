using AshesOfTheEarth.Gameplay.Items;
using System.Collections.Generic; // Pentru Dictionary

namespace AshesOfTheEarth.Entities.Components
{
    public class DropChance
    {
        public ItemType Item { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
        public float Chance { get; set; } // 0.0 to 1.0

        public DropChance(ItemType item, int min, int max, float chance = 1.0f)
        {
            Item = item;
            MinAmount = min;
            MaxAmount = max;
            Chance = chance;
        }
    }
}