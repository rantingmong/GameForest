using GameForestCore.Common;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace GameForestCore.Services
{
    [ServiceContract]
    public interface IGFXGameService
    {
        [OperationContract, WebInvoke(UriTemplate = "?maxcount={maxCount}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetGameList     (int maxCount);

        [OperationContract, WebInvoke(UriTemplate = "/gamelist/{userid}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetUserGameList (string userId);

        [OperationContract, WebInvoke(UriTemplate = "/{gameId}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetGameInfo     (string gameId);

        [OperationContract, WebInvoke(UriTemplate = "?name={name}&description={description}&minplayers={minPlayers}&maxplayers={maxPlayers}&usersessionid={usersessionid}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse CreateGame      (string name, string description, int minPlayers, int maxPlayers, string usersessionid);

        [OperationContract, WebInvoke(UriTemplate = "?name={name}&description={description}&minplayers={minPlayers}&maxplayers={maxPlayers}&usersessionid={usersessionid}&gameid={gameid}", ResponseFormat = WebMessageFormat.Json, Method = "PUT")]
        GFXRestResponse UpdateGame      (string name, string description, int minPlayers, int maxPlayers, string usersessionid, string gameid);

        [OperationContract, WebInvoke(UriTemplate = "?gameid={gameid}&usersessionid={usersessionid}", ResponseFormat = WebMessageFormat.Json, Method = "DELETE")]
        GFXRestResponse DeleteGame      (string usersessionid, string gameid);
    }
}
