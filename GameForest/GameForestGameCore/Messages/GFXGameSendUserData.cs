using GameForestCore.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
<<<<<<< HEAD
                    var players     = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}'", currentPlayer.LobbyID)));
                    var loggedIn    = new List<GFXLoginRow>(server.LoginList.Select(string.Format("SessionId = '{0}'", currentPlayer.SessionID)));
                    var curUser     = new List<GFXUserRow>(server.UserList.Select(string.Format("UserId = '{0}'", loggedIn[0].UserId)));

                    var returnType  = new Dictionary<string, object>();

                    returnType["User"] = JsonConvert.SerializeObject(curUser[0]);
                    returnType["Data"] = JsonConvert.SerializeObject(dataStore.UserData[currentPlayer.SessionID]);
=======
                    List<GFXLobbySessionRow> players = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}'", lobbySessions[0].LobbyID)));
>>>>>>> origin/analytics

                    foreach (var player in players)
                    {
                        server.WebSocketList[player.SessionID].Send(JsonConvert.SerializeObject(new GFXSocketResponse
                            {
                                Subject         = "GFX_USER_DATA_CHANGED",
                                Message         = JsonConvert.SerializeObject(returnType),
                                ResponseCode    = GFXResponseType.Normal
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
