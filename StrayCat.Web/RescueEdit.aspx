<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RescueEdit.aspx.cs" Inherits="StrayCat.Web.RescueEditPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>修改接收记录</h2>
    <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
    <asp:PlaceHolder ID="FormHolder" runat="server">
        <div class="form-grid" data-validate="true">
            <label for="DateBox">接收日期</label>
            <asp:TextBox ID="DateBox" runat="server" TextMode="Date" data-required="true" />
            <asp:RequiredFieldValidator ID="Val1" runat="server" ControlToValidate="DateBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="TimeBox">接收时间</label>
            <asp:TextBox ID="TimeBox" runat="server" TextMode="Time" />
            <asp:RegularExpressionValidator ID="Val2" runat="server" ControlToValidate="TimeBox" ValidationExpression="^([01]\d|2[0-3]):[0-5]\d$" ErrorMessage="请填写正确的日期" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="LocationBox">接收地点</label>
            <asp:TextBox ID="LocationBox" runat="server" MaxLength="200" data-required="true" data-max="200" />
            <asp:RequiredFieldValidator ID="Val3" runat="server" ControlToValidate="LocationBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="FoundWayList">发现方式</label>
            <asp:DropDownList ID="FoundWayList" runat="server" data-required="true" />
            <asp:RequiredFieldValidator ID="Val4" runat="server" ControlToValidate="FoundWayList" InitialValue="" ErrorMessage="请选择发现方式" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="HealthBox">初步情况</label>
            <asp:TextBox ID="HealthBox" runat="server" TextMode="MultiLine" MaxLength="500" data-required="true" data-max="500" />
            <asp:RequiredFieldValidator ID="Val5" runat="server" ControlToValidate="HealthBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
            <label for="RemarkBox">备注</label>
            <asp:TextBox ID="RemarkBox" runat="server" TextMode="MultiLine" MaxLength="500" data-max="500" />
            <div>
                <asp:Button ID="SaveButton" runat="server" Text="保存" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
                <asp:HyperLink ID="BackLink" runat="server" CssClass="secondary" Text="返回" />
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>
