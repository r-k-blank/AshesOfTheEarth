using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.Gameplay.Items;

namespace AshesOfTheEarth.Gameplay.Items
{
    public class ItemData
    {
        public ItemType Type { get; }
        public string Name { get; }
        public string Description { get; }
        public int MaxStackSize { get; }
        public Texture2D Icon { get; private set; }
        public ItemCategory Category { get; }
        public string ToolType { get; }
        public float ToolEffectiveness { get; }
        public float Damage { get; }
        public string EntityToPlaceTag { get; }
        public float HealthGain { get; }
        public float HungerReduction { get; }
        public float StaminaGain { get; }


        public ItemData(ItemType type, string name, string description,
                        ItemCategory category = ItemCategory.Resource,
                        int maxStackSize = 64,
                        string toolType = "None",
                        float toolEffectiveness = 1f,
                        float damage = 0f,
                        string entityToPlaceTag = null,
                        float healthGain = 0f,
                        float hungerReduction = 0f,
                        float staminaGain = 0f
                        )
        {
            Type = type;
            Name = name;
            Description = description;
            Category = category;
            MaxStackSize = maxStackSize;
            ToolType = toolType;
            ToolEffectiveness = toolEffectiveness;
            Damage = damage;
            EntityToPlaceTag = entityToPlaceTag;
            HealthGain = healthGain;
            HungerReduction = hungerReduction;
            StaminaGain = staminaGain;
        }

        public void SetIcon(Texture2D icon)
        {
            Icon = icon;
        }
    }
}