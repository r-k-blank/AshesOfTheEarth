using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Gameplay.Items;
using AshesOfTheEarth.Graphics.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using AshesOfTheEarth.Gameplay.Lighting;

namespace AshesOfTheEarth.Entities.Factories
{
    public class CampfireFactory : IEntityFactory
    {
        private readonly ContentManager _content;
        private SpriteSheet _campfireSpriteSheet;

        public CampfireFactory(ContentManager content)
        {
            _content = content;
            LoadAssets();
        }

        private void LoadAssets()
        {
            try
            {
                Texture2D texture = _content.Load<Texture2D>("Sprites/World/Objects/campfire_spritesheet");
                _campfireSpriteSheet = new SpriteSheet(texture, 32, 32);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading campfire spritesheet: {ex.Message}");
            }
        }

        public Entity CreateEntity(Vector2 position)
        {
            if (_campfireSpriteSheet == null) return null;

            Entity campfire = new Entity("Campfire");
            campfire.AddComponent(new TransformComponent { Position = position, Scale = Vector2.One * 3f });

            var animations = new Dictionary<string, AnimationData>();
            var burningFrames = new List<AnimationFrame>();
            for (int i = 0; i < _campfireSpriteSheet.TotalFrames; i++)
            {
                burningFrames.Add(new AnimationFrame(i, 0.15f));
            }
            animations.Add("Burning", new AnimationData("Burning", burningFrames, true));

            campfire.AddComponent(new AnimationComponent(_campfireSpriteSheet, animations));
            campfire.GetComponent<AnimationComponent>().PlayAnimation("Burning");

            campfire.AddComponent(new SpriteComponent());
            campfire.AddComponent(new PlaceableComponent(ItemType.Campfire, 0));


            var collider = new ColliderComponent(
                new Rectangle(0, 0, (int)(28 * 1.5f), (int)(20 * 1.5f)),
                new Vector2(0, 5 * 1.5f),
                true
            );
            campfire.AddComponent(collider);

            campfire.AddComponent(new LightEmitterComponent(radius: 250f, intensity: 0.8f, color: new Color(255, 180, 100), flickerIntensity: 0.08f, flickerSpeed: 5f));

            return campfire;
        }
    }
}