using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition;

namespace GamifiedInputApp
{
    enum GameState
    {
        Menu,
        Minigame,
        Results
    }

    struct GameContext
    {
        public GameState state; // current game state
        public GameTimer timer; // minigame timer
    }

    class GameCore
    {
        GameContext context;
        ContainerVisual rootVisual;

        public GameCore(ContainerVisual rootVisual)
        {
            context.state = GameState.Menu;
            context.timer = new GameTimer();
            this.rootVisual = rootVisual;
        }
    }
}
