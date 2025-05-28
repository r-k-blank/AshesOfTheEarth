using System;
using System.Collections.Generic;

namespace AshesOfTheEarth.Gameplay.Events
{
    // Delegat generic pentru gestionarea evenimentelor
    public delegate void GameEventHandler(object sender, GameEventArgs args);

    public class EventManager
    {
        private readonly Dictionary<string, GameEventHandler> _eventHandlers =
            new Dictionary<string, GameEventHandler>();

        // Adaugă un listener pentru un anumit tip de eveniment
        public void AddListener(string eventType, GameEventHandler handler)
        {
            if (string.IsNullOrEmpty(eventType) || handler == null) return;

            if (_eventHandlers.TryGetValue(eventType, out GameEventHandler thisEvent))
            {
                // Adaugă handler-ul la delegatul multicast existent
                _eventHandlers[eventType] = thisEvent + handler;
            }
            else
            {
                // Creează un nou delegat pentru acest tip de eveniment
                _eventHandlers.Add(eventType, handler);
            }
            // System.Diagnostics.Debug.WriteLine($"Listener added for event: {eventType}");
        }

        // Elimină un listener dintr-un anumit tip de eveniment
        public void RemoveListener(string eventType, GameEventHandler handler)
        {
            if (string.IsNullOrEmpty(eventType) || handler == null) return;

            if (_eventHandlers.TryGetValue(eventType, out GameEventHandler thisEvent))
            {
                // Elimină handler-ul din delegatul multicast
                thisEvent -= handler;

                // Dacă nu mai sunt listeneri, elimină intrarea din dicționar
                if (thisEvent == null)
                {
                    _eventHandlers.Remove(eventType);
                }
                else
                {
                    _eventHandlers[eventType] = thisEvent;
                }
                // System.Diagnostics.Debug.WriteLine($"Listener removed for event: {eventType}");
            }
        }

        // Declanșează un eveniment, notificând toți listenerii înregistrați
        public void RaiseEvent(string eventType, object sender, GameEventArgs args)
        {
            if (string.IsNullOrEmpty(eventType)) return;

            if (_eventHandlers.TryGetValue(eventType, out GameEventHandler thisEvent))
            {
                // System.Diagnostics.Debug.WriteLine($"Raising event: {eventType}");
                // Invocă delegatul (care va apela toți handlerii atașați)
                thisEvent?.Invoke(sender, args ?? GameEventArgs.Empty);
            }
            // else
            // {
            //      System.Diagnostics.Debug.WriteLine($"No listeners for event: {eventType}");
            // }
        }
    }
}