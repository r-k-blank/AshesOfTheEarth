using AshesOfTheEarth.Gameplay.Items;
using System.Collections.Generic;

namespace AshesOfTheEarth.Gameplay.Crafting
{
    public class Recipe
    {
        public string RecipeId { get; }
        public ItemType OutputItem { get; }
        public int OutputQuantity { get; }
        public Dictionary<ItemType, int> RequiredIngredients { get; }
        public float CraftingTimeSeconds { get; }
        public string RequiredCraftingStationId { get; }
        public ItemData OutputItemData => ItemRegistry.GetData(OutputItem);

        public Recipe(string recipeId, ItemType outputItem, int outputQuantity, Dictionary<ItemType, int> requiredIngredients, float craftingTimeSeconds = 0f, string requiredCraftingStationId = null)
        {
            RecipeId = recipeId;
            OutputItem = outputItem;
            OutputQuantity = outputQuantity;
            RequiredIngredients = requiredIngredients ?? new Dictionary<ItemType, int>();
            CraftingTimeSeconds = craftingTimeSeconds;
            RequiredCraftingStationId = requiredCraftingStationId;
        }
    }
}