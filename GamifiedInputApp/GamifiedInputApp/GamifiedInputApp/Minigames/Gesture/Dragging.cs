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
        private const float SPRITE_SPEED = 0.05f;

        // Input API
        private ExpIndependentPointerInputObserver pointerInputObserver;
        private ExpGestureRecognizer gestureRecognizer;

        // Game variables
        private SpriteVisual ball;
        private SpriteVisual hoop;
        private ContainerVisual rootVisual;
        private const float HOOP_Y_OFFSET = 250;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Dragging", SupportedDeviceTypes.Spatial);

        public void Start(in GameContext gameContext)
        {
            this.Setup(gameContext.Content.RootVisual); // Setup game board

            // PointerInputObserver
            pointerInputObserver = ExpIndependentPointerInputObserver.CreateForVisual(
                hoop,
                Windows.UI.Core.CoreInputDeviceTypes.Mouse);

            pointerInputObserver.PointerPressed += OnPointerPressed;
            pointerInputObserver.PointerReleased += OnPointerReleased;
            pointerInputObserver.PointerMoved += OnPointerMoved; 

            // GestureRecognizer
            gestureRecognizer = new ExpGestureRecognizer();
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.Drag;
            gestureRecognizer.Dragging += Drag;
        }

        public MinigameState Update(in GameContext gameContext)
        {
            this.Animate(gameContext); // Animate game board

            // Do update logic for minigame
            if (ball.Offset.Y < HOOP_Y_OFFSET)
            {
                return MinigameState.Play;
            } else if (BallInHoop())
            {
                return MinigameState.Pass;
            } else
            {
                return MinigameState.Fail;
            }
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

        private void OnPointerMoved(object sender, ExpPointerEventArgs args)
        {
            var pointerPoint = args.CurrentPoint;

            if (pointerPoint.IsInContact)
            {
                var points = args.GetIntermediatePoints(); 
                gestureRecognizer.ProcessMoveEvents(points);
            } 
        }

        // GestureRecognizer
        private void Drag(object sender, ExpDraggingEventArgs eventArgs)
        {
            Vector3 offset = hoop.Offset;
            offset.X = Convert.ToSingle(eventArgs.Position.X);
            hoop.Offset = offset;
        }

        /***** Animation functions *****/

        private void Animate(in GameContext gameContext)
        {
            // Animate things here
            float dt = (float)gameContext.Timer.DeltaTime.TotalMilliseconds;

            Vector3 offset = ball.Offset;
            offset.Y += (dt * SPRITE_SPEED);
            ball.Offset = offset;
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


            var rand = new Random().Next(0,250); 

            this.ball = comp.CreateSpriteVisual();
            this.ball.Brush = ballBrush;
            this.ball.Size = new Vector2(30, 30);
            this.ball.Offset = new Vector3(rand, 0, 0);


            this.hoop = comp.CreateSpriteVisual();
            this.hoop.Brush = hoopBrush;
            this.hoop.Size = new Vector2(100, 100);
            this.hoop.Offset = new Vector3(100, HOOP_Y_OFFSET, 0);

            rootVisual.Children.InsertAtTop(ball);
            rootVisual.Children.InsertAtTop(hoop);
        }

        
        private void Cleanup()
        {
            this.ball.Dispose(); 
            this.hoop.Dispose();
            rootVisual.Children.RemoveAll();
        }

        private bool BallInHoop()
        {
            if((2 * ball.Offset.X + ball.Size.X > 2* hoop.Offset.X) && (2 * ball.Offset.X + ball.Size.X < 2*hoop.Offset.X + 2*hoop.Size.X))
            {
                return true;
            }
            return false;
        }
    }
}
