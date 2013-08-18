using GameForestCore.Common;
using GameForestCore.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace GameForestCore.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single), AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class GFXLobbyService : IGFXLobbyService
    {
        private readonly GFXDatabaseTable<GFXUserRow> userTable;
        private readonly GFXDatabaseTable<GFXGameRow> gameTable;
        private readonly GFXDatabaseTable<GFXLoginRow> loginTable;
        private readonly GFXDatabaseTable<GFXLobbyRow> lobbyTable;
        private readonly GFXDatabaseTable<GFXLobbySessionRow> lobbySessionTable;

        public GFXLobbyService()
        {
            userTable = new GFXDatabaseTable<GFXUserRow>(new GFXUserRowTranslator());
            gameTable = new GFXDatabaseTable<GFXGameRow>(new GFXGameRowTranslator());

            loginTable = new GFXDatabaseTable<GFXLoginRow>(new GFXLoginRowTranslator());
            lobbyTable = new GFXDatabaseTable<GFXLobbyRow>(new GFXLobbyRowTranslator());

            lobbySessionTable = new GFXDatabaseTable<GFXLobbySessionRow>(new GFXLobbySessionRowTranslator());
        }

        // ----------------------------------------------------------------------------------------------------------------

        public GFXRestResponse GetLobbies(int maxcount)
        {
            try
            {
                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(lobbyTable.Select(string.Empty, maxcount)));
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[Lobby|GetLobbies] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse GetLobby(string lobbyid)
        {
            try
            {
                var lobby = new List<GFXLobbyRow>(lobbyTable.Select(string.Format("LobbyId = '{0}'", lobbyid)));

                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(lobby[0]));
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[Lobby|GetLobby] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse CreateLobby(string lobbyname, string gameid, string password, string usersessionid, bool isprivate)
        {
            try
            {
                if (!sessionExists(usersessionid))
                    return constructResponse(GFXResponseType.NotFound, "Invalid user session id.");

                if (!gameExists(gameid))
                    return constructResponse(GFXResponseType.NotFound, "Invalid game id.");

                GFXLobbyRow session = new GFXLobbyRow
                {
                    GameID = Guid.Parse(gameid),
                    LobbyID = Guid.NewGuid(),
                    Name = lobbyname,
                    Password = password,
                    Private = isprivate,
                    Status = GFXLobbyStatus.Waiting
                };

                lobbyTable.Insert(session);

                return constructResponse(GFXResponseType.Normal, session.LobbyID.ToString());
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[Lobby|CreateLobby] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse JoinLobby(string lobbyid, string usersessionid)
        {
            try
            {
                if (!sessionExists(usersessionid))
                    return constructResponse(GFXResponseType.NotFound, "User session ID is invalid.");

                if (!lobbyExists(lobbyid))
                    return constructResponse(GFXResponseType.NotFound, "Lobby ID is invalid.");

                GFXLobbySessionRow session = new GFXLobbySessionRow
                {
                    LobbyID = Guid.Parse(lobbyid),
                    GameID = getGameIdFromLobby(Guid.Parse(lobbyid)),
                    UserID = getUserId(usersessionid),
                    SessionID = Guid.Parse(usersessionid)
                };

                lobbySessionTable.Insert(session);

                GFXLoginRow loginRow = new List<GFXLoginRow>(loginTable.Select(string.Format("UserSessionId = {0}", usersessionid), 1))[0];

                loginRow.UserStatus = GFXLoginStatus.LOBBY;

                loginTable.Update(string.Format("UserSessionId = {0}", usersessionid), loginRow);

                return constructResponse(GFXResponseType.Normal, "");
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[Lobby|JoinLobby] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse LeaveLobby(string usersessionid)
        {
            try
            {
                List<GFXLobbySessionRow> result = new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("SessionId = '{0}'", usersessionid)));
                lobbySessionTable.Remove(string.Format("SessionId = '{0}'", usersessionid));

                if (lobbySessionTable.Count(string.Format("LobbyId = '{0}'", result[0].LobbyID)) == 0)
                {
                    lobbyTable.Remove(string.Format("LobbyId = '{0}'", result[0].LobbyID));
                }

                GFXLoginRow loginRow = new List<GFXLoginRow>(loginTable.Select(string.Format("UserSessionId = {0}", usersessionid), 1))[0];

                loginRow.UserStatus = GFXLoginStatus.MENU;

                loginTable.Update(string.Format("UserSessionId = {0}", usersessionid), loginRow);

                return constructResponse(GFXResponseType.Normal, "");
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[Lobby|LeaveLobby] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse GetUserList(string lobbyid, string usersessionid)
        {
            try
            {
                List<GFXLobbySessionRow> userList = new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("LobbyId = {0}", lobbyid)));

                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(userList));
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[Lobby|LeaveLobby] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        // ----------------------------------------------------------------------------------------------------------------

        private static GFXRestResponse constructResponse(GFXResponseType responseType, string payload)
        {
            return new GFXRestResponse { AdditionalData = payload, ResponseType = responseType };
        }

        private bool userExists(string user)
        {
            Guid userId;

            if (Guid.TryParse(user, out userId))
                return userTable.Count(string.Format("userid = '{0}'", userId)) == 1;

            return this.userTable.Count(string.Format("username = '{0}'", user)) == 1;
        }

        private bool sessionExists(string sessionid)
        {
            return loginTable.Count(string.Format("sessionid = '{0}'", sessionid)) == 1;
        }

        private bool gameExists(string gameid)
        {
            return gameTable.Count(string.Format("gameid = '{0}'", gameid)) == 1;
        }

        private bool lobbyExists(string lobbyid)
        {
            return lobbyTable.Count(string.Format("lobbyid = '{0}'", lobbyid)) == 1;
        }

        private Guid getUserId(string input)
        {
            Guid sessionId;

            if (Guid.TryParse(input, out sessionId))
            {
                var result = new List<GFXLoginRow>(loginTable.Select(string.Format("sessionid = '{0}'", sessionId)));

                return result.Count > 0 ? result[0].UserId : Guid.Empty;
            }
            else
            {
                var result = new List<GFXUserRow>(userTable.Select(string.Format("username = '{0}'", input)));

                return result.Count > 0 ? result[0].UserId : Guid.Empty;
            }
        }

        private Guid getGameIdFromLobby(Guid lobbyId)
        {
            List<GFXLobbyRow> result = new List<GFXLobbyRow>(lobbyTable.Select(string.Format("LobbyId = ", lobbyId)));

            return result[0].GameID;
        }
    }
}
