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

                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(gameList));
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
                GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "GetGameInfo", exp.Message);

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
                GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "CreateGame", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

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
