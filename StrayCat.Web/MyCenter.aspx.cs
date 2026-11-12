using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class MyCenterPage : PageBase
    {
        private const int PageSize = 10;

        protected Label MessageLabel;
        protected Label EmptyReportLabel;
        protected Repeater ReportRepeater;
        protected Literal ReportPagerLiteral;
        protected Label EmptyApplicationLabel;
        protected Repeater ApplicationRepeater;
        protected Literal ApplicationPagerLiteral;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ShowMessage();
                BindReports();
                BindApplications();
            }
        }

        private void ShowMessage()
        {
            string msg = Request.QueryString["msg"];
            if (msg == "report")
            {
                MessageLabel.Text = Messages.ReportSubmitted;
                MessageLabel.Visible = true;
            }
            else if (msg == "application")
            {
                MessageLabel.Text = Messages.ApplicationSubmitted;
                MessageLabel.Visible = true;
            }
        }

        private void BindReports()
        {
            int pageIndex = PageHelper.Positive(Request.QueryString["rp"]);
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            PagedResult<StrayReport> page = new ReportBll().ListByUser(CurrentUser.UserId, pageIndex, PageSize);
            EmptyReportLabel.Visible = page.Items.Count == 0;
            ReportRepeater.Visible = page.Items.Count > 0;
            ReportRepeater.DataSource = page.Items;
            ReportRepeater.DataBind();
            ReportPagerLiteral.Text = PageHelper.Pager("MyCenter.aspx", page.PageIndex, page.TotalCount, PageSize, "rp");
        }

        private void BindApplications()
        {
            int pageIndex = PageHelper.Positive(Request.QueryString["ap"]);
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            PagedResult<AdoptionApplication> page = new ApplicationBll().ListByUser(CurrentUser.UserId, pageIndex, PageSize);
            EmptyApplicationLabel.Visible = page.Items.Count == 0;
            ApplicationRepeater.Visible = page.Items.Count > 0;
            ApplicationRepeater.DataSource = page.Items;
            ApplicationRepeater.DataBind();
            ApplicationPagerLiteral.Text = PageHelper.Pager("MyCenter.aspx", page.PageIndex, page.TotalCount, PageSize, "ap");
        }

        protected string ReportStatusName(object value)
        {
            return ReportStatuses.Name(Convert.ToByte(value));
        }

        protected string ApplicationStatusName(object value)
        {
            return ApplicationStatuses.Name(Convert.ToByte(value));
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

        protected string ApplicationStatusTag(object value)
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

        protected string DateText(object value)
        {
            return Convert.ToDateTime(value).ToString("yyyy-MM-dd");
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
    }
}
