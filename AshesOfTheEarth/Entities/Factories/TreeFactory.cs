// Entities/Factories/TreeFactory.cs
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using AshesOfTheEarth.Graphics.Animation; // Chiar dacă nu are animații complexe acum

namespace AshesOfTheEarth.Entities.Factories
{
    // Enum pentru tipuri specifice de copaci (opțional, pentru variație)
    public enum TreeType { GenericOak, Pine, Birch, CursedWillow }

    public class TreeFactory : IEntityFactory
    {
        private ContentManager _content;
        private Dictionary<TreeType, Texture2D> _treeTextures = new Dictionary<TreeType, Texture2D>();
        // Sau un SpriteSheet dacă ai animații pentru copaci (ex. legănat în vânt, cădere)

        public TreeFactory(ContentManager content)
        {
            _content = content;
            LoadAssets();
        }

        private void LoadAssets()
        {
            // Încarcă texturile pentru diferite tipuri de copaci
            // Numele fișierelor sunt exemple
            try
            {
                _treeTextures[TreeType.GenericOak] = _content.Load<Texture2D>("Sprites/World/Resources/tree_oak");
                _treeTextures[TreeType.Pine] = _content.Load<Texture2D>("Sprites/World/Resources/tree_pine");
                // Adaugă și pentru Birch, CursedWillow etc. dacă ai texturi
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tree textures: {ex.Message}");
            }
        }

        public Entity CreateEntity(Vector2 position)
        {
            // Pentru acest exemplu, creăm un GenericOak.
            // Într-un sistem mai complex, ai putea pasa TreeType ca argument.
            return CreateTree(position, TreeType.GenericOak);
        }

        public Entity CreateTree(Vector2 position, TreeType treeType)
        {
            if (!_treeTextures.TryGetValue(treeType, out Texture2D texture))
            {
                System.Diagnostics.Debug.WriteLine($"Texture for TreeType {treeType} not found. Using default or skipping.");
                if (!_treeTextures.TryGetValue(TreeType.GenericOak, out texture)) // Fallback
                    return null; // Sau o entitate placeholder
            }

            Entity tree = new Entity($"Tree_{treeType}");
            var transform = new TransformComponent { Position = position, Scale = Vector2.One * 3f };
            tree.AddComponent(transform);
            // SpriteComponent
            float visualRootAnchorY = texture.Height * 0.75f;
            var spriteComp = new SpriteComponent(texture);
            // Ajustează originea la baza copacului pentru plasare și sortare corectă
            spriteComp.Origin = new Vector2(texture.Width / 2f, visualRootAnchorY);
            spriteComp.LayerDepth = 0.4f + (position.Y / (Utils.Settings.DefaultWorldHeight * Utils.Settings.WorldTileHeight * 2f)); // Adâncime bazată pe Y
            tree.AddComponent(spriteComp);

            // ResourceSourceComponent
            List<DropChance> drops = new List<DropChance>();
            float health = 50f; // Default health
            string tool = "Axe";

            switch (treeType)
            {
                case TreeType.GenericOak:
                case TreeType.Birch:
                    drops.Add(new DropChance(ItemType.WoodLog, 2, 4, 1.0f)); // 100% șansă pentru bușteni
                    drops.Add(new DropChance(ItemType.Flint, 0, 1, 0.2f));   // 20% șansă pentru un cremene
                    health = 60f;
                    break;
                case TreeType.Pine:
                    drops.Add(new DropChance(ItemType.WoodLog, 3, 5, 1.0f));
                    // Pine poate da și rășină etc.
                    health = 70f;
                    break;
                case TreeType.CursedWillow:
                    drops.Add(new DropChance(ItemType.WoodLog, 1, 3, 0.8f)); // Lemn mai puțin sau de calitate slabă
                    // Poate dropa iteme specifice "blestemate"
                    drops.Add(new DropChance(ItemType.Coal, 0, 2, 0.15f)); // Umbră de cărbune
                    health = 40f;
                    tool = "MagicAxe"; // Poate necesită unealtă specială
                    break;
            }
            tree.AddComponent(new ResourceSourceComponent(treeType.ToString() + " Tree", health, drops, tool));

            // ColliderComponent (pentru interacțiune și pentru a bloca mișcarea)
            // Dreptunghiul trebuie să fie la baza copacului, unde e trunchiul
            // Lățimea mai mică decât sprite-ul, înălțimea și mai mică.
            float colliderWidth = texture.Width * 0.2f * tree.GetComponent<TransformComponent>().Scale.X; // Trunchi și mai subțire pentru coliziune
            float colliderHeight = texture.Height * 0.1f * tree.GetComponent<TransformComponent>().Scale.Y;
            // Offset Y pentru a plasa coliderul la baza sprite-ului, presupunând originea sprite-ului e la (width/2, height)
            Vector2 colliderOffset = new Vector2(0, -colliderHeight / 2f);
            if (spriteComp.Origin.Y < texture.Height * 1f) // Dacă originea NU e la bază
            {
                // Ajustează offsetul dacă originea e în centru sau altundeva
                colliderOffset = new Vector2(0, (spriteComp.Texture.Height * tree.GetComponent<TransformComponent>().Scale.Y / 2f) - (colliderHeight / 2f) - (spriteComp.Origin.Y * tree.GetComponent<TransformComponent>().Scale.Y - spriteComp.Texture.Height * tree.GetComponent<TransformComponent>().Scale.Y / 2f));
                // Linia de mai sus e complexă, mai simplu e să fixezi o convenție pentru Originea sprite-ului
                // Dacă Originea e la (tex.W/2, tex.H) [baza sprite-ului]:
                colliderOffset = new Vector2(0, -colliderHeight / 100f-2f); // Coliderul e centrat vertical la baza sprite-ului
            }


            tree.AddComponent(new ColliderComponent(
                new Rectangle(0, 0, (int)colliderWidth, (int)colliderHeight), // Dimensiuni relative la centrul coliderului
                colliderOffset,
                isSolid: true
             ));
            //System.Diagnostics.Debug.WriteLine($"Created {treeType} at {position}");
            return tree;
        }
    }
}