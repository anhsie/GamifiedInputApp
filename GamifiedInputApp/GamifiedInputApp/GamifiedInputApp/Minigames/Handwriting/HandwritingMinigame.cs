using Microsoft.UI.Composition;
using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GamifiedInputApp.Minigames.Handwriting
{
    class HandwritingMinigame : IMinigame
    {
        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Handwriting Minigame", SupportedDeviceTypes.Spatial);

        ContentHelper m_contentHelper;
        Random m_random;

        int m_letterIndex;
        SpriteVisual m_letterSprite;
        List<ICompositionSurface> m_letterImages;

        int m_nextEllipse = 0;
        List<ShapeVisual> m_ellipseVisuals;
        ExpPointerInputObserver m_pointerInput;

        public HandwritingMinigame()
        {
            m_letterImages = new List<ICompositionSurface>();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                m_letterImages.Add(
                    LoadedImageSurface.StartLoadFromUri(
                        new Uri(string.Format("ms-appx:///Images/LetterTiles/letter_{0}.png", c))));
            }

            m_random = new Random();
            m_letterIndex = m_random.Next(m_letterImages.Count);
        }

        public void Start(in GameContext gameContext, ContentHelper contentHelper)
        {
            m_contentHelper = contentHelper;

            Compositor compositor = m_contentHelper.RootVisual.Compositor;
            m_letterSprite = compositor.CreateSpriteVisual();
            m_letterSprite.Brush = compositor.CreateSurfaceBrush(m_letterImages[m_letterIndex]);
            m_letterSprite.Size = new Vector2(100, 100);
            m_contentHelper.RootVisual.Children.InsertAtTop(m_letterSprite);
            m_contentHelper.Content.StateChanged += Content_StateChanged;

            var ellipse = compositor.CreateEllipseGeometry();
            ellipse.Center = new Vector2(5, 5);
            ellipse.Radius = new Vector2(5, 5);

            var ellipseShape = compositor.CreateSpriteShape();
            ellipseShape.Geometry = ellipse;
            ellipseShape.FillBrush = compositor.CreateColorBrush(Microsoft.UI.Colors.Black);

            m_ellipseVisuals = new List<ShapeVisual>(100);
            for (int i = 0; i < 100; i++)
            {
                ShapeVisual ellipseVisual = compositor.CreateShapeVisual();
                ellipseVisual.Shapes.Add(ellipseShape);
                ellipseVisual.Size = new Vector2(10, 10);
                ellipseVisual.Offset = new Vector3(0, 0, 0);
                ellipseVisual.IsVisible = false;
                m_ellipseVisuals.Add(ellipseVisual);
                m_contentHelper.RootVisual.Children.InsertAtTop(ellipseVisual);
            }

            m_pointerInput = ExpPointerInputObserver.CreateForInputSite(m_contentHelper.InputSite);
            m_pointerInput.PointerPressed += M_pointerInput_PointerPressed;
            m_pointerInput.PointerMoved += M_pointerInput_PointerMoved;
            m_pointerInput.PointerReleased += M_pointerInput_PointerReleased;

            UpdateLayout();
        }

        private void M_pointerInput_PointerPressed(ExpPointerInputObserver sender, ExpPointerEventArgs args)
        {
            Clear();
        }

        void M_pointerInput_PointerMoved(ExpPointerInputObserver sender, ExpPointerEventArgs args)
        {
            var pointerPoint = args.CurrentPoint;

            if (pointerPoint.IsInContact)
            {
                var position = pointerPoint.Position;
                DrawDot((int)position.X, (int)position.Y);
            }
        }

        void M_pointerInput_PointerReleased(ExpPointerInputObserver sender, ExpPointerEventArgs args)
        {
            //...
        }

        void Content_StateChanged(Microsoft.UI.Composition.Experimental.ExpCompositionContent sender, Microsoft.UI.Composition.Experimental.ExpCompositionContentEventArgs args)
        {
            UpdateLayout();
        }

        void UpdateLayout()
        {
            Vector3 offset = new Vector3();
            offset.X = m_contentHelper.Content.ActualSize.X / 3f - m_letterSprite.Size.X / 2f;
            offset.Y = m_contentHelper.Content.ActualSize.Y / 2f - m_letterSprite.Size.Y / 2f;
            m_letterSprite.Offset = offset;
        }

        public MinigameState Update(in GameContext gameContext)
        {
            /*DrawDot(
                m_random.Next((int)m_contentHelper.Content.ActualSize.X),
                m_random.Next((int)m_contentHelper.Content.ActualSize.Y));*/

            return gameContext.Timer.Finished ? MinigameState.Pass : MinigameState.Play;
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
        }

        void DrawDot(int x, int y)
        {
            ShapeVisual ellipse = m_ellipseVisuals[m_nextEllipse];
            ellipse.IsVisible = true;
            ellipse.Offset = new Vector3(x, y, 0);

            m_nextEllipse++;
            if (m_nextEllipse >= m_ellipseVisuals.Count)
            {
                m_nextEllipse = 0;
            }
        }

        void Clear()
        {
            foreach (ShapeVisual shape in m_ellipseVisuals)
            {
                shape.IsVisible = false;
            }
            m_nextEllipse = 0;
        }
    }
}
