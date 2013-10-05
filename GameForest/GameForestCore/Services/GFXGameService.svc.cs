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
        private GFXDatabaseTable<GFXStatRow>    statTable;

        public GFXGameService                   ()
        {
            userTable   = new GFXDatabaseTable<GFXUserRow>(new GFXUserRowTranslator());
            gameTable   = new GFXDatabaseTable<GFXGameRow>(new GFXGameRowTranslator());
            loginTable  = new GFXDatabaseTable<GFXLoginRow>(new GFXLoginRowTranslator());
            statTable   = new GFXDatabaseTable<GFXStatRow>(new GFXStatRowTranslator());
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
                    var user = new List<GFXUserRow>( userTable.Select(string.Format("UserId = '{0}'", game.Creator)));

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

        public GFXRestResponse                  GetUserGames        (string username)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "GetUserGames", "Fetching user's game list...");

            try
            {
                List<GFXUserRow> userList = new List<GFXUserRow>(userTable.Select(string.Format("Username = '{0}'", username)));
                List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select(string.Format("Creator = '{0}'", userList[0].UserId)));

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

        public GFXRestResponse                  AddStat             (string statname, string gameId)
        {
            List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select(string.Format("GameId = '{0}'", gameId)));
            var statGuid = Guid.NewGuid();

            if (gameList.Count <= 0)
                return constructResponse(GFXResponseType.NotSupported, "Game ID does not exist");

            try
            {
                if ((statTable.Count(string.Format("stat_id = '{0}'", statGuid)) == 0) &&
                    (statTable.Count(string.Format("stat_name = '{0}'", statname)) == 0))
                {
                    GFXStatRow statistic = new GFXStatRow
                    {
                        stat_id = statGuid,
                        GameID = Guid.Parse(gameId),
                        stat_name = statname,
                        stat_value = 0
                    };

                    statTable.Insert(statistic);

                    var statList = new List<GFXStatRow>(statTable.Select(string.Format("stat_name = '{0}' AND GameID = '{1}'", statname, gameId)));

                    return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(statList[0]));
                }
                else
                {
                    var statList = new List<GFXStatRow>(statTable.Select(string.Format("stat_name = '{0}'", statname)));

                    return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(statList[0]));
                }
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "AddStat", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse                  GetStat             (string stat, string gameId, bool allcheck)
        {

            List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select(string.Format("GameId = '{0}'", gameId)));

            if (gameList.Count <= 0)
                return constructResponse(GFXResponseType.NotSupported, "Game ID does not exist");

            try
            {
                string returnJSON;

                if (allcheck == false)
                {
                    if ((statTable.Count(string.Format("GameID = '{0}'", gameId)) > 0) &&
                        (statTable.Count(string.Format("stat_name = '{0}'", stat)) == 1))
                    {
                        var result = new List<GFXStatRow>(statTable.Select(string.Format("stat_name = '{0}' AND GameID = '{1}'", stat, gameId)));

                        returnJSON = JsonConvert.SerializeObject(result[0]);

                        return constructResponse(GFXResponseType.Normal, returnJSON);
                    }
                    else
                    {
                        return constructResponse(GFXResponseType.NotFound, "Couldn't find statistic");
                    }
                }
                else if (allcheck == true)
                {
                    if (statTable.Count(string.Format("GameID = '{0}'", gameId)) > 0)
                    {
                        return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(statTable.Select(string.Format("GameID = '{0}'", gameId))));
                    }
                    else
                    {
                        return constructResponse(GFXResponseType.NotFound, "No statistics being tracked");
                    }
                } else {
                    return constructResponse(GFXResponseType.InvalidInput, "allcheck invalid");
                }
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetStat", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse                  UpdateStat          (string statName, string gameId, int stat_value)
        {
            List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select(string.Format("GameId = '{0}'", gameId)));

            if (gameList.Count <= 0)
                return constructResponse(GFXResponseType.NotSupported, "Game ID does not exist");

            try
            {
                var result = new List<GFXStatRow>(statTable.Select(string.Format("stat_name = '{0}'", statName)))[0];

                result.stat_value = stat_value;

                statTable.Update(string.Format("stat_name = '{0}'", statName), result);
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "UpdateStat", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }

            return constructResponse(GFXResponseType.Normal, "Update push success!");
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
