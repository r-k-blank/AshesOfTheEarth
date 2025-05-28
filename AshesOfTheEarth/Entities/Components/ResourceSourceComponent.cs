using AshesOfTheEarth.Gameplay.Items;
using System.Collections.Generic;

namespace AshesOfTheEarth.Entities.Components
{
    public class ResourceSourceComponent : IComponent
    {
        public string ResourceName { get; set; }
        public List<DropChance> PossibleDrops { get; private set; }

        // Made MaxHealth settable for restore
        public float MaxHealth { get; set; }
        public float Health { get; set; }
        public bool Depleted => Health <= 0;
        public string RequiredToolCategory { get; set; }
        public float HarvestTimePerHit { get; set; }
        public bool DestroyOnDepleted { get; set; } = true;


        public ResourceSourceComponent(string name, float maxHealth, List<DropChance> drops, string requiredTool = null, float harvestTime = 1f, bool destroyOnDepleted = true)
        {
            ResourceName = name;
            MaxHealth = maxHealth > 0 ? maxHealth : 1;
            Health = MaxHealth;
            PossibleDrops = drops ?? new List<DropChance>();
            RequiredToolCategory = requiredTool;
            HarvestTimePerHit = harvestTime;
            DestroyOnDepleted = destroyOnDepleted;
        }

        public void TakeDamage(float amount)
        {
            if (Depleted) return;
            Health -= amount;
            if (Health < 0) Health = 0;
        }
    }
}