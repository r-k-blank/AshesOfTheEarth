using System;
using System.Collections.Generic;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Entities.Visitor;

namespace AshesOfTheEarth.Entities
{
    public class Entity : IEntityElement
    {
        private static ulong _nextId = 0;
        public ulong Id { get; private set; }
        public string Tag { get; set; }
        public bool IsActive { get; set; } = true;

        private readonly Dictionary<Type, IComponent> _components;

        public Entity(string tag = null)
        {
            Id = _nextId++;
            Tag = tag ?? $"Entity_{Id}";
            _components = new Dictionary<Type, IComponent>();
        }

        public void AddComponent(IComponent component)
        {
            if (component == null) return;
            _components[component.GetType()] = component;
        }

        public T GetComponent<T>() where T : class, IComponent
        {
            _components.TryGetValue(typeof(T), out IComponent component);
            return component as T;
        }

        public bool HasComponent<T>() where T : class, IComponent
        {
            return _components.ContainsKey(typeof(T));
        }

        public bool HasComponents(params Type[] componentTypes)
        {
            foreach (Type type in componentTypes)
            {
                if (!typeof(IComponent).IsAssignableFrom(type) || !_components.ContainsKey(type))
                {
                    return false;
                }
            }
            return true;
        }

        public void RemoveComponent<T>() where T : class, IComponent
        {
            _components.Remove(typeof(T));
        }

        public IEnumerable<IComponent> GetAllComponents()
        {
            return _components.Values;
        }

        public override bool Equals(object obj)
        {
            return obj is Entity entity && Id == entity.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void Accept(IEntityVisitor visitor)
        {
            if (Tag == "Player" && HasComponent<PlayerControllerComponent>())
            {
                visitor.VisitPlayer(this);
            }
            else if (HasComponent<ResourceSourceComponent>())
            {
                visitor.VisitResourceNode(this);
            }
            else
            {
                visitor.VisitGenericEntity(this);
            }
        }
    }
}