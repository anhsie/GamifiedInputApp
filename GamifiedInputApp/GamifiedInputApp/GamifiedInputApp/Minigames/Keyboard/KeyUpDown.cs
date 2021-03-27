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
        private const float SPRITE_SPEED = 0.25f;

        private ContentHelper content;
        private ContainerVisual rootVisual;
        private SpriteVisual lightVisual;
        private SpriteVisual objectVisual;
        private ExpInputSite inputSite;
        private Compositor compositor;

        private bool RightKeyPressed;
        private bool LeftKeyPressed;
        private bool UpKeyPressed;
        private bool DownKeyPressed;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Key Up/Down", SupportedDeviceTypes.Keyboard);

        public void Start(in GameContext gameContext)
        {
            content = gameContext.Content;
            rootVisual = gameContext.Content.RootVisual;
            inputSite = gameContext.Content.InputSite;
            compositor = rootVisual.Compositor;

            this.Setup(); // Setup game board

            // Set focus on the new window so that keyboard input goes through properly
            var focusController = ExpFocusController.GetForInputSite(inputSite);
            focusController.TrySetFocus();

            var keyboardInput = ExpKeyboardInput.GetForInputSite(inputSite);
            keyboardInput.KeyUp += KeyUp;
            keyboardInput.KeyDown += KeyDown;

        }

        public MinigameState Update(in GameContext gameContext)
        {
            this.Animate(gameContext); // Animate game board

            // Do update logic for minigame
            // If object is centered in light visual pass
            if(IsLightInsideObject(objectVisual))
            {
                return MinigameState.Pass;
            }
            
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
                case Windows.System.VirtualKey.Right:
                    RightKeyPressed = false;
                    return;
                case Windows.System.VirtualKey.Left:
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
                case Windows.System.VirtualKey.Right:
                    RightKeyPressed = true;
                    return;
                case Windows.System.VirtualKey.Left:
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
            RightKeyPressed = false;
            LeftKeyPressed = false;
            UpKeyPressed = false;
            DownKeyPressed = false;

            // Setup game board here
            objectVisual = compositor.CreateSpriteVisual();
            var penguinSurface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/Animals/penguin.png"));
            var penguinBrush = compositor.CreateSurfaceBrush(penguinSurface);
            objectVisual.Brush = penguinBrush;
            objectVisual.Size = new Vector2(80, 80);

            // Generate object within the game window bounds
            var random = new Random();
            // TODO: content.Content.ActualSize returns 0,0 initially, but we would like to use this
            // instead of hardcoding size
            var size = new Vector2(400, 400);
            objectVisual.Offset = new Vector3(random.Next(100, (int)size.X - 100), random.Next(100, (int)size.Y - 100), 0);
            rootVisual.Children.InsertAtTop(objectVisual);

            lightVisual = compositor.CreateSpriteVisual();
            lightVisual.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(150,255,255,0));
            lightVisual.Size = new Vector2(100, 100);
            lightVisual.Offset = new Vector3(0,0,0);
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

        private bool IsLightInsideObject(SpriteVisual visual)
        {
            float xDiff = lightVisual.Size.X - visual.Size.X;
            float yDiff = lightVisual.Size.Y - visual.Size.Y;
            if (lightVisual.Offset.X < visual.Offset.X - xDiff || lightVisual.Offset.X > visual.Offset.X)
            {
                return false;
            }
            if (lightVisual.Offset.Y < visual.Offset.Y - yDiff || lightVisual.Offset.Y > visual.Offset.Y)
            {
                return false;
            }

            return true;
        }

        private void Cleanup()
        {
            lightVisual = null;
            objectVisual = null;
            rootVisual.Children.RemoveAll();
        }
    }
}
