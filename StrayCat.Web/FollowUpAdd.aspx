<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FollowUpAdd.aspx.cs" Inherits="StrayCat.Web.FollowUpAddPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>新增回访</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
    <asp:PlaceHolder ID="FormHolder" runat="server">
        <div class="form-grid" data-validate="true">
            <label for="DateBox">回访日期</label>
            <asp:TextBox ID="DateBox" runat="server" TextMode="Date" data-required="true" />
            <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="DateBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="WayList">回访方式</label>
            <asp:DropDownList ID="WayList" runat="server" data-required="true" />
            <asp:RequiredFieldValidator ID="Val2" runat="server" ControlToValidate="WayList" InitialValue="" ErrorMessage="请选择回访方式" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="ConditionBox">猫咪状况</label>
            <asp:TextBox ID="ConditionBox" runat="server" TextMode="MultiLine" MaxLength="500" data-required="true" data-max="500" />
            <asp:RequiredFieldValidator ID="Val3" runat="server" ControlToValidate="ConditionBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="RemarkBox">备注</label>
            <asp:TextBox ID="RemarkBox" runat="server" TextMode="MultiLine" MaxLength="500" data-max="500" />
            <div>
                <asp:Button ID="SaveButton" runat="server" Text="保存" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
                <asp:HyperLink ID="BackLink" runat="server" CssClass="secondary" Text="返回" />
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>
