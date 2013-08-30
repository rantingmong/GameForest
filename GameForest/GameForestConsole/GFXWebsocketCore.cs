using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GameForestCoreWebSocket;
using GameForestDatabaseConnector.Logger;

namespace GameForestConsole
{
    public class GFXWebsocketCore
    {
        private GFXServerCore serverCore;

        private bool isRunning = true;
        private Thread wsThread;

        public GFXWebsocketCore()
        {
            wsThread = new Thread(new ThreadStart(() =>
                {
                    serverCore = new GFXServerCore();

                    while (isRunning)
                    {
                        Thread.Sleep(100);
                    }
                }));
        }

        public void Start()
        {
            wsThread.Start();
        }

        public void Stop()
        {
            isRunning = false;
        }
    }
}
