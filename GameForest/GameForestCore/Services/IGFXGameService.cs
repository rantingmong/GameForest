using GameForestCore.Common;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace GameForestCore.Services
{
    [ServiceContract]
    interface IGFXGameService
    {
        [OperationContract, WebInvoke(UriTemplate = "/game?maxcount={maxCount}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetGameList(int maxCount);

        [OperationContract, WebInvoke(UriTemplate = "/game/{gameId}", ResponseFormat = WebMessageFormat.Json, Method = "GET")]
        GFXRestResponse GetGameInfo(string gameId);
    }
}
