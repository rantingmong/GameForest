using GameForestCore.Common;
using GameForestCore.Database;
using GameForestDatabaseConnector.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GameForestCore.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GFXGameService : IGFXGameService
    {
        private GFXDatabaseTable<GFXGameRow>    gameTable;
        private GFXDatabaseTable<GFXUserRow>    userTable;
        private GFXDatabaseTable<GFXLoginRow>   loginTable;

        public GFXGameService                   ()
        {
            userTable   = new GFXDatabaseTable<GFXUserRow>(new GFXUserRowTranslator());
            gameTable   = new GFXDatabaseTable<GFXGameRow>(new GFXGameRowTranslator());
            loginTable  = new GFXDatabaseTable<GFXLoginRow>(new GFXLoginRowTranslator());
        }

        public GFXRestResponse                  GetGameList         (int maxCount)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "GetGameList", "Fetching game list...");

            try
            {
                List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select("", maxCount));

                List<Dictionary<string, object>> returnGameList = new List<Dictionary<string, object>>();

                foreach (var game in gameList)
                {
                    var user = new List<GFXUserRow>(userTable.Select(string.Format("UserId = '{0}'", game.Creator)));

                    var item = new Dictionary<string, object>();

                    item["Name"]        = game.Name;
                    item["GameID"]      = game.GameID;
                    item["MinPlayers"]  = game.MinPlayers;
                    item["MaxPlayers"]  = game.MaxPlayers;
                    item["Description"] = game.Description;
                    item["Creator"]     = user[0].Username;
                    item["CreatorName"] = user[0].FirstName + " " + user[0].LastName;

                    returnGameList.Add(item);
                }

                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(returnGameList));
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetGameList", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse                  GetGameInfo         (string gameId)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "GetGameInfo", "Fetching game info...");

            try
            {
                List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select(string.Format("GameId = '{0}'", gameId)));

                if (gameList.Count <= 0)
                {
                    return constructResponse(GFXResponseType.NotSupported, "Game ID not found.");
                }

                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(gameList[0]));
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetGameInfo", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse                  GetUserGameList     (string userId)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "GetUserGameList", "Fetching game list for user...");

            try
            {
                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(gameTable.Select(string.Format("Creator = '{0}'", userId))));
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetGameList", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse                  CreateGame          (string name, string description, int minPlayers, int maxPlayers, string usersessionid)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "CreateGame", "Creating game...");

            if (!sessionExists(usersessionid))
            {
                return constructResponse(GFXResponseType.NotFound, "User session not found!");
            }

            try
            {
                GFXGameRow newGame = new GFXGameRow
                {
                    Creator         = getUserId(usersessionid),
                    Description     = description,
                    GameID          = Guid.NewGuid(),
                    MaxPlayers      = maxPlayers,
                    MinPlayers      = minPlayers,
                    Name            = name,
                    RelativeLink    = "game/" + getUserId(usersessionid) + "/" + name
                };

                gameTable.Insert(newGame);

                return constructResponse(GFXResponseType.Normal, "");
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "CreateGame", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse                  UpdateGame          (string name, string description, int minPlayers, int maxPlayers, string usersessionid, string gameid)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "UpdateGame", "Updating game...");

            if (!sessionExists(usersessionid))
            {
                return constructResponse(GFXResponseType.NotFound, "User session not found!");
            }

            try
            {
                // get user from usersessionid
                var userInfo = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = '{0}'", usersessionid)))[0];
                var gameInfo = new List<GFXGameRow>(gameTable.Select(string.Format("GameId = '{0}'", gameid)))[0];

                if (gameInfo.Creator != userInfo.UserId)
                    return constructResponse(GFXResponseType.InvalidInput, "You cannot update this game! Its not yours. >:(");

                gameTable.Update(string.Format("Creator = '{0}' AND GameId = '{1}'", userInfo.UserId, gameid), new GFXGameRow
                {
                    Creator         = getUserId(usersessionid),
                    Description     = description,
                    GameID          = Guid.Parse(gameid),
                    MaxPlayers      = maxPlayers,
                    MinPlayers      = minPlayers,
                    Name            = name,
                    RelativeLink    = gameInfo.RelativeLink
                });
                
                return constructResponse(GFXResponseType.Normal, "");
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "CreateGame", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse                  DeleteGame          (string usersessionid, string gameid)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "DeleteGame", "Deleting game...");

            if (!sessionExists(usersessionid))
            {
                return constructResponse(GFXResponseType.NotFound, "User session not found!");
            }

            try
            {
                // get user from usersessionid
                var userInfo = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = '{0}'", usersessionid)))[0];
                var gameInfo = new List<GFXGameRow>(gameTable.Select(string.Format("GameId = '{0}'", gameid)))[0];

                if (gameInfo.Creator != userInfo.UserId)
                    return constructResponse(GFXResponseType.InvalidInput, "You cannot delete this game! Its not yours. >:(");

                gameTable.Remove(string.Format("Creator = '{0}' AND GameId = '{1}'", userInfo.UserId, gameid));

                return constructResponse(GFXResponseType.Normal, "");
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "CreateGame", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        // ----------------------------------------------------------------------------------------------------------------

        private GFXRestResponse                 constructResponse   (GFXResponseType responseType, string payload)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "constructResponse", "Returning result with type" + responseType + " and payload " + payload);

            return new GFXRestResponse { AdditionalData = payload, ResponseType = responseType };
        }

        private bool                            userExists          (string user)
        {
            Guid userId;

            if (Guid.TryParse(user, out userId))
                return userTable.Count(string.Format("userid = '{0}'", userId)) == 1;

            return this.userTable.Count(string.Format("username = '{0}'", user)) == 1;
        }

        private bool                            sessionExists       (string sessionid)
        {
            return loginTable.Count(string.Format("sessionid = '{0}'", sessionid)) == 1;
        }

        private Guid                            getUserId           (string input)
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
    }
}
