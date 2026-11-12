using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class ApplyAdoptionPage : PageBase
    {
        protected Label MessageLabel;
        protected Literal CatLiteral;
        protected TextBox NameBox;
        protected TextBox PhoneBox;
        protected TextBox AddressBox;
        protected TextBox ReasonBox;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int catId = PageHelper.Positive(Request.QueryString["catId"]);
                CatDetail detail = catId == 0 ? null : new CatBll().GetDetail(catId);
                if (detail == null || detail.Cat.Status != CatStatuses.WaitingAdoption)
                {
                    MessageLabel.Text = Messages.CatNotAdoptable;
                    SaveButton.Visible = false;
                    return;
                }
                CatLiteral.Text = PageHelper.Html(detail.Cat.CatNo + " " + detail.Cat.TempName);
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            var application = new AdoptionApplication
            {
                CatId = PageHelper.Positive(Request.QueryString["catId"]),
                ApplicantName = NameBox.Text,
                Phone = PhoneBox.Text,
                Address = AddressBox.Text,
                Reason = ReasonBox.Text
            };
            OperationResult<int> result = new ApplicationBll().Create(application, CurrentUser.UserId);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("~/MyCenter.aspx?msg=application");
        }
    }
}
