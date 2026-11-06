using System;
using System.Text;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class AdoptionDetailPage : PageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder DetailHolder;
        protected Literal InfoLiteral;
        protected HyperLink EditLink;
        protected HyperLink FollowLink;
        protected HyperLink AddFollowLink;
        protected Button DeleteButton;
        protected HyperLink BackLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadRecord();
            }
        }

        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            int adoptionId = PageHelper.Positive(Request.QueryString["id"]);
            AdoptionRecord record = adoptionId == 0 ? null : new AdoptionBll().Get(adoptionId);
            if (record == null)
            {
                ShowMissing();
                return;
            }
            OperationResult<int> result = new AdoptionBll().Delete(adoptionId);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("AdoptionList.aspx?catId=" + record.CatId);
        }

        private void LoadRecord()
        {
            int adoptionId = PageHelper.Positive(Request.QueryString["id"]);
            AdoptionRecord record = adoptionId == 0 ? null : new AdoptionBll().Get(adoptionId);
            if (record == null)
            {
                ShowMissing();
                return;
            }
            CatDetail detail = new CatBll().GetDetail(record.CatId);
            StringBuilder html = new StringBuilder();
            html.Append("<p><span>领养人</span>").Append(PageHelper.Html(record.AdopterName)).Append("</p>");
            html.Append("<p><span>手机号</span>").Append(PageHelper.Html(record.AdopterPhone)).Append("</p>");
            html.Append("<p><span>住址</span>").Append(PageHelper.Html(record.AdopterAddress)).Append("</p>");
            html.Append("<p><span>领养日期</span>").Append(PageHelper.Html(record.AdoptDateText)).Append("</p>");
            html.Append("<p><span>状态</span>").Append(PageHelper.Html(record.StatusName)).Append("</p>");
            html.Append("<p><span>退养日期</span>").Append(PageHelper.Html(record.ReturnDateText)).Append("</p>");
            html.Append("<p><span>退养原因</span>").Append(PageHelper.Html(record.ReturnReason)).Append("</p>");
            html.Append("<p><span>回访条数</span>").Append(record.FollowUpCount).Append("</p>");
            html.Append("<p><span>备注</span>").Append(PageHelper.Html(record.Remark)).Append("</p>");
            InfoLiteral.Text = html.ToString();
            EditLink.NavigateUrl = "AdoptionEdit.aspx?id=" + adoptionId;
            FollowLink.NavigateUrl = "FollowUpList.aspx?adoptionId=" + adoptionId;
            AddFollowLink.NavigateUrl = "FollowUpAdd.aspx?adoptionId=" + adoptionId;
            BackLink.NavigateUrl = "CatDetail.aspx?id=" + record.CatId;
            bool active = detail != null && record.Status == AdoptionStatuses.Active && detail.Cat.Status == CatStatuses.Adopted;
            EditLink.Visible = active;
            AddFollowLink.Visible = active && CatRules.CanEditFollowUp(detail.Cat.Status, record.Status);
            DeleteButton.Visible = detail == null || CatRules.CanDeleteChildren(detail.Cat.Status);
        }

        private void ShowMissing()
        {
            MessageLabel.Text = Messages.NotFound;
            DetailHolder.Visible = false;
        }
    }
}
