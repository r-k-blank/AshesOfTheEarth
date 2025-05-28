using System;
using AshesOfTheEarth.Patterns.Bridge;
using AshesOfTheEarth.Core.Services;
using AshesOfTheEarth.UI;

namespace AshesOfTheEarth.Patterns.Bridge
{
    public class HUDNotificationChannel : INotificationChannel
    {
        public void Send(string title, string message)
        {
            try
            {
                var uiManager = ServiceLocator.Get<UIManager>();
                Console.WriteLine($"[HUD NOTIFICATION] {title}: {message} (Implementation needed in UIManager/HUD)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HUD NOTIFICATION ERROR] Could not send notification: {ex.Message}");
            }
        }
    }

    public class DebugLogNotificationChannel : INotificationChannel
    {
        public void Send(string title, string message)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG NOTIFICATION] ({title}) - {message}");
            Console.WriteLine($"[CONSOLE NOTIFICATION] {title}: {message}");
        }
    }
}