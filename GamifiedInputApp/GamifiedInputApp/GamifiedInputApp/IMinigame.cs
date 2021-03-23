using Microsoft.UI.Composition;


namespace GamifiedInputApp.Minigames
{
    enum MinigameState
    {
        Play,
        Pass,
        Fail
    }

    interface IMinigame
    {
        public void Start(in GameContext gameContext, ContainerVisual rootVisual);

        public MinigameState Update(in GameContext gameContext);

        public void End(in GameContext gameContext, in MinigameState finalState);
    }
}