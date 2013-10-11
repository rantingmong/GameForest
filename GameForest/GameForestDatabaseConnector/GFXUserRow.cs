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
        public string   fb_id       { get; set; }
    }

    public sealed class GFXUserRowTranslator : GFXDatabaseTranslator<GFXUserRow>
    {
        public string               TableName
        {
            get { return "RegisteredUsers"; }
        }

        public IEnumerable<string>  TableColumns
        {
            get { return new[] { "UserId", "UserName", "Password", "FirstName", "LastName", "Description", "fb_id" }; }
        }

        public IEnumerable<string>  ToStringValues  (GFXUserRow data)
        {
            var returnData = new string[7];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.UserId.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.Username.ToString());
            returnData[2] = GFXDatabaseCore.ToSQLString(data.Password);
            returnData[3] = GFXDatabaseCore.ToSQLString(data.FirstName);
            returnData[4] = GFXDatabaseCore.ToSQLString(data.LastName);
            returnData[5] = GFXDatabaseCore.ToSQLString(data.Description);
            returnData[6] = GFXDatabaseCore.ToSQLString(data.fb_id);

            return returnData;
        }

        public GFXUserRow           ToNativeData    (MySqlDataReader reader)
        {
            return new GFXUserRow
            {
                UserId      = Guid.Parse(reader.GetString(0)),
                Username    = reader.GetString(1),
                Password    = reader.GetString(2),
                FirstName   = reader.GetString(3),
                LastName    = reader.GetString(4),
                Description = reader.GetString(5),
                fb_id       = reader.GetString(6)
            };
        }
    }
}
