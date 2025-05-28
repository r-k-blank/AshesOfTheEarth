using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Graphics.Animation;
using System.Collections.Generic;
using System;
using AshesOfTheEarth.Gameplay.Lighting;
using AshesOfTheEarth.Core; // Pentru PlayerAnimationSetType

namespace AshesOfTheEarth.Entities.Factories
{
    public class PlayerFactory : IEntityFactory // Nu mai are nevoie de constructorul care primește calea
    {
        private readonly ContentManager _content;

        // Nu mai avem _playerSpriteSheet sau _currentCharacterSpriteSheetPath ca membrii ai clasei
        // Ele vor fi locale metodei CreateEntity

        public PlayerFactory(ContentManager content)
        {
            _content = content;
        }

        // Metoda CreateEntity primește acum calea și tipul de animație
        public Entity CreateEntity(Vector2 position, string spriteSheetPath, PlayerAnimationSetType animationType, int frameWidth = 128, int frameHeight = 128)
        {
            SpriteSheet loadedSpriteSheet;
            try
            {
                Texture2D texture = _content.Load<Texture2D>(spriteSheetPath);
                // Presupunem că toate sprite sheet-urile de player au frame-uri de 128x128
                // Dacă nu, va trebui să pasezi și dimensiunile frame-ului sau să le stochezi în CharacterSelectionData
                loadedSpriteSheet = new SpriteSheet(texture, frameWidth, frameHeight);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading player spritesheet from '{spriteSheetPath}': {ex.Message}");
                return null; // Sau un fallback, dar e mai bine să rezolvi problema de încărcare
            }

            if (loadedSpriteSheet == null)
            {
                System.Diagnostics.Debug.WriteLine("Cannot create player: Spritesheet not loaded or failed to load.");
                return null;
            }

            Entity player = new Entity("Player"); // Tag-ul va fi suprascris în PlayingState
            player.AddComponent(new TransformComponent { Position = position, Scale = Vector2.One * 2f });

            var animations = GetAnimationSet(animationType, loadedSpriteSheet, frameWidth, frameHeight);
            player.AddComponent(new AnimationComponent(loadedSpriteSheet, animations));
            player.AddComponent(new SpriteComponent());

            player.AddComponent(new PlayerControllerComponent { WalkSpeed = 120f, RunSpeedOffset = 80f });
            player.AddComponent(new HealthComponent(100f));
            player.AddComponent(new StatsComponent(100f, 100f) { StaminaRegenRate = 50f, StaminaDrainRateRun = 20f });
            player.AddComponent(new InventoryComponent(20));

            float playerColliderWidth = 128f * 0.3f * player.GetComponent<TransformComponent>().Scale.X;
            float playerColliderHeight = 128f * 0.5f * player.GetComponent<TransformComponent>().Scale.Y;
            Vector2 playerColliderOffset = new Vector2(0, 128f * 0.2f * player.GetComponent<TransformComponent>().Scale.Y);
            player.AddComponent(new ColliderComponent(new Rectangle(0, 0, (int)playerColliderWidth, (int)playerColliderHeight), playerColliderOffset, true));

            player.AddComponent(new LightEmitterComponent(radius: 180f, intensity: 0.6f, color: new Color(255, 220, 150), isActive: false, flickerIntensity: 0.05f, flickerSpeed: 7f));

            System.Diagnostics.Debug.WriteLine($"Player entity created at {position} using spritesheet: {spriteSheetPath}");
            return player;
        }

        // Implementare IEntityFactory (poate un personaj default sau aruncă excepție)
        public Entity CreateEntity(Vector2 position)
        {
            System.Diagnostics.Debug.WriteLine("PlayerFactory.CreateEntity(position) called without specific character. Creating Gotoku default.");
            // Aici pasezi dimensiunile default pentru Gotoku
            return CreateEntity(position, "Sprites/Player/Gotoku_spritelist", PlayerAnimationSetType.Gotoku, 128, 128);
        }


        private Dictionary<string, AnimationData> GetAnimationSet(PlayerAnimationSetType type, SpriteSheet sheet, int frameWidth, int frameHeight)
        {
            switch (type)
            {
                case PlayerAnimationSetType.Gotoku:
                    return CreateGotokuAnimations(sheet);
                case PlayerAnimationSetType.Kunoichi:
                    return CreateKunoichiAnimations(sheet);
                case PlayerAnimationSetType.Ninja:
                    return CreateNinjaAnimations(sheet);
                case PlayerAnimationSetType.Vampire:
                    return CreateVampireAnimations(sheet);
                // Adaugă cazuri pentru Mage, Rogue etc.
                // case PlayerAnimationSetType.Mage:
                //     return CreateMageAnimations(sheet);
                default:
                    System.Diagnostics.Debug.WriteLine($"Warning: Unknown animation set type '{type}'. Falling back to Ronin animations.");
                    return CreateGotokuAnimations(sheet); // Fallback
            }
        }

        // --- Metode Specifice pentru Animațiile Fiecărui Personaj ---

        private Dictionary<string, AnimationData> CreateGotokuAnimations(SpriteSheet sheet)
        {
            float walkFrameDuration = 0.15f;
            float runFrameDuration = 0.1f;
            float idleFrameDuration = 0.2f;
            float attackFrameDuration = 0.07f;
            float frameTime = 0.14f;
            var animations = new Dictionary<string, AnimationData>();

            // Presupunem că 'sheet' este Gotoku_spritelist
            animations.Add("Idle_Down", new AnimationData("Idle_Down", CreateFrames(sheet, 0, 5, idleFrameDuration), true));
            animations.Add("Idle_Up", new AnimationData("Idle_Up", CreateFrames(sheet, 0, 5, idleFrameDuration), true));
            animations.Add("Idle_Left", new AnimationData("Idle_Left", CreateFrames(sheet, 0, 5, idleFrameDuration), true));
            animations.Add("Idle_Right", new AnimationData("Idle_Right", CreateFrames(sheet, 0, 5, idleFrameDuration), true));

            animations.Add("Walk_Down", new AnimationData("Walk_Down", CreateFrames(sheet, 10, 5, walkFrameDuration), true));
            animations.Add("Walk_Up", new AnimationData("Walk_Up", CreateFrames(sheet, 10, 5, walkFrameDuration), true));
            animations.Add("Walk_Left", new AnimationData("Walk_Left", CreateFrames(sheet, 10, 5, walkFrameDuration), true));
            animations.Add("Walk_Right", new AnimationData("Walk_Right", CreateFrames(sheet, 10, 5, walkFrameDuration), true));

            animations.Add("Attack_Down", new AnimationData("Attack_Down", CreateFrames(sheet, 30, 4, attackFrameDuration), false));
            animations.Add("Attack_Up", new AnimationData("Attack_Up", CreateFrames(sheet, 40, 4, attackFrameDuration), false));
            animations.Add("Attack_Left", new AnimationData("Attack_Left", CreateFrames(sheet, 50, 4, attackFrameDuration), false));
            animations.Add("Attack_Right", new AnimationData("Attack_Right", CreateFrames(sheet, 30, 4, attackFrameDuration), false));

            animations.Add("Run_Down", new AnimationData("Run_Down", CreateFrames(sheet, 20, 6, runFrameDuration), true));
            animations.Add("Run_Up", new AnimationData("Run_Up", CreateFrames(sheet, 20, 6, runFrameDuration), true));
            animations.Add("Run_Left", new AnimationData("Run_Left", CreateFrames(sheet, 20, 6, runFrameDuration), true));
            animations.Add("Run_Right", new AnimationData("Run_Right", CreateFrames(sheet, 20, 6, runFrameDuration), true));
            animations.Add("Special", new AnimationData("Special", CreateFrames(sheet, 60, 3, frameTime * 1.2f), false));
            animations.Add("Hurt", new AnimationData("Hurt", CreateFrames(sheet, 70, 2, frameTime), false));
            animations.Add("Dead", new AnimationData("Dead", CreateFrames(sheet, 80, 4, frameTime * 1.1f), false));
            return animations;
        }

        private Dictionary<string, AnimationData> CreateKunoichiAnimations(SpriteSheet sheet)
        {
            float walkFrameDuration = 0.15f;
            float runFrameDuration = 0.1f;
            float idleFrameDuration = 0.2f;
            float attackFrameDuration = 0.07f;
            float frameTime = 0.14f;
            var animations = new Dictionary<string, AnimationData>();

            // Presupunem că 'sheet' este Gotoku_spritelist
            animations.Add("Idle_Down", new AnimationData("Idle_Down", CreateFrames(sheet, 0, 8, idleFrameDuration), true));
            animations.Add("Idle_Up", new AnimationData("Idle_Up", CreateFrames(sheet, 0, 8, idleFrameDuration), true));
            animations.Add("Idle_Left", new AnimationData("Idle_Left", CreateFrames(sheet, 0, 8, idleFrameDuration), true));
            animations.Add("Idle_Right", new AnimationData("Idle_Right", CreateFrames(sheet, 0, 8, idleFrameDuration), true));

            animations.Add("Walk_Down", new AnimationData("Walk_Down", CreateFrames(sheet, 10, 7, walkFrameDuration), true));
            animations.Add("Walk_Up", new AnimationData("Walk_Up", CreateFrames(sheet, 10, 7, walkFrameDuration), true));
            animations.Add("Walk_Left", new AnimationData("Walk_Left", CreateFrames(sheet, 10, 7, walkFrameDuration), true));
            animations.Add("Walk_Right", new AnimationData("Walk_Right", CreateFrames(sheet, 10, 7, walkFrameDuration), true));

            animations.Add("Attack_Down", new AnimationData("Attack_Down", CreateFrames(sheet, 30, 5, attackFrameDuration), false));
            animations.Add("Attack_Up", new AnimationData("Attack_Up", CreateFrames(sheet, 40, 7, attackFrameDuration), false));
            animations.Add("Attack_Left", new AnimationData("Attack_Left", CreateFrames(sheet, 40, 7, attackFrameDuration), false));
            animations.Add("Attack_Right", new AnimationData("Attack_Right", CreateFrames(sheet, 30, 5, attackFrameDuration), false));

            animations.Add("Run_Down", new AnimationData("Run_Down", CreateFrames(sheet, 20, 7, runFrameDuration), true));
            animations.Add("Run_Up", new AnimationData("Run_Up", CreateFrames(sheet, 20, 7, runFrameDuration), true));
            animations.Add("Run_Left", new AnimationData("Run_Left", CreateFrames(sheet, 20, 7, runFrameDuration), true));
            animations.Add("Run_Right", new AnimationData("Run_Right", CreateFrames(sheet, 20, 7, runFrameDuration), true));
            animations.Add("Hurt", new AnimationData("Hurt", CreateFrames(sheet, 80, 2, frameTime), false));
            animations.Add("Dead", new AnimationData("Dead", CreateFrames(sheet, 90, 4, frameTime * 1.1f), false));
            return animations;
        }
        private Dictionary<string, AnimationData> CreateNinjaAnimations(SpriteSheet sheet)
        {
            float walkFrameDuration = 0.15f;
            float runFrameDuration = 0.1f;
            float idleFrameDuration = 0.2f;
            float attackFrameDuration = 0.07f;
            float frameTime = 0.14f;
            var animations = new Dictionary<string, AnimationData>();

            // Presupunem că 'sheet' este Gotoku_spritelist
            animations.Add("Idle_Down", new AnimationData("Idle_Down", CreateFrames(sheet, 0, 5, idleFrameDuration), true));
            animations.Add("Idle_Up", new AnimationData("Idle_Up", CreateFrames(sheet, 0, 5, idleFrameDuration), true));
            animations.Add("Idle_Left", new AnimationData("Idle_Left", CreateFrames(sheet, 0, 5, idleFrameDuration), true));
            animations.Add("Idle_Right", new AnimationData("Idle_Right", CreateFrames(sheet, 0, 5, idleFrameDuration), true));

            animations.Add("Walk_Down", new AnimationData("Walk_Down", CreateFrames(sheet, 10, 7, walkFrameDuration), true));
            animations.Add("Walk_Up", new AnimationData("Walk_Up", CreateFrames(sheet, 10, 7, walkFrameDuration), true));
            animations.Add("Walk_Left", new AnimationData("Walk_Left", CreateFrames(sheet, 10, 7, walkFrameDuration), true));
            animations.Add("Walk_Right", new AnimationData("Walk_Right", CreateFrames(sheet, 10, 7, walkFrameDuration), true));

            animations.Add("Attack_Down", new AnimationData("Attack_Down", CreateFrames(sheet, 50, 5, attackFrameDuration), false));
            animations.Add("Attack_Up", new AnimationData("Attack_Up", CreateFrames(sheet, 50, 5, attackFrameDuration), false));
            animations.Add("Attack_Left", new AnimationData("Attack_Left", CreateFrames(sheet, 60, 3, attackFrameDuration), false));
            animations.Add("Attack_Right", new AnimationData("Attack_Right", CreateFrames(sheet, 60, 3, attackFrameDuration), false));

            animations.Add("Run_Down", new AnimationData("Run_Down", CreateFrames(sheet, 20, 5, runFrameDuration), true));
            animations.Add("Run_Up", new AnimationData("Run_Up", CreateFrames(sheet, 20, 5, runFrameDuration), true));
            animations.Add("Run_Left", new AnimationData("Run_Left", CreateFrames(sheet, 20, 5, runFrameDuration), true));
            animations.Add("Run_Right", new AnimationData("Run_Right", CreateFrames(sheet, 20, 5, runFrameDuration), true));
            animations.Add("Hurt", new AnimationData("Hurt", CreateFrames(sheet, 80, 1, frameTime), false));
            animations.Add("Dead", new AnimationData("Dead", CreateFrames(sheet, 90, 3, frameTime * 1.1f), false));
            return animations;
        }
        private Dictionary<string, AnimationData> CreateVampireAnimations(SpriteSheet sheet)
        {
            float walkFrameDuration = 0.15f;
            float runFrameDuration = 0.1f;
            float idleFrameDuration = 0.2f;
            float attackFrameDuration = 0.07f;
            float frameTime = 0.14f;
            var animations = new Dictionary<string, AnimationData>();

            // Presupunem că 'sheet' este Gotoku_spritelist
            animations.Add("Idle_Down", new AnimationData("Idle_Down", CreateFrames(sheet, 0, 4, idleFrameDuration), true));
            animations.Add("Idle_Up", new AnimationData("Idle_Up", CreateFrames(sheet, 0, 4, idleFrameDuration), true));
            animations.Add("Idle_Left", new AnimationData("Idle_Left", CreateFrames(sheet, 0, 4, idleFrameDuration), true));
            animations.Add("Idle_Right", new AnimationData("Idle_Right", CreateFrames(sheet, 0, 4, idleFrameDuration), true));

            animations.Add("Walk_Down", new AnimationData("Walk_Down", CreateFrames(sheet, 10, 5, walkFrameDuration), true));
            animations.Add("Walk_Up", new AnimationData("Walk_Up", CreateFrames(sheet, 10, 5, walkFrameDuration), true));
            animations.Add("Walk_Left", new AnimationData("Walk_Left", CreateFrames(sheet, 10, 5, walkFrameDuration), true));
            animations.Add("Walk_Right", new AnimationData("Walk_Right", CreateFrames(sheet, 10, 5, walkFrameDuration), true));

            animations.Add("Attack_Down", new AnimationData("Attack_Down", CreateFrames(sheet, 30, 4, attackFrameDuration), false));
            animations.Add("Attack_Up", new AnimationData("Attack_Up", CreateFrames(sheet, 40, 3, attackFrameDuration), false));
            animations.Add("Attack_Left", new AnimationData("Attack_Left", CreateFrames(sheet, 50, 1, attackFrameDuration), false));
            animations.Add("Attack_Right", new AnimationData("Attack_Right", CreateFrames(sheet, 60, 4, attackFrameDuration), false));

            animations.Add("Run_Down", new AnimationData("Run_Down", CreateFrames(sheet, 20, 5, runFrameDuration), true));
            animations.Add("Run_Up", new AnimationData("Run_Up", CreateFrames(sheet, 20, 5, runFrameDuration), true));
            animations.Add("Run_Left", new AnimationData("Run_Left", CreateFrames(sheet, 20, 5, runFrameDuration), true));
            animations.Add("Run_Right", new AnimationData("Run_Right", CreateFrames(sheet, 20, 5, runFrameDuration), true));
            animations.Add("Hurt", new AnimationData("Hurt", CreateFrames(sheet, 80, 1, frameTime), false));
            animations.Add("Dead", new AnimationData("Dead", CreateFrames(sheet, 90, 9, frameTime * 1.1f), false));
            return animations;
        }

        // private Dictionary<string, AnimationData> CreateMageAnimations(SpriteSheet sheet) { ... }
        // private Dictionary<string, AnimationData> CreateRogueAnimations(SpriteSheet sheet) { ... }


        private List<AnimationFrame> CreateFrames(SpriteSheet sheet, int startIndex, int count, float durationPerFrame)
        {
            var frames = new List<AnimationFrame>(count);
            if (sheet == null) return frames;

            for (int i = 0; i < count; i++)
            {
                if (startIndex + i < sheet.TotalFrames)
                {
                    frames.Add(new AnimationFrame(startIndex + i, durationPerFrame));
                }
                else
                {
                    if (sheet.TotalFrames > 0 && frames.Count < count)
                        frames.Add(new AnimationFrame(0, durationPerFrame));
                    else if (frames.Count < count)
                        frames.Add(new AnimationFrame(0, durationPerFrame));
                }
            }
            if (count > 0 && frames.Count == 0 && sheet.TotalFrames > 0)
            {
                frames.Add(new AnimationFrame(0, durationPerFrame));
            }
            else if (count > 0 && frames.Count == 0)
            {
                frames.Add(new AnimationFrame(0, 1f));
            }
            return frames;
        }
    }
}