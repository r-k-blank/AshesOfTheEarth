using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using AshesOfTheEarth.Entities.Components;


namespace AshesOfTheEarth.Core
{
    public class GameStateMemento
    {
        public int WorldSeed { get; set; }
        public int WorldWidth { get; set; }
        public int WorldHeight { get; set; }
        public TimeMemento TimeState { get; set; }
        public PlayerMemento PlayerState { get; set; }
        public List<EntityMemento> EntityStates { get; set; }

        public GameStateMemento()
        {
            EntityStates = new List<EntityMemento>();
        }
    }

    public class TimeMemento
    {
        public float TimeOfDayHours { get; set; }
        public int DayNumber { get; set; }
        public Core.Time.DayPhase CurrentDayPhase { get; set; }
    }

    public class ItemStackMemento
    {
        public ItemType Type { get; set; }
        public int Quantity { get; set; }

        public ItemStackMemento() { }

        public ItemStackMemento(ItemType type, int quantity)
        {
            Type = type;
            Quantity = quantity;
        }
        public static ItemStackMemento FromItemStack(ItemStack stack)
        {
            return new ItemStackMemento(stack.Type, stack.Quantity);
        }
        public ItemStack ToItemStack()
        {
            return new ItemStack(Type, Quantity);
        }
    }

    public class PlayerMemento
    {
        public ulong Id { get; set; }
        public Vector2 Position { get; set; }
        public float CurrentHealth { get; set; }
        public float CurrentHunger { get; set; }
        public float CurrentStamina { get; set; }
        public List<ItemStackMemento> InventoryItems { get; set; }
        public string SelectedCharacterSpriteSheetPath { get; set; } // NOU
        public string SelectedCharacterName { get; set; } // NOU
        public PlayerAnimationSetType SelectedCharacterAnimationType { get; set; }
        public int SelectedCharacterFrameWidth { get; set; } = 128;
        public int SelectedCharacterFrameHeight { get; set; } = 128;


        public PlayerMemento()
        {
            InventoryItems = new List<ItemStackMemento>();

        }
    }

    public class EntityMemento
    {
        public ulong Id { get; set; }
        public string Tag { get; set; }
        public Vector2 Position { get; set; }
        public float? CurrentHealth { get; set; }
        public float? MaxHealth { get; set; }
        public bool? IsDepleted { get; set; }
        public List<ItemStackMemento> InventoryItems { get; set; }

        public EntityMemento()
        {
            InventoryItems = new List<ItemStackMemento>();
        }
    }
}