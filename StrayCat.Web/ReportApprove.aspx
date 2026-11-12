<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportApprove.aspx.cs" Inherits="StrayCat.Web.ReportApprovePage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>采纳上报并建档</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
    <asp:PlaceHolder ID="DetailHolder" runat="server">
        <div class="split detail-split">
            <asp:Image ID="PhotoImage" runat="server" CssClass="photo" Visible="false" AlternateText="上报照片" />
            <div class="main-col">
                <div class="detail">
                    <asp:Literal ID="InfoLiteral" runat="server" Mode="PassThrough" />
                </div>
            </div>
        </div>
        <h3>档案信息</h3>
        <p class="hint">补全档案后提交，上报人上报的照片会作为默认照片。</p>
        <div class="form-grid" data-validate="true">
            <label for="NameBox">临时名字</label>
            <asp:TextBox ID="NameBox" runat="server" MaxLength="50" data-required="true" data-max="50" />
            <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="NameBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="GenderList">性别</label>
            <asp:DropDownList ID="GenderList" runat="server" />
            <label for="AgeBox">估计年龄</label>
            <asp:TextBox ID="AgeBox" runat="server" MaxLength="20" data-max="20" />
            <label for="ColorBox">毛色</label>
            <asp:TextBox ID="ColorBox" runat="server" MaxLength="50" data-max="50" />
            <label for="NeuteredList">绝育</label>
            <asp:DropDownList ID="NeuteredList" runat="server" />
            <label for="PhotoFile">档案照片</label>
            <asp:FileUpload ID="PhotoFile" runat="server" accept=".jpg,.jpeg,.png" />
            <label for="DateBox">接收日期</label>
            <asp:TextBox ID="DateBox" runat="server" TextMode="Date" data-required="true" />
            <asp:RequiredFieldValidator ID="Val2" runat="server" ControlToValidate="DateBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="HealthBox">初步情况</label>
            <asp:TextBox ID="HealthBox" runat="server" TextMode="MultiLine" MaxLength="500" data-required="true" data-max="500" />
            <asp:RequiredFieldValidator ID="Val3" runat="server" ControlToValidate="HealthBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <div>
                <asp:Button ID="SaveButton" runat="server" Text="采纳并建档" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
                <a class="secondary" href="ReportList.aspx">返回</a>
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>
