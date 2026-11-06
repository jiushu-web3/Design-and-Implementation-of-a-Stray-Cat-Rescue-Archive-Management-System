using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class FollowUpEditPage : PageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder FormHolder;
        protected TextBox DateBox;
        protected DropDownList WayList;
        protected TextBox ConditionBox;
        protected TextBox RemarkBox;
        protected Button SaveButton;
        protected HyperLink BackLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PageHelper.BindWays(WayList, Lookups.FollowWays);
                LoadRecord();
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            int followUpId = PageHelper.Positive(Request.QueryString["id"]);
            DateTime date;
            if (!PageHelper.ReadDate(DateBox.Text, out date))
            {
                MessageLabel.Text = Messages.BadDate;
                return;
            }
            OperationResult<int> result = new FollowUpBll().Update(new FollowUpRecord
            {
                FollowUpId = followUpId,
                FollowDate = date,
                FollowWay = WayList.SelectedValue,
                CatCondition = ConditionBox.Text,
                Remark = RemarkBox.Text
            });
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("FollowUpDetail.aspx?id=" + followUpId);
        }

        private void LoadRecord()
        {
            int followUpId = PageHelper.Positive(Request.QueryString["id"]);
            FollowUpRecord record = followUpId == 0 ? null : new FollowUpBll().Get(followUpId);
            if (record == null)
            {
                MessageLabel.Text = Messages.NotFound;
                FormHolder.Visible = false;
                return;
            }
            AdoptionRecord adoption = new AdoptionBll().Get(record.AdoptionId);
            CatDetail detail = adoption == null ? null : new CatBll().GetDetail(adoption.CatId);
            DateBox.Text = record.FollowDate.ToString("yyyy-MM-dd");
            ConditionBox.Text = record.CatCondition;
            RemarkBox.Text = record.Remark;
            PageHelper.Select(WayList, record.FollowWay);
            BackLink.NavigateUrl = "FollowUpDetail.aspx?id=" + followUpId;
            if (detail == null || adoption == null || !CatRules.CanEditFollowUp(detail.Cat.Status, adoption.Status))
            {
                MessageLabel.Text = Messages.StatusNotAllowed;
                DateBox.ReadOnly = true;
                ConditionBox.ReadOnly = true;
                RemarkBox.ReadOnly = true;
                WayList.Enabled = false;
                SaveButton.Visible = false;
            }
        }
    }
}
