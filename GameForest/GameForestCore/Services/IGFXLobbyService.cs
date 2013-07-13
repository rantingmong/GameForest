using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using GameForestCore.Common;

namespace GameForestCore.Services
{
    [ServiceContract]
    public interface IGFXLobbyService
    {
        [OperationContract, WebInvoke(UriTemplate = "/lobby?maxcount={maxcount}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetLobbies  (int maxcount);

        [OperationContract, WebInvoke(UriTemplate = "/lobby/{lobbyid}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetLobby    (string lobbyid);

        [OperationContract, WebInvoke(UriTemplate = "/lobby?name={lobbyname}&gameid={gameid}&password={password}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse CreateLobby (string lobbyname, string gameid, string password);

        [OperationContract, WebInvoke(UriTemplate = "/lobby/join?lobbyid={lobbyid}&usersessionid={usersessionid}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse JoinLobby   (string lobbyid, string usersessionid);

        [OperationContract, WebInvoke(UriTemplate = "/lobby/join?usersessionid={usersessionid}", ResponseFormat = WebMessageFormat.Json, Method = "DELETE")]
        GFXRestResponse LeaveLobby  (string usersessionid);

        [OperationContract, WebInvoke(UriTemplate = "/lobby/kick?usersessionid={usersessionid}&useridtokick={useridtokick}", ResponseFormat = WebMessageFormat.Json, Method = "PUT")]
        GFXRestResponse KickLobby   (string usersessionid, string usertokick);
    }
}
