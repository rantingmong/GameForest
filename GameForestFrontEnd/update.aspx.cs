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
        private string  sessionId           = "";
        private string  gameId              = "";

        protected void  Page_Load           (object sender, EventArgs e)
        {
            var theQuery    = Request.Url.Query.Split('&');

            if (theQuery.Length < 2)
            {
                alertDialogError.Style["display"]   = "normal";
                alertDangerText.InnerHtml           = "Invalid game and session id.";

                return;
            }

            var gameIdQ     = theQuery[0].Split('=');
            var gameId      = gameIdQ[1];

            var sessIdQ     = theQuery[1].Split('=');
            var sessId      = sessIdQ[1];

            this.gameId     = gameId;
            this.sessionId  = sessId;

            HttpWebRequest getGameInfo = (HttpWebRequest)WebRequest.Create(string.Format("http://" + gameforestip.gameForestIP + ":1193/service/game/" + gameId));

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
        protected void  ButtonSubmit_Click  (object sender, EventArgs e)
        {
            var sessId      = this.sessionId;
            var userId      = "";

            var error       = false;
            var basePath    = AppDomain.CurrentDomain.BaseDirectory;

            HttpWebRequest userInfoFromSess = (HttpWebRequest)WebRequest.Create(string.Format("http://" + gameforestip.gameForestIP + ":1193/service/user/session/" + sessId));

            userInfoFromSess.Method         = "GET";
            userInfoFromSess.ContentLength  = 0;

            var response    = (HttpWebResponse)userInfoFromSess.GetResponse();
            var reader      = new StreamReader(response.GetResponseStream());

            var data        = JsonConvert.DeserializeObject<GFXRestResponse>(reader.ReadToEnd());

            if (data.ResponseType != GFXResponseType.Normal)
            {
                error = true;

                alertDialogError.Style["display"]   = "normal";
                alertDangerText.InnerHtml           = "There was something wrong with the server.";

                return;
            }

            var userInfo    = JsonConvert.DeserializeObject<Dictionary<string, object>>((string)data.AdditionalData);

            userId          = (string)userInfo["UserId"];

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
                    
                    alertDialogError.Style["display"]   = "normal";
                    alertDangerText.InnerHtml           = "File specified is not a zip file.";
                }
            }
            else
            {
                error = true;
                
                alertDialogError.Style["display"]   = "normal";
                alertDangerText.InnerHtml           = "Please specify your file (in zip format) for upload.";
            }

            if (error == false)
            {
                // process zip file and place to games folder
                try
                {
                    FastZip zipFile         = new FastZip();
                    string  extractedThings = Path.Combine(basePath, "game", userId, inputGameName.Text);

                    zipFile.ExtractZip(Path.Combine(basePath, "temp", userId, fileUpload.FileName),
                                        extractedThings,
                                        null);
                    // check for the folder that has index.html
                    string finalPath = "";
                    bool gfmain = findIndexHTML(extractedThings, out finalPath);

                    // if something is missing or broken, delete the directory and inform the user
                    if (!gfmain)
                    {
                        alertDialogError.Style["display"] = "normal";
                        alertDangerText.InnerHtml = "Your zip file does not contain the required gameforest files.";

                        Directory.Delete(extractedThings);
                        File.Delete(Path.Combine(basePath, "temp", fileUpload.FileName));

                        return;
                    }
                    else
                    {
                        Directory.Move(finalPath, extractedThings);
                    }
                }
                catch (ZipException)
                {
                    alertDialogError.Style["display"]   = "normal";
                    alertDangerText.InnerHtml           = "Zip file decompression failed. Please check if your the file is a valid zip file.";

                    if (File.Exists(Path.Combine(basePath, "temp", fileUpload.FileName)))
                        File.Delete(Path.Combine(basePath, "temp", fileUpload.FileName));

                    return;
                }

                // replace this to update

                // send REST request to server
                HttpWebRequest newGameRequest = (HttpWebRequest)WebRequest.Create(string.Format("http://" + gameforestip.gameForestIP + ":1193/service/game?name={0}&description={1}&minplayers={2}&maxplayers={3}&usersessionid={4}&gameid={5}",
                                                                                                inputGameName.Text,
                                                                                                inputGameDescription.Text,
                                                                                                inputMinPlayers.Text,
                                                                                                inputMaxPlayers.Text,
                                                                                                Guid.Parse(sessId),
                                                                                                Guid.Parse(gameId)));

                newGameRequest.Method           = "PUT";
                newGameRequest.ContentLength    = 0;

                response    = (HttpWebResponse)newGameRequest.GetResponse();
                reader      = new StreamReader(response.GetResponseStream());

                var respose = JsonConvert.DeserializeObject<GFXRestResponse>(reader.ReadToEnd());

                if (respose.ResponseType == GFXResponseType.Normal)
                {
                    alertDialogAllOk.Style["display"]   = "normal";
                    alertAllOkText.InnerHtml            = "Game creation succesful! The page will now go back to the games page.";
                }
                else
                {
                    alertDialogError.Style["display"]   = "normal";
                    alertDangerText.InnerHtml           = "Game creation was not successful.";
                }
            }
        }

        private bool findIndexHTML(string basePath, out string outpath)
        {
            // from root, check if there's an index.html file
            foreach (string file in Directory.GetFiles(basePath))
            {
                if (Path.GetFileName(file).ToLower() == "index.html")
                {
                    outpath = file;

                    return true;
                }
            }

            // get this directory's folders and search for an index.html there
            foreach (string dir in Directory.GetDirectories(basePath))
            {
                if (findIndexHTML(dir, out outpath))
                {
                    outpath = dir;
                    return true;
                }
            }

            outpath = "";

            return false;
        }
    }
}
