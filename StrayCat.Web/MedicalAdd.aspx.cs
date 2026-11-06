using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class MedicalAddPage : PageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder FormHolder;
        protected TextBox DateBox;
        protected TextBox DiagnosisBox;
        protected TextBox TreatmentBox;
        protected TextBox MedicineBox;
        protected TextBox RemarkBox;
        protected Button SaveButton;
        protected HyperLink BackLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int catId = PageHelper.Positive(Request.QueryString["catId"]);
                CatDetail detail = catId == 0 ? null : new CatBll().GetDetail(catId);
                if (detail == null)
                {
                    MessageLabel.Text = Messages.NotFound;
                    FormHolder.Visible = false;
                    return;
                }
                BackLink.NavigateUrl = "MedicalList.aspx?catId=" + catId;
                DateBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
                if (!CatRules.CanEditMedical(detail.Cat.Status))
                {
                    MessageLabel.Text = Messages.StatusNotAllowed;
                    SaveButton.Visible = false;
                }
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            int catId = PageHelper.Positive(Request.QueryString["catId"]);
            DateTime date;
            if (!PageHelper.ReadDate(DateBox.Text, out date))
            {
                MessageLabel.Text = Messages.BadDate;
                return;
            }
            OperationResult<int> result = new MedicalBll().Add(new MedicalRecord
            {
                CatId = catId,
                MedicalDate = date,
                Diagnosis = DiagnosisBox.Text,
                Treatment = TreatmentBox.Text,
                Medicine = MedicineBox.Text,
                Remark = RemarkBox.Text
            }, CurrentUser.UserId);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("MedicalDetail.aspx?id=" + result.Data);
        }
    }
}
