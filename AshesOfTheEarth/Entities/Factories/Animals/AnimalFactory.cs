using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Entities.Mobs; // For MobType enum if animals are mixed, or a new AnimalType enum
using AshesOfTheEarth.Gameplay.Items;
using AshesOfTheEarth.Graphics.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using AshesOfTheEarth.Entities.Factories.Mobs;

namespace AshesOfTheEarth.Entities.Factories.Animals
{
    public class AnimalFactory : BaseMobFactory // Inherits from BaseMobFactory for CreateMobFrames
    {
        private SpriteSheet _deerSheet;
        private SpriteSheet _rabbit_idle_Sheet;
        private SpriteSheet _rabbit_running_Sheet;
        private const string DEER_PATH = "Sprites/Animals/Rabbit_Idle";     
        private const string RABBIT_IDLE_PATH = "Sprites/Animals/Rabbit_Idle";
        private const string RABBIT_RUNNING_PATH = "Sprites/Animals/Rabbit_Running";


        public AnimalFactory(ContentManager content) : base(content)
        {
            try { _deerSheet = new SpriteSheet(_content.Load<Texture2D>(DEER_PATH), 32, 32); } // Assume sizes
            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load Deer sheet: {e.Message}"); }
            try { 
                    _rabbit_idle_Sheet = new SpriteSheet(_content.Load<Texture2D>(RABBIT_IDLE_PATH), 32, 32);
                    _rabbit_running_Sheet = new SpriteSheet(_content.Load<Texture2D>(RABBIT_IDLE_PATH), 32, 32);
                }

            catch (Exception e) { System.Diagnostics.Debug.WriteLine($"Failed to load Rabbit sheet: {e.Message}"); }
        }

        public override Entity CreateEntity(Vector2 position)
        {
            return CreateAnimal(position, MobType.Rabbit); // Default
        }

        public Entity CreateAnimal(Vector2 position, MobType animalType)
        {
            SpriteSheet activeSheet = null;
            string entityTag = animalType.ToString();
            List<LootDropInfo> loot = new List<LootDropInfo>();
            float health = 10f;
            float moveSpeed = 80f;
            float aggroRange = 150f; // Range at which they'll notice player and flee
            Vector2 scale = Vector2.One * 1.7f;
            Rectangle colliderRect = new Rectangle(0, 0, (int)(25 * scale.X), (int)(40 * scale.Y));
            Vector2 colliderOffset = new Vector2(0, 5 * scale.Y);


            switch (animalType)
            {
                case MobType.Deer:
                    activeSheet = _deerSheet;
                    loot.Add(new LootDropInfo(ItemType.Berries, 2, 4, 1f)); // Placeholder RawMeat
                    loot.Add(new LootDropInfo(ItemType.Coal, 1, 2, 0.5f)); // Placeholder LeatherHide
                    health = 25f;
                    moveSpeed = 90f;
                    scale = Vector2.One * 1.9f;
                    colliderRect = new Rectangle(0, 0, (int)(30 * scale.X), (int)(50 * scale.Y));
                    colliderOffset = new Vector2(0, 8 * scale.Y);
                    break;
                case MobType.Rabbit:
                    activeSheet = _rabbit_idle_Sheet;
                    loot.Add(new LootDropInfo(ItemType.Berries, 1, 2, 1f)); // Placeholder SmallMeat
                    health = 10f;
                    moveSpeed = 110f;
                    aggroRange = 120f;
                    scale = Vector2.One * 1.5f;
                    colliderRect = new Rectangle(0, 0, (int)(15 * scale.X), (int)(20 * scale.Y));
                    colliderOffset = new Vector2(0, 2 * scale.Y);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"AnimalFactory: Unknown animal type {animalType}");
                    return null;
            }

            if (activeSheet == null)
            {
                System.Diagnostics.Debug.WriteLine($"Cannot create {animalType}: Spritesheet not loaded.");
                return null;
            }

            Entity animal = new Entity(entityTag);
            animal.AddComponent(new TransformComponent { Position = position, Scale = scale });
            animal.AddComponent(new SpriteComponent());
            animal.AddComponent(new AnimationComponent(activeSheet, GetAnimalAnimations(animalType, activeSheet)));

            var aiComp = new AIComponent(position) { MaxPatrolRadius = 200f };
            animal.AddComponent(aiComp);
            animal.AddComponent(new HealthComponent(health));
            animal.AddComponent(new LootTableComponent(loot));

            // Animals are non-aggressive, so MobStatsComponent is mainly for their speed and detection.
            // They won't use Damage or AttackRange unless they become aggressive variants.
            animal.AddComponent(new MobStatsComponent { MovementSpeed = moveSpeed, AggroRange = aggroRange, RunSpeedMultiplier = 2.0f });
            animal.AddComponent(new ColliderComponent(colliderRect, colliderOffset, true));

            return animal;
        }

        private Dictionary<string, AnimationData> GetAnimalAnimations(MobType animalType, SpriteSheet sheet)
        {
            var anims = new Dictionary<string, AnimationData>();
            float frameTime = 0.2f;

            if (animalType == MobType.Deer)
            {
                frameTime = 0.18f;
                anims.Add("Idle", new AnimationData("Idle", CreateMobFrames(sheet, 0, 11, frameTime * 1.5f), true));
                anims.Add("Walk", new AnimationData("Walk", CreateMobFrames(sheet, 4, 6, frameTime), true));
                anims.Add("Run", new AnimationData("Run", CreateMobFrames(sheet, 10, 5, frameTime * 0.6f), true)); // Fleeing
                anims.Add("Hurt", new AnimationData("Hurt", CreateMobFrames(sheet, 15, 2, frameTime), false));
                anims.Add("Dead", new AnimationData("Dead", CreateMobFrames(sheet, 17, 5, frameTime * 1.2f), false));
            }
            else if (animalType == MobType.Rabbit)
            {
                frameTime = 0.15f;
                anims.Add("Idle", new AnimationData("Idle", CreateMobFrames(sheet, 0, 11, frameTime * 1.8f), true));
                anims.Add("Walk", new AnimationData("Walk", CreateMobFrames(sheet, 3, 5, frameTime * 1.2f), true)); // Hopping
                anims.Add("Run", new AnimationData("Run", CreateMobFrames(sheet, 8, 4, frameTime * 0.5f), true));  // Fast hopping
                anims.Add("Hurt", new AnimationData("Hurt", CreateMobFrames(sheet, 12, 2, frameTime), false));
                anims.Add("Dead", new AnimationData("Dead", CreateMobFrames(sheet, 14, 4, frameTime * 1.1f), false));
            }
            return anims;
        }
    }
}