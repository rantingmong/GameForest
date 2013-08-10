using Fleck;
using System;

namespace GameForestCoreWebSocket.Messages
{
    public class GFXStart : GFXSocketListener
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
