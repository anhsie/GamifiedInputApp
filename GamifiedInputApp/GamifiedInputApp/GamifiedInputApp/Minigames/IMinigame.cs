using Microsoft.UI.Composition;
using Microsoft.UI.Input.Experimental;
using System;
using System.Text;
using Windows.Devices.Input;

namespace GamifiedInputApp.Minigames
{
    [Flags]
    public enum SupportedDeviceTypes
    {
        None    = 0b0000,
        Mouse   = 0b0001,
        Touch   = 0b0010,
        Pen     = 0b0100,
        Keyboard= 0b1000,

        Pointer = Touch | Pen,
        Spatial = Touch | Pen | Mouse,
        All     = Touch | Pen | Mouse | Keyboard,
    }

    public enum MinigameState
    {
        Play,
        Pass,
        Fail
    }

    public class MinigameInfo
    {
        public MinigameInfo(IMinigame minigame, string name, SupportedDeviceTypes devices)
        {
            Minigame = minigame;
            Name = name;
            Devices = devices;
        }

        public override string ToString() => Name;

        public readonly IMinigame Minigame;
        public readonly string Name; // name of miningame (e.g. "PressTheEnterKey! (KeyDown/KeyUp)")
        public readonly SupportedDeviceTypes Devices; // supported devices
    }

    public interface IMinigame
    {
        internal MinigameInfo Info { get; }

        public void Start(in GameContext gameContext, ContainerVisual rootVisual, ExpInputSite inputSite);

        public MinigameState Update(in GameContext gameContext);

        public void End(in GameContext gameContext, in MinigameState finalState);
    }
}