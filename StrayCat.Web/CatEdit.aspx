<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CatEdit.aspx.cs" Inherits="StrayCat.Web.CatEditPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>修改档案</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
    <asp:PlaceHolder ID="FormHolder" runat="server">
        <div class="form-grid" data-validate="true">
            <asp:Image ID="PhotoImage" runat="server" CssClass="photo" Visible="false" AlternateText="猫咪照片" />
            <label for="TempNameBox">临时名字</label>
            <asp:TextBox ID="TempNameBox" runat="server" MaxLength="50" data-required="true" data-max="50" />
            <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="TempNameBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="GenderList">性别</label>
            <asp:DropDownList ID="GenderList" runat="server" />
            <label for="AgeBox">估计年龄</label>
            <asp:TextBox ID="AgeBox" runat="server" MaxLength="20" data-max="20" />
            <label for="ColorBox">毛色</label>
            <asp:TextBox ID="ColorBox" runat="server" MaxLength="50" data-max="50" />
            <label for="NeuteredList">绝育</label>
            <asp:DropDownList ID="NeuteredList" runat="server" />
            <label for="PhotoUpload">更换照片</label>
            <asp:FileUpload ID="PhotoUpload" runat="server" accept=".jpg,.jpeg,.png" />
            <label for="RemarkBox">档案备注</label>
            <asp:TextBox ID="RemarkBox" runat="server" TextMode="MultiLine" MaxLength="500" data-max="500" />
            <div>
                <asp:Button ID="SaveButton" runat="server" Text="保存" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
                <asp:HyperLink ID="BackLink" runat="server" CssClass="secondary" Text="返回" />
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>
