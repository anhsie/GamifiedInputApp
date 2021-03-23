using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Media;

namespace GamifiedInputApp.Minigames
{
    class CharacterReceived : IMinigame
    {
        private const float SPRITE_SPEED = 0.5f;

        private SpriteVisual letterVisual;
        private Compositor compositor;

        private List<ICompositionSurface> letterImages;

        public void Start(in GameContext gameContext, ContainerVisual rootVisual)
        {
            compositor = rootVisual.Compositor;
            this.Setup(rootVisual); // Setup game board

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

            return gameContext.Timer.Finished ? MinigameState.Pass : MinigameState.Play; // Return new state (auto pass here)
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            this.Cleanup(); // Cleanup game board

            // Do cleanup logic for minigame
        }


        private void Setup(ContainerVisual rootVisual)
        {
            SetupImages();

            // Setup game board here
            letterVisual = compositor.CreateSpriteVisual();
            letterVisual.Size = new Vector2(100, 100);
            letterVisual.Offset = new Vector3(100,0,0);
            var surfaceBrush = compositor.CreateSurfaceBrush();
            surfaceBrush.Surface = letterImages[0];
            letterVisual.Brush = surfaceBrush;
            //var colorBrush = compositor.CreateColorBrush();
            //colorBrush.Color = Microsoft.UI.Colors.Red;
            //letterVisual.Brush = colorBrush;
            rootVisual.Children.InsertAtTop(letterVisual);
        }

        private void Animate(in GameContext gameContext)
        {
            // Animate things here
            float dt = (float)gameContext.Timer.DeltaTime;

            Vector3 offset = letterVisual.Offset;
            offset.Y += (dt * SPRITE_SPEED);
            letterVisual.Offset = offset;
        }

        private void Cleanup()
        {
            letterVisual = null;
        }
    }
}
