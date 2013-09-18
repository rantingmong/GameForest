using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;

namespace GameForestCore.Database
{
    public struct GFXStatRow
    {
        public Guid     stat_id             { get; set; }

        public Guid     GameID              { get; set; }

        public string   stat_name           { get; set; }

        public int      stat_value          { get; set; }
    }

    public class GFXStatRowTranslator : GFXDatabaseTranslator<GFXStatRow>
    {
        public string                   TableName
        {
            get { return "StatList"; }
        }

        public IEnumerable<string>      TableColumns
        {
            get { return new[] { "stat_id", "GameID", "stat_name", "stat_value" }; }
        }

        public IEnumerable<string>      ToStringValues (GFXStatRow data)
        {
            var returnData = new string[4];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.stat_id.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.GameID.ToString());
            returnData[2] = GFXDatabaseCore.ToSQLString(data.stat_name);
            returnData[3] = data.stat_value.ToString();

            return returnData;
        }

        public GFXStatRow               ToNativeData    (MySqlDataReader reader)
        {
            return new GFXStatRow
            {
                stat_id     = Guid.Parse(reader.GetString(0)),
                GameID      = Guid.Parse(reader.GetString(1)),
                stat_name   = reader.GetString(2),
                stat_value  = reader.GetInt32(3)
            };
        }
    }
}
