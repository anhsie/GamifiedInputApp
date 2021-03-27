﻿using System;
using System.Collections.Generic;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;

using GamifiedInputApp.Minigames;
using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Hosting.Experimental;
using Microsoft.UI.Composition.Experimental;
using System.Media;
using System.IO;
using Windows.Media.Playback;
using Windows.Media.Core;
using System.Collections.ObjectModel;
using Microsoft.System;

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

        public ContentHelper Content; // game content
        public NativeWindowHelper Window; // window game is running in

        public int Cleared; // minigames cleared
        public int Score; // total score
        public TimeSpan? Fastest; // fastest clear
    }

    public class ResultsEventArgs
    {
        public const string TimeFormat = @"ss\.ff";

        public ResultsEventArgs(GameContext context)
        {
            TimeLeft = context.Timer.TimeRemaining.ToString(TimeFormat);
            GoToResults = context.State == GameState.Results;
            Results = (context.State == GameState.Play) ? null : new()
            {
                new ScoreItem() { Title = "Cleared", Value = context.Cleared.ToString() },
                new ScoreItem() { Title = "Score", Value = context.Score.ToString() },
                new ScoreItem() { Title = "Fastest", Value = context.Fastest?.ToString(TimeFormat) }
            };
        }

        public Collection<ScoreItem> Results { get; }
        public string TimeLeft { get; }
        public bool GoToResults { get; }
    }

    public class GameCore
    {
        private const double MAX_FPS = 60;

        private static readonly TimeSpan MAX_TIME = TimeSpan.FromMilliseconds(5000);
        private static readonly TimeSpan MIN_TIME = TimeSpan.FromMilliseconds(3000);
        private static readonly TimeSpan TIME_STEP = TimeSpan.FromMilliseconds(250);

        public delegate void ResultsEventHandler(object sender, ResultsEventArgs e);
        public event ResultsEventHandler Results;

        private GameContext m_context;
        private ExpDesktopWindowBridge m_desktopBridge;
        private MainWindow m_mainWindow;
        private Compositor m_compositor;
        private Queue<IMinigame> m_minigameQueue;
        private IMinigame m_currentMinigame;
        private DispatcherQueueTimer m_loopTimer;
        private NativeWindowHelper m_nativeWindow;

        private MediaPlayer m_successMediaPlayer;
        private MediaPlayer m_failureMediaPlayer;

        public bool IsRunning { get; private set; }

        public GameCore(MainWindow mainWindow)
        {
            m_context.State = GameState.Start;
            m_context.Timer = new GameTimer();
            m_context.Timer.StepFrames = true;

            m_mainWindow = mainWindow;
            m_compositor = m_mainWindow.RootVisual.Compositor;

            m_loopTimer = m_mainWindow.DispatcherQueue.CreateTimer();
            m_loopTimer.Interval = TimeSpan.FromSeconds(1.0 / MAX_FPS);
            m_loopTimer.IsRepeating = true;
            m_loopTimer.Tick += GameLoop;

            m_successMediaPlayer = new MediaPlayer();
            m_successMediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Audio/success.wav"));

            m_failureMediaPlayer = new MediaPlayer();
            m_failureMediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Audio/failure.wav"));
        }

        public void Run(IEnumerable<MinigameInfo> minigames)
        {
            if (IsRunning) { return; }

            if (m_nativeWindow == null)
            {
                m_nativeWindow = new NativeWindowHelper(m_mainWindow.GameBounds, m_mainWindow.Handle);
            }

            m_nativeWindow.Show();

            // setup code here
            m_minigameQueue = new Queue<IMinigame>();
            foreach (MinigameInfo info in minigames)
            {
                m_minigameQueue.Enqueue(info.Minigame);
                Console.WriteLine("Queueing minigame: " + info.Name);
            }
            if (m_minigameQueue.Count == 0) { throw new InvalidOperationException("No miningames selected"); }

            // reset values
            m_context.Cleared = 0;
            m_context.Score = 0;
            m_context.Fastest = null;
            m_context.Window = m_nativeWindow;

            // start game
            m_context.State = GameState.Start;
            IsRunning = true;
            m_loopTimer.Start();
        }

        private double GetInterval()
        {
            TimeSpan interval = MAX_TIME - (m_context.Cleared * TIME_STEP);
            return Math.Max(interval.TotalMilliseconds, MIN_TIME.TotalMilliseconds);
        }

        protected void GameLoop(Object source, object e)
        {
            if (!IsRunning) { return; }

            // send results containing updated time remaining,
            // and also score if the state is no longer Play
            Results?.Invoke(this, new ResultsEventArgs(m_context));

            switch (m_context.State)
            {
                case GameState.Start:
                    if (m_minigameQueue.Count == 0) throw new MissingMemberException("No minigames available");

                    // setup minigame
                    m_currentMinigame = m_minigameQueue.Dequeue();

                    // Create a new desktop bridge every time, because of a crash when connecting with a bridge with existing content
                    m_desktopBridge = ExpDesktopWindowBridge.Create(m_compositor, m_nativeWindow.WindowId);
                    PInvoke.User32.SetWindowPos(
                        NativeWindowHelper.GetHwndFromWindowId(m_desktopBridge.ChildWindowId),
                        IntPtr.Zero,
                        0,
                        0,
                        m_nativeWindow.Width,
                        m_nativeWindow.Height,
                        PInvoke.User32.SetWindowPosFlags.SWP_NOACTIVATE |
                            PInvoke.User32.SetWindowPosFlags.SWP_NOOWNERZORDER |
                            PInvoke.User32.SetWindowPosFlags.SWP_NOZORDER
                        );
                    PInvoke.User32.ShowWindow(
                        NativeWindowHelper.GetHwndFromWindowId(m_desktopBridge.ChildWindowId),
                        PInvoke.User32.WindowShowStyle.SW_SHOW);
                    
                    // create new content object and place it into the desktop window bridge
                    m_context.Content = new ContentHelper(m_compositor);
                    m_desktopBridge.Connect(m_context.Content.Content, m_context.Content.InputSite);
                    m_currentMinigame.Start(m_context);

                    // start timer
                    m_context.Timer.Interval = GetInterval();
                    m_context.Timer.Start();
                    m_context.State = GameState.Play;
                    break;
                case GameState.Play:
                    // update minigame
                    this.UpdateMinigame();
                    break;
                case GameState.Results:
                    // stop timer
                    m_loopTimer.Stop();
                    IsRunning = false;
                    m_nativeWindow.Hide();
                    m_desktopBridge?.Dispose();
                    m_desktopBridge = null;
                    break;
            }
        }

        private void UpdateMinigame()
        {
            m_context.Timer.NextFrame();
            MinigameState state = m_currentMinigame.Update(m_context);

            if (state == MinigameState.Play && m_context.Timer.Finished)
            {
                state = MinigameState.Fail; // fail when out of time
            }

            if (state != MinigameState.Play)
            {
                m_currentMinigame.End(m_context, state);
                m_currentMinigame = null;

                if (state == MinigameState.Pass)
                {
                    TimeSpan elapsed = m_context.Timer.TimeElapsed;

                    m_context.Cleared++;
                    // note: TimeRemaining would be bias as things speed up, so we use TimeElapsed for score
                    m_context.Score += Math.Max(100, (int)(MAX_TIME - elapsed).TotalMilliseconds);
                    // note: Fastest is nullable, so it may not have a value. Use MAX_TIME to compare in that case
                    if (elapsed < m_context.Fastest.GetValueOrDefault(MAX_TIME)) { m_context.Fastest = elapsed; }

                    m_successMediaPlayer.Play();
                    m_context.State = (m_minigameQueue.Count > 0) ? GameState.Start : GameState.Results;
                }
                else //if (state == MinigameState.Fail)
                {
                    m_failureMediaPlayer.Play();
                    m_context.State = GameState.Results;
                }
            }
        }
    }
}
