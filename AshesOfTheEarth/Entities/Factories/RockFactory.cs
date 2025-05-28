// Entities/Factories/RockFactory.cs
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace AshesOfTheEarth.Entities.Factories
{
    public enum RockType { StoneNode, IronVein, CoalDeposit, CrystalFormation }

    public class RockFactory : IEntityFactory
    {
        private ContentManager _content;
        private Dictionary<RockType, Texture2D> _rockTextures = new Dictionary<RockType, Texture2D>();

        public RockFactory(ContentManager content)
        {
            _content = content;
            LoadAssets();
        }

        private void LoadAssets()
        {
            try
            {
                _rockTextures[RockType.StoneNode] = _content.Load<Texture2D>("Sprites/World/Resources/rock_stone");
                _rockTextures[RockType.IronVein] = _content.Load<Texture2D>("Sprites/World/Resources/rock_iron");
                _rockTextures[RockType.CoalDeposit] = _content.Load<Texture2D>("Sprites/World/Resources/rock_coal");
                _rockTextures[RockType.CrystalFormation] = _content.Load<Texture2D>("Sprites/World/Resources/rock_crystal");
            }
            catch (System.Exception ex) { System.Diagnostics.Debug.WriteLine($"Error loading rock textures: {ex.Message}"); }
        }

        public Entity CreateEntity(Vector2 position)
        {
            return CreateRock(position, RockType.StoneNode); // Default
        }

        public Entity CreateRock(Vector2 position, RockType rockType)
        {
            if (!_rockTextures.TryGetValue(rockType, out Texture2D texture))
            {
                if (!_rockTextures.TryGetValue(RockType.StoneNode, out texture)) return null;
            }

            Entity rock = new Entity($"Rock_{rockType}");
            var transform = new TransformComponent { Position = position, Scale = Vector2.One * 1.2f };
            rock.AddComponent(transform);

            var spriteComp = new SpriteComponent(texture);
            spriteComp.Origin = new Vector2(texture.Width / 2f, texture.Height / 2f + texture.Height * 0.2f); // Puțin mai jos de centru
            spriteComp.LayerDepth = 0.35f + (position.Y / (Utils.Settings.DefaultWorldHeight * Utils.Settings.WorldTileHeight * 2f));
            rock.AddComponent(spriteComp);

            List<DropChance> drops = new List<DropChance>();
            float health = 40f;
            string tool = "Pickaxe";

            switch (rockType)
            {
                case RockType.StoneNode:
                    drops.Add(new DropChance(ItemType.StoneShard, 3, 6, 1.0f));
                    drops.Add(new DropChance(ItemType.Flint, 0, 2, 0.3f));
                    health = 50f;
                    break;
                case RockType.IronVein:
                    drops.Add(new DropChance(ItemType.StoneShard, 1, 3, 0.7f));
                    drops.Add(new DropChance(ItemType.IronOre, 2, 4, 1.0f));
                    health = 80f;
                    break;
                case RockType.CoalDeposit:
                    drops.Add(new DropChance(ItemType.StoneShard, 1, 2, 0.5f));
                    drops.Add(new DropChance(ItemType.Coal, 3, 5, 1.0f));
                    health = 60f;
                    break;
                case RockType.CrystalFormation:
                    drops.Add(new DropChance(ItemType.StoneShard, 0, 1, 0.2f));
                    drops.Add(new DropChance(ItemType.CrystalShard, 1, 3, 1.0f));
                    health = 100f;
                    tool = "MagicPickaxe"; // Sau un târnăcop mai bun
                    break;
            }
            rock.AddComponent(new ResourceSourceComponent(rockType.ToString(), health, drops, tool));

            // ColliderComponent
            float colliderWidth = texture.Width * 0.85f * transform.Scale.X; // Mai lat
            float colliderHeight = texture.Height * 0.75f * transform.Scale.Y;
            Vector2 colliderOffset = new Vector2(0, texture.Height * 0.1f * transform.Scale.Y - colliderHeight * 0.4f);
            rock.AddComponent(new ColliderComponent(
                new Rectangle(0, 0, (int)colliderWidth, (int)colliderHeight), colliderOffset, isSolid: true
            ));

            System.Diagnostics.Debug.WriteLine($"Created {rockType} at {position}");
            return rock;
        }
    }
}