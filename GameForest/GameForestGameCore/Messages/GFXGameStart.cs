using Fleck;
using System;
using System.Collections.Generic;

using GameForestCore.Database;
using Newtonsoft.Json;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client informing the game should be started.
    /// </summary>
    public class GFXGameStart : GFXSocketListener
    {
        public override string              Subject
        {
            get { return "GFX_GAME_START"; }
        }

        public override GFXSocketResponse   DoMessage   (GFXServerCore server, GFXSocketInfo info, IWebSocketConnection ws)
        {
            try
            {
                // get the lobby the player is in
                List<GFXLobbySessionRow> lobbySessions = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("SessionId = {0}", info.SessionId)));

                if (lobbySessions.Count <= 0)
                    return constructResponse(GFXResponseType.InvalidInput, "User is not playing any games!");

                // send message to other connected players
                List<GFXLobbySessionRow> players = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = {0}", lobbySessions[0].SessionID)));

                foreach (var player in players)
                {
                    if (player.SessionID != info.SessionId)
                    {
                        server.WebSocketList[player.SessionID].Send(JsonConvert.SerializeObject(new GFXSocketSend
                            {
                                Message = "GFX_GAME_START",
                                Payload = ""
                            }));
                    }
                }

                GFXLobbySessionRow currentPlayer = lobbySessions[0];
                currentPlayer.Status = 1;

                server.LobbySessionList.Update(string.Format("RowId = {0}", currentPlayer.RowId), currentPlayer);
                
                return constructResponse(GFXResponseType.Normal, "");
            }
            catch (Exception exp)
            {
                return constructResponse(GFXResponseType.FatalError, exp.Message);
            }
        }
    }
}
