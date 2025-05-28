using AshesOfTheEarth.Core.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace AshesOfTheEarth.Core.Time
{
    public enum DayPhase { Dawn, Day, Dusk, Night }

    public class TimeManager
    {
        public const float SecondsPerMinute = 1f;
        public const float MinutesPerHour = 60f;
        public const float HoursPerDay = 24f;
        public const float DayStartTime = 6f;
        public const float DuskStartTime = 18f;
        public const float NightStartTime = 20f;
        public const float DawnStartTime = 4f;

        public float TimeOfDayHours { get; private set; } = 12f;
        public int DayNumber { get; private set; } = 1;
        public DayPhase CurrentDayPhase { get; private set; } = DayPhase.Day;

        private float _timeAccumulatorSeconds = 0f;
        private int _lastHourBroadcasted = -1;

        private readonly List<ITimeObserver> _timeObservers = new List<ITimeObserver>();
        private readonly List<TimerEvent> _timerEvents = new List<TimerEvent>();

        private class TimerEvent
        {
            public Action Callback { get; }
            public TimeSpan TargetTime { get; }
            public bool IsRecurring { get; }
            public TimeSpan Interval { get; }
            public TimeSpan TimeElapsed { get; set; }

            public TimerEvent(Action callback, TimeSpan delay, bool recurring = false, TimeSpan interval = default)
            {
                Callback = callback; TargetTime = delay; IsRecurring = recurring; Interval = interval; TimeElapsed = TimeSpan.Zero;
            }
        }

        public TimeManager() { UpdateDayPhase(); _lastHourBroadcasted = (int)TimeOfDayHours; }
        public void Subscribe(ITimeObserver observer) { if (observer != null && !_timeObservers.Contains(observer)) _timeObservers.Add(observer); }
        public void Unsubscribe(ITimeObserver observer) { if (observer != null) _timeObservers.Remove(observer); }
        private void NotifyTimeChanged() { foreach (var observer in new List<ITimeObserver>(_timeObservers)) observer.OnTimeChanged(this); }
        private void NotifyDayPhaseChanged() { foreach (var observer in new List<ITimeObserver>(_timeObservers)) observer.OnDayPhaseChanged(CurrentDayPhase); ServiceLocator.Get<Gameplay.Events.EventManager>()?.RaiseEvent("DayPhaseChanged", this, new Gameplay.Events.GameEventArgs { ["NewPhase"] = CurrentDayPhase }); }
        private void NotifyHourElapsed(int hour) { foreach (var observer in new List<ITimeObserver>(_timeObservers)) observer.OnHourElapsed(hour); }

        public void SetTimeout(Action callback, TimeSpan delay)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            _timerEvents.Add(new TimerEvent(callback, delay));
        }
        public void SetInterval(Action callback, TimeSpan interval)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            _timerEvents.Add(new TimerEvent(callback, interval, true, interval));
        }
        public void ProcessTimeouts(GameTime gameTime)
        {
            if (_timerEvents.Count == 0) return;
            List<TimerEvent> eventsToRemove = null;
            for (int i = 0; i < _timerEvents.Count; i++)
            {
                var timer = _timerEvents[i]; timer.TimeElapsed += gameTime.ElapsedGameTime;
                if (timer.TimeElapsed >= timer.TargetTime)
                {
                    try { timer.Callback?.Invoke(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Timer callback error: {ex.Message}"); }
                    if (!timer.IsRecurring) { if (eventsToRemove == null) eventsToRemove = new List<TimerEvent>(); eventsToRemove.Add(timer); }
                    else { timer.TimeElapsed -= timer.Interval; }
                }
            }
            if (eventsToRemove != null) foreach (var timer in eventsToRemove) _timerEvents.Remove(timer);
        }

        public void ResetTime()
        {
            TimeOfDayHours = 8f; DayNumber = 1; _timeAccumulatorSeconds = 0f; UpdateDayPhase(); _lastHourBroadcasted = (int)TimeOfDayHours; _timerEvents.Clear(); NotifyTimeChanged();
        }
        public void RestoreTime(TimeMemento memento)
        {
            if (memento == null) { ResetTime(); return; }
            TimeOfDayHours = memento.TimeOfDayHours; DayNumber = memento.DayNumber; DayPhase oldPhase = CurrentDayPhase; CurrentDayPhase = memento.CurrentDayPhase; _timeAccumulatorSeconds = 0f; _lastHourBroadcasted = (int)TimeOfDayHours; _timerEvents.Clear(); NotifyTimeChanged();
            if (oldPhase != CurrentDayPhase) NotifyDayPhaseChanged();
        }

        public void Update(GameTime gameTime)
        {
            float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds; _timeAccumulatorSeconds += deltaSeconds;
            float minutesPassed = _timeAccumulatorSeconds / SecondsPerMinute;
            if (minutesPassed >= 1f)
            {
                _timeAccumulatorSeconds -= SecondsPerMinute * (int)minutesPassed;
                float hoursPassed = (int)minutesPassed / MinutesPerHour; TimeOfDayHours += hoursPassed;
                int currentHourInt = (int)TimeOfDayHours;
                if (TimeOfDayHours >= HoursPerDay) { TimeOfDayHours -= HoursPerDay; DayNumber++; currentHourInt = (int)TimeOfDayHours; }
                if (currentHourInt != _lastHourBroadcasted) { NotifyHourElapsed(currentHourInt); _lastHourBroadcasted = currentHourInt; UpdateDayPhase(); }
                NotifyTimeChanged();
            }
            ProcessTimeouts(gameTime);
        }

        private void UpdateDayPhase()
        {
            DayPhase oldPhase = CurrentDayPhase; DayPhase newPhase;
            if (TimeOfDayHours >= NightStartTime || TimeOfDayHours < DawnStartTime) newPhase = DayPhase.Night;
            else if (TimeOfDayHours >= DuskStartTime) newPhase = DayPhase.Dusk;
            else if (TimeOfDayHours >= DayStartTime) newPhase = DayPhase.Day; else newPhase = DayPhase.Dawn;
            if (oldPhase != newPhase) { CurrentDayPhase = newPhase; NotifyDayPhaseChanged(); }
        }

        public string GetFormattedTime() { int h = (int)TimeOfDayHours; int m = (int)((TimeOfDayHours - h) * MinutesPerHour); return $"{h:D2}:{m:D2}"; }

        public float GetDaylightFactor()
        {
            const float nightLight = 0.2f;
            const float dawnLightStart = 0.10f;
            const float dayLight = 1.0f;

            switch (CurrentDayPhase)
            {
                case DayPhase.Day:
                    return dayLight;
                case DayPhase.Dawn:
                    float dawnProgress = MathHelper.Clamp((TimeOfDayHours - DawnStartTime) / (DayStartTime - DawnStartTime), 0f, 1f);
                    return MathHelper.Lerp(dawnLightStart, dayLight, dawnProgress);
                case DayPhase.Dusk:
                    float duskProgress = MathHelper.Clamp((TimeOfDayHours - DuskStartTime) / (NightStartTime - DuskStartTime), 0f, 1f);
                    return MathHelper.Lerp(dayLight, nightLight, duskProgress);
                case DayPhase.Night:
                    return nightLight;
                default:
                    return dayLight;
            }
        }
    }
}