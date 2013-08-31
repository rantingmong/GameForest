using GameForestCore.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client informing the server the player's order has changed.
    /// </summary>
    public class GFXConfirmTurn : GFXSocketListener
    {
        public override string              Subject
        {
            get { return "GFX_CONFIRM_TURN"; }
        }

        public override GFXSocketResponse   DoMessage   (GFXServerCore server, GFXSocketInfo info, Fleck.IWebSocketConnection ws)
        {
            try
            {
                // get the lobby the player is in
                List<GFXLobbySessionRow> lobbySessions = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("SessionId = '{0}'", info.SessionId)));

                if (lobbySessions.Count <= 0)
                    return constructResponse(GFXResponseType.InvalidInput, "User is not playing any games!");

                // modify this user's status to 2
                GFXLobbySessionRow currentPlayer    = lobbySessions[0];
                
                currentPlayer.Status    = 2;
                currentPlayer.Order     = Convert.ToInt32(info.Message);

                server.LobbySessionList.Update(string.Format("RowId = '{0}'", currentPlayer.RowId), currentPlayer);
                
                var checkPlayers    = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}'", currentPlayer.LobbyID)));
                var allOkay         = true;

                foreach (var player in checkPlayers)
                {
                    if (player.Status != 2)
                    {
                        allOkay = false;
                    }
                }

                if (allOkay)
                {
                    // if all user's status are 2, change lobby state to playing
                    var lobby           = new List<GFXLobbyRow>(server.LobbyList.Select(string.Format("LobbyId = '{0}'", currentPlayer.LobbyID)))[0];
                        lobby.Status    = GFXLobbyStatus.Playing;

                    server.LobbyList.Update(string.Format("LobbyId = '{0}'", lobby.LobbyID), lobby);

                    // send a GFX_START_GAME to all clients
                    foreach (var player in checkPlayers)
                    {
                        server.WebSocketList[player.SessionID].Send(JsonConvert.SerializeObject(new GFXSocketResponse
                            {
                              Subject       = "GFX_START_GAME",
                              Message       = "",
                              ResponseCode  = GFXResponseType.Normal
                            }));
                    }

                    // send a GFX_TURN_START to the first player and GFX_TURN_CHANGED to other players
                    var order = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}' ORDER BY PlayerOrder ASC", currentPlayer.LobbyID)));

                    server.WebSocketList[order[0].SessionID].Send(JsonConvert.SerializeObject(new GFXSocketResponse
                        {
                            Subject         = "GFX_TURN_START",
                            Message         = "",
                            ResponseCode    = GFXResponseType.Normal
                        }));

                    for (int i = 1; i < order.Count; i++)
                    {
                        server.WebSocketList[order[i].SessionID].Send(JsonConvert.SerializeObject(new GFXSocketResponse
                            {
                                Subject         = "GFX_TURN_CHANGED",
                                Message         = "",
                                ResponseCode    = GFXResponseType.Normal
                            }));
                    }
                }
                else
                {
                    // send a GFX_TURN_RESOLVE to the next player
                    var nextPlayers = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}' AND PlayerOrderOriginal > {1} ORDER BY PlayerOrderOriginal ASC", currentPlayer.LobbyID, currentPlayer.OrderOriginal)));

                    if (nextPlayers.Count <= 0)
                    {
                        // get the first person
                        nextPlayers = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}' ORDER BY PlayerOrderOriginal ASC", currentPlayer.LobbyID)));
                    }

                    GFXLobbySessionRow nextPlayer = nextPlayers[0];

                    server.WebSocketList[nextPlayer.SessionID].Send(JsonConvert.SerializeObject(new GFXSocketResponse
                    {
                        Subject         = "GFX_TURN_RESOLVE",
                        Message         = nextPlayer.Order.ToString(),
                        ResponseCode    = GFXResponseType.Normal
                    }));
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
