using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;

namespace GameForestCore.Database
{
    public struct GFXLobbySessionRow
    {
        public Guid LobbyID     { get; set; }

        public Guid GameID      { get; set; }

        public Guid SessionID   { get; set; }

        public Guid UserID      { get; set; }
    }

    public class GFXLobbySessionRowTranslator : GFXDatabaseTranslator<GFXLobbySessionRow>
    {

        public string TableName
        {
            get { return "LobbySession"; }
        }

        public IEnumerable<string> TableColumns
        {
            get { return new[] { "LobbyId", "GameId", "SessionId", "UserId" }; }
        }

        public IEnumerable<string> ToStringValues(GFXLobbySessionRow data)
        {
            var returnData = new string[4];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.LobbyID.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.GameID.ToString());
            returnData[2] = GFXDatabaseCore.ToSQLString(data.SessionID.ToString());
            returnData[3] = GFXDatabaseCore.ToSQLString(data.UserID.ToString());

            return returnData;
        }

        public GFXLobbySessionRow ToNativeData(MySqlDataReader reader)
        {
            return new GFXLobbySessionRow
            {
                LobbyID     = Guid.Parse(reader.GetString(0)),
                GameID      = Guid.Parse(reader.GetString(1)),
                SessionID   = Guid.Parse(reader.GetString(2)),
                UserID      = Guid.Parse(reader.GetString(3)),
            };
        }
    }
}
