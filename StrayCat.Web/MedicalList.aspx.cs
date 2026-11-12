using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class MedicalListPage : AdminPageBase
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
                TitleLiteral.Text = "诊疗历史";
                HintLabel.Text = "显示最近 50 条。新增诊疗请从档案详情进入。";
                AddLink.Visible = false;
                BackLink.Visible = false;
                Bind(new MedicalBll().Recent());
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
            AddLink.NavigateUrl = "MedicalAdd.aspx?catId=" + catId;
            AddLink.Visible = CatRules.CanEditMedical(detail.Cat.Status);
            BackLink.NavigateUrl = "CatDetail.aspx?id=" + catId;
            List<MedicalRecord> rows = new MedicalBll().ListByCat(catId);
            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].CatNo = detail.Cat.CatNo;
                rows[i].TempName = detail.Cat.TempName;
            }
            EmptyLabel.Visible = rows.Count == 0;
            ListRepeater.Visible = rows.Count > 0;
            Bind(rows);
        }

        private void Bind(List<MedicalRecord> rows)
        {
            EmptyLabel.Visible = rows.Count == 0;
            ListRepeater.Visible = rows.Count > 0;
            ListRepeater.DataSource = rows;
            ListRepeater.DataBind();
        }
    }
}
