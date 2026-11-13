using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    /// <summary>
    /// 数据统计页（管理员）：按年份展示月度接收 / 领养数量，
    /// 图表为后台拼装的内联 SVG，离线可用、可直接打印进论文。
    /// </summary>
    public partial class StatPage : AdminPageBase
    {
        // 图表配色与 site.css 中的设计令牌保持一致
        private const string RescueColor = "#FF8FA3";   // --coral
        private const string AdoptColor = "#7FCDB4";    // --matcha
        private const string GridColor = "#F7E4DD";     // --line
        private const string AxisColor = "#EFD2C8";     // --line-strong
        private const string LabelColor = "#9A857C";    // --muted

        protected DropDownList YearList;
        protected Repeater MonthRepeater;
        protected Literal RescueTotalLiteral;
        protected Literal AdoptionTotalLiteral;
        protected Literal RateLiteral;
        protected Literal ChartLiteral;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                List<int> years = new StatBll().Years();
                for (int i = 0; i < years.Count; i++)
                {
                    YearList.Items.Add(new ListItem(years[i].ToString(), years[i].ToString()));
                }
                PageHelper.Select(YearList, DateTime.Today.Year.ToString());
                BindMonths();
            }
        }

        protected void YearList_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindMonths();
        }

        /// <summary>
        /// 绑定某一年度的月度数据：明细表、汇总指标、柱状图一次算出。
        /// usp_Stat_Monthly 恒定返回 12 行（无数据补 0），所以月度明细不会缺行。
        /// </summary>
        private void BindMonths()
        {
            int year;
            if (!int.TryParse(YearList.SelectedValue, out year))
            {
                year = DateTime.Today.Year;
            }

            List<MonthStat> months = new StatBll().Monthly(year);

            MonthRepeater.DataSource = months;
            MonthRepeater.DataBind();

            int rescueTotal = 0;
            int adoptTotal = 0;
            for (int i = 0; i < months.Count; i++)
            {
                rescueTotal += months[i].RescueCount;
                adoptTotal += months[i].AdoptionCount;
            }

            RescueTotalLiteral.Text = rescueTotal.ToString(CultureInfo.InvariantCulture);
            AdoptionTotalLiteral.Text = adoptTotal.ToString(CultureInfo.InvariantCulture);
            RateLiteral.Text = rescueTotal == 0
                ? "—"
                : (adoptTotal * 100.0 / rescueTotal).ToString("0.#", CultureInfo.InvariantCulture) + "%";

            ChartLiteral.Text = BuildChart(months);
        }

        /// <summary>
        /// 生成月度接收 / 领养对比柱状图（内联 SVG）。
        /// 坐标系：viewBox 720×250，左侧留 46px 放刻度，底部留 44px 放月份。
        /// </summary>
        private static string BuildChart(List<MonthStat> months)
        {
            const int width = 720;
            const int height = 250;
            const int padLeft = 46;
            const int padRight = 16;
            const int padTop = 20;
            const int padBottom = 44;

            if (months == null || months.Count == 0)
            {
                return "<p class=\"empty\">该年度暂无统计数据</p>";
            }

            int plotW = width - padLeft - padRight;
            int plotH = height - padTop - padBottom;
            int baseY = padTop + plotH;

            int peak = 0;
            for (int i = 0; i < months.Count; i++)
            {
                if (months[i].RescueCount > peak) peak = months[i].RescueCount;
                if (months[i].AdoptionCount > peak) peak = months[i].AdoptionCount;
            }
            if (peak <= 0)
            {
                return "<p class=\"empty\">该年度暂无接收 / 领养记录</p>";   // 空库时给空状态，不留一副空坐标轴
            }

            int step = NiceStep(peak);
            int axisMax = ((peak + step - 1) / step) * step;
            if (axisMax <= peak)
            {
                axisMax += step;    // 顶端留一格余量，柱子不贴到画布上沿
            }

            StringBuilder sb = new StringBuilder(4096);
            sb.Append("<svg class=\"chart-svg\" viewBox=\"0 0 ").Append(width).Append(' ').Append(height)
              .Append("\" preserveAspectRatio=\"xMidYMid meet\" role=\"img\"")
              .Append(" aria-label=\"月度救助接收与领养登记数量对比柱状图\"")
              .Append(" xmlns=\"http://www.w3.org/2000/svg\">");

            // 横向网格线 + 左侧刻度值
            for (int value = 0; value <= axisMax; value += step)
            {
                int y = baseY - (int)Math.Round((double)value / axisMax * plotH);
                sb.Append("<line x1=\"").Append(padLeft)
                  .Append("\" y1=\"").Append(y)
                  .Append("\" x2=\"").Append(padLeft + plotW)
                  .Append("\" y2=\"").Append(y)
                  .Append("\" stroke=\"").Append(value == 0 ? AxisColor : GridColor)
                  .Append("\" stroke-width=\"1\"");
                if (value != 0)
                {
                    sb.Append(" stroke-dasharray=\"3 4\"");
                }
                sb.Append("/>");

                sb.Append("<text x=\"").Append(padLeft - 10)
                  .Append("\" y=\"").Append(y + 4)
                  .Append("\" text-anchor=\"end\" font-size=\"11\" fill=\"").Append(LabelColor).Append("\">")
                  .Append(value).Append("</text>");
            }

            int groupW = plotW / months.Count;
            const int barW = 14;
            const int halfGap = 2;

            for (int i = 0; i < months.Count; i++)
            {
                MonthStat month = months[i];
                int centerX = padLeft + groupW * i + groupW / 2;

                AppendBar(sb, centerX - halfGap - barW, baseY, barW, month.RescueCount, axisMax, plotH, RescueColor, month.Month, "接收");
                AppendBar(sb, centerX + halfGap, baseY, barW, month.AdoptionCount, axisMax, plotH, AdoptColor, month.Month, "领养");

                sb.Append("<text x=\"").Append(centerX)
                  .Append("\" y=\"").Append(baseY + 20)
                  .Append("\" text-anchor=\"middle\" font-size=\"11\" fill=\"").Append(LabelColor).Append("\">")
                  .Append(month.Month).Append("月</text>");
            }

            sb.Append("</svg>");
            return sb.ToString();
        }

        private static void AppendBar(StringBuilder sb, int x, int baseY, int barW, int value,
            int axisMax, int plotH, string color, int month, string label)
        {
            if (value <= 0)
            {
                return;     // 0 不画柱子，只留基线
            }

            int h = (int)Math.Round((double)value / axisMax * plotH);
            if (h < 2)
            {
                h = 2;      // 极小值也给一截可见高度
            }

            sb.Append("<rect x=\"").Append(x)
              .Append("\" y=\"").Append(baseY - h)
              .Append("\" width=\"").Append(barW)
              .Append("\" height=\"").Append(h)
              .Append("\" rx=\"3\" fill=\"").Append(color).Append("\">")
              .Append("<title>").Append(month).Append("月 ").Append(label).Append(' ')
              .Append(value).Append(" 条</title>")
              .Append("</rect>");
        }

        /// <summary>
        /// 取一个「好看」的刻度步长，让纵轴落在 1 / 2 / 5 × 10ⁿ 上，目标 4~6 格。
        /// </summary>
        private static int NiceStep(int peak)
        {
            int rough = (int)Math.Ceiling(peak / 5.0);
            int magnitude = 1;
            while (rough > 10)
            {
                rough = (int)Math.Ceiling(rough / 10.0);
                magnitude *= 10;
            }

            int nice;
            if (rough <= 1) nice = 1;
            else if (rough <= 2) nice = 2;
            else if (rough <= 5) nice = 5;
            else nice = 10;

            return nice * magnitude;
        }
    }
}
