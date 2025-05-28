using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;
using AshesOfTheEarth.Core.Input.Command.Templates;
using AshesOfTheEarth.Core.Services;

namespace AshesOfTheEarth.Core.Input.Command
{
    public class ConsumeItemCommand : AbstractGameCommand
    {
        private readonly int _slotIndex;

        public ConsumeItemCommand(int slotIndex)
        {
            _slotIndex = slotIndex;
        }

        protected override bool AllowsExecutionInInventory() => true;

        protected override bool CanExecuteGameplayConditions(Entity entity, UIManager uiManager, GameTime gameTime)
        {
            var inventory = entity.GetComponent<InventoryComponent>();
            if (inventory == null || _slotIndex < 0 || _slotIndex >= inventory.Items.Count)
            {
                return false;
            }
            var itemStack = inventory.Items[_slotIndex];
            return itemStack.Type != ItemType.None && itemStack.Quantity > 0 && itemStack.Data?.Category == ItemCategory.Consumable;
        }

        protected override void PerformAction(Entity entity, GameTime gameTime)
        {
            var inventory = entity.GetComponent<InventoryComponent>();
            var healthComp = entity.GetComponent<HealthComponent>();
            var statsComp = entity.GetComponent<StatsComponent>();

            if (inventory == null || _slotIndex < 0 || _slotIndex >= inventory.Items.Count) return;

            ItemStack itemStack = inventory.Items[_slotIndex];
            if (itemStack.Type == ItemType.None || itemStack.Quantity <= 0 || itemStack.Data == null) return;

            ItemData itemData = itemStack.Data;

            bool consumed = false;
            if (itemData.HealthGain != 0 && healthComp != null)
            {
                healthComp.Heal(itemData.HealthGain);
                consumed = true;
            }
            if (itemData.HungerReduction != 0 && statsComp != null)
            {
                statsComp.DecreaseHunger(itemData.HungerReduction);
                consumed = true;
            }
            if (itemData.StaminaGain != 0 && statsComp != null)
            {
                statsComp.RegenStamina(itemData.StaminaGain);
                consumed = true;
            }

            if (consumed)
            {
                inventory.RemoveItem(itemStack.Type, 1);
                ServiceLocator.Get<UIManager>()?.GetInventoryScreen()?.RefreshInventoryLinks();
            }
        }

        protected override void OnGameplayConditionFailed(Entity entity, GameTime gameTime)
        {
            System.Diagnostics.Debug.WriteLine($"ConsumeItemCommand: Gameplay conditions failed for slot {_slotIndex}.");
        }
    }
}