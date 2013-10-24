
using GameForestCoreWebSocket;
using System;
using System.Threading;

namespace GameForest
{
    public class GFXWebsocketCore
    {
        private GFXServerCore serverCore;
        private GFXChatCore chatCore;

        private bool isRunning = false;
        private Thread wsThreadCore;
        private Thread wsThreadChat;

        public bool IsRunning
        {
            get { return isRunning; }
        }

        public GFXWebsocketCore(string address = "localhost")
        {
            wsThreadCore = new Thread(new ThreadStart(() =>
            {
                serverCore = new GFXServerCore(address);

                while (isRunning)
                {
                    Thread.Sleep(100);
                }
            }));

            wsThreadChat = new Thread(new ThreadStart(() =>
            {
                chatCore = new GFXChatCore(address, "8085");
                chatCore.start();

                while (isRunning)
                {
                    Thread.Sleep(100);
                }
            }));
        }

        public void Start()
        {
            isRunning = true;

            wsThreadCore.Start();
            wsThreadChat.Start();
        }

        public void Stop()
        {
            isRunning = false;
        }
    }
}
