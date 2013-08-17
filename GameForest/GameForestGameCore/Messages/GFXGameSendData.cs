﻿using Fleck;
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
                List<GFXLobbySessionRow> lobby = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("SessionId = {0}", info.SessionId)));

                if (lobby.Count <= 0)
                    return constructResponse(GFXResponseType.InvalidInput, "User is not playing any games!");
                
                GFXLobbySessionRow ownerPlayer = lobby[0];

                // get the data store
                GFXGameData dataStore = server.GameDataList[ownerPlayer.LobbyID];

                if (dataStore.CurrentUserSession == ownerPlayer.SessionID)
                {
                    GFXGameDataEntry entry = JsonConvert.DeserializeObject<GFXGameDataEntry>(info.Message);

                    dataStore.Data[entry.Key] = entry.Data;

                    // send message to other connected players
                    List<GFXLobbySessionRow> players = new List<GFXLobbySessionRow>(server.LobbySessionList.Select(string.Format("LobbyId = {0}", lobby[0].SessionID)));

                    string gameData = JsonConvert.SerializeObject(dataStore);

                    foreach (var player in players)
                    {
                        if (player.SessionID != info.SessionId)
                        {
                            server.WebSocketList[player.SessionID].Send("THE GFX_DATA_CHANGED MESSAGE");
                        }
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
