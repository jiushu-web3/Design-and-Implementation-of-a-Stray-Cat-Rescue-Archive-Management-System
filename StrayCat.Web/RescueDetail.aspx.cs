using System;
using System.Text;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class RescueDetailPage : PageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder DetailHolder;
        protected Literal InfoLiteral;
        protected HyperLink EditLink;
        protected HyperLink BackLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            int catId = PageHelper.Positive(Request.QueryString["catId"]);
            CatDetail detail = catId == 0 ? null : new CatBll().GetDetail(catId);
            if (detail == null)
            {
                MessageLabel.Text = Messages.NotFound;
                DetailHolder.Visible = false;
                return;
            }
            RescueRecord rescue = detail.Rescue;
            StringBuilder html = new StringBuilder();
            html.Append("<p><span>接收时间</span>").Append(PageHelper.Html(rescue.RescueTimeText)).Append("</p>");
            html.Append("<p><span>接收地点</span>").Append(PageHelper.Html(rescue.Location)).Append("</p>");
            html.Append("<p><span>发现方式</span>").Append(PageHelper.Html(rescue.FoundWay)).Append("</p>");
            html.Append("<p><span>初步情况</span>").Append(PageHelper.Html(rescue.InitialHealth)).Append("</p>");
            html.Append("<p><span>备注</span>").Append(PageHelper.Html(rescue.Remark)).Append("</p>");
            InfoLiteral.Text = html.ToString();
            EditLink.NavigateUrl = "RescueEdit.aspx?catId=" + catId;
            EditLink.Visible = CatRules.CanEditArchive(detail.Cat.Status);
            BackLink.NavigateUrl = "CatDetail.aspx?id=" + catId;
        }
    }
}
