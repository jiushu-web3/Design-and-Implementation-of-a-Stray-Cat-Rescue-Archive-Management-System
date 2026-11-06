using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class UserAddPage : AdminPageBase
    {
        protected Label MessageLabel;
        protected TextBox UserNameBox;
        protected TextBox PasswordBox;
        protected TextBox RealNameBox;
        protected TextBox PhoneBox;
        protected DropDownList RoleList;
        protected CheckBox EnabledBox;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PageHelper.BindRoles(RoleList);
                PageHelper.Select(RoleList, RoleCodes.Staff.ToString());
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            byte? role = PageHelper.SelectedByte(RoleList);
            OperationResult<int> result = new UserBll().Create(
                UserNameBox.Text,
                PasswordBox.Text,
                RealNameBox.Text,
                PhoneBox.Text,
                role ?? RoleCodes.Staff,
                EnabledBox.Checked);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("UserList.aspx");
        }
    }
}
