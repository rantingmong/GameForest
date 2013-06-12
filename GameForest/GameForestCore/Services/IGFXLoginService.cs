using System.ServiceModel;
using System.ServiceModel.Web;

namespace GameForestCore.Services
{
    [ServiceContract]
    public interface IGFXLoginService
    {
        /// <summary>
        /// Logs the user in the service.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/login?username={username}&password={password}")]
        string Login(string username, string password);
    }
}
