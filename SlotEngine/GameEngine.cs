using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine
{
    public class GameEngine
    {
        public event EventHandler<EventArgs> GameResultsReady;
        public event EventHandler<EventArgs> BonusResultsReady;

        public GameEngine()
        {

        }

        public virtual void GameResults(EventArgs e)
        {
            EventHandler<EventArgs> handler = GameResultsReady;
            if (handler != null)
                handler(this, e);
        }

        public virtual void BonusResults(EventArgs e)
        {
            EventHandler<EventArgs> handler = BonusResultsReady;
            if (handler != null)
                handler(this, e);
        }

    }
}
