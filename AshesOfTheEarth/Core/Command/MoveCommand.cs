using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Graphics.Animation;
using AshesOfTheEarth.UI;
using AshesOfTheEarth.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using AshesOfTheEarth.Core.Input.Command.Templates;

namespace AshesOfTheEarth.Core.Input.Command
{
    public class MoveCommand : AbstractGameCommand
    {
        private readonly Vector2 _direction;

        public MoveCommand(Vector2 direction)
        {
            _direction = direction;
        }

        protected override bool CanExecuteGameplayConditions(Entity entity, UIManager uiManager, GameTime gameTime)
        {
            var controller = entity.GetComponent<PlayerControllerComponent>();
            if (controller == null || controller.IsAttacking || _direction == Vector2.Zero)
            {
                return false;
            }
            if (entity.GetComponent<TransformComponent>() == null ||
                entity.GetComponent<AnimationComponent>() == null ||
                entity.GetComponent<StatsComponent>() == null ||
                entity.GetComponent<SpriteComponent>() == null)
            {
                return false;
            }
            return true;
        }

        protected override void PerformAction(Entity entity, GameTime gameTime)
        {
            var transform = entity.GetComponent<TransformComponent>();
            var animationComp = entity.GetComponent<AnimationComponent>();
            var stats = entity.GetComponent<StatsComponent>();
            var sprite = entity.GetComponent<SpriteComponent>();
            var controller = entity.GetComponent<PlayerControllerComponent>();
            var inputManager = ServiceLocator.Get<InputManager>();
            var entityManager = ServiceLocator.Get<EntityManager>();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float actualSpeed = controller.WalkSpeed;
            string animPrefix = "Walk_";

            if (inputManager.IsKeyDown(Keys.LeftShift) || inputManager.IsKeyDown(Keys.RightShift))
            {
                if (stats.TryUseStamina(stats.StaminaDrainRateRun * deltaTime))
                {
                    actualSpeed = controller.WalkSpeed + controller.RunSpeedOffset;
                    animPrefix = "Run_";
                }
            }
            else
            {
                stats.RegenStamina(stats.StaminaRegenRate * 0.5f * deltaTime);
            }

            Vector2 moveVector = _direction;
            if (moveVector.LengthSquared() > 1.01f)
                moveVector.Normalize();

            Vector2 velocity = moveVector * actualSpeed;
            Vector2 oldPosition = transform.Position;
            Vector2 nextPosition = transform.Position + velocity * deltaTime;
            bool didMove = false;

            if (controller.CanMoveTo(entity, nextPosition))
            {
                transform.Position = nextPosition;
                didMove = true;
            }
            else
            {
                Vector2 nextPositionX = new Vector2(nextPosition.X, transform.Position.Y);
                if (moveVector.X != 0 && controller.CanMoveTo(entity, nextPositionX))
                {
                    transform.Position = nextPositionX;
                    didMove = true;
                }
                else
                {
                    Vector2 nextPositionY = new Vector2(transform.Position.X, nextPosition.Y);
                    if (moveVector.Y != 0 && controller.CanMoveTo(entity, nextPositionY))
                    {
                        transform.Position = nextPositionY;
                        didMove = true;
                    }
                }
            }

            if (didMove)
            {
                entityManager.OnEntityMoved(entity, oldPosition);

                string targetAnimationName;
                if (Math.Abs(moveVector.X) > Math.Abs(moveVector.Y))
                {
                    targetAnimationName = animPrefix + (moveVector.X > 0 ? "Right" : "Left");
                    sprite.Effects = (moveVector.X > 0) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                }
                else
                {
                    targetAnimationName = animPrefix + (moveVector.Y > 0 ? "Down" : "Up");
                }
                animationComp.PlayAnimation(targetAnimationName);
            }
            else
            {
                string facingDirection = PlayerControllerComponent.GetFacingDirectionFromAnimation(animationComp.Controller.CurrentAnimationName, sprite.Effects);
                string idleAnimName = "Idle_" + facingDirection;
                animationComp.PlayAnimation(idleAnimName);
            }
        }
        protected override void OnGameplayConditionFailed(Entity entity, GameTime gameTime)
        {
            string reason = "Unknown";
            var controller = entity.GetComponent<PlayerControllerComponent>();
            if (controller == null) reason = "No PlayerControllerComponent";
            else if (controller.IsAttacking) reason = "Player is attacking";
            else if (_direction == Vector2.Zero) reason = "No movement direction";
            else if (entity.GetComponent<TransformComponent>() == null) reason = "No TransformComponent";
        }
    }
}