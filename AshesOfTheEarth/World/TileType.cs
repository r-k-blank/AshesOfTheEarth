namespace AshesOfTheEarth.World
{
    public enum TileType : byte
    {
        Empty = 0,      // 0 - Transparent, nemersibil, pentru margini
        Grass,          // 1 - Iarbă standard
        Dirt,           // 2 - Pământ
        Stone,          // 3 - Piatră (podea, sau obstacol dacă nu e plat)
        Water,          // 4 - Apă normală, nemersibilă
        Sand,           // 5 - Nisip
        SwampWater,     // 6 - Apă mlăștinoasă, poate mersibilă cu penalizare (momentan nemersibilă)
        Mud,            // 7 - Noroi, poate încetini (momentan doar vizual)
        DarkGrass,      // 8 - Iarbă întunecată (păduri dese, zone blestemate)
        Snow,           // 9 - Zăpadă
        Ice,            // 10 - Gheață (nemersibilă, sau alunecoasă - momentan nemersibilă)
        RockyGround,    // 11 - Teren stâncos, dar mersibil
        DeadGrass,      // 12 - Iarbă uscată (pustietăți)
        LavaRock,       // 13 - Piatră vulcanică/arsă
        RuinsFloor,     // 14 - Podea de ruine
        DeepWater,      // 15 - Apă adâncă, nemersibilă (pentru margini lacuri/râuri)
        // Adaugă aici și tile-uri pentru copaci/pietre dacă vrei să fie parte din tileset-ul hărții
        // și nu entități separate pentru început. Exemplu:
        // ForestTree,     // 16
        // RockResource    // 17
    }

    public static class TileProperties
    {
        public static bool IsWalkable(TileType type)
        {
            switch (type)
            {
                case TileType.Grass:
                case TileType.Dirt:
                case TileType.Sand:
                case TileType.Mud: // Noroiul e mersibil
                case TileType.DarkGrass:
                case TileType.Snow:
                case TileType.RockyGround:
                case TileType.DeadGrass:
                case TileType.LavaRock: // Să zicem că e răcită
                case TileType.RuinsFloor:
                case TileType.Stone: // Piatra e mersibilă (ca podea)
                    return true;

                case TileType.Empty:
                case TileType.Water:
                case TileType.SwampWater: // Să zicem că e prea adâncă
                case TileType.Ice:
                case TileType.DeepWater:
                default:
                    return false;
            }
        }

        public static int GetTilesetIndex(TileType type)
        {
            // Corespunde ordinii din enum și din fișierul tileset.png
            switch (type)
            {
                case TileType.Grass: return 0;
                case TileType.Dirt: return 1;
                case TileType.Stone: return 2;
                case TileType.Water: return 3;
                case TileType.Sand: return 4;
                case TileType.SwampWater: return 5;
                case TileType.Mud: return 6;
                case TileType.DarkGrass: return 7;
                case TileType.Snow: return 8;
                case TileType.Ice: return 9;
                case TileType.RockyGround: return 10;
                case TileType.DeadGrass: return 11;
                case TileType.LavaRock: return 12;
                case TileType.RuinsFloor: return 13;
                case TileType.DeepWater: return 14;
                // case TileType.ForestTree: return 15; // Exemplu dacă le adaugi ca tile-uri
                // case TileType.RockResource: return 16; // Exemplu
                case TileType.Empty:
                default: return -1; // Nu desena nimic pentru Empty
            }
        }

        // Opțional: cost de mișcare pentru A* pathfinding sau încetinire player
        public static float GetMovementCost(TileType type)
        {
            switch (type)
            {
                case TileType.Mud: return 1.5f; // Mai greu de mers prin noroi
                case TileType.Snow: return 1.2f;
                default: return 1.0f;
            }
        }
    }
}