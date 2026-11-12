using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class FollowUpListPage : PageBase
    {
        protected Literal TitleLiteral;
        protected Label MessageLabel;
        protected Label HintLabel;
        protected PlaceHolder ListHolder;
        protected HyperLink AddLink;
        protected Label EmptyLabel;
        protected Repeater ListRepeater;
        protected HyperLink BackLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["adoptionId"] == null)
            {
                TitleLiteral.Text = "回访历史";
                HintLabel.Text = "显示最近 50 条。新增回访请从有效领养记录进入。";
                AddLink.Visible = false;
                BackLink.Visible = false;
                Bind(new FollowUpBll().Recent());
                return;
            }
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
            if (detail != null)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    rows[i].CatNo = detail.Cat.CatNo;
                    rows[i].TempName = detail.Cat.TempName;
                }
            }
            Bind(rows);
        }

        private void Bind(List<FollowUpRecord> rows)
        {
            EmptyLabel.Visible = rows.Count == 0;
            ListRepeater.Visible = rows.Count > 0;
            ListRepeater.DataSource = rows;
            ListRepeater.DataBind();
        }
    }
}
