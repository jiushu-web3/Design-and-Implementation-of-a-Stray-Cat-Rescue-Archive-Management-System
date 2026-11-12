<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ReportList.aspx.cs" Inherits="StrayCat.Web.ReportListPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>上报审核</h2>
    <div class="filters">
        <div>
            <label for="StatusList">状态</label>
            <asp:DropDownList ID="StatusList" runat="server" />
        </div>
        <div>
            <label for="KeywordBox">地点或上报人</label>
            <asp:TextBox ID="KeywordBox" runat="server" MaxLength="50" />
        </div>
        <asp:Button ID="SearchButton" runat="server" Text="查询" CssClass="primary" CausesValidation="false" OnClick="SearchButton_Click" />
    </div>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" />
    <asp:Label ID="EmptyLabel" runat="server" Text="暂无记录" Visible="false" CssClass="empty" />
    <asp:Repeater ID="ReportRepeater" runat="server" OnItemCommand="ReportRepeater_ItemCommand">
        <HeaderTemplate>
            <div class="list">
                <div class="list-row head">
                    <span class="col-no">编号</span>
                    <span class="col-name">发现地点</span>
                    <span class="col-gender">上报人</span>
                    <span class="col-time">发现日期</span>
                    <span class="col-status">状态</span>
                    <span>操作</span>
                </div>
        </HeaderTemplate>
        <ItemTemplate>
            <div class="list-row">
                <span class="col-no"><%#: Eval("ReportId") %></span>
                <span class="col-name"><%#: Eval("FoundPlace") %></span>
                <span class="col-gender"><%#: ReporterText(Eval("RealName"), Eval("UserName")) %></span>
                <span class="col-time"><%#: DateText(Eval("FoundDate")) %></span>
                <span class="col-status"><span class='tag <%# StatusTag(Eval("Status")) %>'><%#: ReportStatusName(Eval("Status")) %></span></span>
                <span>
                    <asp:HyperLink ID="ApproveLink" runat="server" NavigateUrl='<%# "ReportApprove.aspx?id=" + Eval("ReportId") %>' Visible='<%# IsPending(Eval("Status")) %>' Text="采纳" />
                    <asp:LinkButton ID="RejectButton" runat="server" CommandName="reject" CommandArgument='<%# Eval("ReportId") %>' Visible='<%# IsPending(Eval("Status")) %>' Text="驳回" CausesValidation="false" />
                </span>
            </div>
        </ItemTemplate>
        <FooterTemplate></div></FooterTemplate>
    </asp:Repeater>
    <asp:Literal ID="PagerLiteral" runat="server" Mode="PassThrough" />
    <asp:Panel ID="RejectPanel" runat="server" Visible="false" CssClass="form-grid">
        <asp:HiddenField ID="RejectId" runat="server" />
        <p class="hint">填写驳回理由后确认，理由会显示在用户的上报记录中。</p>
        <label for="ReasonBox">驳回理由</label>
        <asp:TextBox ID="ReasonBox" runat="server" MaxLength="200" data-max="200" />
        <div>
            <asp:Button ID="ConfirmRejectButton" runat="server" Text="确认驳回" CssClass="primary" CausesValidation="false" OnClick="ConfirmRejectButton_Click" />
            <asp:Button ID="CancelRejectButton" runat="server" Text="取消" CssClass="secondary" CausesValidation="false" OnClick="CancelRejectButton_Click" />
        </div>
    </asp:Panel>
</asp:Content>
