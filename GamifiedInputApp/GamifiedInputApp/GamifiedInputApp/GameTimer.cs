using System;

namespace GamifiedInputApp
{
    // Custom timer class with getters for start and end times
    class GameTimer : System.Timers.Timer
    {
        private DateTime m_startTime;
        private DateTime m_endTime;

        private DateTime m_currFrame;
        private DateTime m_lastFrame;

        public GameTimer() : base()
        {
            this.Elapsed += this.ElapsedEvent;
        }

        protected new void Dispose()
        {
            this.Elapsed -= this.ElapsedEvent;
            base.Dispose();
        }

        /// <summary>
        /// Get the time elapsed since the timer started
        /// </summary>
        public double TimeElapsed => (this.m_currFrame - this.m_startTime).TotalMilliseconds;

        /// <summary>
        /// Get the time remaining until the Elapsed event is fired
        /// </summary>
        public double TimeRemaining => (this.m_endTime - this.m_currFrame).TotalMilliseconds;

        /// <summary>
        /// Get the time since the last frame (for animation purposes)
        /// </summary>
        public double DeltaTime => (this.m_currFrame - this.m_lastFrame).TotalMilliseconds;

        /// <summary>
        /// Returns true if the timer has finished (and AutoReset is off) or false otherwise
        /// </summary>
        public bool Finished => !this.AutoReset && this.TimeRemaining <= 0;

        /// <summary>
        /// Should be called once per game loop, prior to calling update() to process game logic
        /// Note that for accurate timing these two values need to be set at the same time
        /// </summary>
        public void NextFrame()
        {
            this.m_lastFrame = this.m_currFrame;
            this.m_currFrame = DateTime.Now;
        }

        public new void Start()
        {
            this.ResetTimer();
            base.Start();
        }

        private void ElapsedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.AutoReset)
            {
                this.ResetTimer();
            }
        }

        private void ResetTimer()
        {
            this.m_startTime = DateTime.Now;
            this.m_endTime = m_startTime.AddMilliseconds(this.Interval);
            this.m_lastFrame = this.m_currFrame = this.m_startTime;
        }
    }
}
