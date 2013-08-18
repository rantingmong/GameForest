using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client informing the game is finished!
    /// </summary>
    public class GFXGameFinish : GFXSocketListener
    {
        public override string              Subject
        {
            get { return "GFX_GAME_FINISH"; }
        }

        public override GFXSocketResponse   DoMessage   (GFXServerCore server, GFXSocketInfo info, Fleck.IWebSocketConnection ws)
        {
            throw new NotImplementedException();
        }
    }
}
