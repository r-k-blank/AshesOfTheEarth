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
            var campfireTransform = new TransformComponent { Position = position, Scale = Vector2.One * 3f };
            campfire.AddComponent(campfireTransform);

            var animations = new Dictionary<string, AnimationData>();
            var burningFrames = new List<AnimationFrame>();
            for (int i = 0; i < _campfireSpriteSheet.TotalFrames; i++)
            {
                burningFrames.Add(new AnimationFrame(i, 0.15f));
            }
            animations.Add("Burning", new AnimationData("Burning", burningFrames, true));

            campfire.AddComponent(new AnimationComponent(_campfireSpriteSheet, animations));
            campfire.GetComponent<AnimationComponent>().PlayAnimation("Burning");

            var spriteComp = new SpriteComponent(); 
            campfire.AddComponent(spriteComp);
            campfire.AddComponent(new PlaceableComponent(ItemType.Campfire, 0));

            int frameW = 32; // _campfireSpriteSheet.FrameWidth;
            int frameH = 32; // _campfireSpriteSheet.FrameHeight;

            float colliderWidthPercentage = 0.8f; // Mai lat
            float colliderHeightPercentage = 0.5f; // Partea de jos

            float actualColliderWidth = frameW * colliderWidthPercentage * campfireTransform.Scale.X;
            float actualColliderHeight = frameH * colliderHeightPercentage * campfireTransform.Scale.Y;

            // Baza coliderului la Transform.Position.Y, extins în sus
            Vector2 colliderOffset = new Vector2(0, -actualColliderHeight / 2f);

            campfire.AddComponent(new ColliderComponent(
                new Rectangle(0, 0, (int)actualColliderWidth, (int)actualColliderHeight),
                colliderOffset,
                true
            ));
            campfire.AddComponent(new LightEmitterComponent(radius: 250f, intensity: 0.8f, color: new Color(255, 180, 100), flickerIntensity: 0.08f, flickerSpeed: 5f));

            return campfire;
        }
    }
}