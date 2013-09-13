using System;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public enum GFXHighscoreListType : int
    {
        AllTime     = 0,
        Monthly     = 1,
        Weekly      = 2,
        Daily       = 3
    }

    public struct GFXHighscoreListRow
    {
        public Guid HighscoreId             { get; set; }

        public Guid GameId                  { get; set; }

        public GFXHighscoreListType Type    { get; set; }

        public string Description           { get; set; }
    }

    public sealed class GFXHighscoreListRowTranslator : GFXDatabaseTranslator<GFXHighscoreListRow>
    {
        public string                   TableName
        {
            get { return "HighscoreList"; }
        }

        public IEnumerable<string>      TableColumns
        {
            get { return new[] { "HighscoreId", "GameId", "Type", "Description" }; }
        }

        public IEnumerable<string>      ToStringValues(GFXHighscoreListRow data)
        {
            var returnData = new string[4];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.HighscoreId.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.GameId.ToString());
            returnData[2] = ((int)data.Type).ToString();
            returnData[3] = Convert.ToString(data.Description);

            return returnData;
        }

        public GFXHighscoreListRow      ToNativeData(MySqlDataReader reader)
        {
            return new GFXHighscoreListRow
            {
                HighscoreId     = reader.GetGuid("HighscoreId"),
                GameId          = reader.GetGuid("GameId"),
                Type            = (GFXHighscoreListType) reader.GetInt32("Type"),
                Description     = reader.GetString("Description")
            };
        }
    }
}
