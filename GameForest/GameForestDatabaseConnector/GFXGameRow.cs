using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;

namespace GameForestCore.Database
{
    public struct GFXGameRow
    {
        public Guid     GameID          { get; set; }

        public string   Name            { get; set; }

        public string   Description     { get; set; }

        public Uri      RelativeLink    { get; set; }
    }

    public class GFXGameRowTranslator : GFXDatabaseTranslator<GFXGameRow>
    {
        public string               TableName
        {
            get { return "GameList"; }
        }

        public IEnumerable<string>  TableColumns
        {
            get { return new[] { "GameID", "Name", "Description", "RelativeLink" }; }
        }

        public IEnumerable<string>  ToStringValues  (GFXGameRow data)
        {
            var returnData = new string[4];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.GameID.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.Name);
            returnData[2] = GFXDatabaseCore.ToSQLString(data.Description);
            returnData[3] = GFXDatabaseCore.ToSQLString(data.RelativeLink.ToString());

            return returnData;
        }

        public GFXGameRow           ToNativeData    (MySqlDataReader reader)
        {
            return new GFXGameRow
            {
                GameID          = Guid.Parse(reader.GetString(0)),
                Name            = reader.GetString(1),
                Description     = reader.GetString(2),
                RelativeLink    = new Uri(reader.GetString(3)),
            };
        }
    }
}
