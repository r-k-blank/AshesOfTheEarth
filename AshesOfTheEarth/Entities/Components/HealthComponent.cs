namespace AshesOfTheEarth.Entities.Components
{
    public class HealthComponent : IComponent
    {
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }

        public bool IsDead => CurrentHealth <= 0;

        public HealthComponent(float maxHealth)
        {
            MaxHealth = maxHealth > 0 ? maxHealth : 1;
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0 || IsDead) return;
            CurrentHealth -= amount;
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }
            System.Diagnostics.Debug.WriteLine($"Entity took {amount} damage. Health: {CurrentHealth}/{MaxHealth}");
            // TODO: Ar putea declanșa un eveniment "EntityDamaged"
        }

        public void Heal(float amount)
        {
            if (amount <= 0 || IsDead) return;
            CurrentHealth += amount;
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
            System.Diagnostics.Debug.WriteLine($"Entity healed {amount}. Health: {CurrentHealth}/{MaxHealth}");
        }
    }
}

