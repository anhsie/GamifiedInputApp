using System;
using System.Collections.Generic;
using Microsoft.UI.Composition;

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
        public int score;
    }

    class GameCore
    {
        public static List<IMinigame> Minigames { get; private set; }
        static GameCore()
        {
            Minigames = new List<IMinigame>();
            Minigames.Add(new DummyMinigame());
        }

        private GameContext m_context;
        private ContainerVisual m_rootVisual;
        private Queue<IMinigame> m_minigameQueue;

        public GameCore(ContainerVisual rootVisual)
        {
            m_context.state = GameState.Start;
            m_context.timer = new GameTimer();
            m_rootVisual = rootVisual;
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

            m_context.state = GameState.Start;
            //while (this.GameLoop()) continue;

            // cleanup code here

            GoToResultsEventArgs args = new GoToResultsEventArgs();
            args.score = m_context.score;
            OnGoToResults(args);
        }

        protected void OnGoToResults(GoToResultsEventArgs e)
        {
            EventHandler<GoToResultsEventArgs> handler = GoToResults;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        public class GoToResultsEventArgs : EventArgs
        {
            public int score { get; set; }
        }

        public event EventHandler<GoToResultsEventArgs> GoToResults;

        protected bool GameLoop()
        {
            switch (m_context.state)
            {
                case GameState.Start:
                    if (m_minigameQueue.Count == 0) throw new MissingMemberException("No minigames selected");

                    IMinigame current = m_minigameQueue.Peek();
                    current.Start(m_context, m_rootVisual);

                    m_context.timer.Interval = 2000;
                    m_context.timer.Start();
                    m_context.state = GameState.Play;
                    break;
                case GameState.Play:
                    if (m_minigameQueue.Count == 0) throw new MissingMemberException("No minigames available");

                    this.MinigameStateLogic();
                    break;
                case GameState.Results:
                    return false; // don't continue game loop
            }

            return true;
        }

        private void MinigameStateLogic()
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
