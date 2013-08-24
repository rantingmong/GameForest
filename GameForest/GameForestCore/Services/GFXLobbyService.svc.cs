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
        private readonly GFXDatabaseTable<GFXUserRow>           userTable;
        private readonly GFXDatabaseTable<GFXGameRow>           gameTable;
        private readonly GFXDatabaseTable<GFXLoginRow>          loginTable;
        private readonly GFXDatabaseTable<GFXLobbyRow>          lobbyTable;
        private readonly GFXDatabaseTable<GFXLobbySessionRow>   lobbySessionTable;

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
                return constructResponse(GFXResponseType.Normal, lobbyTable.Select(string.Empty, maxcount));
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

                if (lobby.Count <= 0)
                {
                    return constructResponse(GFXResponseType.NotFound, "Lobby ID cannot be found!");
                }

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
                    GameID      = Guid.Parse(gameid),
                    LobbyID     = Guid.NewGuid(),
                    Name        = lobbyname,
                    Password    = password,
                    Private     = isprivate,
                    Status      = GFXLobbyStatus.Waiting
                };

                lobbyTable.Insert(session);
                
                // create the owner's session
                GFXLobbySessionRow playerSession = new GFXLobbySessionRow
                {
                    LobbyID     = session.LobbyID,
                    SessionID   = Guid.Parse(usersessionid),
                    Order       = 1,
                    Owner       = true,
                    Status      = 0,
                    RowId       = lobbySessionTable.Count()
                };

                lobbySessionTable.Insert(playerSession);

                GFXLoginRow loginRow = new List<GFXLoginRow>(loginTable.Select(string.Format("UserSessionId = {0}", usersessionid), 1))[0];

                loginRow.UserStatus = GFXLoginStatus.LOBBY;

                loginTable.Update(string.Format("UserSessionId = {0}", usersessionid), loginRow);

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
                    LobbyID     = Guid.Parse(lobbyid),
                    SessionID   = Guid.Parse(usersessionid),
                    Order       = lobbySessionTable.Count(string.Format("LobbyID = {0}", lobbyid)) + 1,
                    Owner       = false,
                    Status      = 0,
                    RowId       = lobbySessionTable.Count()
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
                if (new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("SessionID = {0} AND LobbyID = {1}", usersessionid, lobbyid))).Count <= 0)
                {
                    return constructResponse(GFXResponseType.NotFound, "User session specified is not in the lobby.");
                }

                List<GFXLobbySessionRow> userList = new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("LobbyId = {0}", lobbyid)));

                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(userList));
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[Lobby|LeaveLobby] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse GetCurrentPlayer(string lobbyid, string usersessionid)
        {
            try
            {
                // get lobbyid of user asking
                List<GFXLobbySessionRow> sessions = new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("SessionId = {0}", usersessionid)));

                if (sessions.Count <= 0)
                    return constructResponse(GFXResponseType.NotFound, "User is not playing any games!");

                // get lobby information
                List<GFXLobbyRow> lobbies = new List<GFXLobbyRow>(lobbyTable.Select(string.Format("LobbyId = {0}", sessions[0].LobbyID)));

                if (lobbies.Count <= 0)
                    throw new InvalidProgramException("Bug here! D:");

                // get current player's information
                List<GFXLoginRow> logins = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = {0}", lobbies[0].CurrentPlayer)));

                if (logins.Count <= 0)
                    throw new InvalidProgramException("Another bug here! D:");

                List<GFXUserRow> users = new List<GFXUserRow>(userTable.Select(string.Format("UserId = {0}", logins[0].UserId)));

                if (users.Count <= 0)
                    throw new InvalidProgramException("So close but no cigar!");

                return constructResponse(GFXResponseType.Normal, users[0]);
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[Lobby|LeaveLobby] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse GetNextPlayer(string lobbyid, string usersessionid, string steps)
        {
            // get other users
            List<GFXLobbySessionRow> otherPlayers = new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("LobbyId = {0} AND Order > {1} ORDER BY Order ASC", lobbyid)));

            int lobbyCount = lobbySessionTable.Count(string.Format("LobbyId = {0}", lobbyid));

            // change the CurrentUserSession of the game data to the next player
            if (otherPlayers.Count <= 0)
            {
                // get the first person
                otherPlayers = new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("LobbyId = {0} ORDER BY Order ASC", lobbyid)));

                if (otherPlayers.Count <= 0)
                {
                    throw new InvalidProgramException("Bug bug!");
                }
            }

            GFXLobbySessionRow nextPlayer = otherPlayers[int.Parse(steps) % lobbyCount];

            List<GFXLoginRow> playerSession = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionID = {0}", nextPlayer.SessionID)));

            if (playerSession.Count <= 0)
                throw new InvalidOperationException("Bug bug!");

            List<GFXUserRow> userInfo = new List<GFXUserRow>(userTable.Select(string.Format("UserId = {0}", playerSession[0].UserId)));

            if (userInfo.Count <= 0)
                throw new InvalidOperationException("Bug bug!");

            return constructResponse(GFXResponseType.Normal, userInfo[0]);
        }

        // ----------------------------------------------------------------------------------------------------------------

        private static GFXRestResponse constructResponse(GFXResponseType responseType, object payload)
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
