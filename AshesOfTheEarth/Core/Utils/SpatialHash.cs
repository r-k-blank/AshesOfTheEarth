using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

public class SpatialHash<T>
{
    private readonly Dictionary<long, List<T>> _cells;
    private readonly Dictionary<T, List<long>> _entityToCellKeys;
    private readonly int _cellSize;
    private readonly Func<T, Rectangle> _getBoundsFunc;

    public SpatialHash(int cellSize)
    {
        _cellSize = cellSize > 0 ? cellSize : throw new ArgumentOutOfRangeException(nameof(cellSize), "Cell size must be positive.");
        _cells = new Dictionary<long, List<T>>();
        _entityToCellKeys = new Dictionary<T, List<long>>();
    }

    private long GetCellKey(int cellX, int cellY)
    {
        return (long)cellX << 32 | (uint)cellY;
    }

    private IEnumerable<long> GetCellKeysForBounds(Rectangle bounds)
    {
        var distinctKeys = new HashSet<long>();
        int minX = (int)Math.Floor((float)bounds.Left / _cellSize);
        int maxX = (int)Math.Floor((float)bounds.Right / _cellSize);
        int minY = (int)Math.Floor((float)bounds.Top / _cellSize);
        int maxY = (int)Math.Floor((float)bounds.Bottom / _cellSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                distinctKeys.Add(GetCellKey(x, y));
            }
        }
        return distinctKeys;
    }

    public void Add(T entity, Rectangle bounds)
    {
        if (_entityToCellKeys.ContainsKey(entity))
        {
            return;
        }

        List<long> cellKeys = GetCellKeysForBounds(bounds).ToList();
        _entityToCellKeys[entity] = cellKeys;

        foreach (long key in cellKeys)
        {
            if (!_cells.TryGetValue(key, out List<T> cellEntities))
            {
                cellEntities = new List<T>();
                _cells[key] = cellEntities;
            }
            if (!cellEntities.Contains(entity))
            {
                cellEntities.Add(entity);
            }
        }
    }

    public void Update(T entity, Rectangle oldBounds, Rectangle newBounds)
    {
        if (!_entityToCellKeys.ContainsKey(entity))
        {
            Add(entity, newBounds);
            return;
        }

        List<long> oldCellKeysList = _entityToCellKeys[entity];
        List<long> newCellKeysList = GetCellKeysForBounds(newBounds).ToList();

        if (oldCellKeysList.SequenceEqual(newCellKeysList))
        {
            return;
        }

        RemoveFromCells(entity, oldCellKeysList);
        AddToCells(entity, newCellKeysList);
        _entityToCellKeys[entity] = newCellKeysList;
    }

    private void RemoveFromCells(T entity, IEnumerable<long> cellKeys)
    {
        foreach (long key in cellKeys)
        {
            if (_cells.TryGetValue(key, out List<T> cellEntities))
            {
                cellEntities.Remove(entity);
                if (cellEntities.Count == 0)
                {
                    _cells.Remove(key);
                }
            }
        }
    }

    private void AddToCells(T entity, IEnumerable<long> cellKeys)
    {
        foreach (long key in cellKeys)
        {
            if (!_cells.TryGetValue(key, out List<T> cellEntities))
            {
                cellEntities = new List<T>();
                _cells[key] = cellEntities;
            }
            if (!cellEntities.Contains(entity))
            {
                cellEntities.Add(entity);
            }
        }
    }


    public void Remove(T entity, Rectangle bounds)
    {
        if (_entityToCellKeys.TryGetValue(entity, out List<long> cellKeys))
        {
            foreach (long key in cellKeys)
            {
                if (_cells.TryGetValue(key, out List<T> cellEntities))
                {
                    cellEntities.Remove(entity);
                    if (cellEntities.Count == 0)
                    {
                        _cells.Remove(key);
                    }
                }
            }
            _entityToCellKeys.Remove(entity);
        }
    }

    public IEnumerable<T> GetNearby(Rectangle queryBounds)
    {
        var nearbyEntities = new HashSet<T>();
        IEnumerable<long> cellKeysToQuery = GetCellKeysForBounds(queryBounds);

        foreach (long key in cellKeysToQuery)
        {
            if (_cells.TryGetValue(key, out List<T> cellEntities))
            {
                foreach (T entity in cellEntities)
                {
                    nearbyEntities.Add(entity);
                }
            }
        }
        return nearbyEntities;
    }

    public void Clear()
    {
        _cells.Clear();
        _entityToCellKeys.Clear();
    }
}