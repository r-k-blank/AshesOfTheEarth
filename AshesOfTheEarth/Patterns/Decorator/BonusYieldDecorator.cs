using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.Patterns.Decorator
{
    internal class BonusYieldDecorator : ActionDecorator
    {
        private int _bonusAmount;

        public BonusYieldDecorator(IGameAction action, int bonusAmount = 1) : base(action)
        {
            _bonusAmount = bonusAmount;
        }

        public override string Execute()
        {
            string originalResult = base.Execute();
            return $"{originalResult} (+{_bonusAmount} bonus)";
        }
    }
}
