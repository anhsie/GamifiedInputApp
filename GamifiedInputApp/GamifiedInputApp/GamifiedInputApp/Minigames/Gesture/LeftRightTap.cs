﻿using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Experimental;
using Microsoft.UI.Input.Experimental;
using System;
using System.Numerics;

namespace GamifiedInputApp.Minigames.Gesture
{
    class LeftRightTap : IMinigame
    {
        private ExpPointerInputObserver pointerInputObserver;
        private ExpGestureRecognizer gestureRecognizer;

        private SpriteVisual sprite;
        private ContainerVisual rootVisual;

        // Minigame variables
        private const int TOTAL_TAPS_TO_WIN = 15;
        private int tapCounter;
        private bool tapLeft;

        MinigameInfo IMinigame.Info => new MinigameInfo(this, "Left/Right Tap", SupportedDeviceTypes.Spatial);

        public void End(in GameContext gameContext, in MinigameState finalState)
        {
            this.Cleanup();
            return;
        }

        private void Cleanup()
        {
            this.rootVisual.Children.RemoveAll();
            sprite = null;
            pointerInputObserver = null;
            gestureRecognizer = null;
        }

        public void Start(in GameContext gameContext, ContainerVisual rootVisual, ExpInputSite inputSite)
        {
            this.Setup(rootVisual);
        }

        public MinigameState Update(in GameContext gameContext)
        {
            MinigameState result = MinigameState.Play;

            if (tapCounter >= TOTAL_TAPS_TO_WIN)
            {
                result = MinigameState.Pass;
            }
            else if (gameContext.Timer.Finished)
            {
                result = MinigameState.Fail;
            }

            return result;
        }

        public void Setup(ContainerVisual rootVisual)
        {
            tapCounter = 0;
            tapLeft = true;

            // Generate visual for tap game.
            this.rootVisual = rootVisual;
            Compositor compositor = rootVisual.Compositor;
            sprite = compositor.CreateSpriteVisual();
            sprite.Brush = compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x00, 0xB0, 0xF0));
            sprite.Size = new Vector2(100, 100);

            // Create InputSite
            var content = ExpCompositionContent.Create(compositor);
            content.Root = sprite;
            var inputsite = ExpInputSite.GetOrCreateForContent(content);

            // PointerInputObserver
            pointerInputObserver = ExpPointerInputObserver.CreateForInputSite(inputsite);
            pointerInputObserver.PointerPressed += OnPointerPressed;
            pointerInputObserver.PointerReleased += OnPointerReleased;

            // GestureRecognizer
            gestureRecognizer = new ExpGestureRecognizer();
            gestureRecognizer.GestureSettings = Windows.UI.Input.GestureSettings.Tap | Windows.UI.Input.GestureSettings.RightTap;
            gestureRecognizer.Tapped += Tapped;
            gestureRecognizer.RightTapped += RightTapped;

            rootVisual.Children.InsertAtTop(sprite);
        }

        //
        // Event Handlers 
        //

        // PointerInputObserver
        private void OnPointerPressed(object sender, ExpPointerEventArgs args)
        {
            gestureRecognizer.ProcessDownEvent(args.CurrentPoint);
        }

        private void OnPointerReleased(object sender, ExpPointerEventArgs args)
        {
            gestureRecognizer.ProcessUpEvent(args.CurrentPoint);
        }

        // GestureRecognizer
        private void Tapped(object sender, ExpTappedEventArgs eventArgs)
        {
            if (tapLeft)
            {
                ProcessCorrectTap();
            }
        }



        private void RightTapped(object sender, ExpRightTappedEventArgs eventArgs)
        {
            if (!tapLeft)
            {
                ProcessCorrectTap();
            }
        }

        private void ProcessCorrectTap()
        {
            ++tapCounter;
            tapLeft = (uint)new Random().Next(0, 1) == 0;
            if (tapLeft)
            {

            }
            else
            {

            }
            throw new NotImplementedException();
        }
    }
}