using Fleck;
using GameForestCore.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client informing the client has acknowledging the game should be started.
    /// </summary>
    public class GFXGameStartConfirm : GFXSocketListener
    {
        public override string              Subject
        {
            get { return "GFX_GAME_START_CONFIRM"; }
        }

        public override GFXSocketResponse   DoMessage   (GFXServerCore server, GFXSocketInfo info, IWebSocketConnection ws)
        {
            try
            {
                // get the lobby the player is in
                List<GFXLobbySessionRow> lobbySessions = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("SessionId = {0}", info.SessionId)));

                if (lobbySessions.Count <= 0)
                    return constructResponse(GFXResponseType.InvalidInput, "User is not playing any games!");

                GFXLobbySessionRow currentPlayer = lobbySessions[0];
                currentPlayer.Status = 1;

                server.LobbySessionList.Update(string.Format("RowId = {0}", currentPlayer.RowId), currentPlayer);

                // send message to other connected players
                bool gameReallyStart = true;
                List<GFXLobbySessionRow> players = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = {0}", currentPlayer.SessionID)));

                foreach (var player in players)
                {
                    if (player.Status == 0)
                    {
                        gameReallyStart = false;
                        break;
                    }
                }

                // send a GFX_START message to all clients
                if (gameReallyStart)
                {
                    List<GFXLobbyRow> lobbies = new List<GFXLobbyRow>(server.LobbyList.Select(string.Format("LobbyId = {0}",currentPlayer.LobbyID)));

                    if (lobbies.Count <= 0)
                    {
                        throw new InvalidProgramException("Hm. Weird bug.");
                    }

                    GFXLobbyRow lobby = lobbies[0];
                    lobby.Status = GFXLobbyStatus.ChoosingTurn;

                    server.LobbyList.Update(string.Format("LobbyId = {0}", lobby.LobbyID), lobby);

                    GFXGameData gameData = new GFXGameData(Guid.Empty);

                    // create the data store for the game
                    server.GameDataList.Add(currentPlayer.LobbyID, gameData);

                    foreach (var player in players)
                    {
                        gameData.UserData.Add(player.SessionID, "");
                        server.WebSocketList[player.SessionID].Send(JsonConvert.SerializeObject(new GFXSocketSend
                            {
                                Message = "GFX_START",
                                Payload = ""
                            }));

                        if (player.Owner)
                        {
                            // send a GFX_TURN_RESOLVE to the owner of the lobby
                            server.WebSocketList[player.SessionID].Send(JsonConvert.SerializeObject(new GFXSocketSend
                                {
                                    Message = "GFX_TURN_RESOLVE",
                                    Payload = ""
                                }));
                        }
                    }
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
