using System;
using KK_PantyFairy.Data;

namespace KK_PantyFairy.Events
{
    public class CustomEventData
    {
        public bool Running { get; private set; }
        public bool Initialized { get; private set; }

        public readonly StoryProgress Index;
        private readonly Func<IDisposable> _initialize;
        private readonly Action<bool> _runningChanged;

        private IDisposable _cleanupCallback;

        public CustomEventData(StoryProgress index,
            Func<IDisposable> initialize,
            Action<bool> runningChanged = null)
        {
            Index = index;
            _initialize = initialize;
            _runningChanged = runningChanged;
        }

        public void SetRunning(bool running)
        {
            if (Running != running)
            {
                if (running)
                {
                    if (!Initialized)
                    {
                        _cleanupCallback = _initialize();
                        Initialized = true;
                    }
                }
                else
                {
                    _cleanupCallback?.Dispose();
                    Initialized = false;
                }

                _runningChanged?.Invoke(running);

                Running = running;
            }
        }
    }
}