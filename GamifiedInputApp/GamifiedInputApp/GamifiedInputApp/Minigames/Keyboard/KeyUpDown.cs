using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Xaml.Media;

namespace GamifiedInputApp.Minigames.Keyboard
{
    class KeyUpDown : IMinigame
    {
        private const float SPRITE_SPEED = 0.5f;

        private ContainerVisual rootVisual;
        private SpriteVisual lightVisual;
        private ExpInputSite inputSite;
        private Compositor compositor;

        private bool RightKeyPressed;
        private bool LeftKeyPressed;
        private bool UpKeyPressed;
        private bool DownKeyPressed;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Key Up/Down", SupportedDeviceTypes.Keyboard);

        public void Start(in GameContext gameContext)
        {
            rootVisual = gameContext.Content.RootVisual;
            inputSite = gameContext.Content.InputSite;
            compositor = rootVisual.Compositor;

            this.Setup(); // Setup game board

            var keyboardInput = ExpKeyboardInput.GetForInputSite(inputSite);
            keyboardInput.KeyUp += KeyUp;
            keyboardInput.KeyDown += KeyDown;

        }

        public MinigameState Update(in GameContext gameContext)
        {
            this.Animate(gameContext); // Animate game board

            // Do update logic for minigame
            // If object is centered in light visual pass
            
            return gameContext.Timer.Finished ? MinigameState.Fail : MinigameState.Play; // Return new state (auto pass here)
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            this.Cleanup(); // Cleanup game board

            var keyboardInput = ExpKeyboardInput.GetForInputSite(inputSite);
            keyboardInput.KeyUp -= KeyUp;
            keyboardInput.KeyDown -= KeyDown;

        }

        /******* Event functions *******/

        private void KeyUp(object sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.RightButton:
                    RightKeyPressed = false;
                    return;
                case Windows.System.VirtualKey.LeftButton:
                    LeftKeyPressed = false;
                    return;
                case Windows.System.VirtualKey.Up:
                    UpKeyPressed = false;
                    return;
                case Windows.System.VirtualKey.Down:
                    DownKeyPressed = false;
                    return;
                default:
                    return;
            }
        }

        private void KeyDown(object sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.RightButton:
                    RightKeyPressed = true;
                    return;
                case Windows.System.VirtualKey.LeftButton:
                    LeftKeyPressed = true;
                    return;
                case Windows.System.VirtualKey.Up:
                    UpKeyPressed = true;
                    return;
                case Windows.System.VirtualKey.Down:
                    DownKeyPressed = true;
                    return;
                default:
                    return;
            }
        }

        private void Setup()
        {           
            // Setup game board here
            lightVisual = compositor.CreateSpriteVisual();
            lightVisual.Size = new Vector2(100, 100);
            lightVisual.Offset = new Vector3(100,0,0);
            rootVisual.Children.InsertAtTop(lightVisual);
        }

        private void Animate(in GameContext gameContext)
        {
            // Animate things here
            float dt = (float)gameContext.Timer.DeltaTime.TotalMilliseconds;
            //Check for borders
            Vector3 offset = lightVisual.Offset;
            if(RightKeyPressed && !LeftKeyPressed)
            {
                //Check border
                offset.X += (dt * SPRITE_SPEED);
            } else if (!RightKeyPressed && LeftKeyPressed)
            {
                //Check border
                offset.X -= (dt * SPRITE_SPEED);
            }
            if (UpKeyPressed && !DownKeyPressed)
            {
                //Check border
                offset.Y -= (dt * SPRITE_SPEED);
            }
            else if (!UpKeyPressed && DownKeyPressed)
            {
                //Check border
                offset.Y += (dt * SPRITE_SPEED);
            }
            lightVisual.Offset = offset;
        }

        private void Cleanup()
        {
            lightVisual = null;
            rootVisual.Children.RemoveAll();
        }
    }
}
