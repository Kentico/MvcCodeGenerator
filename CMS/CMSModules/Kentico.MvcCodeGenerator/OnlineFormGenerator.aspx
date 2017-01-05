<%@ Page Title="" Language="C#" MasterPageFile="~/CMSMasterPages/UI/SimplePage.master" AutoEventWireup="true" CodeBehind="OnlineFormGenerator.aspx.cs" Inherits="Kentico.MvcCodeGenerator.Kentico.MvcCodeGenerator.OnlineFormGenerator" Theme="Default" %>

<%@ Register Src="~/CMSModules/Kentico.MvcCodeGenerator/FileSystemSelector.ascx" TagName="FileSystemSelector" TagPrefix="cms" %>

<asp:Content ID="cntBody" ContentPlaceHolderID="plcContent" runat="server">
    <div class="form-horizontal">
        <div class="form-group">
            <asp:PlaceHolder runat="server" ID="phInputControls" />
            <cms:CMSUpdatePanel ID="pnlUpdate" runat="server">
                <ContentTemplate>
                    <div class="form-group">
                        <div class="editing-form-label-cell">
                            <cms:LocalizedLabel CssClass="control-label" runat="server" ResourceString="ObjectType.Kentico.MvcCodeGenerator_RepositoryModelNamespace" DisplayColon="true" />
                        </div>
                        <div class="editing-form-value-cell">
                            <cms:CMSTextBox runat="server" ID="txtRepositoryModelNamespace" MaxLength="200" />
                            <cms:CMSRequiredFieldValidator ID="rfvRepositoryModelNamespace" runat="server" ControlToValidate="txtRepositoryModelNamespace"
                                Display="dynamic" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="editing-form-label-cell">
                            <cms:LocalizedLabel CssClass="control-label" runat="server" ResourceString="ObjectType.Kentico.MvcCodeGenerator_RepositoryModelClass" DisplayColon="true" />
                        </div>
                        <div class="editing-form-value-cell">
                            <cms:CMSTextBox runat="server" ID="txtRepositoryModelClass" MaxLength="200" />
                            <cms:CMSRequiredFieldValidator ID="rfvRepositoryModelClass" runat="server" ControlToValidate="txtRepositoryModelClass" Display="dynamic" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="editing-form-label-cell">
                            <cms:LocalizedLabel CssClass="control-label" runat="server" ResourceString="ObjectType.Kentico.MvcCodeGenerator_ActionResultMethod" DisplayColon="True" />
                        </div>
                        <div class="editing-form-value-cell">
                            <cms:CMSTextBox runat="server" ID="txtActionResultMethod" MaxLength="200" />
                            <cms:CMSRequiredFieldValidator ID="rfvActionResultMethod" runat="server" ControlToValidate="txtActionResultMethod" Display="dynamic" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="editing-form-label-cell">
                            <cms:LocalizedLabel CssClass="control-label" runat="server" ResourceString="ObjectType.Kentico.MvcCodeGenerator_ControllerClass" DisplayColon="True" />
                        </div>
                        <div class="editing-form-value-cell">
                            <cms:CMSTextBox runat="server" ID="txtControllerClass" MaxLength="200" />
                            <cms:CMSRequiredFieldValidator ID="rfvControllerClass" runat="server" ControlToValidate="txtControllerClass" Display="dynamic" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="editing-form-label-cell">
                            <cms:LocalizedLabel CssClass="control-label" runat="server" ResourceString="Classes.Code.SavePath" DisplayColon="True" AssociatedControlID="fssSavePath" />
                        </div>
                        <div class="editing-form-value-cell">
                            <cms:FileSystemSelector runat="server" ID="fssSavePath" ShowFolders="True" />
                        </div>
                    </div>
                    <%--<div class="form-group">
                        <div class="editing-form-label-cell">
                            <cms:LocalizedLabel CssClass="control-label" runat="server" ResourceString="ObjectType.Kentico.MvcCodeGenerator_MvcVersion" DisplayColon="True" AssociatedControlID="ucSavePath" />
                        </div>
                        <div class="editing-form-value-cell">
                            <cms:CMSCheckBox ID="chkMvcVersion" runat="server" AutoPostBack="false" Text="v5.1+" />
                        </div>
                    </div>--%>
                </ContentTemplate>
            </cms:CMSUpdatePanel>
        </div>
        <div class="form-group">
            <div class="editing-form-value-cell editing-form-value-cell-offset">
                <asp:PlaceHolder runat="server" ID="phButtons" />
                <cms:LocalizedButton runat="server" ID="btnGenerateCode" ButtonStyle="Primary" ResourceString="Classes.Code.GenerateCode" />
                <cms:LocalizedButton runat="server" ID="btnSaveCode" ButtonStyle="Primary" ResourceString="Classes.Code.SaveCode" />
            </div>
        </div>
    </div>
    <div class="layout-2-columns">
        <div class="col-50">
            <asp:PlaceHolder runat="server" ID="phRepositoryModel" />
            <cms:LocalizedHeading runat="server" ID="headVmCode" Level="4" EnableViewState="false" ResourceString="ObjectType.Kentico.MvcCodeGenerator_RepositoryModelCodeTextareaTitle" DisplayColon="true" />
            <cms:ExtendedTextArea ID="txaRepositoryModelCode" runat="server" ReadOnly="true" EditorMode="Advanced" Language="CSharp" Height="450px" />
        </div>
        <div class="col-50">
            <asp:PlaceHolder runat="server" ID="phPartialView" />
            <cms:LocalizedHeading runat="server" ID="headPvCode" Level="4" EnableViewState="false" ResourceString="ObjectType.Kentico.MvcCodeGenerator_PartialViewCodeTextareaTitle" DisplayColon="true" />
            <cms:ExtendedTextArea ID="txaPartialViewCode" runat="server" ReadOnly="true" EditorMode="Advanced" Language="CSharp" Height="450px" />
        </div>
    </div>
</asp:Content>
