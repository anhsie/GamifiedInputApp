﻿using System;
using System.Collections.Generic;

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
        public static List<IMinigame> Minigames { get; private set; }
        static GameCore()
        {
            Minigames = new List<IMinigame>();
            Minigames.Add(new DummyMinigame());
        }

        private GameContext m_context;
        private Queue<IMinigame> m_minigameQueue;

        public GameCore()
        {
            m_context.state = GameState.Start;
            m_context.timer = new GameTimer();
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
            while (this.GameLoop()) continue;

            // cleanup code here
            // goto results screen
        }

        protected bool GameLoop()
        {
            switch (m_context.state)
            {
                case GameState.Start:
                    if (m_minigameQueue.Count == 0) throw new MissingMemberException("No minigames selected");

                    IMinigame current = m_minigameQueue.Peek();
                    current.Start(m_context, null /* todo: root visual */);

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
