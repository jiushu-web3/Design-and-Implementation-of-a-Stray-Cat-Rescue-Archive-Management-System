using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class ApplicationListPage : AdminPageBase
    {
        private const int PageSize = 10;

        protected DropDownList StatusList;
        protected Button SearchButton;
        protected Label MessageLabel;
        protected Label EmptyLabel;
        protected Repeater ApplicationRepeater;
        protected Literal PagerLiteral;
        protected Panel RejectPanel;
        protected HiddenField RejectId;
        protected TextBox ReasonBox;
        protected Button ConfirmRejectButton;
        protected Button CancelRejectButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindStatuses();
                PageHelper.Select(StatusList, Request.QueryString["status"]);
                Bind();
            }
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            Leave("ApplicationList.aspx?" + FilterQuery(StatusList.SelectedValue));
        }

        protected void ApplicationRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int applicationId = PageHelper.Positive(Convert.ToString(e.CommandArgument));
            if (applicationId == 0)
            {
                return;
            }
            if (e.CommandName == "approve")
            {
                OperationResult<int> result = new ApplicationBll().Approve(applicationId, CurrentUser.UserId);
                MessageLabel.CssClass = "form-message";
                if (!result.Success)
                {
                    MessageLabel.Text = result.Message;
                    Bind();
                    return;
                }
                MessageLabel.CssClass = "form-message is-success";
                MessageLabel.Text = "已通过申请，并为该猫建立领养记录";
                RejectPanel.Visible = false;
                Bind();
            }
            else if (e.CommandName == "reject")
            {
                RejectId.Value = applicationId.ToString();
                ReasonBox.Text = "";
                MessageLabel.CssClass = "form-message";
                MessageLabel.Text = "";
                RejectPanel.Visible = true;
            }
        }

        protected void ConfirmRejectButton_Click(object sender, EventArgs e)
        {
            int applicationId = PageHelper.Positive(RejectId.Value);
            if (applicationId == 0)
            {
                MessageLabel.Text = Messages.NotFound;
                RejectPanel.Visible = false;
                return;
            }
            OperationResult<int> result = new ApplicationBll().Reject(applicationId, ReasonBox.Text, CurrentUser.UserId);
            MessageLabel.CssClass = "form-message";
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                RejectPanel.Visible = true;
                Bind();
                return;
            }
            MessageLabel.CssClass = "form-message is-success";
            MessageLabel.Text = "已驳回该申请";
            RejectId.Value = "";
            ReasonBox.Text = "";
            RejectPanel.Visible = false;
            Bind();
        }

        protected void CancelRejectButton_Click(object sender, EventArgs e)
        {
            RejectId.Value = "";
            ReasonBox.Text = "";
            RejectPanel.Visible = false;
        }

        private void Bind()
        {
            string statusText = StatusList.SelectedValue;
            int pageIndex = PageHelper.Positive(Request.QueryString["page"]);
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            PagedResult<AdoptionApplication> page = new ApplicationBll().Search(ReadFilter(statusText), pageIndex, PageSize);
            EmptyLabel.Visible = page.Items.Count == 0;
            ApplicationRepeater.Visible = page.Items.Count > 0;
            ApplicationRepeater.DataSource = page.Items;
            ApplicationRepeater.DataBind();
            PagerLiteral.Text = PageHelper.Pager(
                "ApplicationList.aspx?" + FilterQuery(statusText),
                pageIndex,
                page.TotalCount,
                PageSize);
        }

        private void BindStatuses()
        {
            StatusList.Items.Clear();
            StatusList.Items.Add(new ListItem("全部", ""));
            StatusList.Items.Add(new ListItem(ApplicationStatuses.Name(ApplicationStatuses.Pending), ApplicationStatuses.Pending.ToString()));
            StatusList.Items.Add(new ListItem(ApplicationStatuses.Name(ApplicationStatuses.Approved), ApplicationStatuses.Approved.ToString()));
            StatusList.Items.Add(new ListItem(ApplicationStatuses.Name(ApplicationStatuses.Rejected), ApplicationStatuses.Rejected.ToString()));
        }

        private static string FilterQuery(string status)
        {
            return "status=" + PageHelper.Encode(status);
        }

        private static byte? ReadFilter(string text)
        {
            byte value;
            if (string.IsNullOrEmpty(text) || !byte.TryParse(text, out value))
            {
                return null;
            }
            return value;
        }

        protected string StatusName(object value)
        {
            return ApplicationStatuses.Name(Convert.ToByte(value));
        }

        protected string StatusTag(object value)
        {
            byte status = Convert.ToByte(value);
            if (status == ApplicationStatuses.Approved)
            {
                return "tag-adopted";
            }
            if (status == ApplicationStatuses.Rejected)
            {
                return "tag-ended";
            }
            return "tag-wait";
        }

        protected string TimeText(object value)
        {
            return Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm");
        }

        protected string ReasonText(object value)
        {
            string reason = Convert.ToString(value);
            return string.IsNullOrWhiteSpace(reason) ? "—" : reason;
        }

        protected string CatText(object catNo, object tempName)
        {
            return Convert.ToString(catNo) + " · " + Convert.ToString(tempName);
        }

        protected bool IsPending(object value)
        {
            return Convert.ToByte(value) == ApplicationStatuses.Pending;
        }
    }
}
