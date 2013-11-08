using Fleck;
using GameForestCore.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GameForestCoreWebSocket.Messages
{
    public class GFXInformReconnect : GFXSocketListener
    {
        public override string Subject
        {
            get { return "GFX_INFORM_RECONNECT"; }
        }

        public override GFXSocketResponse DoMessage(GFXServerCore server, GFXSocketInfo info, IWebSocketConnection ws)
        {
            if (!server.WebSocketList.ContainsKey(info.SessionId))
            {
                // we check first if the server kicked this user.
                return constructResponse(GFXResponseType.DoesNotExist, "You've been kicked by the server.");
            }

            // get the old websocket information of this user (via the session id)
            var oldWSInfo           = server.WebSocketList[info.SessionId];

                oldWSInfo.webSocket     = ws;   // we now change it >:)
                oldWSInfo.disconnected  = false;
                oldWSInfo.checkCount    = 0;
                oldWSInfo.lastPing      = DateTime.Now;
                oldWSInfo.pingSent      = false;

            // send to other player that this player has now reconnected

            var lobbyList = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("SessionId = '{0}'", info.SessionId)));
            var usessList = new List<GFXLoginRow>(server.LoginList.Select(string.Format("SessionId = '{0}'", info.SessionId)));

            var userId  = usessList[0].UserId;
            var lobbyId = lobbyList[0].LobbyID;

            // We get the users playing in this lobby
            var userList                = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}'", lobbyId)));

            var disconnectedPlayerInfo  = new List<GFXUserRow>(server.UserList.Select(string.Format("UserId = '{0}'", userId)));
            var disconnectedPlayerData  = new Dictionary<string, object>()
                    {
                        { "Name",       disconnectedPlayerInfo[0].FirstName + " " + disconnectedPlayerInfo[0].LastName },
                        { "UserId",     disconnectedPlayerInfo[0].UserId },
                        { "Username",   disconnectedPlayerInfo[0].Username }
                    };

            var serializedInfo          = JsonConvert.SerializeObject(disconnectedPlayerData);

            foreach (var player in userList)
            {
                if (player.SessionID != info.SessionId)
                    server.WebSocketList[player.SessionID].webSocket.Send(JsonConvert.SerializeObject(new GFXSocketResponse
                    {
                        Subject         = "GFX_PLAYER_RECONNECTED",
                        Message         = serializedInfo,
                        ResponseCode    = GFXResponseType.Normal
                    }));
            }

            return constructResponse(GFXResponseType.Normal, "");
        }
    }
}
