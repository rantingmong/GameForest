using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

using System;
using System.IO;
using System.Net;
using System.Web.UI;

public partial class upload : Page
{
    public enum GFXResponseType
    {
        Normal          = 0,
        InvalidInput    = 1,

        NotFound        = 10,
        DuplicateEntry  = 11,

        FatalError      = 30,
        RuntimeError    = 31,

        NotSupported    = 50,
    }

    public struct GFXRestResponse
    {
        public GFXResponseType  ResponseType    { get; set; }
        public object           AdditionalData  { get; set; }
    }

    protected void Page_Load            (object sender, EventArgs e)
    {
        inputUserId.Attributes.Add("readonly", "readonly");
        inputSessionId.Attributes.Add("readonly", "readonly");
    }
    protected void ButtonSubmit_Click   (object sender, EventArgs e)
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
                zipFile.ExtractZip(Path.Combine(basePath, "temp", inputUserId.Text, fileUpload.FileName),
                                   Path.Combine(basePath, "game", inputUserId.Text, inputGameName.Text),
                                   null);

                bool gfjsok = false;
                bool gfmain = false;

                // inspect the newly decompressed game
                foreach (string file in Directory.GetFiles(Path.Combine(basePath, "game", inputUserId.Text, inputGameName.Text)))
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

                    Directory.Delete(Path.Combine("games", inputUserId.Text, inputGameName.Text));
                    File.Delete(Path.Combine(basePath, "temp", fileUpload.FileName));

                    return;
                }
            }
            catch (ZipException zipException)
            {
                alertDialog.Style["display"] = "normal";
                alertDialog.InnerHtml = "<p class='glyphicon glyphicon-warning-sign' style='margin-right:10px'></p>Zip file decompression failed. Please check if your the file is a valid zip file.";

                if (File.Exists(Path.Combine(basePath, "temp", fileUpload.FileName)))
                    File.Delete(Path.Combine(basePath, "temp", fileUpload.FileName));

                return;
            }

            // send REST request to server
            HttpWebRequest newGameRequest = (HttpWebRequest)WebRequest.Create(string.Format("http://localhost:1193/service/game?name={0}&description={1}&minplayers={2}&maxplayers={3}&usersessionid={4}",
                                                                                            inputGameName.Text,
                                                                                            inputDescription.Text,
                                                                                            inputMinPlayers.Text,
                                                                                            inputMaxPlayers.Text,
                                                                                            Guid.Parse(inputSessionId.Text)));

            newGameRequest.Method           = "POST";
            newGameRequest.ContentLength    = 0;

            var response    = (HttpWebResponse)newGameRequest.GetResponse();
            var reader      = new StreamReader(response.GetResponseStream());

            var respose = JsonConvert.DeserializeObject<GFXRestResponse>(reader.ReadToEnd());

            if (respose.ResponseType == GFXResponseType.Normal)
            {
                // inform user we are done processing
                alertDialog.Attributes["class"] = "alert alert-success";
                alertDialog.Style["display"] = "normal";
                alertDialog.InnerHtml = "<p class='glyphicon glyphicon-warning-sign' style='margin-right:10px'></p>Game creation succesful! The page will now go back to the games page.";
            }
            else
            {
                // read response, if it fails, inform the user and don't continue and delete the uploaded file.
                alertDialog.Attributes["class"] = "alert alert-danger";
                alertDialog.Style["display"] = "normal";
                alertDialog.InnerHtml = "<p class='glyphicon glyphicon-warning-sign' style='margin-right:10px'></p>Game creation was not successful.";
            }
        }
    }
}
