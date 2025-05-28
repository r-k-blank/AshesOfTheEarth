using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.Patterns.Bridge
{
    internal interface INotificationChannel
    {
        void Send(string title, string message);
    }
}
