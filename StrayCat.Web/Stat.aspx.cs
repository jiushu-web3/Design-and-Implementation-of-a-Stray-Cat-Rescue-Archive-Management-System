using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using StrayCat.BLL;

namespace StrayCat.Web
{
    public partial class StatPage : AdminPageBase
    {
        protected DropDownList YearList;
        protected Repeater MonthRepeater;

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

        private void BindMonths()
        {
            int year;
            if (!int.TryParse(YearList.SelectedValue, out year))
            {
                year = DateTime.Today.Year;
            }
            MonthRepeater.DataSource = new StatBll().Monthly(year);
            MonthRepeater.DataBind();
        }
    }
}
