
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
        private CompositionColorBrush brushStopped;
        private CompositionColorBrush brushStarted;
        private CompositionColorBrush brushSuccess;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Holding", SupportedDeviceTypes.Spatial);

        public void Start(in GameContext gameContext)
        {
            this.Setup(gameContext.Content.RootVisual);

            // Create InputSite
            this.pointerInputObserver = ExpIndependentPointerInputObserver.CreateForVisual(
                sprite,
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Touch |
                Windows.UI.Core.CoreInputDeviceTypes.Pen);

            // PointerInputObserver
            pointerInputObserver.PointerPressed += OnPointerPressed;
            pointerInputObserver.PointerReleased += OnPointerReleased;

            // GestureRecognizer
            gestureRecognizer = new ExpGestureRecognizer();
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.Hold;
            gestureRecognizer.Holding += HoldingEventHandler;
        }

        public MinigameState Update(in GameContext gameContext)
        {
            //Animate(gameContext);
            MinigameState result = MinigameState.Play;

            if (gameContext.Timer.Finished && Math.Abs(stopwatch.ElapsedMilliseconds - 1000) < 100)
            {
                sprite.Brush = brushSuccess;
                result = MinigameState.Pass;
            }
            else if (gameContext.Timer.Finished)
            {
                result = MinigameState.Fail;
            } else if (Math.Abs(stopwatch.ElapsedMilliseconds - 1000) < 100)
            {
                sprite.Brush = brushSuccess;
            }

            return result; 
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            pointerInputObserver = null;
            gestureRecognizer = null;

            this.Cleanup(); // Cleanup game board
        }

        /******* Event functions *******/

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
        private void HoldingEventHandler(object sender, ExpHoldingEventArgs eventArgs)
        {
            switch (eventArgs.HoldingState)
            {
                case Windows.UI.Input.HoldingState.Started:
                    stopwatch.Reset();
                    stopwatch.Start();
                    sprite.Brush = brushStarted;
                    return;
                case Windows.UI.Input.HoldingState.Completed:
                    stopwatch.Stop();
                    sprite.Brush = brushStopped;
                    return;
                case Windows.UI.Input.HoldingState.Canceled:
                    stopwatch.Stop();
                    stopwatch.Reset();
                    sprite.Brush = brushStopped;
                    return;
                default:
                    return;
            }
        }

        /***** Animation functions *****/

        public void Setup(ContainerVisual rootVisual)
        {
            // Setup timer
            stopwatch = new System.Diagnostics.Stopwatch();

            // Generate visual for tap game.
            this.rootVisual = rootVisual;
            Compositor compositor = rootVisual.Compositor;
            sprite = compositor.CreateSpriteVisual();
            brushStarted = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xB0, 0xF0));
            brushStopped= compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xB0, 0xB0, 0xF0));
            brushSuccess = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xB0, 0xB0, 0x00));
            sprite.Brush = brushStopped;
            sprite.Size = new Vector2(100, 100);
            sprite.Offset = new Vector3(100, 100, 0);
            rootVisual.Children.InsertAtTop(sprite);
        }

        private void Cleanup()
        {
            rootVisual.Children.RemoveAll();
            sprite = null;
            stopwatch = null;
        }

        private void Animate(in GameContext gameContext)
        {
            float dt = (float)gameContext.Timer.DeltaTime.TotalMilliseconds;
        }
    }
}
