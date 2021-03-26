using Microsoft.UI.Composition;
using Microsoft.UI.Input.Experimental;
using System;
using System.Numerics;

namespace GamifiedInputApp.Minigames.Sample
{
    class DummyMinigame : IMinigame
    {
        private ContainerVisual rootVisual;
        private SpriteVisual m_sprite;
        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Dummy Minigame", SupportedDeviceTypes.None);

        public void Start(in GameContext gameContext)
        {
            this.Setup(gameContext.Content.RootVisual); // Setup game board

            // Do start logic for minigame
        }

        public MinigameState Update(in GameContext gameContext)
        {
            this.Animate(gameContext); // Animate game board

            // Do update logic for minigame

            return m_sprite.Offset.X > 400 ? MinigameState.Pass : MinigameState.Play; // Return new state (auto pass here)
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            this.Cleanup(); // Cleanup game board

            // Do cleanup logic for minigame
        }

        /***** Animation functions *****/

        private const float SPRITE_SPEED = 0.2f;

        private void Setup(ContainerVisual rootVisual)
        {
            this.rootVisual = rootVisual;
            // Setup game board here
            Compositor compositor = rootVisual.Compositor;
            m_sprite = compositor.CreateSpriteVisual();
            m_sprite.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xB0, 0xF0));
            m_sprite.Size = new Vector2(100, 100);
            rootVisual.Children.InsertAtTop(m_sprite);
        }

        private void Animate(in GameContext gameContext)
        {
            // Animate things here
            float dt = (float)gameContext.Timer.DeltaTime.TotalMilliseconds;

            Vector3 offset = m_sprite.Offset;
            offset.X += (dt * SPRITE_SPEED);
            m_sprite.Offset = offset;
        }
        
        private void Cleanup()
        {
            rootVisual.Children.RemoveAll();
            m_sprite = null;
        }
    }
}
