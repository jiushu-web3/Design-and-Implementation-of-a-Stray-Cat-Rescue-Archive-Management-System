<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdoptableDetail.aspx.cs" Inherits="StrayCat.Web.AdoptableDetailPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <h2>猫咪详情</h2>
        <a class="primary-link" href="Adoptable.aspx">返回可领养猫</a>
    </div>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:PlaceHolder ID="DetailHolder" runat="server">
        <div class="split detail-split">
            <asp:Image ID="PhotoImage" runat="server" CssClass="photo" Visible="false" AlternateText="猫咪照片" />
            <div class="main-col">
                <h3><asp:Literal ID="NameLiteral" runat="server" Mode="PassThrough" /></h3>
                <p class="cat-no"><asp:Literal ID="MetaLiteral" runat="server" Mode="PassThrough" /></p>
                <div class="detail">
                    <p><span>健康状况</span><asp:Literal ID="StoryLiteral" runat="server" Mode="PassThrough" /></p>
                    <p><span>医疗记录</span><asp:Literal ID="MedicalCountLiteral" runat="server" Mode="PassThrough" /> 条</p>
                    <p><span>领养记录</span><asp:Literal ID="AdoptionCountLiteral" runat="server" Mode="PassThrough" /> 条</p>
                    <p><span>救助站联系方式</span>如需咨询请登录后提交领养申请</p>
                </div>
                <asp:HyperLink ID="ApplyLink" runat="server" CssClass="primary" Text="申请领养" />
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>
