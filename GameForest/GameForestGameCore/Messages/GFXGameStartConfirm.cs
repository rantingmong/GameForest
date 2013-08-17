using Fleck;
using GameForestCore.Database;
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
            get { throw new NotImplementedException(); }
        }

        public override GFXSocketResponse   DoMessage   (GFXServerCore server, GFXSocketInfo info, IWebSocketConnection ws)
        {
            try
            {
                // get the lobby the player is in
                List<GFXLobbySessionRow> lobby = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("SessionId = {0}", info.SessionId)));

                if (lobby.Count <= 0)
                    return constructResponse(GFXResponseType.InvalidInput, "User is not playing any games!");

                GFXLobbySessionRow ownerPlayer = lobby[0];
                ownerPlayer.Ready = true;

                server.LobbySessionList.Update(string.Format("RowId = {0}", ownerPlayer.RowId), ownerPlayer);

                // send message to other connected players
                bool gameReallyStart = true;
                List<GFXLobbySessionRow> players = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = {0}", lobby[0].SessionID)));

                foreach (var player in players)
                {
                    if (player.Ready == false)
                    {
                        gameReallyStart = false;
                        break;
                    }
                }

                // send a GFX_START message to all clients
                if (gameReallyStart)
                {
                    GFXGameData gameData = new GFXGameData(Guid.Empty);

                    // create the data store for the game
                    server.GameDataList.Add(ownerPlayer.LobbyID, gameData);

                    foreach (var player in players)
                    {
                        gameData.UserData.Add(player.SessionID, "");
                        server.WebSocketList[player.SessionID].Send("TODO: SEND GFX_START MESSAGE");

                        if (player.Owner)
                        {
                            // send a GFX_TURN_RESOLVE to the owner of the lobby
                            server.WebSocketList[player.SessionID].Send("TODO: SEND GFX_TURN_RESOLVE MESSAGE");
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
