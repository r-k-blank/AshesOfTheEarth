using AshesOfTheEarth.Entities.Mobs.AI;
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Entities.Components
{
    public class AIComponent : IComponent
    {
        public AIState CurrentState { get; set; } = AIState.Idle;
        public Entity Target { get; set; } = null;
        public Vector2 LastKnownTargetPosition { get; set; }
        public Vector2 PatrolTargetPosition { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public float MaxPatrolRadius { get; set; } = 300f;
        public float StateTimer { get; set; } = 0f; // Timer general pentru starea curentă
        public float MinIdleTime { get; set; } = 2f;
        public float MaxIdleTime { get; set; } = 5f;
        public float CurrentIdleTime { get; set; } = 0f; // Durata specifică a ciclului curent de idle
        public float MaxChaseTimeWithoutAttack { get; set; } = 10f;
        public float CurrentChaseTimeWithoutAttack { get; set; } = 0f;

        // Timer pentru durata efectivă a animației de atac. 
        // Când ajunge la 0, animația de atac s-a terminat.
        public float AttackActionTimer { get; set; } = 0f;

        // Timer pentru cooldown-ul DINTRE atacuri (cât așteaptă mobul după ce un atac s-a încheiat).
        // Se bazează pe AttackSpeed din MobStatsComponent.
        public float AttackIntervalTimer { get; set; } = 0f;

        // Cooldown general pentru acțiuni speciale (ex: protect la schelet)
        public float ActionCooldown { get; set; } = 0f;

        // Direcția în care se uită mobul (pentru animație/con de atac)
        public Vector2 FacingDirection { get; set; } = Vector2.UnitX; // Default spre dreapta

        // Flag pentru a asigura că damage-ul se aplică o singură dată per ciclu de animație de atac
        public bool DamageAppliedThisAttackCycle { get; set; } = false;


        public AIComponent(Vector2 spawnPosition)
        {
            SpawnPosition = spawnPosition;
            PatrolTargetPosition = spawnPosition;
            FacingDirection = _DefaultFacingDirectionBasedOnSpawn(spawnPosition);
        }

        // O mică logică pentru a seta o direcție de facing inițială mai variată
        private Vector2 _DefaultFacingDirectionBasedOnSpawn(Vector2 spawnPos)
        {
            // Simplu: dacă e în jumătatea stângă a unei hărți (presupuse), se uită la dreapta, și invers.
            // Acest lucru e foarte aproximativ și depinde de context.
            // Poți folosi un Random sau o valoare fixă.
            if ((int)spawnPos.X % 2 == 0) return Vector2.UnitX; // Dreapta
            return -Vector2.UnitX; // Stânga
        }
    }
}