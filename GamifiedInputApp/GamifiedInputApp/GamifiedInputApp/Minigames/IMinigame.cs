using Microsoft.UI.Composition;
using Windows.Devices.Input;

namespace GamifiedInputApp.Minigames
{
    enum SupportedDeviceTypes
    {
        Mouse = PointerDeviceType.Mouse,
        Touch = PointerDeviceType.Touch,
        Pen = PointerDeviceType.Pen,
        Pointer, // Touch + Pen
        Spatial, // Mouse + Touch + Pen
        Keyboard, // Keyboard
        All, // Mouse + Touch + Pen + Keyboard
    }

    enum MinigameState
    {
        Play,
        Pass,
        Fail
    }

    class MinigameInfo
    {
        public MinigameInfo(IMinigame minigame, string name, SupportedDeviceTypes devices)
        {
            Minigame = minigame;
            Name = name;
            Devices = devices;
        }

        public override string ToString() { return Name; }

        public readonly IMinigame Minigame;
        public readonly string Name; // name of miningame (e.g. "PressTheEnterKey! (KeyDown/KeyUp)")
        public readonly SupportedDeviceTypes Devices; // supported devices
    }

    interface IMinigame
    {
        internal MinigameInfo Info { get; }

        public void Start(in GameContext gameContext, ContainerVisual rootVisual);

        public MinigameState Update(in GameContext gameContext);

        public void End(in GameContext gameContext, in MinigameState finalState);
    }
}