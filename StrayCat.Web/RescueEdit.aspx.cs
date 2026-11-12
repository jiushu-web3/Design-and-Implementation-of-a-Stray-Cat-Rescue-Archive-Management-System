using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class RescueEditPage : AdminPageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder FormHolder;
        protected TextBox DateBox;
        protected TextBox TimeBox;
        protected TextBox LocationBox;
        protected DropDownList FoundWayList;
        protected TextBox HealthBox;
        protected TextBox RemarkBox;
        protected Button SaveButton;
        protected HyperLink BackLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PageHelper.BindWays(FoundWayList, Lookups.FoundWays);
                LoadRescue();
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            int catId = PageHelper.Positive(Request.QueryString["catId"]);
            DateTime rescueTime;
            if (!PageHelper.ReadDateTime(DateBox.Text, TimeBox.Text, out rescueTime))
            {
                MessageLabel.Text = Messages.BadDate;
                return;
            }
            OperationResult<int> result = new CatBll().UpdateRescue(new RescueRecord
            {
                CatId = catId,
                RescueTime = rescueTime,
                Location = LocationBox.Text,
                FoundWay = FoundWayList.SelectedValue,
                InitialHealth = HealthBox.Text,
                Remark = RemarkBox.Text
            });
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("RescueDetail.aspx?catId=" + catId);
        }

        private void LoadRescue()
        {
            int catId = PageHelper.Positive(Request.QueryString["catId"]);
            CatDetail detail = catId == 0 ? null : new CatBll().GetDetail(catId);
            if (detail == null)
            {
                MessageLabel.Text = Messages.NotFound;
                FormHolder.Visible = false;
                return;
            }
            RescueRecord rescue = detail.Rescue;
            DateBox.Text = rescue.RescueTime.ToString("yyyy-MM-dd");
            TimeBox.Text = rescue.RescueTime.ToString("HH:mm");
            LocationBox.Text = rescue.Location;
            HealthBox.Text = rescue.InitialHealth;
            RemarkBox.Text = rescue.Remark;
            PageHelper.Select(FoundWayList, rescue.FoundWay);
            BackLink.NavigateUrl = "RescueDetail.aspx?catId=" + catId;
            if (!CatRules.CanEditArchive(detail.Cat.Status))
            {
                MessageLabel.Text = Messages.StatusNotAllowed;
                DateBox.ReadOnly = true;
                TimeBox.ReadOnly = true;
                LocationBox.ReadOnly = true;
                HealthBox.ReadOnly = true;
                RemarkBox.ReadOnly = true;
                FoundWayList.Enabled = false;
                SaveButton.Visible = false;
            }
        }
    }
}
