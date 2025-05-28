namespace AshesOfTheEarth.Entities.Components
{
    public class MobStatsComponent : IComponent
    {
        public float Damage { get; set; } = 5f;
        // Cât de des poate INIȚIA un nou atac (1 = o dată pe secundă).
        // Influențează AttackIntervalTimer din AIComponent.
        public float AttackSpeed { get; set; } = 1f;
        public float AttackRange { get; set; } = 70f;
        public float AggroRange { get; set; } = 250f;
        public float MovementSpeed { get; set; } = 70f;
        public float RunSpeedMultiplier { get; set; } = 1.5f;

        // Cât timp rămâne mobul vizibil (pe ultimul frame al animației de moarte) înainte de a dispărea.
        public float DeathLingerDuration { get; set; } = 2.0f;

        // Lățimea conului de atac în grade. Playerul trebuie să fie în acest con pentru a fi lovit.
        public float AttackConeAngleDegrees { get; set; } = 100f;

        public MobStatsComponent() { }
    }
}