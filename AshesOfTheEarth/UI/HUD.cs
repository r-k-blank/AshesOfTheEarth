using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.Core.Services; // Pentru a accesa entitatea Player
using AshesOfTheEarth.Entities; // Pentru Entity
using AshesOfTheEarth.Entities.Components; // Pentru componentele Player-ului
using AshesOfTheEarth.UI.Widgets; // Pentru ProgressBar
using AshesOfTheEarth.Core.Time;
using System.Linq; // Pentru TimeManager și DayPhase

namespace AshesOfTheEarth.UI
{
    public class HUD
    {
        private SpriteFont _font;
        private Texture2D _pixelTexture; // Textură 1x1 albă pentru desenat forme simple

        // Bare de progres pentru Player
        private ProgressBar _healthBar;
        private ProgressBar _hungerBar;
        private ProgressBar _staminaBar;

        // Text pentru informații
        private string _timeText = "00:00";
        private string _dayText = "Day 1";
        private string _phaseText = "Day";
        private Color _phaseColor = Color.White;

        // Poziții UI (pot fi făcute mai dinamice)
        private Vector2 _statsPosition = new Vector2(20, 20);
        private int _barWidth = 150;
        private int _barHeight = 15;
        private int _barSpacing = 5;
        private Vector2 _timePosition; // Calculat pe baza dimensiunii ecranului

        private Entity _player; // Referință către entitatea player

        public HUD(Microsoft.Xna.Framework.Content.ContentManager content, GraphicsDevice graphicsDevice)
        {
            try
            {
                _font = content.Load<SpriteFont>("Fonts/DefaultFont");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading font for HUD: {ex.Message}");
                // Folosește un font default al sistemului sau nu desena text?
            }


            // Creează textura pixel
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });

            // Inițializează barele de progres
            int currentY = (int)_statsPosition.Y;
            _healthBar = new ProgressBar(new Rectangle((int)_statsPosition.X, currentY, _barWidth, _barHeight), 100f)
            {
                ForegroundColor = Color.Red,
                BackgroundColor = Color.DarkRed * 0.7f
            };
            currentY += _barHeight + _barSpacing;
            _hungerBar = new ProgressBar(new Rectangle((int)_statsPosition.X, currentY, _barWidth, _barHeight), 100f)
            {
                ForegroundColor = Color.Orange, // Verde când e plin, portocaliu/roșu când e gol? - Inversăm logica afișării
                BackgroundColor = Color.Brown * 0.7f,
            };
            currentY += _barHeight + _barSpacing;
            _staminaBar = new ProgressBar(new Rectangle((int)_statsPosition.X, currentY, _barWidth, _barHeight), 100f)
            {
                ForegroundColor = Color.Yellow,
                BackgroundColor = Color.DarkGray * 0.7f
            };


            // Calculează poziția textului pentru timp (colț dreapta sus)
            _timePosition = new Vector2(graphicsDevice.Viewport.Width - 150, 20);
        }

        // Metodă pentru a găsi și stoca referința la player
        private void FindPlayer()
        {
            if (_player == null)
            {
                var entityManager = ServiceLocator.Get<EntityManager>();
                _player = entityManager?.GetAllEntities().FirstOrDefault(e => e.HasComponent<PlayerControllerComponent>());
                // if (_player != null) System.Diagnostics.Debug.WriteLine("HUD found Player entity.");
                // else System.Diagnostics.Debug.WriteLine("HUD could not find Player entity.");
            }
        }


        public void Update(GameTime gameTime)
        {
            FindPlayer(); // Încearcă să găsească player-ul dacă nu îl are deja

            if (_player != null)
            {
                // Actualizează barele de progres pe baza componentelor player-ului
                var health = _player.GetComponent<HealthComponent>();
                var stats = _player.GetComponent<StatsComponent>();

                if (health != null)
                {
                    _healthBar?.SetPercentage(health.CurrentHealth / health.MaxHealth);
                }
                if (stats != null)
                {
                    // Inversăm foamea: bara e plină când CurrentHunger e 0
                    _hungerBar?.SetPercentage(1.0f - stats.HungerPercentage);
                    _staminaBar?.SetPercentage(stats.StaminaPercentage);
                }
            }
            else
            {
                // Poate resetează barele la 0 sau le ascunde dacă player-ul nu există (ex: meniu)
                _healthBar?.SetPercentage(0);
                _hungerBar?.SetPercentage(0);
                _staminaBar?.SetPercentage(0);
            }
        }

        // Metode apelate de observatori (din UIManager)
        public void UpdateTimeDisplay(string time, int day)
        {
            _timeText = time;
            _dayText = $"Day {day}";
        }
        public void UpdateDayPhaseDisplay(DayPhase phase)
        {
            _phaseText = phase.ToString();
            switch (phase)
            {
                case DayPhase.Dawn: _phaseColor = Color.LightSkyBlue; break;
                case DayPhase.Day: _phaseColor = Color.White; break;
                case DayPhase.Dusk: _phaseColor = Color.OrangeRed; break;
                case DayPhase.Night: _phaseColor = Color.SlateGray; break;
            }
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            // Desenează barele de progres
            _healthBar?.Draw(spriteBatch, _pixelTexture);
            _hungerBar?.Draw(spriteBatch, _pixelTexture);
            _staminaBar?.Draw(spriteBatch, _pixelTexture);

            // Desenează textul (doar dacă fontul a fost încărcat)
            if (_font != null)
            {
                // Desenează textul pentru timp/zi
                spriteBatch.DrawString(_font, _timeText, _timePosition, _phaseColor);
                spriteBatch.DrawString(_font, _dayText, _timePosition + new Vector2(0, _font.LineSpacing), _phaseColor);
                spriteBatch.DrawString(_font, _phaseText, _timePosition + new Vector2(0, _font.LineSpacing * 2), _phaseColor);


                // Adaugă etichete simple pentru bare (opțional)
                Vector2 labelOffset = new Vector2(0, -_font.LineSpacing * 0.8f); // Putin deasupra barei
                                                                                 // spriteBatch.DrawString(_font, "HP", _healthBar.Bounds.Location.ToVector2() + labelOffset, Color.White);
                                                                                 // spriteBatch.DrawString(_font, "Hunger", _hungerBar.Bounds.Location.ToVector2() + labelOffset, Color.White);
                                                                                 // spriteBatch.DrawString(_font, "Stamina", _staminaBar.Bounds.Location.ToVector2() + labelOffset, Color.White);

            }
        }

        public void Dispose() // Curăță textura pixel
        {
            _pixelTexture?.Dispose();
            _pixelTexture = null;
        }
    }
}