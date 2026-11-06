using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StrayCat.Web
{
    public partial class SiteMaster : MasterPage
    {
        protected Literal UserLiteral;
        protected LinkButton LogoutButton;
        protected PlaceHolder AdminMenu;

        protected void Page_Load(object sender, EventArgs e)
        {
            LogoutButton.Attributes["data-skip-validate"] = "true";
            if (LoginSession.Current != null)
            {
                UserLiteral.Text = LoginSession.Current.RealName;
                AdminMenu.Visible = LoginSession.Current.IsAdmin;
            }
        }

        protected void LogoutButton_Click(object sender, EventArgs e)
        {
            LoginSession.Clear();
            Response.Redirect("~/Login.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
