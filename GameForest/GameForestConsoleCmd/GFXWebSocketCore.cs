using GameForestCoreWebSocket;
using System.Threading;

namespace GameForest
{
    public class GFXWebsocketCore
    {
        private GFXServerCore   serverCore;

        private bool            isRunning           = false;
        private Thread          wsThread;

        public bool             IsRunning
        {
            get { return isRunning; }
        }

        public                  GFXWebsocketCore    ()
        {
            wsThread = new Thread(new ThreadStart(() =>
            {
                serverCore = new GFXServerCore("game-forest.cloudapp.net");

                while (isRunning)
                {
                    Thread.Sleep(100);
                }
            }));
        }

        public void             Start               ()
        {
            isRunning = true;
            wsThread.Start();
        }

        public void             Stop                ()
        {
            isRunning = false;
        }
    }
}
