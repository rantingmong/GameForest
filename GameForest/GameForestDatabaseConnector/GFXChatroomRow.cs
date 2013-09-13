using System;
using System.Collections.Generic;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public struct GFXChatroomRow
    {
        public Guid     ChatroomId          { get; set; }

        public Guid     ChatroomUserId      { get; set; }

        public Guid     SessionId           { get; set; }
    }

    public sealed class GFXChatroomRowTranslator : GFXDatabaseTranslator<GFXChatroomRow>
    {
        public string                   TableName
        {
            get { return "Chatroom"; }
        }

        public IEnumerable<string>      TableColumns
        {
            get { return new[] { "ChatroomId", "ChatroomUserId", "SessionId" }; }
        }

        public IEnumerable<string>      ToStringValues(GFXChatroomRow data)
        {
            var returnData = new string[3];

            returnData[0] = GFXDatabaseCore.ToSQLString(data.ChatroomId.ToString());
            returnData[1] = GFXDatabaseCore.ToSQLString(data.ChatroomUserId.ToString());
            returnData[2] = GFXDatabaseCore.ToSQLString(data.SessionId.ToString());

            return returnData;
        }

        public GFXChatroomRow           ToNativeData (MySqlDataReader reader)
        {
            return new GFXChatroomRow
            {
                ChatroomId      = reader.GetGuid("ChatroomId"),
                ChatroomUserId  = reader.GetGuid("ChatroomUserId"),
                SessionId       = reader.GetGuid("SessionId")
            };
        }
    }
}
