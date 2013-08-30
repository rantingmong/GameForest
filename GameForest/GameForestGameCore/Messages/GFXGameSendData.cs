using Fleck;
using GameForestCore.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client informing the game state data should be updated.
    /// </summary>
    public class GFXGameSendData : GFXSocketListener
    {
        public override string              Subject
        {
            get { return "GFX_SEND_DATA"; }
        }

        public override GFXSocketResponse   DoMessage   (GFXServerCore server, GFXSocketInfo info, IWebSocketConnection ws)
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

                if (dataStore.CurrentUserSession == currentPlayer.SessionID)
                {
                    GFXGameDataEntry entry = JsonConvert.DeserializeObject<GFXGameDataEntry>(info.Message);

                    dataStore.Data[entry.Key] = entry.Data;

                    // send message to other connected players
                    List<GFXLobbySessionRow> players = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}'", lobbySessions[0].LobbyID)));

                    string gameData = JsonConvert.SerializeObject(dataStore);

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
                    return constructResponse(GFXResponseType.InvalidInput, "This player cannot modify the game data store.");
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
