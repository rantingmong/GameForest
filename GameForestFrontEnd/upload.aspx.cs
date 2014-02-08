using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.UI;

namespace GameForestFE
{
    public partial class upload : Page
    {
        private String basePath;

        private String downloadTempPath;
        private String extractsTempPath;
        private String usrDirectoryPath;

        public upload()
        {
        }

        protected void  Page_Load           (object sender, EventArgs e)
        {
            inputUserId.Attributes.Add      ("readonly", "readonly");
            inputSessionId.Attributes.Add   ("readonly", "readonly");
        }
        protected void  ButtonSubmit_Click  (object sender, EventArgs e)
        {
            Guid    fileId      = Guid.NewGuid();

            String  filePath    = "";
            String  extrPath    = "";
            String  gamePath    = "";

            String  htmlPath    = "";

            try
            {
                createPaths         ();
                createDirectories   ();

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

                filePath    = Path.Combine(downloadTempPath, fileId.ToString() + ".zip");
                extrPath    = Path.Combine(extractsTempPath, fileId.ToString());
                gamePath    = Path.Combine(usrDirectoryPath, fileId.ToString());

                htmlPath    = "";

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

                Directory.CreateDirectory   (gamePath);
                Directory.Move              (extrPath, gamePath);

                // delete downloaded file awhile ago
                File.Delete                 (filePath);

                // send REST request to server
                HttpWebRequest newGameRequest = (HttpWebRequest)WebRequest.Create(string.Format("http://" + gameforestip.gameForestIP + ":1193/service/game?name={0}&description={1}&minplayers={2}&maxplayers={3}&usersessionid={4}&fileid={5}",
                                                                                                inputGameName.Text,
                                                                                                inputDescription.Text,
                                                                                                inputMinPlayers.Text,
                                                                                                inputMaxPlayers.Text,
                                                                                                Guid.Parse(inputSessionId.Text),
                                                                                                fileId));

                newGameRequest.Method           = "POST";
                newGameRequest.ContentLength    = 0;

                var response    = (HttpWebResponse)newGameRequest.GetResponse();
                var reader      = new StreamReader(response.GetResponseStream());

                var respose     = JsonConvert.DeserializeObject<GFXRestResponse>(reader.ReadToEnd());

                if (respose.ResponseType == GFXResponseType.Normal)
                {
                    // inform user we are done processing
                    alertDialogAllOk.Style["display"]   = "normal";
                    alertDialogAllOk.InnerHtml          = " Game creation succesful! The page will now go back to the games page.";
                }
                else
                {
                    // read response, if it fails, inform the user and don't continue and delete the uploaded file.
                        
                    alertDialogError.Style["display"]   = "normal";
                    alertDangerText.InnerHtml           = " Game creation was not successful. " + respose.AdditionalData;

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

        private void    createPaths         ()
        {
            basePath            = AppDomain.CurrentDomain.BaseDirectory;

            downloadTempPath    = Path.Combine(basePath, "downloadtemp");
            extractsTempPath    = Path.Combine(basePath, "extractstemp");
            usrDirectoryPath    = Path.Combine(basePath, "gamdirectory", inputUserId.Text);
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
