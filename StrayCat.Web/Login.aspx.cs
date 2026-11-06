using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class LoginPage : Page
    {
        protected Label MessageLabel;
        protected RadioButton UserChoice;
        protected RadioButton AdminChoice;
        protected TextBox UserNameBox;
        protected TextBox PasswordBox;
        protected Button LoginButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (LoginSession.Current != null)
            {
                Response.Redirect("~/Default.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            if (!IsPostBack && Request.QueryString["msg"] == "expired")
            {
                MessageLabel.Text = Messages.Expired;
            }
        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            if (!UserChoice.Checked && !AdminChoice.Checked)
            {
                MessageLabel.Text = Messages.ChooseLogin;
                return;
            }
            OperationResult<CurrentUser> result = new UserBll().Login(UserNameBox.Text, PasswordBox.Text);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            byte expected = AdminChoice.Checked ? RoleCodes.Admin : RoleCodes.Staff;
            if (result.Data.Role != expected)
            {
                MessageLabel.Text = expected == RoleCodes.Admin ? Messages.UseUserLogin : Messages.UseAdminLogin;
                return;
            }
            LoginSession.Set(result.Data);
            Response.Redirect("~/Default.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
