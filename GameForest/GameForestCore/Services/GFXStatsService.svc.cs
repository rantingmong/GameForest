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
    public class GFXStatsService : IGFXStatsService
    {
        private GFXDatabaseTable<GFXGameRow>        gameTable;
        private GFXDatabaseTable<GFXUserRow>        userTable;
        private GFXDatabaseTable<GFXLoginRow>       loginTable;
        private GFXDatabaseTable<GFXStatRow>        statTable;
        private GFXDatabaseTable<GFXUserStatRow>    userStats;

        public GFXStatsService ()
        {
            userTable   = new GFXDatabaseTable<GFXUserRow>(new GFXUserRowTranslator());
            gameTable   = new GFXDatabaseTable<GFXGameRow>(new GFXGameRowTranslator());
            loginTable  = new GFXDatabaseTable<GFXLoginRow>(new GFXLoginRowTranslator());
            statTable   = new GFXDatabaseTable<GFXStatRow>(new GFXStatRowTranslator());
            userStats   = new GFXDatabaseTable<GFXUserStatRow>(new GFXUserStatRowTranslator());
        }

        public GFXRestResponse AddStat(string statname, string gameId)
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

        public GFXRestResponse AddUserStat(string statname, string gameId, string sessionId)
        {
            List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select(string.Format("GameId = '{0}'", gameId)));
            var statGuid = Guid.NewGuid();

            if (gameList.Count <= 0)
                return constructResponse(GFXResponseType.NotSupported, "Game ID does not exist");

            if (!sessionExists(sessionId))
                return constructResponse(GFXResponseType.NotFound, "Session ID not found");

            try
            {
                if ((userStats.Count(string.Format("stat_name = '{0}' AND UserId = '{1}'", statname, getUserId(sessionId))) == 0))
                {
                    GFXUserStatRow statistic = new GFXUserStatRow
                    {
                        ustat_id = statGuid,
                        GameID = Guid.Parse(gameId),
                        UserId = getUserId(sessionId),
                        stat_name = statname,
                        stat_value = 0
                    };

                    userStats.Insert(statistic);

                    var userStatList = new List<GFXUserStatRow>(userStats.Select(string.Format("stat_name = '{0}' AND GameID = '{1}' AND UserId = '{2}'",
                                                                    statname, gameId, getUserId(sessionId))));

                    return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(userStatList[0]));
                }
                else
                {
                    var userStatList = new List<GFXUserStatRow>(userStats.Select(string.Format("stat_name = '{0}' AND GameID = '{1}' AND UserId = '{2}'",
                                                                                    statname, gameId, getUserId(sessionId))));

                    return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(userStatList));
                }
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "AddUserStat", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }

        }

        public GFXRestResponse GetStat(string stat, string gameId, bool allcheck)
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
                else if (allcheck)
                {
                    if (statTable.Count(string.Format("GameID = '{0}'", gameId)) > 0)
                    {
                        return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(statTable.Select(string.Format("GameID = '{0}'", gameId))));
                    }
                    else
                    {
                        return constructResponse(GFXResponseType.NotFound, "No statistics being tracked");
                    }
                }
                else
                {
                    return constructResponse(GFXResponseType.InvalidInput, "allcheck invalid");
                }
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetStat", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse GetUserStat(string stat, string gameId, string sessionId, bool allcheck)
        {
            List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select(string.Format("GameId = '{0}'", gameId)));

            if (gameList.Count <= 0)
                return constructResponse(GFXResponseType.NotSupported, "Game and/or User ID does not exist");

            if (!sessionExists(sessionId))
                return constructResponse(GFXResponseType.NotFound, "Session ID not found");

            try
            {
                string returnJSON;

                if (allcheck == false)
                {
                    if (userStats.Count(string.Format("stat_name = '{0}' AND GameID = '{1}' AND UserId = '{2}'",
                                            stat, gameId, getUserId(sessionId))) == 1)
                    {
                        var result = new List<GFXUserStatRow>(userStats.Select(string.Format("stat_name = '{0}' AND GameID = '{1}' AND UserId = '{2}'",
                                            stat, gameId, getUserId(sessionId))));

                        returnJSON = JsonConvert.SerializeObject(result[0]);

                        return constructResponse(GFXResponseType.Normal, returnJSON);
                    }
                    else
                    {
                        return constructResponse(GFXResponseType.NotFound, "Couldn't find user statistic");
                    }
                }
                else if (allcheck)
                {
                    if (userStats.Count(string.Format("GameID = '{0}' AND UserId = '{1}'", gameId, getUserId(sessionId))) > 0)
                    {
                        var result = new List<GFXUserStatRow>(userStats.Select(string.Format("GameID = '{0}' AND UserID = '{1}'", gameId, getUserId(sessionId))));

                        returnJSON = JsonConvert.SerializeObject(result);

                        return constructResponse(GFXResponseType.Normal, returnJSON);
                    }
                    else
                        return constructResponse(GFXResponseType.NotFound, "No user statistics being tracked");
                }
                else
                {
                    return constructResponse(GFXResponseType.InvalidInput, "allcheck invalid");
                }
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "GetUserStat", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);   
            }
        }

        public GFXRestResponse UpdateStat(string statName, string gameId, int stat_value)
        {
            List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select(string.Format("GameId = '{0}'", gameId)));

            if (gameList.Count <= 0)
                return constructResponse(GFXResponseType.NotSupported, "Game ID does not exist");

            try
            {
                var result = new List<GFXStatRow>(statTable.Select(string.Format("stat_name = '{0}' AND GameId = '{1}'", statName, gameId)))[0];

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

        public GFXRestResponse UpdateUserStat(string statName, string gameId, string username, int stat_value)
        {
            List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select(string.Format("GameId = '{0}'", gameId)));

            if (gameList.Count <= 0)
                return constructResponse(GFXResponseType.NotSupported, "Game and/or User ID does not exist");

            if (!userExists(username))
                return constructResponse(GFXResponseType.NotFound, "user not found");

            try
            {
                var result = new List<GFXUserStatRow>(userStats.Select(string.Format("gameId = '{0}' AND userId = '{1}' AND stat_name = '{2}'",
                                                                                gameId, getUserId(username), statName)))[0];

                result.stat_value = stat_value;

                userStats.Update(string.Format("gameId = '{0}' AND userId = '{1}' AND stat_name = '{2}'", gameId, getUserId(username), statName), result);
            }
            catch (Exception exp)
            {
                GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "UpdateUserStat", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }

            return constructResponse(GFXResponseType.Normal, "Update push success!");
        }

        // ----------------------------------------------------------------------------------------------------------------

        private GFXRestResponse constructResponse(GFXResponseType responseType, string payload)
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "constructResponse", "Returning result with type" + responseType + " and payload " + payload);

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
    }
}
