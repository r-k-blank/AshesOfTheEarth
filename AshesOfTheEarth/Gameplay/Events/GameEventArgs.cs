using System;
using System.Collections.Generic;

namespace AshesOfTheEarth.Gameplay.Events
{
    // Clasă container simplă pentru datele evenimentului
    public class GameEventArgs : EventArgs
    {
        // Folosim un dicționar pentru flexibilitate maximă
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public object this[string key]
        {
            get => _data.TryGetValue(key, out var value) ? value : null;
            set => _data[key] = value;
        }

        public bool ContainsKey(string key) => _data.ContainsKey(key);

        // Helper pentru a obține valori tipate în siguranță
        public T GetValueOrDefault<T>(string key, T defaultValue = default)
        {
            if (_data.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        public static readonly new GameEventArgs Empty = new GameEventArgs();
    }
}