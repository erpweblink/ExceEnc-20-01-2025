﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/AdminMasterPage.master" AutoEventWireup="true" CodeFile="DeliveryChallan.aspx.cs" Inherits="Admin_DeliveryChallan" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">

    <style>
        .dissablebtn {
            cursor: not-allowed;
        }
    </style>
    <style>
        .spancls {
            color: #5d5656 !important;
            font-size: 13px !important;
            font-weight: 600;
            text-align: left;
        }

        .starcls {
            color: red;
            font-size: 18px;
            font-weight: 700;
        }

        .card .card-header span {
            color: #060606;
            display: block;
            font-size: 13px;
            margin-top: 5px;
        }

        .errspan {
            float: right;
            margin-right: 6px;
            margin-top: -25px;
            position: relative;
            z-index: 2;
            color: black;
        }

        .currentlbl {
            text-align: center !important;
        }

        .completionList {
            border: solid 1px Gray;
            border-radius: 5px;
            margin: 0px;
            padding: 3px;
            height: 120px;
            overflow: auto;
            background-color: #FFFFFF;
        }

        .listItem {
            color: #191919;
        }

        .itemHighlighted {
            background-color: #ADD6FF;
        }

        .reqcls {
            color: red;
            font-weight: 600;
            font-size: 14px;
        }

        .aspNetDisabled {
            cursor: not-allowed !important;
        }

        .rwotoppadding {
            padding-top: 10px;
        }
    </style>

    <style type="text/css">
        .cal_Theme1 .ajax__calendar_container {
            background-color: #DEF1F4;
            border: solid 1px #77D5F7;
        }

        .cal_Theme1 .ajax__calendar_header {
            background-color: #ffffff;
            margin-bottom: 4px;
        }

        .cal_Theme1 .ajax__calendar_title,
        .cal_Theme1 .ajax__calendar_next,
        .cal_Theme1 .ajax__calendar_prev {
            color: #004080;
            padding-top: 3px;
        }

        .cal_Theme1 .ajax__calendar_body {
            background-color: #ffffff;
            border: solid 1px #77D5F7;
        }

        .cal_Theme1 .ajax__calendar_dayname {
            text-align: center;
            font-weight: bold;
            margin-bottom: 4px;
            margin-top: 2px;
            color: #004080;
        }

        .cal_Theme1 .ajax__calendar_day {
            color: #004080;
            text-align: center;
        }

        .cal_Theme1 .ajax__calendar_hover .ajax__calendar_day,
        .cal_Theme1 .ajax__calendar_hover .ajax__calendar_month,
        .cal_Theme1 .ajax__calendar_hover .ajax__calendar_year,
        .cal_Theme1 .ajax__calendar_active {
            color: #004080;
            font-weight: bold;
            background-color: #DEF1F4;
        }

        .cal_Theme1 .ajax__calendar_today {
            font-weight: bold;
            font-size: 10px;
        }

        .cal_Theme1 .ajax__calendar_other,
        .cal_Theme1 .ajax__calendar_hover .ajax__calendar_today,
        .cal_Theme1 .ajax__calendar_hover .ajax__calendar_title {
            color: #bbbbbb;
        }

        .ajax__calendar_body {
            height: 158px !important;
            width: 220px !important;
            position: relative;
            overflow: hidden;
            margin: 0 0 0 -5px !important;
        }

        .ajax__calendar_container {
            padding: 4px;
            cursor: default;
            width: 220px !important;
            font-size: 11px;
            text-align: center;
            font-family: tahoma,verdana,helvetica;
        }

        .cal_Theme1 .ajax__calendar_day {
            color: #004080;
            font-size: 14px;
            text-align: center;
        }

        .ajax__calendar_day {
            height: 22px !important;
            width: 27px !important;
            text-align: right;
            padding: 0 14px !important;
            cursor: pointer;
        }

        .cal_Theme1 .ajax__calendar_dayname {
            text-align: center;
            font-weight: bold;
            margin-bottom: 4px;
            margin-top: 2px;
            margin-left: 12px !important;
            color: #004080;
        }

        .ajax__calendar_year {
            height: 50px !important;
            width: 51px !important;
            font-weight: bold;
            text-align: center;
            cursor: pointer;
            overflow: hidden;
            color: #004080;
        }

        .ajax__calendar_month {
            height: 50px !important;
            width: 51px !important;
            text-align: center;
            font-weight: bold;
            cursor: pointer;
            overflow: hidden;
            color: #004080;
        }

        .grid tr:hover {
            background-color: #d4f0fa;
        }

        .pcoded[theme-layout="vertical"][vertical-nav-type="expanded"] .pcoded-header .pcoded-left-header, .pcoded[theme-layout="vertical"][vertical-nav-type="expanded"] .pcoded-navbar {
            width: 210px;
        }
    </style>


    <style>
        .row {
            margin-top: 10px;
        }
    </style>
    <script type='text/javascript'>
        function scrollToElement() {
            var target = document.getElementById("divdtls").offsetTop;
            window.scrollTo(0, target);
        }
    </script>
    <script type='text/javascript'>
        function scrollToElement() {
            var target = document.getElementById("divdtls1").offsetTop;
            window.scrollTo(0, target);
        }
    </script>
    <script>
        function isNumberKey(evt) {
            var charCode = (evt.which) ? evt.which : evt.keyCode;
            if (charCode > 31 && (charCode < 48 || charCode > 57))
                return false;
            return true;
        }
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

    <asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></asp:ToolkitScriptManager>
    <asp:UpdatePanel ID="updatepnl" runat="server">
        <ContentTemplate>
            <div class="page-wrapper">
                <div class="page-body">
                    <div class="row">
                        <div class="col-md-7">
                        </div>
                        <div class="col-md-5">
                            <div class="page-header-breadcrumb">
                                <div style="float: right; margin: 3px; margin-bottom: 5px;">
                                    <span><a href="DeliveryChallanList.aspx" style="font-size: 16px; border: 1px dashed gray; padding: 4px;">&nbsp;Delivery Challan List</a>&nbsp;&nbsp;
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="container py-3">
                        <div class="card">
                            <div class="card-header bg-primary text-uppercase text-white">
                                <h5>Create Delivery Challan</h5>
                            </div>
                            <div class="row">
                                <div class="col-xl-12 col-md-12">
                                    <div class="card-header">
                                        <div class="row">
                                            <div class="col-md-12">
                                                <asp:HiddenField ID="hiddeninvoiceno" runat="server" />
                                                <asp:HiddenField ID="hidden1" runat="server" />
                                                <div class="row">
                                                    <div class="col-md-2 spancls">Customer Name<i class="reqcls">*&nbsp;</i> : </div>
                                                    <div class="col-md-4">
                                                        <asp:TextBox ID="txtCustomerNAme" CssClass="form-control" runat="server" Width="100%" OnTextChanged="txtCustomerNAme_TextChanged" AutoPostBack="true"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" Display="Dynamic" ErrorMessage="Please Enter Customer Name"
                                                            ControlToValidate="txtCustomerNAme" ValidationGroup="form1" ForeColor="Red" SetFocusOnError="true"></asp:RequiredFieldValidator>
                                                        <asp:AutoCompleteExtender ID="AutoCompleteExtender1" runat="server" CompletionListCssClass="completionList"
                                                            CompletionListHighlightedItemCssClass="itemHighlighted" CompletionListItemCssClass="listItem"
                                                            CompletionInterval="10" MinimumPrefixLength="1" ServiceMethod="GetCustomerList"
                                                            TargetControlID="txtCustomerNAme">
                                                        </asp:AutoCompleteExtender>
                                                    </div>
                                                    <div class="col-md-2 spancls">Shipping Customer<i class="reqcls">&nbsp;</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:TextBox ID="txtshippingcustomer" CssClass="form-control" runat="server" Width="100%"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-md-2 spancls">Shipping Address<i class="reqcls">&nbsp;</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:TextBox ID="txtshippingAddress" CssClass="form-control" runat="server" Width="100%" TextMode="MultiLine"></asp:TextBox>
                                                    </div>
                                                    <div class="col-md-2 spancls">Challan Date<i class="reqcls">&nbsp;*</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:TextBox ID="txtchallandate" CssClass="form-control" runat="server" Width="100%" AutoComplete="off"></asp:TextBox>
                                                        <asp:CalendarExtender ID="CalendarExtender1" TargetControlID="txtchallandate" Format="dd-MM-yyyy" CssClass="cal_Theme1" runat="server"></asp:CalendarExtender>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" Display="Dynamic" ErrorMessage="Please Enter Challan Date"
                                                            ControlToValidate="txtchallandate" ValidationGroup="form1" ForeColor="Red" SetFocusOnError="true"></asp:RequiredFieldValidator>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-md-2 spancls">Challan Against<i class="reqcls">&nbsp;</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:DropDownList runat="server" ID="txtchallanagainst" CssClass="form-control" OnTextChanged="txtchallanagainst_TextChanged" AutoPostBack="true">
                                                            <asp:ListItem Value="" Text="--Select--"></asp:ListItem>
                                                            <asp:ListItem Text="Direct"></asp:ListItem>
                                                            <asp:ListItem Text="Order"></asp:ListItem>
                                                        </asp:DropDownList>
                                                        <%-- <asp:TextBox ID="txtinvoiceagainst" CssClass="form-control" runat="server" Width="100%"></asp:TextBox>--%>
                                                    </div>
                                                    <div class="col-md-2 spancls">Against Number<i class="reqcls">&nbsp;</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:DropDownList runat="server" ID="txtagainstNumber" AppendDataBoundItems="true" CssClass="form-control" OnTextChanged="txtagainstNumber_TextChanged" AutoPostBack="true">
                                                            <asp:ListItem Value="" Text="--Select--"></asp:ListItem>
                                                        </asp:DropDownList>
                                                        <%--                                                        <asp:TextBox ID="txtagainstNumber" CssClass="form-control" runat="server" Width="100%"></asp:TextBox>--%>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-md-2 spancls">Customer PO No<i class="reqcls">&nbsp;*</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:TextBox ID="txtcustomerPoNo" CssClass="form-control" runat="server" Width="100%"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" Display="Dynamic" ErrorMessage="Please Enter Customer PO NUmber"
                                                            ControlToValidate="txtcustomerPoNo" ValidationGroup="form1" ForeColor="Red" SetFocusOnError="true"></asp:RequiredFieldValidator>

                                                    </div>
                                                    <div class="col-md-2 spancls">PO Date<i class="reqcls">&nbsp;*</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:HiddenField runat="server" ID="hdnfileData" />
                                                        <asp:HiddenField runat="server" ID="hdnGrandtotal" />
                                                        <asp:TextBox ID="txtpodate" CssClass="form-control" runat="server" Width="100%" AutoComplete="off"></asp:TextBox>
                                                        <asp:CalendarExtender ID="CalendarExtender2" TargetControlID="txtpodate" Format="dd-MM-yyyy" CssClass="cal_Theme1" runat="server"></asp:CalendarExtender>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" Display="Dynamic" ErrorMessage="Please Enter Date"
                                                            ControlToValidate="txtpodate" ValidationGroup="form1" ForeColor="Red" SetFocusOnError="true"></asp:RequiredFieldValidator>
                                                    </div>
                                                </div>

                                                <div class="row">
                                                    <div class="col-md-2 spancls">Customer Challan No<i class="reqcls">&nbsp;</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:TextBox ID="txtCustchallanNo" CssClass="form-control" runat="server" Width="100%"></asp:TextBox>
                                                        <%-- <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" Display="Dynamic" ErrorMessage="Please Enter Challan NUmber"
                                                            ControlToValidate="txtchallanNo" ValidationGroup="form1" ForeColor="Red" SetFocusOnError="true"></asp:RequiredFieldValidator>--%>
                                                    </div>
                                                    <div class="col-md-2 spancls">Customer Challan Date<i class="reqcls">&nbsp;</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:TextBox ID="txtCustchallanDate" CssClass="form-control" runat="server" Width="100%" AutoComplete="off"></asp:TextBox>
                                                        <asp:CalendarExtender ID="CalendarExtender3" TargetControlID="txtCustchallanDate" Format="dd-MM-yyyy" CssClass="cal_Theme1" runat="server"></asp:CalendarExtender>
                                                        <%--          <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" Display="Dynamic" ErrorMessage="Please Enter Challan Date"
                                                            ControlToValidate="txtchallanDate" ValidationGroup="form1" ForeColor="Red" SetFocusOnError="true"></asp:RequiredFieldValidator>--%>
                                                    </div>
                                                </div>

                                                <div class="row">
                                                    <div class="col-md-2 spancls">Transport Mode<i class="reqcls">&nbsp;</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:DropDownList ID="txttransportMode" CssClass="form-control" runat="server" Width="100%">
                                                            <asp:ListItem Value="0">--Select--</asp:ListItem>
                                                            <asp:ListItem>By Road</asp:ListItem>
                                                            <asp:ListItem>By Air</asp:ListItem>
                                                            <asp:ListItem>By Courier</asp:ListItem>
                                                            <asp:ListItem>By Hand</asp:ListItem>
                                                        </asp:DropDownList>
                                                    </div>
                                                    <div class="col-md-2 spancls">Vehical Number<i class="reqcls">&nbsp;</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:TextBox ID="txtvehicalNumber" CssClass="form-control" runat="server" Width="100%"></asp:TextBox>
                                                    </div>
                                                </div>
                                                <div class="row">

                                                    <div class="col-md-2 spancls">Remark<i class="reqcls">&nbsp;</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:TextBox ID="txtremark" CssClass="form-control" runat="server" Width="100%" TextMode="MultiLine"></asp:TextBox>
                                                    </div>
                                                    <%--    <div class="col-md-2 spancls">E-Bill Number<i class="reqcls">&nbsp;</i> :</div>
                                                    <div class="col-md-4">
                                                        <asp:TextBox ID="txtebillnumber" CssClass="form-control" runat="server" Width="100%"></asp:TextBox>
                                                    </div>--%>
                                                </div>

                                                <br />
                                                <div class="table-responsive" id="manuallytable" runat="server">
                                                    <table class="table" border="1" style="width: 100%; border: 1px solid #0c7d38;">
                                                        <tr style="background-color: #7ad2d4; color: #000; font-weight: 600; text-align: center;">
                                                            <td style="width: 50%;">Particulars</td>
                                                            <td>HSN</td>
                                                            <td>Qty</td>
                                                            <td>UOM</td>
                                                            <td>Rate</td>
                                                            <td style="width: 50%;">Description</td>
                                                            <td style="width: 10%">Action</td>
                                                        </tr>
                                                        <tr>
                                                            <td>
                                                                <%--<asp:TextBox ID="txtParticulars" Width="200px" runat="server" AutoPostBack="true" OnTextChanged="txtParticulars_TextChanged"></asp:TextBox>--%>

                                                                <asp:DropDownList runat="server" ID="txtParticulars" AutoPostBack="true" OnTextChanged="txtParticulars_TextChanged">
                                                                    <asp:ListItem>--Select--</asp:ListItem>
                                                                    <asp:ListItem>Enclosures For Control Panel</asp:ListItem>
                                                                    <asp:ListItem>Part of Enclosure for Control Panel</asp:ListItem>
                                                                </asp:DropDownList>
                                                            </td>

                                                            <td>
                                                                <asp:TextBox ID="txtHSN" Width="100px" runat="server" ReadOnly="true"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtQty" Width="50px" runat="server" AutoPostBack="true" OnTextChanged="txtQty_TextChanged" onkeypress="return isNumberKey(event)"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtuom" Width="100px" runat="server" Text="Nos"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtRate" Width="100px" runat="server" onkeypress="return isNumberKey(event)"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtdiscription" Width="200px" runat="server" TextMode="MultiLine"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:Button ID="btnAddMore" CssClass="btn btn-success btn-sm btncss" runat="server" Text="+ Add" OnClick="btnAddMore_Click" />
                                                            </td>
                                                        </tr>

                                                    </table>
                                                </div>

                                                <div class="row" id="divdtls">
                                                    <div class="table-responsive">
                                                        <asp:GridView ID="gvinvoiceParticular" runat="server" CssClass="table" HeaderStyle-BackColor="#009999" AutoGenerateColumns="false"
                                                            EmptyDataText="No records has been added." ShowFooter="false" OnRowEditing="gvinvoiceParticular_RowEditing" DataKeyNames="Id">
                                                            <Columns>
                                                                <asp:TemplateField HeaderText="Sr.No" ItemStyle-Width="20" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="lblsno" runat="server" Text='<%# Container.DataItemIndex+1 %>'></asp:Label>
                                                                        <asp:Label ID="lblid" runat="Server" Text='<%# Eval("Id") %>' Visible="false" />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Particulars" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="lblParticulars" runat="Server" Text='<%# Eval("Particular") %>' />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="HSN" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="lblHSN" runat="Server" Text='<%# Eval("HSN") %>' />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Qty" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center">
                                                                    <EditItemTemplate>
                                                                        <asp:TextBox Text='<%# Eval("Qty") %>' Width="50px" ID="txtQty" runat="server" AutoPostBack="true" OnTextChanged="txtQty_TextChanged1" onkeypress="return isNumberKey(event)"></asp:TextBox>
                                                                    </EditItemTemplate>
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="lblQty" runat="Server" Text='<%# Eval("Qty") %>' />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="UOM" ItemStyle-Width="20" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:Label runat="server" Width="50" ID="txtUOM" Text='<%# Eval("UOM") %>'></asp:Label>
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Rate" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center">

                                                                    <ItemTemplate>
                                                                        <asp:Label ID="lblRate" runat="Server" Text='<%# Eval("Rate") %>' />
                                                                    </ItemTemplate>
                                                                    <EditItemTemplate>
                                                                        <asp:TextBox Text='<%# Eval("Rate") %>' Width="50px" ID="txtrate" runat="server" AutoPostBack="true" OnTextChanged="txtrate_TextChanged" onkeypress="return isNumberKey(event)"></asp:TextBox>
                                                                    </EditItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Description" ItemStyle-Width="20" ItemStyle-HorizontalAlign="Center">
                                                                    <EditItemTemplate>
                                                                        <asp:TextBox ID="txtDescription" TextMode="MultiLine" runat="server" Text='<%# Eval("Description") %>'></asp:TextBox>
                                                                    </EditItemTemplate>
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="txtDescription" TextMode="MultiLine" runat="server" Text='<%# Eval("Description") %>'></asp:Label>
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField ItemStyle-Width="120">
                                                                    <EditItemTemplate>
                                                                        <asp:LinkButton Text="Update" ID="lnkbtnUpdate" ClientIDMode="Static" runat="server" ToolTip="Update" OnClick="lnkbtnUpdate_Click"><i class="fa fa-refresh" style="font-size:28px;color:green;"></i></asp:LinkButton>
                                                                        |
                                                                            <asp:LinkButton Text="Cancel" ID="lnkCancel" runat="server" OnClick="lnkCancel_Click" ToolTip="Cancel"><i class="fa fa-close" style="font-size:28px;color:red;"></i></asp:LinkButton>
                                                                    </EditItemTemplate>
                                                                    <ItemTemplate>
                                                                        <asp:LinkButton Text="Edit" runat="server" CommandName="Edit" ToolTip="Edit"><i class="fa fa-edit" style="font-size:28px;color:blue;"></i></asp:LinkButton>
                                                                        | 
                                                                    <asp:LinkButton ID="lnkDelete" runat="server" CommandArgument='<%# Eval("id") %>' ToolTip="Delete" OnClick="lnkDelete_Click"><i class="fa fa-trash-o" style="font-size:28px;color:red"></i></asp:LinkButton>
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                            </Columns>
                                                        </asp:GridView>

                                                    </div>
                                                </div>

                                                <div class="row" id="divdtls1" runat="server">
                                                    <div class="table-responsive">

                                                        <asp:GridView ID="gvorder" runat="server" CssClass="table" HeaderStyle-BackColor="#009999" AutoGenerateColumns="false"
                                                            EmptyDataText="No records has been added." ShowFooter="false" DataKeyNames="Id" OnRowDataBound="gvorder_RowDataBound" OnRowEditing="gvorder_RowEditing">
                                                            <Columns>
                                                                <asp:TemplateField HeaderText="Sr.No" ItemStyle-Width="20" ItemStyle-HorizontalAlign="Center">
                                                                    <HeaderTemplate>
                                                                        <asp:CheckBox ID="chkHeader" runat="server" OnCheckedChanged="chkHeader_CheckedChanged" AutoPostBack="true" />
                                                                    </HeaderTemplate>
                                                                    <ItemTemplate>
                                                                        <%--<asp:Label ID="lblsno" runat="server" Text='<%# Container.DataItemIndex+1 %>'></asp:Label>--%>
                                                                        <asp:Label ID="lblid" Width="100px" runat="Server" Text='<%# Eval("id") %>' Visible="false" />
                                                                        <asp:CheckBox ID="chkRow" runat="server" OnCheckedChanged="chkRow_CheckedChanged" AutoPostBack="true" />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Particulars" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:TextBox ID="lblParticulars" runat="Server" TextMode="MultiLine" Text="Enclosures For Control Panels" ReadOnly="true" Enabled="false" />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="HSN" ItemStyle-Width="80" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:TextBox ID="lblHSN" Width="100px" runat="Server" Enabled="false" Text='<%# Eval("HSN") %>' />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Qty" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:TextBox ID="lblQty" Width="50px" runat="Server" Text='<%# Eval("Qty") %>' OnTextChanged="lblQty_TextChanged" AutoPostBack="false" Enabled="false" />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="UOM" ItemStyle-Width="20" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:TextBox runat="server" Width="50" ID="txtUOM" Text="Nos" Enabled="false"></asp:TextBox>
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Rate" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:TextBox ID="lblRate" Width="100px" runat="Server" Text='<%# Eval("Price") %>' Enabled="false" OnTextChanged="lblRate_TextChanged" AutoPostBack="true" ReadOnly="true" />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Amount" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="lblAmount" Width="100%" runat="Server" Enabled="false" />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="CGSTPer" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:TextBox ID="lblCGSTPer" Width="50px" runat="Server" Text='<%# Eval("CGST") %>' OnTextChanged="lblCGSTPer_TextChanged" AutoPostBack="true" ReadOnly="true" Enabled="false" />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="CGSAmt" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="lblCGSTAmt" Width="100%" runat="Server" />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="SGSTPer" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:TextBox ID="lblSGSTPer" Width="50px" runat="Server" Text='<%# Eval("SGST") %>' OnTextChanged="lblSGSTPer_TextChanged" AutoPostBack="true" ReadOnly="true" Enabled="false" />

                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="SGSAmt" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="lblSGSTAmt" Width="100%" runat="Server" />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="IGSTPer" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:TextBox ID="lblIGSTPer" Width="50px" runat="Server" Text='<%# Eval("IGST") %>' OnTextChanged="lblIGSTPer_TextChanged" AutoPostBack="true" ReadOnly="true" Enabled="false" />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="IGSAmt" ItemStyle-Width="120" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="lblIGSTAmt" Width="100%" runat="Server" />
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Dis(%)" ItemStyle-Width="20" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:TextBox runat="server" Width="50px" ID="txtdiscount" Text='<%# Eval("Discount") %>' AutoPostBack="true" OnTextChanged="txtdiscount_TextChanged1" Enabled="false"></asp:TextBox>
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Grand Total" ItemStyle-Width="20" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:Label runat="server" Width="100px" ID="txtGrandTotal" ReadOnly="true" Text='<%# Eval("TotalAmount") %>'></asp:Label>
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Description" ItemStyle-Width="20" ItemStyle-HorizontalAlign="Center">
                                                                    <ItemTemplate>
                                                                        <asp:TextBox ID="txtDescription" Enabled="false" TextMode="MultiLine" runat="server" Text='<%# Eval("Description").ToString().Replace("<br>","") %>'></asp:TextBox>
                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                            </Columns>
                                                        </asp:GridView>

                                                    </div>
                                                </div>
                                                <br />
                                                <div class="col-md-12" style="display: none">
                                                    <div class="col-md-2"></div>
                                                    <center>
                                                            <div class="col-md-8">
                                                                <div class="col-md-4"><b>Sum of Product Amount :</b></div>
                                                                <div class="col-md-4">
                                                                    <asp:TextBox ID="sumofAmount" CssClass="form-control" runat="server" Width="100%" ReadOnly="true" Text="0"></asp:TextBox>
                                                                </div>
                                                            </div>
                                                                </center>
                                                    <div class="col-md-2"></div>
                                                </div>


                                                <div class="table-responsive" style="display: none">
                                                    <table class="table" border="1" style="width: 100%; border: 1px solid #0c7d38;">
                                                        <tr style="background-color: #7ad2d4; color: #000; font-weight: 600; text-align: center;">
                                                            <td style="width: 50%;">Charges Description</td>
                                                            <td>HSN</td>
                                                            <td>Rate(%)</td>
                                                            <td>Basic</td>
                                                            <td>CGST</td>
                                                            <td>SGST</td>
                                                            <td>IGST</td>
                                                            <td>Cost</td>
                                                        </tr>
                                                        <tr>
                                                            <td>
                                                                <asp:TextBox ID="txtDescription" Width="250px" runat="server" TextMode="MultiLine" Text="Freight"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txthsntcs" Width="100px" runat="server"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtrateTcs" Width="100px" runat="server" Text="0" AutoPostBack="true" OnTextChanged="txtrateTcs_TextChanged"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtBasic" Width="100px" runat="server" Text="0" Enabled="true" OnTextChanged="txtBasic_TextChanged" AutoPostBack="true"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="CGSTPertcs" Width="50px" runat="server" Text="0" AutoPostBack="true" OnTextChanged="CGSTPertcs_TextChanged"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="SGSTPertcs" Width="50px" runat="server" Text="0" AutoPostBack="true" OnTextChanged="SGSTPertcs_TextChanged"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="IGSTPertcs" Width="50px" runat="server" Text="0" AutoPostBack="true" OnTextChanged="IGSTPertcs_TextChanged"></asp:TextBox>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtCost" Width="100px" runat="server" Enabled="false" Text="0"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </div>

                                                <div class="col-md-12" style="display: none">
                                                    <center>
                                                            <div class="col-md-4">
                                                                <div class="row">
                                                                    <div class="col-md-4">  TCS (%)<asp:DropDownList runat="server" ID="txtTCSPer" CssClass="form-control" placeholder="TCS (%)"  AutoPostBack="true" OnTextChanged="txtTCSPer_TextChanged">
                                                                        <asp:ListItem Value="">Select</asp:ListItem>
                                                                        <asp:ListItem>0.5</asp:ListItem>
                                                                         <asp:ListItem>1</asp:ListItem>
                                                                         <asp:ListItem>1.5</asp:ListItem>
                                                                         <asp:ListItem>2</asp:ListItem>
                                                                         </asp:DropDownList></div>
                                                                    <div class="col-md-8">TCS Amt<asp:TextBox ID="txtTCSAmt" CssClass="form-control" runat="server" Width="100%" ReadOnly="true" Text="0" placeholder="TCS amount"></asp:TextBox></div>
                                                                </div>
                                                            </div>
                                                                   </center>
                                                    <center>
                                                            <div class="col-md-8">
                                                                <div class="col-md-4"><b>Grand Total :</b></div>
                                                                <div class="col-md-4">
                                                                    <asp:TextBox ID="txtGrandTot" CssClass="form-control" runat="server" Width="100%" ReadOnly="true"></asp:TextBox>
                                                                </div>
                                                            </div>
                                                                </center>
                                                </div>

                                                <div class="row" style="display: none">
                                                    <div class="col-md-5"></div>
                                                    <div class="col-md-2">
                                                        &nbsp;&nbsp; 
                                                        <asp:CheckBox runat="server" ID="IsSedndMail" />
                                                        &nbsp;&nbsp;Email
                                                    </div>
                                                    <div class="col-md-5"></div>
                                                </div>

                                                <div class="row">
                                                    <div class="col-md-2" style="margin-left: 18%;"></div>

                                                    <div class="col-md-2">
                                                        <asp:Button ID="btnSubmit" runat="server" ValidationGroup="form1" CssClass="btn btn-primary" Width="100%" Text="Submit" OnClick="btnSubmit_Click" />
                                                    </div>
                                                    <div class="col-md-2">
                                                        <asp:Button ID="btnreset" runat="server" CssClass="btn btn-danger" Width="100%" Text="Reset" />
                                                    </div>

                                                    <div class="col-md-6"></div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnSubmit" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnreset" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>

</asp:Content>

