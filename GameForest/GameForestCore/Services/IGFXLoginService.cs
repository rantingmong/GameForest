using System.ServiceModel;
using System.ServiceModel.Web;

using GameForestCore.Common;

namespace GameForestCore.Services
{
    [ServiceContract]
    public interface IGFXLoginService
    {
        [OperationContract, WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/login?username={username}&password={password}")]
        GFXRestResponse Login       (string username, string password);
        
        [OperationContract, WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/register?username={username}&password={password}")]
        GFXRestResponse Register    (string username, string password);
        
        [OperationContract, WebInvoke(Method = "DELETE", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/login?usersessionid={usersessionid}")]
        GFXRestResponse Logout      (string usersessionid);

        [OperationContract, WebInvoke(Method = "DELETE", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/register?usersessionid={usersessionid}&password={password}")]
        GFXRestResponse Unregister  (string usersessionid, string password);

        [OperationContract, WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/heartbeat?usersessionid={usersessionid}")]
        GFXRestResponse Heartbeat   (string usersessionid);

        [OperationContract, WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/updateinfo?usersessionid={usersessionid}&firstname={firstname}&lastname={lastname}&description={description}")]
        GFXRestResponse UpdateInfo  (string usersessionid, string firstname, string lastname, string description);
        
        [OperationContract, WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, UriTemplate = "/updatepword?usersessionid={usersessionid}&password={newpassword}&oldpassword={curpassword}")]
        GFXRestResponse UpdatePword (string usersessionid, string newpassword, string curpassword);
    }
}
