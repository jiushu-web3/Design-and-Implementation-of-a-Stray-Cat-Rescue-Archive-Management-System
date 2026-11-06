using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class CatStatusPage : PageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder FormHolder;
        protected Literal StatusLiteral;
        protected Panel ToWaitingPanel;
        protected Button WaitingButton;
        protected Panel ToCarePanel;
        protected Button CareButton;
        protected Panel ClosePanel;
        protected Button CloseButton;
        protected Label CloseHint;
        protected Panel TerminatePanel;
        protected TextBox ReasonBox;
        protected Button TerminateButton;
        protected HyperLink BackLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ShowState();
            }
        }

        protected void WaitingButton_Click(object sender, EventArgs e)
        {
            Apply(CatStatuses.WaitingAdoption, null);
        }

        protected void CareButton_Click(object sender, EventArgs e)
        {
            Apply(CatStatuses.InCare, null);
        }

        protected void CloseButton_Click(object sender, EventArgs e)
        {
            Apply(CatStatuses.Closed, null);
        }

        protected void TerminateButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            Apply(CatStatuses.Terminated, ReasonBox.Text);
        }

        private void Apply(byte target, string reason)
        {
            int catId = PageHelper.Positive(Request.QueryString["id"]);
            OperationResult<int> result = new CatBll().ChangeStatus(catId, target, reason);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                ShowState();
                return;
            }
            Leave("CatDetail.aspx?id=" + catId);
        }

        private void ShowState()
        {
            int catId = PageHelper.Positive(Request.QueryString["id"]);
            CatDetail detail = catId == 0 ? null : new CatBll().GetDetail(catId);
            if (detail == null)
            {
                MessageLabel.Text = Messages.NotFound;
                FormHolder.Visible = false;
                return;
            }
            byte status = detail.Cat.Status;
            StatusLiteral.Text = CatStatuses.Name(status);
            BackLink.NavigateUrl = "CatDetail.aspx?id=" + catId;
            ToWaitingPanel.Visible = status == CatStatuses.InCare;
            ToCarePanel.Visible = status == CatStatuses.WaitingAdoption;
            TerminatePanel.Visible = status == CatStatuses.InCare || status == CatStatuses.WaitingAdoption;
            ClosePanel.Visible = status == CatStatuses.Adopted;
            if (ClosePanel.Visible)
            {
                string block = new CatBll().CloseBlock(catId);
                CloseButton.Enabled = block == null;
                CloseHint.Text = block ?? "";
            }
            if (CatStatuses.IsClosed(status) && string.IsNullOrEmpty(MessageLabel.Text))
            {
                MessageLabel.Text = Messages.StatusNotAllowed;
            }
        }
    }
}
