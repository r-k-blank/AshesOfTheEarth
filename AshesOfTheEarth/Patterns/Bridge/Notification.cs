using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.Patterns.Bridge
{
    internal abstract class Notification
    {
        protected INotificationChannel _channel;

        public Notification(INotificationChannel channel)
        {
            _channel = channel;
        }

        public abstract void Notify(string message);
    }
}
