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
    public class MinotaurFactory : BaseMobFactory
    {
        private SpriteSheet _alphaSheet, _betaSheet, _gammaSheet;
        private const string ALPHA_PATH = "Sprites/Mobs/Minotaur_1_Spritelist"; // Placeholder
        private const string BETA_PATH = "Sprites/Mobs/Minotaur_2_Spritelist";   // Placeholder
        private const string GAMMA_PATH = "Sprites/Mobs/Minotaur_3_Spritelist"; // Placeholder


        public MinotaurFactory(ContentManager content) : base(content)
        {
            try { _alphaSheet = new SpriteSheet(_content.Load<Texture2D>(ALPHA_PATH), 128, 128); } // Assume 128x128
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load MinotaurAlpha sheet: {e.Message}"); }
            try { _betaSheet = new SpriteSheet(_content.Load<Texture2D>(BETA_PATH), 128, 128); }
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load MinotaurBeta sheet: {e.Message}"); }
            try { _gammaSheet = new SpriteSheet(_content.Load<Texture2D>(GAMMA_PATH), 128, 128); }
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load MinotaurGamma sheet: {e.Message}"); }
        }

        public override Entity CreateEntity(Vector2 position)
        {
            return CreateMinotaur(position, MobType.MinotaurAlpha); // Default
        }

        public Entity CreateMinotaur(Vector2 position, MobType minotaurType)
        {
            SpriteSheet activeSheet = null;
            switch (minotaurType)
            {
                case MobType.MinotaurAlpha: activeSheet = _alphaSheet; break;
                case MobType.MinotaurBeta: activeSheet = _betaSheet; break;
                case MobType.MinotaurGamma: activeSheet = _gammaSheet; break;
                default: return null;
            }

            if (activeSheet == null)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot create {minotaurType}: Spritesheet not loaded.");
                return null;
            }

            Entity minotaur = new Entity(minotaurType.ToString());
            minotaur.AddComponent(new TransformComponent { Position = position, Scale = Vector2.One * 1.8f });
            minotaur.AddComponent(new SpriteComponent());
            minotaur.AddComponent(new AnimationComponent(activeSheet, GetMinotaurAnimations(activeSheet)));
            minotaur.AddComponent(new AIComponent(position));

            var loot = new List<LootDropInfo> { new LootDropInfo(ItemType.WoodLog, 1, 2, 0.9f) }; // Placeholder MinotaurHorn
            minotaur.AddComponent(new LootTableComponent(loot));

            var collider = new ColliderComponent(new Rectangle(0, 0, (int)(40 * 2f), (int)(60 * 2f)), new Vector2(0, 15 * 2f), true);
            minotaur.AddComponent(collider);

            float health = 100f;
            float damage = 20f;
            float speed = 50f;

            if (minotaurType == MobType.MinotaurBeta) { health = 120f; damage = 25f; speed = 45f; }
            else if (minotaurType == MobType.MinotaurGamma) { health = 150f; damage = 30f; speed = 40f; }

            minotaur.AddComponent(new HealthComponent(health));
            minotaur.AddComponent(new MobStatsComponent { Damage = damage, AttackRange = 100f, AggroRange = 300f, MovementSpeed = speed });

            return minotaur;
        }

        private Dictionary<string, AnimationData> GetMinotaurAnimations(SpriteSheet sheet)
        {
            var anims = new Dictionary<string, AnimationData>();
            float frameTime = 0.18f;
            // Attack, Dead, Hurt, Idle, Walk (5 anims)
            anims.Add("Idle", new AnimationData("Idle", CreateMobFrames(sheet, 0, 8, frameTime * 1.5f), true));
            anims.Add("Walk", new AnimationData("Walk", CreateMobFrames(sheet, 12, 11, frameTime), true));
            anims.Add("Run", new AnimationData("Run", CreateMobFrames(sheet, 12, 11, frameTime * 0.7f), true));
            anims.Add("Attack", new AnimationData("Attack", CreateMobFrames(sheet, 24, 4, frameTime * 0.9f), false));
            anims.Add("Hurt", new AnimationData("Hurt", CreateMobFrames(sheet, 36, 2, frameTime), false));
            anims.Add("Dead", new AnimationData("Dead", CreateMobFrames(sheet, 48, 4, frameTime * 1.1f), false));
            return anims;
        }
    }
}
