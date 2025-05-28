using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.Patterns.Decorator
{
    internal class GatherResourceAction : IGameAction
    {
        private string _resourceName;

        public GatherResourceAction(string resourceName)
        {
            _resourceName = resourceName;
        }

        public string Execute()
        {
            return $"Gathered {_resourceName}";
        }
    }
}
