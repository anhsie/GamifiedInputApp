using Microsoft.UI.Composition;


namespace GamifiedInputApp.Minigame
{
    enum MinigameState
    {
        Play,
        Pass,
        Fail
    }

    interface IMinigame
    {
        public void setup(in GameContext gameContext, ContainerVisual rootVisual);

        public void start(in GameContext gameContext);

        public MinigameState update(in GameContext gameContext);

        public void end(in GameContext gameContext);
    }
}