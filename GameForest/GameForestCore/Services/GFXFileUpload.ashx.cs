using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace GameForestCore.Services
{
    public class GFXFileUpload : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Files.Count > 0)
            {
                string path = context.Server.MapPath("~/Temp");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var file = context.Request.Files[0];

                string fileName;

                if (HttpContext.Current.Request.Browser.Browser.ToUpper() == "IE")
                {
                    string[] files = file.FileName.Split(new char[] { '\\' });
                    fileName = files[files.Length - 1];
                }
                else
                {
                    fileName = file.FileName;
                }

                string strFileName = fileName;
                fileName = Path.Combine(path, fileName);
                file.SaveAs(fileName);

                string msg = "{";
                msg += string.Format("error:'{0}',\n", string.Empty);
                msg += string.Format("msg:'{0}'\n", strFileName);
                msg += "}";
                context.Response.Write(msg);
            }
        }
    }
}
