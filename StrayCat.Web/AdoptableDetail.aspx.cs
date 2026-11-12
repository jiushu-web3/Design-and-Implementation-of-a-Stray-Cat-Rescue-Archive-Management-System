using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class AdoptableDetailPage : Page
    {
        protected Label MessageLabel;
        protected PlaceHolder DetailHolder;
        protected Image PhotoImage;
        protected Literal NameLiteral;
        protected Literal MetaLiteral;
        protected Literal StoryLiteral;
        protected Literal MedicalCountLiteral;
        protected Literal AdoptionCountLiteral;
        protected HyperLink ApplyLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDetail();
            }
        }

        private void LoadDetail()
        {
            int catId = PageHelper.Positive(Request.QueryString["id"]);
            CatDetail detail = catId == 0 ? null : new CatBll().GetPublicDetail(catId);
            if (detail == null)
            {
                MessageLabel.Text = Messages.NotFound;
                ApplyLink.Visible = false;
                DetailHolder.Visible = false;
                return;
            }
            CatArchive cat = detail.Cat;
            PageHelper.ShowPhoto(PhotoImage, cat.PhotoPath, Server);
            NameLiteral.Text = PageHelper.Html(cat.TempName);
            MetaLiteral.Text = string.Format("{0} · {1} · {2} · {3}",
                PageHelper.Html(cat.CatNo),
                Lookups.GenderName(cat.Gender),
                PageHelper.Html(cat.EstimatedAge),
                Lookups.NeuteredName(cat.IsNeutered));
            StoryLiteral.Text = PageHelper.Html(detail.Rescue.InitialHealth);
            MedicalCountLiteral.Text = detail.MedicalCount.ToString();
            AdoptionCountLiteral.Text = detail.AdoptionCount.ToString();
            ApplyLink.NavigateUrl = "ApplyAdoption.aspx?catId=" + catId;
        }
    }
}
