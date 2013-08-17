﻿using GameForestCore.Database;
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
                List<GFXLobbySessionRow> lobby = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("SessionId = {0}", info.SessionId)));

                if (lobby.Count <= 0)
                    return constructResponse(GFXResponseType.InvalidInput, "User is not playing any games!");

                GFXLobbySessionRow ownerPlayer = lobby[0];

                // do a SQL query with order by of column 'order' in ascending order and greater than ownerPlayer.Order

                // send GFX_TURN_START to the client and send GFX_TURN_CHANGED to other clients

                return constructResponse(GFXResponseType.Normal, "");
            }
            catch (Exception exp)
            {
                return constructResponse(GFXResponseType.FatalError, exp.Message);
            }
        }
    }
}
