using Microsoft.Xna.Framework;

namespace AshesOfTheEarth.World
{
    public struct Tile
    {
        public TileType Type;
        // Poți adăuga date specifice per tile dacă e absolut necesar
        // public byte Variant; // Ex: pentru variații vizuale ale aceluiași tip
        // public bool HasResource;

        public Tile(TileType type)
        {
            Type = type;
        }

        public bool IsWalkable => TileProperties.IsWalkable(Type);
        public int TilesetIndex => TileProperties.GetTilesetIndex(Type);
    }
}