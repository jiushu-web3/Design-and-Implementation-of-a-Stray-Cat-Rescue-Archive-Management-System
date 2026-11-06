<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MedicalList.aspx.cs" Inherits="StrayCat.Web.MedicalListPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>医疗记录</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:PlaceHolder ID="ListHolder" runat="server">
        <asp:HyperLink ID="AddLink" runat="server" CssClass="primary-link" Text="新增医疗" />
        <asp:Label ID="EmptyLabel" runat="server" Text="暂无记录" Visible="false" CssClass="empty" />
        <asp:Repeater ID="ListRepeater" runat="server">
            <HeaderTemplate><div class="list"></HeaderTemplate>
            <ItemTemplate>
                <div class="list-row">
                    <span><%#: Eval("MedicalDateText") %></span>
                    <span><%#: Eval("Diagnosis") %></span>
                    <span><%#: Eval("Treatment") %></span>
                    <a href='<%# "MedicalDetail.aspx?id=" + Eval("MedicalId") %>'>查看</a>
                </div>
            </ItemTemplate>
            <FooterTemplate></div></FooterTemplate>
        </asp:Repeater>
        <p><asp:HyperLink ID="BackLink" runat="server" Text="返回档案" /></p>
    </asp:PlaceHolder>
</asp:Content>
