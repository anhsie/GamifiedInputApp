using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Hosting.Experimental;
using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace GamifiedInputApp.Minigames.Gesture
{
    class Tap : IMinigame
    {
        // UI Components
        private ContainerVisual rootVisual; 
        private VisualCollection sprites; // TODO: Delete this its not being used

        private CompositionSurfaceBrush ship;
        private CompositionSurfaceBrush shipWithAlien; 

        // Input API
        private ExpPointerInputObserver pointerInputObserver; 
        private ExpGestureRecognizer gestureRecognizer;

        // Minigame variables
        private const int TOTAL_TAPS_TO_WIN = 10; 
        private int tapCounter;
        MinigameState state; 

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Tap", SupportedDeviceTypes.Spatial);

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
            // TODO: Every 3 or 5 seconds spawn an alien in new location

            if (gameContext.Timer.Finished && (tapCounter < TOTAL_TAPS_TO_WIN))
            {
                this.state = MinigameState.Fail;
            }
            else if (gameContext.Timer.Finished)
            {
                this.state = MinigameState.Pass; 
            }

            return state; 
        }

        // 
        // Helper Functions
        //

        private void Setup(ContainerVisual rootVisual)
        {
            tapCounter = 0;
            this.rootVisual = rootVisual; 

            this.SetupUI();
            this.SetupInputAPI();
        }

        private void SetupUI()
        {
            this.state = MinigameState.Play;

            var shipImg = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/Alien/ShipGreen.png"));
            var shipWithAlienImg = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/Alien/ShipGreen_manned.png")); 
            
            Compositor comp = this.rootVisual.Compositor;

            this.ship = comp.CreateSurfaceBrush();
            this.ship.Surface = shipImg;

            this.shipWithAlien = comp.CreateSurfaceBrush();
            this.shipWithAlien.Surface = shipWithAlienImg; 

            int size = 100; 
            int x = 100;
            int y = 100;

            for (int i = 1; i < 10; i++)
            {
                SpriteVisual sprite = comp.CreateSpriteVisual();
                sprite.Size = new Vector2(size, size);
                sprite.Offset = new Vector3(x, y, 0);
                sprite.Brush = this.ship;
                this.rootVisual.Children.InsertAtTop(sprite);

                if ((i % 3) == 0)
                {
                    x += 100;
                    y = 100;
                }
                else { y += 100; }
            }

            this.SpawnAlien();
        }

        private void SetupInputAPI()
        {
            var compositor = new Compositor(); 
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

        private void SpawnAlien()
        {
            var rand = new Random().Next(1, 10);
            SpriteVisual spriteVisual = (SpriteVisual) this.rootVisual.Children.ElementAt(rand);
            spriteVisual.Brush = this.shipWithAlien;
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
            this.SpawnAlien();  
            ++tapCounter;

            if (tapCounter == TOTAL_TAPS_TO_WIN)
            {
                state = MinigameState.Pass; 
            }
        }
    }
}
