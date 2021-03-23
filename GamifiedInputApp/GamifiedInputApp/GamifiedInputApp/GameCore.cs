using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        GameCore()
        {
            context.state = GameState.Menu;
            context.timer = new GameTimer();
        }
    }
}
