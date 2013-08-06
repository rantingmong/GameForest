using System;

namespace GameForestCoreWebSocket
{
    public abstract class GFXSocketListener
    {
        public abstract string  Subject { get; }

        public abstract void    DoMessage       (GFXSocketInfo info);
    }
}
