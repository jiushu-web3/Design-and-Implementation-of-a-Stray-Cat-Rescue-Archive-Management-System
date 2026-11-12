using System;
using System.Web;
using StrayCat.Model;

namespace StrayCat.Web
{
    public static class LoginSession
    {
        public static CurrentUser Current
        {
            get
            {
                //获取当前上下文
                HttpContext context = HttpContext.Current;
                if (context == null || context.Session == null || context.Session["UserId"] == null)
                {
                    return null;
                }
                return new CurrentUser
                {
                    UserId = Convert.ToInt32(context.Session["UserId"]),
                    UserName = Convert.ToString(context.Session["UserName"]),
                    RealName = Convert.ToString(context.Session["RealName"]),
                    Role = Convert.ToByte(context.Session["Role"])
                };
            }
        }
        public static void Set(CurrentUser user)
        {
            HttpContext.Current.Session["UserId"] = user.UserId;
            HttpContext.Current.Session["UserName"] = user.UserName;
            HttpContext.Current.Session["RealName"] = user.RealName;
            HttpContext.Current.Session["Role"] = user.Role;
        }

        public static void Clear()
        {
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
        }
    }
}
