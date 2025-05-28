using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using AshesOfTheEarth.Core.Input.Command;
using AshesOfTheEarth.Core.Input.ChainOfResponsibility;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.UI;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities.Components;

namespace AshesOfTheEarth.Core.Input
{
    public class InputManager
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;

        private IInputHandler _inputHandlerChain;

        public InputManager()
        {
            SetupInputChain();
        }

        private void SetupInputChain()
        {
            var toggleUIHandler = new ToggleUIHandler();
            var placementModeHandler = new PlacementModeInputHandler();
            var actionKeyHandler = new ActionKeyHandler();
            var movementKeyHandler = new MovementKeyHandler();
            var defaultInputHandler = new DefaultInputHandler();

            toggleUIHandler
                .SetNext(placementModeHandler)
                .SetNext(actionKeyHandler)
                .SetNext(movementKeyHandler)
                .SetNext(defaultInputHandler);

            _inputHandlerChain = toggleUIHandler;
        }

        public void Update(GameTime gameTime)
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
        }

        public bool IsKeyDown(Keys key) => _currentKeyboardState.IsKeyDown(key);
        public bool IsKeyUp(Keys key) => _currentKeyboardState.IsKeyUp(key);
        public bool IsKeyPressed(Keys key) => _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        public bool IsKeyReleased(Keys key) => _currentKeyboardState.IsKeyUp(key) && _previousKeyboardState.IsKeyDown(key);

        public Vector2 GetCurrentMovementDirection()
        {
            Vector2 direction = Vector2.Zero;
            if (IsKeyDown(Keys.W) || IsKeyDown(Keys.Up)) direction.Y -= 1;
            if (IsKeyDown(Keys.S) || IsKeyDown(Keys.Down)) direction.Y += 1;
            if (IsKeyDown(Keys.A) || IsKeyDown(Keys.Left)) direction.X -= 1;
            if (IsKeyDown(Keys.D) || IsKeyDown(Keys.Right)) direction.X += 1;
            return direction;
        }

        public Point MousePosition => _currentMouseState.Position;
        public bool IsLeftMouseButtonDown() => _currentMouseState.LeftButton == ButtonState.Pressed;
        public bool IsRightMouseButtonDown() => _currentMouseState.RightButton == ButtonState.Pressed;
        public bool IsLeftMouseButtonPressed() => _currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
        public bool IsRightMouseButtonPressed() => _currentMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released;
        public int GetMouseScrollWheelDelta() => _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue;

        public ICommand HandleInputForPlayer(Entity playerEntity, GameTime gameTime)
        {
            var uiManager = ServiceLocator.Get<UIManager>();
            if (_inputHandlerChain == null)
            {
                SetupInputChain();
            }
            return _inputHandlerChain.ProcessInput(this, playerEntity, gameTime, uiManager);
        }
    }
}