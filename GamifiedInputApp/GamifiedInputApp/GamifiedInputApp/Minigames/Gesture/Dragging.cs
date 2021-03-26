using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Xaml.Media;
using System;
using System.Numerics;

namespace GamifiedInputApp.Minigames.Gesture
{
    class Dragging : IMinigame
    {
        private const float SPRITE_SPEED = 1.0f;

        // Input API
        private ExpIndependentPointerInputObserver pointerInputObserver;
        private ExpGestureRecognizer gestureRecognizer;

        // Game variables
        private SpriteVisual ball;
        private SpriteVisual hoop;
        private ContainerVisual rootVisual;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Dragging", SupportedDeviceTypes.Spatial);

        public void Start(in GameContext gameContext, ContainerVisual rootVisual, ExpInputSite inputSite)
        {
            this.Setup(rootVisual); // Setup game board

            // Do start logic for minigame
        }

        public MinigameState Update(in GameContext gameContext)
        {
            //this.Animate(gameContext); // Animate game board

            // Do update logic for minigame
            // If ball has not hit y coordinate equal to the basket, play
            // If ball has hit y coordinate and ball's centerpoint location is in between basket offset and basket offset + width, then pass
            // else fail. 

            return gameContext.Timer.Finished ? MinigameState.Pass : MinigameState.Play; // Return new state (auto pass here)
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            this.Cleanup(); // Cleanup game board

            // Do cleanup logic for minigame
        }


        private void Setup(ContainerVisual rootVisual)
        {
            this.rootVisual = rootVisual;

            var ballImg = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/Basketball/ball.png"));
            var hoopImg = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/Basketball/hoop.png"));

            Compositor comp = this.rootVisual.Compositor;

            var ballBrush = comp.CreateSurfaceBrush();
            ballBrush.Surface = ballImg;

            var hoopBrush = comp.CreateSurfaceBrush();
            hoopBrush.Surface = hoopImg;

            this.ball = comp.CreateSpriteVisual();
            this.ball.Brush = ballBrush;
            this.ball.Size = new Vector2(50, 50);
            this.ball.Offset = new Vector3(100, 0, 0); 


            this.hoop = comp.CreateSpriteVisual();
            this.hoop.Brush = hoopBrush;
            this.hoop.Size = new Vector2(100, 100);
            this.hoop.Offset = new Vector3(100, 500, 0);


            // PointerInputObserver
            pointerInputObserver = ExpIndependentPointerInputObserver.CreateForVisual(
                hoop,
                Windows.UI.Core.CoreInputDeviceTypes.Mouse);

            pointerInputObserver.PointerPressed += OnPointerPressed;
            pointerInputObserver.PointerReleased += OnPointerReleased;

            // GestureRecognizer
            gestureRecognizer = new ExpGestureRecognizer();
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.Drag;
            gestureRecognizer.Dragging += Drag;

            rootVisual.Children.InsertAtTop(ball);
            rootVisual.Children.InsertAtTop(hoop);

        }

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
        private void Drag(object sender, ExpDraggingEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        private void Animate(in GameContext gameContext)
        {
            // Animate things here
            //float dt = (float)gameContext.Timer.DeltaTime;

            //Vector3 offset = m_ball.Offset;
            //offset.Y += (dt * SPRITE_SPEED);
            //m_ball.Offset = offset;
        }
        
        private void Cleanup()
        {
            this.ball.Dispose(); 
            this.hoop.Dispose();
            pointerInputObserver = null;
            gestureRecognizer = null;
            rootVisual.Children.RemoveAll();
        }
    }
}
