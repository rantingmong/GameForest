using System;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public enum GFXLoginStatus : int
    {
        MENU            = 0,
        LOBBY           = 1,
        GAME            = 2,
    }

    public struct GFXLoginRow
    {
        public Guid                 UserId          { get; set; }
        public Guid                 UserSessionId   { get; set; }

        public DateTime             LastHeartbeat   { get; set; }

        public GFXLoginStatus       UserStatus      { get; set; }
    }

    public sealed class GFXLoginRowTranslator : GFXDatabaseTranslator<GFXLoginRow>
    {
        public string               TableName
        {
            get { return "LoggedInUsers"; }
        }

        public IEnumerable<string>  TableColumns
        {
            get { return new[] { "SessionId", "UserId", "LastHeartbeat", "Status" }; }
        }

        public IEnumerable<string>  ToStringValues  (GFXLoginRow data)
        {
            var returnData = new string[3];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.UserSessionId.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.UserId.ToString());
            returnData[2] = GFXDatabaseCore.ToSQLString(data.LastHeartbeat.ToFileTime().ToString(CultureInfo.InvariantCulture));
            returnData[3] = ((int)data.UserStatus).ToString();

            return returnData;
        }

        public GFXLoginRow          ToNativeData    (MySqlDataReader reader)
        {
            return new GFXLoginRow
            {
                UserSessionId   = Guid.Parse(reader.GetString(0)),
                UserId          = Guid.Parse(reader.GetString(1)),
                LastHeartbeat   = DateTime.FromFileTime(reader.GetInt64(2)),
                UserStatus      = (GFXLoginStatus)reader.GetInt32(3)
            };
        }
    }
}
