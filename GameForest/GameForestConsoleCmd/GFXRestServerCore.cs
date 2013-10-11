using GameForestCore.Database;
using GameForestCore.Services;
using GameForestCore.Util;

using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Threading;

namespace GameForest
{
    public class GFXRestServerCore
    {
        private bool                isStarted               = false;

        private WebServiceHost      serviceHostUser;
        private WebServiceHost      serviceHostGame;
        private WebServiceHost      serviceHostLobi;
        private WebServiceHost      serviceHostStat;

        public event EventHandler   OnServerRestStop;
        public event EventHandler   OnServerRestStart;

        public bool                 IsStarted
        {
            get { return isStarted; }
        }

        public                      GFXRestServerCore       ()
        {
            ServiceMetadataBehavior behavior = new ServiceMetadataBehavior
            {
                HttpGetEnabled  = true,
                HttpsGetEnabled = true
            };

            serviceHostUser = new WebServiceHost(new GFXUserService(), new Uri("http://localhost:1193/service/user"));
            serviceHostGame = new WebServiceHost(new GFXGameService(), new Uri("http://localhost:1193/service/game"));
            serviceHostLobi = new WebServiceHost(new GFXLobbyService(), new Uri("http://localhost:1193/service/lobby"));
            serviceHostStat = new WebServiceHost(new GFXStatsService(), new Uri("http://localhost:1193/service/stats"));

            var httpBinding = new WebHttpBinding
            {
                CrossDomainScriptAccessEnabled = true
            };

            var uEndpoint = serviceHostUser.AddServiceEndpoint(typeof(GameForestCore.Services.IGFXUserService), httpBinding, "");
            var gEndpoint = serviceHostGame.AddServiceEndpoint(typeof(GameForestCore.Services.IGFXGameService), httpBinding, "");
            var lEndpoint = serviceHostLobi.AddServiceEndpoint(typeof(GameForestCore.Services.IGFXLobbyService), httpBinding, "");
            var sEndpoint = serviceHostStat.AddServiceEndpoint(typeof(GameForestCore.Services.IGFXStatsService), httpBinding, "");

            uEndpoint.Behaviors.Add(new WebHttpBehavior());
            gEndpoint.Behaviors.Add(new WebHttpBehavior());
            lEndpoint.Behaviors.Add(new WebHttpBehavior());
            sEndpoint.Behaviors.Add(new WebHttpBehavior());

            uEndpoint.Behaviors.Add(new BehaviorAttribute());
            gEndpoint.Behaviors.Add(new BehaviorAttribute());
            lEndpoint.Behaviors.Add(new BehaviorAttribute());
            sEndpoint.Behaviors.Add(new BehaviorAttribute());

            serviceHostUser.Description.Behaviors.Add(behavior);
            serviceHostGame.Description.Behaviors.Add(behavior);
            serviceHostLobi.Description.Behaviors.Add(behavior);
            serviceHostStat.Description.Behaviors.Add(behavior);
        }

        public void                 Start                   ()
        {
            isStarted = true;

            var thread = new Thread(new ThreadStart(() =>
            {
                serviceHostUser.Open();
                serviceHostGame.Open();
                serviceHostLobi.Open();
                serviceHostStat.Open();

                if (OnServerRestStart != null)
                    OnServerRestStart(this, EventArgs.Empty);

                while (isStarted)
                {
                    Thread.Sleep(100);
                }

                serviceHostUser.Close();
                serviceHostGame.Close();
                serviceHostLobi.Close();
                serviceHostStat.Close();

                if (OnServerRestStop != null)
                    OnServerRestStop(this, EventArgs.Empty);
            }));

            thread.Start();
        }

        public void                 Stop                    ()
        {
            isStarted = false;
        }
    }
}
