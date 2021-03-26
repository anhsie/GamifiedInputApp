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
        private const float SPRITE_SPEED = 0.25f;

        private ContainerVisual rootVisual;
        private SpriteVisual letterVisual;
        private ExpInputSite inputSite;
        private Compositor compositor;
        private UInt32 ansIndex;

        private List<ICompositionSurface> letterImages;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "CharacterReceived", SupportedDeviceTypes.Keyboard);

        public void Start(in GameContext gameContext, ContainerVisual rootVisual, ExpInputSite inputSite)
        {
            this.rootVisual = rootVisual;
            this.inputSite = inputSite;
            compositor = rootVisual.Compositor;
            this.Setup(rootVisual, inputSite); // Setup game board

            // Do start logic for minigame

        }

        private void SetupImages()
        {
            letterImages = new List<ICompositionSurface>();
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_A.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_B.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_C.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_D.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_E.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_F.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_G.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_H.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_I.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_J.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_K.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_L.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_M.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_N.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_O.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_P.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_Q.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_R.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_S.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_T.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_U.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_V.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_W.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_X.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_Y.png")));
            letterImages.Add(LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/LetterTiles/letter_Z.png")));
        }

        public MinigameState Update(in GameContext gameContext)
        {
            this.Animate(gameContext); // Animate game board

            // Do update logic for minigame
            
            // TODO: fail based on size of window
            if(letterVisual != null && letterVisual.Offset.Y > 500)
            {
                return MinigameState.Fail;
            }

            return gameContext.Timer.Finished ? MinigameState.Pass : MinigameState.Play; // Return new state (auto pass here)
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            this.Cleanup(); // Cleanup game board

            // Do cleanup logic for minigame
        }


        private void Setup(ContainerVisual rootVisual, ExpInputSite inputSite)
        {
            SetupImages();

            if(inputSite != null)
            {
                // Set focus on the new window so that keyboard input goes through properly
                var focusController = ExpFocusController.GetForInputSite(inputSite);
                focusController.TrySetFocus();

                var keyboardInput = ExpKeyboardInput.GetForInputSite(inputSite);
                keyboardInput.CharacterReceived += CharacterReceivedEventHandler;
            }

            var random = new Random();
            ansIndex = (uint) random.Next(0, 25);

            // Setup game board here
            letterVisual = compositor.CreateSpriteVisual();
            letterVisual.Size = new Vector2(100, 100);
            // TODO: Random X based on size of window
            letterVisual.Offset = new Vector3(random.Next(0,500),0,0);
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
        }

        private void Cleanup()
        {
            letterVisual = null;
            rootVisual.Children.RemoveAll();

            if (inputSite != null)
            {
                var keyboardInput = ExpKeyboardInput.GetForInputSite(inputSite);
                keyboardInput.CharacterReceived -= CharacterReceivedEventHandler;
            }
        }

        private void CharacterReceivedEventHandler(object sender, Windows.UI.Core.CharacterReceivedEventArgs args)
        {
            var keyCode = args.KeyCode;
            // make sure key is from a-z
            if (keyCode >= 97 && keyCode <= 123)
            {
                var index = keyCode - 97;
                if(index == ansIndex)
                {
                    letterVisual = null;
                }
            }
        }
    }
}
