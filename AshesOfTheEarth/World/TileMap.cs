using AshesOfTheEarth.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AshesOfTheEarth.World
{
    public class TileMap 
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }

        internal Tile[,] _tiles; // Made internal for iterator access

        public int WidthInPixels => Width * TileWidth;
        public int HeightInPixels => Height * TileHeight;


        public TileMap(int width, int height, int tileWidth, int tileHeight)
        {
            Width = width;
            Height = height;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            _tiles = new Tile[width, height];
        }
        public ITileIterator CreateIterator()
        {
            return new TileMapIterator(this);
        }
        public void SetTile(int x, int y, Tile tile)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                _tiles[x, y] = tile;
            }
        }

        public Tile GetTile(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                return _tiles[x, y];
            }
            return new Tile(TileType.Empty);
        }

        public Tile GetTileAtWorldPosition(Vector2 worldPosition)
        {
            int x = (int)(worldPosition.X / TileWidth);
            int y = (int)(worldPosition.Y / TileHeight);
            return GetTile(x, y);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D tilesetTexture, Rectangle[] sourceRects)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Tile tile = _tiles[x, y];
                    int tileIndex = tile.TilesetIndex;

                    if (tileIndex >= 0 && tileIndex < sourceRects.Length)
                    {
                        Rectangle sourceRect = sourceRects[tileIndex];
                        Vector2 position = new Vector2(x * TileWidth, y * TileHeight);
                        spriteBatch.Draw(tilesetTexture, position, sourceRect, Color.White);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D tilesetTexture, Rectangle[] sourceRects, Camera camera)
        {
            Vector2 topLeft = camera.ScreenToWorld(Point.Zero);
            Vector2 bottomRight = camera.ScreenToWorld(new Point(camera.Viewport.Width, camera.Viewport.Height));

            int buffer = 2;

            int startX = Math.Max(0, (int)(topLeft.X / TileWidth) - buffer);
            int startY = Math.Max(0, (int)(topLeft.Y / TileHeight) - buffer);
            int endX = Math.Min(Width, (int)(bottomRight.X / TileWidth) + buffer);
            int endY = Math.Min(Height, (int)(bottomRight.Y / TileHeight) + buffer);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    Tile tile = _tiles[x, y];
                    int tileIndex = tile.TilesetIndex;

                    if (tileIndex >= 0 && tileIndex < sourceRects.Length)
                    {
                        Rectangle sourceRect = sourceRects[tileIndex];
                        Vector2 position = new Vector2(x * TileWidth, y * TileHeight);
                        spriteBatch.Draw(tilesetTexture, position, sourceRect, Color.White);
                    }
                }
            }
        }
    }
}