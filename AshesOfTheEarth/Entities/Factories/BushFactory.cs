// Entities/Factories/BushFactory.cs
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace AshesOfTheEarth.Entities.Factories
{
    public enum BushType { BerryBush, HerbPlant }

    public class BushFactory : IEntityFactory
    {
        private ContentManager _content;
        private Dictionary<BushType, Texture2D> _bushTextures = new Dictionary<BushType, Texture2D>();
        // Poate și texturi pentru starea "cules"
        private Dictionary<BushType, Texture2D> _harvestedBushTextures = new Dictionary<BushType, Texture2D>();


        public BushFactory(ContentManager content)
        {
            _content = content;
            LoadAssets();
        }

        private void LoadAssets()
        {
            try
            {
                _bushTextures[BushType.BerryBush] = _content.Load<Texture2D>("Sprites/World/Resources/bush_berry");
                _harvestedBushTextures[BushType.BerryBush] = _content.Load<Texture2D>("Sprites/World/Resources/bush_berry_harvested");
                // Adaugă și pentru HerbPlant
            }
            catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine($"Error loading bush textures: {ex.Message}"); }
        }

        public Entity CreateEntity(Vector2 position)
        {
            return CreateBush(position, BushType.BerryBush); // Default
        }

        public Entity CreateBush(Vector2 position, BushType bushType)
        {
            if (!_bushTextures.TryGetValue(bushType, out Texture2D texture)) return null;

            Entity bush = new Entity($"Bush_{bushType}");
            var transform = new TransformComponent { Position = position, Scale = Vector2.One };
            bush.AddComponent(transform);

            var spriteComp = new SpriteComponent(texture);
            spriteComp.Origin = new Vector2(texture.Width / 2f, texture.Height - texture.Height * 0.2f); // La bază
            spriteComp.LayerDepth = 0.3f + (position.Y / (Utils.Settings.DefaultWorldHeight * Utils.Settings.WorldTileHeight * 2f));
            bush.AddComponent(spriteComp);

            List<DropChance> drops = new List<DropChance>();
            // Tufișurile de obicei au viață mică sau se recoltează instant
            float health = 1f; // O singură "lovitură" sau interacțiune
            string tool = "Hand"; // Pot fi culese cu mâna
            bool destroyOnDepleted = false; // Nu se distruge, ci trece în starea "cules" și se regenerează (vom adăuga logica de regenerare mai târziu)

            switch (bushType)
            {
                case BushType.BerryBush:
                    drops.Add(new DropChance(ItemType.Berries, 2, 5, 1.0f));
                    break;
                case BushType.HerbPlant:
                    // drops.Add(new DropChance(ItemType.HerbLeaf, 1, 3, 1.0f));
                    break;
            }
            // Pentru tufișuri, harvestTimePerHit ar putea fi timpul total de culegere.
            bush.AddComponent(new ResourceSourceComponent(bushType.ToString(), health, drops, tool, 0.5f, destroyOnDepleted));

            // ColliderComponent (mic, pentru interacțiune)
            float colliderWidth = texture.Width * 0.7f * transform.Scale.X;
            float colliderHeight = texture.Height * 0.5f * transform.Scale.Y;
            bush.AddComponent(new ColliderComponent(
                new Rectangle(0, 0, (int)colliderWidth, (int)colliderHeight),
                offset: Vector2.Zero, // Sau un mic offset dacă e necesar
                isSolid: false
            ));

            System.Diagnostics.Debug.WriteLine($"Created {bushType} at {position}");
            return bush;
        }

        // Metodă pentru a schimba sprite-ul când e cules (va fi apelată de sistemul de recoltare)
        public static void SetBushToHarvestedState(Entity bushEntity, BushType bushType)
        {
            var bushFactory = ServiceLocator.Get<BushFactory>(); // Presupunând că e înregistrat
            if (bushFactory == null || !bushFactory._harvestedBushTextures.TryGetValue(bushType, out Texture2D harvestedTexture))
            {
                return;
            }

            var spriteComp = bushEntity.GetComponent<SpriteComponent>();
            if (spriteComp != null)
            {
                spriteComp.Texture = harvestedTexture;
            }
        }
    }
}