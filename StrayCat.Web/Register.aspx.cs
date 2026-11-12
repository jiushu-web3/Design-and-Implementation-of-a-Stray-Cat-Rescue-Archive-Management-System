using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class RegisterPage : Page
    {
        protected Label MessageLabel;
        protected TextBox UserNameBox;
        protected TextBox PasswordBox;
        protected TextBox ConfirmBox;
        protected TextBox RealNameBox;
        protected TextBox PhoneBox;
        protected TextBox CaptchaBox;
        protected Image CaptchaImage;
        protected Button RegisterButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (LoginSession.Current != null)
            {
                Response.Redirect("~/Default.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            CaptchaImage.Attributes["data-captcha-scope"] = "register";
            CaptchaImage.ImageUrl = CaptchaSession.ImageUrl("register");
        }

        protected void CaptchaCheck(object source, ServerValidateEventArgs args)
        {
            CaptchaSession.Validate("register", source, args);
        }

        protected void RegisterButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            OperationResult<int> result = new UserBll().Register(
                UserNameBox.Text,
                PasswordBox.Text,
                ConfirmBox.Text,
                RealNameBox.Text,
                PhoneBox.Text);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Response.Redirect("~/Login.aspx?msg=registered", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
