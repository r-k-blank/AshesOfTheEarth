// AshesOfTheEarth.Utils.Settings.cs
namespace AshesOfTheEarth.Utils
{
    public static class Settings
    {
        public static bool DebugShowColliders { get; set; } = false;

        // --- Setări pentru Joc Nou ---
        public enum WorldSizeOption { Small, Medium, Large }
        public static WorldSizeOption SelectedWorldSize { get; set; } = WorldSizeOption.Medium;

        public enum DifficultyOption { Easy, Normal, Hard }
        public static DifficultyOption SelectedDifficulty { get; set; } = DifficultyOption.Normal;

        // (Opțional) Seed-ul Lumii
        public static int WorldSeed { get; set; } = 0; // 0 = Random la New Game
        public static bool UseCustomSeed { get; set; } = false;

        // --- Constante pentru Dimensiuni Efective ---
        public const int SmallWorldWidth = 150;
        public const int SmallWorldHeight = 150;
        public const int MediumWorldWidth = 250;
        public const int MediumWorldHeight = 250;
        public const int LargeWorldWidth = 350;
        public const int LargeWorldHeight = 350;

        public static int GetActualWorldWidth()
        {
            switch (SelectedWorldSize)
            {
                case WorldSizeOption.Small: return SmallWorldWidth;
                case WorldSizeOption.Large: return LargeWorldWidth;
                case WorldSizeOption.Medium:
                default: return MediumWorldWidth;
            }
        }

        public static int GetActualWorldHeight()
        {
            switch (SelectedWorldSize)
            {
                case WorldSizeOption.Small: return SmallWorldHeight;
                case WorldSizeOption.Large: return LargeWorldHeight;
                case WorldSizeOption.Medium:
                default: return MediumWorldHeight;
            }
        }

        // --- Setări Generale Joc (pot fi modificate și în joc, dacă are sens) ---
        // Exemplu: public static float SoundVolume { get; set; } = 0.8f;


        // --- Constantele tale existente ---
        public const int DefaultScreenWidth = 1280;
        public const int DefaultScreenHeight = 720;
        public const int WorldTileWidth = 32;
        public const int WorldTileHeight = 32;
        public const int DefaultWorldWidth = 300; // In tiles
        public const int DefaultWorldHeight = 300; // In tiles
        // DefaultWorldWidth și DefaultWorldHeight sunt acum gestionate de GetActualWorldWidth/Height
    }
}