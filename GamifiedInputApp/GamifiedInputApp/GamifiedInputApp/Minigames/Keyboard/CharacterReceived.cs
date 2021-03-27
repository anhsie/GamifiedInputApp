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
    class CharacterReceived : IMinigame
    {
        private const float SPRITE_SPEED = 0.15f;

        private ContentHelper content;
        private ContainerVisual rootVisual;
        private SpriteVisual letterVisual;
        private ExpInputSite inputSite;
        private Compositor compositor;
        private UInt32 ansIndex;

        private List<ICompositionSurface> letterImages;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "CharacterReceived", SupportedDeviceTypes.Keyboard);

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
            keyboardInput.CharacterReceived += CharacterReceivedEventHandler;
        }

        public MinigameState Update(in GameContext gameContext)
        {
            this.Animate(gameContext); // Animate game board

            // Do update logic for minigame
            if (letterVisual != null && letterVisual.Offset.Y > content.Content.ActualSize.Y)
            {
                return MinigameState.Fail;
            }

            return gameContext.Timer.Finished ? MinigameState.Pass : MinigameState.Play; // Return new state (auto pass here)
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            var keyboardInput = ExpKeyboardInput.GetForInputSite(inputSite);
            keyboardInput.CharacterReceived -= CharacterReceivedEventHandler;

            this.Cleanup(); // Cleanup game board
        }

        /******* Event functions *******/

        private void CharacterReceivedEventHandler(object sender, Windows.UI.Core.CharacterReceivedEventArgs args)
        {
            var keyCode = args.KeyCode;
            // make sure key is from a-z
            if (keyCode >= 97 && keyCode <= 123)
            {
                var index = keyCode - 97;
                if (index == ansIndex)
                {
                    rootVisual.Children.Remove(letterVisual);
                    letterVisual = null;
                }
            }
        }

        /***** Animation functions *****/

        private void Setup()
        {
            if (letterImages == null)
            {
                SetupImages();
            }

            // Setup game board here
            CreateNewLetterVisual();
        }

        private void SetupImages()
        {
            letterImages = new List<ICompositionSurface>();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                letterImages.Add(
                    LoadedImageSurface.StartLoadFromUri(
                        new Uri(string.Format("ms-appx:///Images/LetterTiles/letter_{0}.png", c))));
            }
        }

        private void CreateNewLetterVisual()
        {
            var random = new Random();
            ansIndex = (uint)random.Next(0, 26);

            letterVisual = compositor.CreateSpriteVisual();
            letterVisual.Size = new Vector2(100, 100);

            // Make sure letter is within the window

            // TODO: content.Content.ActualSize returns 0,0 initially, but we would like to use this
            // instead of hardcoding size
            var size = new Vector2(400, 400);
            letterVisual.Offset = new Vector3(random.Next(0, (int)size.X - 100), 0, 0);

            var surfaceBrush = compositor.CreateSurfaceBrush();
            surfaceBrush.Surface = letterImages[(int)ansIndex];
            letterVisual.Brush = surfaceBrush;
            rootVisual.Children.InsertAtTop(letterVisual);
        }

        private void Animate(in GameContext gameContext)
        {
            // Animate things here
            float dt = (float)gameContext.Timer.DeltaTime.TotalMilliseconds;

            if(letterVisual != null)
            {
                Vector3 offset = letterVisual.Offset;
                offset.Y += (dt * SPRITE_SPEED);
                letterVisual.Offset = offset;
            }
            else
            {
                CreateNewLetterVisual();
            }
        }

        private void Cleanup()
        {
            letterVisual = null;
            rootVisual.Children.RemoveAll();
        }

    }
}
