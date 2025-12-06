using System.Windows.Media;
using System.Diagnostics;


namespace SpeculativeContacts.Code
{
    public struct GameLoopUpdateEventArgs
    {
        public TimeSpan Elapsed;

        public GameLoopUpdateEventArgs(TimeSpan elapsed)
        {
            Elapsed = elapsed;
        }
    }

    public class GameLoop
    {
        public delegate void UpdateHandler(object sender, GameLoopUpdateEventArgs e);
        public event UpdateHandler UpdateEvent;

        private bool _paused = true;
        private TimeSpan _lastUpdateTime;
        private TimeSpan _accumulatedTime = TimeSpan.Zero;

        // Target frame time for 60 FPS
        private readonly TimeSpan TargetFrameTime = TimeSpan.FromSeconds(1.0 / 60.0);

        public GameLoop()
        {
        }

        void RenderUpdate(object sender, EventArgs e)
        {
            RenderingEventArgs args = (RenderingEventArgs)e;

            TimeSpan now = args.RenderingTime;

            if (_lastUpdateTime.TotalMilliseconds == 0)
            {
                // get a base value
                _lastUpdateTime = now;
                return;
            }

            // work out delta
            TimeSpan elapsed = (now - _lastUpdateTime);
            _lastUpdateTime = now;
            _accumulatedTime += elapsed;

            Debug.Assert(UpdateEvent != null);

            // Run UpdateEvent as many times as needed to catch up
            while (_accumulatedTime >= TargetFrameTime)
            {
                UpdateEvent(this, new GameLoopUpdateEventArgs(TargetFrameTime));
                _accumulatedTime -= TargetFrameTime;
            }
        }

        public virtual void Start()
        {
            if (_paused)
            {
                CompositionTarget.Rendering += RenderUpdate;
                _paused = false;
            }
        }

        public virtual void Stop()
        {
            if (!_paused)
            {
                CompositionTarget.Rendering -= RenderUpdate;
                _paused = true;
            }
        }

        public virtual bool IsPaused() => _paused;
    }
}