using System;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public struct GFXChatRow
    {
        public Guid     ChatroomId  { get; set; }
        
        public Guid     UserId      { get; set; }
    }

    public sealed class GFXChatRowTranslator : GFXDatabaseTranslator<GFXChatRow>
    {
        public string                   TableName
        {
            get { return "ChatroomUser"; }
        }

        public IEnumerable<string>      TableColumns
        {
            get { return new[] { "ChatroomId", "UserId" }; }
        }

        public IEnumerable<string>      ToStringValues (GFXChatRow data)
        {
            var returnData = new string[2];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.ChatroomId.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.UserId.ToString());

            return returnData;
        }
        
        public GFXChatRow               ToNativeData (MySqlDataReader reader)
        {
            return new GFXChatRow
            {
                ChatroomId  = reader.GetGuid("ChatroomId"),
                UserId      = reader.GetGuid("UserId")
            };
        }
    }
}
