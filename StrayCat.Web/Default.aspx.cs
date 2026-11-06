using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class DefaultPage : PageBase
    {
        protected Label NoticeLabel;
        protected Literal InCareLiteral;
        protected Literal WaitingLiteral;
        protected Literal FollowLiteral;
        protected PlaceHolder UserCountHolder;
        protected Literal UserCountLiteral;
        protected Label EmptyLabel;
        protected Repeater RecentRepeater;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["msg"] == "forbidden")
            {
                NoticeLabel.Text = Messages.Forbidden;
            }
            HomeCounts counts = new CatBll().GetHomeCounts();
            InCareLiteral.Text = counts.InCareCount.ToString();
            WaitingLiteral.Text = counts.WaitingCount.ToString();
            FollowLiteral.Text = counts.AdoptedWithoutFollowUpCount.ToString();
            if (CurrentUser != null && CurrentUser.IsAdmin)
            {
                UserCountHolder.Visible = true;
                UserCountLiteral.Text = new UserBll().CountEnabled().ToString();
            }
            List<CatListItem> recent = new CatBll().GetRecent();
            EmptyLabel.Visible = recent.Count == 0;
            RecentRepeater.Visible = recent.Count > 0;
            RecentRepeater.DataSource = recent;
            RecentRepeater.DataBind();
        }
    }
}
