using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;

namespace GameForestCore.Database
{
    public struct GFXLobbySessionRow
    {
        public int  RowId       { get; set; }

        public Guid LobbyID     { get; set; }

        public Guid SessionID   { get; set; }

        public bool Owner       { get; set; }

        public int  Order       { get; set; }

        /// <summary>
        /// Gets or sets the status of the player playing (ready, choosing, playing)
        /// </summary>
        public int  Status      { get; set; }
    }

    public class GFXLobbySessionRowTranslator : GFXDatabaseTranslator<GFXLobbySessionRow>
    {

        public string               TableName
        {
            get { return "LobbySession"; }
        }

        public IEnumerable<string>  TableColumns
        {
            get { return new[] { "LobbyId", "SessionId", "Owner", "Order", "Status", "RowId" }; }
        }

        public IEnumerable<string>  ToStringValues  (GFXLobbySessionRow data)
        {
            var returnData = new string[6];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.LobbyID.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.SessionID.ToString());
            returnData[2] = Convert.ToString(data.Owner);
            returnData[3] = Convert.ToString(data.Order);
            returnData[4] = Convert.ToString(data.Status);
            returnData[5] = Convert.ToString(data.RowId);

            return returnData;
        }

        public GFXLobbySessionRow   ToNativeData    (MySqlDataReader reader)
        {
            return new GFXLobbySessionRow
            {
                LobbyID     = Guid.Parse(reader.GetString(0)),
                SessionID   = Guid.Parse(reader.GetString(1)),
                Owner       = reader.GetBoolean(2),
                Order       = reader.GetInt32(3),
                Status      = reader.GetInt32(4),
                RowId       = reader.GetInt32(5)
            };
        }
    }
}
