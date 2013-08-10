﻿using Fleck;
using System;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client informing the client has acknowledging the game should be started.
    /// </summary>
    public class GFXGameStartConfirm : GFXSocketListener
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
