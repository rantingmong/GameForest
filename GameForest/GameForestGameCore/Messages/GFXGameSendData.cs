using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client informing the game state data should be updated.
    /// </summary>
    public class GFXGameSendData : GFXSocketListener
    {
        public override string Subject
        {
            get { throw new NotImplementedException(); }
        }

        public override GFXSocketResponse DoMessage(GFXSocketInfo info, IWebSocketConnection ws)
        {
            throw new NotImplementedException();
        }
    }
}
