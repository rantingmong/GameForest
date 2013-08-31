using System;
using System.Collections.Generic;

using System.ServiceModel;
using System.ServiceModel.Configuration;

using GameForestCore;
using GameForestCore.Common;
using GameForestCore.Services;

using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Threading;
using System.ServiceModel.Web;
using GameForestCore.Database;
using GameForestDatabaseConnector.Logger;

namespace GameForestConsole
{
    public class GFXRestServerCore
    {
        private bool isStarted = false;

        private WebServiceHost serviceHostUser;
        private WebServiceHost serviceHostGame;
        private WebServiceHost serviceHostLobi;

        public event EventHandler OnServerRestStop;
        public event EventHandler OnServerRestStart;
        
        public bool IsStarted
        {
            get { return isStarted; }
        }

        public GFXRestServerCore()
        {
            GFXDatabaseCore.Initialize("Server=localhost;Database=GameForest;Uid=root;Pwd=1234;");

            ServiceMetadataBehavior behavior = new ServiceMetadataBehavior
            {
                HttpGetEnabled      = true,
                HttpsGetEnabled     = true
            };

            serviceHostUser = new WebServiceHost(new GFXUserService(), new Uri("http://localhost:1193/service/user"));
            serviceHostGame = new WebServiceHost(new GFXGameService(), new Uri("http://localhost:1193/service/game"));
            serviceHostLobi = new WebServiceHost(new GFXLobbyService(), new Uri("http://localhost:1193/service/lobby"));

            var uEndpoint = serviceHostUser.AddServiceEndpoint(typeof(GameForestCore.Services.IGFXUserService), new WebHttpBinding(), "");
            var gEndpoint = serviceHostGame.AddServiceEndpoint(typeof(GameForestCore.Services.IGFXGameService), new WebHttpBinding(), "");
            var lEndpoint = serviceHostLobi.AddServiceEndpoint(typeof(GameForestCore.Services.IGFXLobbyService), new WebHttpBinding(), "");

            uEndpoint.Behaviors.Add(new WebHttpBehavior());
            gEndpoint.Behaviors.Add(new WebHttpBehavior());
            lEndpoint.Behaviors.Add(new WebHttpBehavior());
            
            serviceHostUser.Description.Behaviors.Add(behavior);
            serviceHostGame.Description.Behaviors.Add(behavior);
            serviceHostLobi.Description.Behaviors.Add(behavior);
        }

        public void Start()
        {
            isStarted = true;

            var thread = new Thread(new ThreadStart(() =>
                {
                    serviceHostUser.Open();
                    serviceHostGame.Open();
                    serviceHostLobi.Open();

                    if (OnServerRestStart != null)
                        OnServerRestStart(this, EventArgs.Empty);

                    while(isStarted)
                    {
                        Thread.Sleep(100);
                    }

                    serviceHostUser.Close();
                    serviceHostGame.Close();
                    serviceHostLobi.Close();

                    if (OnServerRestStop != null)
                        OnServerRestStop(this, EventArgs.Empty);
                }));

            thread.Start();
        }

        public void Stop()
        {
            isStarted = false;
        }
    }
}
