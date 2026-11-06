using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class ChangePasswordPage : PageBase
    {
        protected Label MessageLabel;
        protected TextBox OldBox;
        protected TextBox NewBox;
        protected TextBox ConfirmBox;
        protected Button SaveButton;

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            if (NewBox.Text != ConfirmBox.Text)
            {
                MessageLabel.Text = Messages.PasswordMismatch;
                return;
            }
            OperationResult<int> result = new UserBll().ChangePassword(CurrentUser.UserId, OldBox.Text, NewBox.Text);
            MessageLabel.Text = result.Success ? "密码已修改" : result.Message;
        }
    }
}
