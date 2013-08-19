﻿using GameForestCore.Common;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace GameForestCore.Services
{
    [ServiceContract]
    public interface IGFXLobbyService
    {
        [OperationContract, WebInvoke(UriTemplate = "?maxcount={maxcount}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetLobbies(int maxcount);

        [OperationContract, WebInvoke(UriTemplate = "/{lobbyid}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetLobby(string lobbyid);

        [OperationContract, WebInvoke(UriTemplate = "?name={lobbyname}&gameid={gameid}&password={password}&usersessionid={usersessionid}&private={isprivate}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse CreateLobby(string lobbyname, string gameid, string password, string usersessionid, bool isprivate);

        [OperationContract, WebInvoke(UriTemplate = "/join?lobbyid={lobbyid}&usersessionid={usersessionid}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse JoinLobby(string lobbyid, string usersessionid);

        [OperationContract, WebInvoke(UriTemplate = "/join?usersessionid={usersessionid}", ResponseFormat = WebMessageFormat.Json, Method = "DELETE")]
        GFXRestResponse LeaveLobby(string usersessionid);

        [OperationContract, WebInvoke(UriTemplate = "/users?lobbyid={lobbyid}&usersessionid={usersessionid}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetUserList(string lobbyid, string usersessionid);
    }
}
