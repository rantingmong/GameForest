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
        GFXRestResponse GetLobbies(int maxcount);

        [OperationContract, WebInvoke(UriTemplate = "/lobby/{lobbyid}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetLobby(string lobbyid);

        [OperationContract, WebInvoke(UriTemplate = "/lobby?name={lobbyname}&gameid={gameid}&password={password}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse CreateLobby(string lobbyname, string gameid, string password);


    }
}
