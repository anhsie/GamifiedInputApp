using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Input.Experimental;
using System;
using System.Numerics;
using Windows.UI.Core;

namespace GamifiedInputApp.Minigames.Cursor
{
    class CursorController : IMinigame
    {
        private SpriteVisual leftVisual;
        private SpriteVisual rightVisual;
        private ExpInputSite inputSite;
        private ExpPointerCursorController cursorController;
        private ContainerVisual rootVisual;
        private int VISUAL_SIZE = 100;
        MinigameInfo IMinigame.Info => new MinigameInfo(this, "CursorController", SupportedDeviceTypes.Spatial);

        public void Start(in GameContext gameContext, ContainerVisual rootVisual, ExpInputSite inputSite)
        {
            this.Setup(rootVisual, inputSite); // Setup game board

            // Do start logic for minigame
        }

        public MinigameState Update(in GameContext gameContext)
        {
            // Do update logic for minigame
            if (cursorController != null)
            {
                var cursorPosition = cursorController.Position;
                if (InsideVisual(leftVisual, cursorPosition))
                {
                    CoreCursor cursor = new CoreCursor(CoreCursorType.Hand, 99);
                    cursorController.Cursor = cursor;
                }
                else if (InsideVisual(rightVisual, cursorPosition))
                {
                    CoreCursor cursor = new CoreCursor(CoreCursorType.Person, 99);
                    cursorController.Cursor = cursor;
                }
                else
                {
                    CoreCursor cursor = new CoreCursor(CoreCursorType.Arrow, 99);
                    cursorController.Cursor = cursor;
                }
            }

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

        private void Setup(ContainerVisual rootVisual, ExpInputSite inputSite)
        {
            this.rootVisual = rootVisual;
            // Setup game board here
            Compositor compositor = rootVisual.Compositor;
            leftVisual = compositor.CreateSpriteVisual();
            leftVisual.Brush = compositor.CreateColorBrush(Colors.Red);
            leftVisual.Size = new Vector2(VISUAL_SIZE, VISUAL_SIZE);
            leftVisual.Offset = new Vector3(100, 200, 0);
            rightVisual = compositor.CreateSpriteVisual();
            rightVisual.Brush = compositor.CreateColorBrush(Colors.Red);
            rightVisual.Size = new Vector2(VISUAL_SIZE, VISUAL_SIZE);
            rightVisual.Offset = new Vector3(300, 200, 0);
            rootVisual.Children.InsertAtTop(leftVisual);
            rootVisual.Children.InsertAtTop(rightVisual);

            if(inputSite != null)
            {
                this.inputSite = inputSite;
                cursorController = ExpPointerCursorController.GetForInputSite(inputSite);
            }
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
            leftVisual = null;
            rightVisual = null;

            rootVisual.Children.RemoveAll();
        }
    }
}
