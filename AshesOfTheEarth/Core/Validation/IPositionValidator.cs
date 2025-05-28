// Core/Validation/IPositionValidator.cs
using AshesOfTheEarth.Entities;
using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.Core.Validation
{
    public interface IPositionValidator
    {
        bool IsPositionSafe(Entity entityProspect);
        Vector2 FindSafeSpawnPositionNearby(Entity entityProspectTemplate, Vector2 desiredPosition, float searchRadius, int maxAttempts = 20);
    }
}