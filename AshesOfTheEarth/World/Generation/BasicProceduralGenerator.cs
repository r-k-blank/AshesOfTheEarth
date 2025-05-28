using System; // Pentru Random

namespace AshesOfTheEarth.World.Generation
{
    public class BasicProceduralGenerator : IWorldGenerator
    {
        public void Generate(TileMap tileMap, int seed)
        {
            Random random = new Random(seed); // Folosește seed-ul pentru reproductibilitate

            for (int y = 0; y < tileMap.Height; y++)
            {
                for (int x = 0; x < tileMap.Width; x++)
                {
                    // Generare foarte simplă: predominant iarbă, cu petice de pământ/piatră
                    TileType type;
                    int chance = random.Next(100); // Număr între 0 și 99

                    if (chance < 75)
                    {
                        type = TileType.Grass;
                    }
                    else if (chance < 90)
                    {
                        type = TileType.Dirt;
                    }
                    else if (chance < 98)
                    {
                        type = TileType.Stone;
                    }
                    else
                    {
                        type = TileType.Water; // Rarișor apă
                    }


                    // Setează marginea ca fiind "Empty" sau un tip ne-mersibil
                    if (x == 0 || y == 0 || x == tileMap.Width - 1 || y == tileMap.Height - 1)
                    {
                        // Poți alege un tile specific pentru margine, ex: munți/apă adâncă
                        // type = TileType.Water; // Sau un tip "InvisibleWall"
                        type = TileType.Stone; // Zid de piatra simplu
                    }


                    tileMap.SetTile(x, y, new Tile(type));
                }
            }
            System.Diagnostics.Debug.WriteLine($"Generated basic procedural world ({tileMap.Width}x{tileMap.Height}) with seed {seed}.");
        }
    }
}