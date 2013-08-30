using GameForestCore.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client informing there will be a game state modification/addition.
    /// </summary>
    public class GFXGameSendUserData : GFXSocketListener
    {
        public override string              Subject
        {
            get { return "GFX_SEND_USER_DATA"; }
        }

        public override GFXSocketResponse   DoMessage   (GFXServerCore server, GFXSocketInfo info, Fleck.IWebSocketConnection ws)
        {
            try
            {
                // get the lobby the player is in
                List<GFXLobbySessionRow> lobbySessions = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("SessionId = '{0}'", info.SessionId)));

                if (lobbySessions.Count <= 0)
                    return constructResponse(GFXResponseType.InvalidInput, "User is not playing any games!");

                GFXLobbySessionRow currentPlayer = lobbySessions[0];

                // get the data store
                GFXGameData dataStore = server.GameDataList[currentPlayer.LobbyID];

                if (dataStore.UserData.ContainsKey(currentPlayer.SessionID))
                {
                    dataStore.UserData[currentPlayer.SessionID] = info.Message;

                    // send message to other connected players
                    List<GFXLobbySessionRow> players = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}'", lobbySessions[0].SessionID)));

                    foreach (var player in players)
                    {
                        server.WebSocketList[player.SessionID].Send(JsonConvert.SerializeObject(new GFXSocketResponse
                            {
                                Subject = "GFX_DATA_CHANGED",
                                Message = JsonConvert.SerializeObject(dataStore),
                                ResponseCode = GFXResponseType.Normal
                            }));
                    }
                }
                else
                {
                    return constructResponse(GFXResponseType.DoesNotExist, "User is not in this lobby!");
                }

                return constructResponse(GFXResponseType.Normal, "");
            }
            catch (Exception exp)
            {
                return constructResponse(GFXResponseType.FatalError, exp.Message);
            }
        }
    }
}
