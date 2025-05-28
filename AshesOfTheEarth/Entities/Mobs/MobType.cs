// Entities/Mobs/MobType.cs
namespace AshesOfTheEarth.Entities.Mobs
{
    public enum MobType
    {
        SkeletonSpearman,
        SkeletonWarrior,
        MinotaurAlpha, // Type 1
        MinotaurBeta,  // Type 2
        MinotaurGamma, // Type 3
        GargoyleRed,
        GargoyleGreen,
        GargoyleBlue,
        WerewolfBrown,
        WerewolfBlack,
        WerewolfWhite,
        // Animals
        Deer,
        Rabbit
    }
}

// Entities/Mobs/AI/AIState.cs
namespace AshesOfTheEarth.Entities.Mobs.AI
{
    public enum AIState
    {
        Idle,
        Patrolling,
        Chasing,
        Attacking,
        ReturningToPatrol,
        Fleeing,
        Hurt,
        Dead,
        SpecialAction // For Gargoyle petrify, Skeleton protect etc.
    }
}