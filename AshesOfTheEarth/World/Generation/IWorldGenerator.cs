namespace AshesOfTheEarth.World.Generation
{
    public interface IWorldGenerator
    {
        // Generează tile-urile pentru harta dată
        void Generate(TileMap tileMap, int seed);
    }
}