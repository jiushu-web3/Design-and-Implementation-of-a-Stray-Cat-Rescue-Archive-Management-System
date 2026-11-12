using System;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class CatEditPage : AdminPageBase
    {
        protected Label MessageLabel;
        protected PlaceHolder FormHolder;
        protected Image PhotoImage;
        protected TextBox TempNameBox;
        protected DropDownList GenderList;
        protected TextBox AgeBox;
        protected TextBox ColorBox;
        protected DropDownList NeuteredList;
        protected FileUpload PhotoUpload;
        protected TextBox RemarkBox;
        protected Button SaveButton;
        protected HyperLink BackLink;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PageHelper.BindGenders(GenderList, false);
                PageHelper.BindNeutered(NeuteredList);
                LoadCat();
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }
            int catId = PageHelper.Positive(Request.QueryString["id"]);
            CatDetail detail = catId == 0 ? null : new CatBll().GetDetail(catId);
            if (detail == null)
            {
                ShowMissing();
                return;
            }
            string photoError;
            string uploaded = PageHelper.SavePhoto(PhotoUpload, Server, out photoError);
            if (photoError != null)
            {
                MessageLabel.Text = photoError;
                return;
            }
            CatArchive cat = new CatArchive
            {
                CatId = catId,
                TempName = TempNameBox.Text,
                Gender = PageHelper.SelectedByte(GenderList) ?? 0,
                EstimatedAge = AgeBox.Text,
                Color = ColorBox.Text,
                IsNeutered = PageHelper.SelectedByte(NeuteredList) ?? 0,
                PhotoPath = uploaded ?? detail.Cat.PhotoPath,
                Remark = RemarkBox.Text
            };
            OperationResult<int> result = new CatBll().Update(cat);
            if (!result.Success)
            {
                PageHelper.DeletePhoto(uploaded, Server);
                MessageLabel.Text = result.Message;
                return;
            }
            if (uploaded != null)
            {
                PageHelper.DeletePhoto(detail.Cat.PhotoPath, Server);
            }
            Leave("CatDetail.aspx?id=" + catId);
        }

        private void LoadCat()
        {
            int catId = PageHelper.Positive(Request.QueryString["id"]);
            CatDetail detail = catId == 0 ? null : new CatBll().GetDetail(catId);
            if (detail == null)
            {
                ShowMissing();
                return;
            }
            BackLink.NavigateUrl = "CatDetail.aspx?id=" + catId;
            TempNameBox.Text = detail.Cat.TempName;
            AgeBox.Text = detail.Cat.EstimatedAge;
            ColorBox.Text = detail.Cat.Color;
            RemarkBox.Text = detail.Cat.Remark;
            PageHelper.Select(GenderList, detail.Cat.Gender.ToString());
            PageHelper.Select(NeuteredList, detail.Cat.IsNeutered.ToString());
            PageHelper.ShowPhoto(PhotoImage, detail.Cat.PhotoPath, Server);
            if (!CatRules.CanEditArchive(detail.Cat.Status))
            {
                MessageLabel.Text = Messages.StatusNotAllowed;
                TempNameBox.ReadOnly = true;
                AgeBox.ReadOnly = true;
                ColorBox.ReadOnly = true;
                RemarkBox.ReadOnly = true;
                GenderList.Enabled = false;
                NeuteredList.Enabled = false;
                PhotoUpload.Visible = false;
                SaveButton.Visible = false;
            }
        }

        private void ShowMissing()
        {
            MessageLabel.Text = Messages.NotFound;
            FormHolder.Visible = false;
        }
    }
}
