﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/AdminMasterPage.master" EnableEventValidation="false" AutoEventWireup="true" CodeFile="OutstandingReport.aspx.cs" Inherits="Admin_OutstandingReport" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="asp" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
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

        <style type="text/css" >
        .divgrid {
            height: 200px;
            width: 370px;
        }

        .divgrid table {
            width: 350px;
        }

            .divgrid table th {
                background-color: Green;
                color: #fff;
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
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
    <script type="text/javascript">
        $("[src*=plus]").live("click", function () {
            $(this).closest("tr").after("<tr><td></td><td colspan = '999'>" + $(this).next().html() + "</td></tr>")
            $(this).attr("src", "../img/minus.png");
        });
        $("[src*=minus]").live("click", function () {
            $(this).attr("src", "../img/plus.png");
            $(this).closest("tr").next().remove();
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <asp:ToolkitScriptManager ID="ToolkitScriptManager1" runat="server"></asp:ToolkitScriptManager>

    <%--<asp:UpdatePanel ID="updatepnl" runat="server">
        <ContentTemplate>--%>
    <div class="page-wrapper">
        <div class="page-body">
            <div class="container py-3">
                <div class="row">
                    <div class="col-md-7">
                    </div>
                    <div class="col-md-12">
                        <div class="page-header-breadcrumb">
                            <div style="float: right; margin: 3px; margin-bottom: 5px;">
                                <span id="btnOutstandingbyPayTerm" runat="server"><a href="OutstandingReport_PaymentTerm.aspx" style="font-size: 16px; border: 1px dashed gray; padding: 4px;">&nbsp;Outstanding Report by Payment Term</a>&nbsp;&nbsp;</span>
                                <span id="btnOutstandingList" runat="server"><a href="OutstandingReport_List.aspx" style="font-size: 16px; border: 1px dashed gray; padding: 4px;">&nbsp;Outstanding Report List</a>&nbsp;&nbsp;</span>
                                
                            </div>
                        </div>
                    </div>
                </div>
                <br />
                <div class="card">
                    <div class="card-header bg-primary text-uppercase text-white">
                        <div class="row">
                            <div class="col-md-4">
                                <h5>Report</h5>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-xl-12 col-md-12">
                            <div class="card">
                                <div class="card-header">
                                    <div class="row" style="margin-left: -12px;">
                                        <div class="col-xl-1 col-md-1 spancls">Type<i class="reqcls">&nbsp;</i> :</div>
                                        <div class="col-xl-2 col-md-2 spancls" style="margin-left: 177px;">Party Name<i class="reqcls">&nbsp;</i> :</div>
                                        <div class="col-xl-2 col-md-2 spancls" style="margin-left: 87px;">From Date<i class="reqcls">&nbsp;</i> :</div>
                                        <div class="col-xl-2 col-md-2 spancls" style="margin-left: 92px;">To Date<i class="reqcls">&nbsp;</i> :</div>
                                    </div>
                                    <div class="row">
                                        <div class="col-xl-3 col-md-3">
                                            <asp:TextBox ID="ddltype" runat="server" CssClass="form-control" Width="100%" Text="SALE" ReadOnly="true"></asp:TextBox>
                                        </div>
                                        <div class="col-xl-3 col-md-3">
                                            <asp:TextBox ID="txtPartyName" runat="server" CssClass="form-control" placeholder="Party Name" Width="100%" AutoPostBack="true"></asp:TextBox>
                                            <asp:AutoCompleteExtender ID="AutoCompleteExtender1" runat="server" CompletionListCssClass="completionList"
                                                CompletionListHighlightedItemCssClass="itemHighlighted" CompletionListItemCssClass="listItem"
                                                CompletionInterval="10" MinimumPrefixLength="1" ServiceMethod="GetCustomerList"
                                                TargetControlID="txtPartyName">
                                            </asp:AutoCompleteExtender>
                                        </div>
                                        <div class="col-xl-3 col-md-3">
                                            <asp:TextBox ID="txtfromdate" runat="server" CssClass="form-control" placeholder="From Date" Width="100%" AutoComplete="off"></asp:TextBox>
                                            <asp:CalendarExtender ID="CalendarExtender1" TargetControlID="txtfromdate" Format="dd-MM-yyyy" CssClass="cal_Theme1" runat="server"></asp:CalendarExtender>
                                        </div>
                                        <div class="col-xl-3 col-md-3">
                                            <asp:TextBox ID="txttodate" runat="server" CssClass="form-control" placeholder="To Date" Width="100%" AutoComplete="off"></asp:TextBox>
                                            <asp:CalendarExtender ID="CalendarExtender2" TargetControlID="txttodate" Format="dd-MM-yyyy" CssClass="cal_Theme1" runat="server"></asp:CalendarExtender>
                                        </div>
                                    </div>
                                    <br />
                                    <div class="row" style="display: none">
                                        <div class="col-xl-3 col-md-3">
                                            <asp:Button ID="btnsearch" runat="server" ValidationGroup="form1" CssClass="btn btn-primary" Text="Search" OnClick="btnsearch_Click" />

                                        </div>
                                    </div>
                                    <div class="row" id="btn" runat="server">
                                        <div class="col-xl-6 col-md-6">
                                            <asp:Button ID="btnexcel" runat="server" ValidationGroup="form1" CssClass="btn btn-primary" Text="Export Excel" OnClick="ExportExcel" />
                                            <asp:Button ID="btnpdf" runat="server" ValidationGroup="form1" CssClass="btn btn-primary" Text="Export PDF" OnClick="btnpdf_Click" />
                                            <asp:Button ID="btnresetfilter" CssClass="btn btn-danger" runat="server" Text="Reset" OnClick="btnresetfilter_Click" />
                                        </div>
                                        <div class="col-xl-4 col-md-4"></div>
                                    </div>
                                </div>
                                <div class="col-md-12" style="padding: 20px; margin-top: 5px;">
                                    <iframe id="ifrRight6" runat="server" enableviewstate="false" style="width: 100%; -ms-zoom: 0.75; height: 685px;"></iframe>
                                </div>
                                <div class="col-md-12" style="padding: 0px; margin-top: 5px;">
                                    <div id="DivRoot1" align="left" runat="server">
                                        <div style="overflow: hidden;" id="DivHeaderRow1">
                                        </div>
                                        <div style="overflow: scroll;" class="dt-responsive table-responsive" onscroll="OnScrollDiv(this)" id="DivMainContent1">
                                            <asp:GridView ID="dgvOutstanding" runat="server"
                                                CssClass="table table-striped table-bordered nowrap" AutoGenerateColumns="false"
                                                AllowPaging="false" ShowHeader="true" PageSize="50" DataKeyNames="BillingCustomer" OnRowDataBound="OnRowDataBound">
                                                <Columns>
                                                    <%-- <asp:TemplateField HeaderStyle-Width="20">
                                                        <ItemTemplate>
                                                            <img alt="" style="cursor: pointer" src="../img/plus.png" />
                                                            <asp:Panel ID="pnlOrders" runat="server" Style="display: none">
                                                                <asp:GridView ID="dgvOutstandingDetails" runat="server" AutoGenerateColumns="false" CssClass="ChildGrid">
                                                                    <Columns>
                                                                        <asp:BoundField ItemStyle-Width="150px" DataField="Type" HeaderText="Type" />
                                                                        <asp:BoundField ItemStyle-Width="150px" DataField="InvoiceNo" HeaderText="Invoice No" />
                                                                        <asp:BoundField ItemStyle-Width="150px" DataField="Invoicedate" HeaderText="Invoice Date" />
                                                                        <asp:BoundField ItemStyle-Width="150px" DataField="DocAmt" HeaderText="Doc Amt" />
                                                                        <asp:BoundField ItemStyle-Width="150px" DataField="Payable" HeaderText="Payable" />
                                                                        <asp:BoundField ItemStyle-Width="150px" DataField="Received" HeaderText="Received" />
                                                                        <asp:BoundField ItemStyle-Width="150px" DataField="Cum_Balance" HeaderText="Cum.Balance" />
                                                                        <asp:BoundField ItemStyle-Width="150px" DataField="days" HeaderText="Days" />
                                                                    </Columns>
                                                                </asp:GridView>
                                                            </asp:Panel>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>--%>
                                                    <asp:BoundField ItemStyle-Width="100px" DataField="Type" HeaderText="Type" />
                                                    <asp:BoundField ItemStyle-Width="100px" DataField="InvoiceNo" HeaderText="InvoiceNo" />
                                                    <asp:BoundField ItemStyle-Width="100px" DataField="BillingCustomer" HeaderText="Party Name" />
                                                    <%--   <asp:BoundField ItemStyle-Width="10px" DataField="ShippingAddress" HeaderText="Shipping Address" />--%>
                                                    <asp:BoundField ItemStyle-Width="100px" DataField="DocAmt" HeaderText="DocAmt" />
                                                    <asp:BoundField ItemStyle-Width="100px" DataField="Payable" HeaderText="Payable" />
                                                    <%-- <asp:BoundField ItemStyle-Width="100px" DataField="Received" HeaderText="Received" />--%>

                                                    <asp:TemplateField HeaderText="Balance" ItemStyle-HorizontalAlign="Center">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblbalance" runat="server"></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>

                                                    <asp:TemplateField HeaderText="Cum.Balance" ItemStyle-HorizontalAlign="Center">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblCum_Balance" runat="server"></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:BoundField ItemStyle-Width="100px" DataField="Days" HeaderText="Days" />
                                                </Columns>
                                            </asp:GridView>
                                            <div id="DivFooterRow1" style="overflow: hidden">
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
    </div>
    <%--</ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnsearch" />
            <asp:PostBackTrigger ControlID="btnresetfilter" />
            <asp:PostBackTrigger ControlID="btnexcel" />
            <asp:PostBackTrigger ControlID="btnpdf" />
            <asp:PostBackTrigger ControlID="btndatewise" />
            <asp:PostBackTrigger ControlID="btnmonthwise" />
            <asp:PostBackTrigger ControlID="btnyearwise" />
           <%-- <asp:AsyncPostBackTrigger ControlID="lnkdownload" />
        </Triggers>
    </asp:UpdatePanel>--%>

    <rsweb:ReportViewer ID="ReportViewer1" runat="server" Visible="false" ></rsweb:ReportViewer>
</asp:Content>

