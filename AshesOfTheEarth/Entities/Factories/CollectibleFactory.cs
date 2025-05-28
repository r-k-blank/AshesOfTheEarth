// Entities/Factories/CollectibleFactory.cs
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.Core.Services; // Asigură-te că ai Utils dacă nu e direct accesibil
using AshesOfTheEarth.Utils;
using System;       // Pentru Settings

namespace AshesOfTheEarth.Entities.Factories
{
    public class CollectibleFactory : IEntityFactory
    {
        public CollectibleFactory() { }

        public Entity CreateEntity(Vector2 position)
        {
            // Default, poate un WoodLog, dar e mai bine să fie specificat la apel
            return CreateCollectible(position, ItemType.WoodLog, 1);
        }

        public Entity CreateCollectible(Vector2 position, ItemType itemType, int quantity)
        {
            ItemData itemData = ItemRegistry.GetData(itemType);
            if (itemData == null || itemData.Icon == null)
            {
                System.Diagnostics.Debug.WriteLine($"CollectibleFactory: Cannot create collectible for {itemType}, ItemData or Icon is null.");
                return null;
            }

            Entity collectible = new Entity($"Collectible_{itemType}");

            // --- ÎNCEPUT MODIFICARE SCALARE ---
            // Definește cât de mare vrei să apară iconița în lume, relativ la mărimea unui tile
            float targetDisplayWidthInWorld = Settings.WorldTileWidth * 0.6f; // Iconița să aibă ~60% din lățimea unui tile
            float scaleFactor = 1.0f;

            if (itemData.Icon.Width > 0) // Evită împărțirea la zero
            {
                scaleFactor = targetDisplayWidthInWorld / itemData.Icon.Width;
            }
            else if (itemData.Icon.Height > 0) // Fallback dacă lățimea e 0, dar înălțimea nu
            {
                float targetDisplayHeightInWorld = Settings.WorldTileHeight * 0.6f;
                scaleFactor = targetDisplayHeightInWorld / itemData.Icon.Height;
            }

            // Limitează scalarea pentru a nu face itemele absurd de mici sau mari
            // dacă iconița originală are dimensiuni extreme.
            // Ajustează aceste limite după cum este necesar.
            scaleFactor = MathHelper.Clamp(scaleFactor, 0.1f, 3.0f);

            Vector2 finalScale = Vector2.One * scaleFactor;
            // --- SFÂRȘIT MODIFICARE SCALARE ---

            var transform = new TransformComponent { Position = position, Scale = finalScale };
            collectible.AddComponent(transform);

            var spriteComp = new SpriteComponent(itemData.Icon);
            // Originea la centrul texturii originale va funcționa corect cu scalarea aplicată pe TransformComponent
            spriteComp.Origin = new Vector2(itemData.Icon.Width / 2f, itemData.Icon.Height / 2f);
            spriteComp.LayerDepth = 0.45f + (position.Y / (Settings.DefaultWorldHeight * Settings.WorldTileHeight * 2f));
            collectible.AddComponent(spriteComp);

            collectible.AddComponent(new CollectibleComponent(itemType, quantity));

            // Ajustează collider-ul în funcție de noua scală.
            // Dimensiunea collider-ului va fi bazată pe dimensiunea afișată a iconiței.
            float scaledIconWidth = itemData.Icon.Width * finalScale.X;
            float scaledIconHeight = itemData.Icon.Height * finalScale.Y;

            // Folosim un procent din dimensiunea afișată pentru collider
            float colliderEffectiveSize = Math.Max(scaledIconWidth, scaledIconHeight) * 0.95f; // 75% din dimensiunea maximă afișată

            collectible.AddComponent(new ColliderComponent(
                new Rectangle(0, 0, (int)colliderEffectiveSize, (int)colliderEffectiveSize), // Collider pătrat pentru simplitate
                offset: Vector2.Zero, // Offset-ul este relativ la Transform.Position, care e centrul sprite-ului
                isSolid: false
            ));

            System.Diagnostics.Debug.WriteLine($"CollectibleFactory: Created {quantity}x {itemType} at {position} with scale {finalScale} (Original Icon: {itemData.Icon.Width}x{itemData.Icon.Height}, Scaled: {scaledIconWidth:F2}x{scaledIconHeight:F2})");
            return collectible;
        }
    }
}