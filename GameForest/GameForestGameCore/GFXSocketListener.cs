using Fleck;
using GameForestDatabaseConnector.Logger;
using System;

namespace GameForestCoreWebSocket
{
    public abstract class GFXSocketListener
    {
        public abstract string              Subject             { get; }

        public abstract GFXSocketResponse   DoMessage           (GFXServerCore server, GFXSocketInfo info, IWebSocketConnection ws);

        protected       GFXSocketResponse   constructResponse   (GFXResponseType response, string payload)
        {
            if (string.IsNullOrEmpty(payload))
                GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, Subject, string.Format("Response: {0}", response));
            else
                GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, Subject, string.Format("Response: {0}, Payload: {1}", response, payload));

            return new GFXSocketResponse
            {
                Message         = payload,
                Subject         = Subject,
                ResponseCode    = response
            };
        }
    }
}
