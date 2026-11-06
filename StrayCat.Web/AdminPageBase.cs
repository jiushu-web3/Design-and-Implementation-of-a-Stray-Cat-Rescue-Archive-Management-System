using System;
using StrayCat.Model;

namespace StrayCat.Web
{
    public class AdminPageBase : PageBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (Redirecting)
            {
                return;
            }
            CurrentUser user = LoginSession.Current;
            if (user == null)
            {
                return;
            }
            if (user.Role != RoleCodes.Admin)
            {
                Leave("~/Default.aspx?msg=forbidden");
            }
        }
    }
}
