﻿using System;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public struct GFXLoginRow
    {
        public Guid     UserId          { get; set; }
        public Guid     UserSessionId   { get; set; }

        public DateTime LastHeartbeat   { get; set; }
    }

    public sealed class GFXLoginTranslator : GFXDatabaseTranslator<GFXLoginRow>
    {
        public string TableName
        {
            get { return "LoggedInUsers"; }
        }

        public IEnumerable<string> TableColumns
        {
            get { return new[] { "SessionId", "UserId", "LastHeartbeat" }; }
        }

        public IEnumerable<string> ToStringValues(GFXLoginRow data)
        {
            var returnData = new string[3];

            returnData[0] = string.Format("'{0}'", data.UserSessionId.ToString());
            returnData[1] = string.Format("'{0}'", data.UserId.ToString());
            returnData[2] = data.LastHeartbeat.ToFileTime().ToString(CultureInfo.InvariantCulture);

            return returnData;
        }

        public GFXLoginRow ToNativeData(MySqlDataReader reader)
        {
            return new GFXLoginRow
            {
                UserSessionId   = Guid.Parse(reader.GetString(0)),
                UserId          = Guid.Parse(reader.GetString(1)),
                LastHeartbeat   = DateTime.FromFileTime(reader.GetInt64(2))
            };
        }
    }
}
