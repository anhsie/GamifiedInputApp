using Microsoft.UI.Composition;
using System;
using System.Numerics;

namespace GamifiedInputApp.Minigames.Sample
{
    class DummyMinigame : IMinigame
    {
        private SpriteVisual m_sprite;
        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Dummy Minigame", SupportedDeviceTypes.All);

        public void Start(in GameContext gameContext, ContainerVisual rootVisual)
        {
            this.Setup(rootVisual); // Setup game board

            // Do start logic for minigame
        }

        public MinigameState Update(in GameContext gameContext)
        {
            this.Animate(gameContext); // Animate game board

            // Do update logic for minigame

            return gameContext.Timer.Finished ? MinigameState.Pass : MinigameState.Play; // Return new state (auto pass here)
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            this.Cleanup(); // Cleanup game board

            // Do cleanup logic for minigame
        }

        /*******************************/
        /***** Animation functions *****/
        /*******************************/

        private const float SPRITE_SPEED = 1.0f;

        private void Setup(ContainerVisual rootVisual)
        {
            // Setup game board here
            Compositor compositor = rootVisual.Compositor;
            m_sprite = compositor.CreateSpriteVisual();
            m_sprite.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xB0, 0xF0));
            rootVisual.Children.InsertAtTop(m_sprite);
        }

        private void Animate(in GameContext gameContext)
        {
            // Animate things here
            float dt = (float)gameContext.Timer.DeltaTime;

            Vector3 offset = m_sprite.Offset;
            offset.X += (dt * SPRITE_SPEED);
            m_sprite.Offset = offset;

            Console.WriteLine(dt);
        }
        
        private void Cleanup()
        {
            m_sprite = null;
        }
    }
}
