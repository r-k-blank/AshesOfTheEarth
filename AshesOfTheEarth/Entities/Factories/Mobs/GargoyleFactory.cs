using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Entities.Mobs;
using AshesOfTheEarth.Gameplay.Items;
using AshesOfTheEarth.Graphics.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace AshesOfTheEarth.Entities.Factories.Mobs
{
    public class GargoyleFactory : BaseMobFactory
    {
        private SpriteSheet _redSheet, _greenSheet, _blueSheet;
        private const string RED_PATH = "Sprites/Mobs/Gargona_3_Spritelist";     // Placeholder
        private const string GREEN_PATH = "Sprites/Mobs/Gargona_1_Spritelist"; // Placeholder
        private const string BLUE_PATH = "Sprites/Mobs/Gargona_2_Spritelist";   // Placeholder

        public GargoyleFactory(ContentManager content) : base(content)
        {
            try { _redSheet = new SpriteSheet(_content.Load<Texture2D>(RED_PATH), 128, 128); } // Assume 128x128
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load GargoyleRed sheet: {e.Message}"); }
            try { _greenSheet = new SpriteSheet(_content.Load<Texture2D>(GREEN_PATH), 128, 128); }
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load GargoyleGreen sheet: {e.Message}"); }
            try { _blueSheet = new SpriteSheet(_content.Load<Texture2D>(BLUE_PATH), 128, 128); }
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load GargoyleBlue sheet: {e.Message}"); }
        }

        public override Entity CreateEntity(Vector2 position)
        {
            return CreateGargoyle(position, MobType.GargoyleRed); // Default
        }

        public Entity CreateGargoyle(Vector2 position, MobType gargoyleType)
        {
            SpriteSheet activeSheet = null;
            switch (gargoyleType)
            {
                case MobType.GargoyleRed: activeSheet = _redSheet; break;
                case MobType.GargoyleGreen: activeSheet = _greenSheet; break;
                case MobType.GargoyleBlue: activeSheet = _blueSheet; break;
                default: return null;
            }

            if (activeSheet == null)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot create {gargoyleType}: Spritesheet not loaded.");
                return null;
            }

            Entity gargoyle = new Entity(gargoyleType.ToString());
            var mobTransform = new TransformComponent { Position = position, Scale = Vector2.One * 1.8f }; // Scala ta
            gargoyle.AddComponent(mobTransform);

            var animComp = new AnimationComponent(activeSheet, GetGargoyleAnimations(activeSheet));
            gargoyle.AddComponent(animComp);

            var spriteComp = new SpriteComponent();
            gargoyle.AddComponent(spriteComp);
            gargoyle.AddComponent(new AnimationComponent(activeSheet, GetGargoyleAnimations(activeSheet)));
            gargoyle.AddComponent(new AIComponent(position));

            var loot = new List<LootDropInfo> { new LootDropInfo(ItemType.IronOre, 1, 1, 0.7f) }; // Placeholder GargoyleShard
            gargoyle.AddComponent(new LootTableComponent(loot));

            int frameW = 128; // activeSheet.FrameWidth;
            int frameH = 128; // activeSheet.FrameHeight;

            // Gargoilii pot fi mai lați și mai solizi
            float colliderWidthPercentage = 0.45f;
            float colliderHeightPercentage = 0.7f; // Partea solidă a corpului

            float actualColliderWidth = frameW * colliderWidthPercentage * mobTransform.Scale.X;
            float actualColliderHeight = frameH * colliderHeightPercentage * mobTransform.Scale.Y;

            Vector2 mobColliderOffset = new Vector2(0, -actualColliderHeight / 2f);

            gargoyle.AddComponent(new ColliderComponent(
                new Rectangle(0, 0, (int)actualColliderWidth, (int)actualColliderHeight),
                mobColliderOffset,
                true
            ));

            gargoyle.AddComponent(new HealthComponent(70f));
            var mobStats = new MobStatsComponent { Damage = 15f, AttackRange = 100f, AggroRange = 220f, MovementSpeed = 65f };
            if (gargoyleType == MobType.GargoyleGreen) { mobStats.Damage = 18f; mobStats.MovementSpeed = 70f; }
            if (gargoyleType == MobType.GargoyleBlue) { mobStats.Damage = 12f; /* Blue might have more health or ranged attack*/ }
            gargoyle.AddComponent(mobStats);

            return gargoyle;
        }

        private Dictionary<string, AnimationData> GetGargoyleAnimations(SpriteSheet sheet)
        {
            var anims = new Dictionary<string, AnimationData>();
            float frameTime = 0.14f;
            // Attack_1,Attack_2, Attack_3, Dead, Hurt, Idle, Idle_2, Run, Special, Walk
            anims.Add("Idle", new AnimationData("Idle", CreateMobFrames(sheet, 0, 6, frameTime * 1.6f), true));
            anims.Add("Idle_2", new AnimationData("Idle_2", CreateMobFrames(sheet, 16, 3, frameTime * 1.7f), true)); // Alt idle
            anims.Add("Walk", new AnimationData("Walk", CreateMobFrames(sheet, 32, 12, frameTime * 1.1f), true));
            anims.Add("Run", new AnimationData("Run", CreateMobFrames(sheet, 48, 6, frameTime * 0.8f), true));
            anims.Add("Attack_1", new AnimationData("Attack_1", CreateMobFrames(sheet, 64, 15, frameTime * 0.9f), false));
            anims.Add("Attack_2", new AnimationData("Attack_2", CreateMobFrames(sheet, 80, 6, frameTime * 0.9f), false));
            anims.Add("Attack_3", new AnimationData("Attack_3", CreateMobFrames(sheet, 96, 9, frameTime * 0.95f), false));
            anims.Add("Special", new AnimationData("Special_Petrify", CreateMobFrames(sheet, 112, 4, frameTime * 1.2f), false)); // Petrify
            anims.Add("Hurt", new AnimationData("Hurt", CreateMobFrames(sheet, 128, 2, frameTime), false));
            anims.Add("Dead", new AnimationData("Dead", CreateMobFrames(sheet, 144, 2, frameTime * 1.1f), false));
            return anims;
        }
    }
}