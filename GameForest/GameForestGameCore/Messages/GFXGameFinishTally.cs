using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client indicating who won the game.
    /// </summary>
    public class GFXGameFinishTally : GFXSocketListener
    {
        public override string              Subject
        {
            get { return "GFX_TALLY"; }
        }

        public override GFXSocketResponse   DoMessage   (GFXServerCore server, GFXSocketInfo info, Fleck.IWebSocketConnection ws)
        {
            throw new NotImplementedException();
        }
    }
}
