using AshesOfTheEarth.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.Patterns.Proxy
{
    internal class LockedChestProxy : IInteractable
    {
        private IInteractable _realChest; // Referința la cufărul real (sau alt IInteractable)
        private string _requiredKeyId;    // ID-ul item-ului cheie necesar

        public string InteractionName => _realChest.InteractionName; // Deleagă numele interacțiunii

        public LockedChestProxy(IInteractable realChestToProtect, string requiredKeyId)
        {
            _realChest = realChestToProtect ?? throw new ArgumentNullException(nameof(realChestToProtect));
            _requiredKeyId = requiredKeyId ?? throw new ArgumentNullException(nameof(requiredKeyId));
            Console.WriteLine($"Proxy (LockedChestProxy): Protecting a '{realChestToProtect.GetType().Name}'. Requires key: '{_requiredKeyId}'.");
        }

        /// <summary>
        /// Verifică dacă jucătorul are cheia necesară.
        /// </summary>
        private bool HasKey(Entity interactor)
        {
            // Aici ar fi logica de verificare a inventarului jucătorului
            // var inventory = interactor.GetComponent<InventoryComponent>();
            // bool hasKey = inventory?.HasItem(_requiredKeyId) ?? false;

            // SIMULARE: Pentru test, presupunem că playerul are cheia dacă tag-ul e "PlayerWithKey"
            bool hasKeySimulated = (interactor.Tag == "PlayerWithKey");
            if (hasKeySimulated)
            {
                Console.WriteLine($"Proxy (LockedChestProxy): Check PASSED. {interactor.Tag} has the key '{_requiredKeyId}'.");
            }
            else
            {
                Console.WriteLine($"Proxy (LockedChestProxy): Check FAILED. {interactor.Tag} does NOT have the key '{_requiredKeyId}'.");
            }
            return hasKeySimulated; // Înlocuiește cu logica reală de inventar
        }

        /// <summary>
        /// Proxy-ul decide dacă interactorul poate iniția interacțiunea.
        /// </summary>
        public bool CanInteract(Entity interactor)
        {
            // Poate interacționa dacă are cheia ȘI dacă cufărul real poate fi interacționat (ex: nu e deja gol)
            return HasKey(interactor) && _realChest.CanInteract(interactor);
        }

        /// <summary>
        /// Proxy-ul gestionează încercarea de interacțiune.
        /// </summary>
        public InteractionResult AttemptInteract(Entity interactor)
        {
            if (!HasKey(interactor))
            {
                string message = $"This chest is locked. You need the '{_requiredKeyId}'.";
                Console.WriteLine($"Proxy (LockedChestProxy): {message}");
                return InteractionResult.Failed(message);
            }

            // Dacă are cheia, deleagă interacțiunea către cufărul real.
            Console.WriteLine($"Proxy (LockedChestProxy): Access granted. Delegating interaction to real chest.");
            return _realChest.AttemptInteract(interactor);
        }
    }
}
