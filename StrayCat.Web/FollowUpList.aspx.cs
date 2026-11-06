using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class FollowUpListPage : PageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder ListHolder;
        protected HyperLink AddLink;
        protected Label EmptyLabel;
        protected Repeater ListRepeater;
        protected HyperLink BackLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            int adoptionId = PageHelper.Positive(Request.QueryString["adoptionId"]);
            AdoptionRecord adoption = adoptionId == 0 ? null : new AdoptionBll().Get(adoptionId);
            if (adoption == null)
            {
                MessageLabel.Text = Messages.NotFound;
                ListHolder.Visible = false;
                return;
            }
            CatDetail detail = new CatBll().GetDetail(adoption.CatId);
            AddLink.NavigateUrl = "FollowUpAdd.aspx?adoptionId=" + adoptionId;
            AddLink.Visible = detail != null && CatRules.CanEditFollowUp(detail.Cat.Status, adoption.Status);
            BackLink.NavigateUrl = "AdoptionDetail.aspx?id=" + adoptionId;
            List<FollowUpRecord> rows = new FollowUpBll().ListByAdoption(adoptionId);
            EmptyLabel.Visible = rows.Count == 0;
            ListRepeater.Visible = rows.Count > 0;
            ListRepeater.DataSource = rows;
            ListRepeater.DataBind();
        }
    }
}
