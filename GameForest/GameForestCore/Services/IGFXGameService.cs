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
    }
}
