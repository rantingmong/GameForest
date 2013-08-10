using GameForestCore.Common;
using GameForestCore.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace GameForestCore.Services
{
    public class GFXGameService : IGFXGameService
    {
        private         GFXDatabaseTable<GFXGameRow>    gameTable;

        public          GFXGameService                  ()
        {
            gameTable = new GFXDatabaseTable<GFXGameRow>(new GFXGameRowTranslator());
        }

        public          GFXRestResponse                 GetGameList         (int maxCount)
        {
            try
            {
                List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select("", maxCount));

                return new GFXRestResponse
                    {
                        ResponseType    = GFXResponseType.Normal,
                        AdditionalData  = JsonConvert.SerializeObject(gameList)
                    };
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[Game|GameGameList] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        public          GFXRestResponse                 GetGameInfo         (string gameId)
        {
            try
            {
                List<GFXGameRow> gameList = new List<GFXGameRow>(gameTable.Select(string.Format("GameId = {0}", gameId)));

                return new GFXRestResponse
                {
                    ResponseType    = GFXResponseType.Normal,
                    AdditionalData  = JsonConvert.SerializeObject(gameList)
                };
            }
            catch (Exception exp)
            {
                Console.Error.WriteLine("[Game|GetGameInfo] " + exp.Message);

                return constructResponse(GFXResponseType.RuntimeError, exp.Message);
            }
        }

        private static  GFXRestResponse                 constructResponse   (GFXResponseType responseType, string payload)
        {
            return new GFXRestResponse { AdditionalData = payload, ResponseType = responseType };
        }
    }
}
