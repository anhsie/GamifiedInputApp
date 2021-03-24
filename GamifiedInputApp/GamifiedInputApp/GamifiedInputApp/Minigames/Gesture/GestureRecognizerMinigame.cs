using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Core; 
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Hosting.Experimental;
using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics; 
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GamifiedInputApp.Minigames.Gesture
{
    class GestureRecognizerMinigame : IMinigame
    {
        private Window window;
        private ContainerVisual root; 
        private SpriteVisual sprite; 

        // Input API
        private ExpPointerInputObserver pointerInputObserver; 
        private ExpGestureRecognizer gestureRecognizer;

        // Minigame variables
        private const int TOTAL_TAPS_TO_WIN = 15; 
        private int tapCounter;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "GestureRecognizer", SupportedDeviceTypes.Spatial);

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            this.window.Close(); 
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
            window = new Window();
            window.Activate();  

            tapCounter = 0;

            // Generate visual for tap game.
            Compositor compositor = this.window.Compositor;
            UIElement element = new Grid(); 

            root = compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(element, root); 

            sprite = compositor.CreateSpriteVisual();
            sprite.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xB0, 0xF0));
            sprite.Size = new Vector2(100, 100);

            root.Children.InsertAtTop(sprite);

            var process = Process.GetCurrentProcess();
            var handle = process.MainWindowHandle;

            Microsoft.UI.WindowId windowId;
            WinUITryGetWindowIdFromWindow(handle, out windowId);

            // Create InputSite
            var content = ExpCompositionContent.Create(compositor);
            var bridge = ExpDesktopWindowBridge.Create(compositor, windowId); 

            var inputsite = ExpInputSite.GetOrCreateForContent(content);
            bridge.Connect(content, inputsite); 

            // PointerInputObserver
            pointerInputObserver = ExpPointerInputObserver.CreateForInputSite(inputsite);
            pointerInputObserver.PointerPressed += OnPointerPressed;
            pointerInputObserver.PointerReleased += OnPointerReleased;

            // GestureRecognizer
            gestureRecognizer = new ExpGestureRecognizer();
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.Tap;
            gestureRecognizer.Tapped += Tapped;

            //rootVisual.Children.InsertAtTop(sprite);
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

        [DllImport("Microsoft.Internal.FrameworkUdk.dll", EntryPoint = "WinUITryGetWindowIdFromWindow")]
        static extern int WinUITryGetWindowIdFromWindow(IntPtr hWnd, out Microsoft.UI.WindowId windowId);
    }
}
