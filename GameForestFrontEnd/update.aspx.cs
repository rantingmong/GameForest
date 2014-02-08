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
        private String basePath;

        private String downloadTempPath;
        private String extractsTempPath;
        private String usrDirectoryPath;

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
            var     sessId      = this.sessionId;
            var     userId      = "";

            Guid    fileId      = Guid.Parse(gameId);

            String  filePath    = "";
            String  extrPath    = "";
            String  gamePath    = "";

            String  htmlPath    = "";

            HttpWebRequest userInfoFromSess = (HttpWebRequest)WebRequest.Create(string.Format("http://" + gameforestip.gameForestIP + ":1193/service/user/session/" + sessId));

            userInfoFromSess.Method         = "GET";
            userInfoFromSess.ContentLength  = 0;

            var response    = (HttpWebResponse)userInfoFromSess.GetResponse();
            var reader      = new StreamReader(response.GetResponseStream());

            var data        = JsonConvert.DeserializeObject<GFXRestResponse>(reader.ReadToEnd());

            if (data.ResponseType != GFXResponseType.Normal)
            {
                alertDialogError.Style["display"]   = "normal";
                alertDangerText.InnerHtml           = "There was something wrong with the server.";

                return;
            }

            var userInfo    = JsonConvert.DeserializeObject<Dictionary<string, object>>((string)data.AdditionalData);
                userId      = (string)userInfo["UserId"];

            try
            {
                createPaths(userId);
                createDirectories();

                // check if file uploader has files to be saved
                if (fileUpload.HasFile == false)
                {
                    alertDialogError.Style["display"]   = "normal";
                    alertDangerText.InnerHtml           = "You haven't specified anything to upload.";

                    return;
                }

                // first get file extension
                if (Path.GetExtension(fileUpload.FileName) != "zip")
                {
                    alertDialogError.Style["display"]   = "normal";
                    alertDangerText.InnerHtml           = "File specified is not a zip file.";

                    return;
                }

                filePath = Path.Combine(downloadTempPath, fileId.ToString() + ".zip");
                extrPath = Path.Combine(extractsTempPath, fileId.ToString());
                gamePath = Path.Combine(usrDirectoryPath, fileId.ToString());

                htmlPath = "";

                // save file
                fileUpload.SaveAs(filePath);

                // process zip file and place it in extractsTempPath/fileId
                new FastZip().ExtractZip(filePath, extrPath, null);

                // check the extracted files for correctness
                if (findIndexHTML(extrPath, out htmlPath) == false)
                {
                    alertDialogError.Style["display"]   = "normal";
                    alertDangerText.InnerHtml           = "There is no index.html specified in the zip file you uploaded.";

                    Directory.Delete(extrPath);
                    File.Delete(filePath);

                    return;
                }

                // move files from extrPath to userDirectoryPath/fileId

                if (Directory.Exists(gamePath))
                {
                    Directory.Delete(gamePath);
                    Directory.CreateDirectory(gamePath);
                }

                Directory.CreateDirectory(gamePath);
                Directory.Move(extrPath, gamePath);

                // delete downloaded file awhile ago
                File.Delete(filePath);

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

                    Directory.Delete(gamePath);
                }
            }
            catch (ZipException)
            {
                alertDialogError.Style["display"]   = "normal";
                alertDangerText.InnerHtml           = "Zip file decompression failed. Please check if your the file is a valid zip file.";

                if (File.Exists(filePath))
                    File.Delete(filePath);

                if (Directory.Exists(extrPath))
                    Directory.Delete(extrPath);
            }
            catch (Exception exp)
            {
                alertDialogError.Style["display"]   = "normal";
                alertDangerText.InnerHtml           = "GameForest encountered a serious error. Server-side message: " + exp.Message;

                Directory.Delete(gamePath);
            }
        }

        private void    createPaths         (string userId)
        {
            basePath            = AppDomain.CurrentDomain.BaseDirectory;

            downloadTempPath    = Path.Combine(basePath, "downloadtemp");
            extractsTempPath    = Path.Combine(basePath, "extractstemp");
            usrDirectoryPath    = Path.Combine(basePath, "gamdirectory", userId);
        }
        private void    createDirectories   ()
        {
            // check if base directory for the downloaded files is available
            if (Directory.Exists(downloadTempPath) == false)
            {
                Directory.CreateDirectory(downloadTempPath);
            }

            // check if base directory for extracted files is available
            if (Directory.Exists(extractsTempPath) == false)
            {
                Directory.CreateDirectory(extractsTempPath);
            }

            // check if user's directory is available
            if (Directory.Exists(usrDirectoryPath) == false)
            {
                Directory.CreateDirectory(usrDirectoryPath);
            }
        }

        private bool    findIndexHTML       (string basePath, out string outpath)
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
