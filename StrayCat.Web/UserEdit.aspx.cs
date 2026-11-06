using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class UserEditPage : AdminPageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder FormHolder;
        protected TextBox UserNameBox;
        protected TextBox RealNameBox;
        protected TextBox PhoneBox;
        protected DropDownList RoleList;
        protected CheckBox EnabledBox;
        protected TextBox PasswordBox;
        protected TextBox ConfirmBox;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PageHelper.BindRoles(RoleList);
                LoadUser();
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            int userId = PageHelper.Positive(Request.QueryString["id"]);
            if (userId == 0)
            {
                ShowMissing();
                return;
            }
            byte? role = PageHelper.SelectedByte(RoleList);
            OperationResult<int> result = new UserBll().Update(
                userId,
                CurrentUser.UserId,
                RealNameBox.Text,
                PhoneBox.Text,
                role ?? RoleCodes.Staff,
                EnabledBox.Checked,
                PasswordBox.Text);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("UserList.aspx");
        }

        private void LoadUser()
        {
            int userId = PageHelper.Positive(Request.QueryString["id"]);
            SysUser user = userId == 0 ? null : new UserBll().Get(userId);
            if (user == null)
            {
                ShowMissing();
                return;
            }
            UserNameBox.Text = user.UserName;
            RealNameBox.Text = user.RealName;
            PhoneBox.Text = user.Phone;
            EnabledBox.Checked = user.IsEnabled;
            PageHelper.Select(RoleList, user.Role.ToString());
        }

        private void ShowMissing()
        {
            MessageLabel.Text = Messages.NotFound;
            FormHolder.Visible = false;
        }
    }
}
