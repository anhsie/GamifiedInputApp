
using Microsoft.System;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Hosting.Experimental;
using Microsoft.UI.Input.Experimental; 
using System;
using System.Collections.Generic;
using System.Diagnostics; 
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GamifiedInputApp.Minigames
{
    class GestureRecognizerMinigame : IMinigame
    {
        // Input API
        private ExpPointerInputObserver pointerInputObserver; 
        private ExpGestureRecognizer gestureRecognizer;

        private SpriteVisual sprite;

        // Minigame variables
        private const int TOTAL_TAPS_TO_WIN = 15; 
        private int tapCounter; 

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            return; 
        }

        public void Start(in GameContext gameContext, ContainerVisual rootVisual)
        {
            this.Setup(rootVisual); 
        }

        public MinigameState Update(in GameContext gameContext)
        {
            MinigameState result = MinigameState.Play;

            if (gameContext.Timer.Finished && (tapCounter < TOTAL_TAPS_TO_WIN))
            {
                result = MinigameState.Fail;
            }
            else if (gameContext.Timer.Finished)
            {
                result = MinigameState.Pass; 
            }

            return result; 
        }

        public void Setup(ContainerVisual rootVisual)
        {
            tapCounter = 0;

            // Generate visual for tap game.
            Compositor compositor = rootVisual.Compositor;
            sprite = compositor.CreateSpriteVisual();
            sprite.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xB0, 0xF0));
            sprite.Size = new Vector2(100, 100);

            rootVisual.Children.InsertAtTop(sprite);

            // Create InputSite
            var content = ExpCompositionContent.Create(compositor);

            var process = Process.GetCurrentProcess();
            var handle = process.MainWindowHandle;
            

            //var bridge = ExpCoreWindowBridge.Create(compositor);
            var inputsite = ExpInputSite.GetOrCreateForContent(content);
            //bridge.Connect(content, inputsite); 

            // PointerInputObserver
            pointerInputObserver = ExpPointerInputObserver.CreateForInputSite(inputsite);
            pointerInputObserver.PointerPressed += OnPointerPressed;
            pointerInputObserver.PointerReleased += OnPointerReleased;

            // GestureRecognizer
            gestureRecognizer = new ExpGestureRecognizer();
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.Tap;
            gestureRecognizer.Tapped += Tapped;
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
            ++tapCounter;  
        }
    }
}
