using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class FollowUpAddPage : AdminPageBase
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
                int adoptionId = PageHelper.Positive(Request.QueryString["adoptionId"]);
                AdoptionRecord adoption = adoptionId == 0 ? null : new AdoptionBll().Get(adoptionId);
                if (adoption == null)
                {
                    MessageLabel.Text = Messages.NotFound;
                    FormHolder.Visible = false;
                    return;
                }
                CatDetail detail = new CatBll().GetDetail(adoption.CatId);
                BackLink.NavigateUrl = "FollowUpList.aspx?adoptionId=" + adoptionId;
                DateBox.Text = DateTime.Today.ToString("yyyy-MM-dd");
                if (detail == null || !CatRules.CanEditFollowUp(detail.Cat.Status, adoption.Status))
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
            int adoptionId = PageHelper.Positive(Request.QueryString["adoptionId"]);
            DateTime date;
            if (!PageHelper.ReadDate(DateBox.Text, out date))
            {
                MessageLabel.Text = Messages.BadDate;
                return;
            }
            OperationResult<int> result = new FollowUpBll().Add(new FollowUpRecord
            {
                AdoptionId = adoptionId,
                FollowDate = date,
                FollowWay = WayList.SelectedValue,
                CatCondition = ConditionBox.Text,
                Remark = RemarkBox.Text
            }, CurrentUser.UserId);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("FollowUpDetail.aspx?id=" + result.Data);
        }
    }
}
