using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class AdoptionListPage : AdminPageBase
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
            if (Request.QueryString["catId"] == null)
            {
                TitleLiteral.Text = "领养记录";
                HintLabel.Text = "显示最近 50 条。登记领养前会校验档案是否为待领养。";
                AddLink.Visible = false;
                BackLink.Visible = false;
                Bind(new AdoptionBll().Recent());
                return;
            }
            int catId = PageHelper.Positive(Request.QueryString["catId"]);
            CatDetail detail = catId == 0 ? null : new CatBll().GetDetail(catId);
            if (detail == null)
            {
                MessageLabel.Text = Messages.NotFound;
                ListHolder.Visible = false;
                return;
            }
            AddLink.NavigateUrl = "AdoptionAdd.aspx?catId=" + catId;
            AddLink.Visible = CatRules.CanAddAdoption(detail.Cat.Status);
            BackLink.NavigateUrl = "CatDetail.aspx?id=" + catId;
            List<AdoptionRecord> rows = new AdoptionBll().ListByCat(catId);
            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].CatNo = detail.Cat.CatNo;
                rows[i].TempName = detail.Cat.TempName;
            }
            Bind(rows);
        }

        private void Bind(List<AdoptionRecord> rows)
        {
            EmptyLabel.Visible = rows.Count == 0;
            ListRepeater.Visible = rows.Count > 0;
            ListRepeater.DataSource = rows;
            ListRepeater.DataBind();
        }
    }
}
