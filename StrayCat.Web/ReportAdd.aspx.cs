using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class ReportAddPage : PageBase
    {
        protected Label MessageLabel;
        protected TextBox PlaceBox;
        protected TextBox DateBox;
        protected TextBox DescBox;
        protected TextBox PhoneBox;
        protected FileUpload PhotoFile;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DateBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            DateTime found;
            if (!PageHelper.ReadDate(DateBox.Text, out found))
            {
                MessageLabel.Text = Messages.BadDate;
                return;
            }
            string photoError;
            string photo = PageHelper.SavePhoto(PhotoFile, Server, out photoError);
            if (photoError != null)
            {
                MessageLabel.Text = photoError;
                return;
            }
            StrayReport report = new StrayReport
            {
                FoundPlace = PlaceBox.Text,
                FoundDate = found,
                Description = DescBox.Text,
                ContactPhone = PhoneBox.Text,
                PhotoPath = photo
            };
            OperationResult<int> result = new ReportBll().Create(report, CurrentUser.UserId);
            if (!result.Success)
            {
                PageHelper.DeletePhoto(photo, Server);
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("~/MyCenter.aspx?msg=report");
        }
    }
}
