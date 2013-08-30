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
    /// Message sent by the client informing the server to go to the next player.
    /// </summary>
    public class GFXGameNextTurn : GFXSocketListener
    {
        public override string              Subject
        {
            get { return "GFX_NEXT_TURN"; }
        }

        public override GFXSocketResponse   DoMessage   (GFXServerCore server, GFXSocketInfo info, Fleck.IWebSocketConnection ws)
        {
            try
            {
                // get the lobby the player is in
                var lobbySessions   = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("SessionId = '{0}'", info.SessionId)));

                if (lobbySessions.Count <= 0)
                    return constructResponse(GFXResponseType.InvalidInput, "User is not playing any games!");

                var nextPlayer      = new GFXLobbySessionRow();
                var currentPlayer   = lobbySessions[0];

                // do a SQL query with order by of column 'order' in ascending order and greater than currentPlayer.Order
                var lobbies         = new List<GFXLobbyRow>(server.LobbyList.Select(string.Format("LobbyId = '{0}'", currentPlayer.LobbyID)));
                var nextPlayers     = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}' AND Order > {1} ORDER BY Order ASC", currentPlayer.LobbyID, currentPlayer.Order)));

                var lobby           = lobbies[0];

                // change the CurrentUserSession of the game data to the next player
                if (nextPlayers.Count <= 0)
                {
                    // get the first person
                    nextPlayers = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}' ORDER BY Order ASC", currentPlayer.LobbyID)));

                    if (nextPlayers.Count <= 0)
                    {
                        throw new InvalidProgramException("Bug bug!");
                    }
                }

                nextPlayer          = nextPlayers[0];
                lobby.CurrentPlayer = nextPlayer.SessionID;

                server.LobbyList.Update(string.Format("LobbyId = '{0}'", lobby.LobbyID), lobby);

                // send GFX_TURN_START to the next client and send GFX_TURN_CHANGED to other clients
                var players = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = '{0}'", currentPlayer.LobbyID)));

                foreach (var player in players)
                {
                    if (player.SessionID == nextPlayer.SessionID)
                    {
                        server.WebSocketList[player.SessionID].Send(JsonConvert.SerializeObject(new GFXSocketResponse
                            {
                                Subject = "GFX_TURN_START",
                                Message = "",
                                ResponseCode = GFXResponseType.Normal
                            }));
                    }
                    else
                    {
                        server.WebSocketList[player.SessionID].Send(JsonConvert.SerializeObject(new GFXSocketResponse
                            {
                                Subject = "GFX_TURN_CHANGED",
                                Message = "",
                                ResponseCode = GFXResponseType.Normal
                            }));
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
