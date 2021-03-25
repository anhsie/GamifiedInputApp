using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Input.Experimental;
using System;
using System.Numerics;

namespace GamifiedInputApp.Minigames
{
    class GestureRecognizerMinigameDoubleTap : IMinigame
    {
        private const float SPRITE_SPEED = 1.0f;

        // Input API
        private ExpPointerInputObserver pointerInputObserver;
        private ExpGestureRecognizer gestureRecognizer;

        // Minigame variables
        private SpriteVisual m_sprite;
        private int tapCounter;
        private const int TOTAL_TAPS_TO_WIN = 5;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "GestureRecognizer", SupportedDeviceTypes.Spatial);

        public void Start(in GameContext gameContext, ContainerVisual rootVisual, ExpInputSite inputSite)
        {
            this.Setup(rootVisual); // Setup game board

            // Do start logic for minigame
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
            this.Cleanup(); // Cleanup game board

            // Do cleanup logic for minigame
        }


        private void Setup(ContainerVisual rootVisual)
        {
            // Setup game board here
            tapCounter = 0;

            // Generate visual for tap game.
            Compositor compositor = rootVisual.Compositor;
            m_sprite = compositor.CreateSpriteVisual();
            m_sprite.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xB0, 0xF0));
            m_sprite.Size = new Vector2(100, 100);
            //Specify random offset within game window.

            // Create InputSite
            var content = ExpCompositionContent.Create(compositor);
            content.Root = m_sprite;
            var inputsite = ExpInputSite.GetOrCreateForContent(content);

            // PointerInputObserver
            pointerInputObserver = ExpPointerInputObserver.CreateForInputSite(inputsite);
            pointerInputObserver.PointerPressed += OnPointerPressed;
            pointerInputObserver.PointerReleased += OnPointerReleased;

            // GestureRecognizer
            gestureRecognizer = new ExpGestureRecognizer();
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.DoubleTap;
            gestureRecognizer.Tapped += Tapped;

            rootVisual.Children.InsertAtTop(m_sprite);
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
        private void Tapped(object sender, ExpTappedEventArgs eventArgs)
        {
            MoveSprite();
            ++tapCounter;           
        }

        private void MoveSprite()
        {
            // Create new random offset within game window and place sprite there.
            return;
        }

        private void Cleanup()
        {
            m_sprite = null;
        }
    }
}
