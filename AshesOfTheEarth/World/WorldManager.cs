// World/WorldManager.cs
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.World.Generation;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using System;
using System.Linq;
using AshesOfTheEarth.Graphics;
using AshesOfTheEarth.Core.Validation;

namespace AshesOfTheEarth.World
{
    public class WorldManager
    {
        public TileMap TileMap { get; private set; }
        public IWorldGenerator WorldGenerator { get; set; }

        private Texture2D _tilesetTexture;
        private Rectangle[] _tileSourceRectangles;
        private int _tileWidth = Utils.Settings.WorldTileWidth;
        private int _tileHeight = Utils.Settings.WorldTileHeight;

        private EntityManager _entityManager;

        public WorldManager()
        {
            _entityManager = ServiceLocator.Get<EntityManager>();
            var positionValidator = ServiceLocator.Get<IPositionValidator>();
            if (positionValidator != null)
            {
                WorldGenerator = new AdvancedProceduralGenerator(positionValidator);
            }
            else
            {
                WorldGenerator = new AdvancedProceduralGenerator(positionValidator);
            }

        }

        public void LoadContent(ContentManager content)
        {
            try
            {
                _tilesetTexture = content.Load<Texture2D>("Sprites/Tiles/tileset3");

                int columns = _tilesetTexture.Width / _tileWidth;
                int rows = _tilesetTexture.Height / _tileHeight;
                int totalTilesInTexture = columns * rows;

                int maxTileIndex = Enum.GetValues(typeof(TileType)).Cast<TileType>().Max(t => TileProperties.GetTilesetIndex(t));
                int numberOfDefinedTileTypes = maxTileIndex + 1;


                if (numberOfDefinedTileTypes > totalTilesInTexture)
                {
                    numberOfDefinedTileTypes = totalTilesInTexture;
                }


                _tileSourceRectangles = new Rectangle[numberOfDefinedTileTypes];
                for (int i = 0; i < numberOfDefinedTileTypes; i++)
                {
                    int x = (i % columns) * _tileWidth;
                    int y = (i / columns) * _tileHeight;
                    _tileSourceRectangles[i] = new Rectangle(x, y, _tileWidth, _tileHeight);
                }

            }
            catch (Exception ex)
            {
                _tilesetTexture = null;
            }
        }

        public void GenerateWorld(int seed, int width = 200, int height = 200)
        {
            if (WorldGenerator == null)
            {
                var positionValidator = ServiceLocator.Get<IPositionValidator>();
                if (positionValidator != null)
                {
                    WorldGenerator = new AdvancedProceduralGenerator(positionValidator);
                }
                else
                {
                    WorldGenerator = new BasicProceduralGenerator();
                }
            }
            TileMap = new TileMap(width, height, _tileWidth, _tileHeight);
            WorldGenerator.Generate(TileMap, seed);
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera)
        {
            if (TileMap != null && _tilesetTexture != null && _tileSourceRectangles != null)
            {
                TileMap.Draw(spriteBatch, _tilesetTexture, _tileSourceRectangles, camera);
            }
        }

        public bool IsPositionWalkable(Vector2 worldPosition)
        {
            if (TileMap == null) return false;
            Tile tile = TileMap.GetTileAtWorldPosition(worldPosition);
            return tile.IsWalkable;
        }

        public void UnloadContent()
        {
            _tilesetTexture = null;
            _tileSourceRectangles = null;
            TileMap = null;
        }
    }
}