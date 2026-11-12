<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MyCenter.aspx.cs" Inherits="StrayCat.Web.MyCenterPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <h2>我的中心</h2>
        <a class="primary-link" href="ReportAdd.aspx">上报流浪猫</a>
    </div>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message is-success" Visible="false" />
    <h3>我的上报</h3>
    <asp:Label ID="EmptyReportLabel" runat="server" Text="暂无上报记录" Visible="false" CssClass="empty" />
    <asp:Repeater ID="ReportRepeater" runat="server">
        <HeaderTemplate>
            <div class="list">
                <div class="list-row head">
                    <span class="col-name">发现地点</span>
                    <span class="col-time">发现日期</span>
                    <span class="col-status">状态</span>
                    <span>驳回理由</span>
                </div>
        </HeaderTemplate>
        <ItemTemplate>
            <div class="list-row">
                <span class="col-name"><%#: Eval("FoundPlace") %></span>
                <span class="col-time"><%# DateText(Eval("FoundDate")) %></span>
                <span class="col-status"><span class='tag <%# StatusTag(Eval("Status")) %>'><%#: ReportStatusName(Eval("Status")) %></span></span>
                <span><%#: ReasonText(Eval("RejectReason")) %></span>
            </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
    </asp:Repeater>
    <asp:Literal ID="ReportPagerLiteral" runat="server" Mode="PassThrough" />
    <h3>我的申请</h3>
    <asp:Label ID="EmptyApplicationLabel" runat="server" Text="暂无领养申请" Visible="false" CssClass="empty" />
    <asp:Repeater ID="ApplicationRepeater" runat="server">
        <HeaderTemplate>
            <div class="list">
                <div class="list-row head">
                    <span class="col-name">猫咪</span>
                    <span class="col-time">申请时间</span>
                    <span class="col-status">状态</span>
                    <span>驳回理由</span>
                </div>
        </HeaderTemplate>
        <ItemTemplate>
            <div class="list-row">
                <span class="col-name"><%#: Eval("CatNo") %> · <%#: Eval("TempName") %></span>
                <span class="col-time"><%# TimeText(Eval("CreateTime")) %></span>
                <span class="col-status"><span class='tag <%# ApplicationStatusTag(Eval("Status")) %>'><%#: ApplicationStatusName(Eval("Status")) %></span></span>
                <span><%#: ReasonText(Eval("RejectReason")) %></span>
            </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
    </asp:Repeater>
    <asp:Literal ID="ApplicationPagerLiteral" runat="server" Mode="PassThrough" />
</asp:Content>
