﻿
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Input.Experimental; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GamifiedInputApp.Minigames.Gesture
{
    class Holding : IMinigame
    {
        // Input API
        private ExpPointerInputObserver pointerInputObserver; 
        private ExpGestureRecognizer gestureRecognizer;


        // Minigame variables
        private SpriteVisual sprite;
        private ContainerVisual rootVisual;
        private System.Diagnostics.Stopwatch stopwatch;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Holding", SupportedDeviceTypes.Spatial);

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            this.Cleanup(); // Cleanup game board
            return;
        }

        private void Cleanup()
        {
            pointerInputObserver = null;
            gestureRecognizer = null;
            rootVisual.Children.RemoveAll();
            sprite = null;
            stopwatch = null;

        }

        public void Start(in GameContext gameContext, ContainerVisual rootVisual, ExpInputSite inputSite)
        {
            this.Setup(rootVisual); 
        }

        public MinigameState Update(in GameContext gameContext)
        {
            Animate(gameContext);
            MinigameState result = MinigameState.Play;

            if (Math.Abs(stopwatch.ElapsedMilliseconds - 1000) < 100)
            {
                result = MinigameState.Pass;
            }
            else if (gameContext.Timer.Finished)
            {
                result = MinigameState.Fail;
            }

            return result; 
        }

        private void Animate(in GameContext gameContext)
        {
            float dt = (float)gameContext.Timer.DeltaTime;
            throw new NotImplementedException();
        }

        public void Setup(ContainerVisual rootVisual)
        {
            // Setup timer
            stopwatch = new System.Diagnostics.Stopwatch();

            // Generate visual for tap game.
            this.rootVisual = rootVisual;
            Compositor compositor = rootVisual.Compositor;
            sprite = compositor.CreateSpriteVisual();
            sprite.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xB0, 0xF0));
            sprite.Size = new Vector2(100, 100);

            // Create InputSite
            var content = ExpCompositionContent.Create(compositor);
            content.Root = sprite;
            var inputsite = ExpInputSite.GetOrCreateForContent(content);

            // PointerInputObserver
            pointerInputObserver = ExpPointerInputObserver.CreateForInputSite(inputsite);
            pointerInputObserver.PointerPressed += OnPointerPressed;
            pointerInputObserver.PointerReleased += OnPointerReleased;

            // GestureRecognizer
            gestureRecognizer = new ExpGestureRecognizer();
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.Hold;
            gestureRecognizer.Holding += Holding;

            rootVisual.Children.InsertAtTop(sprite);
        }

        //
        // Event Handlers 
        //

        // PointerInputObserver
        private void OnPointerPressed(object sender, ExpPointerEventArgs args)
        {
            gestureRecognizer.ProcessDownEvent(args.CurrentPoint); 
        }

        private void OnPointerReleased(object sender, ExpPointerEventArgs args)
        {
            gestureRecognizer.ProcessUpEvent(args.CurrentPoint);
        }

        // GestureRecognizer
        private void Holding(object sender, ExpHoldingEventArgs eventArgs)
        {
            switch (eventArgs.HoldingState)
            {
                case Windows.UI.Input.HoldingState.Started:
                    stopwatch.Start();
                    return;
                case Windows.UI.Input.HoldingState.Completed:
                    stopwatch.Stop();
                    return;
                case Windows.UI.Input.HoldingState.Canceled:
                    stopwatch.Stop();
                    return;
                default:
                    return;
            }
        }
    }
}