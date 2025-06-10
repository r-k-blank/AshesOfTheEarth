using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.Entities;
using AshesOfTheEarth.Entities.Components;
using AshesOfTheEarth.UI;
using Microsoft.Xna.Framework;
using AshesOfTheEarth.Core.Input.Command.Templates;

namespace AshesOfTheEarth.Core.Input.Command
{
    public class InteractCommand : AbstractGameCommand
    {
        public static readonly InteractCommand Instance = new InteractCommand();

        private InteractCommand() { }

        protected override bool CanExecuteGameplayConditions(Entity entity, UIManager uiManager, GameTime gameTime)
        {
            var controller = entity.GetComponent<PlayerControllerComponent>();
            return controller != null && !controller.IsAttacking && controller.InteractCooldownTimer <= 0;
        }

        protected override void PerformAction(Entity entity, GameTime gameTime)
        {
            var controller = entity.GetComponent<PlayerControllerComponent>();
            controller?.InitiateInteraction(gameTime);
        }
        protected override void OnGameplayConditionFailed(Entity entity, GameTime gameTime)
        {
            //System.Diagnostics.Debug.WriteLine("Interact command gameplay conditions not met (e.g. on cooldown, attacking).");
        }
        protected override void OnPreConditionFailed(Entity entity, GameTime gameTime)
        {
            //System.Diagnostics.Debug.WriteLine("Interact command pre-conditions not met (e.g. inventory open).");
        }
    }
}