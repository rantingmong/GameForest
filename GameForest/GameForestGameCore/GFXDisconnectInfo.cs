using System;

namespace GameForestCoreWebSocket
{
    public struct GFXDisconnectInfo
    {
        public Guid UserId      { get; set; }

        public Guid SessionId   { get; set; }

        public Guid LobbyId     { get; set; }
    }
}
