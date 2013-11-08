using Fleck;
using GameForestDatabaseConnector.Logger;
using System;
using System.Collections.Generic;

namespace GameForestCoreWebSocket.Messages
{
    public class GFXPingRespond : GFXSocketListener
    {
        public override string Subject
        {
            get { return "GFX_PING_RESPOND"; }
        }

        public override GFXSocketResponse DoMessage(GFXServerCore server, GFXSocketInfo info, IWebSocketConnection ws)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.WARN, "PING", "Response recevied! " + info.SessionId);

            // update the ping data
            var webSocketEntry          = server.WebSocketList[info.SessionId];
                webSocketEntry.lastPing = DateTime.Now;
                webSocketEntry.pingSent = false;

            return constructResponse(GFXResponseType.Normal, "");
        }
    }
}
