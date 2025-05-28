using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.Patterns.Decorator
{
    internal abstract class ActionDecorator : IGameAction
    {
        protected IGameAction _wrappedAction;

        public ActionDecorator(IGameAction action)
        {
            _wrappedAction = action;
        }

        public virtual string Execute()
        {
            return _wrappedAction?.Execute() ?? string.Empty;
        }
    }
}
