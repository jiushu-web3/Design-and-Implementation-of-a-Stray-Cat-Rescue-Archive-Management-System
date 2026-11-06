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
                Leave("~/Login.aspx?msg=expired");
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

        protected void Leave(string url)
        {
            Redirecting = true;
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
