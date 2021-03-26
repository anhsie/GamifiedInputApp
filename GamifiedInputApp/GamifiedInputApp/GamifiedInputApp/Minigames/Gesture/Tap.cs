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
    class DoubleTap : IMinigame
    {
        // UI Components
        private ContainerVisual rootVisual;
        private SpriteVisual sprite;

        private CompositionSurfaceBrush ship;
        private CompositionSurfaceBrush shipWithAlien;


        // Input API
        private ExpIndependentPointerInputObserver pointerInputObserver; 
        private ExpGestureRecognizer gestureRecognizer;

        // Minigame variables
        private const int TOTAL_TAPS_TO_WIN = 2; 
        private int tapCounter;
        MinigameState state;

        private int? currentAlienIndex; 

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "DoubleTap", SupportedDeviceTypes.Spatial);

        public void Start(in GameContext gameContext)
        {
            this.Setup(gameContext.Content.RootVisual);
            this.SpawnAlien();

            // GestureRecognizer
            this.gestureRecognizer = new ExpGestureRecognizer();
            this.gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.DoubleTap;
            this.gestureRecognizer.Tapped += Tapped;
        }

        public MinigameState Update(in GameContext gameContext)
        {
            // TODO: Every 3 or 5 seconds spawn an alien in new location

            if (this.state != MinigameState.Play) { return state; }

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
            if (gestureRecognizer != null)
            {
                gestureRecognizer.ProcessDownEvent(args.CurrentPoint);
            }
        }

        private void OnPointerReleased(object sender, ExpPointerEventArgs args)
        {
            if (gestureRecognizer != null)
            {
                gestureRecognizer.ProcessUpEvent(args.CurrentPoint);
            }
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

        /***** Animation functions *****/

        private void Setup(ContainerVisual rootVisual)
        {
            this.rootVisual = rootVisual;
            this.state = MinigameState.Play;
            tapCounter = 0;

            var shipImg = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/Alien/ShipGreen.png"));
            var shipWithAlienImg = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/Alien/ShipGreen_manned.png"));

            Compositor comp = this.rootVisual.Compositor;

            this.ship = comp.CreateSurfaceBrush();
            this.ship.Surface = shipImg;

            this.shipWithAlien = comp.CreateSurfaceBrush();
            this.shipWithAlien.Surface = shipWithAlienImg;

            int size = 100;
            int x = 50;
            int y = 50;

            for (int i = 1; i < 10; i++)
            {
                SpriteVisual sprite = comp.CreateSpriteVisual();
                sprite.Size = new Vector2(size, size);
                sprite.Offset = new Vector3(x, y, 0);
                sprite.Brush = this.ship;
                this.rootVisual.Children.InsertAtTop(sprite);

                if ((i % 3) == 0)
                {
                    y += 100;
                    x = 50;
                }
                else { x += 100; }
            }
        }

        private void Cleanup()
        {
            this.rootVisual.Children.RemoveAll();
            this.ship.Dispose();
            this.shipWithAlien.Dispose();
        }

        private void SpawnAlien()
        {
            var rand = new Random().Next(1, 10);

            if (currentAlienIndex != null)
            {
                // Undo alien from current location
                SpriteVisual undoVisual = (SpriteVisual)this.rootVisual.Children.ElementAt((int)this.currentAlienIndex);
                undoVisual.Brush = this.ship;
            }

            // Draw alien in new location. 
            sprite = (SpriteVisual) this.rootVisual.Children.ElementAt(rand);
            sprite.Brush = this.shipWithAlien;

            this.pointerInputObserver = ExpIndependentPointerInputObserver.CreateForVisual(
                sprite,
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Touch |
                Windows.UI.Core.CoreInputDeviceTypes.Pen);
            this.pointerInputObserver.PointerPressed += OnPointerPressed;
            this.pointerInputObserver.PointerReleased += OnPointerReleased;

            this.currentAlienIndex = rand;
        }
    }
}
