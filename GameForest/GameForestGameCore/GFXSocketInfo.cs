using System;

namespace GameForestCoreWebSocket
{
    public struct GFXSocketInfo
    {
        public Guid     ConnectionId    { get; set; }
        public Guid     SessionId       { get; set; }

        public string   Subject         { get; set; }

        public string   Message         { get; set; }
    }
}
