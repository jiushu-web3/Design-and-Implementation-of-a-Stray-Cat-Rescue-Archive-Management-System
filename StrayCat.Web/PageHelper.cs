using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using StrayCat.Model;

namespace StrayCat.Web
{
    public static class PageHelper
    {
        public static int Positive(string text)
        {
            int value;
            if (int.TryParse(text, out value) && value > 0)
            {
                return value;
            }
            return 0;
        }

        public static bool ReadDate(string text, out DateTime value)
        {
            return DateTime.TryParseExact((text ?? "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
        }

        public static bool ReadDateTime(string dateText, string timeText, out DateTime value)
        {
            value = DateTime.MinValue;
            DateTime date;
            if (!ReadDate(dateText, out date))
            {
                return false;
            }
            string time = (timeText ?? "").Trim();
            if (time.Length == 0)
            {
                time = "00:00";
            }
            DateTime clock;
            if (!DateTime.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out clock))
            {
                return false;
            }
            value = date.Date.Add(clock.TimeOfDay);
            return true;
        }

        public static string Encode(string value)
        {
            return HttpUtility.UrlEncode(value ?? "");
        }

        public static string Html(string value)
        {
            return HttpUtility.HtmlEncode(value ?? "");
        }

        public static byte? SelectedByte(DropDownList list)
        {
            byte value;
            if (list.SelectedValue.Length == 0 || !byte.TryParse(list.SelectedValue, out value))
            {
                return null;
            }
            return value;
        }

        public static void Select(DropDownList list, string value)
        {
            ListItem item = list.Items.FindByValue(value ?? "");
            if (item != null)
            {
                list.SelectedValue = item.Value;
            }
        }

        public static void BindStatuses(DropDownList list, bool includeAll)
        {
            list.Items.Clear();
            if (includeAll)
            {
                list.Items.Add(new ListItem("全部", ""));
            }
            AddStatus(list, CatStatuses.InCare);
            AddStatus(list, CatStatuses.WaitingAdoption);
            AddStatus(list, CatStatuses.Adopted);
            AddStatus(list, CatStatuses.Closed);
            AddStatus(list, CatStatuses.Terminated);
        }

        public static void BindGenders(DropDownList list, bool includeAll)
        {
            list.Items.Clear();
            if (includeAll)
            {
                list.Items.Add(new ListItem("全部", ""));
            }
            list.Items.Add(new ListItem("未知", "0"));
            list.Items.Add(new ListItem("公", "1"));
            list.Items.Add(new ListItem("母", "2"));
        }

        public static void BindNeutered(DropDownList list)
        {
            list.Items.Clear();
            list.Items.Add(new ListItem("未知", "0"));
            list.Items.Add(new ListItem("已绝育", "1"));
            list.Items.Add(new ListItem("未绝育", "2"));
        }

        public static void BindRoles(DropDownList list)
        {
            list.Items.Clear();
            list.Items.Add(new ListItem("管理员", RoleCodes.Admin.ToString()));
            list.Items.Add(new ListItem("工作人员", RoleCodes.Staff.ToString()));
        }

        public static void BindWays(DropDownList list, string[] options)
        {
            list.Items.Clear();
            list.Items.Add(new ListItem("请选择", ""));
            for (int i = 0; i < options.Length; i++)
            {
                list.Items.Add(new ListItem(options[i], options[i]));
            }
        }

        public static string SavePhoto(FileUpload upload, HttpServerUtility server, out string error)
        {
            error = null;
            if (upload == null || !upload.HasFile)
            {
                return null;
            }
            string ext = Path.GetExtension(upload.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
            {
                error = Messages.PhotoType;
                return null;
            }
            if (upload.PostedFile.ContentLength > 2 * 1024 * 1024)
            {
                error = Messages.PhotoSize;
                return null;
            }
            string name = Guid.NewGuid().ToString("N") + ext;
            string folder = server.MapPath("~/Uploads/cats");
            Directory.CreateDirectory(folder);
            upload.SaveAs(Path.Combine(folder, name));
            return "Uploads/cats/" + name;
        }

        public static void DeletePhoto(string relative, HttpServerUtility server)
        {
            if (string.IsNullOrWhiteSpace(relative))
            {
                return;
            }
            string normalized = relative.Replace('\\', '/').Trim();
            if (normalized.StartsWith("~/", StringComparison.Ordinal))
            {
                normalized = normalized.Substring(2);
            }
            if (normalized.IndexOf("..", StringComparison.Ordinal) >= 0)
            {
                return;
            }
            if (!normalized.StartsWith("Uploads/cats/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            string root = Path.GetFullPath(server.MapPath("~/Uploads/cats"));
            if (!root.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                root += Path.DirectorySeparatorChar;
            }
            string full = Path.GetFullPath(server.MapPath("~/" + normalized));
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
            {
                return;
            }
            try
            {
                File.Delete(full);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public static void ShowPhoto(Image image, string relative, HttpServerUtility server)
        {
            image.Visible = false;
            if (string.IsNullOrWhiteSpace(relative))
            {
                return;
            }
            string normalized = relative.Replace('\\', '/').Trim();
            if (!normalized.StartsWith("Uploads/cats/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            if (!File.Exists(server.MapPath("~/" + normalized)))
            {
                return;
            }
            image.ImageUrl = "~/" + normalized;
            image.Visible = true;
        }

        public static string Pager(string hrefWithoutPage, int pageIndex, int total, int pageSize)
        {
            if (total <= pageSize)
            {
                return "";
            }
            int pages = (total + pageSize - 1) / pageSize;
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            if (pageIndex > pages)
            {
                pageIndex = pages;
            }
            StringBuilder html = new StringBuilder();
            html.Append("<div class=\"pager\">");
            AppendLink(html, hrefWithoutPage, pageIndex - 1, "上一页", pageIndex > 1);
            int start = Math.Max(1, pageIndex - 3);
            int end = Math.Min(pages, pageIndex + 3);
            for (int i = start; i <= end; i++)
            {
                if (i == pageIndex)
                {
                    html.Append("<span>").Append(i).Append("</span>");
                }
                else
                {
                    AppendLink(html, hrefWithoutPage, i, i.ToString(), true);
                }
            }
            AppendLink(html, hrefWithoutPage, pageIndex + 1, "下一页", pageIndex < pages);
            html.Append("</div>");
            return html.ToString();
        }

        private static void AppendLink(StringBuilder html, string hrefWithoutPage, int page, string text, bool enabled)
        {
            if (!enabled)
            {
                return;
            }
            string join = hrefWithoutPage.IndexOf('?') >= 0 ? "&" : "?";
            html.Append("<a href=\"").Append(hrefWithoutPage).Append(join).Append("page=").Append(page).Append("\">");
            html.Append(Html(text)).Append("</a>");
        }

        private static void AddStatus(DropDownList list, byte status)
        {
            list.Items.Add(new ListItem(CatStatuses.Name(status), status.ToString()));
        }
    }
}
