using AshesOfTheEarth.Entities.Components;
using System;
using System.Linq;
using System.Text;

namespace AshesOfTheEarth.Entities.Visitor
{
    public class EntityReportVisitor : IEntityVisitor
    {
        private StringBuilder _reportBuilder = new StringBuilder();

        public string GetReport()
        {
            return _reportBuilder.ToString();
        }

        public void VisitPlayer(Entity playerEntity)
        {
            _reportBuilder.AppendLine($"--- Player Report (ID: {playerEntity.Id}) ---");
            var transform = playerEntity.GetComponent<TransformComponent>();
            if (transform != null)
                _reportBuilder.AppendLine($"  Position: {transform.Position}");

            var health = playerEntity.GetComponent<HealthComponent>();
            if (health != null)
                _reportBuilder.AppendLine($"  Health: {health.CurrentHealth}/{health.MaxHealth}");

            var stats = playerEntity.GetComponent<StatsComponent>();
            if (stats != null)
            {
                _reportBuilder.AppendLine($"  Stamina: {stats.CurrentStamina}/{stats.MaxStamina}");
                _reportBuilder.AppendLine($"  Hunger: {stats.CurrentHunger}/{stats.MaxHunger}");
            }

            var inventory = playerEntity.GetComponent<InventoryComponent>();
            if (inventory != null)
            {
                int itemCount = inventory.Items.Count(s => s.Type != Gameplay.Items.ItemType.None);
                _reportBuilder.AppendLine($"  Inventory ({itemCount}/{inventory.Capacity}):");
                foreach (var itemStack in inventory.Items)
                {
                    if (itemStack.Type != Gameplay.Items.ItemType.None)
                    {
                        _reportBuilder.AppendLine($"    - {itemStack.Data?.Name ?? itemStack.Type.ToString()}: {itemStack.Quantity}");
                    }
                }
            }
            _reportBuilder.AppendLine("--- End Player Report ---");
        }

        public void VisitResourceNode(Entity resourceEntity)
        {
            _reportBuilder.AppendLine($"--- Resource Node Report (ID: {resourceEntity.Id}, Tag: {resourceEntity.Tag}) ---");
            var transform = resourceEntity.GetComponent<TransformComponent>();
            if (transform != null)
                _reportBuilder.AppendLine($"  Position: {transform.Position}");

            var rsc = resourceEntity.GetComponent<ResourceSourceComponent>();
            if (rsc != null)
            {
                _reportBuilder.AppendLine($"  Resource Name: {rsc.ResourceName}");
                _reportBuilder.AppendLine($"  Health: {rsc.Health}/{rsc.MaxHealth}");
                _reportBuilder.AppendLine($"  Depleted: {rsc.Depleted}");
                _reportBuilder.AppendLine($"  Required Tool: {rsc.RequiredToolCategory ?? "Any"}");
            }
            _reportBuilder.AppendLine("--- End Resource Node Report ---");
        }

        public void VisitGenericEntity(Entity genericEntity)
        {
            _reportBuilder.AppendLine($"--- Generic Entity Report (ID: {genericEntity.Id}, Tag: {genericEntity.Tag}) ---");
            var transform = genericEntity.GetComponent<TransformComponent>();
            if (transform != null)
                _reportBuilder.AppendLine($"  Position: {transform.Position}");

            _reportBuilder.AppendLine("  Components:");
            foreach (var comp in genericEntity.GetAllComponents())
            {
                _reportBuilder.AppendLine($"    - {comp.GetType().Name}");
            }
            _reportBuilder.AppendLine("--- End Generic Entity Report ---");
        }
    }
}