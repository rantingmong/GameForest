﻿using System;

namespace GameForestCoreWebSocket
{
    public abstract class GFXSocketListener
    {
        public abstract string              Subject             { get; }

        public abstract GFXSocketResponse   DoMessage           (GFXSocketInfo info);

        protected       GFXSocketResponse   constructResponse   (GFXResponseType response, string payload)
        {
            return new GFXSocketResponse
            {
                Message         = payload,
                Subject         = Subject,
                ResponseCode    = response
            };
        }
    }
}
