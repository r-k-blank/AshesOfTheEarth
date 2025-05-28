// World/Generation/AdvancedProceduralGenerator.cs
using System;
using System.Collections.Generic;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Factories;
using AshesOfTheEarth.Entities.Factories.Mobs;
using AshesOfTheEarth.Entities.Factories.Animals;
using AshesOfTheEarth.Entities.Mobs;
using AshesOfTheEarth.Core.Services;
using Microsoft.Xna.Framework;
using AshesOfTheEarth.Core.Validation;

namespace AshesOfTheEarth.World.Generation
{
    public class AdvancedProceduralGenerator : IWorldGenerator
    {
        private PerlinNoise _elevationNoise;
        private PerlinNoise _moistureNoise;
        private PerlinNoise _detailNoise;
        private PerlinNoise _biomeNoise;
        private PerlinNoise _resourcePlacementNoise;
        private PerlinNoise _forestDensityNoise;

        private Random _random;

        private EntityManager _entityManager;
        private TreeFactory _treeFactory;
        private RockFactory _rockFactory;
        private BushFactory _bushFactory;
        private SkeletonFactory _skeletonFactory;
        private MinotaurFactory _minotaurFactory;
        private GargoyleFactory _gargoyleFactory;
        private WerewolfFactory _werewolfFactory;
        private AnimalFactory _animalFactory;
        private IPositionValidator _positionValidator;


        private BiomeType[,] _generatedBiomeMap;
        private List<Vector2> _placedTreePositions;
        private List<Vector2> _placedRockPositions;
        private List<Vector2> _placedBushPositions;
        private List<Vector2> _placedMobPositions;


        private const double DEEP_WATER_THRESHOLD = 0.25;
        private const double WATER_THRESHOLD = 0.35;
        private const double SAND_THRESHOLD = 0.40;
        private const double ROCKY_THRESHOLD = 0.85;
        private const double FOREST_THRESHOLD = 0.85;

        private const double ELEVATION_SCALE = 0.015;
        private const double MOISTURE_SCALE = 0.025;
        private const double BIOME_REGION_SCALE = 0.007;
        private const double DETAIL_SCALE = 0.08;
        private const double RESOURCE_NOISE_SCALE = 0.05;
        private const double FOREST_DENSITY_NOISE_SCALE = 0.008;

        private const double FOREST_ZONE_THRESHOLD = 0.55;

        private const float MIN_DISTANCE_BETWEEN_TREES = 100f;
        private const float MIN_DISTANCE_SQUARED_TREES = MIN_DISTANCE_BETWEEN_TREES * MIN_DISTANCE_BETWEEN_TREES;

        private const float MIN_DISTANCE_BETWEEN_ROCKS = 45f;
        private const float MIN_DISTANCE_SQUARED_ROCKS = MIN_DISTANCE_BETWEEN_ROCKS * MIN_DISTANCE_BETWEEN_ROCKS;

        private const float MIN_DISTANCE_TREE_ROCK = 60f;
        private const float MIN_DISTANCE_SQUARED_TREE_ROCK = MIN_DISTANCE_TREE_ROCK * MIN_DISTANCE_TREE_ROCK;

        private const float MIN_MOB_DISTANCE = 150f;
        private const float MIN_MOB_DISTANCE_SQUARED = MIN_MOB_DISTANCE * MIN_MOB_DISTANCE;

        private const double TREE_SPAWN_CHANCE_FOREST = 0.65;
        private const double TREE_SPAWN_CHANCE_PLAINS_ISOLATED = 0.95;
        private const double TREE_SPAWN_CHANCE_PLAINS_IN_FOREST_ZONE = 0.35;
        private const double TREE_SPAWN_CHANCE_CURSED = 0.70;
        private const double TREE_SPAWN_CHANCE_ROCKYHILLS_IN_FOREST_ZONE = 0.40;

        private const double ROCK_SPAWN_CHANCE_HILLS = 0.2;
        private const double ROCK_SPAWN_CHANCE_BADLANDS = 0.40;
        private const double ROCK_SPAWN_CHANCE_RARE = 0.10;
        private const double ROCK_SPAWN_CHANCE_RUINS_CRYSTAL = 0.20;

        private const double BUSH_SPAWN_CHANCE_PLAINS_FOREST = 0.55;
        private const double BUSH_SPAWN_CHANCE_ROCKYHILLS = 0.25;

        private const double MOB_SPAWN_CHANCE_COMMON = 0.30;
        private const double MOB_DENSITY_PER_BIOME = 0.90;

        public AdvancedProceduralGenerator(IPositionValidator positionValidator)
        {
            _positionValidator = positionValidator;
        }


        public void Generate(TileMap tileMap, int seed)
        {
            _entityManager = ServiceLocator.Get<EntityManager>();
            _treeFactory = ServiceLocator.Get<TreeFactory>();
            _rockFactory = ServiceLocator.Get<RockFactory>();
            _bushFactory = ServiceLocator.Get<BushFactory>();
            _skeletonFactory = ServiceLocator.Get<SkeletonFactory>();
            _minotaurFactory = ServiceLocator.Get<MinotaurFactory>();
            _gargoyleFactory = ServiceLocator.Get<GargoyleFactory>();
            _werewolfFactory = ServiceLocator.Get<WerewolfFactory>();
            _animalFactory = ServiceLocator.Get<AnimalFactory>();


            _elevationNoise = new PerlinNoise(seed);
            _moistureNoise = new PerlinNoise(seed + 1);
            _detailNoise = new PerlinNoise(seed + 2);
            _biomeNoise = new PerlinNoise(seed + 3);
            _resourcePlacementNoise = new PerlinNoise(seed + 4);
            _forestDensityNoise = new PerlinNoise(seed + 5);
            _random = new Random(seed);

            _generatedBiomeMap = AssignBiomeRegions(tileMap.Width, tileMap.Height, seed);
            _placedTreePositions = new List<Vector2>();
            _placedRockPositions = new List<Vector2>();
            _placedBushPositions = new List<Vector2>();
            _placedMobPositions = new List<Vector2>();
            int spawnedTrees = 0, spawnedRocks = 0, spawnedBushes = 0, spawnedMobs = 0;

            for (int y = 0; y < tileMap.Height; y++)
            {
                for (int x = 0; x < tileMap.Width; x++)
                {
                    BiomeType currentBiome = _generatedBiomeMap[x, y];
                    double elevation = (_elevationNoise.OctaveNoise(x * ELEVATION_SCALE, y * ELEVATION_SCALE, 0, 4, 0.5) + 1) / 2.0;
                    double moisture = (_moistureNoise.OctaveNoise(x * MOISTURE_SCALE, y * MOISTURE_SCALE, 0, 3, 0.6) + 1) / 2.0;
                    double detail = (_detailNoise.OctaveNoise(x * DETAIL_SCALE, y * DETAIL_SCALE, 0, 2, 0.7) + 1) / 2.0;

                    TileType tileType = DetermineTileType(currentBiome, elevation, moisture, detail, x, y, tileMap.Width, tileMap.Height);

                    if (x < 2 || y < 2 || x >= tileMap.Width - 2 || y >= tileMap.Height - 2)
                    {
                        if (TileProperties.IsWalkable(tileType)) tileType = TileType.DeepWater;
                    }
                    tileMap.SetTile(x, y, new Tile(tileType));

                    if (TileProperties.IsWalkable(tileType) && tileType != TileType.Water && tileType != TileType.DeepWater && tileType != TileType.Sand && tileType != TileType.RuinsFloor && tileType != TileType.Ice && tileType != TileType.SwampWater)
                    {
                        double resourcePlacementVal = (_resourcePlacementNoise.OctaveNoise(x * RESOURCE_NOISE_SCALE, y * RESOURCE_NOISE_SCALE, 0, 2, 0.6) + 1) / 2.0;
                        double forestDensityVal = (_forestDensityNoise.OctaveNoise(x * FOREST_DENSITY_NOISE_SCALE, y * FOREST_DENSITY_NOISE_SCALE, 0, 2, 0.5) + 1) / 2.0;

                        Vector2 entityPosition = new Vector2(
                            x * tileMap.TileWidth + tileMap.TileWidth / 2f + _random.Next(-tileMap.TileWidth / 5, tileMap.TileWidth / 5 + 1),
                            y * tileMap.TileHeight + tileMap.TileHeight / 2f + _random.Next(-tileMap.TileHeight / 5, tileMap.TileHeight / 5 + 1)
                        );

                        if (TryPlaceTree(currentBiome, tileType, resourcePlacementVal, forestDensityVal, entityPosition)) spawnedTrees++;
                        if (TryPlaceRock(currentBiome, tileType, resourcePlacementVal, entityPosition)) spawnedRocks++;
                        if (TryPlaceBush(currentBiome, tileType, resourcePlacementVal, entityPosition)) spawnedBushes++;
                        if (TryPlaceMobOrAnimal(currentBiome, tileType, resourcePlacementVal, entityPosition)) spawnedMobs++;
                    }
                }
            }
        }

        private bool TryPlaceMobOrAnimal(BiomeType biome, TileType groundTile, double resourceNoiseValue, Vector2 position)
        {
            if (resourceNoiseValue > MOB_SPAWN_CHANCE_COMMON) return false;

            foreach (var mobPos in _placedMobPositions)
            {
                if (Vector2.DistanceSquared(position, mobPos) < MIN_MOB_DISTANCE_SQUARED) return false;
            }

            MobType? mobToSpawn = null;
            double specificBiomeChance = MOB_DENSITY_PER_BIOME;

            switch (biome)
            {
                case BiomeType.Plains:
                    if (_random.NextDouble() < 0.6) mobToSpawn = MobType.SkeletonSpearman;
                    else mobToSpawn = _random.NextDouble() < 0.7 ? MobType.Rabbit : MobType.Deer;
                    break;
                case BiomeType.Forest:
                    if (_random.NextDouble() < 0.5) mobToSpawn = MobType.SkeletonWarrior;
                    else mobToSpawn = MobType.Rabbit;
                    break;
                case BiomeType.Swamp:
                    mobToSpawn = MobType.MinotaurAlpha;
                    specificBiomeChance *= 1.1;
                    break;
                case BiomeType.RockyHills:
                    mobToSpawn = MobType.GargoyleRed;
                    specificBiomeChance *= 1.1;
                    break;
                case BiomeType.Badlands:
                    mobToSpawn = MobType.WerewolfBrown;
                    specificBiomeChance *= 1.2;
                    break;
                case BiomeType.AncientRuins:
                    mobToSpawn = _random.NextDouble() < 0.33 ? MobType.GargoyleRed : (_random.NextDouble() < 0.5 ? MobType.GargoyleGreen : MobType.GargoyleBlue);
                    specificBiomeChance *= 1.3;
                    break;
                case BiomeType.CursedForest:
                    mobToSpawn = _random.NextDouble() < 0.33 ? MobType.WerewolfBrown : (_random.NextDouble() < 0.5 ? MobType.WerewolfBlack : MobType.WerewolfWhite);
                    specificBiomeChance *= 1.25;
                    break;
                default: return false;
            }

            if (mobToSpawn.HasValue && resourceNoiseValue < (MOB_SPAWN_CHANCE_COMMON * specificBiomeChance))
            {
                Entity mobEntity = null;
                if (mobToSpawn.Value.ToString().Contains("Skeleton")) mobEntity = _skeletonFactory.CreateSkeleton(position, mobToSpawn.Value);
                else if (mobToSpawn.Value.ToString().Contains("Minotaur")) mobEntity = _minotaurFactory.CreateMinotaur(position, mobToSpawn.Value);
                else if (mobToSpawn.Value.ToString().Contains("Gargoyle")) mobEntity = _gargoyleFactory.CreateGargoyle(position, mobToSpawn.Value);
                else if (mobToSpawn.Value.ToString().Contains("Werewolf")) mobEntity = _werewolfFactory.CreateWerewolf(position, mobToSpawn.Value);
                else if (mobToSpawn.Value == MobType.Deer || mobToSpawn.Value == MobType.Rabbit) mobEntity = _animalFactory.CreateAnimal(position, mobToSpawn.Value);

                if (mobEntity != null)
                {
                    if (_positionValidator.IsPositionSafe(mobEntity))
                    {
                        _entityManager.AddEntity(mobEntity);
                        _placedMobPositions.Add(mobEntity.GetComponent<Entities.Components.TransformComponent>().Position);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryPlaceTree(BiomeType biome, TileType groundTile, double resourceNoiseValue, double forestDensityValue, Vector2 position)
        {
            TreeType? treeToSpawn = null;
            double requiredResourceNoiseThreshold = 1.0;
            bool isInPotentialForestZone = forestDensityValue < FOREST_ZONE_THRESHOLD;
            switch (biome)
            {
                case BiomeType.Forest:
                    if (isInPotentialForestZone && (groundTile == TileType.Grass || groundTile == TileType.Dirt || groundTile == TileType.DarkGrass))
                    {
                        treeToSpawn = _random.NextDouble() < 0.65 ? TreeType.GenericOak : TreeType.Pine;
                        requiredResourceNoiseThreshold = TREE_SPAWN_CHANCE_FOREST;
                    }
                    break;
                case BiomeType.Plains:
                    if (groundTile == TileType.Grass)
                    {
                        if (isInPotentialForestZone)
                        {
                            treeToSpawn = TreeType.GenericOak;
                            requiredResourceNoiseThreshold = TREE_SPAWN_CHANCE_PLAINS_IN_FOREST_ZONE;
                        }
                        else if (_random.NextDouble() < 0.03)
                        {
                            treeToSpawn = TreeType.GenericOak;
                            requiredResourceNoiseThreshold = TREE_SPAWN_CHANCE_PLAINS_ISOLATED;
                        }
                    }
                    break;
                case BiomeType.CursedForest:
                    if (groundTile == TileType.DarkGrass || groundTile == TileType.Dirt)
                    {
                        treeToSpawn = TreeType.CursedWillow;
                        requiredResourceNoiseThreshold = TREE_SPAWN_CHANCE_CURSED;
                    }
                    break;
                case BiomeType.RockyHills:
                    if (isInPotentialForestZone && (groundTile == TileType.Grass || groundTile == TileType.Dirt))
                    {
                        treeToSpawn = TreeType.Pine;
                        requiredResourceNoiseThreshold = TREE_SPAWN_CHANCE_ROCKYHILLS_IN_FOREST_ZONE;
                    }
                    break;
            }

            if (treeToSpawn.HasValue && resourceNoiseValue < requiredResourceNoiseThreshold)
            {
                foreach (Vector2 placedPos in _placedTreePositions)
                {
                    if (Vector2.DistanceSquared(position, placedPos) < MIN_DISTANCE_SQUARED_TREES) return false;
                }
                foreach (Vector2 placedRockPos in _placedRockPositions)
                {
                    if (Vector2.DistanceSquared(position, placedRockPos) < MIN_DISTANCE_SQUARED_TREE_ROCK) return false;
                }
                foreach (Vector2 placedMobPos in _placedMobPositions)
                {
                    if (Vector2.DistanceSquared(position, placedMobPos) < MIN_DISTANCE_SQUARED_TREES * 0.5f) return false;
                }


                Entity tree = _treeFactory.CreateTree(position, treeToSpawn.Value);
                if (tree != null)
                {
                    if (_positionValidator.IsPositionSafe(tree))
                    {
                        _entityManager.AddEntity(tree);
                        _placedTreePositions.Add(tree.GetComponent<Entities.Components.TransformComponent>().Position);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryPlaceRock(BiomeType biome, TileType groundTile, double resourceNoiseValue, Vector2 position)
        {
            RockType? rockToSpawn = null;
            double requiredResourceNoiseThreshold = 1.0;
            switch (biome)
            {
                case BiomeType.RockyHills:
                    if (groundTile == TileType.RockyGround || groundTile == TileType.Stone || groundTile == TileType.Dirt)
                    {
                        double r = _random.NextDouble();
                        if (r < 0.55) rockToSpawn = RockType.StoneNode;
                        else if (r < 0.85) rockToSpawn = RockType.IronVein;
                        else rockToSpawn = RockType.CoalDeposit;
                        requiredResourceNoiseThreshold = ROCK_SPAWN_CHANCE_HILLS;
                    }
                    break;
                case BiomeType.Badlands:
                    if (groundTile == TileType.LavaRock || groundTile == TileType.DeadGrass || groundTile == TileType.Dirt)
                    {
                        double r = _random.NextDouble();
                        if (r < 0.4) rockToSpawn = RockType.StoneNode;
                        else if (r < 0.75) rockToSpawn = RockType.CoalDeposit;
                        else rockToSpawn = RockType.IronVein;
                        requiredResourceNoiseThreshold = ROCK_SPAWN_CHANCE_BADLANDS;
                    }
                    break;
                case BiomeType.Forest:
                case BiomeType.Plains:
                    if (groundTile == TileType.Dirt || groundTile == TileType.Grass || groundTile == TileType.RockyGround)
                    {
                        if (_random.NextDouble() < 0.10)
                        {
                            rockToSpawn = RockType.StoneNode;
                            requiredResourceNoiseThreshold = ROCK_SPAWN_CHANCE_RARE;
                        }
                    }
                    break;
                case BiomeType.AncientRuins:
                    if (groundTile == TileType.RockyGround || groundTile == TileType.DarkGrass)
                    {
                        if (_random.NextDouble() < 0.15)
                        {
                            rockToSpawn = _random.NextDouble() < 0.7 ? RockType.StoneNode : RockType.CrystalFormation;
                            requiredResourceNoiseThreshold = ROCK_SPAWN_CHANCE_RUINS_CRYSTAL;
                        }
                    }
                    break;
            }

            if (rockToSpawn.HasValue && resourceNoiseValue < requiredResourceNoiseThreshold)
            {
                foreach (Vector2 placedRockPos in _placedRockPositions)
                {
                    if (Vector2.DistanceSquared(position, placedRockPos) < MIN_DISTANCE_SQUARED_ROCKS) return false;
                }
                foreach (Vector2 placedTreePos in _placedTreePositions)
                {
                    if (Vector2.DistanceSquared(position, placedTreePos) < MIN_DISTANCE_SQUARED_TREE_ROCK) return false;
                }
                foreach (Vector2 placedMobPos in _placedMobPositions)
                {
                    if (Vector2.DistanceSquared(position, placedMobPos) < MIN_DISTANCE_SQUARED_ROCKS * 0.8f) return false;
                }

                Entity rock = _rockFactory.CreateRock(position, rockToSpawn.Value);
                if (rock != null)
                {
                    if (_positionValidator.IsPositionSafe(rock))
                    {
                        _entityManager.AddEntity(rock);
                        _placedRockPositions.Add(rock.GetComponent<Entities.Components.TransformComponent>().Position);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryPlaceBush(BiomeType biome, TileType groundTile, double resourceNoiseValue, Vector2 position)
        {
            BushType? bushToSpawn = null;
            double requiredResourceNoiseThreshold = 1.0;
            switch (biome)
            {
                case BiomeType.Plains:
                case BiomeType.Forest:
                    if (groundTile == TileType.Grass || groundTile == TileType.Dirt || groundTile == TileType.DarkGrass)
                    {
                        bushToSpawn = BushType.BerryBush;
                        requiredResourceNoiseThreshold = BUSH_SPAWN_CHANCE_PLAINS_FOREST;
                    }
                    break;
                case BiomeType.RockyHills:
                    if (groundTile == TileType.Grass || groundTile == TileType.Dirt)
                    {
                        if (_random.NextDouble() < 0.3)
                        {
                            bushToSpawn = BushType.BerryBush;
                            requiredResourceNoiseThreshold = BUSH_SPAWN_CHANCE_ROCKYHILLS;
                        }
                    }
                    break;
            }

            if (bushToSpawn.HasValue && resourceNoiseValue < requiredResourceNoiseThreshold)
            {
                float minBushDistToSolidSq = (MIN_DISTANCE_BETWEEN_TREES / 2.5f) * (MIN_DISTANCE_BETWEEN_TREES / 2.5f);
                foreach (Vector2 placedTreePos in _placedTreePositions)
                {
                    if (Vector2.DistanceSquared(position, placedTreePos) < minBushDistToSolidSq) return false;
                }
                foreach (Vector2 placedRockPos in _placedRockPositions)
                {
                    if (Vector2.DistanceSquared(position, placedRockPos) < minBushDistToSolidSq) return false;
                }
                foreach (Vector2 placedBushPos in _placedBushPositions)
                {
                    if (Vector2.DistanceSquared(position, placedBushPos) < (20f * 20f)) return false;
                }
                foreach (Vector2 placedMobPos in _placedMobPositions)
                {
                    if (Vector2.DistanceSquared(position, placedMobPos) < 30f * 30f) return false;
                }

                Entity bush = _bushFactory.CreateBush(position, bushToSpawn.Value);
                if (bush != null)
                {
                    if (_positionValidator.IsPositionSafe(bush))
                    {
                        _entityManager.AddEntity(bush);
                        _placedBushPositions.Add(bush.GetComponent<Entities.Components.TransformComponent>().Position);
                        return true;
                    }
                }
            }
            return false;
        }

        private BiomeType[,] AssignBiomeRegions(int width, int height, int seed)
        {
            BiomeType[,] map = new BiomeType[width, height];
            BiomeType[] mainBiomes = {
                BiomeType.Plains, BiomeType.Forest, BiomeType.Swamp,
                BiomeType.RockyHills, BiomeType.Badlands, BiomeType.AncientRuins, BiomeType.CursedForest
            };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double biomeVal = (_biomeNoise.OctaveNoise(x * BIOME_REGION_SCALE, y * BIOME_REGION_SCALE, 0, 2, 0.5) + 1) / 2.0;
                    int biomeIndex = (int)(biomeVal * mainBiomes.Length);
                    biomeIndex = Math.Clamp(biomeIndex, 0, mainBiomes.Length - 1);
                    map[x, y] = mainBiomes[biomeIndex];
                }
            }
            return map;
        }

        private TileType DetermineTileType(BiomeType biome, double elevation, double moisture, double detail, int x, int y, int mapWidth, int mapHeight)
        {
            if (elevation < DEEP_WATER_THRESHOLD) return TileType.DeepWater;
            if (elevation < WATER_THRESHOLD) return TileType.Water;
            if (elevation < SAND_THRESHOLD) return TileType.Sand;
            switch (biome)
            {
                case BiomeType.Ocean: return TileType.DeepWater;
                case BiomeType.Lake: return TileType.Water;
                case BiomeType.Plains:
                    if (elevation > ROCKY_THRESHOLD + 0.05) return TileType.Snow;
                    if (elevation > ROCKY_THRESHOLD) return TileType.RockyGround;
                    if (moisture < 0.35 && detail > 0.55) return TileType.Dirt;
                    if (moisture > 0.7 && detail < 0.3) return TileType.Mud;
                    return TileType.Grass;
                case BiomeType.Forest:
                    if (elevation > ROCKY_THRESHOLD + 0.05) return TileType.Snow;
                    if (elevation > ROCKY_THRESHOLD) return TileType.Stone;
                    if (moisture > 0.65 && detail < 0.35) return TileType.Mud;
                    if (detail > 0.6) return TileType.Dirt;
                    return TileType.Grass;
                case BiomeType.Swamp:
                    if (elevation < WATER_THRESHOLD + 0.08)
                    {
                        return detail > 0.4 ? TileType.SwampWater : TileType.Mud;
                    }
                    if (moisture > 0.45) return TileType.Mud;
                    return TileType.DarkGrass;
                case BiomeType.RockyHills:
                    if (elevation > ROCKY_THRESHOLD + 0.05) return TileType.Snow;
                    if (elevation > ROCKY_THRESHOLD * 0.9)
                    {
                        return detail > 0.4 ? TileType.Stone : TileType.RockyGround;
                    }
                    if (moisture < 0.4) return TileType.DeadGrass;
                    return TileType.Grass;
                case BiomeType.Badlands:
                    if (elevation > ROCKY_THRESHOLD + 0.03) return TileType.Stone;
                    if (elevation > ROCKY_THRESHOLD * 0.95) return TileType.LavaRock;
                    if (moisture < 0.25 && detail > 0.45) return TileType.LavaRock;
                    if (detail < 0.35 && moisture < 0.5) return TileType.Dirt;
                    return TileType.DeadGrass;
                case BiomeType.AncientRuins:
                    if (elevation > FOREST_THRESHOLD && detail > 0.5) return TileType.Stone;
                    if (detail < 0.45 && moisture < 0.6) return TileType.RuinsFloor;
                    if (moisture > 0.65) return TileType.DarkGrass;
                    return TileType.RockyGround;
                case BiomeType.CursedForest:
                    if (elevation > ROCKY_THRESHOLD + 0.05) return TileType.Snow;
                    if (elevation > ROCKY_THRESHOLD) return TileType.Stone;
                    if (moisture > 0.7 && detail < 0.3) return TileType.Mud;
                    if (detail > 0.55) return TileType.Dirt;
                    return TileType.DarkGrass;
                default:
                    return TileType.Grass;
            }
        }
    }
}