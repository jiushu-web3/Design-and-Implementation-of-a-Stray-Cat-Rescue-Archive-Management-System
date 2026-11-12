using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class CatDetailPage : AdminPageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder DetailHolder;
        protected Image PhotoImage;
        protected Literal InfoLiteral;
        protected HyperLink EditLink;
        protected HyperLink RescueLink;
        protected HyperLink StatusLink;
        protected HyperLink MedicalLink;
        protected HyperLink AddMedicalLink;
        protected HyperLink AdoptionLink;
        protected HyperLink AddAdoptionLink;
        protected Button DeleteButton;
        protected Label MedicalEmptyLabel;
        protected Repeater MedicalRepeater;
        protected Label AdoptionEmptyLabel;
        protected Repeater AdoptionRepeater;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDetail();
            }
        }

        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            int catId = PageHelper.Positive(Request.QueryString["id"]);
            CatDetail detail = catId == 0 ? null : new CatBll().GetDetail(catId);
            if (detail == null)
            {
                ShowMissing();
                return;
            }
            OperationResult<int> result = new CatBll().Delete(catId);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                LoadDetail();
                return;
            }
            PageHelper.DeletePhoto(detail.Cat.PhotoPath, Server);
            Leave("CatList.aspx");
        }

        private void LoadDetail()
        {
            int catId = PageHelper.Positive(Request.QueryString["id"]);
            CatDetail detail = catId == 0 ? null : new CatBll().GetDetail(catId);
            if (detail == null)
            {
                ShowMissing();
                return;
            }
            CatArchive cat = detail.Cat;
            RescueRecord rescue = detail.Rescue;
            PageHelper.ShowPhoto(PhotoImage, cat.PhotoPath, Server);
            StringBuilder html = new StringBuilder();
            Line(html, "档案编号", cat.CatNo);
            Line(html, "临时名字", cat.TempName);
            Line(html, "性别", Lookups.GenderName(cat.Gender));
            Line(html, "估计年龄", cat.EstimatedAge);
            Line(html, "毛色", cat.Color);
            Line(html, "绝育", Lookups.NeuteredName(cat.IsNeutered));
            Line(html, "状态", CatStatuses.Name(cat.Status));
            Line(html, "领养资格", CatRules.AdoptionEligibility(cat.Status));
            if (cat.Status == CatStatuses.Terminated)
            {
                Line(html, "终止原因", cat.TerminateReason);
            }
            Line(html, "档案备注", cat.Remark);
            Line(html, "接收时间", rescue.RescueTimeText);
            Line(html, "接收地点", rescue.Location);
            Line(html, "发现方式", rescue.FoundWay);
            Line(html, "初步情况", rescue.InitialHealth);
            InfoLiteral.Text = html.ToString();

            EditLink.NavigateUrl = "CatEdit.aspx?id=" + catId;
            EditLink.Visible = CatRules.CanEditArchive(cat.Status);
            RescueLink.NavigateUrl = "RescueDetail.aspx?catId=" + catId;
            StatusLink.NavigateUrl = "CatStatus.aspx?id=" + catId;
            MedicalLink.NavigateUrl = "MedicalList.aspx?catId=" + catId;
            AddMedicalLink.NavigateUrl = "MedicalAdd.aspx?catId=" + catId;
            AddMedicalLink.Visible = CatRules.CanEditMedical(cat.Status);
            AdoptionLink.NavigateUrl = "AdoptionList.aspx?catId=" + catId;
            AddAdoptionLink.NavigateUrl = "AdoptionAdd.aspx?catId=" + catId;
            AddAdoptionLink.Visible = CatRules.CanAddAdoption(cat.Status);

            List<MedicalRecord> medical = new MedicalBll().ListByCat(catId);
            if (medical.Count > 5)
            {
                medical = medical.GetRange(0, 5);
            }
            MedicalEmptyLabel.Visible = medical.Count == 0;
            MedicalRepeater.Visible = medical.Count > 0;
            MedicalRepeater.DataSource = medical;
            MedicalRepeater.DataBind();

            List<AdoptionRecord> adoptions = new AdoptionBll().ListByCat(catId);
            AdoptionEmptyLabel.Visible = adoptions.Count == 0;
            AdoptionRepeater.Visible = adoptions.Count > 0;
            AdoptionRepeater.DataSource = adoptions;
            AdoptionRepeater.DataBind();
        }

        private static void Line(StringBuilder html, string name, string value)
        {
            html.Append("<p><span>").Append(PageHelper.Html(name)).Append("</span>");
            html.Append(PageHelper.Html(value)).Append("</p>");
        }

        private void ShowMissing()
        {
            MessageLabel.Text = Messages.NotFound;
            DetailHolder.Visible = false;
        }
    }
}
