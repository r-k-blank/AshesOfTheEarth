using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.Patterns.Decorator
{
    internal class LoggingActionDecorator : ActionDecorator
    {
        public LoggingActionDecorator(IGameAction action) : base(action) { }

        public override string Execute()
        {
            Console.WriteLine($"[LOG] Starting action: {_wrappedAction?.GetType().Name ?? "Unknown"}...");
            string result = base.Execute();
            Console.WriteLine($"[LOG] Action finished. Result: {result}");
            return result;
        }
    }
}
