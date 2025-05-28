using System;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Patterns.Proxy;

namespace AshesOfTheEarth.Patterns.Proxy
{
    internal class TreasureChest : IInteractable
    {
        private string _lootItemId;
        private int _lootQuantity;
        private bool _isOpened;

        public string InteractionName => _isOpened ? "Chest (Empty)" : "Open Chest";

        public TreasureChest(string lootItemId, int lootQuantity)
        {
            _lootItemId = lootItemId;
            _lootQuantity = lootQuantity;
            _isOpened = false;
            Console.WriteLine($"RealSubject (TreasureChest): Created with loot '{_lootItemId}' x{_lootQuantity}.");
        }

        /// <summary>
        /// Pentru un cufăr simplu, oricine poate încerca să-l deschidă (dacă nu e blocat de un proxy).
        /// </summary>
        public bool CanInteract(Entity interactor)
        {
            return !_isOpened; // Poate interacționa doar dacă nu a fost deja deschis
        }

        /// <summary>
        /// Logica de deschidere a cufărului și acordare a recompensei.
        /// </summary>
        public InteractionResult AttemptInteract(Entity interactor)
        {
            if (_isOpened)
            {
                return InteractionResult.Failed("Chest is already empty.");
            }

            // Aici ar fi logica de adăugare a item-ului în inventarul jucătorului
            // var inventory = interactor.GetComponent<InventoryComponent>();
            // inventory?.AddItem(new Item(_lootItemId, _lootQuantity));

            _isOpened = true;
            string message = $"Opened the chest and found {_lootQuantity}x {_lootItemId}!";
            Console.WriteLine($"RealSubject (TreasureChest): {message}");
            // ServiceLocator.Get<Gameplay.Events.EventManager>()?.RaiseEvent("LootCollected", this, new GameEventArgs { ["Item"] = _lootItemId, ["Quantity"] = _lootQuantity });
            return InteractionResult.Succeeded(message);
        }
    }
}
