using System;
using Windows.Globalization.DateTimeFormatting;

namespace GamifiedInputApp
{
    // Custom timer class with getters for start and end times
    public class GameTimer
    {
        private DateTime m_startTime;
        private DateTime m_endTime;

        private DateTime m_currFrame;
        private DateTime m_lastFrame;

        public bool StepFrames { get; set; } = false;
        public double Interval { get; set; } = 1000;

        /// <summary>
        /// Get the current time, as seen by the timer
        /// </summary>
        public DateTime CurrentTime => StepFrames ? m_currFrame : DateTime.Now;

        /// <summary>
        /// Get the time elapsed since the timer started
        /// </summary>
        public TimeSpan TimeElapsed => (CurrentTime - m_startTime);

        /// <summary>
        /// Get the time remaining until the Elapsed event is fired
        /// </summary>
        public TimeSpan TimeRemaining => (m_endTime - CurrentTime);

        /// <summary>
        /// Get the time since the last frame (for animation purposes)
        /// If StepFrames is false, returns 0
        /// </summary>
        public TimeSpan DeltaTime => StepFrames ? (m_currFrame - m_lastFrame) : new TimeSpan(0);

        /// <summary>
        /// Returns true if the timer has finished (and AutoReset is off) or false otherwise
        /// </summary>
        public bool Finished => TimeRemaining.TotalMilliseconds <= 0;

        /// <summary>
        /// Should be called once per game loop, prior to calling update() to process game logic
        /// Note that for accurate timing these two values need to be set at the same time
        /// </summary>
        public void NextFrame()
        {
            m_lastFrame = m_currFrame;
            m_currFrame = DateTime.Now;
        }

        public void Start()
        {
            m_startTime = DateTime.Now;
            m_endTime = m_startTime.AddMilliseconds(Interval);
            m_lastFrame = m_currFrame = m_startTime;
        }
    }
}
