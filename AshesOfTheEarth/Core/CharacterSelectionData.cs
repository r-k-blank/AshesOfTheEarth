// In Core/CharacterSelectionData.cs
using Microsoft.Xna.Framework.Graphics; // Pentru Texture2D, dacă alegi să salvezi preview-ul ca textură separată

namespace AshesOfTheEarth.Core
{
    public enum PlayerAnimationSetType // Deja existent
    {
        Gotoku,
        Kunoichi,
        Ninja,
        Vampire
    }

    public class CharacterSelectionData
    {
        public string Name { get; }
        public string Description { get; }
        public string SpriteSheetPath { get; }
        public PlayerAnimationSetType AnimationType { get; }
        // public Texture2D PreviewImage { get; set; } // Putem renunța la asta deocamdată

        // NOU: Adaugă dimensiunile frame-ului dacă variază
        public int FrameWidth { get; }
        public int FrameHeight { get; }

        public CharacterSelectionData(string name, string description, string spriteSheetPath, PlayerAnimationSetType animationType, int frameWidth = 128, int frameHeight = 128) // Valori default
        {
            Name = name;
            Description = description;
            SpriteSheetPath = spriteSheetPath;
            AnimationType = animationType;
            FrameWidth = frameWidth; // NOU
            FrameHeight = frameHeight; // NOU
        }
    }
}