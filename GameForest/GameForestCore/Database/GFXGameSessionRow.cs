using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace GameForestCore.Database
{
    public struct GFXGameSessionRow
    {
        public Guid GameSessionId   { get; set; }

        public Guid LobbyId         { get; set; }

        public Guid GameId          { get; set; }
    }

    public class GFXGameSessionRowTranslator : GFXDatabaseTranslator<GFXGameSessionRow>
    {
        public string               TableName
        {
            get { return "GameSessionList"; }
        }

        public IEnumerable<string>  TableColumns
        {
            get { return new[] { "GameSessionId", "LobbyId", "GameId" }; }
        }

        public IEnumerable<string>  ToStringValues  (GFXGameSessionRow data)
        {
            var returnData = new string[3];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.GameSessionId.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.LobbyId.ToString());
            returnData[2] = GFXDatabaseCore.ToSQLString(data.GameId.ToString());

            return returnData;
        }

        public GFXGameSessionRow    ToNativeData    (MySqlDataReader reader)
        {
            return new GFXGameSessionRow
            {
                GameSessionId   = Guid.Parse(reader.GetString(0)),
                LobbyId         = Guid.Parse(reader.GetString(1)),
                GameId          = Guid.Parse(reader.GetString(1)),
            };
        }
    }
}
