using System;
using System.Collections.Generic;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;

using GamifiedInputApp.Minigames;

namespace GamifiedInputApp
{
    public enum GameState
    {
        Start,
        Play,
        Results
    }

    public struct GameContext
    {
        public GameState State; // current game state
        public GameTimer Timer; // minigame timer
        public int Score; // total score
    }

    public class ResultsEventArgs
    {
        public ResultsEventArgs(GameContext context)
        {
            Score = context.Score;
        }

        public int Score { get; }
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

        public delegate void ResultsEventHandler(object sender, ResultsEventArgs e);
        public event ResultsEventHandler Results;

        private GameContext m_context;
        private ContainerVisual m_rootVisual;
        private Queue<IMinigame> m_minigameQueue;
        private DispatcherTimer m_loopTimer;

        public GameCore(ContainerVisual rootVisual)
        {
            m_context.State = GameState.Start;
            m_context.Timer = new GameTimer();
            m_context.Timer.AutoReset = false;
            m_loopTimer = new DispatcherTimer();
            m_rootVisual = rootVisual;

            m_loopTimer.Interval = TimeSpan.FromSeconds(1.0 / MAX_FPS);
            m_loopTimer.Tick += GameLoop;
        }

        public void Run()
        {
            // setup code here
            m_minigameQueue = new Queue<IMinigame>();
            Minigames.Add(new CharacterReceived());

            // TODO: filter by selected minigames
            foreach (IMinigame minigame in Minigames)
            {
                m_minigameQueue.Enqueue(minigame);
            }

            // start game
            m_context.State = GameState.Start;
            m_loopTimer.Start();
        }


        protected void GameLoop(Object source, object e)
        {
            if (!m_loopTimer.IsEnabled) return; // timer is disabled, ignore remaining queued events

            switch (m_context.State)
            {
                case GameState.Start:
                    if (m_minigameQueue.Count == 0) throw new MissingMemberException("No minigames selected");

                    // setup minigame
                    IMinigame current = m_minigameQueue.Peek();
                    current.Start(m_context, m_rootVisual);

                    // start timer
                    m_context.Timer.Interval = 2000;
                    m_context.Timer.Start();
                    m_context.State = GameState.Play;
                    break;
                case GameState.Play:
                    if (m_minigameQueue.Count == 0) throw new MissingMemberException("No minigames available");

                    // update minigame
                    this.UpdateMinigame();
                    break;
                case GameState.Results:
                    // stop timer
                    m_loopTimer.Stop();

                    // invoke results event
                    Results?.Invoke(this, new ResultsEventArgs(m_context));
                    break;
            }
        }

        private void UpdateMinigame()
        {
            IMinigame current = m_minigameQueue.Peek();

            m_context.Timer.NextFrame();
            MinigameState state = current.Update(m_context);

            if (state == MinigameState.Play && m_context.Timer.Finished)
            {
                state = MinigameState.Fail;
            }

            if (state == MinigameState.Pass)
            {
                current.End(m_context, state);
                m_context.Timer.Stop();

                m_minigameQueue.Dequeue();
                m_context.State = (m_minigameQueue.Count > 0) ? GameState.Start : GameState.Results;
            }
            else if (state == MinigameState.Fail)
            {
                current.End(m_context, state);
                m_context.Timer.Stop();
                m_context.State = GameState.Results;
            }
            else
            {
                // continue play
            }
        }
    }
}
