using GameForestCore.Common;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace GameForestCore.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IGFXUserService" in both code and config file together.
    [ServiceContract]
    public interface IGFXUserService
    {
        [OperationContract, WebInvoke(UriTemplate = "/user/{username}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetUserInfo(string username);

        [OperationContract, WebInvoke(UriTemplate = "/user/{username}?firstname={firstname}&lastname={lastname}&description={description}&usersessionid={usersessionid}", ResponseFormat = WebMessageFormat.Json, Method = "PUT")]
        GFXRestResponse SetUserInfo(string username, string firstname, string lastname, string description, string usersessionid);

        [OperationContract, WebInvoke(UriTemplate = "/user/login?username={username}&password={password}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse Login(string username, string password);

        [OperationContract, WebInvoke(UriTemplate = "/user/login?usersessionid={usersessionid}", ResponseFormat = WebMessageFormat.Json, Method = "DELETE")]
        GFXRestResponse Logout(string usersessionid);

        [OperationContract, WebInvoke(UriTemplate = "/user/changepassword?usersessionid={usersessionid}&oldpassword={oldpassword}&newpassword={newpassword}", ResponseFormat = WebMessageFormat.Json, Method = "PUT")]
        GFXRestResponse ChangePassword(string usersessionid, string oldpassword, string newpassword);

        [OperationContract, WebInvoke(UriTemplate = "/user/login?usersessionid={usersessionid}&heartbeattime={heartbeattime}", ResponseFormat = WebMessageFormat.Json, Method = "PUT")]
        GFXRestResponse Heartbeat(string usersessionid, string heartbeattime);

        [OperationContract, WebInvoke(UriTemplate = "/user/register?username={username}&password={password}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse Register(string username, string password);

        [OperationContract, WebInvoke(UriTemplate = "/user/register?username={username}&password={password}", ResponseFormat = WebMessageFormat.Json, Method = "DELETE")]
        GFXRestResponse Unregister(string username, string password);
    }
}
