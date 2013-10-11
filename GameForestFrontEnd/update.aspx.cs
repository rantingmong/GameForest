using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.UI;

namespace GameForestFE
{
    public partial class update : Page
    {
        private string sessionId    = "";
        private string gameId       = "";

        protected void Page_Load            (object sender, EventArgs e)
        {
            var theQuery    = Request.Url.Query.Split('&');

            var gameIdQ     = theQuery[0].Split('=');
            var gameId      = gameIdQ[1];

            var sessIdQ     = theQuery[1].Split('=');
            var sessId      = sessIdQ[1];

            this.gameId     = gameId;
            this.sessionId  = sessId;

            HttpWebRequest getGameInfo = (HttpWebRequest)WebRequest.Create(string.Format("http://localhost:1193/service/game/" + gameId));

            getGameInfo.Method          = "GET";
            getGameInfo.ContentLength   = 0;

            var response    = (HttpWebResponse)getGameInfo.GetResponse();
            var reader      = new StreamReader(response.GetResponseStream());

            var data        = JsonConvert.DeserializeObject<GFXRestResponse>(reader.ReadToEnd());
            var gameInfo    = JsonConvert.DeserializeObject<Dictionary<string, object>>((string)data.AdditionalData);

            inputGameDescription.Text   = (string)gameInfo["Description"];
            inputGameName.Text          = (string)gameInfo["Name"];
            inputMaxPlayers.Text        = Convert.ToString(gameInfo["MaxPlayers"]);
            inputMinPlayers.Text        = Convert.ToString(gameInfo["MinPlayers"]);

            gameName.InnerText          = (string)gameInfo["Name"];
        }

        protected void ButtonSubmit_Click   (object sender, EventArgs e)
        {
            var sessId      = this.sessionId;
            var userId      = "";

            var error       = false;
            var basePath    = AppDomain.CurrentDomain.BaseDirectory;

            HttpWebRequest userInfoFromSess = (HttpWebRequest)WebRequest.Create(string.Format("http://localhost:1193/service/user/session/" + sessId));

            userInfoFromSess.Method         = "GET";
            userInfoFromSess.ContentLength  = 0;

            var response    = (HttpWebResponse)userInfoFromSess.GetResponse();
            var reader      = new StreamReader(response.GetResponseStream());

            var data        = JsonConvert.DeserializeObject<GFXRestResponse>(reader.ReadToEnd());
            var userInfo    = JsonConvert.DeserializeObject<Dictionary<string, object>>((string)data.AdditionalData);

            userId          = (string)userInfo["UserID"];

            if (fileUpload.HasFile)
            {
                string ext = Path.GetExtension(fileUpload.FileName);

                if (ext.Contains("zip"))
                {
                    if (!Directory.Exists(Path.Combine(basePath, "temp")))
                    {
                        Directory.CreateDirectory(Path.Combine(basePath, "temp"));
                    }

                    if (!Directory.Exists(Path.Combine(basePath, "temp", userId)))
                    {
                        Directory.CreateDirectory(Path.Combine(basePath, "temp", userId));
                    }

                    fileUpload.SaveAs(Path.Combine(basePath, "temp", userId, fileUpload.FileName));
                }
                else
                {
                    error = true;

                    alertDialog.Style["display"] = "normal";
                    alertDialog.InnerHtml = "<p class='glyphicon glyphicon-warning-sign' style='margin-right:10px'></p>File specified is not a zip file.";
                }
            }
            else
            {
                error = true;

                alertDialog.Style["display"] = "normal";
                alertDialog.InnerHtml = "<p class='glyphicon glyphicon-warning-sign' style='margin-right:10px'></p>Please specify your file (in zip format) for upload.";
            }

            if (error == false)
            {
                // process zip file and place to games folder
                try
                {
                    FastZip zipFile = new FastZip();
                    zipFile.ExtractZip(Path.Combine(basePath, "temp", userId, fileUpload.FileName),
                                       Path.Combine(basePath, "game", userId, inputGameName.Text),
                                       null);

                    bool gfjsok = false;
                    bool gfmain = false;

                    // inspect the newly decompressed game
                    foreach (string file in Directory.GetFiles(Path.Combine(basePath, "game", userId, inputGameName.Text)))
                    {
                        if (file.ToLower().Contains("gameforest.js"))
                            gfjsok = true;

                        if (file.ToLower().Contains("index.html"))
                            gfmain = true;
                    }

                    // if something is missing or broken, delete the directory and inform the user
                    if (!gfjsok || !gfmain)
                    {
                        alertDialog.Attributes["class"] = "alert alert-danger";
                        alertDialog.Style["display"] = "normal";
                        alertDialog.InnerHtml = "<p class='glyphicon glyphicon-warning-sign' style='margin-right:10px'></p>Your zip file does not contain the required gameforest files.";

                        Directory.Delete(Path.Combine("games", userId, inputGameName.Text));
                        File.Delete(Path.Combine(basePath, "temp", fileUpload.FileName));

                        return;
                    }
                }
                catch (ZipException)
                {
                    alertDialog.Style["display"] = "normal";
                    alertDialog.InnerHtml = "<p class='glyphicon glyphicon-warning-sign' style='margin-right:10px'></p>Zip file decompression failed. Please check if your the file is a valid zip file.";

                    if (File.Exists(Path.Combine(basePath, "temp", fileUpload.FileName)))
                        File.Delete(Path.Combine(basePath, "temp", fileUpload.FileName));

                    return;
                }

                // replace this to update

                // send REST request to server
                HttpWebRequest newGameRequest = (HttpWebRequest)WebRequest.Create(string.Format("http://localhost:1193/service/game?name={0}&description={1}&minplayers={2}&maxplayers={3}&usersessionid={4}",
                                                                                                inputGameName.Text,
                                                                                                inputGameDescription.Text,
                                                                                                inputMinPlayers.Text,
                                                                                                inputMaxPlayers.Text,
                                                                                                Guid.Parse(sessId)));

                newGameRequest.Method = "POST";
                newGameRequest.ContentLength = 0;

                response    = (HttpWebResponse)newGameRequest.GetResponse();
                reader      = new StreamReader(response.GetResponseStream());

                var respose = JsonConvert.DeserializeObject<GFXRestResponse>(reader.ReadToEnd());

                if (respose.ResponseType == GFXResponseType.Normal)
                {
                    alertDialog.Attributes["class"]     = "alert alert-success";
                    alertDialog.Style["display"]        = "normal";
                    alertDialog.InnerHtml               = "<p class='glyphicon glyphicon-warning-sign' style='margin-right:10px'></p>Game creation succesful! The page will now go back to the games page.";
                }
                else
                {
                    alertDialog.Attributes["class"]     = "alert alert-danger";
                    alertDialog.Style["display"]        = "normal";
                    alertDialog.InnerHtml               = "<p class='glyphicon glyphicon-warning-sign' style='margin-right:10px'></p>Game creation was not successful.";
                }
            }
        }
    }
}
