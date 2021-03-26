using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;

namespace GamifiedInputApp.Minigames.Handwriting
{
    class HandwritingMinigame : IMinigame
    {
        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Handwriting Minigame", SupportedDeviceTypes.Spatial);

        // Input and Ink Analysis APIs
        ExpPointerInputObserver m_pointerInput;
        InkAnalyzer m_inkAnalyzer = new InkAnalyzer();
        InkStrokeBuilder m_inkStrokebuilder = new InkStrokeBuilder();

        // Minigame variables
        Random m_random = new Random();
        MinigameState m_state = MinigameState.Play;
        int m_letterIndex = 0;
        string m_letterAsString = "A";
        SpriteVisual m_letterSprite;
        List<ICompositionSurface> m_letterImages;
        int m_nextEllipse = 0;
        List<ShapeVisual> m_ellipseVisuals;

        public HandwritingMinigame()
        {
            m_letterImages = new List<ICompositionSurface>();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                m_letterImages.Add(
                    LoadedImageSurface.StartLoadFromUri(
                        new Uri(string.Format("ms-appx:///Images/LetterTiles/letter_{0}.png", c))));
            }
        }

        public void Start(in GameContext gameContext)
        {
            // Pick a random letter of the alphabet to display and perform ink analysis on.
            m_letterIndex = m_random.Next(m_letterImages.Count);
            m_letterAsString = Convert.ToChar(Convert.ToInt32('A') + m_letterIndex).ToString();
            
            // Setup game and UI state.
            m_state = MinigameState.Play;
            SetupVisuals(gameContext);

            // Clear our ink analyzer between runs of the minigame.            
            m_inkAnalyzer.ClearDataForAllStrokes();
            m_nextEllipse = 0;

            // Setup our pointer input observer to track handwriting.
            m_pointerInput = ExpPointerInputObserver.CreateForInputSite(gameContext.Content.InputSite);
            m_pointerInput.PointerPressed += M_pointerInput_PointerPressed;
            m_pointerInput.PointerMoved += M_pointerInput_PointerMoved;
            m_pointerInput.PointerReleased += M_pointerInput_PointerReleased;

            UpdateLayout(gameContext.Content.Content);
        }

        public MinigameState Update(in GameContext gameContext)
        {
            return m_state;
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            Cleanup(gameContext);
        }

        /******* Event functions *******/

        private void M_pointerInput_PointerPressed(ExpPointerInputObserver sender, ExpPointerEventArgs args)
        {
            if (args.CurrentPoint.Properties.IsEraser)
            {
                ClearHandwriting();
            }
        }

        void M_pointerInput_PointerMoved(ExpPointerInputObserver sender, ExpPointerEventArgs args)
        {
            var pointerPoint = args.CurrentPoint;

            if (pointerPoint.IsInContact && !pointerPoint.Properties.IsEraser)
            {
                var position = pointerPoint.Position;
                DrawHandwritingDot((int)position.X, (int)position.Y);
            }
        }

        void M_pointerInput_PointerReleased(ExpPointerInputObserver sender, ExpPointerEventArgs args)
        {
            if (m_inkAnalyzer.IsAnalyzing)
            {
                return;
            }

            List<Windows.Foundation.Point> points = GetHandwritingDotsAsPoints();

            if (points.Count > 0)
            {
                InkStroke stroke = m_inkStrokebuilder.CreateStroke(points);
                m_inkAnalyzer.AddDataForStroke(stroke);
                m_inkAnalyzer.SetStrokeDataKind(stroke.Id, InkAnalysisStrokeKind.Writing);
                
                var operation = m_inkAnalyzer.AnalyzeAsync();
                operation.Completed = M_inkAnalyzer_AnalyzeAsync_Completed;
            }
        }

        private void M_inkAnalyzer_AnalyzeAsync_Completed(IAsyncOperation<InkAnalysisResult> result, AsyncStatus status)
        {
            // Check our analysis results to see if the player successfully drew the requested
            // letter. If the recognition fails, we don't immediately fail the game. Instead,
            // we allow the player to keep adding new strokes to see if they complete the
            // recognition. This allows for multi-stroke letters to be recognized (e.g. 'E').

            Debugger.Log(1, "Ink Analysis Result", string.Format("Status = {0}", status));
            if (status == Windows.Foundation.AsyncStatus.Completed)
            {
                InkAnalysisResult analysisResult = result.GetResults();
                if (analysisResult.Status == InkAnalysisStatus.Updated)
                {
                    // Find all strokes that are recognized as handwriting and see if the
                    // recognized text matches our letter.
                    var inkwordNodes =
                        m_inkAnalyzer.AnalysisRoot.FindNodes(
                            InkAnalysisNodeKind.InkWord);

                    // Iterate through each InkWord node.
                    foreach (InkAnalysisInkWord node in inkwordNodes)
                    {
                        Debugger.Log(1, "Ink Analysis Result", string.Format("Word Recognized = '{0}', Looking for = '{1}'", node.RecognizedText, m_letterAsString));
                        if (string.Compare(node.RecognizedText, m_letterAsString, true) == 0)
                        {
                            m_state = MinigameState.Pass;
                        }
                    }
                }
            }
        }

        void Content_StateChanged(ExpCompositionContent sender, ExpCompositionContentEventArgs args)
        {
            UpdateLayout(sender);
        }

        /***** Animation and Drawing functions *****/

        void SetupVisuals(GameContext gameContext)
        {
            Compositor compositor = gameContext.Content.RootVisual.Compositor;
            m_letterSprite = compositor.CreateSpriteVisual();
            m_letterSprite.Brush = compositor.CreateSurfaceBrush(m_letterImages[m_letterIndex]);
            m_letterSprite.Size = new Vector2(100, 100);
            gameContext.Content.RootVisual.Children.InsertAtTop(m_letterSprite);
            gameContext.Content.Content.StateChanged += Content_StateChanged;

            var ellipse = compositor.CreateEllipseGeometry();
            ellipse.Center = new Vector2(5, 5);
            ellipse.Radius = new Vector2(5, 5);

            var ellipseShape = compositor.CreateSpriteShape();
            ellipseShape.Geometry = ellipse;
            ellipseShape.FillBrush = compositor.CreateColorBrush(Microsoft.UI.Colors.Black);

            const int totalDrawingShapes = 100;
            m_ellipseVisuals = new List<ShapeVisual>(totalDrawingShapes);
            for (int i = 0; i < totalDrawingShapes; i++)
            {
                ShapeVisual ellipseVisual = compositor.CreateShapeVisual();
                ellipseVisual.Shapes.Add(ellipseShape);
                ellipseVisual.Size = new Vector2(10, 10);
                ellipseVisual.Offset = new Vector3(0, 0, 0);
                ellipseVisual.IsVisible = false;
                m_ellipseVisuals.Add(ellipseVisual);
                gameContext.Content.RootVisual.Children.InsertAtTop(ellipseVisual);
            }
        }

        void UpdateLayout(ExpCompositionContent content)
        {
            float contentWidth = content.ActualSize.X;
            float contentHeight = content.ActualSize.Y;

            // Set the letter image to fill the window but maintain its aspect ratio.
            float lesserDimension = Math.Min(contentWidth, contentHeight);
            m_letterSprite.Size = new Vector2(lesserDimension, lesserDimension);

            // Center the letter image
            Vector3 offset = new Vector3();
            offset.X = contentWidth / 2f - lesserDimension / 2f;
            offset.Y = contentHeight / 2f - lesserDimension / 2f;
            m_letterSprite.Offset = offset;
        }

        public void Cleanup(GameContext gameContext)
        {
            m_pointerInput.PointerReleased -= M_pointerInput_PointerReleased;
            m_pointerInput.PointerMoved -= M_pointerInput_PointerMoved;
            m_pointerInput.PointerPressed -= M_pointerInput_PointerPressed;
            m_pointerInput = null;

            m_nextEllipse = 0;
            m_inkAnalyzer.ClearDataForAllStrokes();
            m_ellipseVisuals = null;

            gameContext.Content.Content.StateChanged -= Content_StateChanged;
            m_letterSprite = null;
        }

        void DrawHandwritingDot(int x, int y)
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

        List<Windows.Foundation.Point> GetHandwritingDotsAsPoints()
        {
            List<Windows.Foundation.Point> points = new List<Windows.Foundation.Point>(m_ellipseVisuals.Count);

            int currentEllipse = m_nextEllipse;
            for (int strokesInspected = 0; strokesInspected < m_ellipseVisuals.Count; strokesInspected++)
            {
                if (m_ellipseVisuals[currentEllipse].IsVisible)
                {
                    points.Add(new Windows.Foundation.Point(
                        m_ellipseVisuals[currentEllipse].Offset.X,
                        m_ellipseVisuals[currentEllipse].Offset.Y));
                }

                currentEllipse++;
                if (currentEllipse >= m_ellipseVisuals.Count)
                {
                    currentEllipse = 0;
                }
            }

            return points;
        }

        void ClearHandwriting()
        {
            foreach (ShapeVisual shape in m_ellipseVisuals)
            {
                shape.IsVisible = false;
            }
            m_nextEllipse = 0;

            m_inkAnalyzer.ClearDataForAllStrokes();
        }
    }
}
