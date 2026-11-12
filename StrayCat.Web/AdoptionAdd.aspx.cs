using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class AdoptionAddPage : AdminPageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder FormHolder;
        protected TextBox NameBox;
        protected TextBox PhoneBox;
        protected TextBox AddressBox;
        protected TextBox DateBox;
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
                BackLink.NavigateUrl = "AdoptionList.aspx?catId=" + catId;
                DateBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
                if (!CatRules.CanAddAdoption(detail.Cat.Status))
                {
                    MessageLabel.Text = CatRules.AdoptionEligibility(detail.Cat.Status);
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
            OperationResult<int> result = new AdoptionBll().Add(new AdoptionRecord
            {
                CatId = catId,
                AdopterName = NameBox.Text,
                AdopterPhone = PhoneBox.Text,
                AdopterAddress = AddressBox.Text,
                AdoptDate = date,
                Remark = RemarkBox.Text
            }, CurrentUser.UserId);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("AdoptionDetail.aspx?id=" + result.Data);
        }
    }
}
