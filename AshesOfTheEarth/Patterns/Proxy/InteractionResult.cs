using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.Patterns.Proxy
{
    internal class InteractionResult
    {
        public bool Success { get; }
        public string Message { get; } // Mesaj pentru jucător (ex: "Chest opened", "You need a key")

        public InteractionResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static InteractionResult Succeeded(string message = "Interaction successful.") => new InteractionResult(true, message);
        public static InteractionResult Failed(string message) => new InteractionResult(false, message);
    }
}
