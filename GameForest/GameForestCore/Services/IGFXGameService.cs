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

        [OperationContract, WebInvoke(UriTemplate = "?name={0}&description={1}&minplayers={2}&maxplayers={3}&usersessionid={4}", ResponseFormat = WebMessageFormat.Json, Method = "POST")]
        GFXRestResponse CreateGame(string name, string description, int minPlayers, int maxPlayers, string usersessionid);
    }
}
