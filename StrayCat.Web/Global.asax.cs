using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

namespace StrayCat.Web
{
    public class Global : HttpApplication
    {
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception error = Server.GetLastError();
            if (error == null)
            {
                return;
            }
            Exception root = error.GetBaseException();
            if (root is ThreadAbortException)
            {
                return;
            }
            try
            {
                string dir = Server.MapPath("~/App_Data/logs");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, DateTime.Now.ToString("yyyyMMdd") + ".txt");
                string userName = "匿名";
                if (LoginSession.Current != null && !string.IsNullOrEmpty(LoginSession.Current.UserName))
                {
                    userName = LoginSession.Current.UserName;
                }
                string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + "\t" + Request.Url
                    + "\t" + userName
                    + "\t" + error
                    + Environment.NewLine;
                File.AppendAllText(path, line, Encoding.UTF8);
            }
            catch (Exception)
            {
            }
        }
    }
}
