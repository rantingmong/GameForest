using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class upload : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void ButtonSubmit_Click(object sender, EventArgs e)
    {
        bool error = false;

        if (FileUpload.HasFile)
        {
            if (Path.GetExtension(FileUpload.FileName) == "zip")
            {
                FileUpload.SaveAs(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", FileUpload.FileName));
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
            // send REST request to server
            HttpWebRequest newGameRequest = (HttpWebRequest)WebRequest.Create(string.Format("http://localhost:1193/game?name={0}&description={1}&minplayers={2}&maxplayers={3}&usersessionid={4}",
                                                                                            inputGameName,
                                                                                            inputDescription,
                                                                                            inputMinPlayers,
                                                                                            inputMaxPlayers,
                                                                                            Guid.Parse(loginSession.InnerHtml));

            newGameRequest.ContentType  = "application/json";
            newGameRequest.Method       = "POST";

            newGameRequest.BeginGetResponse(new AsyncCallback((iar) =>
                {
                    var response    = (HttpWebResponse)newGameRequest.GetResponse();
                    var reader      = new StreamReader(response.GetResponseStream());

                    string stringResponse = reader.ReadToEnd();

                }), null);
        }
    }
}
