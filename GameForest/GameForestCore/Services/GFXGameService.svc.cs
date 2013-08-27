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
        private GFXLogger                       logger;
        private GFXDatabaseTable<GFXGameRow>    gameTable;

        public GFXGameService                   ()
        {
            logger      = new GFXLogger("game service");
            gameTable   = new GFXDatabaseTable<GFXGameRow>(new GFXGameRowTranslator());
        }

        public GFXGameService                   (GFXLogger gameLogger)
        {
            logger      = gameLogger;
            gameTable   = new GFXDatabaseTable<GFXGameRow>(new GFXGameRowTranslator());
        }

        public GFXRestResponse                  GetGameList         (int maxCount)
        {
            logger.Log(GFXLoggerLevel.INFO, "GetGameList", "Fetching game list...");

            try
            {
                List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select("", maxCount));

                return constructResponse(GFXResponseType.Normal, JsonConvert.SerializeObject(gameList));
            }
            catch (Exception exp)
            {
                logger.Log(GFXLoggerLevel.FATAL, "GetGameList", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public GFXRestResponse                  GetGameInfo         (string gameId)
        {
            logger.Log(GFXLoggerLevel.INFO, "GetGameInfo", "Fetching game info...");

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
                logger.Log(GFXLoggerLevel.INFO, "GetGameInfo", exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        private GFXRestResponse                 constructResponse   (GFXResponseType responseType, string payload)
        {
            logger.Log(GFXLoggerLevel.INFO, "constructResponse", "Returning result with type" + responseType + " and payload " + payload);

            return new GFXRestResponse { AdditionalData = payload, ResponseType = responseType };
        }
    }
}
