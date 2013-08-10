using Fleck;
using System;
using System.Collections.Generic;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client informing the game should be started.
    /// </summary>
    public class GFXGameStart : GFXSocketListener
    {
        public override string              Subject
        {
            get { return "GFX_GAME_START"; }
        }

        public override GFXSocketResponse   DoMessage   (GFXSocketInfo info, IWebSocketConnection ws)
        {
            // send message to other connected players

            return constructResponse(GFXResponseType.Normal, "");
        }
    }
}
