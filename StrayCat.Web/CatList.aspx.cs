using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class CatListPage : PageBase
    {
        protected TextBox KeywordBox;
        protected DropDownList StatusList;
        protected DropDownList GenderList;
        protected TextBox FromBox;
        protected TextBox ToBox;
        protected Button SearchButton;
        protected Label MessageLabel;
        protected Label EmptyLabel;
        protected Repeater ListRepeater;
        protected Literal PagerLiteral;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PageHelper.BindStatuses(StatusList, true);
                PageHelper.BindGenders(GenderList, true);
                KeywordBox.Text = Request.QueryString["kw"] ?? "";
                PageHelper.Select(StatusList, Request.QueryString["status"]);
                PageHelper.Select(GenderList, Request.QueryString["gender"]);
                FromBox.Text = Request.QueryString["from"] ?? "";
                ToBox.Text = Request.QueryString["to"] ?? "";
                Bind();
            }
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            DateTime from;
            DateTime to;
            bool hasFrom = PageHelper.ReadDate(FromBox.Text, out from);
            bool hasTo = PageHelper.ReadDate(ToBox.Text, out to);
            if (hasFrom && hasTo && from.Date > to.Date)
            {
                MessageLabel.Text = Messages.DateOrder;
                return;
            }
            Leave("CatList.aspx?" + FilterQuery(KeywordBox.Text, StatusList.SelectedValue, GenderList.SelectedValue, FromBox.Text, ToBox.Text));
        }

        private void Bind()
        {
            string keyword = Request.QueryString["kw"] ?? "";
            string statusText = Request.QueryString["status"] ?? "";
            string genderText = Request.QueryString["gender"] ?? "";
            string fromText = Request.QueryString["from"] ?? "";
            string toText = Request.QueryString["to"] ?? "";
            DateTime from;
            DateTime to;
            DateTime? rescueFrom = PageHelper.ReadDate(fromText, out from) ? from : (DateTime?)null;
            DateTime? rescueTo = PageHelper.ReadDate(toText, out to) ? to : (DateTime?)null;
            if (rescueFrom.HasValue && rescueTo.HasValue && rescueFrom.Value.Date > rescueTo.Value.Date)
            {
                MessageLabel.Text = Messages.DateOrder;
                EmptyLabel.Visible = true;
                ListRepeater.Visible = false;
                return;
            }
            int pageIndex = PageHelper.Positive(Request.QueryString["page"]);
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            PagedResult<CatListItem> page = new CatBll().Search(new CatSearch
            {
                Keyword = keyword,
                Status = ReadFilter(statusText),
                Gender = ReadFilter(genderText),
                RescueFrom = rescueFrom,
                RescueTo = rescueTo,
                PageIndex = pageIndex
            });
            EmptyLabel.Visible = page.Items.Count == 0;
            ListRepeater.Visible = page.Items.Count > 0;
            ListRepeater.DataSource = page.Items;
            ListRepeater.DataBind();
            PagerLiteral.Text = PageHelper.Pager("CatList.aspx?" + FilterQuery(keyword, statusText, genderText, fromText, toText), page.PageIndex, page.TotalCount, page.PageSize);
        }

        private static string FilterQuery(string keyword, string status, string gender, string from, string to)
        {
            string query = "kw=" + PageHelper.Encode(keyword ?? "");
            if (!string.IsNullOrEmpty(status))
            {
                query += "&status=" + PageHelper.Encode(status);
            }
            if (!string.IsNullOrEmpty(gender))
            {
                query += "&gender=" + PageHelper.Encode(gender);
            }
            if (!string.IsNullOrEmpty(from))
            {
                query += "&from=" + PageHelper.Encode(from);
            }
            if (!string.IsNullOrEmpty(to))
            {
                query += "&to=" + PageHelper.Encode(to);
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
    }
}
