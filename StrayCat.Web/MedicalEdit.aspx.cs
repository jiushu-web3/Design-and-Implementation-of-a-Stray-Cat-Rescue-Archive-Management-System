using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class MedicalEditPage : AdminPageBase
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
                LoadRecord();
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            int medicalId = PageHelper.Positive(Request.QueryString["id"]);
            DateTime date;
            if (!PageHelper.ReadDate(DateBox.Text, out date))
            {
                MessageLabel.Text = Messages.BadDate;
                return;
            }
            OperationResult<int> result = new MedicalBll().Update(new MedicalRecord
            {
                MedicalId = medicalId,
                MedicalDate = date,
                Diagnosis = DiagnosisBox.Text,
                Treatment = TreatmentBox.Text,
                Medicine = MedicineBox.Text,
                Remark = RemarkBox.Text
            });
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("MedicalDetail.aspx?id=" + medicalId);
        }

        private void LoadRecord()
        {
            int medicalId = PageHelper.Positive(Request.QueryString["id"]);
            MedicalRecord record = medicalId == 0 ? null : new MedicalBll().Get(medicalId);
            if (record == null)
            {
                MessageLabel.Text = Messages.NotFound;
                FormHolder.Visible = false;
                return;
            }
            CatDetail detail = new CatBll().GetDetail(record.CatId);
            DateBox.Text = record.MedicalDate.ToString("yyyy-MM-dd");
            DiagnosisBox.Text = record.Diagnosis;
            TreatmentBox.Text = record.Treatment;
            MedicineBox.Text = record.Medicine;
            RemarkBox.Text = record.Remark;
            BackLink.NavigateUrl = "MedicalDetail.aspx?id=" + medicalId;
            if (detail == null || !CatRules.CanEditMedical(detail.Cat.Status))
            {
                MessageLabel.Text = detail == null ? Messages.NotFound : Messages.StatusNotAllowed;
                DateBox.ReadOnly = true;
                DiagnosisBox.ReadOnly = true;
                TreatmentBox.ReadOnly = true;
                MedicineBox.ReadOnly = true;
                RemarkBox.ReadOnly = true;
                SaveButton.Visible = false;
            }
        }
    }
}
