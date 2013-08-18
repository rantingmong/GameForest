using System;
using System.ServiceModel.Activation;
using System.Web.Routing;

using GameForestCore.Services;
using GameForestCore.Database;

namespace GameForestCore
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var strings = System.Configuration.ConfigurationManager.ConnectionStrings;

            if (strings["GameForestConnection"] != null)
                GFXDatabaseCore.Initialize(strings["GameForestConnection"].ConnectionString);

            RouteTable.Routes.Add(new ServiceRoute("", new WebServiceHostFactory(), typeof(GFXUserService)));
            RouteTable.Routes.Add(new ServiceRoute("", new WebServiceHostFactory(), typeof(GFXLobbyService)));
            RouteTable.Routes.Add(new ServiceRoute("", new WebServiceHostFactory(), typeof(GFXGameService)));
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}