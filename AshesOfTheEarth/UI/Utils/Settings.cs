namespace AshesOfTheEarth.Utils
{
    // O clasă simplă pentru constante sau setări globale
    // Poate fi extinsă pentru a citi dintr-un fișier de configurare
    public static class Settings
    {
        // Poți muta constantele din TimeManager sau alte locuri aici
        public const int DefaultScreenWidth = 1280;
        public const int DefaultScreenHeight = 720;

        public const int WorldTileWidth = 32;
        public const int WorldTileHeight = 32;

        public const int DefaultWorldWidth = 150; // In tiles
        public const int DefaultWorldHeight = 150; // In tiles

        public static bool DebugShowColliders { get; set; } = false;
        // Alte setări...
        // public static float MusicVolume = 0.7f;
        // public static float SfxVolume = 0.8f;
        // public static bool ShowDebugInfo = false;
    }
}
//Escape, I, Tab, F5, Spațiu, E, WASD