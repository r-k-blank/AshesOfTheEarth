using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;
using AshesOfTheEarth.Core.Input.Command.Templates;

namespace AshesOfTheEarth.Core.Input.Command
{
    public class AttackCommand : AbstractGameCommand
    {
        public static readonly AttackCommand Instance = new AttackCommand();

        private AttackCommand() { }

        protected override bool CanExecuteGameplayConditions(Entity entity, UIManager uiManager, GameTime gameTime)
        {
            var controller = entity.GetComponent<PlayerControllerComponent>();
            if (controller == null || controller.IsAttacking || controller.AttackCooldownTimer > 0)
            {
                return false;
            }
            var stats = entity.GetComponent<StatsComponent>();
            if (stats != null && !stats.TryUseStamina(5f))
            {
                System.Diagnostics.Debug.WriteLine("Attack failed: Not enough stamina.");
                return false;
            }
            return true;
        }

        protected override void PerformAction(Entity entity, GameTime gameTime)
        {
            var controller = entity.GetComponent<PlayerControllerComponent>();
            controller?.InitiateAttack(gameTime);
        }

        protected override void OnActionSuccess(Entity entity, GameTime gameTime)
        {
            var controller = entity.GetComponent<PlayerControllerComponent>();
            if (controller != null)
            {
                controller.AttackCooldownTimer = PlayerControllerComponent.ATTACK_COOLDOWN_VALUE;
            }
            base.OnActionSuccess(entity, gameTime);
        }

        protected override void OnGameplayConditionFailed(Entity entity, GameTime gameTime)
        {
            // System.Diagnostics.Debug.WriteLine("Attack command gameplay conditions not met (e.g. on cooldown, no stamina, already attacking).");
        }
        protected override void OnPreConditionFailed(Entity entity, GameTime gameTime)
        {
            // System.Diagnostics.Debug.WriteLine("Attack command pre-conditions not met (e.g. inventory open).");
        }
    }
}