using System;
using System.Collections.Generic;

namespace AshesOfTheEarth.Core.Services
{
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        private readonly Dictionary<Type, object> _services;

        private ServiceLocator()
        {
            _services = new Dictionary<Type, object>();
        }

        public static ServiceLocator Initialize()
        {
            if (_instance == null)
            {
                _instance = new ServiceLocator();
            }
            return _instance;
        }

        public static T Get<T>()
        {
            if (_instance == null)
            {
                Initialize();
            }
            Type type = typeof(T);
            if (!_instance._services.ContainsKey(type))
            {
                return default(T);
            }
            return (T)_instance._services[type];
        }

        public static void Register<T>(T service)
        {
            if (_instance == null)
            {
                Initialize();
            }
            Type type = typeof(T);
            _instance._services[type] = service;
        }

        public static void Unregister<T>()
        {
            if (_instance == null) return;
            Type type = typeof(T);
            if (_instance._services.TryGetValue(type, out object serviceInstance))
            {
                if (serviceInstance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _instance._services.Remove(type);
            }
        }

        public static void Cleanup()
        {
            if (_instance != null)
            {
                foreach (var servicePair in new Dictionary<Type, object>(_instance._services))
                {
                    if (servicePair.Value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                _instance._services.Clear();
                _instance = null;
            }
        }
    }
}