using AshesOfTheEarth.Entities;
using AshesOfTheEarth.UI;
using AshesOfTheEarth.Core.Services;
using Microsoft.Xna.Framework;
using AshesOfTheEarth.Entities.Components;

namespace AshesOfTheEarth.Core.Input.Command.Templates
{
    public abstract class AbstractGameCommand : ICommand
    {
        public void Execute(Entity entity, GameTime gameTime)
        {
            var uiManager = ServiceLocator.Get<UIManager>();

            if (!CanExecutePreConditions(entity, uiManager, gameTime))
            {
                OnPreConditionFailed(entity, gameTime);
                return;
            }

            if (!CanExecuteGameplayConditions(entity, uiManager, gameTime))
            {
                OnGameplayConditionFailed(entity, gameTime);
                return;
            }

            PerformAction(entity, gameTime);
            OnActionSuccess(entity, gameTime);
        }

        protected virtual bool CanExecutePreConditions(Entity entity, UIManager uiManager, GameTime gameTime)
        {
            if (entity == null)
            {
                return false;
            }

            var playerController = entity.GetComponent<PlayerControllerComponent>();
            if (playerController != null && playerController.IsInPlacementMode && AllowsExecutionDuringPlacementMode())
            {
                return true;
            }

            if ((uiManager.IsInventoryVisible() || uiManager.IsCraftingScreenVisible()) && !AllowsExecutionInInventory())
            {
                return false;
            }
            return true;
        }

        protected abstract bool CanExecuteGameplayConditions(Entity entity, UIManager uiManager, GameTime gameTime);

        protected abstract void PerformAction(Entity entity, GameTime gameTime);

        protected virtual void OnPreConditionFailed(Entity entity, GameTime gameTime)
        {
            //System.Diagnostics.Debug.WriteLine($"Command {this.GetType().Name} pre-conditions failed.");
        }
        protected virtual void OnGameplayConditionFailed(Entity entity, GameTime gameTime)
        {
            //System.Diagnostics.Debug.WriteLine($"Command {this.GetType().Name} gameplay conditions failed.");
        }
        protected virtual void OnActionSuccess(Entity entity, GameTime gameTime)
        {
            //System.Diagnostics.Debug.WriteLine($"Command {this.GetType().Name} executed successfully.");
        }

        protected virtual bool AllowsExecutionInInventory() => false;
        protected virtual bool AllowsExecutionDuringPlacementMode() => false;
    }
}