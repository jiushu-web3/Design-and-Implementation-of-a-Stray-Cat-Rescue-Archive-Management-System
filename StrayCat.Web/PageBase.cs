using System;
using System.Web.UI;
using StrayCat.Model;

namespace StrayCat.Web
{
    public class PageBase : Page
    {
        protected bool Redirecting { get; private set; }

        protected CurrentUser CurrentUser
        {
            get { return LoginSession.Current; }
        }

        protected override void OnInit(EventArgs e)
        {
            if (LoginSession.Current == null)
            {
                Leave("~/Login.aspx?msg=expired", Request.RawUrl);
                return;
            }
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (Redirecting)
            {
                return;
            }
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (Redirecting)
            {
                return;
            }
            base.OnPreRender(e);
        }

        protected void Leave(string url, string returnUrl = null)
        {
            Redirecting = true;
            if (!string.IsNullOrEmpty(returnUrl))
            {
                url += (url.Contains("?") ? "&" : "?") + "returnUrl=" + System.Web.HttpUtility.UrlEncode(returnUrl);
            }
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
