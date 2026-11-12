<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Adoptable.aspx.cs" Inherits="StrayCat.Web.AdoptablePage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <h2>待领养猫咪</h2>
    </div>
    <asp:Label ID="EmptyLabel" runat="server" Text="暂无待领养猫咪" Visible="false" CssClass="empty" />
    <asp:Repeater ID="ListRepeater" runat="server">
        <HeaderTemplate><div class="cat-grid"></HeaderTemplate>
        <ItemTemplate>
            <div class="cat">
                <div class="thumb"><%# PhotoMarkup(Eval("PhotoPath")) %></div>
                <div>
                    <a class="cat-name" href='<%# "AdoptableDetail.aspx?id=" + Eval("CatId") %>'><%#: Eval("TempName") %></a>
                    <div class="cat-no"><%#: Eval("CatNo") %></div>
                    <div class="cat-facts"><%#: Eval("GenderName") %> · <%#: Eval("EstimatedAge") %> · <%#: Eval("NeuteredName") %></div>
                </div>
                <div class="cat-side">
                    <span class="tag tag-<%#: Eval("Status") %>">待领养</span>
                    <div class="cat-time"><a href='<%# "AdoptableDetail.aspx?id=" + Eval("CatId") %>'>查看详情</a></div>
                </div>
            </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
    </asp:Repeater>
    <asp:Literal ID="PagerLiteral" runat="server" Mode="PassThrough" />
</asp:Content>
