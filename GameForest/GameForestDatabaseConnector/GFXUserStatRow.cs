using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;

namespace GameForestCore.Database
{
    public struct GFXUserStatRow
    {
        public Guid     ustat_id    { get; set; }

        public Guid     GameID      { get; set; }

        public Guid     UserId      { get; set; }

        public string   stat_name   { get; set; }

        public int      stat_value  { get; set; }
    }

    public class GFXUserStatRowTranslator : GFXDatabaseTranslator<GFXUserStatRow>
    {

        public string                   TableName
        {
            get { return "userstats"; }
        }

        public IEnumerable<string>      TableColumns
        {
            get { return new[] { "ustat_id", "GameID", "UserId", "stat_name", "stat_value" }; }
        }

        public IEnumerable<string>      ToStringValues(GFXUserStatRow data)
        {
            var returnData = new String[5];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.ustat_id.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.GameID.ToString());
            returnData[2] = GFXDatabaseCore.ToSQLString(data.UserId.ToString());
            returnData[3] = GFXDatabaseCore.ToSQLString(data.stat_name);
            returnData[4] = data.stat_value.ToString();

            return returnData;
        }

        public GFXUserStatRow           ToNativeData(MySqlDataReader reader)
        {
            return new GFXUserStatRow
            {
                ustat_id    = Guid.Parse(reader.GetString(0)),
                GameID      = Guid.Parse(reader.GetString(1)),
                UserId      = Guid.Parse(reader.GetString(2)),
                stat_name   = reader.GetString(3),
                stat_value  = reader.GetInt32(4)
            };
        }

    }
}
