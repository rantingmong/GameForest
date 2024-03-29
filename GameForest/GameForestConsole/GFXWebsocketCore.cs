﻿using System;
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
        private GFXChatCore chatCore;

        private bool isRunning = false;
        private Thread wsThread;

        public bool IsRunning
        {
            get { return isRunning; }
        }

        public GFXWebsocketCore()
        {
            wsThread = new Thread(new ThreadStart(() =>
                {
                    serverCore = new GFXServerCore();
                    chatCore = new GFXChatCore("localhost", "8085");

                    while (isRunning)
                    {
                        Thread.Sleep(100);
                    }
                }));
        }

        public void Start()
        {
            isRunning = true;
            wsThread.Start();
        }

        public void Stop()
        {
            isRunning = false;
        }
    }
}
