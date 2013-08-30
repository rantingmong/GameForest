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

        public string   RelativeLink    { get; set; }

        public Guid     Creator         { get; set; }

        public int      MinPlayers      { get; set; }

        public int      MaxPlayers      { get; set; }
    }

    public class GFXGameRowTranslator : GFXDatabaseTranslator<GFXGameRow>
    {
        public string               TableName
        {
            get { return "GameList"; }
        }

        public IEnumerable<string>  TableColumns
        {
            get { return new[] { "GameID", "Creator", "Name", "Description", "RelativeLink", "MinPlayers", "MaxPlayers" }; }
        }

        public IEnumerable<string>  ToStringValues  (GFXGameRow data)
        {
            var returnData = new string[7];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.GameID.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.Creator.ToString());
            returnData[2] = GFXDatabaseCore.ToSQLString(data.Name);
            returnData[3] = GFXDatabaseCore.ToSQLString(data.Description);
            returnData[4] = GFXDatabaseCore.ToSQLString(data.RelativeLink.ToString());
            returnData[5] = data.MinPlayers.ToString();
            returnData[6] = data.MaxPlayers.ToString();

            return returnData;
        }

        public GFXGameRow           ToNativeData    (MySqlDataReader reader)
        {
            return new GFXGameRow
            {
                GameID          = Guid.Parse(reader.GetString(0)),
                Creator         = Guid.Parse(reader.GetString(1)),
                Name            = reader.GetString(2),
                Description     = reader.GetString(3),
                RelativeLink    = reader.GetString(4),
                MinPlayers      = reader.GetInt32(5),
                MaxPlayers      = reader.GetInt32(6)
            };
        }
    }
}
