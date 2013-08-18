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
        public override string              Subject
        {
            get { return "GFX_CONFIRM_TURN"; }
        }

        public override GFXSocketResponse   DoMessage   (GFXServerCore server, GFXSocketInfo info, Fleck.IWebSocketConnection ws)
        {
            // modify this user's status to 2

            // send a GFX_TURN_RESOLVE to the next player

            // if all user's status are 2, change lobby state to playing

            // send a GFX_START_GAME to all clients

            // send a GFX_TURN_START to the first player and GFX_TURN_CHANGED to other players

            return constructResponse(GFXResponseType.Normal, "");
        }
    }
}
