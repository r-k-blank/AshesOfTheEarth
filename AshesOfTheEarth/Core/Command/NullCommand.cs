using AshesOfTheEarth.Entities;
using Microsoft.Xna.Framework;
using AshesOfTheEarth.Core.Input.Command.Templates;
using AshesOfTheEarth.UI;

namespace AshesOfTheEarth.Core.Input.Command
{
    public class NullCommand : AbstractGameCommand
    {
        public static readonly NullCommand Instance = new NullCommand();

        private NullCommand() { }

        protected override bool AllowsExecutionInInventory() => true;

        protected override bool CanExecutePreConditions(Entity entity, UIManager uiManager, GameTime gameTime)
        {
            return true;
        }
        protected override bool CanExecuteGameplayConditions(Entity entity, UIManager uiManager, GameTime gameTime)
        {
            return true;
        }
        protected override void PerformAction(Entity entity, GameTime gameTime)
        {
        }
    }
}