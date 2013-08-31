using System;

namespace GameForestCoreWebSocket
{
    public struct GFXSocketInfo
    {
        /// <summary>
        /// Gets or sets the websocket connection id. (To be given by the server when the user connects)
        /// </summary>
        public Guid     ConnectionId    { get; set; }
        /// <summary>
        /// Gets or sets the user's session id.
        /// </summary>
        public Guid     SessionId       { get; set; }

        public string   Subject         { get; set; }

        public string   Message         { get; set; }
    }
}
