using GameForestCore.Common;
using GameForestCore.Database;
using GameForestDatabaseConnector.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Linq;

namespace GameForestCore.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GFXLobbyService : IGFXLobbyService
    {
        private readonly GFXDatabaseTable<GFXUserRow>                   userTable;
        private readonly GFXDatabaseTable<GFXGameRow>                   gameTable;
        private readonly GFXDatabaseTable<GFXLoginRow>                  loginTable;
        private readonly GFXDatabaseTable<GFXLobbyRow>                  lobbyTable;
        private readonly GFXDatabaseTable<GFXLobbySessionRow>           lobbySessionTable;

        public                              GFXLobbyService             ()
        {
            userTable           = new GFXDatabaseTable<GFXUserRow>(new GFXUserRowTranslator());
            gameTable           = new GFXDatabaseTable<GFXGameRow>(new GFXGameRowTranslator());

            loginTable          = new GFXDatabaseTable<GFXLoginRow>(new GFXLoginRowTranslator());
            lobbyTable          = new GFXDatabaseTable<GFXLobbyRow>(new GFXLobbyRowTranslator());

            lobbySessionTable   = new GFXDatabaseTable<GFXLobbySessionRow>(new GFXLobbySessionRowTranslator());
        }

        // ----------------------------------------------------------------------------------------------------------------

        public  GFXRestResponse             GetLobbies                  (string maxcount)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "GetLobbies", "Fetching lobby list...");

            try
            {
                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(lobbyTable.Select(string.Empty, int.Parse(maxcount))));
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetGameList", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse             GetLobby                    (string lobbyid)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "GetLobby", "Fetching lobby info...");

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
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetLobby", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse             GetPlayerOrder              (string lobbyid, string usersessionid)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "GetPlayerOrder", "Fetching lobby info...");

            try
            {
                // get user playing the lobby
                var players = new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("LobbyId = '{0}' AND SessionId = '{1}'", lobbyid, usersessionid)));
                
                if (players.Count <= 0)
                {
                    GFXLogger.GetInstance().Log(GFXLoggerLevel.WARN, "GetPlayerOrder", "User is not playing any games!");

                    return constructResponse(GFXResponseType.NotFound, "User is not playing any games!");
                }

                return constructResponse(GFXResponseType.Normal, players[0].Order.ToString());
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetPlayerOrder", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse             CreateLobby                 (string lobbyname, string gameid, string password, string usersessionid, bool isprivate)
        {
            try
            {
                if (!sessionExists(usersessionid))
                {
                    GFXLogger.GetInstance().Log(GFXLoggerLevel.WARN, "CreateLobby", "Invalid user session id.");
                    return constructResponse(GFXResponseType.NotFound, "Invalid user session id.");
                }

                if (!gameExists(gameid))
                {
                    GFXLogger.GetInstance().Log(GFXLoggerLevel.WARN, "CreateLobby", "Invalid game id.");
                    return constructResponse(GFXResponseType.NotFound, "Invalid game id.");
                }

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
                    LobbyID         = session.LobbyID,
                    SessionID       = Guid.Parse(usersessionid),
                    Order           = 1,
                    OrderOriginal   = 1,
                    Owner           = true,
                    Status          = 0
                };

                lobbySessionTable.Insert(playerSession);

                GFXLoginRow loginRow = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = '{0}'", usersessionid), 1))[0];

                loginRow.UserStatus = GFXLoginStatus.LOBBY;

                loginTable.Update(string.Format("SessionId = '{0}'", usersessionid), loginRow);

                return constructResponse(GFXResponseType.Normal, session.LobbyID.ToString());
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "CreateLobby", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse             JoinLobby                   (string lobbyid, string usersessionid)
        {
            try
            {
                if (!sessionExists(usersessionid))
                {
                    GFXLogger.GetInstance().Log(GFXLoggerLevel.WARN, "JoinLobby", "User session ID is invalid.");

                    return constructResponse(GFXResponseType.NotFound, "User session ID is invalid.");
                }

                if (!lobbyExists(lobbyid))
                {
                    GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "JoinLobby", "Lobby id is invalid.");

                    return constructResponse(GFXResponseType.NotFound, "Lobby ID is invalid.");
                }

                int lobbyCount = Convert.ToInt32(GetUserCount(lobbyid).AdditionalData);

                var games   = new List<GFXLobbyRow>(lobbyTable.Select(string.Format("LobbyId='{0}'", lobbyid)))[0];
                var game    = new List<GFXGameRow>(gameTable.Select(string.Format("GameId='{0}'", games.GameID)))[0];

                if (lobbyCount + 1 > game.MaxPlayers)
                {
                    GFXLogger.GetInstance().Log(GFXLoggerLevel.WARN, "JoinLobby", "This lobby is full!");

                    return constructResponse(GFXResponseType.InvalidInput, "This lobby is full!");
                }

                var order = lobbySessionTable.Count(string.Format("LobbyID = '{0}'", lobbyid)) + 1;

                GFXLobbySessionRow session = new GFXLobbySessionRow
                {
                    LobbyID         = Guid.Parse(lobbyid),
                    SessionID       = Guid.Parse(usersessionid),
                    Order           = order,
                    OrderOriginal   = order,
                    Owner           = false,
                    Status          = 0
                };

                lobbySessionTable.Insert(session);

                GFXLoginRow loginRow = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = '{0}'", usersessionid), 1))[0];

                loginRow.UserStatus = GFXLoginStatus.LOBBY;

                loginTable.Update(string.Format("SessionId = '{0}'", usersessionid), loginRow);

                return constructResponse(GFXResponseType.Normal, lobbyid);
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "JoinLobby", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse             LeaveLobby                  (string usersessionid)
        {
            try
            {
                List<GFXLobbySessionRow> result = new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("SessionId = '{0}'", usersessionid)));
                lobbySessionTable.Remove(string.Format("SessionId = '{0}'", usersessionid));

                if (lobbySessionTable.Count(string.Format("LobbyId = '{0}'", result[0].LobbyID)) == 0)
                {
                    lobbyTable.Remove(string.Format("LobbyId = '{0}'", result[0].LobbyID));
                }

                GFXLoginRow loginRow = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = '{0}'", usersessionid), 1))[0];

                loginRow.UserStatus = GFXLoginStatus.MENU;

                loginTable.Update(string.Format("SessionId = '{0}'", usersessionid), loginRow);

                return constructResponse(GFXResponseType.Normal, "");
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "LeaveLobby", exp.Message);
                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse             GetUserList                 (string lobbyid, string usersessionid)
        {
            try
            {
                var lobbySessions = new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("SessionID = '{0}' AND LobbyID = '{1}'", usersessionid,lobbyid)));

                if (lobbySessions.Count <= 0)
                {
                    GFXLogger.GetInstance().Log(GFXLoggerLevel.WARN, "GetUserList", "User session specified is not in the lobby.");
                    return constructResponse(GFXResponseType.NotFound, "User session specified is not in the lobby.");
                }

                var returnList = new List<Dictionary<string, object>>();

                foreach (var item in lobbySessionTable.Select(string.Format("LobbyId = '{0}'", lobbyid)).OrderBy((entry) => entry.Order))
                {
                    var sessionList = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = '{0}'", item.SessionID)));
                    var userList    = new List<GFXUserRow>(userTable.Select(string.Format("UserId = '{0}'", sessionList[0].UserId)));

                    returnList.Add(userInfoToJson(userList[0]));
                }

                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(returnList));
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.ERROR, "GetUserList", exp.Message);
                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse             GetUserCount                (string lobbyid)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "GetUserCount", "Getting user count for lobby " + lobbyid);

            try
            {
                var userList = lobbySessionTable.Count(string.Format("LobbyId = '{0}'", lobbyid));

                return constructResponse(GFXResponseType.Normal, userList.ToString());
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetUserCount", exp.Message);
                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse             GetCurrentPlayer            (string lobbyid, string usersessionid)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "GetCurrentPlayer", "Getting current player for session " + lobbyid);

            try
            {
                // get lobbyid of user asking
                List<GFXLobbySessionRow> sessions = new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("SessionId = '{0}'", usersessionid)));

                if (sessions.Count <= 0)
                {
                    GFXLogger.GetInstance().Log(GFXLoggerLevel.WARN, "GetCurrentPlayer", "User is not playing any games!");

                    return constructResponse(GFXResponseType.NotFound, "User is not playing any games!");
                }

                // get lobby information
                var lobbies = new List<GFXLobbyRow>(lobbyTable.Select(string.Format("LobbyId = '{0}'", sessions[0].LobbyID)));

                // get current player's information
                var logins  = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = '{0}'", lobbies[0].CurrentPlayer)));
                var users   = new List<GFXUserRow>(userTable.Select(string.Format("UserId = '{0}'", logins[0].UserId)));

                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(userInfoToJson(users[0])));
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetCurrentPlayer", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public  GFXRestResponse             GetNextPlayer               (string lobbyid, string usersessionid, string steps)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "GetNextPlayer", "Getting current player for session " + lobbyid);

            try
            {
                // get other users
                List<GFXLobbySessionRow> players = new List<GFXLobbySessionRow>(lobbySessionTable.Select(string.Format("LobbyId = '{0}'", lobbyid)));

                int lobbyCount = lobbySessionTable.Count(string.Format("LobbyId = '{0}'", lobbyid));

                // get the calling player's information
                var callingPlayer = new GFXLobbySessionRow();

                foreach (var item in players)
                {
                    if (item.SessionID == Guid.Parse(usersessionid))
                    {
                        callingPlayer = item;
                    }
                }

                // get the player who has callingplayer.playerorder + steps
                int calledPlayer    = ((callingPlayer.Order + Convert.ToInt32(steps) - 1) % lobbyCount) + 1;
                var selectedPlayer  = new GFXLobbySessionRow();

                foreach (var item in players)
                {
                    if (item.Order == calledPlayer)
                    {
                        selectedPlayer = item;
                    }
                }

                var loggedInUsers   = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = '{0}'", selectedPlayer.SessionID)));
                var users           = new List<GFXUserRow>(userTable.Select(string.Format("UserId = '{0}'", loggedInUsers[0].UserId)));

                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(userInfoToJson(users[0])));
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetNextPlayer", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        // ----------------------------------------------------------------------------------------------------------------

        private Dictionary<string, object>  userInfoToJson              (GFXUserRow userRow)
        {
            var uResult = new Dictionary<string, object>();

            uResult["Name"]         = userRow.FirstName + " " + userRow.LastName;
            uResult["UserId"]       = userRow.UserId;
            uResult["Description"]  = userRow.Description;
            uResult["Username"]     = userRow.Username;
            uResult["NameFirst"]    = userRow.FirstName;
            uResult["NameLast"]     = userRow.LastName;

            return uResult;
        }

        private GFXRestResponse             constructResponse           (GFXResponseType responseType, string payload)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "constructResponse", "Returning result with type" + responseType + " and payload " + payload);

            return new GFXRestResponse { AdditionalData = payload, ResponseType = responseType };
        }

        private bool                        sessionExists               (string sessionid)
        {
            return loginTable.Count(string.Format("sessionid = '{0}'", sessionid)) == 1;
        }

        private bool                        gameExists                  (string gameid)
        {
            return gameTable.Count(string.Format("gameid = '{0}'", gameid)) == 1;
        }

        private bool                        lobbyExists                 (string lobbyid)
        {
            return lobbyTable.Count(string.Format("lobbyid = '{0}'", lobbyid)) == 1;
        }
    }
}
