using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Entities.Components
{
    public class StatsComponent : IComponent
    {
        public float MaxHunger { get; set; } = 100f;
        public float CurrentHunger { get; set; } = 0f;

        public float MaxStamina { get; set; } = 5000f;
        public float CurrentStamina { get; set; } = 100f;
        public float StaminaRegenRate { get; set; } = 5f; // Stamina per secundă
        public float StaminaDrainRateRun { get; set; } = 10f; // Stamina consumată pe secundă la alergare

        public bool IsExhausted => CurrentStamina <= 0;

        public StatsComponent(float maxHunger = 100f, float maxStamina = 100f)
        {
            MaxHunger = maxHunger;
            CurrentHunger = 0;
            MaxStamina = maxStamina;
            CurrentStamina = maxStamina;
        }

        public void IncreaseHunger(float amount)
        {
            if (amount > 0)
                CurrentHunger = MathHelper.Min(CurrentHunger + amount, MaxHunger);
        }

        public void DecreaseHunger(float amount)
        {
            if (amount > 0)
                CurrentHunger = MathHelper.Max(CurrentHunger - amount, 0);
        }

        public bool TryUseStamina(float amount) // Schimbat pentru a returna bool
        {
            if (CurrentStamina >= amount)
            {
                CurrentStamina -= amount;
                CurrentStamina = MathHelper.Max(CurrentStamina, 0);
                return true;
            }
            return false;
        }

        public void RegenStamina(float amount)
        {
            if (amount > 0 && CurrentHunger < MaxHunger * 0.8f) // Nu regenerezi stamină dacă ești prea flămând
                CurrentStamina = MathHelper.Min(CurrentStamina + amount, MaxStamina);
        }

        public float HungerPercentage => (MaxHunger > 0 ? (CurrentHunger / MaxHunger) : 0);
        public float StaminaPercentage => (MaxStamina > 0 ? (CurrentStamina / MaxStamina) : 0);
    }
}