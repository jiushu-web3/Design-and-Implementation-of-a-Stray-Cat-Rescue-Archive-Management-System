using System.Web;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public static class CaptchaSession
    {
        public static bool Take(string scope, string input)
        {
            HttpContext context = HttpContext.Current;
            if (context == null || context.Session == null || !IsScope(scope))
            {
                return false;
            }
            string key = Key(scope);
            string expected = context.Session[key] as string;
            context.Session.Remove(key);
            return CaptchaCode.Matches(expected, input);
        }

        public static void Validate(string scope, object source, ServerValidateEventArgs args)
        {
            string text = args.Value == null ? "" : args.Value.Trim();
            CustomValidator validator = (CustomValidator)source;
            bool match = Take(scope, args.Value);
            if (!CaptchaCode.IsAlphabet(text))
            {
                validator.ErrorMessage = Messages.CaptchaRule;
                args.IsValid = false;
                return;
            }
            args.IsValid = match;
            if (!match)
            {
                validator.ErrorMessage = Messages.BadCaptcha;
            }
        }

        public static string ImageUrl(string scope)
        {
            return "Captcha.aspx?scope=" + scope + "&t=" + System.DateTime.UtcNow.Ticks;
        }

        private static bool IsScope(string scope)
        {
            return scope == "login" || scope == "register";
        }

        private static string Key(string scope)
        {
            return "CaptchaCode_" + scope;
        }
    }
}
