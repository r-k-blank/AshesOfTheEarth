using AshesOfTheEarth.Entities;


namespace AshesOfTheEarth.Patterns.Proxy
{
    internal interface IInteractable
    {
        string InteractionName { get; } // Numele afișat jucătorului, ex: "Open Chest", "Mine Ore"
        bool CanInteract(Entity interactor); // Verifică dacă interactorul poate iniția interacțiunea
        InteractionResult AttemptInteract(Entity interactor); // Execută interacțiunea dacă este posibil
    }

}
