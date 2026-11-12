//登录页面
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    //继承Page类
    public partial class LoginPage : Page
    {
        protected Label MessageLabel;
        protected RadioButton UserChoice;
        protected RadioButton AdminChoice;
        protected TextBox UserNameBox;
        protected TextBox PasswordBox;
        protected TextBox CaptchaBox;
        protected Image CaptchaImage;
        protected Button LoginButton;

        //页面加载事件
        protected void Page_Load(object sender, EventArgs e)
        {
            if (LoginSession.Current != null)
            {
                //
                Response.Redirect("~/Default.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            CaptchaImage.Attributes["data-captcha-scope"] = "login";
            CaptchaImage.ImageUrl = CaptchaSession.ImageUrl("login");
            if (!IsPostBack && Request.QueryString["msg"] == "expired")
            {
                MessageLabel.Text = Messages.Expired;
            }
            if (!IsPostBack && Request.QueryString["msg"] == "registered")
            {
                MessageLabel.CssClass = "form-message is-success";
                MessageLabel.Text = Messages.Registered;
            }
        }

        protected void CaptchaCheck(object source, ServerValidateEventArgs args)
        {
            CaptchaSession.Validate("login", source, args);
        }

        protected void LoginButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            if (!UserChoice.Checked && !AdminChoice.Checked)
            {
                //如果用户没有选择登录类型，提示用户选择
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
