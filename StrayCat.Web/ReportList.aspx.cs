using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class ReportListPage : AdminPageBase
    {
        private const int PageSize = 10;

        protected DropDownList StatusList;
        protected TextBox KeywordBox;
        protected Button SearchButton;
        protected Label MessageLabel;
        protected Label EmptyLabel;
        protected Repeater ReportRepeater;
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
                KeywordBox.Text = Request.QueryString["kw"] ?? "";
                PageHelper.Select(StatusList, Request.QueryString["status"]);
                ShowMessage();
                Bind();
            }
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            Leave("ReportList.aspx?" + FilterQuery(KeywordBox.Text, StatusList.SelectedValue));
        }

        protected void ReportRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int reportId = PageHelper.Positive(Convert.ToString(e.CommandArgument));
            if (e.CommandName != "reject" || reportId == 0)
            {
                return;
            }
            RejectId.Value = reportId.ToString();
            ReasonBox.Text = "";
            MessageLabel.CssClass = "form-message";
            MessageLabel.Text = "";
            RejectPanel.Visible = true;
        }

        protected void ConfirmRejectButton_Click(object sender, EventArgs e)
        {
            int reportId = PageHelper.Positive(RejectId.Value);
            if (reportId == 0)
            {
                MessageLabel.Text = Messages.NotFound;
                RejectPanel.Visible = false;
                return;
            }
            OperationResult<int> result = new ReportBll().Reject(reportId, ReasonBox.Text, CurrentUser.UserId);
            MessageLabel.CssClass = "form-message";
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            MessageLabel.CssClass = "form-message is-success";
            MessageLabel.Text = "已驳回该上报";
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

        private void ShowMessage()
        {
            if (Request.QueryString["msg"] == "approved")
            {
                MessageLabel.CssClass = "form-message is-success";
                MessageLabel.Text = "上报已采纳并建立档案";
            }
        }

        private void Bind()
        {
            string keyword = Request.QueryString["kw"] ?? "";
            string statusText = Request.QueryString["status"] ?? "";
            int pageIndex = PageHelper.Positive(Request.QueryString["page"]);
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            PagedResult<StrayReport> page = new ReportBll().Search(ReadFilter(statusText), keyword, pageIndex, PageSize);
            EmptyLabel.Visible = page.Items.Count == 0;
            ReportRepeater.Visible = page.Items.Count > 0;
            ReportRepeater.DataSource = page.Items;
            ReportRepeater.DataBind();
            PagerLiteral.Text = PageHelper.Pager(
                "ReportList.aspx?" + FilterQuery(keyword, statusText),
                pageIndex,
                page.TotalCount,
                PageSize);
        }

        private void BindStatuses()
        {
            StatusList.Items.Clear();
            StatusList.Items.Add(new ListItem("全部", ""));
            StatusList.Items.Add(new ListItem(ReportStatuses.Name(ReportStatuses.Pending), ReportStatuses.Pending.ToString()));
            StatusList.Items.Add(new ListItem(ReportStatuses.Name(ReportStatuses.Approved), ReportStatuses.Approved.ToString()));
            StatusList.Items.Add(new ListItem(ReportStatuses.Name(ReportStatuses.Rejected), ReportStatuses.Rejected.ToString()));
        }

        private static string FilterQuery(string keyword, string status)
        {
            string query = "kw=" + PageHelper.Encode((keyword ?? "").Trim());
            if (!string.IsNullOrEmpty(status))
            {
                query += "&status=" + PageHelper.Encode(status);
            }
            return query;
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

        protected string ReportStatusName(object value)
        {
            return ReportStatuses.Name(Convert.ToByte(value));
        }

        protected string StatusTag(object value)
        {
            byte status = Convert.ToByte(value);
            if (status == ReportStatuses.Approved)
            {
                return "tag-adopted";
            }
            if (status == ReportStatuses.Rejected)
            {
                return "tag-ended";
            }
            return "tag-wait";
        }

        protected string DateText(object value)
        {
            return Convert.ToDateTime(value).ToString("yyyy-MM-dd");
        }

        protected string ReporterText(object realName, object userName)
        {
            string real = Convert.ToString(realName);
            string name = Convert.ToString(userName);
            return string.IsNullOrWhiteSpace(real) ? name : real;
        }

        protected bool IsPending(object value)
        {
            return Convert.ToByte(value) == ReportStatuses.Pending;
        }
    }
}
