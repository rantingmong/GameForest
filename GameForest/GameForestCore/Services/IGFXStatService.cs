using GameForestCore.Common;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace GameForestCore.Services
{
    [ServiceContract]
    public interface IGFXStatService
    {

        [OperationContract, WebInvoke(UriTemplate = "/stats?getstat={stat}&gameid={gameid}&all={allcheck}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetStat(string stat, string gameId, bool allcheck);

        [OperationContract, WebInvoke(UriTemplate = "/stat?addstat={statname}&gameid={gameId}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse AddStat(string statname, string gameId);

        [OperationContract, WebInvoke(UriTemplate = "/stats?updatestat={statname}&gameid={gameId}&statvalue={stat_value}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse UpdateStat(string statname, string gameId, int stat_value);

    }
}