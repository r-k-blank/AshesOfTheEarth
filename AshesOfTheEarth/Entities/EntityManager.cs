using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using AshesOfTheEarth.Entities.Components;

namespace AshesOfTheEarth.Entities
{
    public class EntityManager
    {
        private readonly Dictionary<ulong, Entity> _entities;
        private readonly List<Entity> _entitiesToAdd;
        private readonly List<ulong> _entitiesToRemove;
        private readonly Dictionary<Type, List<Entity>> _componentCache;
        private Entity _playerCache;
        private readonly SpatialHash<Entity> _spatialHash;
        private const int SPATIAL_HASH_CELL_SIZE = 128;

        public EntityManager()
        {
            _entities = new Dictionary<ulong, Entity>();
            _entitiesToAdd = new List<Entity>();
            _entitiesToRemove = new List<ulong>();
            _componentCache = new Dictionary<Type, List<Entity>>();
            _spatialHash = new SpatialHash<Entity>(SPATIAL_HASH_CELL_SIZE);
        }

        public void AddEntity(Entity entity)
        {
            if (entity == null || _entities.ContainsKey(entity.Id) || _entitiesToAdd.Contains(entity)) return;
            _entitiesToAdd.Add(entity);
            if (entity.Tag == "Player") _playerCache = entity;

        }

        public void RemoveEntity(ulong entityId)
        {
            if (!_entities.ContainsKey(entityId) || _entitiesToRemove.Contains(entityId)) return;

            _entitiesToRemove.Add(entityId);
            if (_playerCache != null && _playerCache.Id == entityId) _playerCache = null;
        }
        public void RemoveEntity(Entity entity)
        {
            if (entity != null) RemoveEntity(entity.Id);
        }

        public Entity GetEntity(ulong entityId)
        {
            _entities.TryGetValue(entityId, out Entity entity);
            return entity;
        }

        public Entity GetEntityByTag(string tag)
        {
            if (tag == "Player" && _playerCache != null) return _playerCache;

            var entity = _entities.Values.FirstOrDefault(e => e.Tag == tag);
            if (tag == "Player" && entity != null) _playerCache = entity;
            return entity;
        }

        public IEnumerable<Entity> GetAllEntities() => _entities.Values;

        public IEnumerable<Entity> GetAllEntitiesWithComponents<T1>() where T1 : class, IComponent
        {
            return _entities.Values.Where(e => e.HasComponent<T1>());
        }

        public IEnumerable<Entity> GetAllEntitiesWithComponents<T1, T2>()
            where T1 : class, IComponent
            where T2 : class, IComponent
        {
            return _entities.Values.Where(e => e.HasComponent<T1>() && e.HasComponent<T2>());
        }

        public IEnumerable<Entity> GetAllEntitiesWithComponents<T1, T2, T3>()
           where T1 : class, IComponent
           where T2 : class, IComponent
            where T3 : class, IComponent
        {
            return _entities.Values.Where(e => e.HasComponent<T1>() && e.HasComponent<T2>() && e.HasComponent<T3>());
        }
        public IEnumerable<Entity> GetAllEntitiesWithComponents<T1, T2, T3, T4>()
           where T1 : class, IComponent
           where T2 : class, IComponent
            where T3 : class, IComponent
            where T4 : class, IComponent
        {
            return _entities.Values.Where(e => e.HasComponent<T1>() && e.HasComponent<T2>() && e.HasComponent<T3>() && e.HasComponent<T4>());
        }
        public IEnumerable<Entity> GetAllEntitiesWithComponents<T1, T2, T3, T4, T5>()
           where T1 : class, IComponent
           where T2 : class, IComponent
            where T3 : class, IComponent
            where T4 : class, IComponent
            where T5 : class, IComponent
        {
            return _entities.Values.Where(e => e.HasComponent<T1>() && e.HasComponent<T2>() && e.HasComponent<T3>() && e.HasComponent<T4>() && e.HasComponent<T5>());
        }
        public IEnumerable<Entity> GetAllEntitiesWithComponents<T1, T2, T3, T4, T5, T6>()
           where T1 : class, IComponent
           where T2 : class, IComponent
            where T3 : class, IComponent
            where T4 : class, IComponent
            where T5 : class, IComponent
            where T6 : class, IComponent
        {
            return _entities.Values.Where(e => e.HasComponent<T1>() && e.HasComponent<T2>() && e.HasComponent<T3>() && e.HasComponent<T4>() && e.HasComponent<T5>() && e.HasComponent<T6>());
        }

        public IEnumerable<Entity> GetAllEntitiesWithComponents(params Type[] componentTypes)
        {
            if (componentTypes == null || componentTypes.Length == 0)
                return GetAllEntities();

            return _entities.Values.Where(e => componentTypes.All(type => typeof(IComponent).IsAssignableFrom(type) && e.HasComponents(type)));
        }

        public void Update(GameTime gameTime)
        {
            ProcessQueues();
            foreach (var entity in _entities.Values.ToList())
            {
                var animationComp = entity.GetComponent<AnimationComponent>();
                animationComp?.Update(gameTime);
            }
        }

        private void ProcessQueues()
        {
            if (_entitiesToAdd.Count > 0)
            {
                foreach (var entity in _entitiesToAdd)
                {
                    if (!_entities.ContainsKey(entity.Id))
                    {
                        _entities.Add(entity.Id, entity);
                        UpdateComponentCacheAdd(entity);
                        if (entity.Tag == "Player") _playerCache = entity;

                        var transform = entity.GetComponent<TransformComponent>();
                        var collider = entity.GetComponent<ColliderComponent>();
                        if (transform != null && collider != null)
                        {
                            _spatialHash.Add(entity, collider.GetWorldBounds(transform));
                        }
                    }
                }
                _entitiesToAdd.Clear();
            }

            if (_entitiesToRemove.Count > 0)
            {
                foreach (var entityId in _entitiesToRemove)
                {
                    if (_entities.TryGetValue(entityId, out var entity))
                    {
                        var transform = entity.GetComponent<TransformComponent>();
                        var collider = entity.GetComponent<ColliderComponent>();
                        if (transform != null && collider != null)
                        {
                            _spatialHash.Remove(entity, collider.GetWorldBounds(transform));
                        }

                        UpdateComponentCacheRemove(entity);
                        _entities.Remove(entityId);
                        if (_playerCache != null && _playerCache.Id == entityId) _playerCache = null;
                    }
                }
                _entitiesToRemove.Clear();
            }
        }

        public void OnEntityMoved(Entity entity, Vector2 oldPosition)
        {
            var transform = entity.GetComponent<TransformComponent>();
            var collider = entity.GetComponent<ColliderComponent>();

            if (transform != null && collider != null)
            {
                var oldWorldBounds = new Rectangle(
                    (int)(oldPosition.X + collider.Offset.X - collider.Bounds.Width / 2f),
                    (int)(oldPosition.Y + collider.Offset.Y - collider.Bounds.Height / 2f),
                    collider.Bounds.Width,
                    collider.Bounds.Height
                );
                var newWorldBounds = collider.GetWorldBounds(transform);

                if (oldWorldBounds != newWorldBounds)
                {
                    _spatialHash.Update(entity, oldWorldBounds, newWorldBounds);
                }
            }
        }

        public IEnumerable<Entity> GetEntitiesInBounds(Rectangle queryBounds)
        {
            return _spatialHash.GetNearby(queryBounds);
        }

        private void UpdateComponentCacheAdd(Entity entity)
        {
            foreach (var componentType in entity.GetAllComponents().Select(c => c.GetType()))
            {
                if (!_componentCache.ContainsKey(componentType))
                {
                    _componentCache[componentType] = new List<Entity>();
                }
                if (!_componentCache[componentType].Contains(entity))
                    _componentCache[componentType].Add(entity);
            }
        }

        private void UpdateComponentCacheRemove(Entity entity)
        {
            foreach (var componentType in entity.GetAllComponents().Select(c => c.GetType()))
            {
                if (_componentCache.TryGetValue(componentType, out var list))
                {
                    list.Remove(entity);
                }
            }
        }

        public void ClearAllEntities()
        {
            _spatialHash.Clear();
            _entities.Clear();
            _entitiesToAdd.Clear();
            _entitiesToRemove.Clear();
            _componentCache.Clear();
            _playerCache = null;
        }
    }
}