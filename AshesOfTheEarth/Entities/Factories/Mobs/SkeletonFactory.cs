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
    public class SkeletonFactory : BaseMobFactory
    {
        private SpriteSheet _spearmanSheet;
        private SpriteSheet _warriorSheet;
        private const string SPEARMAN_PATH = "Sprites/Mobs/Skeleton_Spearman_Spritelist"; // Placeholder
        private const string WARRIOR_PATH = "Sprites/Mobs/Skeleton_Warrior_Spritelist";   // Placeholder

        public SkeletonFactory(ContentManager content) : base(content)
        {
            try { _spearmanSheet = new SpriteSheet(_content.Load<Texture2D>(SPEARMAN_PATH), 128, 128); } 
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load Spearman sheet: {e.Message}"); }
            try { _warriorSheet = new SpriteSheet(_content.Load<Texture2D>(WARRIOR_PATH), 128, 128); }   // Assume 128x128 frames
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load Warrior sheet: {e.Message}"); }
        }

        public override Entity CreateEntity(Vector2 position)
        {
            return CreateSkeleton(position, MobType.SkeletonWarrior); // Default
        }

        public Entity CreateSkeleton(Vector2 position, MobType skeletonType)
        {
            if ((skeletonType == MobType.SkeletonSpearman && _spearmanSheet == null) ||
                (skeletonType == MobType.SkeletonWarrior && _warriorSheet == null))
            {
                System.Diagnostics.Debug.WriteLine($"Cannot create {skeletonType}: Spritesheet not loaded.");
                return null;
            }

            Entity skeleton = new Entity(skeletonType.ToString());
            var mobTransform = new TransformComponent { Position = position, Scale = Vector2.One * 1.8f }; // Scala ta
            skeleton.AddComponent(mobTransform);

            SpriteSheet activeSheet = (skeletonType == MobType.SkeletonSpearman) ? _spearmanSheet : _warriorSheet;
            if (activeSheet == null) { /* gestionează eroare */ return null; }

            Dictionary<string, AnimationData> animations = (skeletonType == MobType.SkeletonSpearman)
                ? GetSpearmanAnimations(activeSheet)
                : GetWarriorAnimations(activeSheet);
            var animComp = new AnimationComponent(activeSheet, animations);
            skeleton.AddComponent(animComp);


            var spriteComp = new SpriteComponent();
            skeleton.AddComponent(spriteComp);
            skeleton.AddComponent(new AIComponent(position));

            var loot = new List<LootDropInfo> { new LootDropInfo(ItemType.StoneShard, 1, 3, 0.8f) }; // Placeholder Bone item
            skeleton.AddComponent(new LootTableComponent(loot));

            int frameW = 128; // activeSheet.FrameWidth;
            int frameH = 128; // activeSheet.FrameHeight;

            // Scheleții sunt mai subțiri
            float colliderWidthPercentage = 0.30f;
            float colliderHeightPercentage = 0.65f; // Partea principală a oaselor

            float actualColliderWidth = frameW * colliderWidthPercentage * mobTransform.Scale.X;
            float actualColliderHeight = frameH * colliderHeightPercentage * mobTransform.Scale.Y;

            Vector2 mobColliderOffset = new Vector2(0, -actualColliderHeight / 2f);

            skeleton.AddComponent(new ColliderComponent(
                new Rectangle(0, 0, (int)actualColliderWidth, (int)actualColliderHeight),
                mobColliderOffset,
                true
            ));

            if (skeletonType == MobType.SkeletonSpearman)
            {
                skeleton.AddComponent(new AnimationComponent(_spearmanSheet, GetSpearmanAnimations(_spearmanSheet)));
                skeleton.AddComponent(new HealthComponent(30f));
                skeleton.AddComponent(new MobStatsComponent { Damage = 8f, AttackRange = 100f, AggroRange = 280f, MovementSpeed = 60f });
            }
            else // SkeletonWarrior
            {
                skeleton.AddComponent(new AnimationComponent(_warriorSheet, GetWarriorAnimations(_warriorSheet)));
                skeleton.AddComponent(new HealthComponent(50f));
                skeleton.AddComponent(new MobStatsComponent { Damage = 12f, AttackRange = 100f, AggroRange = 260f, MovementSpeed = 55f });
            }
            return skeleton;
        }

        private Dictionary<string, AnimationData> GetSpearmanAnimations(SpriteSheet sheet)
        {
            var anims = new Dictionary<string, AnimationData>();
            float frameTime = 0.15f;
            anims.Add("Idle", new AnimationData("Idle", CreateMobFrames(sheet, 0, 6, frameTime * 1.5f), true));
            anims.Add("Walk", new AnimationData("Walk", CreateMobFrames(sheet, 10, 6, frameTime), true));
            anims.Add("Run", new AnimationData("Run", CreateMobFrames(sheet, 20, 5, frameTime * 0.7f), true));
            anims.Add("Attack_1", new AnimationData("Attack_1", CreateMobFrames(sheet, 40, 3, frameTime * 0.8f), false));
            anims.Add("Attack_2", new AnimationData("Attack_2", CreateMobFrames(sheet, 50, 3, frameTime * 0.8f), false));
            anims.Add("Run_Attack", new AnimationData("Run_Attack", CreateMobFrames(sheet, 30, 4, frameTime * 0.75f), false));
            anims.Add("Protect", new AnimationData("Protect", CreateMobFrames(sheet, 60, 1, frameTime * 3f), false)); // Single frame, long duration
            anims.Add("Hurt", new AnimationData("Hurt", CreateMobFrames(sheet, 80, 2, frameTime * 0.9f), false));
            anims.Add("Fall", new AnimationData("Fall", CreateMobFrames(sheet, 70, 5, frameTime * 1.1f), false)); // Longer hurt/knockdown
            anims.Add("Dead", new AnimationData("Dead", CreateMobFrames(sheet, 90, 4, frameTime * 1.2f), false));
            return anims;
        }

        private Dictionary<string, AnimationData> GetWarriorAnimations(SpriteSheet sheet)
        {
            var anims = new Dictionary<string, AnimationData>();
            float frameTime = 0.16f;
            anims.Add("Idle", new AnimationData("Idle", CreateMobFrames(sheet, 0, 6, frameTime * 1.5f), true));
            anims.Add("Walk", new AnimationData("Walk", CreateMobFrames(sheet, 10, 6, frameTime), true));
            anims.Add("Run", new AnimationData("Run", CreateMobFrames(sheet, 20, 7, frameTime * 0.7f), true));
            anims.Add("Attack_1", new AnimationData("Attack_1", CreateMobFrames(sheet, 60, 3, frameTime * 0.8f), false));
            anims.Add("Attack_2", new AnimationData("Attack_2", CreateMobFrames(sheet, 40, 4, frameTime * 0.8f), false));
            anims.Add("Attack_3", new AnimationData("Attack_3", CreateMobFrames(sheet, 50, 5, frameTime * 0.85f), false));
            anims.Add("Run_Attack", new AnimationData("Run_Attack", CreateMobFrames(sheet, 30, 6, frameTime * 0.75f), false));
            anims.Add("Protect", new AnimationData("Protect", CreateMobFrames(sheet, 70, 0, frameTime * 3f), false));
            anims.Add("Hurt", new AnimationData("Hurt", CreateMobFrames(sheet, 80, 1, frameTime * 0.9f), false));
            anims.Add("Dead", new AnimationData("Dead", CreateMobFrames(sheet, 90, 3, frameTime * 1.2f), false));
            return anims;
        }
    }
}