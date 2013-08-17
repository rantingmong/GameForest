using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client informing the server the player's order has changed.
    /// </summary>
    public class GFXConfirmTurn : GFXSocketListener
    {
        public override string Subject
        {
            get { return "GFX_CONFIRM_TURN"; }
        }

        public override GFXSocketResponse DoMessage(GFXServerCore server, GFXSocketInfo info, Fleck.IWebSocketConnection ws)
        {
            throw new NotImplementedException();
        }
    }
}
