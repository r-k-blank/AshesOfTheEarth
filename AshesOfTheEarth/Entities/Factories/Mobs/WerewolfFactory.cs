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
    public class WerewolfFactory : BaseMobFactory
    {
        private SpriteSheet _brownSheet, _blackSheet, _whiteSheet;
        private const string BROWN_PATH = "Sprites/Mobs/Werewolf_Spritelist"; // Placeholder
        private const string BLACK_PATH = "Sprites/Mobs/Red_Werewolf_Spritelist"; // Placeholder
        private const string WHITE_PATH = "Sprites/Mobs/White_Werewolf_Spritelist"; // Placeholder

        public WerewolfFactory(ContentManager content) : base(content)
        {
            try { _brownSheet = new SpriteSheet(_content.Load<Texture2D>(BROWN_PATH), 128, 128); } // Assume 128x128
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load WerewolfBrown sheet: {e.Message}"); }
            try { _blackSheet = new SpriteSheet(_content.Load<Texture2D>(BLACK_PATH), 128, 128); }
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load WerewolfBlack sheet: {e.Message}"); }
            try { _whiteSheet = new SpriteSheet(_content.Load<Texture2D>(WHITE_PATH), 128, 128); }
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load WerewolfWhite sheet: {e.Message}"); }
        }

        public override Entity CreateEntity(Vector2 position)
        {
            return CreateWerewolf(position, MobType.WerewolfBrown); // Default
        }

        public Entity CreateWerewolf(Vector2 position, MobType werewolfType)
        {
            SpriteSheet activeSheet = null;
            switch (werewolfType)
            {
                case MobType.WerewolfBrown: activeSheet = _brownSheet; break;
                case MobType.WerewolfBlack: activeSheet = _blackSheet; break;
                case MobType.WerewolfWhite: activeSheet = _whiteSheet; break;
                default: return null;
            }

            if (activeSheet == null)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot create {werewolfType}: Spritesheet not loaded.");
                return null;
            }

            Entity werewolf = new Entity(werewolfType.ToString());
            werewolf.AddComponent(new TransformComponent { Position = position, Scale = Vector2.One * 1.8f });
            werewolf.AddComponent(new SpriteComponent());
            werewolf.AddComponent(new AnimationComponent(activeSheet, GetWerewolfAnimations(activeSheet)));
            werewolf.AddComponent(new AIComponent(position));

            var loot = new List<LootDropInfo> { new LootDropInfo(ItemType.Flint, 1, 3, 0.6f) }; // Placeholder WerewolfPelt
            werewolf.AddComponent(new LootTableComponent(loot));

            var collider = new ColliderComponent(new Rectangle(0, 0, (int)(35 * 2.1f), (int)(60 * 2.1f)), new Vector2(0, 10 * 2.1f), true);
            werewolf.AddComponent(collider);

            float health = 90f;
            float damage = 22f;
            float speed = 75f;

            if (werewolfType == MobType.WerewolfBlack) { health = 110f; damage = 28f; speed = 80f; }
            if (werewolfType == MobType.WerewolfWhite) { health = 100f; damage = 25f; speed = 85f; /* White might be faster/agile */ }

            werewolf.AddComponent(new HealthComponent(health));
            werewolf.AddComponent(new MobStatsComponent { Damage = damage, AttackRange = 100f, AggroRange = 320f, MovementSpeed = speed, RunSpeedMultiplier = 1.8f });

            return werewolf;
        }

        private Dictionary<string, AnimationData> GetWerewolfAnimations(SpriteSheet sheet)
        {
            var anims = new Dictionary<string, AnimationData>();
            float frameTime = 0.12f;
            // Attack_1,Attack_2, Attack_3, Dead, Hurt, Idle, Jump, Run, Run+Attack, walk
            anims.Add("Idle", new AnimationData("Idle", CreateMobFrames(sheet, 12, 1, frameTime * 1.8f), true));
            anims.Add("Walk", new AnimationData("Walk", CreateMobFrames(sheet, 12, 9, frameTime * 1.1f), true));
            anims.Add("Run", new AnimationData("Run", CreateMobFrames(sheet, 24, 8, frameTime * 0.7f), true));
            anims.Add("Jump", new AnimationData("Jump", CreateMobFrames(sheet, 36, 7, frameTime * 0.9f), false)); // Jump can be part of attack or separate
            anims.Add("Attack_1", new AnimationData("Attack_1", CreateMobFrames(sheet, 84, 4, frameTime * 0.8f), false));
            anims.Add("Attack_2", new AnimationData("Attack_2", CreateMobFrames(sheet, 60, 5, frameTime * 0.8f), false));
            anims.Add("Attack_3", new AnimationData("Attack_3", CreateMobFrames(sheet, 72, 3, frameTime * 0.85f), false)); // Could be a howl or stronger attack
            anims.Add("Run_Attack", new AnimationData("Run_Attack", CreateMobFrames(sheet, 48, 6, frameTime * 0.75f), false)); // Lunge attack
            anims.Add("Hurt", new AnimationData("Hurt", CreateMobFrames(sheet, 96, 1, frameTime), false));
            anims.Add("Dead", new AnimationData("Dead", CreateMobFrames(sheet, 108, 1, frameTime * 1.2f), false));
            return anims;
        }
    }
}