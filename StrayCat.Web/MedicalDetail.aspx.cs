using System;
using System.Text;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class MedicalDetailPage : AdminPageBase
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
            int medicalId = PageHelper.Positive(Request.QueryString["id"]);
            MedicalRecord record = medicalId == 0 ? null : new MedicalBll().Get(medicalId);
            if (record == null)
            {
                ShowMissing();
                return;
            }
            OperationResult<int> result = new MedicalBll().Delete(medicalId);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("MedicalList.aspx?catId=" + record.CatId);
        }

        private void LoadRecord()
        {
            int medicalId = PageHelper.Positive(Request.QueryString["id"]);
            MedicalRecord record = medicalId == 0 ? null : new MedicalBll().Get(medicalId);
            if (record == null)
            {
                ShowMissing();
                return;
            }
            CatDetail detail = new CatBll().GetDetail(record.CatId);
            StringBuilder html = new StringBuilder();
            html.Append("<p><span>日期</span>").Append(PageHelper.Html(record.MedicalDateText)).Append("</p>");
            html.Append("<p><span>诊断</span>").Append(PageHelper.Html(record.Diagnosis)).Append("</p>");
            html.Append("<p><span>处理</span>").Append(PageHelper.Html(record.Treatment)).Append("</p>");
            html.Append("<p><span>用药</span>").Append(PageHelper.Html(record.Medicine)).Append("</p>");
            html.Append("<p><span>备注</span>").Append(PageHelper.Html(record.Remark)).Append("</p>");
            InfoLiteral.Text = html.ToString();
            EditLink.NavigateUrl = "MedicalEdit.aspx?id=" + medicalId;
            BackLink.NavigateUrl = "MedicalList.aspx?catId=" + record.CatId;
            bool editable = detail != null && CatRules.CanEditMedical(detail.Cat.Status);
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
