
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

namespace GamifiedInputApp.Minigames
{
    class GestureRecognizerMinigameHolding : IMinigame
    {
        // Input API
        private ExpPointerInputObserver pointerInputObserver; 
        private ExpGestureRecognizer gestureRecognizer;

        private SpriteVisual sprite;

        // Minigame variables
        private const int TOTAL_TAPS_TO_WIN = 15; 
        private int tapCounter;
        private bool tapLeft;
        
        MinigameInfo IMinigame.Info => new MinigameInfo(this, "GestureRecognizer", SupportedDeviceTypes.Spatial);

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            return; 
        }

        public void Start(in GameContext gameContext, ContainerVisual rootVisual, ExpInputSite inputSite)
        {
            this.Setup(rootVisual); 
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

        public void Setup(ContainerVisual rootVisual)
        {
            tapCounter = 0;
            tapLeft = true;

            // Generate visual for tap game.
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
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.Tap | Windows.UI.Input.GestureSettings.RightTap;
            gestureRecognizer.Tapped += Tapped;
            gestureRecognizer.RightTapped += RightTapped;

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
        private void Tapped(object sender, ExpTappedEventArgs eventArgs)
        {
            if (tapLeft)
            {
                //tapLeft = randomized true or false. Also change spriteVisual to indicate which direction
                ++tapCounter;
            }
        }

        private void RightTapped(object sender, ExpRightTappedEventArgs eventArgs)
        {
            if (!tapLeft)
            {
                //tapLeft = randomized true or false. Also change spriteVisual to indicate which direction
                ++tapCounter;
            }
        }
    }
}
