using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class AdoptionEditPage : AdminPageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder FormHolder;
        protected Literal DateLiteral;
        protected TextBox NameBox;
        protected TextBox PhoneBox;
        protected TextBox AddressBox;
        protected TextBox RemarkBox;
        protected Button SaveButton;
        protected Panel ReturnPanel;
        protected TextBox ReturnDateBox;
        protected TextBox ReturnReasonBox;
        protected Button ReturnButton;
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
            int adoptionId = PageHelper.Positive(Request.QueryString["id"]);
            OperationResult<int> result = new AdoptionBll().UpdateContact(new AdoptionRecord
            {
                AdoptionId = adoptionId,
                AdopterName = NameBox.Text,
                AdopterPhone = PhoneBox.Text,
                AdopterAddress = AddressBox.Text,
                Remark = RemarkBox.Text
            });
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("AdoptionDetail.aspx?id=" + adoptionId);
        }

        protected void ReturnButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            int adoptionId = PageHelper.Positive(Request.QueryString["id"]);
            DateTime date;
            if (!PageHelper.ReadDate(ReturnDateBox.Text, out date))
            {
                MessageLabel.Text = Messages.BadDate;
                return;
            }
            OperationResult<int> result = new AdoptionBll().Return(adoptionId, date, ReturnReasonBox.Text);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("AdoptionDetail.aspx?id=" + adoptionId);
        }

        private void LoadRecord()
        {
            int adoptionId = PageHelper.Positive(Request.QueryString["id"]);
            AdoptionRecord record = adoptionId == 0 ? null : new AdoptionBll().Get(adoptionId);
            if (record == null)
            {
                MessageLabel.Text = Messages.NotFound;
                FormHolder.Visible = false;
                return;
            }
            CatDetail detail = new CatBll().GetDetail(record.CatId);
            DateLiteral.Text = record.AdoptDateText;
            NameBox.Text = record.AdopterName;
            PhoneBox.Text = record.AdopterPhone;
            AddressBox.Text = record.AdopterAddress;
            RemarkBox.Text = record.Remark;
            ReturnDateBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
            BackLink.NavigateUrl = "AdoptionDetail.aspx?id=" + adoptionId;
            bool active = detail != null && record.Status == AdoptionStatuses.Active && CatRules.CanEditActiveAdoption(detail.Cat.Status);
            ReturnPanel.Visible = active;
            if (!active)
            {
                MessageLabel.Text = Messages.StatusNotAllowed;
                NameBox.ReadOnly = true;
                PhoneBox.ReadOnly = true;
                AddressBox.ReadOnly = true;
                RemarkBox.ReadOnly = true;
                SaveButton.Visible = false;
            }
        }
    }
}
