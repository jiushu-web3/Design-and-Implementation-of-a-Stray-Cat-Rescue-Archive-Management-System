using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class AdoptablePage : Page
    {
        private const int PageSize = 12;

        protected Repeater ListRepeater;
        protected Literal PagerLiteral;
        protected Label EmptyLabel;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Bind();
            }
        }

        private void Bind()
        {
            int pageIndex = PageHelper.Positive(Request.QueryString["page"]);
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            PagedResult<PublicCat> page = new CatBll().ListAdoptable(pageIndex, PageSize);
            EmptyLabel.Visible = page.Items.Count == 0;
            ListRepeater.Visible = page.Items.Count > 0;
            ListRepeater.DataSource = page.Items;
            ListRepeater.DataBind();
            PagerLiteral.Text = PageHelper.Pager("Adoptable.aspx", pageIndex, page.TotalCount, PageSize);
        }

        protected string PhotoMarkup(object value)
        {
            string path = Convert.ToString(value).Replace('\\', '/').Trim();
            if (path.Length == 0)
            {
                return "";
            }
            if (path.StartsWith("~/", StringComparison.Ordinal))
            {
                path = path.Substring(2);
            }
            return "<img src=\"" + PageHelper.Html(ResolveUrl("~/" + path)) + "\" alt=\"\" />";
        }
    }
}
