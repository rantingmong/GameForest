using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;

namespace GameForestCore.Database
{
    public struct GFXLobbySessionRow
    {
        public Guid LobbyID         { get; set; }

        public Guid SessionID       { get; set; }

        public bool Owner           { get; set; }

        public int  Order           { get; set; }

        public int  OrderOriginal   { get; set; }

        /// <summary>
        /// Gets or sets the status of the player playing (ready, choosing, playing)
        /// </summary>
        public int  Status          { get; set; }
    }

    public class GFXLobbySessionRowTranslator : GFXDatabaseTranslator<GFXLobbySessionRow>
    {
        public string               TableName
        {
            get { return "LobbySession"; }
        }

        public IEnumerable<string>  TableColumns
        {
            get { return new[] { "LobbyId", "SessionId", "IsOwner", "PlayerOrder", "PlayerOrderOriginal", "Status" }; }
        }

        public IEnumerable<string>  ToStringValues  (GFXLobbySessionRow data)
        {
            var returnData = new string[6];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.LobbyID.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.SessionID.ToString());
            returnData[2] = Convert.ToString(data.Owner);
            returnData[3] = Convert.ToString(data.Order);
            returnData[4] = Convert.ToString(data.OrderOriginal);
            returnData[5] = Convert.ToString(data.Status);

            return returnData;
        }

        public GFXLobbySessionRow   ToNativeData    (MySqlDataReader reader)
        {
            return new GFXLobbySessionRow
            {
                LobbyID         = reader.GetGuid("LobbyId"),
                SessionID       = reader.GetGuid("SessionId"),
                Owner           = reader.GetBoolean("IsOwner"),
                Order           = reader.GetInt32("PlayerOrder"),
                OrderOriginal   = reader.GetInt32("PlayerOrderOriginal"),
                Status          = reader.GetInt32("Status")
            };
        }
    }
}
