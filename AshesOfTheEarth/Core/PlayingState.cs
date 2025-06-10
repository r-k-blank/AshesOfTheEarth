using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Graphics;
using AshesOfTheEarth.World;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Entities.Factories;
using AshesOfTheEarth.Gameplay.Survival;
using AshesOfTheEarth.Gameplay;
using AshesOfTheEarth.UI;
using AshesOfTheEarth.Core.Time;
using AshesOfTheEarth.Gameplay.Systems;
using AshesOfTheEarth.Gameplay.Items;
using System;
using System.Linq;
using AshesOfTheEarth.Entities.Visitor;
using AshesOfTheEarth.Core.Mediator;
using AshesOfTheEarth.Core.Input;
using AshesOfTheEarth.Gameplay.Crafting;
using AshesOfTheEarth.Entities.Factories.Mobs;
using AshesOfTheEarth.Entities.Factories.Animals;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using AshesOfTheEarth.Core.Validation;
using AshesOfTheEarth.Gameplay.Placement;
using AshesOfTheEarth.Gameplay.Lighting;


namespace AshesOfTheEarth.Core
{
    public class PlayingState : IGameState
    {
        private EntityManager _entityManager;
        private WorldManager _worldManager;
        private Renderer _renderer;
        private UIManager _uiManager;
        private Camera _camera;
        private TimeManager _timeManager;
        private SurvivalSystem _survivalSystem;
        private CombatSystem _combatSystem;
        private CraftingSystem _craftingSystem;
        private HarvestingSystem _harvestingSystem;
        private AISystem _aiSystem;
        private DropGenerationSystem _dropGenerationSystem;
        private SaveLoadManager _saveLoadManager;
        private IGameplayMediator _gameplayMediator;
        private IPositionValidator _positionValidator;
        private IPlacementValidator _placementValidator;
        private GameStateManager _gameStateManager;
        private SpriteFont _gameOverFont;
        private LightSystem _lightSystem;
        private DarknessDamageSystem _darknessDamageSystem;


        private bool _contentLoaded = false;
        private bool _isInitialized = false;
        private GameStateMemento _initialMemento = null;
        private string _selectedCharacterSpriteSheetPath = "Sprites/Player/Gotoku_spritelist"; // Default
        private string _selectedCharacterName = "Gotoku"; // Default
        private PlayerAnimationSetType _selectedCharacterAnimationType = PlayerAnimationSetType.Gotoku;

        private int _currentWorldSeed = 0;

        private Entity _player;
        private bool _isGameOverSequenceActive = false;
        private float _gameOverDisplayTimer = 0f;
        private const float GAME_OVER_DISPLAY_DURATION = 3f;


        private int _selectedCharacterFrameWidth = 128; // Default
        private int _selectedCharacterFrameHeight = 128; // Default

        public PlayingState(GameStateMemento mementoToLoad = null,
                            string selectedCharacterSpriteSheetPath = null,
                            string selectedCharacterName = null,
                            PlayerAnimationSetType? animationType = null,
                            int? frameWidth = null, // NOU
                            int? frameHeight = null) // NOU
        {
            _initialMemento = mementoToLoad;
            if (!string.IsNullOrEmpty(selectedCharacterSpriteSheetPath))
                _selectedCharacterSpriteSheetPath = selectedCharacterSpriteSheetPath;
            if (!string.IsNullOrEmpty(selectedCharacterName))
                _selectedCharacterName = selectedCharacterName;
            if (animationType.HasValue)
                _selectedCharacterAnimationType = animationType.Value;
            if (frameWidth.HasValue) // NOU
                _selectedCharacterFrameWidth = frameWidth.Value;
            if (frameHeight.HasValue) // NOU
                _selectedCharacterFrameHeight = frameHeight.Value;

            ServiceLocator.Register<PlayingState>(this);
        }

        public void LoadContent(ContentManager content)
        {
            if (_contentLoaded) return;

            ItemRegistry.Initialize(content);

            _entityManager = ServiceLocator.Get<EntityManager>();
            _worldManager = ServiceLocator.Get<WorldManager>();
            _renderer = ServiceLocator.Get<Renderer>();
            _uiManager = ServiceLocator.Get<UIManager>();
            _camera = ServiceLocator.Get<Camera>();
            _timeManager = ServiceLocator.Get<TimeManager>();
            _harvestingSystem = ServiceLocator.Get<HarvestingSystem>();
            _saveLoadManager = ServiceLocator.Get<SaveLoadManager>();
            _gameplayMediator = ServiceLocator.Get<IGameplayMediator>();
            _combatSystem = ServiceLocator.Get<CombatSystem>();
            _dropGenerationSystem = ServiceLocator.Get<DropGenerationSystem>();
            _positionValidator = ServiceLocator.Get<IPositionValidator>();
            _placementValidator = ServiceLocator.Get<IPlacementValidator>();
            _gameStateManager = ServiceLocator.Get<GameStateManager>();
            _gameOverFont = content.Load<SpriteFont>("Fonts/DefaultFont");
            _lightSystem = ServiceLocator.Get<LightSystem>();
            _darknessDamageSystem = ServiceLocator.Get<DarknessDamageSystem>();


            _survivalSystem = new SurvivalSystem(_entityManager);
            _craftingSystem = ServiceLocator.Get<CraftingSystem>();
            _aiSystem = ServiceLocator.Get<AISystem>();

            _worldManager.LoadContent(content);
            _uiManager.LoadContent(content);

            _contentLoaded = true;
        }

        private void InitializeState()
        {
            if (_isInitialized) return;
            var aiSystemInstance = ServiceLocator.Get<AISystem>();
            aiSystemInstance?.InvalidatePlayerReference();
            //System.Diagnostics.Debug.WriteLine("[PlayingState.InitializeState] START: Player reference in AISystem invalidated.");
            _player = null;

            if (_initialMemento != null)
            {
                _selectedCharacterSpriteSheetPath = _initialMemento.PlayerState?.SelectedCharacterSpriteSheetPath ?? "Sprites/Player/Gotoku_spritelist";
                _selectedCharacterName = _initialMemento.PlayerState?.SelectedCharacterName ?? "Gotoku";
                _selectedCharacterAnimationType = _initialMemento.PlayerState?.SelectedCharacterAnimationType ?? PlayerAnimationSetType.Gotoku; // NOU
                RestoreFromMemento(_initialMemento);
                _currentWorldSeed = _initialMemento.WorldSeed;
            }
            else // Joc Nou
            {
                // Folosește seed-ul din setări dacă e specificat, altfel generează unul nou
                _currentWorldSeed = Utils.Settings.UseCustomSeed ? Utils.Settings.WorldSeed : Environment.TickCount;
                if (Utils.Settings.UseCustomSeed && Utils.Settings.WorldSeed == 0) // Dacă e custom dar 0, consideră random
                {
                    _currentWorldSeed = Environment.TickCount;
                    Utils.Settings.WorldSeed = _currentWorldSeed; // Salvează-l pentru afișare
                }

                _worldManager.GenerateWorld(_currentWorldSeed,
                                            Utils.Settings.GetActualWorldWidth(),
                                            Utils.Settings.GetActualWorldHeight());
                _timeManager.ResetTime();

                PlayerFactory playerFactory = ServiceLocator.Get<PlayerFactory>();
                Vector2 desiredPlayerPos = new Vector2(_worldManager.TileMap.WidthInPixels / 2f, _worldManager.TileMap.HeightInPixels / 2f);

                // Preluăm setările de personaj din CharacterSelectionState (dacă există)
                // sau folosim valorile default din PlayingState
                _player = playerFactory.CreateEntity(desiredPlayerPos,
                                                     _selectedCharacterSpriteSheetPath,
                                                     _selectedCharacterAnimationType,
                                                     _selectedCharacterFrameWidth,
                                                     _selectedCharacterFrameHeight);

                if (_player != null)
                {
                    _player.Tag = _selectedCharacterName; // Setează numele corect
                    var playerTransform = _player.GetComponent<TransformComponent>();
                    Vector2 safePlayerPos = _positionValidator.FindSafeSpawnPositionNearby(_player, playerTransform.Position, 200f, 50);
                    playerTransform.Position = safePlayerPos;
                    _player.GetComponent<PlayerControllerComponent>()?.Initialize(_player);
                    _entityManager.AddEntity(_player);
                    _camera.Follow(_player);
                    SpawnInitialResourcesNearPlayer(_player);

                    // Aici ai putea aplica setările de dificultate la player sau la lume
                    ApplyDifficultySettings(_player, Utils.Settings.SelectedDifficulty);
                }
            }
            if (_player == null) _player = _entityManager.GetEntityByTag(_selectedCharacterName) ?? _entityManager.GetEntityByTag("Player");


            _uiManager.LoadHUD();
            _timeManager.Subscribe(_uiManager);
            _timeManager.Subscribe(_survivalSystem);
            if (_lightSystem != null) _timeManager.Subscribe(_lightSystem);
            _uiManager.OnTimeChanged(_timeManager);
            _uiManager.OnDayPhaseChanged(_timeManager.CurrentDayPhase);
            var collectibleFactoryInstance = ServiceLocator.Get<CollectibleFactory>();
            if (collectibleFactoryInstance != null)
            {
                // Adaugă o verificare în CollectibleFactory.InitializePool pentru a nu re-inițializa dacă pool-ul există deja
                collectibleFactoryInstance.InitializePool();
            }
            else
            {
                // Acest caz nu ar trebui să apară dacă ai înregistrat corect în Game1
                System.Diagnostics.Debug.WriteLine("CRITICAL: CollectibleFactory not found in ServiceLocator for pool initialization.");
                // Poți încerca să o creezi și să o înregistrezi aici ca un fallback de urgență, dar e mai bine să fie în Game1.
                // collectibleFactoryInstance = new CollectibleFactory();
                // ServiceLocator.Register<CollectibleFactory>(collectibleFactoryInstance);
                // collectibleFactoryInstance.InitializePool();
            }
            

            _isInitialized = true;
        }
        private void ApplyDifficultySettings(Entity player, Utils.Settings.DifficultyOption difficulty)
        {
            if (player == null) return;
            var healthComp = player.GetComponent<HealthComponent>();
            var statsComp = player.GetComponent<StatsComponent>();

            // Acest loc este și pentru a modifica parametrii de spawn ai mobilor, resurse etc.
            // Pentru moment, modificăm doar player-ul ca exemplu.
            //System.Diagnostics.Debug.WriteLine($"Applying difficulty: {difficulty}");

            switch (difficulty)
            {
                case Utils.Settings.DifficultyOption.Easy:
                    if (healthComp != null) healthComp.MaxHealth *= 1.2f; // Player mai rezistent
                    if (statsComp != null) statsComp.StaminaRegenRate *= 1.5f; // Regenerare stamină mai rapidă
                    // TODO: Scade damage-ul mobilor, crește drop-rate-ul resurselor etc.
                    break;
                case Utils.Settings.DifficultyOption.Normal:
                    // Setările default sunt deja considerate "Normal"
                    break;
                case Utils.Settings.DifficultyOption.Hard:
                    if (healthComp != null) healthComp.MaxHealth *= 0.8f; // Player mai fragil
                    if (statsComp != null) statsComp.StaminaRegenRate *= 0.75f;
                    // TODO: Crește damage-ul mobilor, scade drop-rate-ul etc.
                    break;
            }
            if (healthComp != null) healthComp.CurrentHealth = healthComp.MaxHealth; // Setează viața la maximul nou
            if (statsComp != null) statsComp.CurrentStamina = statsComp.MaxStamina; // Și stamina
        }
        private void SpawnInitialResourcesNearPlayer(Entity player)
        {
            var playerTransform = player.GetComponent<TransformComponent>();
            if (playerTransform == null) return;

            var collectibleFactory = ServiceLocator.Get<CollectibleFactory>();
            
            var initialResources = new List<(ItemType type, int quantity, Vector2 offset)>
            {
                (ItemType.WoodLog, 2, new Vector2(50, -20)),
                (ItemType.WoodLog, 1, new Vector2(70, 10)),
                (ItemType.StoneShard, 3, new Vector2(-40, 30)),
                (ItemType.StoneShard, 2, new Vector2(-60, 5)),
                (ItemType.Flint, 1, new Vector2(10, 60)),
                (ItemType.Berries, 5, new Vector2(-20, -50)),
                (ItemType.Campfire, 1, new Vector2(30, 40)),
                (ItemType.Torch, 1, new Vector2(-30, 40))
            };
            foreach (var res_info in initialResources)
            {
                Vector2 spawnPosition = playerTransform.Position + res_info.offset;
                Entity collectible = collectibleFactory.CreateCollectible(spawnPosition, res_info.type, res_info.quantity);

                if (collectible != null)
                {
                    if (_positionValidator.IsPositionSafe(collectible))
                    {
                        _entityManager.AddEntity(collectible);
                    }
                    else
                    {
                        // Dacă poziția nu e sigură, returnează obiectul în pool pentru a evita pierderea lui
                        collectibleFactory.ReturnCollectibleToPool(collectible);
                    }
                }
            }
        }

        public void SaveGameAndReturnToMenu()
        {
            SaveGame();
            _gameStateManager.ChangeState(new MainMenuState());
        }

        public void TriggerSaveGame()
        {
            SaveGame();
        }

        public void Update(GameTime gameTime)
        {
            if (!_contentLoaded) return;
            if (!_isInitialized)
            {
                InitializeState();
                return;
            }

            if (_player == null) _player = _entityManager.GetEntityByTag(_selectedCharacterName) ?? _entityManager.GetEntityByTag("Player");


            if (_isGameOverSequenceActive)
            {
                _gameOverDisplayTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_gameOverDisplayTimer <= 0)
                {
                    _saveLoadManager.DeleteSave();
                    _gameStateManager.ChangeState(new MainMenuState());
                    _isGameOverSequenceActive = false;
                }
                return;
            }

            if (_player != null)
            {
                var playerHealth = _player.GetComponent<HealthComponent>();
                if (playerHealth != null && playerHealth.IsDead && !_isGameOverSequenceActive)
                {
                    StartGameOverSequence();
                    return;
                }
            }


            var inputManager = ServiceLocator.Get<InputManager>();
            _player?.GetComponent<PlayerControllerComponent>()?.Update(gameTime);

            _entityManager.Update(gameTime);
            _survivalSystem.Update(gameTime);
            _combatSystem.Update(gameTime);
            _harvestingSystem.Update(gameTime);
            _aiSystem.Update(gameTime);
            _worldManager.Update(gameTime);
            _craftingSystem.Update(gameTime);
            _camera.Update(gameTime);
            _uiManager.Update(gameTime);
            _lightSystem?.Update(gameTime);
            _darknessDamageSystem?.Update(gameTime);
            _timeManager.ProcessTimeouts(gameTime);
            _timeManager.Update(gameTime);
        }

        private void StartGameOverSequence()
        {
            _isGameOverSequenceActive = true;
            _gameOverDisplayTimer = GAME_OVER_DISPLAY_DURATION;
            _uiManager.HideHUD();
        }


        public void Draw(GameTime gameTime)
        {
            if (!_contentLoaded || !_isInitialized)
            {
                ServiceLocator.Get<GraphicsDevice>().Clear(Color.CornflowerBlue);
                return;
            };

            var spriteBatch = ServiceLocator.Get<SpriteBatch>();
            var camera = ServiceLocator.Get<Camera>();

            _renderer.BeginDraw(_camera.GetViewMatrix());
            _renderer.DrawWorld(_worldManager, spriteBatch);
            _renderer.DrawEntities(_entityManager, spriteBatch);
            if (Utils.Settings.DebugShowColliders)
            {
                _renderer.DrawColliders(_entityManager, spriteBatch);
            }
            _renderer.EndDraw();

            _renderer.BeginDraw();
            _renderer.DrawLightingEffects(spriteBatch, camera);


            if (!_isGameOverSequenceActive)
            {
                _renderer.DrawUI(_uiManager, spriteBatch);
            }
            else if (_gameOverDisplayTimer > 0 && _gameOverFont != null)
            {
                string gameOverText = "GAME OVER";
                Vector2 textSize = _gameOverFont.MeasureString(gameOverText);
                GraphicsDevice gd = ServiceLocator.Get<GraphicsDevice>();
                Vector2 position = new Vector2((gd.Viewport.Width - textSize.X) / 2, (gd.Viewport.Height - textSize.Y) / 2);

                Texture2D pixel = ServiceLocator.Get<Texture2D>();
                Rectangle backgroundRect = new Rectangle(0, (int)position.Y - 10, gd.Viewport.Width, (int)textSize.Y + 20);
                if (pixel != null) spriteBatch.Draw(pixel, backgroundRect, Color.Black * 0.7f);
                spriteBatch.DrawString(_gameOverFont, gameOverText, position, Color.Red);
            }
            _renderer.EndDraw();
        }

        public void UnloadContent()
        {
            if (!_contentLoaded) return;

            if (_timeManager != null)
            {
                if (_uiManager != null) _timeManager.Unsubscribe(_uiManager);
                if (_survivalSystem != null) _timeManager.Unsubscribe(_survivalSystem);
                if (_lightSystem != null) _timeManager.Unsubscribe(_lightSystem);
            }
            var aiSystemInstance = ServiceLocator.Get<AISystem>();
            aiSystemInstance?.InvalidatePlayerReference();
            //System.Diagnostics.Debug.WriteLine("[PlayingState.UnloadContent] Player reference in AISystem invalidated.");
            ServiceLocator.Unregister<PlayingState>();
            _entityManager?.ClearAllEntities();
            _worldManager?.UnloadContent();
            _uiManager?.UnloadContent();
            _player = null;
            _contentLoaded = false;
            _isInitialized = false;
            _isGameOverSequenceActive = false;
        }
        private void SaveGame()
        {
            if (!_isInitialized) return;
            GameStateMemento memento = CreateMemento();
            if (memento != null)
            {
                _saveLoadManager.SaveGame(memento);
            }
        }

        private GameStateMemento CreateMemento()
        {
            if (_player == null) _player = _entityManager.GetAllEntities().FirstOrDefault(e => e.HasComponent<PlayerControllerComponent>());
            if (_player == null) return null;

            var playerTransform = _player.GetComponent<TransformComponent>();
            var playerHealth = _player.GetComponent<HealthComponent>();
            var playerStats = _player.GetComponent<StatsComponent>();
            var playerInventory = _player.GetComponent<InventoryComponent>();

            var playerMemento = new PlayerMemento
            {
                Id = _player.Id,
                Position = playerTransform?.Position ?? Vector2.Zero,
                CurrentHealth = playerHealth?.CurrentHealth ?? 100f,
                CurrentHunger = playerStats?.CurrentHunger ?? 0f,
                CurrentStamina = playerStats?.CurrentStamina ?? 100f,
                InventoryItems = playerInventory?.Items?.Where(s => s.Type != ItemType.None && s.Quantity > 0).Select(ItemStackMemento.FromItemStack).ToList() ?? new List<ItemStackMemento>(),
                SelectedCharacterSpriteSheetPath = _selectedCharacterSpriteSheetPath,
                SelectedCharacterName = _selectedCharacterName,
                SelectedCharacterAnimationType = _selectedCharacterAnimationType, // NOU
                SelectedCharacterFrameWidth = _selectedCharacterFrameWidth, // SALVEAZĂ
                SelectedCharacterFrameHeight = _selectedCharacterFrameHeight // SALVEAZĂ
            };

            var entityMementos = new System.Collections.Generic.List<EntityMemento>();
            var saveableEntities = _entityManager.GetAllEntities()
                                     .Where(e => e != _player && (e.HasComponent<ResourceSourceComponent>() || e.HasComponent<AIComponent>() || e.HasComponent<PlaceableComponent>() || e.HasComponent<LightEmitterComponent>()));
            foreach (var entity in saveableEntities)
            {
                var transform = entity.GetComponent<TransformComponent>();
                if (transform == null) continue;

                var entityMem = new EntityMemento
                {
                    Id = entity.Id,
                    Tag = entity.Tag,
                    Position = transform.Position
                };

                if (entity.HasComponent<ResourceSourceComponent>())
                {
                    var resSource = entity.GetComponent<ResourceSourceComponent>();
                    entityMem.CurrentHealth = resSource.Health;
                    entityMem.MaxHealth = resSource.MaxHealth;
                    entityMem.IsDepleted = resSource.Depleted;
                }
                else if (entity.HasComponent<HealthComponent>())
                {
                    var healthComp = entity.GetComponent<HealthComponent>();
                    entityMem.CurrentHealth = healthComp.CurrentHealth;
                    entityMem.MaxHealth = healthComp.MaxHealth;
                }
                if (entity.HasComponent<InventoryComponent>())
                {
                    entityMem.InventoryItems = entity.GetComponent<InventoryComponent>().Items?
                               .Where(s => s.Type != ItemType.None && s.Quantity > 0)
                               .Select(ItemStackMemento.FromItemStack)
                               .ToList() ?? new System.Collections.Generic.List<ItemStackMemento>();
                }
                entityMementos.Add(entityMem);
            }

            var timeMemento = new TimeMemento
            {
                DayNumber = _timeManager.DayNumber,
                TimeOfDayHours = _timeManager.TimeOfDayHours,
                CurrentDayPhase = _timeManager.CurrentDayPhase
            };

            return new GameStateMemento
            {
                WorldSeed = _currentWorldSeed,
                WorldWidth = _worldManager.TileMap?.Width ?? Utils.Settings.GetActualWorldWidth(),
                WorldHeight = _worldManager.TileMap?.Height ?? Utils.Settings.GetActualWorldHeight(),
                SavedWorldSize = Utils.Settings.SelectedWorldSize, // Salvează setarea curentă
                SavedDifficulty = Utils.Settings.SelectedDifficulty,
                TimeState = timeMemento,
                PlayerState = playerMemento,
                EntityStates = entityMementos
            };
        }

        private void RestoreFromMemento(GameStateMemento memento)
        {
            if (memento == null) { _initialMemento = null; InitializeState(); return; }
            Utils.Settings.SelectedWorldSize = memento.SavedWorldSize;
            Utils.Settings.SelectedDifficulty = memento.SavedDifficulty;
            _selectedCharacterSpriteSheetPath = memento.PlayerState?.SelectedCharacterSpriteSheetPath ?? "Sprites/Player/Gotoku_spritelist";
            _selectedCharacterName = memento.PlayerState?.SelectedCharacterName ?? "Gotoku";
            _selectedCharacterAnimationType = memento.PlayerState?.SelectedCharacterAnimationType ?? PlayerAnimationSetType.Gotoku; // NOU

            _selectedCharacterFrameWidth = memento.PlayerState?.SelectedCharacterFrameWidth ?? 128; // RESTAUREAZĂ
            _selectedCharacterFrameHeight = memento.PlayerState?.SelectedCharacterFrameHeight ?? 128; // RESTAUREAZĂ

            _entityManager.ClearAllEntities();
            _worldManager.GenerateWorld(memento.WorldSeed, memento.WorldWidth, memento.WorldHeight);
            _timeManager.RestoreTime(memento.TimeState);

            PlayerFactory playerFactory = ServiceLocator.Get<PlayerFactory>(); // Obține din ServiceLocator
            _player = playerFactory.CreateEntity(memento.PlayerState.Position, _selectedCharacterSpriteSheetPath, _selectedCharacterAnimationType, _selectedCharacterFrameWidth, _selectedCharacterFrameHeight);

            if (_player != null)
            {
                _player.Tag = _selectedCharacterName; // Setează tag-ul corect
                                                      // APLICĂ STĂRILE DIN MEMENTO IMEDIAT DUPĂ CREARE
                _player.GetComponent<HealthComponent>().CurrentHealth = memento.PlayerState.CurrentHealth;
                var stats = _player.GetComponent<StatsComponent>();
                stats.CurrentHunger = memento.PlayerState.CurrentHunger;
                stats.CurrentStamina = memento.PlayerState.CurrentStamina;
                var inv = _player.GetComponent<InventoryComponent>();
                inv.InitializeEmptySlots(); // Asigură-te că e gol înainte de a adăuga
                if (memento.PlayerState.InventoryItems != null)
                    foreach (var itemMem in memento.PlayerState.InventoryItems) inv.AddItem(itemMem.Type, itemMem.Quantity);

                var playerTransform = _player.GetComponent<TransformComponent>();
                Vector2 safePlayerPos = _positionValidator.FindSafeSpawnPositionNearby(_player, playerTransform.Position, 150f, 50);
                playerTransform.Position = safePlayerPos;
                _player.GetComponent<PlayerControllerComponent>()?.Initialize(_player);
                _entityManager.AddEntity(_player);
                _camera.Follow(_player);
                ApplyDifficultySettings(_player, memento.SavedDifficulty);
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("[PlayingState.RestoreFromMemento] CRITICAL: Player could not be created from memento.");
                _initialMemento = null; // Previne încercări repetate de restaurare eșuate
                _isInitialized = false; // Forțează o reinițializare la un joc nou la următorul Update
                return;
            }
            var aiSystemInstance = ServiceLocator.Get<AISystem>(); // Obține instanța
            aiSystemInstance?.InvalidatePlayerReference();
            _uiManager.UnloadContent(); // Descarcă ecranele vechi
            _uiManager.LoadContent(ServiceLocator.Get<ContentManager>()); // Reîncarcă-le, acum vor lua noul _player
            _uiManager.LoadHUD(); // Și HUD-ul
            _uiManager.OnTimeChanged(_timeManager);
            _uiManager.OnDayPhaseChanged(_timeManager.CurrentDayPhase);
            var resourceEntitiesAfterRegen = _entityManager.GetAllEntitiesWithComponents<ResourceSourceComponent>().ToList();
            var mobEntitiesAfterRegen = _entityManager.GetAllEntitiesWithComponents<AIComponent>().ToList();
            var placedEntitiesAfterRegen = _entityManager.GetAllEntitiesWithComponents<PlaceableComponent>().ToList();


            System.Collections.Generic.Dictionary<string, Entity> existingEntityLookup = new System.Collections.Generic.Dictionary<string, Entity>();
            foreach (var res in resourceEntitiesAfterRegen)
            {
                var transform = res.GetComponent<TransformComponent>();
                if (transform == null) continue;
                string key = $"{res.Tag}_{Math.Round(transform.Position.X)}_{Math.Round(transform.Position.Y)}";
                if (!existingEntityLookup.ContainsKey(key)) existingEntityLookup.Add(key, res);
            }
            foreach (var mob in mobEntitiesAfterRegen)
            {
                var transform = mob.GetComponent<TransformComponent>();
                if (transform == null) continue;
                string key = $"{mob.Tag}_{Math.Round(transform.Position.X)}_{Math.Round(transform.Position.Y)}";
                if (!existingEntityLookup.ContainsKey(key)) existingEntityLookup.Add(key, mob);
            }
            foreach (var placed in placedEntitiesAfterRegen)
            {
                var transform = placed.GetComponent<TransformComponent>();
                if (transform == null) continue;
                string key = $"{placed.Tag}_{Math.Round(transform.Position.X)}_{Math.Round(transform.Position.Y)}";
                if (!existingEntityLookup.ContainsKey(key)) existingEntityLookup.Add(key, placed);
            }


            System.Collections.Generic.List<ulong> idsToRemove = new System.Collections.Generic.List<ulong>();
            foreach (var entityMemento in memento.EntityStates)
            {
                string lookupKey = $"{entityMemento.Tag}_{Math.Round(entityMemento.Position.X)}_{Math.Round(entityMemento.Position.Y)}";
                Entity foundEntity = null;

                if (existingEntityLookup.TryGetValue(lookupKey, out Entity matchedEntity))
                {
                    foundEntity = matchedEntity;
                }
                else
                {
                    if (entityMemento.Tag == "Campfire")
                    {
                        var factory = ServiceLocator.Get<CampfireFactory>();
                        foundEntity = factory?.CreateEntity(entityMemento.Position);
                        if (foundEntity != null) _entityManager.AddEntity(foundEntity);
                    }
                }


                if (foundEntity != null)
                {
                    var currentTransform = foundEntity.GetComponent<TransformComponent>();
                    currentTransform.Position = entityMemento.Position;

                    if (!_positionValidator.IsPositionSafe(foundEntity))
                    {
                        Vector2 safePos = _positionValidator.FindSafeSpawnPositionNearby(foundEntity, entityMemento.Position, 50f, 20);
                        currentTransform.Position = safePos;
                    }


                    if (entityMemento.IsDepleted == true && foundEntity.HasComponent<ResourceSourceComponent>() && foundEntity.GetComponent<ResourceSourceComponent>().DestroyOnDepleted)
                    {
                        idsToRemove.Add(foundEntity.Id); existingEntityLookup.Remove(lookupKey); continue;
                    }

                    if (foundEntity.HasComponent<ResourceSourceComponent>())
                    {
                        var resSource = foundEntity.GetComponent<ResourceSourceComponent>();
                        if (entityMemento.MaxHealth.HasValue) resSource.MaxHealth = entityMemento.MaxHealth.Value;
                        resSource.Health = MathHelper.Clamp(entityMemento.CurrentHealth ?? resSource.MaxHealth, 0, resSource.MaxHealth);
                        if (resSource.Depleted && !resSource.DestroyOnDepleted && foundEntity.Tag.StartsWith("Bush_"))
                        {
                            if (System.Enum.TryParse<BushType>(foundEntity.Tag.Substring("Bush_".Length), out BushType type))
                                BushFactory.SetBushToHarvestedState(foundEntity, type);
                        }
                    }
                    if (foundEntity.HasComponent<HealthComponent>())
                    {
                        var healthComp = foundEntity.GetComponent<HealthComponent>();
                        if (entityMemento.MaxHealth.HasValue) healthComp.MaxHealth = entityMemento.MaxHealth.Value;
                        healthComp.CurrentHealth = MathHelper.Clamp(entityMemento.CurrentHealth ?? healthComp.MaxHealth, 0, healthComp.MaxHealth);
                        if (healthComp.IsDead)
                        {
                            var ai = foundEntity.GetComponent<AIComponent>();
                            if (ai != null) ai.CurrentState = Entities.Mobs.AI.AIState.Dead;
                            foundEntity.GetComponent<AnimationComponent>()?.PlayAnimation("Dead");
                        }
                    }
                    if (foundEntity.HasComponent<InventoryComponent>() && entityMemento.InventoryItems != null)
                    {
                        var invComp = foundEntity.GetComponent<InventoryComponent>();
                        invComp.InitializeEmptySlots();
                        foreach (var itemMem in entityMemento.InventoryItems) invComp.AddItem(itemMem.Type, itemMem.Quantity);
                    }
                }
            }
            if (idsToRemove.Count > 0) foreach (ulong idToRemove in idsToRemove) _entityManager.RemoveEntity(idToRemove);
        }
    }
}