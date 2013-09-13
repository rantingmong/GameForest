using System;
using System.Collections.Generic;

namespace GameForestCoreWebSocket
{
    public struct GFXGameDataEntry
    {
        public string Key;
        public string Data;
    }

    public class GFXGameData
    {
        public Guid                         LobbySessionId
        {
            get;
            private set;
        }

        public Guid                         CurrentUserSession
        {
            get;
            set;
        }

        public Dictionary<string, string>   Data
        {
            get;
            private set;
        }

        public Dictionary<Guid, string>     UserData
        {
            get;
            private set;
        }

        public                              GFXGameData     (Guid lobbySessionId)
        {
            Data            = new Dictionary<string, string>();
            UserData        = new Dictionary<Guid, string>();

            LobbySessionId  = lobbySessionId;
        }


    }
}
