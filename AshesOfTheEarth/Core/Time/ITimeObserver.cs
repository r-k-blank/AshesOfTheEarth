using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AshesOfTheEarth.Core.Time
{
    public interface ITimeObserver
    {
        void OnTimeChanged(TimeManager timeManager); // Notificare generală
        void OnDayPhaseChanged(DayPhase newPhase);    // Notificare specifică schimbării fazei
        void OnHourElapsed(int hour);                 // Notificare la fiecare oră de joc trecută
    }
}
