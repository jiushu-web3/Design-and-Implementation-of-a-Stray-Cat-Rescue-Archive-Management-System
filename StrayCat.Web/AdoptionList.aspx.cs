using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class AdoptionListPage : PageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder ListHolder;
        protected HyperLink AddLink;
        protected Label EmptyLabel;
        protected Repeater ListRepeater;
        protected HyperLink BackLink;

        protected void Page_Load(object sender, EventArgs e)
        {
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
            EmptyLabel.Visible = rows.Count == 0;
            ListRepeater.Visible = rows.Count > 0;
            ListRepeater.DataSource = rows;
            ListRepeater.DataBind();
        }
    }
}
