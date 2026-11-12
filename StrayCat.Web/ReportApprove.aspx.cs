using System;
using System.Text;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class ReportApprovePage : AdminPageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder DetailHolder;
        protected Image PhotoImage;
        protected Literal InfoLiteral;
        protected TextBox NameBox;
        protected DropDownList GenderList;
        protected TextBox AgeBox;
        protected TextBox ColorBox;
        protected DropDownList NeuteredList;
        protected FileUpload PhotoFile;
        protected TextBox DateBox;
        protected TextBox HealthBox;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PageHelper.BindGenders(GenderList, false);
                GenderList.Items.Insert(0, new ListItem("请选择", ""));
                PageHelper.BindNeutered(NeuteredList);
            }
            LoadReport();
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            int reportId = PageHelper.Positive(Request.QueryString["id"]);
            if (reportId == 0)
            {
                MessageLabel.Text = Messages.NotFound;
                DetailHolder.Visible = false;
                return;
            }
            DateTime rescueTime;
            if (!PageHelper.ReadDate(DateBox.Text, out rescueTime))
            {
                MessageLabel.Text = Messages.BadDate;
                return;
            }
            string photoError;
            string photo = PageHelper.SavePhoto(PhotoFile, Server, out photoError);
            if (photoError != null)
            {
                MessageLabel.Text = photoError;
                return;
            }
            CatArchive cat = new CatArchive
            {
                TempName = NameBox.Text,
                Gender = PageHelper.SelectedByte(GenderList) ?? 0,
                EstimatedAge = AgeBox.Text,
                Color = ColorBox.Text,
                IsNeutered = PageHelper.SelectedByte(NeuteredList) ?? 0,
                PhotoPath = photo
            };
            OperationResult<int> result = new ReportBll().Approve(
                reportId, cat, rescueTime, HealthBox.Text, CurrentUser.UserId);
            if (!result.Success)
            {
                PageHelper.DeletePhoto(photo, Server);
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("~/ReportList.aspx?msg=approved");
        }

        private void LoadReport()
        {
            int reportId = PageHelper.Positive(Request.QueryString["id"]);
            StrayReport report = reportId == 0 ? null : new ReportBll().Get(reportId);
            if (report == null)
            {
                MessageLabel.Text = Messages.NotFound;
                DetailHolder.Visible = false;
                return;
            }
            if (report.Status != ReportStatuses.Pending)
            {
                MessageLabel.Text = Messages.StatusNotAllowed;
                DetailHolder.Visible = false;
                return;
            }
            PageHelper.ShowPhoto(PhotoImage, report.PhotoPath, Server);
            StringBuilder html = new StringBuilder();
            Line(html, "上报编号", report.ReportId.ToString());
            Line(html, "上报人", ReporterText(report));
            Line(html, "联系电话", report.ContactPhone);
            Line(html, "发现地点", report.FoundPlace);
            Line(html, "发现日期", report.FoundDate.ToString("yyyy-MM-dd"));
            Line(html, "情况描述", report.Description);
            Line(html, "上报时间", report.CreateTime.ToString("yyyy-MM-dd HH:mm"));
            Line(html, "状态", ReportStatuses.Name(report.Status));
            InfoLiteral.Text = html.ToString();
            if (!IsPostBack)
            {
                DateBox.Text = report.FoundDate.ToString("yyyy-MM-dd");
            }
        }

        private static string ReporterText(StrayReport report)
        {
            return string.IsNullOrWhiteSpace(report.RealName)
                ? report.UserName
                : report.RealName + "（" + report.UserName + "）";
        }

        private static void Line(StringBuilder html, string name, string value)
        {
            html.Append("<p><span>").Append(PageHelper.Html(name)).Append("</span>");
            html.Append(PageHelper.Html(value)).Append("</p>");
        }
    }
}
