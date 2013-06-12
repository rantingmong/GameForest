using System.ServiceModel;
using System.ServiceModel.Activation;

namespace GameForestCore.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GFXLoginService : IGFXLoginService
    {
        public string Login(string username, string password)
        {
            return "Hello " + username;
        }
    }
}
