using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.Graphics.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AshesOfTheEarth.UI;
using AshesOfTheEarth.Core.Input.Command.Templates;

namespace AshesOfTheEarth.Core.Input.Command
{
    public class IdleCommand : AbstractGameCommand
    {
        public static readonly IdleCommand Instance = new IdleCommand();

        private IdleCommand() { }

        protected override bool AllowsExecutionInInventory() => true;

        protected override bool CanExecuteGameplayConditions(Entity entity, UIManager uiManager, GameTime gameTime)
        {
            var controller = entity.GetComponent<PlayerControllerComponent>();
            if (controller != null && controller.IsAttacking)
            {
                return false;
            }
            return entity.GetComponent<AnimationComponent>() != null &&
                   entity.GetComponent<SpriteComponent>() != null;
        }

        protected override void PerformAction(Entity entity, GameTime gameTime)
        {
            var animationComp = entity.GetComponent<AnimationComponent>();
            var sprite = entity.GetComponent<SpriteComponent>();
            var stats = entity.GetComponent<StatsComponent>();

            string currentAnimName = animationComp.Controller.CurrentAnimationName ?? "Idle_Down";
            string facingDirection = PlayerControllerComponent.GetFacingDirectionFromAnimation(currentAnimName, sprite.Effects);
            string targetAnimationName = "Idle_" + facingDirection;

            animationComp.PlayAnimation(targetAnimationName);

            if (facingDirection == "Left") sprite.Effects = SpriteEffects.FlipHorizontally;
            else if (facingDirection == "Right") sprite.Effects = SpriteEffects.None;

            stats?.RegenStamina(stats.StaminaRegenRate * (float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}