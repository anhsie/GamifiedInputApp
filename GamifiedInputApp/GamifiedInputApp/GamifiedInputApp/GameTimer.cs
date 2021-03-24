using System;

namespace GamifiedInputApp
{
    // Custom timer class with getters for start and end times
    public class GameTimer : System.Timers.Timer
    {
        private DateTime m_startTime;
        private DateTime m_endTime;

        private DateTime m_currFrame;
        private DateTime m_lastFrame;

        public bool StepFrames { get; set; }

        public GameTimer() : base()
        {
            Elapsed += ElapsedEvent;
            StepFrames = false;
        }

        protected new void Dispose()
        {
            Elapsed -= ElapsedEvent;
            base.Dispose();
        }

        /// <summary>
        /// Get the current time, as seen by the timer
        /// </summary>
        public DateTime CurrentTime => StepFrames ? m_currFrame : DateTime.Now;

        /// <summary>
        /// Get the time elapsed since the timer started
        /// </summary>
        public double TimeElapsed => (CurrentTime - m_startTime).TotalMilliseconds;

        /// <summary>
        /// Get the time remaining until the Elapsed event is fired
        /// </summary>
        public double TimeRemaining => (m_endTime - CurrentTime).TotalMilliseconds;

        /// <summary>
        /// Get the time since the last frame (for animation purposes)
        /// If StepFrames is false, returns 0
        /// </summary>
        public double DeltaTime => StepFrames ? (m_currFrame - m_lastFrame).TotalMilliseconds : 0;

        /// <summary>
        /// Returns true if the timer has finished (and AutoReset is off) or false otherwise
        /// </summary>
        public bool Finished => !AutoReset && TimeRemaining <= 0;

        /// <summary>
        /// Should be called once per game loop, prior to calling update() to process game logic
        /// Note that for accurate timing these two values need to be set at the same time
        /// </summary>
        public void NextFrame()
        {
            m_lastFrame = m_currFrame;
            m_currFrame = DateTime.Now;
        }

        public new void Start()
        {
            ResetTimer();
            base.Start();
        }

        private void ElapsedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (AutoReset) { ResetTimer(); }
        }

        private void ResetTimer()
        {
            m_startTime = DateTime.Now;
            m_endTime = m_startTime.AddMilliseconds(Interval);
            m_lastFrame = m_currFrame = m_startTime;
        }
    }
}
