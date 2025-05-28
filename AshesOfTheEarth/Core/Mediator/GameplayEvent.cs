namespace AshesOfTheEarth.Core.Mediator
{
    public enum GameplayEvent
    {
        PlayerAttackAttempt,
        PlayerInteractAttempt,
        PlayerDamaged,
        EntityDamaged, // Added
        EntityDied,
        ItemCollected,
        InventoryOpened,
        InventoryClosed,
        MobAggroedPlayer // Added
    }
}