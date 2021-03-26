using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Input.Experimental;
using System;
using System.Numerics;

namespace GamifiedInputApp.Minigames.Gesture
{
    class Dragging : IMinigame
    {
        private const float SPRITE_SPEED = 1.0f;

        // Input API
        private ExpPointerInputObserver pointerInputObserver;
        private ExpGestureRecognizer gestureRecognizer;

        // Game variables
        private SpriteVisual m_ball;
        private SpriteVisual m_basket;
        private ContainerVisual rootVisual;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Dragging", SupportedDeviceTypes.Spatial);

        public void Start(in GameContext gameContext, ContainerVisual rootVisual, ExpInputSite inputSite)
        {
            this.Setup(rootVisual); // Setup game board

            // Do start logic for minigame
        }

        public MinigameState Update(in GameContext gameContext)
        {
            this.Animate(gameContext); // Animate game board

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
            // Setup game board here
            Compositor compositor = rootVisual.Compositor;
            m_ball = compositor.CreateSpriteVisual();
            m_ball.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xB0, 0xF0));
            m_basket = compositor.CreateSpriteVisual();
            m_basket.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xF0, 0xB0, 0x00));

            var content = ExpCompositionContent.Create(compositor);
            content.Root = m_basket;
            var inputsite = ExpInputSite.GetOrCreateForContent(content);

            // PointerInputObserver
            pointerInputObserver = ExpPointerInputObserver.CreateForInputSite(inputsite);
            pointerInputObserver.PointerPressed += OnPointerPressed;
            pointerInputObserver.PointerReleased += OnPointerReleased;

            // GestureRecognizer
            gestureRecognizer = new ExpGestureRecognizer();
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.Drag;
            gestureRecognizer.Dragging += Drag;

            rootVisual.Children.InsertAtTop(m_ball);
            rootVisual.Children.InsertAtTop(m_basket);

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
            float dt = (float)gameContext.Timer.DeltaTime.TotalMilliseconds;

            Vector3 offset = m_ball.Offset;
            offset.Y += (dt * SPRITE_SPEED);
            m_ball.Offset = offset;
        }
        
        private void Cleanup()
        {
            m_ball = null;
            m_basket = null;
            pointerInputObserver = null;
            gestureRecognizer = null;
            rootVisual.Children.RemoveAll();
        }
    }
}
