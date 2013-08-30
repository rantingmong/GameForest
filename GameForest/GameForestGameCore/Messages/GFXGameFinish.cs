using GameForestCore.Database;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GameForestCoreWebSocket.Messages
{
    /// <summary>
    /// Message sent by the client informing the game is finished!
    /// </summary>
    public class GFXGameFinish : GFXSocketListener
    {
        public override string              Subject
        {
            get { return "GFX_GAME_FINISH"; }
        }

        public override GFXSocketResponse   DoMessage   (GFXServerCore server, GFXSocketInfo info, Fleck.IWebSocketConnection ws)
        {
            // get user's lobbyid
            List<GFXLobbySessionRow> sessions = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("SessionId = '{0}'", info.SessionId)));

            if (sessions.Count <= 0)
                return constructResponse(GFXResponseType.DoesNotExist, "User is not playing any games!");

            List<GFXLobbySessionRow> players = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}'", sessions[0].LobbyID)));

            foreach (var player in players)
            {
                // send GFX_GAME_FINISHED
                server.WebSocketList[player.SessionID].Send(JsonConvert.SerializeObject(new GFXSocketResponse
                    {
                        ResponseCode    = GFXResponseType.Normal,
                        Subject         = "GFX_GAME_FINISHED",
                        Message         = ""
                    }));
            }

            return constructResponse(GFXResponseType.Normal, "");
        }
    }
}
