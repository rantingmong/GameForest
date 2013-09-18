using GameForestCore.Common;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace GameForestCore.Services
{
    [ServiceContract]
    public interface IGFXGameService
    {
        [OperationContract, WebInvoke(UriTemplate = "?maxcount={maxCount}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetGameList(int maxCount);

        [OperationContract, WebInvoke(UriTemplate = "/{gameId}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetGameInfo(string gameId);

        [OperationContract, WebInvoke(UriTemplate = "?getstat={stat}&gameid={gameid}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetStat(string stat, string gameId);

        [OperationContract, WebInvoke(UriTemplate = "?name={name}&description={description}&minplayers={minPlayers}&maxplayers={maxPlayers}&usersessionid={usersessionid}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse CreateGame(string name, string description, int minPlayers, int maxPlayers, string usersessionid);

        [OperationContract, WebInvoke(UriTemplate = "?addstat={statname}&gameid={gameId}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse AddStat(string statname, string gameId);

        [OperationContract, WebInvoke(UriTemplate = "?updatestat={statId}&gameid={gameId}&statvalue={stat_value}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse UpdateStat(string statId, string gameId, int stat_value);
    }
}
