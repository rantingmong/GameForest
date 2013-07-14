using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public struct GFXUserRow
    {
        public Guid     UserId      { get; set; }

        public string   Password    { get; set; }
        public string   Username    { get; set; }

        public string   FirstName   { get; set; }
        public string   LastName    { get; set; }

        public string   Description { get; set; }
    }

    public sealed class GFXUserRowTranslator : GFXDatabaseTranslator<GFXUserRow>
    {
        public string TableName
        {
            get { return "RegisteredUsers"; }
        }

        public IEnumerable<string> TableColumns
        {
            get { return new[] { "UserId", "UserName", "Password", "FirstName", "LastName", "Description" }; }
        }

        public IEnumerable<string> ToStringValues(GFXUserRow data)
        {
            var returnData = new string[6];

            returnData[0] = string.Format("'{0}'", data.UserId);
            returnData[1] = string.Format("'{0}'", data.Username);
            returnData[2] = string.Format("'{0}'", data.Password);
            returnData[3] = string.Format("'{0}'", data.FirstName);
            returnData[4] = string.Format("'{0}'", data.LastName);
            returnData[5] = string.Format("'{0}'", data.Description);

            return returnData;
        }

        public GFXUserRow ToNativeData(MySqlDataReader reader)
        {
            return new GFXUserRow
            {
                UserId      = Guid.Parse(reader.GetString(0)),
                Username    = reader.GetString(1),
                Password    = reader.GetString(2),
                FirstName   = reader.GetString(3),
                LastName    = reader.GetString(4),
                Description = reader.GetString(5),
            };
        }
    }
}
