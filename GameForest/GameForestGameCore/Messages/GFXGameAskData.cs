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
    /// Message sent by the client asking for the current data.
    /// </summary>
    public class GFXGameAskData : GFXSocketListener
    {
        public override string Subject
        {
            get { throw new NotImplementedException(); }
        }

        public override GFXSocketResponse DoMessage(GFXServerCore server, GFXSocketInfo info, Fleck.IWebSocketConnection ws)
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

                if (dataStore.UserData.ContainsKey(ownerPlayer.SessionID))
                {
                    string returnData = JsonConvert.SerializeObject(dataStore);
                    server.WebSocketList[ownerPlayer.SessionID].Send("TODO: SEND THE GAME DATA.");
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
