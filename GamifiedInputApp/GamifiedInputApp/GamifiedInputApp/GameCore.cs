using System;
using System.Collections.Generic;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;

using GamifiedInputApp.Minigames;

namespace GamifiedInputApp
{
    enum GameState
    {
        Start,
        Play,
        Results
    }

    struct GameContext
    {
        public GameState state; // current game state
        public GameTimer timer; // minigame timer
    }

    class GameCore
    {
        private const double MAX_FPS = 60;

        public static List<IMinigame> Minigames { get; private set; }

        static GameCore()
        {
            Minigames = new List<IMinigame>();
            Minigames.Add(new DummyMinigame());
        }

        private GameContext m_context;
        private ContainerVisual m_rootVisual;
        private Queue<IMinigame> m_minigameQueue;
        private DispatcherTimer m_loopTimer;

        public GameCore(ContainerVisual rootVisual)
        {
            m_context.state = GameState.Start;
            m_context.timer = new GameTimer();
            m_loopTimer = new DispatcherTimer();
            m_rootVisual = rootVisual;

            m_loopTimer.Interval = TimeSpan.FromSeconds(1.0 / MAX_FPS);
            m_loopTimer.Tick += GameLoop;
        }

        public void Run()
        {
            // setup code here
            m_minigameQueue = new Queue<IMinigame>();

            // TODO: filter by selected minigames
            foreach (IMinigame minigame in Minigames)
            {
                m_minigameQueue.Enqueue(minigame);
            }

            // start game
            m_context.state = GameState.Start;
            m_loopTimer.Start();
        }

        protected void GameLoop(Object source, object e)
        {
            if (!m_loopTimer.IsEnabled) return; // timer is disabled, ignore remaining queued events

            switch (m_context.state)
            {
                case GameState.Start:
                    if (m_minigameQueue.Count == 0) throw new MissingMemberException("No minigames selected");

                    IMinigame current = m_minigameQueue.Peek();
                    current.Start(m_context, m_rootVisual);

                    m_context.timer.Start();
                    m_context.state = GameState.Play;
                    break;
                case GameState.Play:
                    if (m_minigameQueue.Count == 0) throw new MissingMemberException("No minigames available");

                    this.UpdateMinigame();
                    break;
                case GameState.Results:
                    m_loopTimer.Stop();
                    // goto results screen
                    break;
            }
        }

        private void UpdateMinigame()
        {
            IMinigame current = m_minigameQueue.Peek();

            m_context.timer.NextFrame();
            MinigameState state = current.Update(m_context);

            if (state == MinigameState.Play && m_context.timer.Finished)
            {
                state = MinigameState.Fail;
            }

            if (state == MinigameState.Pass)
            {
                current.End(m_context, state);
                m_context.timer.Stop();

                m_minigameQueue.Dequeue();
                m_context.state = (m_minigameQueue.Count > 0) ? GameState.Start : GameState.Results;
            }
            else if (state == MinigameState.Fail)
            {
                current.End(m_context, state);
                m_context.timer.Stop();
                m_context.state = GameState.Results;
            }
            else
            {
                // continue play
            }
        }
    }
}
