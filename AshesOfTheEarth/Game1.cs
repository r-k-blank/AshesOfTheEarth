using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AshesOfTheEarth.Core;
using AshesOfTheEarth.Core.Input;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Graphics;
using AshesOfTheEarth.World;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Factories;
using AshesOfTheEarth.Core.Time;
using System;
using AshesOfTheEarth.Utils;
using AshesOfTheEarth.Gameplay.Systems;
using AshesOfTheEarth.Core.Mediator;
using AshesOfTheEarth.Gameplay;
using AshesOfTheEarth.UI;
using AshesOfTheEarth.Gameplay.Crafting;
using AshesOfTheEarth.Entities.Factories.Mobs;
using AshesOfTheEarth.Entities.Factories.Animals;
using AshesOfTheEarth.Core.Validation;
using AshesOfTheEarth.Gameplay.Placement;
using AshesOfTheEarth.Gameplay.Lighting;
using AshesOfTheEarth.Gameplay.Survival;
using Microsoft.Xna.Framework.Content;


namespace AshesOfTheEarth
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = Settings.DefaultScreenWidth;
            _graphics.PreferredBackBufferHeight = Settings.DefaultScreenHeight;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            ServiceLocator.Initialize();
            ServiceLocator.Register<GraphicsDevice>(GraphicsDevice);
            ServiceLocator.Register<Microsoft.Xna.Framework.Content.ContentManager>(Content);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            ServiceLocator.Register<SpriteBatch>(_spriteBatch);

            Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            ServiceLocator.Register<Texture2D>(pixel);


            ServiceLocator.Register<InputManager>(new InputManager());
            ServiceLocator.Register<GameStateManager>(new GameStateManager());
            ServiceLocator.Register<EntityManager>(new EntityManager());
            ServiceLocator.Register<IPositionValidator>(new PositionValidator());
            ServiceLocator.Register<WorldManager>(new WorldManager());
            ServiceLocator.Register<TimeManager>(new TimeManager());
            ServiceLocator.Get<TimeManager>();
            ServiceLocator.Register<Gameplay.Events.EventManager>(new Gameplay.Events.EventManager());

            var gameplayMediator = new GameplayMediator();
            ServiceLocator.Register<IGameplayMediator>(gameplayMediator);

            ServiceLocator.Register<Renderer>(new Renderer());
            ServiceLocator.Register<SaveLoadManager>(new SaveLoadManager());

            ServiceLocator.Register<TreeFactory>(new TreeFactory(Content));
            ServiceLocator.Register<RockFactory>(new RockFactory(Content));
            ServiceLocator.Register<BushFactory>(new BushFactory(Content));
            ServiceLocator.Register<CollectibleFactory>(new CollectibleFactory());
            ServiceLocator.Register<CampfireFactory>(new CampfireFactory(Content));

            ServiceLocator.Register<SkeletonFactory>(new SkeletonFactory(Content));
            ServiceLocator.Register<MinotaurFactory>(new MinotaurFactory(Content));
            ServiceLocator.Register<GargoyleFactory>(new GargoyleFactory(Content));
            ServiceLocator.Register<WerewolfFactory>(new WerewolfFactory(Content));
            ServiceLocator.Register<AnimalFactory>(new AnimalFactory(Content));
            ServiceLocator.Register<PlayerFactory>(new PlayerFactory(Content));

            var camera = new Camera(GraphicsDevice.Viewport);
            ServiceLocator.Register<Camera>(camera);

            var entityManager = ServiceLocator.Get<EntityManager>();
            var worldManager = ServiceLocator.Get<WorldManager>();

            ServiceLocator.Register<LightSystem>(new LightSystem());
            ServiceLocator.Register<DarknessDamageSystem>(new DarknessDamageSystem());


            ServiceLocator.Register<HarvestingSystem>(new HarvestingSystem(entityManager));
            ServiceLocator.Register<CombatSystem>(new CombatSystem(entityManager));
            ServiceLocator.Register<DropGenerationSystem>(new DropGenerationSystem(entityManager));
            ServiceLocator.Register<CraftingSystem>(new CraftingSystem());
            ServiceLocator.Register<AISystem>(new AISystem(entityManager, worldManager));

            ServiceLocator.Register<IPlacementValidator>(new PlacementValidator());
            ServiceLocator.Register<UIManager>(new UIManager());


            ServiceLocator.Get<Renderer>().Initialize(camera);
            ServiceLocator.Get<GameStateManager>().ChangeState(new MainMenuState());
            ServiceLocator.Register<Game>(this);
            ServiceLocator.Register<GameTime>(new GameTime());

            base.Initialize();
        }


        protected override void Update(GameTime gameTime)
        {
            ServiceLocator.Register<GameTime>(gameTime);

            if (ServiceLocator.Get<InputManager>().IsKeyPressed(Keys.F1))
            {
                Settings.DebugShowColliders = !Settings.DebugShowColliders;
            }
            ServiceLocator.Get<InputManager>().Update(gameTime);
            ServiceLocator.Get<GameStateManager>().Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ServiceLocator.Get<GameStateManager>().Draw(gameTime);
            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            var pixelTex = ServiceLocator.Get<Texture2D>();
            pixelTex?.Dispose();

            var lightMask = ServiceLocator.Get<ContentManager>().Load<Texture2D>("Sprites/UI/light_mask_radial");
            lightMask?.Dispose();

            ServiceLocator.Cleanup();
            base.OnExiting(sender, args);
        }
    }
}

namespace AshesOfTheEarth.Graphics.Animation
{
    public static class AnimationDataExtensions
    {
        public static float TotalDuration(this AnimationData animData)
        {
            if (animData == null || animData.Frames == null) return 0f;
            float total = 0f;
            foreach (var frame in animData.Frames)
            {
                total += frame.Duration;
            }
            return total;
        }
    }
}