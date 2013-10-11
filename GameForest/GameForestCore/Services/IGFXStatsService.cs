using GameForestCore.Common;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace GameForestCore.Services
{
    [ServiceContract]
    public interface IGFXStatsService
    {
        [OperationContract, WebInvoke(UriTemplate = "/?getstat={stat}&gameid={gameId}&all={allcheck}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetStat(string stat, string gameId, bool allcheck);

        [OperationContract, WebInvoke(UriTemplate = "/user?getstat={stat}&gameid={gameId}&session={sessionId}&all={allcheck}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetUserStat(string stat, string gameId, string sessionId, bool allcheck);

        [OperationContract, WebInvoke(UriTemplate = "/stats?addstat={statname}&gameid={gameId}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse AddStat(string statname, string gameId);

        [OperationContract, WebInvoke(UriTemplate = "/users?addstat={statname}&gameid={gameId}&session={sessionId}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse AddUserStat(string statname, string gameId, string sessionId);

        [OperationContract, WebInvoke(UriTemplate = "/?updatestat={statname}&gameid={gameId}&statvalue={stat_value}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse UpdateStat(string statname, string gameId, int stat_value);

        [OperationContract, WebInvoke(UriTemplate = "/user?updatestat={statname}&gameid={gameId}&user={username}&statvalue={stat_value}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse UpdateUserStat(string statname, string gameId, string username, int stat_value);
    }
}
