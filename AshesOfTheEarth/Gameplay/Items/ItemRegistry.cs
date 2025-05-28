using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using AshesOfTheEarth.Core.Services;
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Gameplay.Items
{
    public static class ItemRegistry
    {
        private static readonly Dictionary<ItemType, ItemData> _itemDataMap = new Dictionary<ItemType, ItemData>();
        private static bool _isInitialized = false;
        private static ContentManager _content;

        public static void Initialize(ContentManager content)
        {
            if (_isInitialized) return;
            _content = content;

            RegisterItem(new ItemData(ItemType.WoodLog, "Wood Log", "A sturdy log of wood.", ItemCategory.Resource, 50), "Items/wood_log_icon");
            RegisterItem(new ItemData(ItemType.StoneShard, "Stone Shard", "A sharp piece of rock.", ItemCategory.Resource, 100), "Items/stone_shard_icon");
            RegisterItem(new ItemData(ItemType.Flint, "Flint", "A piece of flint, good for sparks.", ItemCategory.Resource, 50), "Items/flint_icon");

            RegisterItem(new ItemData(ItemType.Berries, "Berries", "Sweet and juicy berries.",
                                      ItemCategory.Consumable, 20,
                                      healthGain: 5f, hungerReduction: 10f), "Items/berries_icon");

            RegisterItem(new ItemData(ItemType.Coal, "Coal", "A lump of coal.", ItemCategory.Resource, 50), "Items/coal_icon");
            RegisterItem(new ItemData(ItemType.IronOre, "Iron Ore", "Raw iron ore.", ItemCategory.Resource, 30), "Items/iron_ore_icon");
            RegisterItem(new ItemData(ItemType.GoldOre, "Gold Ore", "Shiny gold ore.", ItemCategory.Resource, 20), "Items/gold_ore_icon");
            RegisterItem(new ItemData(ItemType.CrystalShard, "Crystal Shard", "A glowing crystal fragment.", ItemCategory.Resource, 20), "Items/crystal_icon");
            RegisterItem(new ItemData(ItemType.HerbLeaf, "Herb Leaf", "A medicinal herb leaf.", ItemCategory.CraftingMaterial, 30), "Items/herb_leaf_icon");
            RegisterItem(new ItemData(ItemType.WoodenStick, "Wooden Stick", "A simple wooden stick.", ItemCategory.CraftingMaterial, 50), "Items/stick_icon");

            RegisterItem(new ItemData(ItemType.WoodenPickaxe, "Wooden Pickaxe", "A basic pickaxe made of wood.", ItemCategory.Tool, 1, "Pickaxe", 1.5f, 3f), "Items/wooden_pickaxe_icon");
            RegisterItem(new ItemData(ItemType.StoneAxe, "Stone Axe", "A crude axe with a stone head.", ItemCategory.Tool, 1, "Axe", 2f, 5f), "Items/stone_axe_icon");
            RegisterItem(new ItemData(ItemType.WoodenShovel, "Wooden Shovel", "Used for digging softer materials.", ItemCategory.Tool, 1, "Shovel", 1.0f), "Items/wooden_shovel_icon");

            RegisterItem(new ItemData(ItemType.Campfire, "Campfire Kit", "Used to place a campfire.", ItemCategory.Placeable, 1, entityToPlaceTag: "Campfire"), "Items/campfire_icon");
            RegisterItem(new ItemData(ItemType.SharpenedStone, "Sharpened Stone", "A stone sharpened to a point. Better than fists.", ItemCategory.Weapon, 1, "Hand", 1f, 2f), "Items/sharpened_stone_icon");

            RegisterItem(new ItemData(ItemType.CookedMeat, "Cooked Meat", "Restores significant health and reduces hunger.",
                                      ItemCategory.Consumable, 10,
                                      healthGain: 25f, hungerReduction: 40f, staminaGain: 10f), "Items/cooked_meat_icon");
            RegisterItem(new ItemData(ItemType.HealingPotionSmall, "Small Healing Potion", "A weak potion that restores some health.",
                                      ItemCategory.Consumable, 5,
                                      healthGain: 30f), "Items/healing_potion_small_icon");
            RegisterItem(new ItemData(ItemType.Torch, "Torch", "Provides light in the darkness. Consumes slowly.", ItemCategory.Tool, 1, toolType: "LightSource"), "Items/torch_icon");


            RegisterItem(new ItemData(ItemType.Bone, "Bone", "A sturdy bone.", ItemCategory.CraftingMaterial, 50), "Items/bone_icon");
            RegisterItem(new ItemData(ItemType.ArrowShaft, "Arrow Shaft", "A smooth wooden shaft for an arrow.", ItemCategory.CraftingMaterial, 50), "Items/arrow_shaft_icon");
            RegisterItem(new ItemData(ItemType.MinotaurHorn, "Minotaur Horn", "A large, sharp horn.", ItemCategory.CraftingMaterial, 10), "Items/minotaur_horn_icon");
            RegisterItem(new ItemData(ItemType.ToughLeather, "Tough Leather", "Thick and durable leather.", ItemCategory.CraftingMaterial, 20), "Items/tough_leather_icon");
            RegisterItem(new ItemData(ItemType.GargoyleShard, "Gargoyle Shard", "A fragment of enchanted stone.", ItemCategory.CraftingMaterial, 30), "Items/gargoyle_shard_icon");
            RegisterItem(new ItemData(ItemType.WerewolfPelt, "Werewolf Pelt", "The thick fur of a werewolf.", ItemCategory.CraftingMaterial, 10), "Items/werewolf_pelt_icon");
            RegisterItem(new ItemData(ItemType.RawMeat, "Raw Meat", "Needs to be cooked. Slightly reduces hunger if eaten raw.", ItemCategory.Consumable, 20, healthGain: -5f, hungerReduction: 5f), "Items/raw_meat_icon");
            RegisterItem(new ItemData(ItemType.SmallMeat, "Small Raw Meat", "A small piece of raw meat. Barely edible.", ItemCategory.Consumable, 30, healthGain: -2f, hungerReduction: 3f), "Items/small_meat_icon");
            RegisterItem(new ItemData(ItemType.AnimalHide, "Animal Hide", "A common animal hide.", ItemCategory.CraftingMaterial, 25), "Items/animal_hide_icon");
            RegisterItem(new ItemData(ItemType.MysticEssence, "Mystic Essence", "Pulsating with faint magical energy.", ItemCategory.CraftingMaterial, 15), "Items/mystic_essence_icon");


            _isInitialized = true;
        }

        private static void RegisterItem(ItemData data, string iconPath = null)
        {
            if (!_itemDataMap.ContainsKey(data.Type))
            {
                if (!string.IsNullOrEmpty(iconPath) && _content != null)
                {
                    try
                    {
                        Texture2D icon = _content.Load<Texture2D>(Path.Combine("Sprites", iconPath));
                        data.SetIcon(icon);
                    }
                    catch (System.Exception)
                    {
                        Texture2D fallbackIcon = ServiceLocator.Get<Texture2D>();
                        if (fallbackIcon == null)
                        {
                            fallbackIcon = new Texture2D(ServiceLocator.Get<GraphicsDevice>(), 32, 32);
                            Color[] cdata = new Color[32 * 32];
                            for (int i = 0; i < cdata.Length; ++i) cdata[i] = Color.Magenta;
                            fallbackIcon.SetData(cdata);
                        }
                        data.SetIcon(fallbackIcon);
                    }
                }
                _itemDataMap.Add(data.Type, data);
            }
        }

        public static ItemData GetData(ItemType type)
        {
            if (!_isInitialized)
            {
                return GetFallbackData(type);
            }
            if (_itemDataMap.TryGetValue(type, out ItemData data))
            {
                return data;
            }
            return GetFallbackData(type);
        }

        private static ItemData GetFallbackData(ItemType type)
        {
            var fallbackIcon = ServiceLocator.Get<Texture2D>();
            if (fallbackIcon == null)
            {
                fallbackIcon = new Texture2D(ServiceLocator.Get<GraphicsDevice>(), 32, 32);
                Color[] cdata = new Color[32 * 32];
                for (int i = 0; i < cdata.Length; ++i) cdata[i] = Color.Magenta;
                fallbackIcon.SetData(cdata);
            }


            var fd = new ItemData(type, type.ToString() + " (Missing)", "This item is missing from the registry.", ItemCategory.None, 1);
            fd.SetIcon(fallbackIcon);
            return fd;
        }

        public static IEnumerable<ItemData> GetAllItemData() => _itemDataMap.Values;
    }
}