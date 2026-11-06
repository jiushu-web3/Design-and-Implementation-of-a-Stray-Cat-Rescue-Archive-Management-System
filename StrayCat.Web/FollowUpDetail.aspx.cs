using System;
using System.Text;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class FollowUpDetailPage : PageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder DetailHolder;
        protected Literal InfoLiteral;
        protected HyperLink EditLink;
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
            int followUpId = PageHelper.Positive(Request.QueryString["id"]);
            FollowUpRecord record = followUpId == 0 ? null : new FollowUpBll().Get(followUpId);
            if (record == null)
            {
                ShowMissing();
                return;
            }
            OperationResult<int> result = new FollowUpBll().Delete(followUpId);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("FollowUpList.aspx?adoptionId=" + record.AdoptionId);
        }

        private void LoadRecord()
        {
            int followUpId = PageHelper.Positive(Request.QueryString["id"]);
            FollowUpRecord record = followUpId == 0 ? null : new FollowUpBll().Get(followUpId);
            if (record == null)
            {
                ShowMissing();
                return;
            }
            AdoptionRecord adoption = new AdoptionBll().Get(record.AdoptionId);
            CatDetail detail = adoption == null ? null : new CatBll().GetDetail(adoption.CatId);
            StringBuilder html = new StringBuilder();
            html.Append("<p><span>日期</span>").Append(PageHelper.Html(record.FollowDateText)).Append("</p>");
            html.Append("<p><span>方式</span>").Append(PageHelper.Html(record.FollowWay)).Append("</p>");
            html.Append("<p><span>猫咪状况</span>").Append(PageHelper.Html(record.CatCondition)).Append("</p>");
            html.Append("<p><span>备注</span>").Append(PageHelper.Html(record.Remark)).Append("</p>");
            InfoLiteral.Text = html.ToString();
            EditLink.NavigateUrl = "FollowUpEdit.aspx?id=" + followUpId;
            BackLink.NavigateUrl = "FollowUpList.aspx?adoptionId=" + record.AdoptionId;
            bool editable = detail != null && adoption != null && CatRules.CanEditFollowUp(detail.Cat.Status, adoption.Status);
            EditLink.Visible = editable;
            DeleteButton.Visible = detail == null || CatRules.CanDeleteChildren(detail.Cat.Status);
        }

        private void ShowMissing()
        {
            MessageLabel.Text = Messages.NotFound;
            DetailHolder.Visible = false;
        }
    }
}
