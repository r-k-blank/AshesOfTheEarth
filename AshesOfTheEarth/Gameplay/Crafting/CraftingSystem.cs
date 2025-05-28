using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using System.Collections.Generic;
using System.Linq;
using AshesOfTheEarth.Core.Services; // For TimeManager if crafting takes time

namespace AshesOfTheEarth.Gameplay.Crafting
{
    public class CraftingSystem
    {
        private List<Recipe> _allRecipes;

        public CraftingSystem()
        {
            _allRecipes = LoadAllRecipes();
        }

        private List<Recipe> LoadAllRecipes()
        {
            var recipes = new List<Recipe>
            {
                new Recipe("WoodenStick", ItemType.WoodenStick, 2, new Dictionary<ItemType, int> { { ItemType.WoodLog, 1 } }),
                new Recipe("WoodenPickaxe", ItemType.WoodenPickaxe, 1, new Dictionary<ItemType, int> { { ItemType.WoodenStick, 3 }, { ItemType.WoodLog, 2 } }),
                new Recipe("StoneAxe", ItemType.StoneAxe, 1, new Dictionary<ItemType, int> { { ItemType.WoodenStick, 2 }, { ItemType.StoneShard, 3 }, { ItemType.WoodLog, 1 } }),
                new Recipe("Campfire", ItemType.Campfire, 1, new Dictionary<ItemType, int> { { ItemType.WoodLog, 5 }, { ItemType.StoneShard, 3 } }),
                new Recipe("SharpenedStone", ItemType.SharpenedStone, 1, new Dictionary<ItemType, int> { { ItemType.StoneShard, 2 }, { ItemType.Flint, 1 } }),
            };
            System.Diagnostics.Debug.WriteLine($"CraftingSystem: Loaded {recipes.Count} recipes.");
            return recipes;
        }

        public List<Recipe> GetAllRecipes()
        {
            return new List<Recipe>(_allRecipes);
        }

        public bool CanCraft(Recipe recipe, InventoryComponent playerInventory, string currentCraftingStationId = null)
        {
            if (recipe == null || playerInventory == null) return false;

            if (recipe.RequiredCraftingStationId != null && recipe.RequiredCraftingStationId != currentCraftingStationId)
            {
                return false;
            }

            foreach (var ingredient in recipe.RequiredIngredients)
            {
                if (!playerInventory.HasItem(ingredient.Key, ingredient.Value))
                {
                    return false;
                }
            }
            return true;
        }

        public bool TryCraftItem(Recipe recipe, InventoryComponent playerInventory, string currentCraftingStationId = null)
        {
            if (!CanCraft(recipe, playerInventory, currentCraftingStationId))
            {
                System.Diagnostics.Debug.WriteLine($"CraftingSystem: Cannot craft {recipe.OutputItemData.Name}. Missing ingredients or station.");
                return false;
            }

            foreach (var ingredient in recipe.RequiredIngredients)
            {
                playerInventory.RemoveItem(ingredient.Key, ingredient.Value);
            }

            bool added = playerInventory.AddItem(recipe.OutputItem, recipe.OutputQuantity);
            if (!added)
            {
                System.Diagnostics.Debug.WriteLine($"CraftingSystem: Crafted {recipe.OutputItemData.Name}, but could not add to inventory (full?). Item may be lost or needs drop logic.");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"CraftingSystem: Successfully crafted {recipe.OutputQuantity}x {recipe.OutputItemData.Name}.");
            return true;
        }
        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
        }
    }
}