using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GamifiedInputApp.Minigames.Gesture
{
    class GestureRecognizerMinigame : IMinigame
    {
        // UI Components
        private Window window;
        private ContainerVisual rootVisual; 
        private SpriteVisual sprite; // TODO: Might need a list of these ?

        // Input API
        private ExpPointerInputObserver pointerInputObserver; 
        private ExpGestureRecognizer gestureRecognizer;

        // Minigame variables
        private const int TOTAL_TAPS_TO_WIN = 15; 
        private int tapCounter;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "GestureRecognizer", SupportedDeviceTypes.Spatial);

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            window.Close();
        }

        public void Start(in GameContext gameContext, ContainerVisual rootVisual, ExpInputSite inputSite)
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

        // 
        // Helper Functions
        //

        private void Setup(ContainerVisual rootVisual)
        {
            tapCounter = 0;

            this.UISetup();
            this.InputAPISetup(); 
        }

        private void UISetup()
        {
            window = new Window();

            var compositor = window.Compositor;
            rootVisual = compositor.CreateContainerVisual();

            UIElement rootElement = new Grid();
            ElementCompositionPreview.SetElementChildVisual(rootElement, rootVisual);

            sprite = compositor.CreateSpriteVisual();
            sprite.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xB0, 0xF0));
            sprite.Size = new Vector2(100, 100);

            rootVisual.Children.InsertAtTop(sprite);
            window.Activate(); 
        }

        private void InputAPISetup()
        {
            var compositor = sprite.Compositor; 

            // Create InputSite
            var content = ExpCompositionContent.Create(compositor);
            var inputsite = ExpInputSite.GetOrCreateForContent(content);

            // PointerInputObserver
            pointerInputObserver = ExpPointerInputObserver.CreateForInputSite(inputsite);
            pointerInputObserver.PointerPressed += OnPointerPressed;
            pointerInputObserver.PointerReleased += OnPointerReleased;

            // GestureRecognizer
            gestureRecognizer = new ExpGestureRecognizer();
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.Tap;
            gestureRecognizer.Tapped += Tapped;
        }
    }
}
