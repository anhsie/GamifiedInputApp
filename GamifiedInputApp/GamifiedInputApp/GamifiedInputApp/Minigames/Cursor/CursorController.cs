using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI.Core;

namespace GamifiedInputApp.Minigames.Cursor
{
    class CursorController : IMinigame
    {
        enum CursorType
        {
            IBeam,
            Cross
        }

        private List<SpriteVisual> cursorVisuals;
        private List<CursorType> cursorTypesForVisuals;
        private SpriteVisual ansVisual;
        private CursorType ansCursorType;
        private MinigameState currentState;

        private ExpInputSite inputSite;
        private ExpPointerCursorController cursorController;
        private ExpPointerInputObserver pointerInputObserver;
        private ContainerVisual rootVisual;
        private int VISUAL_SIZE = 100;
        MinigameInfo IMinigame.Info => new MinigameInfo(this, "CursorController", SupportedDeviceTypes.Spatial);

        public void Start(in GameContext gameContext)
        {
            this.Setup(gameContext.Content.RootVisual); // Setup game board
            
            // Do start logic for minigame
            if (gameContext.Content.InputSite != null)
            {
                this.inputSite = gameContext.Content.InputSite;
                cursorController = ExpPointerCursorController.GetForInputSite(inputSite);
                pointerInputObserver = ExpPointerInputObserver.CreateForInputSite(inputSite);
                pointerInputObserver.PointerReleased += PointerInputObserver_PointerReleased;
            }
        }

        public MinigameState Update(in GameContext gameContext)
        {
            // Do update logic for minigame
            if (cursorController != null)
            {
                var cursorPosition = cursorController.Position;
                CoreCursor cursor = new CoreCursor(CoreCursorType.Arrow, 99);
                for (int i = 0; i < cursorVisuals.Count; i++)
                {
                    if (InsideVisual(cursorVisuals[i], cursorPosition))
                    {
                        switch (cursorTypesForVisuals[i])
                        {
                            case CursorType.Cross:
                                cursor = new CoreCursor(CoreCursorType.Cross, 99);
                                break;
                            case CursorType.IBeam:
                                cursor = new CoreCursor(CoreCursorType.IBeam, 99);
                                break;
                        }
                        break;
                    }
                }
                cursorController.Cursor = cursor;
            }

            if(currentState != MinigameState.Play)
            {
                return currentState;
            }

            return gameContext.Timer.Finished ? MinigameState.Fail : MinigameState.Play; // Return new state (auto pass here)
        }

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            pointerInputObserver.PointerReleased -= PointerInputObserver_PointerReleased;

            this.Cleanup(); // Cleanup game board
        }

        /******* Event functions *******/

        private void PointerInputObserver_PointerReleased(ExpPointerInputObserver sender, ExpPointerEventArgs args)
        {
            for (int i = 0; i < cursorVisuals.Count; i++)
            {
                if (InsideVisual(cursorVisuals[i], args.CurrentPoint.Position))
                {
                    if (cursorTypesForVisuals[i] == ansCursorType)
                    {
                        currentState = MinigameState.Pass;
                    }
                    else
                    {
                        currentState = MinigameState.Fail;
                    }
                    break;
                }
            }
        }


        /***** Animation functions *****/

        private const float SPRITE_SPEED = 1.0f;

        private void Setup(ContainerVisual rootVisual)
        {
            currentState = MinigameState.Play;
            this.rootVisual = rootVisual;
            cursorVisuals = new List<SpriteVisual>();
            cursorTypesForVisuals = new List<CursorType>();
            // Setup game board here
            Compositor compositor = rootVisual.Compositor;
            var leftVisual = compositor.CreateSpriteVisual();
            leftVisual.Brush = compositor.CreateColorBrush(Colors.Red);
            leftVisual.Size = new Vector2(VISUAL_SIZE, VISUAL_SIZE);
            leftVisual.Offset = new Vector3(50, 200, 0);
            rootVisual.Children.InsertAtTop(leftVisual);
            cursorVisuals.Add(leftVisual);
            cursorTypesForVisuals.Add(CursorType.Cross);
            
            var rightVisual = compositor.CreateSpriteVisual();
            rightVisual.Brush = compositor.CreateColorBrush(Colors.Red);
            rightVisual.Size = new Vector2(VISUAL_SIZE, VISUAL_SIZE);
            rightVisual.Offset = new Vector3(250, 200, 0);
            rootVisual.Children.InsertAtTop(rightVisual);
            cursorVisuals.Add(rightVisual);
            cursorTypesForVisuals.Add(CursorType.IBeam);

            ansVisual = compositor.CreateSpriteVisual();
            var surfaceBrush = compositor.CreateSurfaceBrush();
            var random = new Random();
            var randomNumber = random.Next(0, 2);
            if(randomNumber == (int)CursorType.Cross)
            {
                var surface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/CursorTypes/cross.png"));
                surfaceBrush.Surface = surface;
                ansCursorType = CursorType.Cross;
            }
            else
            {
                var surface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Images/CursorTypes/ibeam.png"));
                surfaceBrush.Surface = surface;
                ansCursorType = CursorType.IBeam;
            }
            ansVisual.Brush = surfaceBrush;
            ansVisual.Size = new Vector2(VISUAL_SIZE, VISUAL_SIZE);
            ansVisual.Offset = new Vector3(150, 0, 0);
            rootVisual.Children.InsertAtTop(ansVisual);
        }

        private bool InsideVisual(SpriteVisual visual, Windows.Foundation.Point point)
        {
            if (point.X < visual.Offset.X)
            {
                return false;
            }
            if (point.Y < visual.Offset.Y)
            {
                return false;
            }
            if (point.X > visual.Offset.X + visual.Size.X)
            {
                return false;
            }
            if (point.Y > visual.Offset.Y + visual.Size.Y)
            {
                return false;
            }

            return true;
        }
        
        private void Cleanup()
        {
            rootVisual.Children.RemoveAll();
        }
    }
}
