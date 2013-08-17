using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;

namespace GameForestCore.Database
{
    public enum GFXLobbyStatus : int
    {
        Waiting         = 0,
        ChoosingTurn    = 1,
        Playing         = 2,
    }

    public struct GFXLobbyRow
    {
        public Guid             LobbyID         { get; set; }

        public Guid             GameID          { get; set; }

        public string           Name            { get; set; }

        public string           Password        { get; set; }

        public bool             Private         { get; set; }

        public GFXLobbyStatus   Status          { get; set; }

        public Guid             CurrentPlayer   { get; set; }
    }

    public class GFXLobbyRowTranslator : GFXDatabaseTranslator<GFXLobbyRow>
    {
        public string               TableName
        {
            get { return "LobbyList"; }
        }

        public IEnumerable<string>  TableColumns
        {
            get { return new[] { "LobbyId", "GameId", "Name", "Password", "Private", "Status", "CurrentPlayer" }; }
        }

        public IEnumerable<string>  ToStringValues  (GFXLobbyRow data)
        {
            var returnString = new string[7];

            returnString[0] = GFXDatabaseCore.ToSQLString(data.LobbyID.ToString());
            returnString[1] = GFXDatabaseCore.ToSQLString(data.GameID.ToString());
            returnString[2] = GFXDatabaseCore.ToSQLString(data.Name);
            returnString[3] = GFXDatabaseCore.ToSQLString(data.Password);
            returnString[4] = data.Private.ToString();
            returnString[5] = ((int)data.Status).ToString();
            returnString[6] = GFXDatabaseCore.ToSQLString(data.CurrentPlayer.ToString());

            return returnString;
        }

        public GFXLobbyRow          ToNativeData    (MySqlDataReader reader)
        {
            return new GFXLobbyRow
            {
                LobbyID         = Guid.Parse(reader.GetString(0)),
                GameID          = Guid.Parse(reader.GetString(1)),
                Name            = reader.GetString(2),
                Password        = reader.GetString(3),
                Private         = reader.GetBoolean(4),
                Status          = (GFXLobbyStatus)reader.GetInt32(5),
                CurrentPlayer   = Guid.Parse(reader.GetString(6))
            };
        }
    }
}
