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
        protected void Page_Load(object sender, EventArgs e)
        {
            inputUserId.Attributes.Add("readonly", "readonly");
            inputSessionId.Attributes.Add("readonly", "readonly");
        }
        protected void ButtonSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                var error       = false;
                var basePath    = AppDomain.CurrentDomain.BaseDirectory;

                if (fileUpload.HasFile)
                {
                    string ext = Path.GetExtension(fileUpload.FileName);

                    if (ext.Contains("zip"))
                    {
                        if (!Directory.Exists(Path.Combine(basePath, "temp")))
                        {
                            Directory.CreateDirectory(Path.Combine(basePath, "temp"));
                        }

                        if (!Directory.Exists(Path.Combine(basePath, "temp", inputUserId.Text)))
                        {
                            Directory.CreateDirectory(Path.Combine(basePath, "temp", inputUserId.Text));
                        }

                        fileUpload.SaveAs(Path.Combine(basePath, "temp", inputUserId.Text, fileUpload.FileName));
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
                        string  extractedThings = Path.Combine(basePath, "game", inputUserId.Text, inputGameName.Text);

                        zipFile.ExtractZip(Path.Combine(basePath, "temp", inputUserId.Text, fileUpload.FileName),
                                           extractedThings,
                                           null);

                        // check for the folder that has index.html
                        bool gfmain = findIndexHTML(extractedThings);

                        // if something is missing or broken, delete the directory and inform the user
                        if (!gfmain)
                        {
                            alertDialogError.Style["display"]   = "normal";
                            alertDangerText.InnerHtml           = "Your zip file does not contain the required gameforest files.";

                            Directory.Delete(extractedThings);
                            File.Delete     (Path.Combine(basePath, "temp", fileUpload.FileName));

                            return;
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

                    // send REST request to server
                    HttpWebRequest newGameRequest = (HttpWebRequest)WebRequest.Create(string.Format("http://" + gameforestip.gameForestIP + ":1193/service/game?name={0}&description={1}&minplayers={2}&maxplayers={3}&usersessionid={4}",
                                                                                                    inputGameName.Text,
                                                                                                    inputDescription.Text,
                                                                                                    inputMinPlayers.Text,
                                                                                                    inputMaxPlayers.Text,
                                                                                                    Guid.Parse(inputSessionId.Text)));

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
                    }
                }
            }
            catch (Exception exp)
            {
                alertDialogError.Style["display"]   = "normal";
                alertDangerText.InnerHtml           = "FATAL ERROR! " + exp.Message + "<br/>" + exp.StackTrace;
            }
        }

        private bool findIndexHTML(string basePath)
        {
            // from root, check if there's an index.html file
            foreach (string file in Directory.GetFiles(basePath))
            {
                if (Path.GetFileName(file).ToLower() == "index.html")
                {
                    return true;
                }
            }

            // get this directory's folders and search for an index.html there

            bool indexHTMLFound = false;

            foreach (string dir in Directory.GetDirectories(basePath))
            {
                if (findIndexHTML(basePath))
                {
                    indexHTMLFound = true;
                    break;
                }
            }

            return indexHTMLFound;
        }
    }
}
