using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class CatAddPage : PageBase
    {
        protected Label MessageLabel;
        protected TextBox TempNameBox;
        protected DropDownList GenderList;
        protected TextBox AgeBox;
        protected TextBox ColorBox;
        protected DropDownList NeuteredList;
        protected FileUpload PhotoUpload;
        protected TextBox RemarkBox;
        protected TextBox DateBox;
        protected TextBox TimeBox;
        protected TextBox LocationBox;
        protected DropDownList FoundWayList;
        protected TextBox HealthBox;
        protected TextBox RescueRemarkBox;
        protected Button SaveButton;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PageHelper.BindGenders(GenderList, false);
                PageHelper.BindNeutered(NeuteredList);
                PageHelper.BindWays(FoundWayList, Lookups.FoundWays);
                DateBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            DateTime rescueTime;
            if (!PageHelper.ReadDateTime(DateBox.Text, TimeBox.Text, out rescueTime))
            {
                MessageLabel.Text = Messages.BadDate;
                return;
            }
            string photoError;
            string photo = PageHelper.SavePhoto(PhotoUpload, Server, out photoError);
            if (photoError != null)
            {
                MessageLabel.Text = photoError;
                return;
            }
            CatArchive cat = new CatArchive
            {
                TempName = TempNameBox.Text,
                Gender = PageHelper.SelectedByte(GenderList) ?? 0,
                EstimatedAge = AgeBox.Text,
                Color = ColorBox.Text,
                IsNeutered = PageHelper.SelectedByte(NeuteredList) ?? 0,
                PhotoPath = photo,
                Remark = RemarkBox.Text
            };
            RescueRecord rescue = new RescueRecord
            {
                RescueTime = rescueTime,
                Location = LocationBox.Text,
                FoundWay = FoundWayList.SelectedValue,
                InitialHealth = HealthBox.Text,
                Remark = RescueRemarkBox.Text
            };
            OperationResult<int> result = new CatBll().Create(cat, rescue, CurrentUser.UserId);
            if (!result.Success)
            {
                PageHelper.DeletePhoto(photo, Server);
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("CatDetail.aspx?id=" + result.Data);
        }
    }
}
