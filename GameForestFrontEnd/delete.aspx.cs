using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GameForestFE
{
    public partial class delete : System.Web.UI.Page
    {
        private string sessionId    = "";
        private string gameId       = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            var theQuery    = Request.Url.Query.Split('&');

            if (theQuery.Length < 2)
            {
                rowDelete.Style["display"]  = "none";
                rowError.Style["display"]   = "normal";

                errorHeader.InnerHtml       = " Invalid game and session id.";

                return;
            }

            var gameIdQ     = theQuery[0].Split('=');
            var gameId      = gameIdQ[1];

            var sessIdQ     = theQuery[1].Split('=');
            var sessId      = sessIdQ[1];

            this.gameId     = gameId;
            this.sessionId  = sessId;

            HttpWebRequest getGameInfo  = (HttpWebRequest)WebRequest.Create(string.Format("http://" + gameforestip.gameForestIP + ":1193/service/game/" + gameId));

            getGameInfo.Method          = "GET";
            getGameInfo.ContentLength   = 0;

            var response        = (HttpWebResponse)getGameInfo.GetResponse();
            var reader          = new StreamReader(response.GetResponseStream());

            var data            = JsonConvert.DeserializeObject<GFXRestResponse>(reader.ReadToEnd());
            var gameInfo        = JsonConvert.DeserializeObject<Dictionary<string, object>>((string)data.AdditionalData);

            gameName.InnerText  = (string)gameInfo["Name"];
        }

        protected void buttonDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // request a delete command to server
                HttpWebRequest deleteGame   = (HttpWebRequest)WebRequest.Create(string.Format("http://" + gameforestip.gameForestIP + ":1193/service/game?gameid={0}&usersessionid={1}", gameId, sessionId));

                deleteGame.Method           = "DELETE";
                deleteGame.ContentLength    = 0;

                var response    = deleteGame.GetResponse();
                var reader      = new StreamReader(response.GetResponseStream());

                var data        = JsonConvert.DeserializeObject<GFXRestResponse>(reader.ReadToEnd());
                
                if (data.ResponseType == GFXResponseType.Normal)
                {
                    // delete the game from the directory

                    rowDelete.Style["display"]  = "none";
                    rowError.Style["display"]   = "normal";

                    errorHeader.InnerHtml       = "Game deleted from server!";
                }
                else
                {
                    rowDelete.Style["display"]  = "none";
                    rowError.Style["display"]   = "normal";

                    errorHeader.InnerHtml       = "GameForest is having problems deleting your game: " + data.AdditionalData.ToString();
                }
            }
            catch (Exception exp)
            {
                rowDelete.Style["display"]  = "none";
                rowError.Style["display"]   = "normal";

                errorHeader.InnerHtml       = "FATAL ERROR! " + exp.Message;
            }
        }
    }
}
