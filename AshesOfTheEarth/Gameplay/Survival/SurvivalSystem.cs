using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using Microsoft.Xna.Framework; // Pentru GameTime
using AshesOfTheEarth.Core.Time;

namespace AshesOfTheEarth.Gameplay.Survival
{
    public class SurvivalSystem : ITimeObserver // Implementează interfața
    {
        private readonly EntityManager _entityManager;
        private const float HUNGER_PER_HOUR = 2.5f;
        private const float STAMINA_REGEN_PER_SECOND = 5f;
        private const float HUNGER_DAMAGE_THRESHOLD = 80f;
        private const float HUNGER_DAMAGE_PER_HOUR = 5f;

        public SurvivalSystem(EntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        // --- METODE ITimeObserver ---
        public void OnHourElapsed(int hour)
        {
            var players = _entityManager.GetAllEntitiesWithComponents<PlayerControllerComponent, StatsComponent, HealthComponent>();
            foreach (var player in players)
            {
                var stats = player.GetComponent<StatsComponent>();
                stats.IncreaseHunger(HUNGER_PER_HOUR);
                System.Diagnostics.Debug.WriteLine($"Player Hunger increased: {stats.CurrentHunger}/{stats.MaxHunger}");

                if (stats.CurrentHunger >= HUNGER_DAMAGE_THRESHOLD)
                {
                    var health = player.GetComponent<HealthComponent>();
                    health.TakeDamage(HUNGER_DAMAGE_PER_HOUR);
                    System.Diagnostics.Debug.WriteLine($"Player taking hunger damage! Health: {health.CurrentHealth}");
                }
            }
        }

        public void OnTimeChanged(TimeManager timeManager)
        {
            // SurvivalSystem nu reacționează la fiecare schimbare minoră de timp
        }

        public void OnDayPhaseChanged(DayPhase newPhase)
        {
            // SurvivalSystem nu reacționează specific la schimbarea fazei zilei (deși ar putea, ex: monștri mai periculoși noaptea)
        }
        // --- SFÂRȘIT METODE ITimeObserver ---

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var entitiesWithStats = _entityManager.GetAllEntitiesWithComponents<StatsComponent>();
            foreach (var entity in entitiesWithStats)
            {
                var stats = entity.GetComponent<StatsComponent>();
                stats.RegenStamina(STAMINA_REGEN_PER_SECOND * deltaTime);
            }
        }
    }
}