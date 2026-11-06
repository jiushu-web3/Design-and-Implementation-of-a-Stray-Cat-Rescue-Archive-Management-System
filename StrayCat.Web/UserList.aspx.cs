using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class UserListPage : AdminPageBase
    {
        protected TextBox KeywordBox;
        protected Button SearchButton;
        protected Label MessageLabel;
        protected Label EmptyLabel;
        protected Repeater ListRepeater;
        protected Literal PagerLiteral;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                KeywordBox.Text = Request.QueryString["kw"] ?? "";
                Bind();
            }
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            Leave("UserList.aspx?kw=" + PageHelper.Encode(KeywordBox.Text.Trim()) + "&page=1");
        }

        protected void ListRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int userId = PageHelper.Positive(Convert.ToString(e.CommandArgument));
            bool enabled = e.CommandName == "on";
            OperationResult<int> result = new UserBll().SetEnabled(userId, CurrentUser.UserId, enabled);
            MessageLabel.Text = result.Success ? "" : result.Message;
            Bind();
        }

        private void Bind()
        {
            string keyword = Request.QueryString["kw"] ?? "";
            int pageIndex = PageHelper.Positive(Request.QueryString["page"]);
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            PagedResult<SysUser> page = new UserBll().Search(keyword, pageIndex);
            EmptyLabel.Visible = page.Items.Count == 0;
            ListRepeater.Visible = page.Items.Count > 0;
            ListRepeater.DataSource = page.Items;
            ListRepeater.DataBind();
            PagerLiteral.Text = PageHelper.Pager("UserList.aspx?kw=" + PageHelper.Encode(keyword), page.PageIndex, page.TotalCount, page.PageSize);
        }
    }
}
