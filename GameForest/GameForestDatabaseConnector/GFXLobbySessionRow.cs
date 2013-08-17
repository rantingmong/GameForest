using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;

namespace GameForestCore.Database
{
    public struct GFXLobbySessionRow
    {
        public int  RowId       { get; set; }

        public Guid LobbyID     { get; set; }

        public Guid GameID      { get; set; }

        public Guid SessionID   { get; set; }

        public Guid UserID      { get; set; }

        public bool Owner       { get; set; }

        public int  Order       { get; set; }

        public bool Ready       { get; set; }
    }

    public class GFXLobbySessionRowTranslator : GFXDatabaseTranslator<GFXLobbySessionRow>
    {

        public string               TableName
        {
            get { return "LobbySession"; }
        }

        public IEnumerable<string>  TableColumns
        {
            get { return new[] { "LobbyId", "GameId", "SessionId", "UserId", "Owner", "Order", "Ready", "RowId" }; }
        }

        public IEnumerable<string>  ToStringValues  (GFXLobbySessionRow data)
        {
            var returnData = new string[4];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.LobbyID.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.GameID.ToString());
            returnData[2] = GFXDatabaseCore.ToSQLString(data.SessionID.ToString());
            returnData[3] = GFXDatabaseCore.ToSQLString(data.UserID.ToString());
            returnData[4] = Convert.ToString(data.Owner);
            returnData[5] = Convert.ToString(data.Order);
            returnData[6] = Convert.ToString(data.Ready);
            returnData[7] = Convert.ToString(data.RowId);

            return returnData;
        }

        public GFXLobbySessionRow   ToNativeData    (MySqlDataReader reader)
        {
            return new GFXLobbySessionRow
            {
                LobbyID     = Guid.Parse(reader.GetString(0)),
                GameID      = Guid.Parse(reader.GetString(1)),
                SessionID   = Guid.Parse(reader.GetString(2)),
                UserID      = Guid.Parse(reader.GetString(3)),
                Owner       = reader.GetBoolean(4),
                Order       = reader.GetInt32(5),
                Ready       = reader.GetBoolean(6),
                RowId       = reader.GetInt32(7)
            };
        }
    }
}
