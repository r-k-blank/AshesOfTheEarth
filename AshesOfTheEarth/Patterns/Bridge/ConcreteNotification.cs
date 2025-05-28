using AshesOfTheEarth.Patterns.Bridge;

namespace AshesOfTheEarth.Patterns.Bridge
{
    internal class SimpleNotification : Notification
    {
        public SimpleNotification(INotificationChannel channel) : base(channel) { }

        public override void Notify(string message)
        {
            _channel.Send("Info", message);
        }
    }

   internal  class UrgentNotification : Notification
    {
        public UrgentNotification(INotificationChannel channel) : base(channel) { }

        public override void Notify(string message)
        {
            _channel.Send("URGENT!", message.ToUpper());
        }
    }
}