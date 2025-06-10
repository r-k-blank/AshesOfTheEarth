using AshesOfTheEarth.Gameplay.Items;
using System.Collections.Generic;
using System.Linq;

namespace AshesOfTheEarth.Entities.Components
{
    public class ItemStack
    {
        public ItemType Type { get; internal set; } // Made internal set for direct restore
        public int Quantity { get; set; }
        public ItemData Data => ItemRegistry.GetData(Type);

        public ItemStack(ItemType type, int quantity)
        {
            Type = type;
            Quantity = quantity;
            if (Data == null && type != ItemType.None)
            {
                //System.Diagnostics.Debug.WriteLine($"Warning: ItemStack created for {type} but ItemData is null in registry.");
            }
        }
    }

    public class InventoryComponent : IComponent
    {
        // Made public set for easier restore, but use with caution
        public List<ItemStack> Items { get; set; }
        public int Capacity { get; private set; }

        public InventoryComponent(int capacity = 16)
        {
            Capacity = capacity;
            Items = new List<ItemStack>(capacity);
            InitializeEmptySlots();
        }

        public void InitializeEmptySlots()
        {
            Items.Clear();
            for (int i = 0; i < Capacity; i++)
            {
                Items.Add(new ItemStack(ItemType.None, 0));
            }
        }


        public bool AddItem(ItemType itemType, int quantityToAdd)
        {
            if (quantityToAdd <= 0 || itemType == ItemType.None) return false;

            ItemData data = ItemRegistry.GetData(itemType);
            if (data == null)
            {
                //System.Diagnostics.Debug.WriteLine($"Cannot add item: {itemType} not found in ItemRegistry.");
                return false;
            }

            foreach (var stack in Items.Where(s => s.Type == itemType))
            {
                if (stack.Quantity < data.MaxStackSize)
                {
                    int canAdd = data.MaxStackSize - stack.Quantity;
                    int toAddNow = System.Math.Min(quantityToAdd, canAdd);
                    stack.Quantity += toAddNow;
                    quantityToAdd -= toAddNow;
                    if (quantityToAdd <= 0) return true;
                }
            }

            if (quantityToAdd > 0)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Type == ItemType.None)
                    {
                        int toAddNow = System.Math.Min(quantityToAdd, data.MaxStackSize);
                        // Directly modify the existing empty stack object
                        Items[i].Type = itemType;
                        Items[i].Quantity = toAddNow;
                        // Items[i] = new ItemStack(itemType, toAddNow); // Old way
                        quantityToAdd -= toAddNow;
                        if (quantityToAdd <= 0) return true;
                    }
                }
            }

            if (quantityToAdd > 0)
            {
                //System.Diagnostics.Debug.WriteLine($"Inventory full. Could not add {quantityToAdd} of {itemType}.");
                return false;
            }
            return true;
        }

        public bool RemoveItem(ItemType itemType, int quantityToRemove)
        {
            if (quantityToRemove <= 0 || itemType == ItemType.None) return false;

            int totalRemoved = 0;
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                if (Items[i].Type == itemType)
                {
                    int canRemoveFromStack = Items[i].Quantity;
                    int toRemoveNow = System.Math.Min(quantityToRemove - totalRemoved, canRemoveFromStack);

                    Items[i].Quantity -= toRemoveNow;
                    totalRemoved += toRemoveNow;

                    if (Items[i].Quantity <= 0)
                    {
                        // Reset the existing stack object to None
                        Items[i].Type = ItemType.None;
                        Items[i].Quantity = 0;
                        // Items[i] = new ItemStack(ItemType.None, 0); // Old way
                    }

                    if (totalRemoved >= quantityToRemove) break;
                }
            }
            return totalRemoved >= quantityToRemove;
        }

        public int GetItemCount(ItemType itemType)
        {
            return Items.Where(s => s.Type == itemType).Sum(s => s.Quantity);
        }

        public bool HasItem(ItemType itemType, int quantity = 1)
        {
            return GetItemCount(itemType) >= quantity;
        }

        public void DebugPrintInventory()
        {
            System.Diagnostics.Debug.WriteLine("--- Inventory ---");
            foreach (var stack in Items.Where(s => s.Type != ItemType.None))
            {
                System.Diagnostics.Debug.WriteLine($"{stack.Data?.Name ?? stack.Type.ToString()}: {stack.Quantity}");
            }
            System.Diagnostics.Debug.WriteLine("-----------------");
        }
    }
}