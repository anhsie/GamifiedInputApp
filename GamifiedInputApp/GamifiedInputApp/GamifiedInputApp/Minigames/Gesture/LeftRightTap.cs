using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Xaml.Media;
using System;
using System.Numerics;

namespace GamifiedInputApp.Minigames.Gesture
{
    class LeftRightTap : IMinigame
    {
        private ExpPointerInputObserver pointerInputObserver;
        private ExpGestureRecognizer gestureRecognizer;

        private SpriteVisual sprite;
        private ContainerVisual rootVisual;
        private CompositionSurfaceBrush leftArrowBrush;
        private CompositionSurfaceBrush rightArrowBrush;

        // Minigame variables
        private const int TOTAL_TAPS_TO_WIN = 5;
        private int tapCounter;
        private bool tapLeft;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Left/Right Tap", SupportedDeviceTypes.Spatial);

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
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.Tap | Windows.UI.Input.GestureSettings.RightTap;
            gestureRecognizer.Tapped += Tapped;
            gestureRecognizer.RightTapped += RightTapped;
        }

        public MinigameState Update(in GameContext gameContext)
        {
            MinigameState result = MinigameState.Play;

            if (tapCounter >= TOTAL_TAPS_TO_WIN)
            {
                result = MinigameState.Pass;
            }
            else if (gameContext.Timer.Finished)
            {
                result = MinigameState.Fail;
            }

            return result;
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            pointerInputObserver.Dispose();
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
        private void Tapped(object sender, ExpTappedEventArgs eventArgs)
        {
            if (tapLeft)
            {
                ProcessCorrectTap();
            }
        }

        private void RightTapped(object sender, ExpRightTappedEventArgs eventArgs)
        {
            if (!tapLeft)
            {
                ProcessCorrectTap();
            }
        }

        private void ProcessCorrectTap()
        {
            ++tapCounter;
            var previousTapLeft = tapLeft;
            tapLeft = (uint)new Random().Next(0, 2) == 0;
            if (tapLeft && !previousTapLeft)
            {
                sprite.Brush = leftArrowBrush;
            }
            else if (!tapLeft && previousTapLeft)
            {
                sprite.Brush = rightArrowBrush;
            }
        }

        /***** Animation functions *****/

        public void Setup(ContainerVisual rootVisual)
        {
            tapCounter = 0;
            tapLeft = true;

            // Generate visual for tap game.
            this.rootVisual = rootVisual;
            Compositor compositor = rootVisual.Compositor;
            leftArrowBrush = compositor.CreateSurfaceBrush(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/Arrows/leftArrow.png")));
            rightArrowBrush = compositor.CreateSurfaceBrush(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/Arrows/rightArrow.png")));

            sprite = compositor.CreateSpriteVisual();
            sprite.Brush = leftArrowBrush;
            sprite.Size = new Vector2(100, 100);
            sprite.Offset = new Vector3(100, 100, 0);
            rootVisual.Children.InsertAtTop(sprite);
        }

        private void Cleanup()
        {
            this.rootVisual.Children.RemoveAll();
            sprite.Dispose();
        }
    }
}
