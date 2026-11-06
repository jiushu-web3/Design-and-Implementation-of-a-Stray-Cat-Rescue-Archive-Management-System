<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CatAdd.aspx.cs" Inherits="StrayCat.Web.CatAddPage" MasterPageFile="~/Site.Master" %>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h2>新增档案</h2>
    <p class="hint">档案和接收记录一次保存。</p>
    <div class="form-grid" data-validate="true">
        <asp:Label ID="MessageLabel" runat="server" CssClass="form-message" data-form-message="true" />
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
        <label for="PhotoUpload">照片</label>
        <asp:FileUpload ID="PhotoUpload" runat="server" accept=".jpg,.jpeg,.png" />
        <label for="RemarkBox">档案备注</label>
        <asp:TextBox ID="RemarkBox" runat="server" TextMode="MultiLine" MaxLength="500" data-max="500" />
        <label for="DateBox">接收日期</label>
        <asp:TextBox ID="DateBox" runat="server" TextMode="Date" data-required="true" />
        <asp:RequiredFieldValidator ID="Val2" runat="server" ControlToValidate="DateBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="TimeBox">接收时间</label>
        <asp:TextBox ID="TimeBox" runat="server" TextMode="Time" />
        <asp:RegularExpressionValidator ID="Val3" runat="server" ControlToValidate="TimeBox" ValidationExpression="^([01]\d|2[0-3]):[0-5]\d$" ErrorMessage="请填写正确的日期" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="LocationBox">接收地点</label>
        <asp:TextBox ID="LocationBox" runat="server" MaxLength="200" data-required="true" data-max="200" />
        <asp:RequiredFieldValidator ID="Val4" runat="server" ControlToValidate="LocationBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="FoundWayList">发现方式</label>
        <asp:DropDownList ID="FoundWayList" runat="server" data-required="true" />
        <asp:RequiredFieldValidator ID="Val5" runat="server" ControlToValidate="FoundWayList" InitialValue="" ErrorMessage="请选择发现方式" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="HealthBox">初步情况</label>
        <asp:TextBox ID="HealthBox" runat="server" TextMode="MultiLine" MaxLength="500" data-required="true" data-max="500" />
        <asp:RequiredFieldValidator ID="Val6" runat="server" ControlToValidate="HealthBox" ErrorMessage="请填写必填项" Display="Dynamic" CssClass="field-error" ValidationGroup="save" />
        <label for="RescueRemarkBox">接收备注</label>
        <asp:TextBox ID="RescueRemarkBox" runat="server" TextMode="MultiLine" MaxLength="500" data-max="500" />
        <div>
            <asp:Button ID="SaveButton" runat="server" Text="保存" CssClass="primary" ValidationGroup="save" OnClick="SaveButton_Click" />
            <a class="secondary" href="CatList.aspx">返回</a>
        </div>
    </div>
</asp:Content>
