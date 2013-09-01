using System;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public struct GFXHighscoreUsersRow
    {
        public Guid HighscoreId { get; set; }

        public Guid UserId      { get; set; }

        public int Score        { get; set; }
    }

    public class GFXHighscoreUsersRowTranslator : GFXDatabaseTranslator<GFXHighscoreUsersRow>
    {
        public string TableName
        {
            get { return "HighscoreUsers"; }
        }

        public IEnumerable<string> TableColumns
        {
            get { return new[] { "HighscoreId", "UserId", "Score" }; }
        }

        public IEnumerable<string> ToStringValues(GFXHighscoreUsersRow data)
        {
            var returnString = new string[3];

            returnString[0] = GFXDatabaseCore.ToSQLString(data.HighscoreId.ToString());
            returnString[1] = GFXDatabaseCore.ToSQLString(data.UserId.ToString());
            returnString[2] = ((int)data.Score).ToString();

            return returnString;
        }

        public GFXHighscoreUsersRow ToNativeData(MySqlDataReader reader)
        {
            return new GFXHighscoreUsersRow
            {
                HighscoreId = Guid.Parse(reader.GetString(0)),
                UserId      = Guid.Parse(reader.GetString(1)),
                Score       = reader.GetInt32(3)
            };
        }
    }
}
