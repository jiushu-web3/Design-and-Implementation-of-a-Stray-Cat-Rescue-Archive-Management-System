using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.UI;
using StrayCat.BLL;

namespace StrayCat.Web
{
    public partial class CaptchaPage : Page
    {
        private string _scope;
        private string _code;

        protected void Page_Load(object sender, EventArgs e)
        {
            _scope = Request.QueryString["scope"];
            if (_scope != "login" && _scope != "register")
            {
                _scope = "login";
            }
            _code = CaptchaCode.Create();
            Session["CaptchaCode_" + _scope] = _code;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            Response.ClearContent();
            Response.ContentType = "image/png";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
            using (Bitmap image = CaptchaImage.Draw(_code))
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, ImageFormat.Png);
                Response.BinaryWrite(stream.ToArray());
            }
        }
    }
}
