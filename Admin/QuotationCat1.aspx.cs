﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Net.Mail;
using iTextSharp.text.pdf;
using System.Globalization;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office.CustomUI;

public partial class Admin_Quotation : System.Web.UI.Page
{
    static int quotationid; static string oldfile1, oldfile2, date;

    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
    string action = "", quotationno = "";
    DataTable dtConstructionType = new DataTable();
    DataTable dt = new DataTable();
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["name"] == null)
        {
            Response.Redirect("../Login.aspx");
        }
        else
        {
            if (!IsPostBack)
            {
                fillddlpaymentterm();
                BindCurrency();
                List<string> lstValidityOffer = new List<string>();
                lstValidityOffer.Add("30 days from the date of quotation and confirmation thereafter");
                lstValidityOffer.Add("7 days from the date of quotation and confirmation thereafter");
                ddlvalidityofoffer.DataSource = lstValidityOffer;
                ddlvalidityofoffer.DataBind();

                //ddlvalidityofoffer.Text = "30 days from the date of quotation and confirmation thereafter";
                ddldeliveryperiod.Text = "2-3 weeks from date of purchase order";
                ddltransportation.Text = "Extra as applicable";
                ddlStandardPacking.Text = "The enclosures will be packed in 2 ply corrugated sheet. The charges for the same are included in above price";
                ddlSpecialPacking.Text = "Any special packing will be supplied at extra cost";
                ddlinspection.SelectedItem.Text = "You/Your representative can inspect the enclosure at our factory";
                txtHsn1.Text = "85381010";

                constype = string.Empty;
                //if (Request.QueryString["Ccode"] != null)
                if (Request.QueryString["Ccode"] != null && Request.QueryString["id"] != null)
                {
                    ViewState["RowNo"] = 0;
                    dt.Columns.AddRange(new DataColumn[14] { new DataColumn("id"),
                 new DataColumn("description"), new DataColumn("hsncode")
                , new DataColumn("qty"), new DataColumn("rate"), new DataColumn("CGST")
                , new DataColumn("SGST"), new DataColumn("IGST"), new DataColumn("CGSTamt"), new DataColumn("SGSTamt")
                , new DataColumn("IGSTamt"), new DataColumn("totaltax"),new DataColumn("discount"),new DataColumn("amount")
            });
                    ViewState["QuatationData"] = dt;
                    Session["QuatationData"] = dt;
                    GenerateCode();
                    GetCompanyDataByName(Decrypt(Request.QueryString["Ccode"].ToString()));
                    HFEnqID.Value = Decrypt(Request.QueryString["id"].ToString());
                    // FillExcelsheet();
                }
                else
                {
                    //Response.Redirect("QuotationList.aspx");
                }


                if (Request.QueryString["cdd"] != null)
                {
                    ViewState["RowNo"] = 0;
                    dt.Columns.AddRange(new DataColumn[14] { new DataColumn("id"),
                 new DataColumn("description"), new DataColumn("hsncode")
                , new DataColumn("qty"), new DataColumn("rate"), new DataColumn("CGST")
                , new DataColumn("SGST"), new DataColumn("IGST"), new DataColumn("CGSTamt"), new DataColumn("SGSTamt")
                , new DataColumn("IGSTamt"), new DataColumn("totaltax"),new DataColumn("discount"),new DataColumn("amount")
            });
                    ViewState["QuatationData"] = dt;
                    Session["QuatationData"] = dt;
                    GetQuotationdata();
                    FillExcelsheet();
                    //ddlConstype.Enabled = false;
                }
                if (Request.QueryString["cdd"] == null)
                {
                    oldfile1 = ""; oldfile2 = ""; txtdate.Text = DateTime.Now.ToString("dd-MM-yyyy");
                }

                Getemail();

                txtCGSTamt.Attributes.Add("readonly", "readonly");
                txtSGSTamt.Attributes.Add("readonly", "readonly");
                txtIGSTamt.Attributes.Add("readonly", "readonly");
                txtAmt1.Attributes.Add("readonly", "readonly");


            }


            dtConstructionType.Columns.AddRange(new DataColumn[9] { new DataColumn("quotationno", typeof(string)),new DataColumn("quotationid", typeof(Int32)),
                    new DataColumn("categoryname",typeof(string)),new DataColumn("category1", typeof(string)),new DataColumn("category2", typeof(string)),
                    new DataColumn("category3", typeof(string)),new DataColumn("category4",typeof(string)),new DataColumn("category5", typeof(string)),new DataColumn("category6", typeof(string)) });

        }
    }

    // 22-03-2022
    private void BindCurrency()
    {
        SqlDataAdapter DA = new SqlDataAdapter("select * from [ExcelEncLive].tblCurrencyMaster", con);
        DataTable DT = new DataTable();
        DA.Fill(DT);

        if (DT.Rows.Count > 0)
        {
            ddlCurrency.DataSource = DT;
            ddlCurrency.DataValueField = "Symbol";
            ddlCurrency.DataTextField = "ISOCode";
            ddlCurrency.DataBind();

        }
        ddlCurrency.Items.Insert(0, new ListItem("--Select Currency--", "0"));
    }

    protected void Insert(object sender, EventArgs e)
    {
        if (txtQty1.Text == "" || txtRate1.Text == "" || txtAmt1.Text == "")
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Please fill quantity and price !!!');", true);
            txtQty1.Focus();
        }
        else
        {
            Show_Grid();
        }
    }

    private void Show_Grid()
    {
        string DescrData = Description();
        ViewState["RowNo"] = (int)ViewState["RowNo"] + 1;
        DataTable dt = (DataTable)ViewState["QuatationData"];
        decimal totalTax = Convert.ToDecimal(txtCGSTamt.Text) + Convert.ToDecimal(txtSGSTamt.Text);

        dt.Rows.Add(ViewState["RowNo"], DescrData, txtHsn1.Text, txtQty1.Text, txtRate1.Text, txtCGST.Text, txtSGST.Text, txtIGST.Text, Request.Form[txtCGSTamt.UniqueID].ToString(), Request.Form[txtSGSTamt.UniqueID].ToString(), Request.Form[txtIGSTamt.UniqueID].ToString(), totalTax, txtdisc1.Text, Request.Form[txtAmt1.UniqueID].ToString());
        ViewState["QuatationData"] = dt;

        dgvQuatationDtl.DataSource = (DataTable)ViewState["QuatationData"];
        dgvQuatationDtl.DataBind();

        txtQty1.Text = string.Empty;
        txtRate1.Text = string.Empty;
        txtCGST.Text = string.Empty;
        txtSGST.Text = string.Empty;
        txtIGST.Text = string.Empty;
        txtCGSTamt.Text = string.Empty;
        txtSGSTamt.Text = string.Empty;
        txtIGSTamt.Text = string.Empty;
        txtdisc1.Text = string.Empty;
        txtAmt1.Text = string.Empty;
    }
    protected void getQutationdts()
    {

        DataTable Dtproduct = new DataTable();
        SqlDataAdapter daa = new SqlDataAdapter("SELECT [id],[quotationid],[quotationno],[sno],[description],[hsncode],[qty],[rate],[CGST],[SGST],[IGST],[CGSTamt],[SGSTamt],[IGSTamt],[totaltax],[discount],[amount]FROM QuotationData where quotationno='" + txQutno.Text + "'", con);
        daa.Fill(Dtproduct);
        ViewState["RowNo"] = (int)ViewState["RowNo"] + 1;

        DataTable dt = Session["QuatationData"] as DataTable;

        if (Dtproduct.Rows.Count > 0)
        {
            for (int i = 0; i < Dtproduct.Rows.Count; i++)
            {
                dt.Rows.Add(ViewState["RowNo"], Dtproduct.Rows[i]["description"].ToString(), Dtproduct.Rows[i]["hsncode"].ToString(), Dtproduct.Rows[i]["qty"].ToString(), Dtproduct.Rows[i]["rate"].ToString(), "", "", "", "", "", "", Dtproduct.Rows[i]["totaltax"].ToString(), Dtproduct.Rows[i]["discount"].ToString(), Dtproduct.Rows[i]["amount"].ToString());
                ViewState["QuatationData"] = dt;
            }
        }
        dgvQuatationDtl.DataSource = dt;
        dgvQuatationDtl.DataBind();
    }

    private void FillExcelsheet()
    {
        string constype = "";
        if (ddlConstype.SelectedItem.Text == "PC ENCLOSURE")
        {
            if (rdlpcenclosure.SelectedIndex == 0)
            {
                constype = "Shop Floor PC Enclosure Standing";
            }
            if (rdlpcenclosure.SelectedIndex == 1)
            {
                constype = "PC Enclosure ECO-Standing";
            }
            if (rdlpcenclosure.SelectedIndex == 2)
            {
                constype = "PC Enclosure ECO-Sitting";
            }
        }
        if (ddlConstype.SelectedItem.Text != "PC ENCLOSURE")
        {
            constype = ddlConstype.SelectedItem.Text;
        }

        if (!string.IsNullOrEmpty(myconstype))
        {
            if (myconstype == "JBboxdata")
            {
                myconstype = "JB Box";
            }
            if (myconstype == "WMM23AEBox")
            {
                myconstype = "WMM-22";
            }
            if (myconstype == "EcoFrame")
            {
                myconstype = "Eco Frame 43mm";
            }
            if (myconstype == "Modular")
            {
                myconstype = "Modular W-Big 43 mm";
            }
            if (myconstype == "WMM30MCCBox")
            {
                myconstype = "WMM-30 (MCC Box)";
            }
            if (myconstype == "EcoMCC")
            {
                myconstype = "Eco MCC 30mm";
            }
            if (myconstype == "MFS")
            {
                myconstype = "MFS (Modular Floor Standing Enclosure)";
            }
            if (myconstype == "PCEnclosureECOStanding")
            {
                myconstype = "PC Enclosure ECO-Standing";
            }
            if (myconstype == "PCEnclosureECOSitting")
            {
                myconstype = "PC Enclosure ECO-Sitting";
            }
            if (myconstype == "ShopFloorPCEnclosureStanding")
            {
                myconstype = "Shop Floor PC Enclosure Standing";
            }
        }

        SqlDataAdapter addefault = new SqlDataAdapter();
        if (Request.QueryString["cdd"] == null)
        {
            addefault = new SqlDataAdapter("select id,sheetname,category,path, status from excelsheetdata where category='" + constype + "' and quotationid is null and Isactive=0", con);
            myconstype = constype;
        }
        else
        {
            addefault = new SqlDataAdapter("select id,sheetname,category,path, status from excelsheetdata where category='" + myconstype + "' and Isactive=0 and quotationid=" + myquotationid, con);
        }

        DataTable dtdefault = new DataTable();
        addefault.Fill(dtdefault);

        if (dtdefault.Rows.Count > 0)
        {
            Repeater1.DataSource = dtdefault;
            Repeater1.DataBind();
        }
        if (dtdefault.Rows.Count < 1)
        {
            Repeater1.DataSource = dtdefault;
            Repeater1.DataBind();
        }

        if (Request.QueryString["cdd"] == null)
        { }
        else
        {
            if (dtdefault.Rows.Count == 0)
            {
                addefault = new SqlDataAdapter("select id,sheetname,category,path, status from excelsheetdata where category='" + myconstype + "' and quotationid is null and Isactive=0", con);
                DataTable dtdefault2 = new DataTable();
                addefault.Fill(dtdefault2);

                if (dtdefault2.Rows.Count > 0)
                {
                    Repeater1.DataSource = dtdefault2;
                    Repeater1.DataBind();
                }
                if (dtdefault2.Rows.Count < 1)
                {
                    Repeater1.DataSource = dtdefault2;
                    Repeater1.DataBind();
                }
            }
        }
    }

    private void fillddlpaymentterm()
    {
        SqlDataAdapter adpt = new SqlDataAdapter("select distinct paymentterm,transportation,standardpackaging,specialpackaging,deliveryperiod,validityofoffer,inspection from QuotationMainFooter", con);
        DataTable dtpt = new DataTable();
        adpt.Fill(dtpt);

        if (dtpt.Rows.Count > 0)
        {
            ////////1
            List<string> lstpaytm = new List<string>();
            for (int i = 0; i < dtpt.Rows.Count; i++)
            {
                if (dtpt.Rows[i]["paymentterm"].ToString() != "")
                {
                    lstpaytm.Add(dtpt.Rows[i]["paymentterm"].ToString());
                }
            }
            ddlpaymentterm.DataSource = lstpaytm;
            ddlpaymentterm.DataBind();

            ///////////2
            List<string> lstdelp = new List<string>();
            for (int i = 0; i < dtpt.Rows.Count; i++)
            {
                if (dtpt.Rows[i]["deliveryperiod"].ToString() != "")
                {
                    lstdelp.Add(dtpt.Rows[i]["deliveryperiod"].ToString());
                }
            }
            ddldeliveryperiod.DataSource = lstdelp;
            ddldeliveryperiod.DataBind();

            ////3
            List<string> lstval = new List<string>();
            for (int i = 0; i < dtpt.Rows.Count; i++)
            {
                if (dtpt.Rows[i]["validityofoffer"].ToString() != "")
                {
                    lstval.Add(dtpt.Rows[i]["validityofoffer"].ToString());
                }
            }
            ddlvalidityofoffer.DataSource = lstval;
            ddlvalidityofoffer.DataBind();


            ////4
            List<string> lsttrans = new List<string>();
            for (int i = 0; i < dtpt.Rows.Count; i++)
            {
                if (dtpt.Rows[i]["transportation"].ToString() != "")
                {
                    lsttrans.Add(dtpt.Rows[i]["transportation"].ToString());
                }
            }
            ddltransportation.DataSource = lsttrans;
            ddltransportation.DataBind();

            ////5
            List<string> lststandpkg = new List<string>();
            for (int i = 0; i < dtpt.Rows.Count; i++)
            {
                if (dtpt.Rows[i]["standardpackaging"].ToString() != "")
                {
                    lststandpkg.Add(dtpt.Rows[i]["standardpackaging"].ToString());
                }
            }
            ddlStandardPacking.DataSource = lststandpkg;
            ddlStandardPacking.DataBind();

            ////6
            List<string> lstspecpack = new List<string>();
            for (int i = 0; i < dtpt.Rows.Count; i++)
            {
                if (dtpt.Rows[i]["specialpackaging"].ToString() != "")
                {
                    lstspecpack.Add(dtpt.Rows[i]["specialpackaging"].ToString());
                }
            }
            ddlSpecialPacking.DataSource = lstspecpack;
            ddlSpecialPacking.DataBind();

            ////7
            List<string> lstinspection = new List<string>();
            for (int i = 0; i < dtpt.Rows.Count; i++)
            {
                if (dtpt.Rows[i]["inspection"].ToString() != "")
                {
                    lstinspection.Add(dtpt.Rows[i]["inspection"].ToString());
                }
            }
            ddlinspection.DataSource = lstinspection;
            ddlinspection.DataBind();
        }
    }
    static string myconstype; static int myquotationid;
    #region Fill Data for Update
    private void GetQuotationdata()
    {
        SqlCommand cmdall = new SqlCommand("select * from QuotationMain where id=@id", con);
        cmdall.Parameters.AddWithValue("@id", +Convert.ToInt32(Decrypt(Request.QueryString["cdd"].ToString())));

        SqlDataAdapter adall = new SqlDataAdapter(cmdall);
        DataTable dtall = new DataTable();
        adall.Fill(dtall);

        if (dtall.Rows.Count > 0)
        {
            btnsubmit.Text = "Update/Save";
            myquotationid = Convert.ToInt32(dtall.Rows[0]["id"].ToString());
            txtcname.Text = dtall.Rows[0]["partyname"].ToString();
            txQutno.Text = dtall.Rows[0]["quotationno"].ToString();
            GetCompanyDataByName(dtall.Rows[0]["ccode"].ToString());
            ddlkindatt.Text = dtall.Rows[0]["kindatt"].ToString();
            ddltaxation.SelectedValue = dtall.Rows[0]["Taxation"].ToString();
            ddlCurrency.SelectedItem.Text = dtall.Rows[0]["Currency"].ToString();

            string str = dtall.Rows[0]["date"].ToString();
            str = str.Replace("12:00:00 AM", "");
            var time = Convert.ToDateTime(str);// 2017/1/15 12:00:00

            txtdate.Text = time.ToString("dd/MM/yyyy");
            txtshippingaddress.Text = dtall.Rows[0]["address"].ToString();
            txtremark.Text = dtall.Rows[0]["remark"].ToString();
            Session["Constructiontype"] = dtall.Rows[0]["Constructiontype"].ToString();
            Session["material"] = dtall.Rows[0]["material"].ToString();
            // txtsr1.Text = dtall.Rows[0]["sno"].ToString();
            //txtHsn1.Text = dtall.Rows[0]["hsncode"].ToString();
            //txtQty1.Text = dtall.Rows[0]["qty"].ToString();
            //txtRate1.Text = dtall.Rows[0]["rate"].ToString();
            //txtdisc1.Text = dtall.Rows[0]["discount"].ToString();

            //txtCGST.Text = dtall.Rows[0]["CGST"].ToString();
            //txtSGST.Text = dtall.Rows[0]["SGST"].ToString();
            //txtIGST.Text = dtall.Rows[0]["IGST"].ToString();
            //txtCGSTamt.Text = dtall.Rows[0]["CGSTamt"].ToString();
            //txtSGSTamt.Text = dtall.Rows[0]["SGSTamt"].ToString();
            //txtIGSTamt.Text = dtall.Rows[0]["IGSTamt"].ToString();

            //txtAmt1.Text = dtall.Rows[0]["amount"].ToString();
            //txtwidth.Text = dtall.Rows[0]["width"].ToString();
            //txtdepth.Text = dtall.Rows[0]["depth"].ToString();
            //txtheight.Text = dtall.Rows[0]["height"].ToString();
            //txtbase.Text = dtall.Rows[0]["base"].ToString();

            //txtcanopy.Text = dtall.Rows[0]["canopy"].ToString();
            //ddlmaterial.Text = dtall.Rows[0]["material"].ToString();
            // txtSpecifymaterial.Text = dtall.Rows[0]["specifymaterial"].ToString();
            //txtconstructiontype.Text = dtall.Rows[0]["Specifyconstruction"].ToString();

            //if (ddlmaterial.Text == "Specify")
            //{
            //    txtSpecifymaterial.Visible = true;
            //}
            //else
            //    txtSpecifymaterial.Visible = false;

            if (dtall.Rows[0]["paymentterm1"].ToString() == "Specify")
            {
                ddlpaymentterm.SelectedItem.Text = dtall.Rows[0]["paymentterm1"].ToString();
                txtPTSpecify.Text = dtall.Rows[0]["paymentterm2"].ToString();
                txtPTSpecify.Visible = true;
                lblSpecify.Visible = true;
            }
            else
            {
                ddlpaymentterm.SelectedItem.Text = dtall.Rows[0]["paymentterm1"].ToString();
                txtPTSpecify.Visible = false;
                lblSpecify.Visible = false;
            }
            //txtpayment2.Text = dtall.Rows[0]["paymentterm2"].ToString();
            //txtpayment3.Text = dtall.Rows[0]["paymentterm3"].ToString();
            //txtpayment4.Text = dtall.Rows[0]["paymentterm4"].ToString();
            //txtpayment5.Text = dtall.Rows[0]["paymentterm5"].ToString();

            ddlvalidityofoffer.Text = dtall.Rows[0]["validityofoffer"].ToString();
            ddldeliveryperiod.Text = dtall.Rows[0]["deliveryperiod"].ToString();
            ddltransportation.Text = dtall.Rows[0]["transportation"].ToString();
            ddlStandardPacking.Text = dtall.Rows[0]["standardpackaging"].ToString();
            ddlSpecialPacking.Text = dtall.Rows[0]["specialpackaging"].ToString();
            ddlinspection.SelectedItem.Text = dtall.Rows[0]["inspection"].ToString();

            if (!string.IsNullOrEmpty(dtall.Rows[0]["filename1"].ToString()))
            {
                lblimg.Visible = true;
                lblimg.Text = "<u>Attachment </u>: " + dtall.Rows[0]["filename1"].ToString();
                oldfile1 = dtall.Rows[0]["filename1"].ToString();
                oldfile2 = dtall.Rows[0]["filename2"].ToString();
            }
            getQutationdts();
            //if (!string.IsNullOrEmpty(txtpayment2.Text))
            //{
            //    paym2.Visible = true;
            //    btnpayment1.Visible = false;
            //}
            //if (!string.IsNullOrEmpty(txtpayment3.Text))
            //{
            //    paym3.Visible = true;
            //    btnpayment3.Visible = false;
            //}
            //if (!string.IsNullOrEmpty(txtpayment4.Text))
            //{
            //    paym4.Visible = true;
            //    btnpayment4.Visible = false;
            //}
            //if (!string.IsNullOrEmpty(txtpayment5.Text))
            //{
            //    paym5.Visible = true;
            //    btnpayment5.Visible = false;
            //}

            //myconstype = dtall.Rows[0]["Constructiontype"].ToString();

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "JBboxdata")
            //{
            //    ddlConstype.Text = "JB Box";

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        if (category == "Welded Main Body")
            //        {
            //            chkJbweldedmainbody.Checked = true;
            //            ddlJbweldedmainbodycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlJbweldedmainbodycat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }

            //        if (category == "Gland Plate")
            //        {
            //            chkjbGlandplat.Checked = true;
            //            ddljbGlandplatcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddljbGlandplatcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }

            //        if (category == "Component Mtg Plate" || category == "M4 Tapped Z welded" || category == "DIN Channel")
            //        {
            //            ddljbComponetmtgplt.Text = dtall.Rows[3]["categoryname"].ToString();
            //            ddljbComponetmtgpltcat1.Text = dtall.Rows[3]["category1"].ToString();
            //            ddljbComponetmtgpltcat2.Text = dtall.Rows[3]["category2"].ToString();
            //        }

            //        if (category == "Front Screwed Cover")
            //        {
            //            chkjbfrontscrewcover.Checked = true;
            //            ddljbfrontscrewcovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }

            //        if (category == "Lock")
            //        {
            //            Chkjblock.Checked = true;
            //            ddljbLockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }

            //        if (category == "Transperent Door")
            //        {
            //            chkjbTransparentdoor.Checked = true;
            //            ddljbTransparentdoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddljbTransparentdoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddljbTransparentdoorcat3.Text = dtall.Rows[i]["category3"].ToString();

            //            if (ddljbTransparentdoorcat2.Text == "Specify")
            //            {
            //                txtjbTransparentdoorcat4.Visible = true;
            //                txtjbTransparentdoorcat4.Text = dtall.Rows[i]["category4"].ToString();
            //            }
            //        }

            //        if (category == "Wall Mtg. Bracket")
            //        {
            //            chkjbwallmtgbracket.Checked = true;
            //        }

            //        if (category == "Power Coating Shade")
            //        {
            //            ChkjbPowercoatingshade.Checked = true;
            //            ddljbPowercoatingshadecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtjbPowercoatingshadecat2.Text = dtall.Rows[i]["category2"].ToString();

            //            if (ddljbPowercoatingshadecat1.Text == "Specify")
            //            {
            //                txtjbPowercoatingshadecat2.Visible = true;
            //            }
            //        }

            //        if (category == "Fan")
            //        {
            //            ChkjbFan.Checked = true;
            //            ddljbfancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtjbfanqtycat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }

            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkjbJointlesspolyurethane.Checked = true;
            //        }

            //        if (category == "Any additional component")
            //        {
            //            ChkjbAnyadditionalcomponent.Checked = true;
            //            txtjbAnyadditionalcomponentcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "WMM23AEBox")
            //{
            //    ddlConstype.Text = "WMM-23.5 (AE Box)";

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Welded Main Body")
            //        {
            //            chkWMM23WeldedMainBody.Checked = true;
            //            ddlWMM23WeldedMainBodycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM23WeldedMainBodycat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //2
            //        if (category == "Gland Plate")
            //        {
            //            ChkWMM23GlandPlate.Checked = true;
            //            ddlWMM23GlandPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM23GlandPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //3
            //        if (category == "Canopy")
            //        {
            //            ChkWMM23Canopy.Checked = true;
            //            ddlWMM23Canopycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM23Canopycat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //4
            //        if (category == "Component Mtg. Plate")
            //        {
            //            ChkWMM23ComponentMtgPlate.Checked = true;
            //            ddlWMM23ComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM23ComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlWMM23ComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //5
            //        if (category == "Side C Plate")
            //        {
            //            ChkWMM23SideCPlate.Checked = true;
            //            ddlWMM23SideCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM23SideCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlWMM23SideCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //6
            //        if (category == "Door C Plate")
            //        {
            //            ChkWMM23DoorCPlate.Checked = true;
            //            ddlWMM23DoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM23DoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlWMM23DoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //7
            //        if (category == "Wall Mtg. Bracket")
            //        {
            //            ChkWMM23WallMtgBracket.Checked = true;
            //            ddlWMM23WallMtgBracketcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //8
            //        if (category == "Front Door")
            //        {
            //            ChkWMM23FrontDoor.Checked = true;
            //            ddlWMM23FrontDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM23FrontDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //9
            //        if (category == "Rear Door")
            //        {
            //            ChkWMM23RearDoor.Checked = true;
            //            ddlWMM23RearDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM23RearDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }

            //        //10
            //        if (category == "Door Stiffener")
            //        {
            //            ChkWMM23DoorStiffener.Checked = true;
            //        }
            //        //11                    
            //        if (category == "Lock")
            //        {
            //            ChkWMM23Lock.Checked = true;
            //            ddlWMM23Lockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtWMM23Lockcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //12                    
            //        if (category == "Cable Support Angle")
            //        {
            //            ChkWMM23CableSupportAngle.Checked = true;
            //            txtWMM23CableSupportAnglecat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //13                    
            //        if (category == "Power Coating Shade")
            //        {
            //            ChkWMM23PowerCoatingShade.Checked = true;
            //            ddlWMM23PowerCoatingShadecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtddlWMM23PowerCoatingShadecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //14                    
            //        if (category == "Lifting I-Bolt")
            //        {
            //            ChkWMM23LiftingIBolt.Checked = true;
            //            txtWMM23LiftingIBoltcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //15                    
            //        if (category == "Base")
            //        {
            //            ChkWMM23Base.Checked = true;
            //            ddlWMM23Basecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM23Basecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            txtWMM23Basecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //16
            //        if (category == "Transparent Door")
            //        {
            //            ChkWMM23TransparentDoor.Checked = true;
            //            ddlWMM23TransparentDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM23TransparentDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlWMM23TransparentDoorcat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //17
            //        if (category == "Fan")
            //        {
            //            chkWMM23fan.Checked = true;
            //            ddlWMM23fancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtWMM23fancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //18
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            chkWMM23Jointlesspolyurethanefoamedinplacegasketing.Checked = true;
            //        }
            //        //19
            //        if (category == "Any additional component")
            //        {
            //            ChkWMM23Anyadditionalcomponent.Checked = true;
            //            txtWMM23Anyadditionalcomponentcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "WMM30MCCBox")
            //{
            //    ddlConstype.Text = "WMM-30 (MCC Box)";

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Welded Main Body")
            //        {
            //            ChkWMM30WeldedMainBody.Checked = true;
            //            ddlWMM30WeldedMainBodycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM30WeldedMainBodycat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //2
            //        if (category == "Gland Plate")
            //        {
            //            ChkWMM30GlandPlat.Checked = true;
            //            ddlWMM30GlandPlatcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM30GlandPlatcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //3
            //        if (category == "Canopy")
            //        {
            //            ChkWMM30Canopy.Checked = true;
            //            ddlWMM30Canopycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM30Canopycat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //4
            //        if (category == "Component Mtg. Plate")
            //        {
            //            ChkWMM30ComponentMtgPlate.Checked = true;
            //            ddlWMM30ComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM30ComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlWMM30ComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //5
            //        if (category == "Side C Plate")
            //        {
            //            ChkWMM30SideCPlate.Checked = true;
            //            ddlWMM30SideCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM30SideCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlWMM30SideCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //6
            //        if (category == "Door C Plate")
            //        {
            //            ChkWMM30DoorCPlate.Checked = true;
            //            ddlWMM30DoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM30DoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlWMM30DoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //7
            //        if (category == "Wall Mtg. Bracket")
            //        {
            //            ChkWMM30WallMtgBracket.Checked = true;
            //            ddlWMM30WallMtgBracketcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //8
            //        if (category == "Front Door")
            //        {
            //            ChkWMM30FrontDoor.Checked = true;
            //            ddlWMM30FrontDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM30FrontDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //9
            //        if (category == "Rear Door")
            //        {
            //            ChkWMM30RearDoor.Checked = true;
            //            ddlWMM30RearDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM30RearDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //10
            //        if (category == "Door Stiffener")
            //        {
            //            ChkWMM30DoorStiffener.Checked = true;
            //        }
            //        //11                    
            //        if (category == "Lock")
            //        {
            //            ChkWMM30Lock.Checked = true;
            //            ddlWMM30Lockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtWMM30Lockcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //12                    
            //        if (category == "Cable Support Angle")
            //        {
            //            ChkWMM30CableSupportAngle.Checked = true;
            //            txtWMM30CableSupportAnglecat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //13                    
            //        if (category == "Power Coating Shade")
            //        {
            //            ChkWMM30PowerCoatingShade.Checked = true;
            //            ddlWMM30PowerCoatingShadecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtWMM30PowerCoatingShadecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //14                    
            //        if (category == "Lifting I-Bolt")
            //        {
            //            ChkWMM30LiftingIBolt.Checked = true;
            //            txtWMM30LiftingIBoltcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //15                    
            //        if (category == "Base")
            //        {
            //            ChkWMM30Base.Checked = true;
            //            ddlWMM30Basecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM30Basecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlWMM30Basecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //16
            //        if (category == "Transparent Door")
            //        {
            //            ChkWMM30TransparentDoor.Checked = true;
            //            ddlWMM30TransparentDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlWMM30TransparentDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlWMM30TransparentDoorcat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtWMM30TransparentDoorcat4.Text = dtall.Rows[i]["category4"].ToString();

            //            if (ddlWMM30TransparentDoorcat2.Text == "Specify")
            //            {
            //                txtWMM30TransparentDoorcat5.Visible = true;
            //                txtWMM30TransparentDoorcat5.Text = dtall.Rows[i]["category5"].ToString();
            //            }
            //        }
            //        //18
            //        if (category == "Fan")
            //        {
            //            ChkWMM30fan.Checked = true;
            //            ddlWMM30fancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtWMM30fancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //19
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkWMM30Jointlesspolyurethanefoamedinplacegasketing.Checked = true;
            //        }
            //        //20
            //        if (category == "Any additional component")
            //        {
            //            ChkWMM30Anyadditionalcomponent.Checked = true;
            //            txtWMM30Anyadditionalcomponentcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "MFS")
            //{
            //    ddlConstype.Text = "MFS (Modular Floor Standing Enclosure)";

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Main frame Structure Welded")
            //        {
            //            ChkMFSMainframeStructureWelded.Checked = true;
            //            ddlMFSMainframeStructureWeldedcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //2
            //        if (category == "Bottom Cover")
            //        {
            //            ChkMFSBottomCover.Checked = true;
            //            ddlMFSBottomCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSBottomCovercat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //3
            //        if (category == "Gland Plate")
            //        {
            //            ChkMFSGlandPlate.Checked = true;
            //            ddlMFSGlandPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSGlandPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlMFSGlandPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //4
            //        if (category == "Component Mtg. Plate")
            //        {
            //            ChkMFSComponentMtgPlate.Checked = true;
            //            ddlMFSComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlMFSComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtMFSComponentMtgPlatecat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //5
            //        if (category == "Side C Plate")
            //        {
            //            ChkMFSSideCPlate.Checked = true;
            //            ddlMFSSideCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSSideCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlMFSSideCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtMFSSideCPlatecat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //6
            //        if (category == "Door C Plate")
            //        {
            //            ChkMFSDoorCPlate.Checked = true;
            //            ddlMFSDoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSDoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlMFSDoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtMFSDoorCPlatecat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //7
            //        if (category == "Partial Mounting Plate")
            //        {
            //            ChkMFSPartialMountingPlate.Checked = true;
            //            ddlMFSPartialMountingPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSPartialMountingPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlMFSPartialMountingPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtMFSPartialMountingPlatecat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //8
            //        if (category == "Filler Tray")
            //        {
            //            ChkMFSFillerTray.Checked = true;
            //            ddlMFSFillerTraycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSFillerTraycat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlMFSFillerTraycat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtMFSFillerTraycat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //9
            //        if (category == "Front Door")
            //        {
            //            ChkMFSFrontDoor.Checked = true;
            //            ddlMFSFrontDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSFrontDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //10
            //        if (category == "Rear Door")
            //        {
            //            ChkMFSRearDoor.Checked = true;
            //            ddlMFSRearDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSRearDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //11                    
            //        if (category == "Lock")
            //        {
            //            ChkMFSLock.Checked = true;
            //            ddlMFSLockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //12                    
            //        if (category == "Rear Cover")
            //        {
            //            ChkMFSRearCover.Checked = true;
            //            ddlMFSRearCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //13                    
            //        if (category == "Side Cover")
            //        {
            //            ChkMFSSideCover.Checked = true;
            //            ddlMFSSideCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //14                    
            //        if (category == "Top Cover")
            //        {
            //            ChkMFSTopCover.Checked = true;
            //            ddlMFSTopCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //15                    
            //        if (category == "Power Coating Shade")
            //        {
            //            ChkMFSPowerCoatingShade.Checked = true;
            //            ddlMFSPowerCoatingShadecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtMFSPowerCoatingShadecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //16                    
            //        if (category == "Lifting Arrangement")
            //        {
            //            ChkMFSLiftingArrangement.Checked = true;
            //            ddlMFSLiftingArrangementcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSLiftingArrangementcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //17                    
            //        if (category == "Base")
            //        {
            //            ChkMFSBase.Checked = true;
            //            ddlMFSBasecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSBasecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlMFSBasecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //18
            //        if (category == "Anti-vibration pad")
            //        {
            //            ChkMFSAntivibrationpad.Checked = true;
            //            ddlMFSAntivibrationpadcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //19
            //        if (category == "Drawing Pocket")
            //        {
            //            ChkMFSDrawingPocket.Checked = true;
            //            ddlMFSDrawingPocketcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSDrawingPocketcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //20
            //        if (category == "Micro switch bracket")
            //        {
            //            ChkMFSMicroswitchbracket.Checked = true;
            //        }
            //        //21
            //        if (category == "Tubelight Bracket")
            //        {
            //            ChkMFSTubelightBracket.Checked = true;
            //        }
            //        //22
            //        if (category == "Canopy")
            //        {
            //            ChkMFSCanopy.Checked = true;
            //            ddlMFSCanopycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlMFSCanopycat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlMFSCanopycat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //23
            //        if (category == "Fan")
            //        {
            //            ChkMFSfan.Checked = true;
            //            ddlMFSfancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtMFSfancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //24
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkMFSJointlesspolyurethanefoamedinplacegasketing.Checked = true;
            //        }
            //        //25
            //        if (category == "Any additional component")
            //        {
            //            ChkMFSAnyadditionalcomponent.Checked = true;
            //            txtMFSAnyadditionalcomponentcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "EcoMCC")
            //{
            //    ddlConstype.Text = "Eco MCC 30mm";

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Main frame Structure Welded")
            //        {
            //            ChkEcoMCCMainframeStructureWelded.Checked = true;
            //            ddlEcoMCCMainframeStructureWeldedcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //2
            //        if (category == "Gland Plate")
            //        {
            //            ChkEcoMCCGlandPlate.Checked = true;
            //            ddlEcoMCCGlandPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoMCCGlandPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //3
            //        if (category == "Component Mtg. Plate")
            //        {
            //            ChkEcoMCCComponentMtgPlate.Checked = true;
            //            ddlEcoMCCComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoMCCComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlEcoMCCComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtEcoMCCComponentMtgPlatecat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //4
            //        if (category == "Side C Plate")
            //        {
            //            ChkEcoMCCSideCPlate.Checked = true;
            //            ddlEcoMCCSideCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoMCCSideCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlEcoMCCSideCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtEcoMCCSideCPlatecat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //5
            //        if (category == "Door C Plate")
            //        {
            //            ChkEcoMCCDoorCPlate.Checked = true;
            //            ddlEcoMCCDoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoMCCDoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlEcoMCCDoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtEcoMCCDoorCPlatecat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //6
            //        if (category == "Front Door")
            //        {
            //            ChkEcoMCCFrontDoor.Checked = true;
            //            ddlEcoMCCFrontDoorca1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoMCCFrontDoorca2.Text = dtall.Rows[i]["category2"].ToString();

            //        }
            //        //7
            //        if (category == "Rear Door")
            //        {
            //            ChkEcoMCCRearDoor.Checked = true;
            //            ddlEcoMCCRearDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoMCCRearDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //8                 
            //        if (category == "Rear Cover")
            //        {
            //            ChkEcoMCCRearCover.Checked = true;
            //            ddlEcoMCCRearCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //9                 
            //        if (category == "Side Cover")
            //        {
            //            ChkEcoMCCSideCover.Checked = true;
            //            ddlEcoMCCSideCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //10
            //        if (category == "Lock")
            //        {
            //            ChkEcoMCCLock.Checked = true;
            //            ddlEcoMCCLockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }

            //        //11                    
            //        if (category == "Power Coating Shade")
            //        {
            //            ChkEcoMCCPowerCoatingShade.Checked = true;
            //            ddlEcoMCCPowerCoatingShadecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtEcoMCCPowerCoatingShadecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //12                    
            //        if (category == "Lifting Arrangement")
            //        {
            //            ChkEcoMCCLiftingArrangement.Checked = true;
            //            ddlEcoMCCLiftingArrangementcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoMCCLiftingArrangementcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //13                    
            //        if (category == "Base")
            //        {
            //            ChkEcoMCCBase.Checked = true;
            //            ddlEcoMCCBasecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoMCCBasecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlEcoMCCBasecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //14
            //        if (category == "Anti-vibration pad")
            //        {
            //            ChkEcoMCCAntivibrationpad.Checked = true;
            //            ddlEcoMCCAntivibrationpadcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //15
            //        if (category == "Drawing Pocket")
            //        {
            //            ChkEcoMCCDrawingPocket.Checked = true;
            //            ddlEcoMCCDrawingPocketcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoMCCDrawingPocketcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //16
            //        if (category == "Micro switch bracket")
            //        {
            //            ChkEcoMCCMicroswitchbracket.Checked = true;
            //        }
            //        //17
            //        if (category == "Tubelight Bracket")
            //        {
            //            ChkEcoMCCTubelightBracket.Checked = true;
            //        }
            //        //18
            //        if (category == "Canopy")
            //        {
            //            ChkEcoMCCCanopy.Checked = true;
            //            ddlEcoMCCCanopycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoMCCCanopycat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlEcoMCCCanopycat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //19
            //        if (category == "Fan")
            //        {
            //            ChkEcoMCCfan.Checked = true;
            //            ddlEcoMCCfancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtEcoMCCfancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //20
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkEcoMCCJointlesspolyurethanefoamedinplacegasketing.Checked = true;
            //        }
            //        //21
            //        if (category == "Any additional component")
            //        {
            //            ChkEcoMCCAnyadditionalcomponent.Checked = true;
            //            txtEcoMCCAnyadditionalcomponentcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "Modular")
            //{
            //    ddlConstype.Text = "Modular W-Big 43 mm";

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Welded Main Body")
            //        {
            //            ChkModularWeldedMainBody.Checked = true;
            //            ddlModularWeldedMainBodycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlModularWeldedMainBodycat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //2
            //        if (category == "Gland Plate")
            //        {
            //            ChkModularGlandPlate.Checked = true;
            //            ddlModularGlandPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlModularGlandPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //3
            //        if (category == "Component Mtg. Plate")
            //        {
            //            ChkModularComponentMtgPlate.Checked = true;
            //            ddlModularComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlModularComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlModularComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //4
            //        if (category == "Side C Plate")
            //        {
            //            ChkModularSideCPlate.Checked = true;
            //            ddlModularSideCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlModularSideCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlModularSideCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //5
            //        if (category == "Door C Plate")
            //        {
            //            ChkModularDoorCPlate.Checked = true;
            //            ddlModularDoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlModularDoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlModularDoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //6
            //        if (category == "Wall Mtg. Bracket")
            //        {
            //            ChkModularWallMtgBracket.Checked = true;
            //            ddlModularWallMtgBracketcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //7
            //        if (category == "Front Door")
            //        {
            //            ChkModularFrontDoor.Checked = true;
            //            ddlModularFrontDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlModularFrontDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //8                 
            //        if (category == "Rear Door")
            //        {
            //            ChkModularRearDoor.Checked = true;
            //            ddlModularRearDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //9                 
            //        if (category == "Rear Cover")
            //        {
            //            ChkModularRearCover.Checked = true;
            //            ddlModularRearCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //10
            //        if (category == "Cable Support Angle")
            //        {
            //            ChkModularCableSupportAngle.Checked = true;
            //            txtModularCableSupportAnglecat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //11
            //        if (category == "Lock")
            //        {
            //            ChkModularLock.Checked = true;
            //            ddlModularLockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //12                    
            //        if (category == "Power Coating Shade")
            //        {
            //            ChkModularPowerCoatingShade.Checked = true;
            //            ddlModularPowerCoatingShadecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlModularPowerCoatingShadecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            txtModularPowerCoatingShadecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //13                    
            //        if (category == "Lifting Arrangement")
            //        {
            //            ChkModularLiftingArrangement.Checked = true;
            //            ddlModularLiftingArrangementcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlModularLiftingArrangementcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //14                    
            //        if (category == "Base")
            //        {
            //            ChkModularBase.Checked = true;
            //            ddlModularBasecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlModularBasecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //15
            //        if (category == "Anti-vibration pad")
            //        {
            //            ChkModularAntivibrationpad.Checked = true;
            //            ddlModularAntivibrationpadcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //16
            //        if (category == "Canopy")
            //        {
            //            ChkModularCanopy.Checked = true;
            //            ddlModularCanopycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlModularCanopycat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlModularCanopycat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //17
            //        if (category == "Fan")
            //        {
            //            ChkModularfan.Checked = true;
            //            ddlModularfancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtModularfancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //18
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkModularJointlesspolyurethanefoamedinplacegasketing.Checked = true;
            //        }
            //        //19
            //        if (category == "Any additional component")
            //        {
            //            ChkModularAnyadditionalcomponent.Checked = true;
            //            txtModularAnyadditionalcomponentcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "EcoFrame")
            //{
            //    ddlConstype.Text = "Eco Frame 43mm";

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Main Frame Top Bottom Welded Structure")
            //        {
            //            ChkEcoFrameMainFrameTopBottomWeldedStructure.Checked = true;
            //            ddlEcoFrameMainFrameTopBottomWeldedStructurecat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //2
            //        if (category == "Top/Bottom Gland Plate")
            //        {
            //            ChkEcoFrameTopBottomGlandPlate.Checked = true;
            //            ddlEcoFrameTopBottomGlandPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //3
            //        if (category == "Component Mtg. Plate")
            //        {
            //            ChkEcoFrameComponentMtgPlate.Checked = true;
            //            ddlEcoFrameComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoFrameComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlEcoFrameComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //4
            //        if (category == "Side C Plate")
            //        {
            //            ChkEcoFrameSideCPlate.Checked = true;
            //            ddlEcoFrameSideCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoFrameSideCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlEcoFrameSideCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //5
            //        if (category == "Door C Plate")
            //        {
            //            ChkEcoFrameDoorCPlate.Checked = true;
            //            ddlEcoFrameDoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoFrameDoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlEcoFrameDoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //6
            //        if (category == "Front Door")
            //        {
            //            ChkEcoFrameFrontDoor.Checked = true;
            //            ddlEcoFrameFrontDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoFrameFrontDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //7                 
            //        if (category == "Rear Door")
            //        {
            //            ChkEcoFrameRearDoor.Checked = true;
            //            ddlEcoFrameRearDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //8                 
            //        if (category == "Rear Cover")
            //        {
            //            ChkEcoFrameRearCover.Checked = true;
            //            ddlEcoFrameRearCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //9
            //        if (category == "Side Cover")
            //        {
            //            ChkEcoFrameSideCover.Checked = true;
            //            ddlEcoFrameSideCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //10
            //        if (category == "Lock")
            //        {
            //            ChkEcoFrameLock.Checked = true;
            //            ddlEcoFrameLockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //11                    
            //        if (category == "Power Coating Shade")
            //        {
            //            ChkEcoFramePowerCoatingShade.Checked = true;
            //            ddlEcoFramePowerCoatingShadecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtddlEcoFramePowerCoatingShadecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            {
            //                if (ddlEcoFramePowerCoatingShadecat1.Text == "Specify")
            //                {
            //                    txtddlEcoFramePowerCoatingShadecat2.Visible = true;
            //                }
            //            }
            //        }
            //        //12                   
            //        if (category == "Lifting Arrangement")
            //        {
            //            ChkEcoFrameLiftingArrangement.Checked = true;
            //            ddlEcoFrameLiftingArrangementcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoFrameLiftingArrangementcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //13                    
            //        if (category == "Base")
            //        {
            //            ChkEcoFrameBase.Checked = true;
            //            ddlEcoFrameBasecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoFrameBasecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }

            //        //14
            //        if (category == "Canopy")
            //        {
            //            ChkEcoFrameCanopy.Checked = true;
            //            ddlEcoFrameCanopycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlEcoFrameCanopycat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlEcoFrameCanopycat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //15
            //        if (category == "Fan")
            //        {
            //            ChkEcoFramefan.Checked = true;
            //            ddlEcoFramefancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtEcoFramefancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //16
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkEcoFrameJointlesspolyurethanefoamedinplacegasketing.Checked = true;
            //        }
            //        //17
            //        if (category == "Any additional component")
            //        {
            //            ChkEcoFrameAnyadditionalcomponent.Checked = true;
            //            txtEcoFrameAnyadditionalcomponentcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "ShopFloorPCEnclosureStanding")
            //{
            //    ddlConstype.Text = "PC ENCLOSURE";
            //    rdlpcenclosure.SelectedIndex = 0;
            //    Panel81.Visible = true; rdlpcenclosure.Enabled = false;

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Main Frame Structure Welded")
            //        {
            //            ChkPCEncShopFloorStandingMainframestructure.Checked = true;
            //            ddlPCEncShopFloorStandingMainframestructurecat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //2
            //        if (category == "Bottom Cover")
            //        {
            //            ChkPCEncShopFloorStandingBottomCover.Checked = true;
            //            ddlPCEncShopFloorStandingBottomCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingBottomCovercat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //3
            //        if (category == "Component Mtg Plate")
            //        {
            //            ChkPCEncShopFloorStandingComponentMtgPlate.Checked = true;
            //            txtPCEncShopFloorStandingComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEncShopFloorStandingComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat4.Text = dtall.Rows[i]["category4"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat5.Text = dtall.Rows[i]["category5"].ToString();
            //        }
            //        //32
            //        if (category == "Component Mtg Plate2")
            //        {
            //            ChkPCEncShopFloorStandingComponentMtgPlate.Checked = true;
            //            txtPCEncShopFloorStandingComponentMtgPlatecat21.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEncShopFloorStandingComponentMtgPlatecat22.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat23.Text = dtall.Rows[i]["category3"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat24.Text = dtall.Rows[i]["category4"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat25.Text = dtall.Rows[i]["category5"].ToString();

            //            if (!string.IsNullOrEmpty(txtPCEncShopFloorStandingComponentMtgPlatecat21.Text))
            //            {
            //                componetmtgplat2.Visible = true;
            //            }
            //        }

            //        //33
            //        if (category == "Component Mtg Plate3")
            //        {
            //            ChkPCEncShopFloorStandingComponentMtgPlate.Checked = true;
            //            txtPCEncShopFloorStandingComponentMtgPlatecat31.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEncShopFloorStandingComponentMtgPlatecat32.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat33.Text = dtall.Rows[i]["category3"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat34.Text = dtall.Rows[i]["category4"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat35.Text = dtall.Rows[i]["category5"].ToString();

            //            if (!string.IsNullOrEmpty(txtPCEncShopFloorStandingComponentMtgPlatecat31.Text))
            //            {
            //                componetmtgplat3.Visible = true;
            //            }
            //        }
            //        //34
            //        if (category == "Component Mtg Plate4")
            //        {
            //            ChkPCEncShopFloorStandingComponentMtgPlate.Checked = true;
            //            txtPCEncShopFloorStandingComponentMtgPlatecat41.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEncShopFloorStandingComponentMtgPlatecat42.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat43.Text = dtall.Rows[i]["category3"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat44.Text = dtall.Rows[i]["category4"].ToString();
            //            ddlPCEncShopFloorStandingComponentMtgPlatecat45.Text = dtall.Rows[i]["category5"].ToString();

            //            if (!string.IsNullOrEmpty(txtPCEncShopFloorStandingComponentMtgPlatecat41.Text))
            //            {
            //                componetmtgplat4.Visible = true;
            //            }
            //        }
            //        //4
            //        if (category == "Side C Plate")
            //        {
            //            ChkPCEncShopFloorStandingSidecPlate.Checked = true;
            //            ddlPCEncShopFloorStandingSidecPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingSidecPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingSidecPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //42
            //        if (category == "Side C Plate2")
            //        {
            //            ChkPCEncShopFloorStandingSidecPlate.Checked = true;
            //            ddlPCEncShopFloorStandingSidecPlatecat21.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingSidecPlatecat22.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingSidecPlatecat23.Text = dtall.Rows[i]["category3"].ToString();

            //            if (ddlPCEncShopFloorStandingSidecPlatecat21.Text != "Select")
            //            {
            //                SidecPlate2.Visible = true;
            //            }
            //        }
            //        //43
            //        if (category == "Side C Plate3")
            //        {
            //            ChkPCEncShopFloorStandingSidecPlate.Checked = true;
            //            ddlPCEncShopFloorStandingSidecPlatecat31.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingSidecPlatecat32.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingSidecPlatecat33.Text = dtall.Rows[i]["category3"].ToString();

            //            if (ddlPCEncShopFloorStandingSidecPlatecat31.Text != "Select")
            //            {
            //                SidecPlate3.Visible = true;
            //            }
            //        }
            //        //44
            //        if (category == "Side C Plate4")
            //        {
            //            ChkPCEncShopFloorStandingSidecPlate.Checked = true;
            //            ddlPCEncShopFloorStandingSidecPlatecat41.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingSidecPlatecat42.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingSidecPlatecat43.Text = dtall.Rows[i]["category3"].ToString();

            //            if (ddlPCEncShopFloorStandingSidecPlatecat41.Text != "Select")
            //            {
            //                SidecPlate4.Visible = true;
            //            }
            //        }

            //        //5
            //        if (category == "Door C Plate")
            //        {
            //            ChkPCEncShopFloorStandingDoorCPlate.Checked = true;
            //            ddlPCEncShopFloorStandingDoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingDoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingDoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //52
            //        if (category == "Door C Plate2")
            //        {
            //            ChkPCEncShopFloorStandingDoorCPlate.Checked = true;
            //            ddlPCEncShopFloorStandingDoorCPlatecat21.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingDoorCPlatecat22.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingDoorCPlatecat23.Text = dtall.Rows[i]["category3"].ToString();

            //            if (ddlPCEncShopFloorStandingDoorCPlatecat21.Text != "Select")
            //            {
            //                DoorcPlate2.Visible = true;
            //            }
            //        }
            //        //53
            //        if (category == "Door C Plate3")
            //        {
            //            ChkPCEncShopFloorStandingDoorCPlate.Checked = true;
            //            ddlPCEncShopFloorStandingDoorCPlatecat31.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingDoorCPlatecat32.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingDoorCPlatecat33.Text = dtall.Rows[i]["category3"].ToString();

            //            if (ddlPCEncShopFloorStandingDoorCPlatecat31.Text != "Select")
            //            {
            //                DoorcPlate3.Visible = true;
            //            }
            //        }
            //        //54
            //        if (category == "Door C Plate4")
            //        {
            //            ChkPCEncShopFloorStandingDoorCPlate.Checked = true;
            //            ddlPCEncShopFloorStandingDoorCPlatecat41.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingDoorCPlatecat42.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEncShopFloorStandingDoorCPlatecat43.Text = dtall.Rows[i]["category3"].ToString();

            //            if (ddlPCEncShopFloorStandingDoorCPlatecat41.Text != "Select")
            //            {
            //                DoorcPlate4.Visible = true;
            //            }
            //        }
            //        //6
            //        if (category == "Front Door")
            //        {
            //            ChkPCEncShopFloorStandingFrontDoor.Checked = true;
            //            ddlPCEncShopFloorStandingFrontDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingFrontDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //7                 
            //        if (category == "Rear Door")
            //        {
            //            ChkPCEncShopFloorStandingRearDoor.Checked = true;
            //            ddlPCEncShopFloorStandingRearDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingRearDoorcat2.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //8                 
            //        if (category == "Lock")
            //        {
            //            ChkPCEncShopFloorStandingLock.Checked = true;
            //            ddlPCEncShopFloorStandingLockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEncShopFloorStandingLockcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //9
            //        if (category == "Rear Cover")
            //        {
            //            ChkPCEncShopFloorStandingRearCover.Checked = true;
            //            ddlPCEncShopFloorStandingRearCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //10
            //        if (category == "Side Cover")
            //        {
            //            ChkPCEncShopFloorStandingSideCover.Checked = true;
            //            ddlPCEncShopFloorStandingSideCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //11
            //        if (category == "Top Cover")
            //        {
            //            ChkPCEncShopFloorStandingTopCover.Checked = true;
            //            ddlPCEncShopFloorStandingTopCovercat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //12
            //        if (category == "Horizontal Partition")
            //        {
            //            ChkPCEncShopFloorStandingHorizontalPartition.Checked = true;
            //            ddlPCEncShopFloorStandingHorizontalPartitioncat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingHorizontalPartitioncat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }

            //        //13
            //        if (category == "Sliding Keyboard drawer with Telescopic Rails")
            //        {
            //            ChkPCEncShopFloorStandingSlidingKeyboarddrawer.Checked = true;
            //            ddlPCEncShopFloorStandingSlidingKeyboarddrawercat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingSlidingKeyboarddrawercat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }

            //        //14
            //        if (category == "Powder Coating Shade")
            //        {
            //            ChkPCEncShopFloorStandingPowderCoatingShade.Checked = true;
            //            ddlPCEncShopFloorStandingPowderCoatingShadecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEncShopFloorStandingPowderCoatingShadecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            if (ddlPCEncShopFloorStandingPowderCoatingShadecat1.Text == "Specify")
            //            {
            //                txtPCEncShopFloorStandingPowderCoatingShadecat2.Visible = true;
            //            }
            //        }

            //        //15                    
            //        if (category == "Lifting Arrangement")
            //        {
            //            ChkPCEncShopFloorStandingLiftingArrangement.Checked = true;
            //            ddlPCEncShopFloorStandingLiftingArrangementcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingLiftingArrangementcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //16                    
            //        if (category == "Base")
            //        {
            //            ChkPCEncShopFloorStandingBase.Checked = true;
            //            ddlPCEncShopFloorStandingBasecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingBasecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            txtddlPCEncShopFloorStandingBasecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //17
            //        if (category == "Anti-vibration pad")
            //        {
            //            ChkPCEncShopFloorStandingAntivibrationpad.Checked = true;
            //            ddlPCEncShopFloorStandingAntivibrationpadcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //18
            //        if (category == "Caster Wheel")
            //        {
            //            ChkPCEncShopFloorStandingAntivibrationCasterWheel.Checked = true;
            //            txtPCEncShopFloorStandingAntivibrationCasterWheelcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEncShopFloorStandingAntivibrationCasterWheelcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            txtPCEncShopFloorStandingAntivibrationCasterWheelcat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtPCEncShopFloorStandingAntivibrationCasterWheelcat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //19
            //        if (category == "Drawing Pocket")
            //        {
            //            ChkPCEncShopFloorStandingDrawingPocket.Checked = true;
            //            ddlChkPCEncShopFloorStandingDrawingPocketcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlChkPCEncShopFloorStandingDrawingPocketcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //20
            //        if (category == "Micro switch bracket")
            //        {
            //            ChkPCEncShopFloorStandingMicroswitchbracket.Checked = true;
            //        }
            //        //21
            //        if (category == "Tubelight Bracket")
            //        {
            //            ChkPCEncShopFloorStandingTubelightBracket.Checked = true;
            //        }
            //        //22
            //        if (category == "Transparent Door")
            //        {
            //            ChkPCEncShopFloorStandingTransparentDoor.Checked = true;
            //            ddlPCEncShopFloorStandingTransparentDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEncShopFloorStandingTransparentDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            txtPCEncShopFloorStandingTransparentDoorcat3.Text = dtall.Rows[i]["category3"].ToString();
            //            ddlPCEncShopFloorStandingTransparentDoorcat4.Text = dtall.Rows[i]["category4"].ToString();

            //            if (ddlPCEncShopFloorStandingTransparentDoorcat2.Text == "Specify")
            //            {
            //                txtPCEncShopFloorStandingTransparentDoorcat3.Visible = true;
            //            }
            //        }
            //        //23
            //        if (category == "Filters")
            //        {
            //            ChkPCEncShopFloorStandingFilters.Checked = true;
            //            ddlPCEncShopFloorStandingFilterscat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //24
            //        if (category == "Gas spring for Monitor door")
            //        {
            //            ChkPCEncShopFloorStandingGasspring.Checked = true;
            //        }
            //        //25
            //        if (category == "Aluminium Extrusion For Monitor door")
            //        {
            //            ChkPCEncShopFloorStandingAluminiumExtrusion.Checked = true;
            //        }
            //        //26
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkPCEncShopFloorStandingJointlesspolyurethane.Checked = true;
            //        }
            //        //27
            //        if (category == "Fan")
            //        {
            //            ChkPCEncShopFloorStandingfan.Checked = true;
            //            ddlPCEncShopFloorStandingfancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtChkPCEncShopFloorStandingfancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //28
            //        if (category == "Any additional component")
            //        {
            //            ChkPCEncShopFloorStandingAnyadditional.Checked = true;
            //            txtPCEncShopFloorStandingAnyadditionalcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "PCEnclosureECOStanding" || dtall.Rows[0]["Constructiontype"].ToString() == "PCEnclosureECOSitting")
            //{
            //    ddlConstype.Text = "PC ENCLOSURE";
            //    if (dtall.Rows[0]["Constructiontype"].ToString() == "PCEnclosureECOStanding")
            //    {
            //        rdlpcenclosure.SelectedIndex = 1;
            //    }
            //    if (dtall.Rows[0]["Constructiontype"].ToString() == "PCEnclosureECOSitting")
            //    {
            //        rdlpcenclosure.SelectedIndex = 2;
            //    }
            //    rdlpcenclosure.Enabled = false;
            //    Panel82.Visible = true;

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Welded Main Body")
            //        {
            //            ChkPCEnclosureECOStandingWeldedMainBody.Checked = true;
            //            ddlPCEnclosureECOStandingWeldedMainBodycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEnclosureECOStandingWeldedMainBodycat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //2
            //        if (category == "Gland Plate")
            //        {
            //            ChkPCEnclosureECOStandingGlandPlate.Checked = true;
            //            ddlPCEnclosureECOStandingGlandPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEnclosureECOStandingGlandPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEnclosureECOStandingGlandPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //3
            //        if (category == "Component Mtg Plate")
            //        {
            //            ChkPCEnclosureECOStandingComponentMtgPlate.Checked = true;
            //            txtPCEnclosureECOStandingComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEnclosureECOStandingComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEnclosureECOStandingComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            ddlPCEnclosureECOStandingComponentMtgPlatecat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //4
            //        if (category == "Side C Plate")
            //        {
            //            ChkPCEnclosureECOStandingSideCPlate.Checked = true;
            //            txtPCEnclosureECOStandingSideCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEnclosureECOStandingSideCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEnclosureECOStandingSideCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            ddlPCEnclosureECOStandingSideCPlatecat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //5
            //        if (category == "Door C Plate")
            //        {
            //            ChkPCEnclosureECOStandingDoorCPlate.Checked = true;
            //            txtPCEnclosureECOStandingDoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEnclosureECOStandingDoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEnclosureECOStandingDoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            ddlPCEnclosureECOStandingDoorCPlatecat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //6
            //        if (category == "Front Door with stiffeners")
            //        {
            //            ChkPCEnclosureECOStandingFrontDoorwithstiffeners.Checked = true;
            //            ddlPCEnclosureECOStandingFrontDoorwithstiffenerscat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEnclosureECOStandingFrontDoorwithstiffenerscat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //7                 
            //        if (category == "Cable Support Angle")
            //        {
            //            ChkPCEnclosureECOStandingCableSupportAngle.Checked = true;
            //            txtPCEnclosureECOStandingCableSupportAnglecat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //8                 
            //        if (category == "Lock")
            //        {
            //            ChkPCEnclosureECOStandingLock.Checked = true;
            //            ddlPCEnclosureECOStandingLockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEnclosureECOStandingLockcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //9
            //        if (category == "Horizontal Partition")
            //        {
            //            ChkPCEnclosureECOStandingHorizontalPartition.Checked = true;
            //            ddlPCEnclosureECOStandingHorizontalPartitioncat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEnclosureECOStandingHorizontalPartitioncat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //10
            //        if (category == "Sliding Keyboard drawer with telescopic rails")
            //        {
            //            ChkPCEnclosureECOStandingSlidingKeyboarddrawer.Checked = true;
            //            ddlPCEnclosureECOStandingSlidingKeyboarddrawercat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEnclosureECOStandingSlidingKeyboarddrawercat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }

            //        //11                   
            //        if (category == "Lifting I-Bolt")
            //        {
            //            ChkPCEnclosureECOStandingLiftingIBolt.Checked = true;
            //            txtPCEnclosureECOStandingLiftingIBoltcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //12                    
            //        if (category == "Base")
            //        {
            //            ChkPCEnclosureECOStandingBase.Checked = true;
            //            ddlPCEnclosureECOStandingBasecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEnclosureECOStandingBasecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEnclosureECOStandingBasecat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtPCEnclosureECOStandingBasecat4.Text = dtall.Rows[i]["category4"].ToString();

            //            if (ddlPCEnclosureECOStandingBasecat3.Text == "Specify")
            //            {
            //                txtPCEnclosureECOStandingBasecat4.Visible = true;
            //            }
            //        }

            //        //13                    
            //        if (category == "Anti-vibration pad")
            //        {
            //            ChkPCEnclosureECOStandingAntivibrationpad.Checked = true;
            //            ddlPCEnclosureECOStandingAntivibrationpadcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }

            //        //14
            //        if (category == "Transparent Door")
            //        {
            //            ChkPCEnclosureECOStandingTransparentDoor.Checked = true;
            //            ddlPCEnclosureECOStandingTransparentDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPCEnclosureECOStandingTransparentDoor2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPCEnclosureECOStandingTransparentDoor3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //15
            //        if (category == "Caster Wheel")
            //        {
            //            ChkPCEnclosureECOStandingCasterWheel.Checked = true;
            //            txtPCEnclosureECOStandingCasterWheelcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEnclosureECOStandingCasterWheelcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            txtPCEnclosureECOStandingCasterWheelcat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtPCEnclosureECOStandingCasterWheelcat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //16
            //        if (category == "Filters")
            //        {
            //            ChkPCEnclosureECOStandingFilters.Checked = true;
            //            ddlPCEnclosureECOStandingFilterscat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEnclosureECOStandingFilterscat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //17
            //        if (category == "Telescopic Rail")
            //        {
            //            ChkPCEnclosureECOStandingTelescopicRail.Checked = true;
            //        }
            //        //18
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkPCEnclosureECOStandingJointlesspolyurethane.Checked = true;
            //        }
            //        //19
            //        if (category == "Powder Coating Shade")
            //        {
            //            ChkPCEnclosureECOStandingPowderCoating.Checked = true;
            //            ddlPCEnclosureECOStandingPowderCoatingcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEnclosureECOStandingPowderCoatingcat2.Text = dtall.Rows[i]["category2"].ToString();

            //            if (ddlPCEnclosureECOStandingPowderCoatingcat1.Text == "Specify")
            //            {
            //                txtPCEnclosureECOStandingPowderCoatingcat2.Visible = true;
            //            }
            //        }
            //        //20
            //        if (category == "Fan")
            //        {
            //            ChkPCEnclosureECOStandingfan.Checked = true;
            //            ddlPCEnclosureECOStandingfancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPCEnclosureECOStandingfancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //21
            //        if (category == "Any additional component")
            //        {
            //            ChkPCEnclosureECOStandingAnyadditional.Checked = true;
            //            txtPCEnclosureECOStandingAnyadditionalcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "PC TABLE")
            //{
            //    ddlConstype.Text = "PC TABLE";

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Welded Main Body")
            //        {
            //            ChkPcTableWeldedMainbody.Checked = true;
            //            ddlPcTableWeldedMainbodycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableWeldedMainbodycat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //2
            //        if (category == "Gland Plate")
            //        {
            //            ChkPcTableGlandPlate.Checked = true;
            //            ddlPcTableGlandPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableGlandPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //3
            //        if (category == "Component Mtg. Plate")
            //        {
            //            ChkPcTableComponentMtgPlate.Checked = true;
            //            ddlPcTableComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPcTableComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //4
            //        if (category == "Side C Plate")
            //        {
            //            ChkPcTableSideCPlate.Checked = true;
            //            ddlPcTableSideCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableSideCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPcTableSideCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //5
            //        if (category == "Door C Plate")
            //        {
            //            ChkPcTableDoorCPlate.Checked = true;
            //            ddlPcTableDoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableDoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPcTableDoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //6
            //        if (category == "Front Door with stiffeners")
            //        {
            //            ChkPcTableFrontDoor.Checked = true;
            //            ddlPcTableFrontDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableFrontDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //7                 
            //        if (category == "Rear Door with stiffeners")
            //        {
            //            ChkPcTableRearDoor.Checked = true;
            //            ddlPcTableRearDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableRearDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //8                 
            //        if (category == "Lock")
            //        {
            //            ChkPcTableLock.Checked = true;
            //            ddlPcTableLockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPcTableLockcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //9
            //        if (category == "Cable Support Angle")
            //        {
            //            ChkPcTableCableSupportAngle.Checked = true;
            //            txtPcTableCableSupportAnglecat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //10                    
            //        if (category == "Horizontal Partition")
            //        {
            //            ChkPcTableHorizontalPartition.Checked = true;
            //            ddlPcTableHorizontalPartitioncat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableHorizontalPartitioncat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //11                  
            //        if (category == "Sliding Keyboard drawer with telescopic rails")
            //        {
            //            ChkPcTableSlidingKeyboarddrawer.Checked = true;
            //            ddlPcTableSlidingKeyboarddrawercat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableSlidingKeyboarddrawercat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //12                    
            //        if (category == "Sliding CPU drawer with telescopic rails")
            //        {
            //            ChkPcTableSlidingCPUdrawer.Checked = true;
            //            ddlPcTableSlidingCPUdrawercat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableSlidingCPUdrawercat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }

            //        //13
            //        if (category == "Monitor mounting bracket with tilt adjustment")
            //        {
            //            ChkPcTableMonitomountingbracket.Checked = true;
            //            ddlPcTableMonitomountingbracketcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableMonitomountingbracketcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //14
            //        if (category == "Lifting I-Bolt")
            //        {
            //            ChkPcTableLiftingIBolt.Checked = true;
            //            txtPcTableLiftingIBoltcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //15
            //        if (category == "Base")
            //        {
            //            ChkPcTableBase.Checked = true;
            //            ddlPcTableBasecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableBasecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPcTableBasecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //16
            //        if (category == "Anti-vibration pad")
            //        {
            //            ChkPcTableAntivibrationpad.Checked = true;
            //            ddlPcTableAntivibrationpadcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //17
            //        if (category == "Transparent Door")
            //        {
            //            ChkPcTableTransparentDoor.Checked = true;
            //            ddlPcTableTransparentDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPcTableTransparentDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPcTableTransparentDoorcat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //18
            //        if (category == "Caster Wheel")
            //        {
            //            ChkPcTableCasterWheel.Checked = true;
            //            txtPcTableCasterWheelcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPcTableCasterWheelcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            txtPcTableCasterWheelcat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtPcTableCasterWheelcat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //19
            //        if (category == "Filters")
            //        {
            //            ChkPcTableFilters.Checked = true;
            //            ddlPcTableFilterscat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPcTableFilterscat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //20
            //        if (category == "Powder Coating Shade")
            //        {
            //            ChkPcTablePowderCoatingShade.Checked = true;
            //            ddlPcTablePowderCoatingShadecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPcTablePowderCoatingShadecat2.Text = dtall.Rows[i]["category2"].ToString();

            //            if (dtall.Rows[i]["category1"].ToString() == "Specify")
            //            {
            //                txtPcTablePowderCoatingShadecat2.Visible = true;
            //            }
            //        }
            //        //21
            //        if (category == "Fan")
            //        {
            //            ChkPcTablefan.Checked = true;
            //            ddlPcTablefancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPcTablefancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //22
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkPcTableJointlesspolyurethanefoamed.Checked = true;
            //        }
            //        //23
            //        if (category == "Any additional component")
            //        {
            //            ChkPcTableAnyadditionalcomponent.Checked = true;
            //            txtPcTableAnyadditionalcomponentcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "PRINTER TABLE")
            //{
            //    ddlConstype.Text = "PRINTER TABLE";

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Welded Main Body")
            //        {
            //            ChkPrinterTableWeldedMainBody.Checked = true;
            //            ddlPrinterTableWeldedMainBodycat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPrinterTableWeldedMainBodycat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //2
            //        if (category == "Gland Plate")
            //        {
            //            ChkPrinterGlandPlate.Checked = true;
            //            ddlPrinterGlandPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPrinterGlandPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //3
            //        if (category == "Component Mtg. Plate")
            //        {
            //            ChkPrinterTableComponentMtgPlate.Checked = true;
            //            ddlPrinterTableComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPrinterTableComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPrinterTableComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //4
            //        if (category == "Side C Plate")
            //        {
            //            ChkPrinterTableSideCPlate.Checked = true;
            //            ddlPrinterTableSideCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPrinterTableSideCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPrinterTableSideCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //5
            //        if (category == "Door C Plate")
            //        {
            //            ChkPrinterTableDoorCPlate.Checked = true;
            //            ddlPrinterTableDoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPrinterTableDoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPrinterTableDoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //6
            //        if (category == "Front Door with stiffeners")
            //        {
            //            ChkPrinterTableFrontDoor.Checked = true;
            //            ddlPrinterTableFrontDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPrinterTableFrontDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //7                 
            //        if (category == "Rear Door with stiffeners")
            //        {
            //            ChkPrinterTableRearDoor.Checked = true;
            //            ddlPrinterTableRearDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPrinterTableRearDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //8                 
            //        if (category == "Door Stiffener")
            //        {
            //            ChkPrinterTableDoorStiffener.Checked = true;
            //        }
            //        //9
            //        if (category == "Lock")
            //        {
            //            ChkPrinterTableLock.Checked = true;
            //            ddlPrinterTableLockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPrinterTableLockcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //10
            //        if (category == "Cable Support Angle")
            //        {
            //            ChkPrinterTableCableSupportAngle.Checked = true;
            //            txtPrinterTableCableSupportAnglecat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //11                    
            //        if (category == "Horizontal Partition")
            //        {
            //            ChkPrinterTableHorizontalPartition.Checked = true;
            //            ddlPrinterTableHorizontalPartitioncat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPrinterTableHorizontalPartitioncat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //12                   
            //        if (category == "Sliding drawer with telescopic rails")
            //        {
            //            ChkPrinterTableSlidingdrawer.Checked = true;
            //            ddlPrinterTableSlidingdrawercat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPrinterTableSlidingdrawercat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //13                    
            //        if (category == "Lifting I-Bolt")
            //        {
            //            ChkPrinterTableLiftingIBolt.Checked = true;
            //            txtPrinterTableLiftingIBoltcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //14
            //        if (category == "Base")
            //        {
            //            ChkPrinterTableBase.Checked = true;
            //            ddlPrinterTableBasecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPrinterTableBasecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPrinterTableBasecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //15
            //        if (category == "Anti-vibration pad")
            //        {
            //            ChkPrinterTableAntivibrationpad.Checked = true;
            //            ddlPrinterTableAntivibrationpadcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //16
            //        if (category == "Transparent Door")
            //        {
            //            ChkPrinterTableTransparentDoor.Checked = true;
            //            ddlPrinterTableTransparentDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlPrinterTableTransparentDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlPrinterTableTransparentDoorcat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //17
            //        if (category == "Caster Wheel")
            //        {
            //            ChkPrinterTableCasterWheel.Checked = true;
            //            txtPrinterTableCasterWheelcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPrinterTableCasterWheelcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            txtPrinterTableCasterWheelcat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtPrinterTableCasterWheelcat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //18
            //        if (category == "Filters")
            //        {
            //            ChkPrinterTableFilters.Checked = true;
            //            ddlPrinterTableFilterscat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPrinterTableFilterscat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //19
            //        if (category == "Powder Coating Shade")
            //        {
            //            ChkPrinterTablePowderCoatingShade.Checked = true;
            //            ddlPrinterTablePowderCoatingShadecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPrinterTablePowderCoatingShadecat2.Text = dtall.Rows[i]["category2"].ToString();

            //            if (dtall.Rows[i]["category1"].ToString() == "Specify")
            //            {
            //                txtPrinterTablePowderCoatingShadecat2.Visible = true;
            //            }
            //        }
            //        //20
            //        if (category == "Fan")
            //        {
            //            ChkPrinterTablefan.Checked = true;
            //            ddlPrinterTablefancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtPrinterTablefancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //21
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkPrinterTableJointlesspolyurethanefoamed.Checked = true;
            //        }
            //        //22
            //        if (category == "Any additional component")
            //        {
            //            ChkPrinterTableAnyadditionalcomponent.Checked = true;
            //            txtPrinterTableAnyadditionalcomponentcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "Single Piece Desk")
            //{
            //    ddlConstype.Text = "Single Piece Desk";

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Welded Main Body")
            //        {
            //            ChkSinglePieceWeldedMainBody.Checked = true;
            //            ddlSinglePieceWeldedMainBodycat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //2
            //        if (category == "Gland Plate")
            //        {
            //            ChkSinglePieceGlandPlate.Checked = true;
            //            ddlSinglePieceGlandPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlSinglePieceGlandPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //3
            //        if (category == "Component Mtg. Plate")
            //        {
            //            ChkSinglePieceComponentMtgPlate.Checked = true;
            //            ddlSinglePieceComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlSinglePieceComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlSinglePieceComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //4
            //        if (category == "Side C Plate")
            //        {
            //            ChkSinglePieceSideCPlate.Checked = true;
            //            ddlSinglePieceSideCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlSinglePieceSideCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlSinglePieceSideCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //5
            //        if (category == "Door C Plate")
            //        {
            //            ChkSinglePieceDoorCPlate.Checked = true;
            //            ddlSinglePieceDoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlSinglePieceDoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlSinglePieceDoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //6
            //        if (category == "Front Door")
            //        {
            //            ChkSinglePieceFrontDoor.Checked = true;
            //            ddlSinglePieceFrontDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlSinglePieceFrontDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //7                 
            //        if (category == "Rear Door")
            //        {
            //            ChkSinglePieceRearDoor.Checked = true;
            //            ddlSinglePieceRearDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlSinglePieceRearDoorcat2.Text = dtall.Rows[i]["category2"].ToString();

            //        }
            //        //8
            //        if (category == "Door Stiffener")
            //        {
            //            ChkSinglePieceDoorStiffener.Checked = true;
            //        }
            //        //9
            //        if (category == "Lock")
            //        {
            //            ChkSinglePieceLock.Checked = true;
            //            ddlSinglePieceLockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtSinglePieceLockcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //10                    
            //        if (category == "Cable Support Angle")
            //        {
            //            ChkSinglePieceCableSupportAngle.Checked = true;
            //            txtSinglePieceCableSupportAnglecat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //11                   
            //        if (category == "Sliding Keyboard drawer with telescopic rails")
            //        {
            //            ChkSinglePieceSlidingKeyboarddrawertelescopicrails.Checked = true;
            //            ddlSinglePieceSlidingKeyboarddrawertelescopicrailscat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlSinglePieceSlidingKeyboarddrawertelescopicrailscat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //12                   
            //        if (category == "Sliding drawer with telescopic rails")
            //        {
            //            ChkSinglePieceSlidingdrawerwithtelescopicrails.Checked = true;
            //            ddlSinglePieceSlidingdrawerwithtelescopicrailscat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlSinglePieceSlidingdrawerwithtelescopicrailscat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //13                   
            //        if (category == "Monitor mounting arrangement")
            //        {
            //            ChkSinglePieceMonitormountingarrangement.Checked = true;
            //            ddlSinglePieceMonitormountingarrangementcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlSinglePieceMonitormountingarrangementcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //14                    
            //        if (category == "Base")
            //        {
            //            ChkSinglePieceBase.Checked = true;
            //            ddlSinglePieceBasecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlSinglePieceBasecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlSinglePieceBasecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //15
            //        if (category == "Anti-vibration pad")
            //        {
            //            ChkSinglePieceAntivibrationpad.Checked = true;
            //            ddlSinglePieceAntivibrationpadcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //16                    
            //        if (category == "Transparent Door")
            //        {
            //            ChkSinglePieceTransparentDoor.Checked = true;
            //            ddlSinglePieceTransparentDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlSinglePieceTransparentDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlSinglePieceTransparentDoorcat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //17                    
            //        if (category == "Caster Wheel")
            //        {
            //            ChkSinglePieceCasterWheel.Checked = true;
            //            txtSinglePieceCasterWheelcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtSinglePieceCasterWheelcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            txtSinglePieceCasterWheelcat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtSinglePieceCasterWheelcat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //18
            //        if (category == "Filters")
            //        {
            //            ChkSinglePieceFilters.Checked = true;
            //            ddlSinglePieceFilterscat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtSinglePieceFilterscat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //19
            //        if (category == "Fan")
            //        {
            //            ChkSinglePiecefan.Checked = true;
            //            ddlSinglePiecefancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtSinglePiecefancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //20
            //        if (category == "Powder Coating Shade")
            //        {
            //            ChkSinglePiecePowderCoatingShade.Checked = true;
            //            ddlSinglePiecePowderCoatingShadecat1.Text = dtall.Rows[i]["category1"].ToString();

            //            if (ddlSinglePiecePowderCoatingShadecat1.Text == "Specify")
            //            {
            //                txtSinglePiecePowderCoatingShadecat2.Visible = true;
            //                txtSinglePiecePowderCoatingShadecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            }
            //            else
            //            {
            //                txtSinglePiecePowderCoatingShadecat2.Visible = false;
            //            }
            //        }
            //        //21
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkSinglePieceJointlesspolyurethanefoamed.Checked = true;
            //        }
            //        //22
            //        if (category == "Any additional component")
            //        {
            //            ChkSinglePieceAnyadditionalcomponent.Checked = true;
            //            txtSinglePieceAnyadditionalcomponentcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "Three Piece Desk")
            //{
            //    ddlConstype.Text = "Three Piece Desk";

            //    for (int i = 1; i < dtall.Rows.Count; i++)
            //    {
            //        string category = dtall.Rows[i]["categoryname"].ToString();

            //        //1
            //        if (category == "Welded Main Body")
            //        {
            //            ChkThreePieceWeldedMainBody.Checked = true;
            //            ddlThreePieceWeldedMainBodycat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //2
            //        if (category == "Gland Plate")
            //        {
            //            ChkThreePieceGlandPlate.Checked = true;
            //            ddlThreePieceGlandPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlThreePieceGlandPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //3
            //        if (category == "Component Mtg.Plate")
            //        {
            //            ChkThreePieceComponentMtgPlate.Checked = true;
            //            ddlThreePieceComponentMtgPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlThreePieceComponentMtgPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlThreePieceComponentMtgPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //4
            //        if (category == "Side C Plate")
            //        {
            //            ChkThreePieceSideCPlate.Checked = true;
            //            ddlThreePieceSideCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlThreePieceSideCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlThreePieceSideCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //5
            //        if (category == "Door C Plate")
            //        {
            //            ChkThreePieceDoorCPlate.Checked = true;
            //            ddlThreePieceDoorCPlatecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlThreePieceDoorCPlatecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlThreePieceDoorCPlatecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //6
            //        if (category == "Front Door")
            //        {
            //            ChkThreePieceFrontDoor.Checked = true;
            //            ddlThreePieceFrontDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlThreePieceFrontDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //7                 
            //        if (category == "Rear Door")
            //        {
            //            ChkThreePieceRearDoor.Checked = true;
            //            ddlThreePieceRearDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlThreePieceRearDoorcat2.Text = dtall.Rows[i]["category2"].ToString();

            //        }
            //        //8
            //        if (category == "Door Stiffener")
            //        {
            //            ChkThreePieceDoorStiffener.Checked = true;
            //        }
            //        //9
            //        if (category == "Lock")
            //        {
            //            ChkThreePieceLock.Checked = true;
            //            ddlThreePieceLockcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtThreePieceLockcat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //10                    
            //        if (category == "Cable Support Angle")
            //        {
            //            ChkThreePieceCableSupportAngle.Checked = true;
            //            txtThreePieceCableSupportAnglecat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //11                   
            //        if (category == "Horizontal Partition")
            //        {
            //            ChkThreePieceHorizontalPartition.Checked = true;
            //            ddlThreePieceHorizontalPartitioncat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlThreePieceHorizontalPartitioncat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //12                   
            //        if (category == "Sliding drawer with telescopic rails")
            //        {
            //            ChkThreePieceSlidingdrawerwithtelescopic.Checked = true;
            //            ddlThreePieceSlidingdrawerwithtelescopiccat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlThreePieceSlidingdrawerwithtelescopiccat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //13                   
            //        if (category == "Lifting I-Bolt")
            //        {
            //            ChkThreePieceLiftingIBolt.Checked = true;
            //            txtThreePieceLiftingIBoltcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //14                    
            //        if (category == "Base")
            //        {
            //            ChkThreePieceBase.Checked = true;
            //            ddlThreePieceBasecat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlThreePieceBasecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlThreePieceBasecat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //15
            //        if (category == "Anti-vibration pad")
            //        {
            //            ChkThreePieceAntivibrationpad.Checked = true;
            //            ddlThreePieceAntivibrationpadcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //        //16                    
            //        if (category == "Transparent Door")
            //        {
            //            ChkThreePieceTransparentDoor.Checked = true;
            //            ddlThreePieceTransparentDoorcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            ddlThreePieceTransparentDoorcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            ddlThreePieceTransparentDoorcat3.Text = dtall.Rows[i]["category3"].ToString();
            //        }
            //        //17                    
            //        if (category == "Caster Wheel")
            //        {
            //            ChkThreePieceCasterWheel.Checked = true;
            //            txtThreePieceCasterWheelcat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtThreePieceCasterWheelcat2.Text = dtall.Rows[i]["category2"].ToString();
            //            txtThreePieceCasterWheelcat3.Text = dtall.Rows[i]["category3"].ToString();
            //            txtThreePieceCasterWheelcat4.Text = dtall.Rows[i]["category4"].ToString();
            //        }
            //        //18
            //        if (category == "Filters")
            //        {
            //            ChkThreePieceFilters.Checked = true;
            //            ddlThreePieceFilterscat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtThreePieceFilterscat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //19
            //        if (category == "Fan")
            //        {
            //            ChkThreePiecefan.Checked = true;
            //            ddlThreePiecefancat1.Text = dtall.Rows[i]["category1"].ToString();
            //            txtThreePiecefancat2.Text = dtall.Rows[i]["category2"].ToString();
            //        }
            //        //20
            //        if (category == "Powder Coating Shade")
            //        {
            //            ChkThreePiecePowderCoatingShade.Checked = true;
            //            ddlThreePiecePowderCoatingShadecat1.Text = dtall.Rows[i]["category1"].ToString();

            //            if (ddlThreePiecePowderCoatingShadecat1.Text == "Specify")
            //            {
            //                txtThreePiecePowderCoatingShadecat2.Visible = true;
            //                txtThreePiecePowderCoatingShadecat2.Text = dtall.Rows[i]["category2"].ToString();
            //            }
            //            else
            //            {
            //                txtThreePiecePowderCoatingShadecat2.Visible = false;
            //            }
            //        }
            //        //21
            //        if (category == "Jointless polyurethane foamed in place gasketing")
            //        {
            //            ChkThreePieceJointlesspolyurethanefoamed.Checked = true;
            //        }
            //        //22
            //        if (category == "Any additional component")
            //        {
            //            ChkThreePieceAnyadditionalcomponent.Checked = true;
            //            txtThreePieceAnyadditionalcomponentcat1.Text = dtall.Rows[i]["category1"].ToString();
            //        }
            //    }
            //}

            //if (dtall.Rows[0]["Constructiontype"].ToString() == "Specify")
            //{
            //    ddlConstype.Text = "Specify";
            //    txtconstructiontype.Visible = true;
            //    int temcount = 0;
            //    temcount = dtall.Rows.Count;

            //    //1     
            //    if (temcount > 2 || temcount == 2)
            //    {
            //        txtspecify1cat1.Text = dtall.Rows[1]["categoryname"].ToString();
            //        txtspecify1cat2.Text = dtall.Rows[1]["category1"].ToString();
            //        txtspecify1cat3.Text = dtall.Rows[1]["category2"].ToString();
            //        if (txtspecify1cat1.Text != "")
            //        {
            //            ChkSpecify1.Checked = true;
            //        }
            //    }
            //    //2
            //    if (temcount > 3 || temcount == 3)
            //    {
            //        txtspecify2cat1.Text = dtall.Rows[2]["categoryname"].ToString();
            //        txtspecify2cat2.Text = dtall.Rows[2]["category1"].ToString();
            //        txtspecify2cat3.Text = dtall.Rows[2]["category2"].ToString();
            //        if (txtspecify2cat1.Text != "")
            //        {
            //            ChkSpecify2.Checked = true;
            //        }
            //    }
            //    //3
            //    if (temcount > 4 || temcount == 4)
            //    {
            //        txtspecify3cat1.Text = dtall.Rows[3]["categoryname"].ToString();
            //        txtspecify3cat2.Text = dtall.Rows[3]["category1"].ToString();
            //        txtspecify3cat3.Text = dtall.Rows[3]["category2"].ToString();
            //        if (txtspecify3cat1.Text != "")
            //        {
            //            ChkSpecify3.Checked = true;
            //        }
            //    }
            //    //4
            //    if (temcount > 5 || temcount == 5)
            //    {
            //        txtspecify4cat1.Text = dtall.Rows[4]["categoryname"].ToString();
            //        txtspecify4cat2.Text = dtall.Rows[4]["category1"].ToString();
            //        txtspecify4cat3.Text = dtall.Rows[4]["category2"].ToString();
            //        if (txtspecify4cat1.Text != "")
            //        {
            //            ChkSpecify4.Checked = true;
            //        }
            //    }
            //    //5
            //    if (temcount > 6 || temcount == 6)
            //    {
            //        txtspecify5cat1.Text = dtall.Rows[5]["categoryname"].ToString();
            //        txtspecify5cat2.Text = dtall.Rows[5]["category1"].ToString();
            //        txtspecify5cat3.Text = dtall.Rows[5]["category2"].ToString();
            //        if (txtspecify5cat1.Text != "")
            //        {
            //            ChkSpecify5.Checked = true;
            //        }
            //    }
            //    //6
            //    if (temcount > 7 || temcount == 7)
            //    {
            //        txtspecify6cat1.Text = dtall.Rows[6]["categoryname"].ToString();
            //        txtspecify6cat2.Text = dtall.Rows[6]["category1"].ToString();
            //        txtspecify6cat3.Text = dtall.Rows[6]["category2"].ToString();
            //        if (txtspecify6cat1.Text != "")
            //        {
            //            ChkSpecify6.Checked = true;
            //        }
            //    }
            //    //7                 
            //    if (temcount > 8 || temcount == 8)
            //    {
            //        txtspecify7cat1.Text = dtall.Rows[7]["categoryname"].ToString();
            //        txtspecify7cat2.Text = dtall.Rows[7]["category1"].ToString();
            //        txtspecify7cat3.Text = dtall.Rows[7]["category2"].ToString();
            //        if (txtspecify7cat1.Text != "")
            //        {
            //            ChkSpecify7.Checked = true;
            //        }
            //    }
            //    //8                 
            //    if (temcount > 9 || temcount == 9)
            //    {
            //        txtspecify8cat1.Text = dtall.Rows[8]["categoryname"].ToString();
            //        txtspecify8cat2.Text = dtall.Rows[8]["category1"].ToString();
            //        txtspecify8cat3.Text = dtall.Rows[8]["category2"].ToString();
            //        if (txtspecify8cat1.Text != "")
            //        {
            //            ChkSpecify8.Checked = true;
            //        }
            //    }
            //    //9
            //    if (temcount > 10 || temcount == 10)
            //    {
            //        txtspecify9cat1.Text = dtall.Rows[9]["categoryname"].ToString();
            //        txtspecify9cat2.Text = dtall.Rows[9]["category1"].ToString();
            //        txtspecify9cat3.Text = dtall.Rows[9]["category2"].ToString();
            //        if (txtspecify9cat1.Text != "")
            //        {
            //            ChkSpecify9.Checked = true;
            //        }
            //    }
            //    //10
            //    if (temcount > 11 || temcount == 11)
            //    {
            //        txtspecify10cat1.Text = dtall.Rows[10]["categoryname"].ToString();
            //        txtspecify10cat2.Text = dtall.Rows[10]["category1"].ToString();
            //        txtspecify10cat3.Text = dtall.Rows[10]["category2"].ToString();
            //        if (txtspecify10cat1.Text != "")
            //        {
            //            ChkSpecify10.Checked = true;
            //        }
            //    }
            //    //11                    
            //    if (temcount > 12 || temcount == 12)
            //    {
            //        txtspecify11cat1.Text = dtall.Rows[11]["categoryname"].ToString();
            //        txtspecify11cat2.Text = dtall.Rows[11]["category1"].ToString();
            //        txtspecify11cat3.Text = dtall.Rows[11]["category2"].ToString();
            //        if (txtspecify11cat1.Text != "")
            //        {
            //            ChkSpecify11.Checked = true;
            //        }
            //    }
            //    //12                   
            //    if (temcount > 13 || temcount == 13)
            //    {
            //        txtspecify12cat1.Text = dtall.Rows[12]["categoryname"].ToString();
            //        txtspecify12cat2.Text = dtall.Rows[12]["category1"].ToString();
            //        txtspecify12cat3.Text = dtall.Rows[12]["category2"].ToString();
            //        if (txtspecify12cat1.Text != "")
            //        {
            //            ChkSpecify12.Checked = true;
            //        }
            //    }
            //    //13                    
            //    if (temcount > 14 || temcount == 14)
            //    {
            //        txtspecify13cat1.Text = dtall.Rows[13]["categoryname"].ToString();
            //        txtspecify13cat2.Text = dtall.Rows[13]["category1"].ToString();
            //        txtspecify13cat3.Text = dtall.Rows[13]["category2"].ToString();
            //        if (txtspecify13cat1.Text != "")
            //        {
            //            ChkSpecify13.Checked = true;
            //        }
            //    }

            //    //14
            //    if (temcount > 15 || temcount == 15)
            //    {
            //        txtspecify14cat1.Text = dtall.Rows[14]["categoryname"].ToString();
            //        txtspecify14cat2.Text = dtall.Rows[14]["category1"].ToString();
            //        txtspecify14cat3.Text = dtall.Rows[14]["category2"].ToString();
            //        if (txtspecify14cat1.Text != "")
            //        {
            //            ChkSpecify14.Checked = true;
            //        }
            //    }
            //    //15
            //    if (temcount > 15 || temcount == 16)
            //    {
            //        txtspecify15cat1.Text = dtall.Rows[15]["categoryname"].ToString();
            //        txtspecify15cat2.Text = dtall.Rows[15]["category1"].ToString();
            //        txtspecify15cat3.Text = dtall.Rows[15]["category2"].ToString();
            //        if (txtspecify15cat1.Text != "")
            //        {
            //            ChkSpecify15.Checked = true;
            //        }
            //    }
            //}
        }
        /////////////

        //string query = "";

        //if (Request.QueryString["cdd"] != null)
        //{
        //    query = @"SELECT [QuotationMain].[id],[QuotationMain].[quotationno],[ccode],[partyname],[kindatt],format(date,'dd-MM-yyyy') as [date],[address],[remark],[Toatlamt],[width],
        //    [depth],[height],[base],[canopy],[material],[Constructiontype],[sessionname],[sno],[hsncode],[qty],[rate],[discount],[amount],[description] 
        //    FROM [QuotationMain] join [QuotationData] ON [QuotationMain].id=[QuotationData].[quotationid]  where [QuotationMain].id=" + Decrypt(Request.QueryString["cdd"].ToString());
        //}
        //SqlDataAdapter adp = new SqlDataAdapter(query, con);
        //DataTable dt = new DataTable();
        //adp.Fill(dt);

        //if (dt.Rows.Count > 0)
        //{
        //    #region ConsTypeOpenpanel
        //    if (ddlConstype.Text == "JB Box")
        //    {
        //        PanelType1.Visible = true;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = false;
        //        PanelType13.Visible = false;
        //    }
        //    if (ddlConstype.Text == "WMM-23.5 (AE Box)")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = true;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = false;
        //        PanelType13.Visible = false;
        //    }
        //    if (ddlConstype.Text == "WMM-30 (MCC Box)")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = true;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = false;
        //        PanelType13.Visible = false;
        //    }
        //    if (ddlConstype.Text == "MFS (Modular Floor Standing Enclosure)")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = true;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = false;
        //        PanelType13.Visible = false;
        //    }
        //    if (ddlConstype.Text == "Eco MCC 30mm")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = true;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = false;
        //        PanelType13.Visible = false;
        //    }
        //    if (ddlConstype.Text == "Modular W-Big 43 mm")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = true;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = false;
        //        PanelType13.Visible = false;
        //    }
        //    if (ddlConstype.Text == "Eco Frame 43mm")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = true;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = false;
        //        PanelType13.Visible = false;
        //    }
        //    if (ddlConstype.Text == "PC ENCLOSURE")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = true;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = false;
        //        PanelType13.Visible = false;
        //    }
        //    if (ddlConstype.Text == "PC TABLE")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = true;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = false;
        //        PanelType13.Visible = false;
        //    }
        //    if (ddlConstype.Text == "PRINTER TABLE")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = true;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = false;
        //        PanelType13.Visible = false;
        //    }
        //    if (ddlConstype.Text == "Single Piece Desk")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = true;

        //    }
        //    if (ddlConstype.Text == "Three Piece Desk")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = true;
        //        PanelType13.Visible = false;
        //    }
        //    if (ddlConstype.Text == "Specify")
        //    {
        //        PanelType1.Visible = false;
        //        PanelType2.Visible = false;
        //        PanelType3.Visible = false;
        //        PanelType4.Visible = false;
        //        PanelType5.Visible = false;
        //        PanelType6.Visible = false;
        //        PanelType7.Visible = false;
        //        PanelType8.Visible = false;
        //        PanelType9.Visible = false;
        //        PanelType10.Visible = false;
        //        PanelType11.Visible = false;
        //        PanelType12.Visible = false;
        //        PanelType13.Visible = true;
        //    }
        //    #endregion
        //}
    }

    #endregion Fill Data

    static string regdate = string.Empty;

    protected void GetCompanyDataByName(string ccode)
    {
        string query = "";
        query = "SELECT top 1 [id],[ccode],[cname],[oname1],[oname2],[oname3],[oname4],[oname5],[email1],[mobile1],[billingaddress],[shippingaddress],[gstno],[paymentterm1] FROM [Company] where [isdeleted]=0 and status=0 and ccode='" + ccode.Trim() + "' order by id desc ";

        SqlDataAdapter ad = new SqlDataAdapter(query, con);
        DataTable dt = new DataTable();
        ad.Fill(dt);
        if (dt.Rows.Count > 0)
        {
            HFccode.Value = dt.Rows[0]["ccode"].ToString();
            txtcname.Text = dt.Rows[0]["cname"].ToString();
            List<string> kind = new List<string> { };
            if (!string.IsNullOrEmpty(dt.Rows[0]["oname1"].ToString()))
            {
                kind.Add(dt.Rows[0]["oname1"].ToString());
            }
            if (!string.IsNullOrEmpty(dt.Rows[0]["oname2"].ToString()))
            {
                kind.Add(dt.Rows[0]["oname2"].ToString());
            }
            if (!string.IsNullOrEmpty(dt.Rows[0]["oname3"].ToString()))
            {
                kind.Add(dt.Rows[0]["oname3"].ToString());
            }
            if (!string.IsNullOrEmpty(dt.Rows[0]["oname4"].ToString()))
            {
                kind.Add(dt.Rows[0]["oname4"].ToString());
            }
            if (!string.IsNullOrEmpty(dt.Rows[0]["oname5"].ToString()))
            {
                kind.Add(dt.Rows[0]["oname5"].ToString());
            }
            ddlkindatt.DataSource = kind;
            ddlkindatt.DataBind();
            ddlkindatt.Items.Insert(0, "Select");
            ddlpaymentterm.Text = dt.Rows[0]["paymentterm1"].ToString();
            txtshippingaddress.Text = dt.Rows[0]["billingaddress"].ToString();
        }
        else
        {
            ddlkindatt.DataSource = null;
            ddlkindatt.DataBind();
            ddlkindatt.Items.Insert(0, "Select");
        }
    }

    public string Decrypt(string cipherText)
    {
        string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        cipherText = cipherText.Replace(" ", "+");
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
        }
        return cipherText;
    }

    static string ComCode = string.Empty;

    //protected string GenerateCode() commented on 18_12_23
    //{
    //    string FinYear = null;
    //    string FinFullYear = null;
    //    if (DateTime.Today.Month > 3)
    //    {
    //        FinYear = DateTime.Today.AddYears(1).ToString("yy");
    //        FinFullYear = DateTime.Today.AddYears(1).ToString("yyyy");
    //    }
    //    else
    //    {
    //        var finYear = DateTime.Today.AddYears(1).ToString("yy");
    //        FinYear = (Convert.ToInt32(finYear) - 1).ToString();

    //        var finfYear = DateTime.Today.AddYears(1).ToString("yyyy");
    //        FinFullYear = (Convert.ToInt32(finfYear) - 1).ToString();
    //    }
    //    string previousyear = (Convert.ToDecimal(FinFullYear) - 1).ToString();
    //    string strQuotationNumber = "";
    //    string fY = previousyear.ToString() + "-" + FinYear;
    //     string strSelect = @"select ISNULL(MAX(quotationno), '') AS maxno from QuotationMain where quotationno like '%" + fY + "%'";
    //    //string strSelect = @"select TOP 1 quotationno AS maxno from QuotationMain where quotationno like '%" + fY + "%' ORDER BY ID DESC";
    //    SqlCommand cmd = new SqlCommand();
    //    cmd.Connection = con;
    //    cmd.CommandType = CommandType.Text;
    //    cmd.CommandText = strSelect;
    //    con.Open();
    //    string result = cmd.ExecuteScalar().ToString();
    //    con.Close();
    //    if (result != "")
    //    {
    //        string stringBeforeChar = result.Substring(8, result.IndexOf("-"));
    //        int numbervalue = Convert.ToInt32(stringBeforeChar);
    //         if (numbervalue < 9)
    //        {
    //            numbervalue = numbervalue + 1;
    //            strQuotationNumber = fY + "/" + "000" + numbervalue + "-" + "0";  // if no is less than 9
    //            strQuotationNumber = fY + "/" + "00" + numbervalue + "-" + "0";  // if no  9
    //            txQutno.Text = strQuotationNumber;
    //        }
    //        else if (numbervalue < 100)
    //        {
    //            numbervalue = numbervalue + 1;
    //            strQuotationNumber = fY + "/" + "00" + numbervalue + "-" + "0";
    //            txQutno.Text = strQuotationNumber;
    //        }
    //        //else if (numbervalue > 100)
    //        //{
    //        //    numbervalue = numbervalue + 1;
    //        //    strQuotationNumber = fY + "/" + "0" + numbervalue + "-" + "0";
    //        //    txQutno.Text = strQuotationNumber;
    //        //}
    //        //if (numbervalue < 9)
    //        //{
    //        //    numbervalue = numbervalue + 1;
    //        //    strQuotationNumber = fY + "/" + "000" + numbervalue + "-" + "0";
    //        //    txQutno.Text = strQuotationNumber;
    //        //}
    //        else if (numbervalue >= 100)
    //        {
    //            numbervalue = numbervalue + 1;
    //            strQuotationNumber = fY + "/" + "0" + numbervalue + "-" + "0";
    //            txQutno.Text = strQuotationNumber;
    //        }
    //    }
    //    else
    //    {
    //        strQuotationNumber = fY + "/" + "0001-0";
    //        txQutno.Text = strQuotationNumber;
    //    }
    //    return strQuotationNumber;
    //}   

    static string ComCodeUpdate = string.Empty; static string visitingcardPath = string.Empty;
    protected void btnadd_Click(object sender, EventArgs e)
    {

    }

    protected void btnreset_Click(object sender, EventArgs e)
    {
        Response.Redirect("QuotationList.aspx");
    }

    protected void txtcname_TextChanged(object sender, EventArgs e)
    {
        GetCompanyDataByName(txtcname.Text);
    }

    [System.Web.Script.Services.ScriptMethod()]
    [System.Web.Services.WebMethod]
    public static List<string> GetCompanyList(string prefixText, int count)
    {
        return AutoFillCompanyName(prefixText);
    }

    public static List<string> AutoFillCompanyName(string prefixText)
    {
        using (SqlConnection con = new SqlConnection())
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlCommand com = new SqlCommand())
            {
                com.CommandText = "Select DISTINCT [cname] from [Company] where " + "cname like @Search + '%' and status=0 and [isdeleted]=0";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> countryNames = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        countryNames.Add(sdr["cname"].ToString());
                    }
                }
                con.Close();
                return countryNames;
            }
        }
    }

    [System.Web.Script.Services.ScriptMethod()]
    [System.Web.Services.WebMethod]
    public static List<string> GetValidityofofferList(string prefixText, int count)
    {
        return AutoFillValidityofoffer(prefixText);
    }

    public static List<string> AutoFillValidityofoffer(string prefixText)
    {
        using (SqlConnection con = new SqlConnection())
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlCommand com = new SqlCommand())
            {
                com.CommandText = "Select DISTINCT [validityofoffer] from [QuotationMainFooter] where " + "validityofoffer like @Search + '%'";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> validityofoffer = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        validityofoffer.Add(sdr["validityofoffer"].ToString());
                    }
                }
                con.Close();
                return validityofoffer;
            }
        }
    }

    [System.Web.Script.Services.ScriptMethod()]
    [System.Web.Services.WebMethod]
    public static List<string> GetDeliveryperiodList(string prefixText, int count)
    {
        return AutoFillDeliveryperiod(prefixText);
    }

    public static List<string> AutoFillDeliveryperiod(string prefixText)
    {
        using (SqlConnection con = new SqlConnection())
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlCommand com = new SqlCommand())
            {
                com.CommandText = "Select DISTINCT [deliveryperiod] from [QuotationMainFooter] where " + "deliveryperiod like @Search + '%'";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> deliveryperiod = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        deliveryperiod.Add(sdr["deliveryperiod"].ToString());
                    }
                }
                con.Close();
                return deliveryperiod;
            }
        }
    }

    [System.Web.Script.Services.ScriptMethod()]
    [System.Web.Services.WebMethod]
    public static List<string> GetTransportationList(string prefixText, int count)
    {
        return AutoFillTransportation(prefixText);
    }

    public static List<string> AutoFillTransportation(string prefixText)
    {
        using (SqlConnection con = new SqlConnection())
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlCommand com = new SqlCommand())
            {
                com.CommandText = "Select DISTINCT [transportation] from [QuotationMainFooter] where " + "transportation like @Search + '%'";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> deliveryperiod = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        deliveryperiod.Add(sdr["transportation"].ToString());
                    }
                }
                con.Close();
                return deliveryperiod;
            }
        }
    }

    [System.Web.Script.Services.ScriptMethod()]
    [System.Web.Services.WebMethod]
    public static List<string> GetStandaradpkgList(string prefixText, int count)
    {
        return AutoFillStandaradpkg(prefixText);
    }

    public static List<string> AutoFillStandaradpkg(string prefixText)
    {
        using (SqlConnection con = new SqlConnection())
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlCommand com = new SqlCommand())
            {
                com.CommandText = "Select DISTINCT [standardpackaging] from [QuotationMainFooter] where " + "standardpackaging like @Search + '%'";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> standardpackaging = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        standardpackaging.Add(sdr["standardpackaging"].ToString());
                    }
                }
                con.Close();
                return standardpackaging;
            }
        }
    }

    [System.Web.Script.Services.ScriptMethod()]
    [System.Web.Services.WebMethod]
    public static List<string> GetSpecialpkgList(string prefixText, int count)
    {
        return AutoFillspecialpackaging(prefixText);
    }

    public static List<string> AutoFillspecialpackaging(string prefixText)
    {
        using (SqlConnection con = new SqlConnection())
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlCommand com = new SqlCommand())
            {
                com.CommandText = "Select DISTINCT [specialpackaging] from [QuotationMainFooter] where " + "specialpackaging like @Search + '%'";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> specialpackaging = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        specialpackaging.Add(sdr["specialpackaging"].ToString());
                    }
                }
                con.Close();
                return specialpackaging;
            }
        }
    }

    [System.Web.Script.Services.ScriptMethod()]
    [System.Web.Services.WebMethod]
    public static List<string> GetinspectionList(string prefixText, int count)
    {
        return AutoFillinspection(prefixText);
    }

    public static List<string> AutoFillinspection(string prefixText)
    {
        using (SqlConnection con = new SqlConnection())
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlCommand com = new SqlCommand())
            {
                com.CommandText = "Select DISTINCT [inspection] from [QuotationMainFooter] where " + "inspection like @Search + '%'";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> inspection = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        inspection.Add(sdr["inspection"].ToString());
                    }
                }
                con.Close();
                return inspection;
            }
        }
    }

    protected void btnaddperticular_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = true;
        this.modelprofile.Show();
    }

    protected void btnSubmitjbbox_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnSubmitWMM23_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnSumbitWMM30_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnSubmitMFS_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnSubmitEcoMCC_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnSubmitModular_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnEcoFrame_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnSubmitPCEncShopFloorStanding_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnSubmitSpecify_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }

    static string constype = string.Empty; static int validation;
    protected void btnsubmit_Click(object sender, EventArgs e)
    {
        if (ddltaxation.SelectedValue.ToString() != "0")
        {
            if (dgvQuatationDtl.Rows.Count > 0)
            {

                string Action = "";
                if (Request.QueryString["cdd"] == null)
                {
                    Action = "insert";
                }
                else
                {
                    //Action = "update";
                    Action = "insert";
                }
                #region Insert into QuotationMain
                string filename1 = "", filename2 = "";
                if (FileUploadrefdoc.HasFile)
                {
                    string filePath = FileUploadrefdoc.PostedFile.FileName;
                    filename1 = Path.GetFileName(filePath);
                    string[] avc = filename1.Split('.');
                    string ext = Path.GetExtension(filename1);
                    string contenttype = String.Empty;
                    string timest = DateTime.Now.ToString("ddMMyyyyhhmmssfff");
                    filename2 = avc[0] + timest + ext;
                    FileUploadrefdoc.SaveAs(Server.MapPath("~/RefDocument/") + filename2);
                }

                if (FileUploadrefdoc.HasFile == false && oldfile1 != "" && oldfile2 != "")
                {
                    filename1 = oldfile1; filename2 = oldfile2;
                }

                SqlCommand cmdquotation = new SqlCommand("SP_QuotationIns", con);
                cmdquotation.CommandType = CommandType.StoredProcedure;
                cmdquotation.Parameters.AddWithValue("@action", Action);
                cmdquotation.Parameters.AddWithValue("@partyname", Request.Form[txtcname.UniqueID].ToString());
                cmdquotation.Parameters.AddWithValue("@ccode", HFccode.Value);
                cmdquotation.Parameters.AddWithValue("@EnqID", HFEnqID.Value);
                cmdquotation.Parameters.AddWithValue("@kindatt", ddlkindatt.Text);
                cmdquotation.Parameters.AddWithValue("@date", Request.Form[txtdate.UniqueID].ToString());
                cmdquotation.Parameters.AddWithValue("@address", txtshippingaddress.Text);
                cmdquotation.Parameters.AddWithValue("@remark", txtremark.Text);
                cmdquotation.Parameters.AddWithValue("@Toatlamt", txtAmt1.Text);
                cmdquotation.Parameters.AddWithValue("@width", txtwidth.Text);
                cmdquotation.Parameters.AddWithValue("@depth", txtdepth.Text);
                cmdquotation.Parameters.AddWithValue("@height", txtheight.Text);
                cmdquotation.Parameters.AddWithValue("@base", txtbase.Text);
                cmdquotation.Parameters.AddWithValue("@canopy", txtcanopy.Text);
                if (Request.QueryString["cdd"] == null)
                {
                    cmdquotation.Parameters.AddWithValue("@material", ddlmaterial.Text);
                }
                else
                {
                    cmdquotation.Parameters.AddWithValue("@material", Session["material"].ToString());
                }
                cmdquotation.Parameters.AddWithValue("@specifymaterial", txtSpecifymaterial.Text);
                cmdquotation.Parameters.AddWithValue("@Maincat", "Enclosure For Control Panel");

                if (ddlpaymentterm.Text == "Specify")
                {
                    cmdquotation.Parameters.AddWithValue("@paymentterm1", ddlpaymentterm.SelectedItem.Text);
                    cmdquotation.Parameters.AddWithValue("@paymentterm2", txtPTSpecify.Text);
                }
                else
                {
                    cmdquotation.Parameters.AddWithValue("@paymentterm1", ddlpaymentterm.SelectedItem.Text);
                }
                cmdquotation.Parameters.AddWithValue("@Specifyconstruction", txtconstructiontype.Text);
                //cmdquotation.Parameters.AddWithValue("@paymentterm3", txtpayment3.Text);
                //cmdquotation.Parameters.AddWithValue("@paymentterm4", txtpayment4.Text);
                //cmdquotation.Parameters.AddWithValue("@paymentterm5", txtpayment5.Text);

                cmdquotation.Parameters.AddWithValue("@validityofoffer", ddlvalidityofoffer.SelectedItem.Text);
                cmdquotation.Parameters.AddWithValue("@deliveryperiod", ddldeliveryperiod.SelectedItem.Text);
                cmdquotation.Parameters.AddWithValue("@standardpackaging", ddlStandardPacking.SelectedItem.Text);
                cmdquotation.Parameters.AddWithValue("@specialpackaging", ddlSpecialPacking.SelectedItem.Text);
                cmdquotation.Parameters.AddWithValue("@inspection", ddlinspection.SelectedItem.Text);
                cmdquotation.Parameters.AddWithValue("@transportation", ddltransportation.SelectedItem.Text);
                cmdquotation.Parameters.AddWithValue("@filename1", filename1);
                cmdquotation.Parameters.AddWithValue("@filename2", filename2);

                if (Request.QueryString["cdd"] == null)
                {
                    Thread.Sleep(5000);
                    //GenerateCode();
                    //quotationno = Request.Form[txQutno.UniqueID].ToString();
                    quotationno = GenerateCode();
                    cmdquotation.Parameters.AddWithValue("@quotationno", quotationno);
                }
                else
                {
                    quotationno = Request.Form[txQutno.UniqueID].ToString();

                    cmdquotation.Parameters.AddWithValue("@id", Convert.ToInt32(Decrypt(Request.QueryString["cdd"].ToString())));
                    //cmdquotation.Parameters.AddWithValue("@quotationno", Request.Form[txQutno.UniqueID].ToString());
                    string quot11 = Request.Form[txQutno.UniqueID].ToString();
                    if (!string.IsNullOrEmpty(quot11))
                    {
                        //string[] qt22 = quot11.Split('/');
                        //string[] qt33 = qt22[1].Split('-');

                        //SqlDataAdapter quotad = new SqlDataAdapter("select top 1 quotationno from QuotationMain where quotationno like '" + qt33[0] + "-" + "%' order by id desc", con);
                        //DataTable quotdt = new DataTable();
                        // quotad.Fill(quotdt);

                        //if (quotdt.Rows.Count > 0)
                        //{
                        //  string quot = quotdt.Rows[0]["quotationno"].ToString();
                        //  string[] qt = quot.Split('/');
                        // if (qt[1].ToString() == "2022-23")
                        // {
                        //   string[] qt1 = qt[0].Split('-');
                        //   int nextquot = (Convert.ToInt32(qt1[1])) + 1;
                        //quotationno = qt1[0] + "-" + nextquot.ToString() + "/" + qt[0].ToString();
                        //   quotationno = qt[0].ToString() + "/" + qt1[0] + "-" + nextquot.ToString();
                        //  }
                        //else {
                        //  string[] qt1 = qt[1].Split('-');
                        //  int nextquot = (Convert.ToInt32(qt1[1])) + 1;
                        //quotationno = qt1[0] + "-" + nextquot.ToString() + "/" + qt[0].ToString();
                        //quotationno = qt[0].ToString() + "/" + qt1[0] + "-" + nextquot.ToString();
                        // }
                        //}

                        string[] qt = quot11.Split('/');
                        string[] qt1 = qt[1].Split('-');
                        int nextquot = (Convert.ToInt32(qt1[1])) + 1;
                        quotationno = qt[0].ToString() + "/" + qt1[0] + "-" + nextquot.ToString();
                        cmdquotation.Parameters.AddWithValue("@quotationno", quotationno);
                    }
                }

                if (ddlConstype.SelectedItem.Text == "JB Box")
                {
                    constype = "JBboxdata";
                }
                if (ddlConstype.SelectedItem.Text == "WMM-22")
                {
                    constype = "WMM23AEBox";
                }
                if (ddlConstype.SelectedItem.Text == "WMM-30 (MCC Box)")
                {
                    constype = "WMM30MCCBox";
                }
                if (ddlConstype.SelectedItem.Text == "MFS (Modular Floor Standing Enclosure)")
                {
                    constype = "MFS";
                }
                if (ddlConstype.SelectedItem.Text == "Eco MCC 30mm")
                {
                    constype = "EcoMCC";
                }
                if (ddlConstype.SelectedItem.Text == "Modular W-Big 43 mm")
                {
                    constype = "Modular";
                }
                if (ddlConstype.SelectedItem.Text == "Eco Frame 43mm")
                {
                    constype = "EcoFrame";
                }
                if (ddlConstype.SelectedItem.Text == "PC ENCLOSURE")
                {
                    if (rdlpcenclosure.SelectedItem.Text == "Shop Floor PC Enclosure Standing")
                    {
                        constype = "ShopFloorPCEnclosureStanding";
                    }
                    if (rdlpcenclosure.SelectedItem.Text == "PC Enclosure ECO-Standing")
                    {
                        constype = "PCEnclosureECOStanding";
                    }
                    if (rdlpcenclosure.SelectedItem.Text == "PC Enclosure ECO-Sitting")
                    {
                        constype = "PCEnclosureECOSitting";
                    }
                }
                if (ddlConstype.SelectedItem.Text == "PC TABLE")
                {
                    constype = "PC TABLE";
                }
                if (ddlConstype.SelectedItem.Text == "PRINTER TABLE")
                {
                    constype = "PRINTER TABLE";
                }
                if (ddlConstype.SelectedItem.Text == "Single Piece Desk")
                {
                    constype = "Single Piece Desk";
                }
                if (ddlConstype.SelectedItem.Text == "Three Piece Desk")
                {
                    constype = "Three Piece Desk";
                }
                if (ddlConstype.SelectedItem.Text == "Specify")
                {
                    constype = "Specify";
                }

                if (Request.QueryString["cdd"] != null)
                {
                    constype = Session["Constructiontype"].ToString();
                }

                cmdquotation.Parameters.AddWithValue("@Constructiontype", constype);

                cmdquotation.Parameters.AddWithValue("@Taxation", ddltaxation.SelectedValue.ToString());

                cmdquotation.Parameters.AddWithValue("@Currency", ddlCurrency.SelectedItem.Text);

                cmdquotation.Parameters.AddWithValue("@sessionname", Session["name"].ToString());

                cmdquotation.Parameters.Add("@myquotationid", SqlDbType.Int).Direction = ParameterDirection.Output;

                //Thread.Sleep(8000);
                //await Task.Delay(8000);
                con.Open();
                cmdquotation.ExecuteNonQuery();
                con.Close();

                if (Request.QueryString["Ccode"] != null)
                {
                    if (dgvQuatationDtl.Rows.Count > 0)
                    {
                        foreach (GridViewRow g1 in dgvQuatationDtl.Rows)
                        {
                            Label lbldescription = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lbldescription");
                            //TextBox lbldescription = (TextBox)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lbldescription");
                            Label lbltottax = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lbltottax");
                            Label lblprice = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lblprice");
                            Label lblqty = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lblqty");
                            Label lbltxtdiscount = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lbltxtdiscount");
                            Label lbltotalamt = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lbltotalamt");

                            string Description = lbldescription.Text.Replace("<br>", "<br>");
                            string HSN = g1.Cells[2].Text;
                            string Qty = lblqty.Text;
                            string Price = lblprice.Text;
                            string Discount = lbltxtdiscount.Text;
                            string CGST = g1.Cells[5].Text;
                            string SGST = g1.Cells[6].Text;
                            string IGST = g1.Cells[7].Text;
                            string CGSTAmt = g1.Cells[8].Text;
                            string SGSTAmt = g1.Cells[9].Text;
                            string IGSTamt = g1.Cells[10].Text;
                            string TotalTax = lbltottax.Text;
                            string TotalAmount = lbltotalamt.Text;

                            SqlCommand cmdquotdata = new SqlCommand(@"INSERT INTO [QuotationData]([quotationid],[quotationno]
                    ,[description],[hsncode],[qty],[rate],[CGST],[SGST],[IGST],[CGSTamt],[SGSTamt],[IGSTamt],[totaltax],[discount],[amount]) 
                     VALUES (" + quotationid + ",'" + quotationno + "','" + Description + "','" + HSN + "'," +
                             "'" + Qty + "','" + Price + "','" + CGST + "','" + SGST + "','" + IGST + "'," +
                             "'" + CGSTAmt + "','" + SGSTAmt + "'," +
                             "'" + IGSTamt + "'," + TotalTax + ",'" + Discount + "'," +
                             "'" + TotalAmount + "')", con);
                            con.Open();
                            cmdquotdata.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }

                if (Request.QueryString["cdd"] != null)
                {
                    if (dgvQuatationDtl.Rows.Count > 0)
                    {
                        foreach (GridViewRow g1 in dgvQuatationDtl.Rows)
                        {
                            Label lbldescription = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lbldescription");
                            //TextBox lbldescription = (TextBox)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lbldescription");
                            Label lbltottax = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lbltottax");
                            Label lblprice = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lblprice");
                            Label lblqty = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lblqty");
                            Label lbltxtdiscount = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lbltxtdiscount");
                            Label lbltotalamt = (Label)dgvQuatationDtl.Rows[g1.RowIndex].FindControl("lbltotalamt");

                            string Description = lbldescription.Text.Replace("<br>", "<br>");
                            string HSN = g1.Cells[2].Text;
                            string Qty = lblqty.Text;
                            string Price = lblprice.Text;
                            string Discount = lbltxtdiscount.Text;
                            string CGST = g1.Cells[5].Text;
                            string SGST = g1.Cells[6].Text;
                            string IGST = g1.Cells[7].Text;
                            string CGSTAmt = g1.Cells[8].Text;
                            string SGSTAmt = g1.Cells[9].Text;
                            string IGSTamt = g1.Cells[10].Text;
                            string TotalTax = lbltottax.Text;
                            string TotalAmount = lbltotalamt.Text;

                            SqlCommand cmdquotdata = new SqlCommand(@"INSERT INTO [QuotationData]([quotationid],[quotationno]
                    ,[description],[hsncode],[qty],[rate],[CGST],[SGST],[IGST],[CGSTamt],[SGSTamt],[IGSTamt],[totaltax],[discount],[amount]) 
                     VALUES (" + quotationid + ",'" + quotationno + "','" + Description + "','" + HSN + "'," +
                             "'" + Qty + "','" + Price + "','" + CGST + "','" + SGST + "','" + IGST + "'," +
                             "'" + CGSTAmt + "','" + SGSTAmt + "'," +
                             "'" + IGSTamt + "'," + TotalTax + ",'" + Discount + "'," +
                             "'" + TotalAmount + "')", con);
                            con.Open();
                            cmdquotdata.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }

                ////////////////////////////////
                #endregion

                if (Request.QueryString["cdd"] != null)
                {
                    try
                    {
                        SqlCommand cmdupdate = new SqlCommand("Update QuotationMain set IsRevise=1 where quotationno='" + txQutno.Text + "'", con);
                        con.Open();
                        cmdupdate.ExecuteNonQuery();
                        con.Close();
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                //if (Request.QueryString["cdd"] == null)
                //{
                //    SqlCommand cmdupdate = new SqlCommand("Update EnquiryData set IsActive=0 where EnqCode='" + HFEnqID.Value + "'", con);
                //    con.Open();
                //    cmdupdate.ExecuteNonQuery();
                //    con.Close();
                //}

                if (Chkemail.Checked == true)
                {
                    Sendemail();
                }

                if (Action == "update")
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "Success", " alert('All details Updated successfully !!!');window.location='QuotationList.aspx';", true);
                    validation = 0;
                }
                else
                {
                    //if (Request.QueryString["cdd"] == null)
                    //{
                    //    SqlCommand cmdupdate = new SqlCommand("Update EnquiryData set IsActive=0 where EnqCode='" + HFEnqID.Value + "'", con);
                    //    con.Open();
                    //    cmdupdate.ExecuteNonQuery();
                    //    con.Close();
                    //}
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "Success", " alert('All details saved successfully !!!');window.location='QuotationList.aspx';", true);
                    validation = 0;
                }
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Success", " alert('Please Add Atleast 1 Product..!');", true);
            }
        }
        else
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Success", " alert('Please Select Taxation..!');", true);
        }
    }

    protected void ddljbTransparentdoorcat2_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddljbTransparentdoorcat2.SelectedItem.Text == "Specify")
        {
            txtjbTransparentdoorcat4.Visible = true;
        }
        else
            txtjbTransparentdoorcat4.Visible = false;
    }
    protected void btnCancelWMM23_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnCanceljbbox_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnCancelWMM30_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void BtnCancelMFS_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnCancelEcoMCC_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnCancelModular_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnCancelEcoFrame_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnCancelPCEncShopFloorStanding_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }
    protected void btnCancelSpecify_Click(object sender, EventArgs e)
    {
        PopupAddDetail.Visible = false;
    }

    protected void ddlmaterial_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlmaterial.Text == "Specify")
        {
            txtSpecifymaterial.Visible = true;
        }
        else
            txtSpecifymaterial.Visible = false;
    }

    protected void ddljbPowercoatingshadecat1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddljbPowercoatingshadecat1.SelectedItem.Text == "Specify")
        {
            txtjbPowercoatingshadecat2.Visible = true;
        }
        else
            txtjbPowercoatingshadecat2.Visible = false;
    }

    protected void ddlWMM30TransparentDoorcat2_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlWMM30TransparentDoorcat2.SelectedItem.Text == "Specify")
        {
            txtWMM30TransparentDoorcat5.Visible = true;
        }
        else
            txtWMM30TransparentDoorcat5.Visible = false;
    }

    protected void ddlModularPowerCoatingShadecat1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlModularPowerCoatingShadecat1.SelectedItem.Text == "Specify")
        {
            txtModularPowerCoatingShadecat3.Visible = true;
        }
        else
            txtModularPowerCoatingShadecat3.Visible = false;
    }

    protected void ddlEcoFramePowerCoatingShadecat1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlEcoFramePowerCoatingShadecat1.SelectedItem.Text == "Specify")
        {
            txtddlEcoFramePowerCoatingShadecat2.Visible = true;
        }
        else
            txtddlEcoFramePowerCoatingShadecat2.Visible = false;
    }

    protected void ddlConstype_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlConstype.SelectedItem.Text == "PC ENCLOSURE")
        {
            PopupAddDetail.Visible = true;
            this.modelprofile.Show();
        }
        FillExcelsheet();
        if (ddlConstype.Text == "JB Box")
        {
            PanelType1.Visible = true;
            this.modelprofile.Show();
            PanelType2.Visible = false;
            PanelType3.Visible = false;
            PanelType4.Visible = false;
            PanelType5.Visible = false;
            PanelType6.Visible = false;
            PanelType7.Visible = false;
            PanelType8.Visible = false;
            PanelType9.Visible = false;
            PanelType10.Visible = false;
            PanelType11.Visible = false;
            PanelType12.Visible = false;
            PanelType13.Visible = false;
            txtconstructiontype.Visible = false;
        }
        if (ddlConstype.Text == "WMM-22")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = true;
            this.modelprofile.Show();
            PanelType3.Visible = false;
            PanelType4.Visible = false;
            PanelType5.Visible = false;
            PanelType6.Visible = false;
            PanelType7.Visible = false;
            PanelType8.Visible = false;
            PanelType9.Visible = false;
            PanelType10.Visible = false;
            PanelType11.Visible = false;
            PanelType12.Visible = false;
            PanelType13.Visible = false;
            txtconstructiontype.Visible = false;
        }
        if (ddlConstype.Text == "WMM-30 (MCC Box)")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = false;
            PanelType3.Visible = true;
            this.modelprofile.Show();
            PanelType4.Visible = false;
            PanelType5.Visible = false;
            PanelType6.Visible = false;
            PanelType7.Visible = false;
            PanelType8.Visible = false;
            PanelType9.Visible = false;
            PanelType10.Visible = false;
            PanelType11.Visible = false;
            PanelType12.Visible = false;
            PanelType13.Visible = false;
            txtconstructiontype.Visible = false;
        }
        if (ddlConstype.Text == "MFS (Modular Floor Standing Enclosure)")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = false;
            PanelType3.Visible = false;
            PanelType4.Visible = true;
            PanelType5.Visible = false;
            PanelType6.Visible = false;
            PanelType7.Visible = false;
            PanelType8.Visible = false;
            PanelType9.Visible = false;
            PanelType10.Visible = false;
            PanelType11.Visible = false;
            this.modelprofile.Show();
            txtconstructiontype.Visible = false;
        }
        if (ddlConstype.Text == "Eco MCC 30mm")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = false;
            PanelType3.Visible = false;
            PanelType4.Visible = false;
            PanelType5.Visible = true;
            PanelType6.Visible = false;
            PanelType7.Visible = false;
            PanelType8.Visible = false;
            PanelType9.Visible = false;
            PanelType10.Visible = false;
            PanelType11.Visible = false;
            PanelType12.Visible = false;
            PanelType13.Visible = false;
            this.modelprofile.Show();
            txtconstructiontype.Visible = false;
        }
        if (ddlConstype.Text == "Modular W-Big 43 mm")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = false;
            PanelType3.Visible = false;
            PanelType4.Visible = false;
            PanelType5.Visible = false;
            PanelType6.Visible = true;
            PanelType7.Visible = false;
            PanelType8.Visible = false;
            PanelType9.Visible = false;
            PanelType10.Visible = false;
            PanelType11.Visible = false;
            PanelType12.Visible = false;
            PanelType13.Visible = false;
            this.modelprofile.Show();
            txtconstructiontype.Visible = false;
        }
        if (ddlConstype.Text == "Eco Frame 43mm")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = false;
            PanelType3.Visible = false;
            PanelType4.Visible = false;
            PanelType5.Visible = false;
            PanelType6.Visible = false;
            PanelType7.Visible = true;
            PanelType8.Visible = false;
            PanelType9.Visible = false;
            PanelType10.Visible = false;
            PanelType11.Visible = false;
            PanelType12.Visible = false;
            PanelType13.Visible = false;
            this.modelprofile.Show();
            txtconstructiontype.Visible = false;
        }
        if (ddlConstype.Text == "PC ENCLOSURE")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = false;
            PanelType3.Visible = false;
            PanelType4.Visible = false;
            PanelType5.Visible = false;
            PanelType6.Visible = false;
            PanelType7.Visible = false;
            PanelType8.Visible = true;
            PanelType9.Visible = false;
            PanelType10.Visible = false;
            PanelType11.Visible = false;
            PanelType12.Visible = false;
            PanelType13.Visible = false;
            this.modelprofile.Show();
            txtconstructiontype.Visible = false;
        }
        if (ddlConstype.Text == "PC TABLE")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = false;
            PanelType3.Visible = false;
            PanelType4.Visible = false;
            PanelType5.Visible = false;
            PanelType6.Visible = false;
            PanelType7.Visible = false;
            PanelType8.Visible = false;
            PanelType9.Visible = true;
            PanelType10.Visible = false;
            PanelType11.Visible = false;
            PanelType12.Visible = false;
            PanelType13.Visible = false;
            this.modelprofile.Show();
            txtconstructiontype.Visible = false;
        }
        if (ddlConstype.Text == "PRINTER TABLE")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = false;
            PanelType3.Visible = false;
            PanelType4.Visible = false;
            PanelType5.Visible = false;
            PanelType6.Visible = false;
            PanelType7.Visible = false;
            PanelType8.Visible = false;
            PanelType9.Visible = false;
            PanelType10.Visible = true;
            PanelType11.Visible = false;
            PanelType12.Visible = false;
            PanelType13.Visible = false;
            this.modelprofile.Show();
            txtconstructiontype.Visible = false;
        }
        if (ddlConstype.Text == "Single Piece Desk")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = false;
            PanelType3.Visible = false;
            PanelType4.Visible = false;
            PanelType5.Visible = false;
            PanelType6.Visible = false;
            PanelType7.Visible = false;
            PanelType8.Visible = false;
            PanelType9.Visible = false;
            PanelType10.Visible = false;
            PanelType11.Visible = true;
            PanelType12.Visible = false;
            PanelType13.Visible = false;
            this.modelprofile.Show();
        }
        if (ddlConstype.Text == "Three Piece Desk")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = false;
            PanelType3.Visible = false;
            PanelType4.Visible = false;
            PanelType5.Visible = false;
            PanelType6.Visible = false;
            PanelType7.Visible = false;
            PanelType8.Visible = false;
            PanelType9.Visible = false;
            PanelType10.Visible = false;
            PanelType11.Visible = false;
            PanelType12.Visible = true;
            PanelType13.Visible = false;
            this.modelprofile.Show();
        }
        if (ddlConstype.Text == "Specify")
        {
            PanelType1.Visible = false;
            PanelType2.Visible = false;
            PanelType3.Visible = false;
            PanelType4.Visible = false;
            PanelType5.Visible = false;
            PanelType6.Visible = false;
            PanelType7.Visible = false;
            PanelType8.Visible = false;
            PanelType9.Visible = false;
            PanelType10.Visible = false;
            PanelType11.Visible = false;
            PanelType12.Visible = false;
            PanelType13.Visible = true;
            this.modelprofile.Show();
            txtconstructiontype.Visible = true;
        }
    }

    protected void ddlkindatt_SelectedIndexChanged(object sender, EventArgs e)
    {
        Getemail();
    }

    private void Getemail()
    {
        SqlCommand cmdget = new SqlCommand("SP_GetEmail", con);
        cmdget.CommandType = CommandType.StoredProcedure;
        cmdget.Parameters.AddWithValue("@oname", ddlkindatt.SelectedItem == null ? "" : ddlkindatt.SelectedItem.Text);

        SqlDataAdapter dataAdapter = new SqlDataAdapter(cmdget);
        DataTable dataTable = new DataTable();
        dataAdapter.Fill(dataTable);

        if (dataTable.Rows.Count > 0)
        {
            lblemail.Text = dataTable.Rows[0]["Email"].ToString();
        }
    }
    string filename = "", party = "";
    private void Sendemail()
    {
        string servername = "", dbname = "", userid = "", pass = "";
        byte[] attachment; StringBuilder sb = new StringBuilder();
        //TableLogOnInfos crtableLogoninfos = new TableLogOnInfos();
        //TableLogOnInfo crtableLogoninfo = new TableLogOnInfo();
        //ConnectionInfo crConnectionInfo = new ConnectionInfo();
        //Tables CrTables;
        DataTable dtt = new DataTable();
        List<byte[]> files = new List<byte[]>();

        string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
        dbname = builder.InitialCatalog;
        servername = builder.DataSource;
        userid = builder.UserID;
        pass = builder.Password;

        SqlDataAdapter da2 = new SqlDataAdapter(@"SELECT QuotationData.description, QuotationData.hsncode, QuotationData.qty, QuotationData.rate,QuotationData.CGST
        ,QuotationData.SGST,QuotationData.IGST,QuotationData.CGSTamt,QuotationData.SGSTamt,QuotationData.IGSTamt,QuotationData.totaltax, QuotationData.discount, QuotationData.amount,
        QuotationMain.quotationno, QuotationMain.partyname, QuotationMain.ccode,QuotationMain.kindatt, QuotationMain.address,
        QuotationMain.paymentterm1,QuotationMain.paymentterm2,QuotationMain.paymentterm3,QuotationMain.paymentterm4,QuotationMain.paymentterm5,
		QuotationMain.validityofoffer,QuotationMain.deliveryperiod,QuotationMain.specialpackaging,QuotationMain.standardpackaging,QuotationMain.inspection,QuotationMain.transportation,
        Format(QuotationMain.date,'dd-MM-yyyy')as date, QuotationMain.remark, QuotationMain.width, QuotationMain.Toatlamt, QuotationMain.depth, 
        QuotationMain.height, QuotationMain.base, QuotationMain.canopy, QuotationMain.material,QuotationMain.specifymaterial, QuotationMain.Constructiontype,
        QuotationMain.descriptionall, QuotationMain.sessionname,QuotationMain.filename1,QuotationMain.filename2, QuotationMain.createddate
        FROM QuotationData INNER JOIN QuotationMain ON QuotationData.quotationid = QuotationMain.id where QuotationMain.id=" + quotationid + "", con);
        da2.Fill(dtt);

        //ReportDocument cryRpt = new ReportDocument();

        ////////New Quotation//////////
        //cryRpt.Load(Server.MapPath(string.Format("../SalesQuotationReport.rpt", 1)));

        //crConnectionInfo.ServerName = servername;
        //crConnectionInfo.DatabaseName = dbname;
        //crConnectionInfo.UserID = userid;
        //crConnectionInfo.Password = pass;

        //// CrTables = cryRpt.Database.Tables;
        //SqlDataAdapter adpp = new SqlDataAdapter("select cname,mobile1,mobile2 from Company where ccode='" + dtt.Rows[0]["ccode"].ToString() + "'", con);
        //DataTable data = new DataTable();
        //adpp.Fill(data);

        //if (data.Rows.Count > 0)
        //{
        //    if (string.IsNullOrEmpty(data.Rows[0]["mobile2"].ToString()))
        //    {
        //        cryRpt.SetParameterValue("contact", data.Rows[0]["mobile1"].ToString());
        //    }
        //    if (!string.IsNullOrEmpty(data.Rows[0]["mobile1"].ToString()) && !string.IsNullOrEmpty(data.Rows[0]["mobile2"].ToString()))
        //    {
        //        cryRpt.SetParameterValue("contact", data.Rows[0]["mobile1"].ToString() + " / " + data.Rows[0]["mobile2"].ToString());
        //    }
        //}
        //if (dtt.Rows.Count > 0)
        //{
        //    oldfile1 = dtt.Rows[0]["filename1"].ToString();
        //    oldfile2 = dtt.Rows[0]["filename2"].ToString();
        //    date = dtt.Rows[0]["date"].ToString();

        //    if (!string.IsNullOrEmpty(dtt.Rows[0]["paymentterm1"].ToString()))
        //    {
        //        sb.Append(dtt.Rows[0]["paymentterm1"].ToString() + "<br/>");
        //    }
        //    if (!string.IsNullOrEmpty(dtt.Rows[0]["paymentterm2"].ToString()))
        //    {
        //        sb.Append(dtt.Rows[0]["paymentterm2"].ToString() + "<br/>");
        //    }
        //    if (!string.IsNullOrEmpty(dtt.Rows[0]["paymentterm3"].ToString()))
        //    {
        //        sb.Append(dtt.Rows[0]["paymentterm3"].ToString() + "<br/>");
        //    }
        //    if (!string.IsNullOrEmpty(dtt.Rows[0]["paymentterm4"].ToString()))
        //    {
        //        sb.Append(dtt.Rows[0]["paymentterm4"].ToString() + "<br/>");
        //    }
        //    if (!string.IsNullOrEmpty(dtt.Rows[0]["paymentterm5"].ToString()))
        //    {
        //        sb.Append(dtt.Rows[0]["paymentterm5"].ToString() + "<br/>");
        //    }

        //    cryRpt.SetParameterValue("descriptionall", dtt.Rows[0]["descriptionall"].ToString());
        //    cryRpt.SetParameterValue("partyname", dtt.Rows[0]["partyname"].ToString());
        //    cryRpt.SetParameterValue("address", dtt.Rows[0]["address"].ToString());
        //    cryRpt.SetParameterValue("qno", dtt.Rows[0]["quotationno"].ToString());
        //    cryRpt.SetParameterValue("qdate", dtt.Rows[0]["date"].ToString());
        //    cryRpt.SetParameterValue("hsn", dtt.Rows[0]["hsncode"].ToString());
        //    cryRpt.SetParameterValue("qty", dtt.Rows[0]["qty"].ToString());
        //    cryRpt.SetParameterValue("discount", dtt.Rows[0]["discount"].ToString());
        //    cryRpt.SetParameterValue("rate", dtt.Rows[0]["rate"].ToString());
        //    cryRpt.SetParameterValue("amount", dtt.Rows[0]["amount"].ToString());
        //    cryRpt.SetParameterValue("remark", dtt.Rows[0]["remark"].ToString());
        //    cryRpt.SetParameterValue("Paymentterm", sb.ToString());
        //    cryRpt.SetParameterValue("kindatt", dtt.Rows[0]["kindatt"].ToString());

        //    filename = dtt.Rows[0]["quotationno"].ToString();
        //    party = dtt.Rows[0]["partyname"].ToString();

        //    if (dtt.Rows[0]["CGST"].ToString() == "0" && dtt.Rows[0]["SGST"].ToString() == "0")
        //    {
        //        cryRpt.SetParameterValue("CGST", "Not Applicable");
        //        cryRpt.SetParameterValue("SGST", "Not Applicable");
        //        cryRpt.SetParameterValue("IGST", "Extra as applicable (Presently " + dtt.Rows[0]["IGST"].ToString() + "%)");
        //    }
        //    if (dtt.Rows[0]["CGST"].ToString() != "0" && dtt.Rows[0]["SGST"].ToString() != "0")
        //    {
        //        cryRpt.SetParameterValue("CGST", "Extra as applicable (Presently " + dtt.Rows[0]["CGST"].ToString() + "%)");
        //        cryRpt.SetParameterValue("SGST", "Extra as applicable (Presently " + dtt.Rows[0]["SGST"].ToString() + "%)");
        //        cryRpt.SetParameterValue("IGST", "Not Applicable");
        //    }

        //    cryRpt.SetParameterValue("validityofoffer", dtt.Rows[0]["validityofoffer"].ToString());
        //    cryRpt.SetParameterValue("deliveryperiod", dtt.Rows[0]["deliveryperiod"].ToString());
        //    cryRpt.SetParameterValue("transportation", dtt.Rows[0]["transportation"].ToString());
        //    cryRpt.SetParameterValue("standardpackaging", dtt.Rows[0]["standardpackaging"].ToString());
        //    cryRpt.SetParameterValue("specialpackaging", dtt.Rows[0]["specialpackaging"].ToString());
        //    cryRpt.SetParameterValue("inspection", dtt.Rows[0]["inspection"].ToString());
        //}
        //CrTables = cryRpt.Database.Tables;
        //foreach (CrystalDecisions.CrystalReports.Engine.Table CrTable in CrTables)
        //{
        //    crtableLogoninfo = CrTable.LogOnInfo;
        //    crtableLogoninfo.ConnectionInfo = crConnectionInfo;
        //    CrTable.ApplyLogOnInfo(crtableLogoninfo);
        //}

        //Stream stream1 = cryRpt.ExportToStream(ExportFormatType.PortableDocFormat);

        //byte[] attach = ReadFully(stream1);

        //using (MemoryStream stream = new MemoryStream())
        //{
        //    PdfReader reader = new PdfReader(attach);
        //    using (PdfStamper stamper = new PdfStamper(reader, stream))
        //    {
        //        int pages = reader.NumberOfPages;
        //    }
        //    attachment = stream.ToArray();
        //}
        //Stream stream2 = new MemoryStream(attachment);

        string FromMailID = ConfigurationManager.AppSettings["mailUserName"];
        string ToMailID = lblemail.Text;//"pushpendra@weblinkservices.net";

        MailMessage mm = new MailMessage();
        mm.From = new MailAddress(FromMailID);

        mm.Subject = "Quotation from Excel Enclosure";
        mm.To.Add(ToMailID);

        mm.IsBodyHtml = true;
        SmtpClient smtp = new SmtpClient();
        smtp.Host = ConfigurationManager.AppSettings["Host"];
        smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
        System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
        NetworkCred.UserName = ConfigurationManager.AppSettings["mailUserName"];
        NetworkCred.Password = ConfigurationManager.AppSettings["mailUserPass"];
        smtp.UseDefaultCredentials = true;
        smtp.Credentials = NetworkCred;
        smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);

        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        };

        string[] abc = filename.Split('/');
        string abc2 = abc[0].ToString();
        string newfilename = abc2 + "-" + party + ".pdf";
        //mm.Attachments.Add(new Attachment(new MemoryStream(attachment), newfilename));

        if (!string.IsNullOrEmpty(oldfile2))
        {
            byte[] file = File.ReadAllBytes((Server.MapPath("~/RefDocument/")) + oldfile2);

            Stream stream = new MemoryStream(file);
            Attachment aa = new Attachment(stream, oldfile1);
            mm.Attachments.Add(aa);
        }

        mm.Body = @"<html><body style='border:1px solid #148bc4;padding:10px;width:600px;'><b style='font-size:15px';><u>Quotation Details</u></b><br><br><b>
            Quotation No : </b> " + txQutno.Text + " <br> <b>Quotation Date : </b> " + date +
        "<br> <b>Delivery Period: </b> " + ddldeliveryperiod.SelectedItem.Text + "  <br><b> Transportation: </b> " + ddltransportation.SelectedItem.Text + "<br><b> Statndard packaging: </b> " + ddlStandardPacking.SelectedItem.Text + "<br><b> Special packaging : </b> " + ddlSpecialPacking.SelectedItem.Text + "<br><b> Inspection : </b> " + ddlinspection.SelectedItem.Text + " <br><b> Total Amount : " + txtAmt1.Text + "/- Rs. </b> <br><b> Download Sales Quotation : <a href='http://www.weblinkservices.in/Reports/SalesQuotationRptPDF.aspx?ID=" + quotationid + "'>Download Sales Quotation Invoice</a></b> </body></html> ";
        smtp.Send(mm);
    }

    public static byte[] ReadFully(Stream input)
    {
        byte[] buffer = new byte[16 * 1024];
        using (MemoryStream ms = new MemoryStream())
        {
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }

            return ms.ToArray();
        }
    }

    protected void rdlpcenclosure_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (rdlpcenclosure.SelectedItem.Text == "Shop Floor PC Enclosure Standing")
        {
            Panel81.Visible = true; Panel82.Visible = false; PopupAddDetail.Visible = true;
            this.modelprofile.Show();
        }
        if (rdlpcenclosure.SelectedItem.Text == "PC Enclosure ECO-Standing" || rdlpcenclosure.SelectedItem.Text == "PC Enclosure ECO-Sitting")
        {
            Panel81.Visible = false; Panel82.Visible = true; PopupAddDetail.Visible = true;
            this.modelprofile.Show();
        }
        FillExcelsheet();
    }

    protected void btnAddStandingComponentMtgPlate1_Click(object sender, EventArgs e)
    {
        componetmtgplat2.Visible = true;
    }

    protected void AddStandingComponentMtgPlate2_Click(object sender, EventArgs e)
    {
        componetmtgplat3.Visible = true;
    }

    protected void AddStandingComponentMtgPlate3_Click(object sender, EventArgs e)
    {
        componetmtgplat4.Visible = true;
    }

    protected void btnPCEncShopFloorStandingSidecPlate1_Click(object sender, EventArgs e)
    {
        SidecPlate2.Visible = true;
    }

    protected void btnPCEncShopFloorStandingSidecPlate2_Click(object sender, EventArgs e)
    {
        SidecPlate3.Visible = true;
    }

    protected void btnPCEncShopFloorStandingSidecPlate3_Click(object sender, EventArgs e)
    {
        SidecPlate4.Visible = true;
    }

    protected void btnPCEncShopFloorStandingDoorCPlate1_Click(object sender, EventArgs e)
    {
        DoorcPlate2.Visible = true;
    }

    protected void btnPCEncShopFloorStandingDoorCPlate2_Click(object sender, EventArgs e)
    {
        DoorcPlate3.Visible = true;
    }

    protected void btnPCEncShopFloorStandingDoorCPlate3_Click(object sender, EventArgs e)
    {
        DoorcPlate4.Visible = true;
    }

    protected void ddlPCEncShopFloorStandingPowderCoatingShadecat1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlPCEncShopFloorStandingPowderCoatingShadecat1.SelectedItem.Text == "Specify")
        {
            txtPCEncShopFloorStandingPowderCoatingShadecat2.Visible = true;
        }
        if (ddlPCEncShopFloorStandingPowderCoatingShadecat1.SelectedItem.Text != "Specify")
        {
            txtPCEncShopFloorStandingPowderCoatingShadecat2.Visible = false;
        }
    }

    protected void ddlPCEncShopFloorStandingTransparentDoorcat2_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlPCEncShopFloorStandingTransparentDoorcat2.SelectedItem.Text == "Specify")
        {
            txtPCEncShopFloorStandingTransparentDoorcat3.Visible = true;
        }
        if (ddlPCEncShopFloorStandingTransparentDoorcat2.SelectedItem.Text != "Specify")
        {
            txtPCEncShopFloorStandingTransparentDoorcat3.Visible = false;
        }
    }

    protected void ddlPCEnclosureECOStandingBasecat3_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlPCEnclosureECOStandingBasecat3.SelectedItem.Text != "Specify")
        {
            txtPCEnclosureECOStandingBasecat4.Visible = false;
        }
        if (ddlPCEnclosureECOStandingBasecat3.SelectedItem.Text == "Specify")
        {
            txtPCEnclosureECOStandingBasecat4.Visible = true;
        }
    }

    protected void ddlPCEnclosureECOStandingPowderCoatingcat1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlPCEnclosureECOStandingPowderCoatingcat1.SelectedItem.Text == "Specify")
        {
            txtPCEnclosureECOStandingPowderCoatingcat2.Visible = true;
        }
        if (ddlPCEnclosureECOStandingPowderCoatingcat1.SelectedItem.Text != "Specify")
        {
            txtPCEnclosureECOStandingPowderCoatingcat2.Visible = false;
        }
    }

    protected void ddlPcTablePowderCoatingShadecat1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlPcTablePowderCoatingShadecat1.SelectedItem.Text == "Specify")
        {
            txtPcTablePowderCoatingShadecat2.Visible = true;
        }
        if (ddlPcTablePowderCoatingShadecat1.SelectedItem.Text != "Specify")
        {
            txtPcTablePowderCoatingShadecat2.Visible = false;
        }
    }

    protected void ddlPrinterTablePowderCoatingShadecat1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlPrinterTablePowderCoatingShadecat1.SelectedItem.Text == "Specify")
        {
            txtPrinterTablePowderCoatingShadecat2.Visible = true;
        }
        if (ddlPrinterTablePowderCoatingShadecat1.SelectedItem.Text != "Specify")
        {
            txtPrinterTablePowderCoatingShadecat2.Visible = false;
        }
    }

    protected void ddlSinglePiecePowderCoatingShadecat1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlSinglePiecePowderCoatingShadecat1.SelectedItem.Text == "Specify")
        {
            txtSinglePiecePowderCoatingShadecat2.Visible = true;
        }
        if (ddlSinglePiecePowderCoatingShadecat1.SelectedItem.Text != "Specify")
        {
            txtSinglePiecePowderCoatingShadecat2.Visible = false;
        }
    }

    protected void ddlThreePiecePowderCoatingShadecat1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlThreePiecePowderCoatingShadecat1.SelectedItem.Text == "Specify")
        {
            txtThreePiecePowderCoatingShadecat2.Visible = true;
        }
        if (ddlThreePiecePowderCoatingShadecat1.SelectedItem.Text != "Specify")
        {
            txtThreePiecePowderCoatingShadecat2.Visible = false;
        }
    }

    static List<string> lstsheetname = new List<string>();
    static List<string> lstsheetpath = new List<string>();

    protected void Repeater1_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName == "download")
        {
            int id = Convert.ToInt32(e.CommandArgument.ToString());

            SqlDataAdapter addefault = new SqlDataAdapter("select id,sheetname,category,path, status from excelsheetdata where category='" + myconstype + "' and id=" + id, con);
            DataTable path = new DataTable();
            addefault.Fill(path);

            if (path.Rows.Count > 0)
            {
                Response.Clear();
                Response.AddHeader("Content-Disposition", "attachment; filename=" + path.Rows[0]["sheetname"].ToString() + ";");
                Response.TransmitFile(path.Rows[0]["path"].ToString());
                Response.End();
            }
        }
        if (e.CommandName == "upload")
        {
            lstsheetname.Clear(); //lstsheetpath.Clear();
            if (!string.IsNullOrEmpty(myconstype))
            {
                if (myconstype == "JBboxdata")
                {
                    myconstype = "JB Box";
                }
                if (myconstype == "WMM23AEBox")
                {
                    myconstype = "WMM-22";
                }
                if (myconstype == "EcoFrame")
                {
                    myconstype = "Eco Frame 43mm";
                }
                if (myconstype == "Modular")
                {
                    myconstype = "Modular W-Big 43 mm";
                }
                if (myconstype == "WMM30MCCBox")
                {
                    myconstype = "WMM-30 (MCC Box)";
                }
                if (myconstype == "EcoMCC")
                {
                    myconstype = "Eco MCC 30mm";
                }
                if (myconstype == "MFS")
                {
                    myconstype = "MFS (Modular Floor Standing Enclosure)";
                }
                if (myconstype == "PCEnclosureECOStanding")
                {
                    myconstype = "PC Enclosure ECO-Standing";
                }
                if (myconstype == "PCEnclosureECOSitting")
                {
                    myconstype = "PC Enclosure ECO-Sitting";
                }
                if (myconstype == "ShopFloorPCEnclosureStanding")
                {
                    myconstype = "Shop Floor PC Enclosure Standing";
                }
            }
            int id = Convert.ToInt32(e.CommandArgument.ToString());



            SqlCommand sqlCommand = new SqlCommand("SP_Sheetuploaddata", con);
            sqlCommand.CommandType = CommandType.StoredProcedure;

            //sqlCommand.Parameters.AddWithValue("@id", id);
            //sqlCommand.Parameters.AddWithValue("@oldsheetname", dtpath2.Rows[0]["sheetname"].ToString());
            sqlCommand.Parameters.AddWithValue("@quotationid", myquotationid);
            //sqlCommand.Parameters.AddWithValue("@sheetname", fileName);
            //sqlCommand.Parameters.AddWithValue("@updatedsheetname", updatedsheetname);
            //sqlCommand.Parameters.AddWithValue("@category", myconstype);
            //sqlCommand.Parameters.AddWithValue("@path", "../Updatedsheets/" + updatedsheetname);
            //sqlCommand.Parameters.AddWithValue("@status", "updated");
            //sqlCommand.Parameters.AddWithValue("@Isactive", "0");

            con.Open();
            sqlCommand.ExecuteNonQuery();
            con.Close();

            foreach (RepeaterItem item in Repeater1.Items)
            {
                if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                {
                    FileUpload fu = (FileUpload)item.FindControl("FileUpload1");
                    if (fu.HasFile)
                    {
                        string path = Server.MapPath("../Updatedsheets/");
                        string fileName = Path.GetFileName(fu.FileName);
                        string[] filename2 = fileName.Split('.');
                        string fileExt = Path.GetExtension(fu.FileName).ToLower();
                        string time = DateTime.Now.Ticks.ToString();
                        string updatedsheetname = filename2[0].ToString() + "-" + myquotationid + "-" + time + fileExt;

                        if (fileExt == ".xls" || fileExt == ".xlsx")
                        {
                            fu.SaveAs(path + updatedsheetname);
                        }

                        //SqlCommand sqlCommand = new SqlCommand("SP_Sheetuploaddata", con);
                        //sqlCommand.CommandType = CommandType.StoredProcedure;

                        //sqlCommand.Parameters.AddWithValue("@id", id);
                        //sqlCommand.Parameters.AddWithValue("@oldsheetname", dtpath2.Rows[0]["sheetname"].ToString());
                        //sqlCommand.Parameters.AddWithValue("@quotationid", myquotationid);
                        //sqlCommand.Parameters.AddWithValue("@sheetname", fileName);
                        //sqlCommand.Parameters.AddWithValue("@updatedsheetname", updatedsheetname);
                        //sqlCommand.Parameters.AddWithValue("@category", myconstype);
                        //sqlCommand.Parameters.AddWithValue("@path", "../Updatedsheets/" + updatedsheetname);
                        //sqlCommand.Parameters.AddWithValue("@status", "updated");
                        //sqlCommand.Parameters.AddWithValue("@Isactive", "0");

                        //con.Open();
                        //sqlCommand.ExecuteNonQuery();
                        //con.Close();

                        // ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", " alert('Costing sheet uploaded successfully !!!');", true);

                        PopupAddDetail.Visible = true;
                        this.modelprofile.Show();


                        SqlCommand cmdupdate = new SqlCommand(@"INSERT INTO [Excelsheetdata]([quotationid],[sheetname],[updatedsheetname],[category],[path],[status],[Isactive])
                        VALUES (" + myquotationid + ",'" + fileName + "','" + updatedsheetname + "','" + myconstype + "','" + "../Updatedsheets/" + updatedsheetname + "','updated',0)", con);
                        con.Open();
                        cmdupdate.ExecuteNonQuery();
                        con.Close();
                    }
                    if (fu.HasFile)
                    {
                        continue;
                    }
                    else
                    {
                        Label lblsheetname = (Label)item.FindControl("lblsheetname");
                        lstsheetname.Add(lblsheetname.Text);

                        Label lblsheetpath = (Label)item.FindControl("lblpath");
                        lstsheetpath.Add(lblsheetpath.Text);
                    }
                }
            }



            for (int i = 0; i < lstsheetname.Count; i++)
            {
                string path = lstsheetpath[i].ToString();
                string fileName = lstsheetname[i].ToString();
                string[] filename2 = fileName.Split('.');
                string fileExt = "." + filename2[1].ToString();
                string time = DateTime.Now.Ticks.ToString();
                string updatedsheetname = filename2[0].ToString() + "-" + myquotationid + "-" + time + fileExt;

                /////////////////////////////

                //if (fileExt == ".xls" || fileExt == ".xlsx")
                //{

                //}

                SqlCommand cmdupdate = new SqlCommand(@"INSERT INTO [Excelsheetdata]([quotationid],[sheetname],[updatedsheetname],[category],[path],[status],[Isactive])
                    VALUES (" + myquotationid + ",'" + lstsheetname[i].ToString() + "','" + fileName + "','" + myconstype + "','" + lstsheetpath[i].ToString() + "','updated',0)", con);
                con.Open();
                cmdupdate.ExecuteNonQuery();
                con.Close();
            }
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", " alert('Costing sheet uploaded successfully !!!');", true);
        }
        // }
    }

    protected void Repeater1_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (Request.QueryString["cdd"] != null)
        {
            System.Web.UI.WebControls.Button btn = e.Item.FindControl("btnupload") as System.Web.UI.WebControls.Button;
            btn.Enabled = true;
        }
    }

    protected string Description()
    {

        #region 1.JBBox
        if (ddlConstype.Text == "JB Box")
        {
            if (chkJbweldedmainbody.Checked == false && chkjbGlandplat.Checked == false
                && ddljbComponetmtgplt.Text == "Select" && chkjbfrontscrewcover.Checked == false
                && Chkjblock.Checked == false && chkjbTransparentdoor.Checked == false
                && chkjbwallmtgbracket.Checked == false && ChkjbPowercoatingshade.Checked == false && ChkjbFan.Checked == false
                && ChkjbJointlesspolyurethane.Checked == false && ChkjbAnyadditionalcomponent.Checked == false)
            {
                validation = 1;
            }

            action = "insertJBbox";
            //1
            if (chkJbweldedmainbody.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlJbweldedmainbodycat1.SelectedItem.Text != "Select")
                {
                    first = ddlJbweldedmainbodycat1.SelectedItem.Text;
                    sentence = chkJbweldedmainbody.Text + " " + ddlJbweldedmainbodycat1.SelectedItem.Text;
                }
                if (ddlJbweldedmainbodycat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlJbweldedmainbodycat2.SelectedItem.Text;
                    sentence += " in " + second + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, chkJbweldedmainbody.Text, first, second, "", "", "", sentence);
            }

            //2
            if (chkjbGlandplat.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddljbGlandplatcat1.SelectedItem.Text != "Select")
                {
                    first = ddljbGlandplatcat1.SelectedItem.Text;
                    sentence = chkjbGlandplat.Text + ":" + first;
                }
                if (ddljbGlandplatcat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddljbGlandplatcat2.SelectedItem.Text;
                    sentence += " in " + ddljbGlandplatcat2.SelectedItem.Text + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, chkjbGlandplat.Text, first, second, "", "", "", sentence);
            }

            //3
            {
                if (ddljbComponetmtgplt.Text != "Select")
                {
                    validation = 0;
                    string second = "", third = "", sentence = "";

                    if (ddljbComponetmtgpltcat1.SelectedItem.Text != "Select")
                    {
                        second = ddljbComponetmtgpltcat1.SelectedItem.Text;
                        sentence = ddljbComponetmtgplt.Text + " : " + second;
                    }
                    if (ddljbComponetmtgpltcat2.SelectedItem.Text != "Select Colour")
                    {
                        third = ddljbComponetmtgpltcat2.SelectedItem.Text;
                        sentence += " colour " + third + " : ";
                    }
                    if (txtJbweldedmainbodycat.Text != "")
                    {
                        third = txtJbweldedmainbodycat.Text;
                        sentence += " Thickness " + third;
                    }


                    dtConstructionType.Rows.Add(quotationno, quotationid, ddljbComponetmtgplt.Text, second, third, "", "", "", sentence);
                }

                //4
                if (chkjbfrontscrewcover.Checked == true)
                {
                    validation = 0;
                    string second = "", sentence = "";

                    if (ddljbfrontscrewcovercat1.SelectedItem.Text != "Select Thickness")
                    {
                        second = ddljbfrontscrewcovercat1.SelectedItem.Text;
                        sentence = chkjbfrontscrewcover.Text + " in " + second + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, chkjbfrontscrewcover.Text, second, "", "", "", "", sentence);
                }

                //5
                if (Chkjblock.Checked == true)
                {
                    validation = 0;
                    string second = "", sentence = "";
                    if (ddljbLockcat1.SelectedItem.Text != "Select")
                    {
                        second = ddljbLockcat1.SelectedItem.Text;
                        sentence = Chkjblock.Text + " in " + second + " , ";
                    }
                    if (txtQty.Text != "")
                    {

                        second = txtQty.Text;
                        sentence += " Qty " + second;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, Chkjblock.Text, second, "", "", "", "", sentence);
                }

                //6
                if (chkjbTransparentdoor.Checked == true)
                {
                    string second = "", third = "", fourth = "", sentence = "";

                    if (ddljbTransparentdoorcat1.SelectedItem.Text != "Select")
                    {
                        second = ddljbTransparentdoorcat1.SelectedItem.Text;
                        sentence = chkjbTransparentdoor.Text + " : " + second + " ";
                    }
                    if (ddljbTransparentdoorcat2.SelectedItem.Text != "Select")
                    {
                        if (ddljbTransparentdoorcat2.Text == "Specify")
                        {
                            sentence += txtjbTransparentdoorcat4.Text + " ";
                            third = ddljbTransparentdoorcat2.SelectedItem.Text;
                        }
                        else
                        {
                            third = ddljbTransparentdoorcat2.SelectedItem.Text;
                            sentence += third + " ";
                        }
                    }
                    if (ddljbTransparentdoorcat3.SelectedItem.Text != "Select")
                    {
                        fourth = ddljbTransparentdoorcat3.SelectedItem.Text;
                        sentence += fourth;
                    }
                    if (txtjbTransparentdoorcat4.Text != "")
                    {
                        sentence += " " + txtjbTransparentdoorcat4.Text;
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, chkjbTransparentdoor.Text, second, third, fourth, txtjbTransparentdoorcat4.Text, "", sentence);
                }
            }

            //7
            if (chkjbwallmtgbracket.Checked == true)
            {
                validation = 0;
                string sentence = "";

                sentence = chkjbwallmtgbracket.Text;

                dtConstructionType.Rows.Add(quotationno, quotationid, chkjbwallmtgbracket.Text, "", "", "", "", "", sentence);
            }

            //8
            if (ChkjbPowercoatingshade.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddljbPowercoatingshadecat1.SelectedItem.Text != "Select Colour")
                {
                    first = ddljbPowercoatingshadecat1.SelectedItem.Text;
                    sentence = ChkjbPowercoatingshade.Text + ": colour " + first + " " + txtjbPowercoatingshadecat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkjbPowercoatingshade.Text, first, txtjbPowercoatingshadecat2.Text, "", "", "", sentence);
            }

            //9
            if (ChkjbFan.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddljbfancat1.SelectedItem.Text != "Select Size")
                {
                    first = ddljbfancat1.SelectedItem.Text;
                    sentence = ChkjbFan.Text + ": In Size " + first + " inch qty: " + txtjbfanqtycat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkjbFan.Text, first, txtjbfanqtycat2.Text, "", "", "", sentence);
            }

            //10
            if (ChkjbJointlesspolyurethane.Checked == true)
            {
                validation = 0;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkjbJointlesspolyurethane.Text, "", "", "", "", "", ChkjbJointlesspolyurethane.Text);
            }

            //11
            if (ChkjbAnyadditionalcomponent.Checked == true)
            {
                validation = 0;
                string sentence = "";

                sentence = ChkjbAnyadditionalcomponent.Text + ": " + txtjbAnyadditionalcomponentcat1.Text;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkjbAnyadditionalcomponent.Text, txtjbAnyadditionalcomponentcat1.Text, "", "", "", "", sentence);
            }
        }

        #endregion

        #region 2.WMM-22

        if (ddlConstype.Text == "WMM-22")
        {
            if (chkWMM23WeldedMainBody.Checked == false && ChkWMM23GlandPlate.Checked == false &&
                ChkWMM23Canopy.Checked == false && ChkWMM23ComponentMtgPlate.Checked == false &&
                ChkWMM23SideCPlate.Checked == false && ChkWMM23DoorCPlate.Checked == false &&
                ChkWMM23WallMtgBracket.Checked == false && ChkWMM23FrontDoor.Checked == false &&
                ChkWMM23RearDoor.Checked == false && ChkWMM23DoorStiffener.Checked == false &&
                ChkWMM23Lock.Checked == false && ChkWMM23CableSupportAngle.Checked == false &&
                ChkWMM23PowerCoatingShade.Checked == false && ChkWMM23LiftingIBolt.Checked == false
                && ChkWMM23Base.Checked == false && ChkWMM23TransparentDoor.Checked == false && chkWMM23fan.Checked == false
                && chkWMM23Jointlesspolyurethanefoamedinplacegasketing.Checked == false && ChkWMM23Anyadditionalcomponent.Checked == false)
            {
                validation = 1;
            }

            action = "insertWMM23";
            //1
            if (chkWMM23WeldedMainBody.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlWMM23WeldedMainBodycat1.SelectedItem.Text != "Select")
                {
                    first = ddlWMM23WeldedMainBodycat1.SelectedItem.Text;
                    sentence = chkWMM23WeldedMainBody.Text + " " + first;
                }
                if (ddlWMM23WeldedMainBodycat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlWMM23WeldedMainBodycat2.SelectedItem.Text;
                    sentence += " in " + second + " in thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, chkWMM23WeldedMainBody.Text, first, second, "", "", "", sentence);
            }

            //2
            if (ChkWMM23GlandPlate.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlWMM23GlandPlatecat1.SelectedItem.Text != "Select")
                {
                    first = ddlWMM23GlandPlatecat1.SelectedItem.Text;
                    sentence = ChkWMM23GlandPlate.Text + ": " + first;
                }
                if (ddlWMM23GlandPlatecat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlWMM23GlandPlatecat2.SelectedItem.Text;
                    sentence += " in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23GlandPlate.Text, first, second, "", "", "", sentence);
            }

            //3
            if (ChkWMM23Canopy.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";
                if (ddlWMM23Canopycat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlWMM23Canopycat1.SelectedItem.Text;
                    sentence = ChkWMM23Canopy.Text + ": " + second;
                }
                if (ddlWMM23Canopycat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlWMM23Canopycat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23Canopy.Text, second, third, "", "", "", sentence);
            }

            //4
            if (ChkWMM23ComponentMtgPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlWMM23ComponentMtgPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlWMM23ComponentMtgPlatecat1.SelectedItem.Text;
                    sentence = ChkWMM23ComponentMtgPlate.Text + ": " + second + " ";
                }
                if (ddlWMM23ComponentMtgPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlWMM23ComponentMtgPlatecat2.SelectedItem.Text;
                    sentence += "colour " + third;
                }
                if (ddlWMM23ComponentMtgPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlWMM23ComponentMtgPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23ComponentMtgPlate.Text, second, third, fourth, "", "", sentence);
            }

            //5
            if (ChkWMM23SideCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";
                if (ddlWMM23SideCPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlWMM23SideCPlatecat1.SelectedItem.Text;
                    sentence = ChkWMM23SideCPlate.Text + ": " + second;
                }
                if (ddlWMM23SideCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlWMM23SideCPlatecat2.SelectedItem.Text;
                    sentence += " colour " + third;
                }
                if (ddlWMM23SideCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlWMM23SideCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness" + ",";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23SideCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //6
            if (ChkWMM23DoorCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlWMM23DoorCPlatecat2.SelectedItem.Text != "Select Size")
                {
                    second = ddlWMM23DoorCPlatecat1.SelectedItem.Text;
                    sentence = ChkWMM23SideCPlate.Text + ": " + second;
                }
                if (ddlWMM23DoorCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlWMM23DoorCPlatecat2.SelectedItem.Text;
                    sentence += " colour " + third;
                }
                if (ddlWMM23DoorCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlWMM23DoorCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23DoorCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //7
            if (ChkWMM23WallMtgBracket.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlWMM23WallMtgBracketcat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlWMM23WallMtgBracketcat1.SelectedItem.Text;
                    sentence = ChkWMM23WallMtgBracket.Text + ": " + second + " size";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23WallMtgBracket.Text, second, "", "", "", "", sentence);
            }

            //8
            if (ChkWMM23FrontDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";
                if (ddlWMM23FrontDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlWMM23FrontDoorcat1.SelectedItem.Text;
                    sentence = ChkWMM23FrontDoor.Text + ": " + second;
                }
                if (ddlWMM23FrontDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlWMM23FrontDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23FrontDoor.Text, second, third, "", "", "", sentence);
            }

            //9
            if (ChkWMM23RearDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlWMM23RearDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlWMM23RearDoorcat1.SelectedItem.Text;
                    sentence = ChkWMM23FrontDoor.Text + ": " + second;
                }
                if (ddlWMM23RearDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlWMM23RearDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23RearDoor.Text, second, third, "", "", "", sentence);
            }
            //10
            if (CheckboxinnerDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlinnerdoor.SelectedItem.Text != "Select")
                {
                    second = ddlinnerdoor.SelectedItem.Text;
                    sentence = "Inner Door with " + ": " + second;
                }
                if (ddlthickness.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlthickness.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23RearDoor.Text, second, third, "", "", "", sentence);
            }
            //11
            if (ChkWMM23DoorStiffener.Checked == true)
            {
                validation = 0;
                string sentence = "";
                sentence = ChkWMM23DoorStiffener.Text;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23DoorStiffener.Text, ChkWMM23DoorStiffener.Text, "", "", "", "", sentence);
            }

            //12
            if (ChkWMM23Lock.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlWMM23Lockcat1.SelectedItem.Text != "Select")
                {
                    second = ddlWMM23Lockcat1.SelectedItem.Text;
                    sentence = ChkWMM23Lock.Text + ": " + second;
                }
                if (txtWMM23Lockcat2.Text != "")
                {
                    sentence += " with qty " + txtWMM23Lockcat2.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23Lock.Text, second, txtWMM23Lockcat2.Text, "", "", "", sentence);
            }

            //13
            if (ChkWMM23CableSupportAngle.Checked == true)
            {
                validation = 0;
                string sentence = "";

                sentence = ChkWMM23CableSupportAngle.Text + ": ";
                if (txtWMM23CableSupportAnglecat1.Text != "")
                {
                    sentence += " qty" + txtWMM23CableSupportAnglecat1.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23CableSupportAngle.Text, txtWMM23CableSupportAnglecat1.Text, "", "", "", "", sentence);
            }

            //14
            if (ChkWMM23PowerCoatingShade.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlWMM23PowerCoatingShadecat1.SelectedItem.Text != "Select Colour")
                {
                    first = ddlWMM23PowerCoatingShadecat1.SelectedItem.Text;
                    sentence = ChkWMM23PowerCoatingShade.Text + ": " + first + " " + txtddlWMM23PowerCoatingShadecat2.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23PowerCoatingShade.Text, first, txtddlWMM23PowerCoatingShadecat2.Text, "", "", "", sentence);
            }

            //15
            if (ChkWMM23LiftingIBolt.Checked == true)
            {
                validation = 0;
                string sentence = "";
                sentence = ChkWMM23LiftingIBolt.Text;

                if (txtWMM23LiftingIBoltcat1.Text != "")
                {
                    sentence = ChkWMM23LiftingIBolt.Text + ":" + " with qty" + txtWMM23LiftingIBoltcat1.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23LiftingIBolt.Text, txtWMM23LiftingIBoltcat1.Text, "", "", "", "", sentence);
            }

            //16
            if (ChkWMM23Base.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlWMM23Basecat1.SelectedItem.Text != "Select")
                {
                    second = ddlWMM23Basecat1.SelectedItem.Text;
                    sentence = ChkWMM23Base.Text + ": " + second + " ";
                }
                if (ddlWMM23Basecat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlWMM23Basecat2.SelectedItem.Text;
                    sentence += third + " thickness " + ddlWMM23Basecat2.Text;
                }
                if (txtWMM23Basecat3.Text != "")
                {
                    sentence += " " + txtWMM23Basecat3.Text + ",";
                }

                if (ddlheight.Text != "")
                {
                    sentence += "Height " + ddlheight.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23Base.Text, second, third, txtWMM23Basecat3.Text, "", "", sentence);
            }

            //17
            if (ChkWMM23TransparentDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlWMM23TransparentDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlWMM23TransparentDoorcat1.SelectedItem.Text;
                    sentence = ChkWMM23TransparentDoor.Text + ": " + second;
                }
                if (ddlWMM23TransparentDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlWMM23TransparentDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlWMM23TransparentDoorcat3.SelectedItem.Text != "Select")
                {
                    fourth = ddlWMM23TransparentDoorcat3.SelectedItem.Text;
                    sentence += fourth;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23TransparentDoor.Text, second, third, fourth, "", "", sentence);
            }

            if (chkWMM23fan.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlWMM23fancat1.SelectedItem.Text != "Select Size")
                {
                    first = ddlWMM23fancat1.SelectedItem.Text;
                    sentence = chkWMM23fan.Text + ": In Size " + first + " inch qty: " + txtWMM23fancat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, chkWMM23fan.Text, first, txtWMM23fancat2.Text, "", "", "", sentence);
            }

            //18
            if (chkWMM23Jointlesspolyurethanefoamedinplacegasketing.Checked == true)
            {
                validation = 0;
                dtConstructionType.Rows.Add(quotationno, quotationid, chkWMM23Jointlesspolyurethanefoamedinplacegasketing.Text, "", "", "", "", "", chkWMM23Jointlesspolyurethanefoamedinplacegasketing.Text);
            }

            //19
            if (ChkWMM23Anyadditionalcomponent.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                sentence = ChkWMM23Anyadditionalcomponent.Text + ":";

                if (txtWMM23Anyadditionalcomponentcat1.Text != "")
                {
                    first = txtWMM23Anyadditionalcomponentcat1.Text;
                    sentence += " " + first;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23Anyadditionalcomponent.Text, first, "", "", "", "", sentence);
            }
        }
        #endregion

        #region 3.WMM-30 (MCC Box)

        if (ddlConstype.Text == "WMM-30 (MCC Box)")
        {
            if (ChkWMM30WeldedMainBody.Checked == false && ChkWMM30GlandPlat.Checked == false
                && ChkWMM30Canopy.Checked == false && ChkWMM30ComponentMtgPlate.Checked == false
                && ChkWMM30SideCPlate.Checked == false && ChkWMM30DoorCPlate.Checked == false
                && ChkWMM30WallMtgBracket.Checked == false && ChkWMM30FrontDoor.Checked == false
                && ChkWMM30RearDoor.Checked == false && ChkWMM30DoorStiffener.Checked == false
                && ChkWMM30Lock.Checked == false && ChkWMM30CableSupportAngle.Checked == false
                && ChkWMM30PowerCoatingShade.Checked == false && ChkWMM30LiftingIBolt.Checked == false
                && ChkWMM30Base.Checked == false && ChkWMM30TransparentDoor.Checked == false && ChkWMM30fan.Checked == false
                && ChkWMM30Jointlesspolyurethanefoamedinplacegasketing.Checked == false
                && ChkWMM30Anyadditionalcomponent.Checked == false)
            {
                validation = 1;
            }

            action = "insertWMM30";
            //1
            if (ChkWMM30WeldedMainBody.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlWMM30WeldedMainBodycat1.SelectedItem.Text != "Select")
                {
                    first = ddlWMM30WeldedMainBodycat1.SelectedItem.Text;
                    sentence = ChkWMM30WeldedMainBody.Text + " " + first;
                }
                if (ddlWMM30WeldedMainBodycat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlWMM30WeldedMainBodycat2.SelectedItem.Text;
                    sentence += " in " + second + " in thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30WeldedMainBody.Text, first, second, "", "", "", sentence);
            }

            //2
            if (ChkWMM30GlandPlat.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlWMM30GlandPlatcat1.SelectedItem.Text != "Select")
                {
                    first = ddlWMM30GlandPlatcat1.SelectedItem.Text;
                    sentence = ChkWMM30GlandPlat.Text + ": " + first;
                }
                if (ddlWMM30GlandPlatcat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlWMM30GlandPlatcat2.SelectedItem.Text;
                    sentence += " in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30GlandPlat.Text, first, second, "", "", "", sentence);
            }

            //3
            if (ChkWMM30Canopy.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlWMM30Canopycat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlWMM30Canopycat1.SelectedItem.Text;
                    sentence = ChkWMM30Canopy.Text + ": " + second;
                }
                if (ddlWMM30Canopycat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlWMM30Canopycat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30Canopy.Text, second, third, "", "", "", sentence);
            }

            //4
            if (ChkWMM30ComponentMtgPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlWMM30ComponentMtgPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlWMM30ComponentMtgPlatecat1.SelectedItem.Text;
                    sentence = ChkWMM30ComponentMtgPlate.Text + ": " + second + " ";
                }
                if (ddlWMM30ComponentMtgPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlWMM30ComponentMtgPlatecat2.SelectedItem.Text;
                    sentence += "colour " + third;
                }
                if (ddlWMM30ComponentMtgPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlWMM30ComponentMtgPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30ComponentMtgPlate.Text, second, third, fourth, "", "", sentence);
            }

            //5
            if (ChkWMM30SideCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlWMM30SideCPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlWMM30SideCPlatecat1.SelectedItem.Text;
                    sentence = ChkWMM30SideCPlate.Text + ": " + second;
                }
                if (ddlWMM30SideCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlWMM30SideCPlatecat2.SelectedItem.Text;
                    sentence += " colour " + third;
                }
                if (ddlWMM30SideCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlWMM30SideCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness" + ",";
                }


                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30SideCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //6
            if (ChkWMM30DoorCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlWMM30DoorCPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlWMM30DoorCPlatecat1.SelectedItem.Text;
                    sentence = ChkWMM30DoorCPlate.Text + ": " + second;
                }
                if (ddlWMM30DoorCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlWMM30DoorCPlatecat2.SelectedItem.Text;
                    sentence += " colour " + third;
                }
                if (ddlWMM30DoorCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlWMM30DoorCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30DoorCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //7
            if (ChkWMM30WallMtgBracket.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";
                if (ddlWMM30WallMtgBracketcat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlWMM30WallMtgBracketcat1.SelectedItem.Text;
                    sentence = ChkWMM30WallMtgBracket.Text + ": " + second + " size";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30WallMtgBracket.Text, second, "", "", "", "", sentence);
            }

            //8
            if (ChkWMM30FrontDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlWMM30FrontDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlWMM30FrontDoorcat1.SelectedItem.Text;
                    sentence = ChkWMM30FrontDoor.Text + ": " + second;
                }
                if (ddlWMM30FrontDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlWMM30FrontDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30FrontDoor.Text, second, third, "", "", "", sentence);
            }

            //9
            if (ChkWMM30RearDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlWMM30RearDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlWMM30RearDoorcat1.SelectedItem.Text;
                    sentence = ChkWMM30RearDoor.Text + ": " + second;
                }
                if (ddlWMM30RearDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlWMM30RearDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30RearDoor.Text, second, third, "", "", "", sentence);
            }

            //10
            if (ChkWMM30DoorStiffener.Checked == true)
            {
                validation = 0;
                string sentence = "";
                sentence = ChkWMM30DoorStiffener.Text;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30DoorStiffener.Text, "", "", "", "", "", sentence);
            }
            //11
            if (ChkWMM30Lock.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlWMM30Lockcat1.SelectedItem.Text != "Select")
                {
                    second = ddlWMM30Lockcat1.SelectedItem.Text;
                    sentence = ChkWMM30Lock.Text + ": " + second;
                }

                if (txtWMM30Lockcat2.Text != "")
                {
                    sentence += " with qty " + txtWMM30Lockcat2.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30Lock.Text, second, txtWMM30Lockcat2.Text, "", "", "", sentence);
            }

            //12
            if (ChkWMM30CableSupportAngle.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                sentence = ChkWMM30CableSupportAngle.Text + ": ";
                if (txtWMM30CableSupportAnglecat1.Text != "")
                {
                    first = txtWMM30CableSupportAnglecat1.Text;
                    sentence += ChkWMM30CableSupportAngle.Text + " with qty " + txtWMM30CableSupportAnglecat1.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30CableSupportAngle.Text, first, txtWMM30CableSupportAnglecat1.Text, "", "", "", sentence);
            }

            //13
            if (ChkWMM30PowerCoatingShade.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlWMM30PowerCoatingShadecat1.SelectedItem.Text != "Select Colour")
                {
                    first = ddlWMM30PowerCoatingShadecat1.SelectedItem.Text;
                    sentence = ChkWMM30PowerCoatingShade.Text + ": " + first + " " + txtWMM30PowerCoatingShadecat2.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30PowerCoatingShade.Text, first, txtWMM30PowerCoatingShadecat2.Text, "", "", "", sentence);
            }

            //14
            if (ChkWMM30LiftingIBolt.Checked == true)
            {
                validation = 0;
                string sentence = "";
                sentence = ChkWMM30LiftingIBolt.Text;
                if (txtWMM30LiftingIBoltcat1.Text != "")
                {
                    sentence += ": with qty " + txtWMM30LiftingIBoltcat1.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30LiftingIBolt.Text, txtWMM30LiftingIBoltcat1.Text, "", "", "", "", sentence);
            }

            //15
            if (ChkWMM30Base.Checked == true)
            {
                validation = 0;
                string first = "", second = "", third = "", sentence = "";
                if (ddlWMM30Basecat1.SelectedItem.Text != "Select")
                {
                    first = ddlWMM30Basecat1.SelectedItem.Text;
                }
                if (ddlWMM30Basecat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlWMM30Basecat2.SelectedItem.Text;
                    sentence = ChkWMM30Base.Text + ": " + second + " thickness ";
                }
                if (ddlWMM30Basecat3.SelectedItem.Text != "Select Height")
                {
                    third = ddlWMM30Basecat3.SelectedItem.Text;
                    sentence += "and " + third + " Height";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30Base.Text, first, second, third, "", "", sentence);
            }

            //16
            if (ChkWMM30TransparentDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";
                if (ddlWMM30TransparentDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlWMM30TransparentDoorcat1.SelectedItem.Text;
                    sentence = ChkWMM30TransparentDoor.Text + ": " + second;
                }
                if (ddlWMM30TransparentDoorcat2.SelectedItem.Text != "Select")
                {
                    third = ddlWMM30TransparentDoorcat2.SelectedItem.Text;

                    if (ddlWMM30TransparentDoorcat2.SelectedItem.Text == "Specify")
                    {
                        sentence += " " + txtWMM30TransparentDoorcat5.Text + " ";
                    }
                    else
                        sentence += " " + third + " ";
                }
                if (ddlWMM30TransparentDoorcat3.SelectedItem.Text != "Select")
                {
                    fourth = ddlWMM30TransparentDoorcat3.SelectedItem.Text;
                    sentence += fourth + " " + txtWMM30TransparentDoorcat4.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30TransparentDoor.Text, second, third, fourth, txtWMM30TransparentDoorcat4.Text, txtWMM30TransparentDoorcat5.Text, sentence);
            }
            //17
            if (ChkWMM30fan.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlWMM30fancat1.SelectedItem.Text != "Select Size")
                {
                    first = ddlWMM30fancat1.SelectedItem.Text;
                    sentence = ChkWMM30fan.Text + ": In Size " + first + " inch qty: " + txtWMM30fancat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30fan.Text, first, txtWMM30fancat2.Text, "", "", "", sentence);
            }
            //18
            if (ChkWMM30Jointlesspolyurethanefoamedinplacegasketing.Checked == true)
            {
                validation = 0;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30Jointlesspolyurethanefoamedinplacegasketing.Text, "", "", "", "", "", ChkWMM30Jointlesspolyurethanefoamedinplacegasketing.Text);
            }

            //19
            if (ChkWMM30Anyadditionalcomponent.Checked == true)
            {
                validation = 0;
                string sentence = "";
                sentence = ChkWMM30Anyadditionalcomponent.Text;
                if (txtWMM30Anyadditionalcomponentcat1.Text != "")
                {
                    sentence += ": " + txtWMM30Anyadditionalcomponentcat1.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM30Anyadditionalcomponent.Text, txtWMM30Anyadditionalcomponentcat1.Text, "", "", "", "", sentence);
            }
        }
        #endregion

        #region 4.MFS (Modular Floor Standing Enclosure)

        if (ddlConstype.Text == "MFS (Modular Floor Standing Enclosure)")
        {
            action = "insertMFS";

            if (ChkMFSMainframeStructureWelded.Checked == false && ChkMFSBottomCover.Checked == false
                && ChkMFSGlandPlate.Checked == false && ChkMFSComponentMtgPlate.Checked == false
                && ChkMFSSideCPlate.Checked == false && ChkMFSDoorCPlate.Checked == false
                && ChkMFSPartialMountingPlate.Checked == false && ChkMFSFillerTray.Checked == false
                && ChkMFSFrontDoor.Checked == false && ChkMFSRearDoor.Checked == false
                && ChkMFSLock.Checked == false && ChkMFSRearCover.Checked == false
                && ChkMFSSideCover.Checked == false && ChkMFSTopCover.Checked == false
                && ChkMFSPowerCoatingShade.Checked == false && ChkMFSLiftingArrangement.Checked == false
                && ChkMFSBase.Checked == false && ChkMFSAntivibrationpad.Checked == false
                && ChkMFSDrawingPocket.Checked == false && ChkMFSMicroswitchbracket.Checked == false
                && ChkMFSTubelightBracket.Checked == false && ChkMFSCanopy.Checked == false && ChkMFSfan.Checked == false
                && ChkMFSJointlesspolyurethanefoamedinplacegasketing.Checked == false
                && ChkMFSAnyadditionalcomponent.Checked == false)
            {
                validation = 1;
            }

            //1
            if (ChkMFSMainframeStructureWelded.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlMFSMainframeStructureWeldedcat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlMFSMainframeStructureWeldedcat1.SelectedItem.Text;
                    sentence = ChkMFSMainframeStructureWelded.Text + " in " + first + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSMainframeStructureWelded.Text, first, "", "", "", "", sentence);
            }

            //2
            if (ChkMFSBottomCover.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlMFSBottomCovercat1.SelectedItem.Text != "Select")
                {
                    first = ddlMFSBottomCovercat1.SelectedItem.Text;
                    sentence = ChkMFSBottomCover.Text + ": " + first;
                }
                if (ddlMFSBottomCovercat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlMFSBottomCovercat2.SelectedItem.Text;
                    sentence += " in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSBottomCover.Text, first, second, "", "", "", sentence);
            }

            //3
            if (ChkMFSGlandPlate.Checked == true)
            {
                validation = 0;
                string first = "", second = "", third = "", sentence = "";
                if (ddlMFSGlandPlatecat1.SelectedItem.Text != "Select")
                {
                    first = ddlMFSGlandPlatecat1.SelectedItem.Text;
                }
                if (ddlMFSGlandPlatecat2.SelectedItem.Text != "Select Size")
                {
                    second = ddlMFSGlandPlatecat2.SelectedItem.Text;
                    sentence = ChkMFSGlandPlate.Text + ": " + first + " " + second;
                }
                if (ddlMFSGlandPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlMFSGlandPlatecat3.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSGlandPlate.Text, first, second, third, "", "", sentence);
            }

            //4
            if (ChkMFSComponentMtgPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlMFSComponentMtgPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlMFSComponentMtgPlatecat1.SelectedItem.Text;
                    sentence = ChkMFSComponentMtgPlate.Text + ": " + second + " ";
                }
                if (ddlMFSComponentMtgPlatecat2.SelectedItem.Text != "Select Color")
                {
                    third = ddlMFSComponentMtgPlatecat2.SelectedItem.Text;
                    //sentence += "colour " + third;
                }
                if (ddlMFSComponentMtgPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlMFSComponentMtgPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness" + " " + txtMFSComponentMtgPlatecat4.Text + " qty" + " In Colour " + third;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSComponentMtgPlate.Text, second, third, fourth, txtMFSComponentMtgPlatecat4.Text, "", sentence);
            }

            //5
            if (ChkMFSSideCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";
                if (ddlMFSSideCPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlMFSSideCPlatecat1.SelectedItem.Text;
                    sentence = ChkMFSSideCPlate.Text + ": " + second + " ";
                }
                if (ddlMFSSideCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlMFSSideCPlatecat2.SelectedItem.Text;
                    sentence += "colour " + third;
                }
                if (ddlMFSSideCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlMFSSideCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness" + " " + txtMFSSideCPlatecat4.Text + " qty";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSSideCPlate.Text, second, third, fourth, txtMFSSideCPlatecat4.Text, "", sentence);
            }

            //6
            if (ChkMFSDoorCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlMFSDoorCPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlMFSDoorCPlatecat1.SelectedItem.Text;
                    sentence = ChkMFSDoorCPlate.Text + ": " + second + " ";
                }
                if (ddlMFSDoorCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlMFSDoorCPlatecat2.SelectedItem.Text;
                    sentence += "colour " + third;
                }
                if (ddlMFSDoorCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlMFSDoorCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness" + " " + txtMFSDoorCPlatecat4.Text + " qty";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSDoorCPlate.Text, second, third, fourth, txtMFSDoorCPlatecat4.Text, "", sentence);
            }

            //7
            if (ChkMFSPartialMountingPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlMFSPartialMountingPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlMFSPartialMountingPlatecat1.SelectedItem.Text;
                    sentence = ChkMFSPartialMountingPlate.Text + ": " + second + " ";
                }
                if (ddlMFSPartialMountingPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlMFSPartialMountingPlatecat2.SelectedItem.Text;
                    sentence += "colour " + third;
                }
                if (ddlMFSPartialMountingPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlMFSPartialMountingPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness" + " " + txtMFSPartialMountingPlatecat4.Text + " qty";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSPartialMountingPlate.Text, second, third, fourth, txtMFSPartialMountingPlatecat4.Text, "", sentence);
            }

            //8
            if (ChkMFSFillerTray.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlMFSFillerTraycat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlMFSFillerTraycat1.SelectedItem.Text;
                    sentence = ChkMFSFillerTray.Text + ": " + second + " ";
                }
                if (ddlMFSFillerTraycat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlMFSFillerTraycat2.SelectedItem.Text;
                    sentence += "colour " + third;
                }
                if (ddlMFSFillerTraycat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlMFSFillerTraycat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness" + " " + txtMFSFillerTraycat4.Text + " qty";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSFillerTray.Text, second, third, fourth, txtMFSFillerTraycat4.Text, "", sentence);
            }

            //9
            if (ChkMFSFrontDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlMFSFrontDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlMFSFrontDoorcat1.SelectedItem.Text;
                    sentence = ChkMFSFrontDoor.Text + ": " + second;
                }
                if (ddlMFSFrontDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlMFSFrontDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSFrontDoor.Text, second, third, "", "", "", sentence);
            }

            //10
            if (ChkMFSRearDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlMFSRearDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlMFSRearDoorcat1.SelectedItem.Text;
                    sentence = ChkMFSRearDoor.Text + ": " + second;
                }
                if (ddlMFSRearDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlMFSRearDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSRearDoor.Text, second, third, "", "", "", sentence);
            }
            //11
            if (ChkMFSLock.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlMFSLockcat1.SelectedItem.Text != "Select")
                {
                    second = ddlMFSLockcat1.SelectedItem.Text;
                    sentence = ChkMFSLock.Text + ": " + second + ",";
                }

                if (txtmsfqty.Text != "")
                {
                    second = txtmsfqty.Text;
                    sentence += " Qty " + second;
                }


                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSLock.Text, second, "", "", "", "", sentence);
            }

            //12
            if (ChkMFSRearCover.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlMFSRearCovercat1.SelectedItem.Text != "Select")
                {
                    second = ddlMFSRearCovercat1.SelectedItem.Text;
                    sentence = ChkMFSRearCover.Text + ": in " + second + " thickness ";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSRearCover.Text, second, "", "", "", "", sentence);
            }

            //13
            if (ChkMFSSideCover.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlMFSSideCovercat1.SelectedItem.Text != "Select")
                {
                    second = ddlMFSSideCovercat1.SelectedItem.Text;
                    sentence = ChkMFSSideCover.Text + ": in " + second + " thickness ";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSSideCover.Text, second, "", "", "", "", sentence);
            }

            //14
            if (ChkMFSTopCover.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlMFSTopCovercat1.SelectedItem.Text != "Select")
                {
                    second = ddlMFSTopCovercat1.SelectedItem.Text;
                    sentence = ChkMFSTopCover.Text + ": in " + second + " thickness ";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSTopCover.Text, second, "", "", "", "", sentence);
            }

            //15
            if (ChkMFSPowerCoatingShade.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                if (ddlMFSPowerCoatingShadecat1.SelectedItem.Text != "Select Color")
                {
                    first = ddlMFSPowerCoatingShadecat1.SelectedItem.Text + ",";
                }

                if (txtMFSPowerCoatingShadecat2.Text != "")
                {
                    first += "Specify : " + txtMFSPowerCoatingShadecat2.Text;
                }

                //sentence = ChkMFSPowerCoatingShade.Text + ": colour " + first + " " + txtMFSPowerCoatingShadecat2.Text;
                //dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSPowerCoatingShade.Text, first, txtMFSPowerCoatingShadecat2.Text, "", "", "", sentence);
                sentence = ChkMFSPowerCoatingShade.Text + ": colour " + first;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSPowerCoatingShade.Text, first, "", "", "", sentence);
            }

            //16
            if (ChkMFSLiftingArrangement.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlMFSLiftingArrangementcat1.SelectedItem.Text != "Select")
                {
                    second = ddlMFSLiftingArrangementcat1.SelectedItem.Text;
                    sentence = ChkMFSLiftingArrangement.Text + ": " + second;
                }
                if (ddlMFSLiftingArrangementcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlMFSLiftingArrangementcat2.SelectedItem.Text;
                    sentence += " " + third + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSLiftingArrangement.Text, second, third, "", "", "", sentence);
            }

            //17
            if (ChkMFSBase.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlMFSBasecat1.SelectedItem.Text != "Select")
                {
                    second = ddlMFSBasecat1.SelectedItem.Text;
                    sentence = ChkMFSBase.Text + ": " + second;
                }
                if (ddlMFSBasecat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlMFSBasecat2.SelectedItem.Text;
                    sentence += " " + third + " thickness";
                }
                if (ddlMFSBasecat3.SelectedItem.Text != "Select Height")
                {
                    fourth = ddlMFSBasecat3.SelectedItem.Text;
                    sentence += " " + fourth + " height" + " ";
                }
                if (ddllistforshitMfs.SelectedItem.Text != "Select")
                {
                    fourth = ddllistforshitMfs.SelectedItem.Text;
                    sentence += "With " + fourth;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSBase.Text, second, third, fourth, "", "", sentence);
            }

            //18
            if (ChkMFSAntivibrationpad.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlMFSAntivibrationpadcat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlMFSAntivibrationpadcat1.SelectedItem.Text;
                    sentence = ChkMFSAntivibrationpad.Text + ": In" + second + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSAntivibrationpad.Text, second, "", "", "", "", sentence);
            }



            if (ChkWMM23TransparentDoorMFS.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlWMM23TransparentDoorcat1MFS1.SelectedItem.Text != "Select")
                {
                    second = ddlWMM23TransparentDoorcat1MFS1.SelectedItem.Text;
                    sentence = ChkWMM23TransparentDoorMFS.Text + ": " + second;
                }
                if (ddlWMM23TransparentDoorcat2MFS.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlWMM23TransparentDoorcat2MFS.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlWMM23TransparentDoorcat3MFS.SelectedItem.Text != "Select")
                {
                    fourth = ddlWMM23TransparentDoorcat3MFS.SelectedItem.Text;
                    sentence += fourth;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23TransparentDoorMFS.Text, second, third, fourth, "", "", sentence);
            }


            //19
            if (ChkMFSDrawingPocket.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlMFSDrawingPocketcat1.SelectedItem.Text != "Select")
                {
                    second = ddlMFSDrawingPocketcat1.SelectedItem.Text;
                    sentence = ChkMFSDrawingPocket.Text + ": " + second + " ";
                }
                if (ddlMFSDrawingPocketcat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlMFSDrawingPocketcat2.SelectedItem.Text;
                    sentence += "colour " + third;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSDrawingPocket.Text, second, third, "", "", "", sentence);
            }

            //20
            if (ChkMFSMicroswitchbracket.Checked == true)
            {
                validation = 0;
                string sentence = "";
                sentence = ChkMFSMicroswitchbracket.Text;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSMicroswitchbracket.Text, "", "", "", "", "", sentence);
            }

            //21
            if (ChkMFSTubelightBracket.Checked == true)
            {
                validation = 0;
                string sentence = "";

                sentence = ChkMFSTubelightBracket.Text;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSTubelightBracket.Text, "", "", "", "", "", sentence);
            }

            //22
            if (ChkMFSCanopy.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlMFSCanopycat1.SelectedItem.Text != "Select")
                {
                    second = ddlMFSCanopycat1.SelectedItem.Text;
                    sentence = ChkMFSCanopy.Text + ": " + second + " ";
                }
                if (ddlMFSCanopycat2.SelectedItem.Text != "Select")
                {
                    third = ddlMFSCanopycat2.SelectedItem.Text;
                    sentence += third + " ";
                }
                if (ddlMFSCanopycat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlMFSCanopycat3.SelectedItem.Text;
                    sentence += fourth + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSCanopy.Text, second, third, fourth, "", "", sentence);
            }
            //23
            if (ChkMFSfan.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlMFSfancat1.SelectedItem.Text != "Select Size")
                {
                    first = ddlMFSfancat1.SelectedItem.Text;
                    sentence = ChkMFSfan.Text + ": In Size " + first + " inch qty: " + txtMFSfancat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSfan.Text, first, txtMFSfancat2.Text, "", "", "", sentence);
            }
            //24
            if (ChkMFSJointlesspolyurethanefoamedinplacegasketing.Checked == true)
            {
                validation = 0;
                string sentence = ChkMFSJointlesspolyurethanefoamedinplacegasketing.Text;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSJointlesspolyurethanefoamedinplacegasketing.Text, "", "", "", "", "", sentence);
            }
            //25
            if (ChkMFSAnyadditionalcomponent.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                sentence = ChkMFSAnyadditionalcomponent.Text;
                if (txtMFSAnyadditionalcomponentcat1.Text != "")
                {
                    first = txtMFSAnyadditionalcomponentcat1.Text;
                    sentence += ": " + first;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkMFSAnyadditionalcomponent.Text, first, "", "", "", "", sentence);
            }
        }
        #endregion

        #region 5.Eco MCC 30mm

        if (ddlConstype.Text == "Eco MCC 30mm")
        {
            if (ChkEcoMCCMainframeStructureWelded.Checked == false &&
                ChkEcoMCCGlandPlate.Checked == false && ChkEcoMCCComponentMtgPlate.Checked == false
                && ChkEcoMCCSideCPlate.Checked == false && ChkEcoMCCDoorCPlate.Checked == false
                && ChkEcoMCCFrontDoor.Checked == false && ChkEcoMCCRearDoor.Checked == false
                && ChkEcoMCCRearCover.Checked == false && ChkEcoMCCSideCover.Checked == false
                && ChkEcoMCCLock.Checked == false && ChkEcoMCCPowerCoatingShade.Checked == false
                && ChkEcoMCCLiftingArrangement.Checked == false && ChkEcoMCCBase.Checked == false
                && ChkEcoMCCAntivibrationpad.Checked == false && ChkEcoMCCDrawingPocket.Checked == false
                && ChkEcoMCCTubelightBracket.Checked == false && ChkEcoMCCCanopy.Checked == false && ChkEcoMCCfan.Checked == false
                && ChkEcoMCCJointlesspolyurethanefoamedinplacegasketing.Checked == false
                && ChkEcoMCCAnyadditionalcomponent.Checked == false)
            {
                validation = 1;
            }

            action = "insertEcoMCC";
            //1
            if (ChkEcoMCCMainframeStructureWelded.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                if (ddlEcoMCCMainframeStructureWeldedcat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlEcoMCCMainframeStructureWeldedcat1.SelectedItem.Text;
                    sentence = ChkEcoMCCMainframeStructureWelded.Text + " in " + first + " in thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCMainframeStructureWelded.Text, first, "", "", "", "", sentence);
            }

            //2
            if (ChkEcoMCCGlandPlate.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlEcoMCCGlandPlatecat1.SelectedItem.Text != "Select")
                {
                    first = ddlEcoMCCGlandPlatecat1.SelectedItem.Text;
                    sentence = ChkEcoMCCGlandPlate.Text + ": " + first;
                }
                if (ddlEcoMCCGlandPlatecat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlEcoMCCGlandPlatecat2.SelectedItem.Text;
                    sentence += " in " + second + " Thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCGlandPlate.Text, first, second, "", "", "", sentence);
            }

            //3
            if (ChkEcoMCCComponentMtgPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlEcoMCCComponentMtgPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlEcoMCCComponentMtgPlatecat1.SelectedItem.Text;
                    sentence = ChkEcoMCCComponentMtgPlate.Text + ": " + second;
                }
                if (ddlEcoMCCComponentMtgPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlEcoMCCComponentMtgPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlEcoMCCComponentMtgPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlEcoMCCComponentMtgPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                if (txtEcoMCCComponentMtgPlatecat4.Text != "")
                {
                    sentence += txtEcoMCCComponentMtgPlatecat4.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCComponentMtgPlate.Text, second, third, fourth, txtEcoMCCComponentMtgPlatecat4.Text, "", sentence);
            }

            //4
            if (ChkEcoMCCSideCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlEcoMCCSideCPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlEcoMCCSideCPlatecat1.SelectedItem.Text;
                    sentence = ChkEcoMCCSideCPlate.Text + ": " + second;
                }
                if (ddlEcoMCCSideCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlEcoMCCSideCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlEcoMCCSideCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlEcoMCCSideCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                if (txtEcoMCCSideCPlatecat4.Text != "")
                {
                    sentence += txtEcoMCCSideCPlatecat4.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCSideCPlate.Text, second, third, fourth, txtEcoMCCSideCPlatecat4.Text, "", sentence);
            }

            //5
            if (ChkEcoMCCDoorCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlEcoMCCDoorCPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlEcoMCCDoorCPlatecat1.SelectedItem.Text;
                    sentence = ChkEcoMCCDoorCPlate.Text + ": " + second;
                }
                if (ddlEcoMCCDoorCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlEcoMCCDoorCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlEcoMCCDoorCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlEcoMCCDoorCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                if (txtEcoMCCDoorCPlatecat4.Text != "")
                {
                    sentence += txtEcoMCCDoorCPlatecat4.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCDoorCPlate.Text, second, third, fourth, txtEcoMCCDoorCPlatecat4.Text, "", sentence);
            }

            //6
            if (ChkEcoMCCFrontDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlEcoMCCFrontDoorca1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoMCCFrontDoorca1.SelectedItem.Text;
                    sentence = ChkEcoMCCFrontDoor.Text + ": " + second;
                }
                if (ddlEcoMCCFrontDoorca2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlEcoMCCFrontDoorca2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCFrontDoor.Text, second, third, "", "", "", sentence);
            }

            //7
            if (ChkEcoMCCRearDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlEcoMCCRearDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoMCCRearDoorcat1.SelectedItem.Text;
                    sentence = ChkEcoMCCRearDoor.Text + ": " + second;
                }
                if (ddlEcoMCCRearDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlEcoMCCRearDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCRearDoor.Text, second, third, "", "", "", sentence);
            }

            //8
            if (checkboxinnerdoorEcomc.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlmat.SelectedItem.Text != "Select")
                {
                    second = ddlmat.SelectedItem.Text;
                    sentence = checkboxinnerdoorEcomc.Text + ": " + second;
                }
                if (ddlthicknessforinnerdoor.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlthicknessforinnerdoor.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, checkboxinnerdoorEcomc.Text, second, third, "", "", "", sentence);
            }


            //9
            if (ChkEcoMCCRearCover.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlEcoMCCRearCovercat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlEcoMCCRearCovercat1.SelectedItem.Text;
                    sentence = ChkEcoMCCRearCover.Text + ": in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCRearCover.Text, second, "", "", "", "", sentence);
            }

            //10
            if (ChkEcoMCCSideCover.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlEcoMCCSideCovercat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlEcoMCCSideCovercat1.SelectedItem.Text;
                    sentence = ChkEcoMCCSideCover.Text + ": in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCSideCover.Text, second, "", "", "", "", sentence);
            }

            //11
            if (ChkEcoMCCLock.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlEcoMCCLockcat1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoMCCLockcat1.SelectedItem.Text;
                    sentence = ChkEcoMCCLock.Text + ": " + second;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCLock.Text, second, "", "", "", "", sentence);
            }

            //12
            if (ChkEcoMCCPowerCoatingShade.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlEcoMCCPowerCoatingShadecat1.SelectedItem.Text != "Select Colour")
                {
                    first = ddlEcoMCCPowerCoatingShadecat1.SelectedItem.Text;
                    sentence = ChkEcoMCCPowerCoatingShade.Text + ": " + first + " " + txtEcoMCCPowerCoatingShadecat2.Text;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCPowerCoatingShade.Text, first, txtEcoMCCPowerCoatingShadecat2.Text, "", "", "", sentence);
            }

            //13
            if (ChkEcoMCCLiftingArrangement.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (ddlEcoMCCLiftingArrangementcat1.SelectedItem.Text != "Select")
                {
                    first = ddlEcoMCCLiftingArrangementcat1.SelectedItem.Text;
                    sentence = ChkEcoMCCLiftingArrangement.Text + ": " + first;
                }

                if (ddlEcoMCCLiftingArrangementcat2.Text != "Select Thickness")
                {
                    second = ddlEcoMCCLiftingArrangementcat2.Text;
                    sentence += " in " + ddlEcoMCCLiftingArrangementcat2.Text + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCLiftingArrangement.Text, first, second, "", "", "", sentence);
            }

            //14
            if (ChkEcoMCCBase.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlEcoMCCBasecat1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoMCCBasecat1.SelectedItem.Text;
                    sentence = ChkEcoMCCBase.Text + ": " + second;
                }
                if (ddlEcoMCCBasecat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlEcoMCCBasecat2.SelectedItem.Text;
                    sentence += " in " + second + " thickness ";
                }
                if (ddlEcoMCCBasecat3.SelectedItem.Text != "Select Height")
                {
                    fourth = ddlEcoMCCBasecat3.SelectedItem.Text;
                    sentence += "Height " + fourth + " ";
                }
                if (ddlEcoMCCbaseshits.SelectedItem.Text != "Select ")
                {
                    fourth = ddlEcoMCCbaseshits.SelectedItem.Text;
                    sentence += "Transparent Door With " + fourth;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCBase.Text, second, third, fourth, "", "", sentence);
            }
            //15
            if (ChkWMM23TransparentDoorEcoframe.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlWMM23TransparentDoorcat1Ecoframe.SelectedItem.Text != "Select")
                {
                    second = ddlWMM23TransparentDoorcat1Ecoframe.SelectedItem.Text;
                    sentence = ChkWMM23TransparentDoorEcoframe.Text + ": " + second;
                }
                if (ddlWMM23TransparentDoorcat2Ecoframe.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlWMM23TransparentDoorcat2Ecoframe.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlWMM23TransparentDoorcat3Ecoframe.SelectedItem.Text != "Select")
                {
                    fourth = ddlWMM23TransparentDoorcat3Ecoframe.SelectedItem.Text;
                    sentence += fourth;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23TransparentDoor.Text, second, third, fourth, "", "", sentence);
            }



            //16
            if (ChkEcoMCCAntivibrationpad.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlEcoMCCAntivibrationpadcat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlEcoMCCAntivibrationpadcat1.SelectedItem.Text;
                    sentence = ChkEcoMCCAntivibrationpad.Text + ": in" + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCAntivibrationpad.Text, second, "", "", "", "", sentence);
            }

            //16
            if (ChkEcoMCCDrawingPocket.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlEcoMCCDrawingPocketcat1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoMCCDrawingPocketcat1.SelectedItem.Text;
                    sentence = ChkEcoMCCDrawingPocket.Text + ": " + second;
                }
                if (ddlEcoMCCDrawingPocketcat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlEcoMCCDrawingPocketcat2.SelectedItem.Text;
                    sentence += " colour " + third;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCDrawingPocket.Text, second, third, "", "", "", sentence);
            }




            //17
            if (ChkEcoMCCTubelightBracket.Checked == true)
            {
                validation = 0;
                string sentence = "";
                sentence = ChkEcoMCCTubelightBracket.Text;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCTubelightBracket.Text, sentence, "", "", "", "", sentence);
            }

            //18
            if (ChkEcoMCCMicroswitchbracket.Checked == true)
            {
                validation = 0;
                string sentence = "";
                sentence = ChkEcoMCCMicroswitchbracket.Text;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCMicroswitchbracket.Text, sentence, "", "", "", "", sentence);
            }
            //19
            if (ChkEcoMCCCanopy.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlEcoMCCCanopycat1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoMCCCanopycat1.SelectedItem.Text;
                    sentence = ChkEcoMCCCanopy.Text + ": " + second;
                }
                if (ddlEcoMCCCanopycat2.SelectedItem.Text != "Perforation Hole")
                {
                    third = ddlEcoMCCCanopycat2.SelectedItem.Text;
                    sentence += " " + third;
                }
                if (ddlEcoMCCCanopycat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlEcoMCCCanopycat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCCanopy.Text, second, third, fourth, "", "", sentence);
            }
            //20
            if (ChkEcoMCCfan.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlEcoMCCfancat1.SelectedItem.Text != "Select Size")
                {
                    first = ddlEcoMCCfancat1.SelectedItem.Text;
                    sentence = ChkEcoMCCfan.Text + ": In Size " + first + " inch qty: " + txtEcoMCCfancat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCfan.Text, first, txtEcoMCCfancat2.Text, "", "", "", sentence);
            }
            //20
            if (ChkEcoMCCJointlesspolyurethanefoamedinplacegasketing.Checked == true)
            {
                validation = 0;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCJointlesspolyurethanefoamedinplacegasketing.Text, "", "", "", "", "", ChkEcoMCCJointlesspolyurethanefoamedinplacegasketing.Text);
            }

            //21
            if (ChkEcoMCCAnyadditionalcomponent.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                first = txtEcoMCCAnyadditionalcomponentcat1.Text;
                sentence = ChkEcoMCCAnyadditionalcomponent.Text + ": " + first;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoMCCAnyadditionalcomponent.Text, txtEcoMCCAnyadditionalcomponentcat1.Text, "", "", "", "", sentence);
            }
        }
        #endregion

        #region 6.Modular W-Big 43 mm

        if (ddlConstype.Text == "Modular W-Big 43 mm")
        {
            if (ChkModularWeldedMainBody.Checked == false && ChkModularGlandPlate.Checked == false
                && ChkModularComponentMtgPlate.Checked == false && ChkModularSideCPlate.Checked == false
                && ChkModularDoorCPlate.Checked == false && ChkModularWallMtgBracket.Checked == false
                && ChkModularFrontDoor.Checked == false && ChkModularRearDoor.Checked == false
                && ChkModularRearCover.Checked == false && ChkModularCableSupportAngle.Checked == false
                && ChkModularLock.Checked == false && ChkModularPowerCoatingShade.Checked == false
                && ChkModularLiftingArrangement.Checked == false && ChkModularBase.Checked == false
                && ChkModularAntivibrationpad.Checked == false && ChkModularCanopy.Checked == false && ChkModularfan.Checked == false
                && ChkModularJointlesspolyurethanefoamedinplacegasketing.Checked == false
                && ChkModularAnyadditionalcomponent.Checked == false)
            {
                validation = 1;
            }

            action = "insertModular";
            //1
            if (ChkModularWeldedMainBody.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (ddlModularWeldedMainBodycat1.SelectedItem.Text != "Select")
                {
                    first = ddlModularWeldedMainBodycat1.SelectedItem.Text;
                    sentence = ChkModularWeldedMainBody.Text + " " + first;
                }
                if (ddlModularWeldedMainBodycat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlModularWeldedMainBodycat2.SelectedItem.Text;
                    sentence += " in " + second + " in thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularWeldedMainBody.Text, first, second, "", "", "", sentence);
            }

            //2
            if (ChkModularGlandPlate.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlModularGlandPlatecat1.SelectedItem.Text != "Select")
                {
                    first = ddlModularGlandPlatecat1.SelectedItem.Text;
                    sentence = ChkModularGlandPlate.Text + ": " + first;
                }
                if (ddlModularGlandPlatecat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlModularGlandPlatecat2.SelectedItem.Text;
                    sentence += " in " + second + " Thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularGlandPlate.Text, first, second, "", "", "", sentence);
            }

            //3
            if (ChkModularComponentMtgPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlModularComponentMtgPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlModularComponentMtgPlatecat1.SelectedItem.Text;
                    sentence = ChkModularComponentMtgPlate.Text + ": " + second;
                }
                if (ddlModularComponentMtgPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlModularComponentMtgPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlModularComponentMtgPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlModularComponentMtgPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularComponentMtgPlate.Text, second, third, fourth, "", "", sentence);
            }

            //4
            if (ChkModularSideCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlModularSideCPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlModularSideCPlatecat1.SelectedItem.Text;
                    sentence = ChkModularSideCPlate.Text + ": " + second;
                }
                if (ddlModularSideCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlModularSideCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlModularSideCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlModularSideCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularSideCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //5
            if (ChkModularDoorCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlModularDoorCPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlModularDoorCPlatecat1.SelectedItem.Text;
                    sentence = ChkModularDoorCPlate.Text + ": " + second;
                }
                if (ddlModularDoorCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlModularDoorCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlModularDoorCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlModularDoorCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness " + ",";
                }



                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularDoorCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //6
            if (ChkModularWallMtgBracket.Checked == true)
            {
                validation = 0;
                string third = "", sentence = "";
                sentence = ChkModularWallMtgBracket.Text;
                if (ddlModularWallMtgBracketcat1.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlModularWallMtgBracketcat1.SelectedItem.Text;
                    sentence += ": in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularWallMtgBracket.Text, third, "", "", "", "", sentence);
            }

            //7
            if (ChkModularFrontDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlModularFrontDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlModularFrontDoorcat1.SelectedItem.Text;
                    sentence = ChkModularFrontDoor.Text + ": " + second;
                }
                if (ddlModularFrontDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlModularFrontDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularFrontDoor.Text, second, third, "", "", "", sentence);
            }

            //8
            if (ChkModularRearDoor.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlModularRearDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlModularRearDoorcat1.SelectedItem.Text;
                    sentence = ChkModularRearDoor.Text + ": " + second + " ";
                }
                if (ddlthickmodularecord.SelectedItem.Text != "Select")
                {
                    second = ddlthickmodularecord.SelectedItem.Text;
                    sentence += "With Thickness " + second;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularRearDoor.Text, second, "", "", "", "", sentence);
            }

            //9
            if (ChkModularRearCover.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlModularRearCovercat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlModularRearCovercat1.SelectedItem.Text;
                    sentence = ChkModularRearCover.Text + ": in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularRearCover.Text, second, "", "", "", "", sentence);
            }

            //10
            if (ChkModularCableSupportAngle.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";
                sentence = ChkModularCableSupportAngle.Text;
                if (txtModularCableSupportAnglecat1.Text != "")
                {
                    second = txtModularCableSupportAnglecat1.Text;
                    sentence += ": Qty " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularCableSupportAngle.Text, second, "", "", "", "", sentence);
            }

            //11
            if (ChkModularLock.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlModularLockcat1.SelectedItem.Text != "Select")
                {
                    second = ddlModularLockcat1.SelectedItem.Text;
                    sentence = ChkModularLock.Text + ": " + second + ",";
                }
                if (txtlockQty.Text != "")
                {
                    second = txtlockQty.Text;
                    sentence += "Qty" + ": " + second;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularLock.Text, second, "", "", "", "", sentence);
            }

            //12
            if (ChkModularPowerCoatingShade.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                sentence = ChkModularPowerCoatingShade.Text + ": ";
                if (ddlModularPowerCoatingShadecat1.SelectedItem.Text != "Select Colour")
                {
                    first = ddlModularPowerCoatingShadecat1.SelectedItem.Text;

                    if (ddlModularPowerCoatingShadecat1.SelectedItem.Text == "Specify")
                    {
                        sentence += txtModularPowerCoatingShadecat3.Text;
                    }
                    else

                        sentence = first + " ";
                }
                if (ddlModularPowerCoatingShadecat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlModularPowerCoatingShadecat2.SelectedItem.Text;
                    sentence += " in " + second + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularPowerCoatingShade.Text, first, second, txtModularPowerCoatingShadecat3.Text, "", "", sentence);
            }

            //13
            if (ChkModularLiftingArrangement.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (ddlModularLiftingArrangementcat1.SelectedItem.Text != "Select")
                {
                    first = ddlModularLiftingArrangementcat1.SelectedItem.Text;
                    sentence = ChkModularLiftingArrangement.Text + ": " + first;
                }

                if (ddlModularLiftingArrangementcat2.Text != "Select Thickness")
                {
                    second = ddlModularLiftingArrangementcat2.Text;
                    sentence += " in " + second + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularLiftingArrangement.Text, first, second, "", "", "", sentence);
            }

            //14
            if (ChkModularBase.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlModularBasecat1.SelectedItem.Text != "Select")
                {
                    second = ddlModularBasecat1.SelectedItem.Text;
                    sentence = ChkModularBase.Text + ": " + second;
                }
                if (ddlModularBasecat2.SelectedItem.Text != "Select Height")
                {
                    third = ddlModularBasecat2.SelectedItem.Text;
                    sentence += " Height " + third + ",";
                }
                if (ddltype.SelectedItem.Text != "")
                {
                    third = ddltype.SelectedItem.Text;
                    sentence += "Transparent Door with" + third;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularBase.Text, second, third, "", "", "", sentence);
            }

            //15
            if (ChkWMM23TransparentDoorModular.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlWMM23TransparentDoorcat1modular.SelectedItem.Text != "Select")
                {
                    second = ddlWMM23TransparentDoorcat1modular.SelectedItem.Text;
                    sentence = ChkWMM23TransparentDoorModular.Text + ": " + second;
                }
                if (ddlWMM23TransparentDoorcat2Modualr.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlWMM23TransparentDoorcat2Modualr.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlWMM23TransparentDoorcat3modular.SelectedItem.Text != "Select")
                {
                    fourth = ddlWMM23TransparentDoorcat3modular.SelectedItem.Text;
                    sentence += fourth;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkWMM23TransparentDoor.Text, second, third, fourth, "", "", sentence);
            }

            //16
            if (ChkModularAntivibrationpad.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlModularAntivibrationpadcat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlModularAntivibrationpadcat1.SelectedItem.Text;
                    sentence = ChkModularAntivibrationpad.Text + ": in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularAntivibrationpad.Text, second, "", "", "", "", sentence);
            }

            //17
            if (ChkModularCanopy.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlModularCanopycat1.SelectedItem.Text != "Select")
                {
                    second = ddlModularCanopycat1.SelectedItem.Text;
                    sentence = ChkModularCanopy.Text + ": " + second;
                }
                if (ddlModularCanopycat2.SelectedItem.Text != "Perforation Hole")
                {
                    third = ddlModularCanopycat2.SelectedItem.Text;
                    sentence += " " + third;
                }
                if (ddlModularCanopycat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlModularCanopycat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularCanopy.Text, second, third, fourth, "", "", sentence);
            }

            if (ChkModularfan.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlModularfancat1.SelectedItem.Text != "Select Size")
                {
                    first = ddlModularfancat1.SelectedItem.Text;
                    sentence = ChkModularfan.Text + ": In Size " + first + " inch qty: " + txtModularfancat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularfan.Text, first, txtModularfancat2.Text, "", "", "", sentence);
            }

            //18
            if (ChkModularJointlesspolyurethanefoamedinplacegasketing.Checked == true)
            {
                validation = 0;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularJointlesspolyurethanefoamedinplacegasketing.Text, "", "", "", "", "", ChkModularJointlesspolyurethanefoamedinplacegasketing.Text);
            }

            //19
            if (ChkModularAnyadditionalcomponent.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                first = txtModularAnyadditionalcomponentcat1.Text;
                sentence = ChkModularAnyadditionalcomponent.Text + ": " + first;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkModularAnyadditionalcomponent.Text, txtModularAnyadditionalcomponentcat1.Text, "", "", "", "", sentence);
            }
        }
        #endregion

        #region 7.Eco Frame 43mm

        if (ddlConstype.Text == "Eco Frame 43mm")
        {
            if (ChkEcoFrameMainFrameTopBottomWeldedStructure.Checked == false && ChkEcoFrameTopBottomGlandPlate.Checked == false
                && ChkEcoFrameComponentMtgPlate.Checked == false && ChkEcoFrameSideCPlate.Checked == false
                && ChkEcoFrameDoorCPlate.Checked == false && ChkEcoFrameFrontDoor.Checked == false &&
                ChkEcoFrameRearDoor.Checked == false && ChkEcoFrameRearCover.Checked == false
                && ChkEcoFrameLock.Checked == false && ChkEcoFramePowerCoatingShade.Checked == false
                && ChkEcoFrameLiftingArrangement.Checked == false && ChkEcoFrameBase.Checked == false
                && ChkEcoFrameCanopy.Checked == false && ChkEcoFrameSideCover.Checked == false && ChkEcoMCCfan.Checked == false
                && ChkEcoFrameJointlesspolyurethanefoamedinplacegasketing.Checked == false
                && ChkEcoFrameAnyadditionalcomponent.Checked == false)
            {
                validation = 1;
            }

            action = "insertEcoFrame";
            //1
            if (ChkEcoFrameMainFrameTopBottomWeldedStructure.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                if (ddlEcoFrameMainFrameTopBottomWeldedStructurecat1.SelectedItem.Text != "Select")
                {
                    first = ddlEcoFrameMainFrameTopBottomWeldedStructurecat1.SelectedItem.Text;
                    sentence = ChkEcoFrameMainFrameTopBottomWeldedStructure.Text + " in " + first + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameMainFrameTopBottomWeldedStructure.Text, first, "", "", "", "", sentence);
            }

            //2
            if (ChkEcoFrameTopBottomGlandPlate.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlEcoFrameTopBottomGlandPlatecat1.SelectedItem.Text != "Select")
                {
                    first = ddlEcoFrameTopBottomGlandPlatecat1.SelectedItem.Text;
                    sentence = ChkEcoFrameTopBottomGlandPlate.Text + ": in " + first + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameTopBottomGlandPlate.Text, first, "", "", "", "", sentence);
            }

            //3
            if (ChkEcoFrameComponentMtgPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlEcoFrameComponentMtgPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlEcoFrameComponentMtgPlatecat1.SelectedItem.Text;
                    sentence = ChkEcoFrameComponentMtgPlate.Text + ": " + second;
                }
                if (ddlEcoFrameComponentMtgPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlEcoFrameComponentMtgPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlEcoFrameComponentMtgPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlEcoFrameComponentMtgPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameComponentMtgPlate.Text, second, third, fourth, "", "", sentence);
            }

            //4
            if (ChkEcoFrameSideCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlEcoFrameSideCPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlEcoFrameSideCPlatecat1.SelectedItem.Text;
                    sentence = ChkEcoFrameSideCPlate.Text + ": " + second;
                }
                if (ddlEcoFrameSideCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlEcoFrameSideCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlEcoFrameSideCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlEcoFrameSideCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameSideCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //5
            if (ChkEcoFrameDoorCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlEcoFrameDoorCPlatecat1.SelectedItem.Text != "Select Size")
                {
                    second = ddlEcoFrameDoorCPlatecat1.SelectedItem.Text;
                    sentence = ChkEcoFrameDoorCPlate.Text + ": " + second;
                }
                if (ddlEcoFrameDoorCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlEcoFrameDoorCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlEcoFrameDoorCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlEcoFrameDoorCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness " + ",";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameDoorCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //6
            if (ChkEcoFrameFrontDoor.Checked == true)
            {
                validation = 0;
                string third = "", second = "", sentence = "";
                sentence = ChkEcoFrameFrontDoor.Text;
                if (ddlEcoFrameFrontDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoFrameFrontDoorcat1.SelectedItem.Text;
                    sentence += ": " + second;
                }
                if (ddlEcoFrameFrontDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlEcoFrameFrontDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameFrontDoor.Text, second, third, "", "", "", sentence);
            }

            //7
            if (ChkEcoFrameRearDoor.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlEcoFrameRearDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoFrameRearDoorcat1.SelectedItem.Text;
                    sentence = ChkEcoFrameRearDoor.Text + ": " + second + " ";
                }
                if (ddlthicknessecoframe.SelectedItem.Text != "Select")
                {
                    second = ddlthicknessecoframe.SelectedItem.Text;
                    sentence += "With Thickness " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameRearDoor.Text, second, "", "", "", "", sentence);
            }

            //8
            if (ChkEcoFrameRearCover.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlEcoFrameRearCovercat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlEcoFrameRearCovercat1.SelectedItem.Text;
                    sentence = ChkEcoFrameRearCover.Text + ": in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameRearCover.Text, second, "", "", "", "", sentence);
            }

            //9
            if (ChkEcoFrameSideCover.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlEcoFrameSideCovercat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlEcoFrameSideCovercat1.SelectedItem.Text;
                    sentence = ChkEcoFrameSideCover.Text + ": in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameSideCover.Text, second, "", "", "", "", sentence);
            }

            //10
            if (ChkEcoFrameLock.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlEcoFrameLockcat1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoFrameLockcat1.SelectedItem.Text;
                    sentence = ChkEcoFrameLock.Text + ": " + second + ",";
                }

                if (txtecoframelockqty.Text != "")
                {
                    second = txtecoframelockqty.Text;
                    sentence += "Qty:" + second;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameLock.Text, second, "", "", "", "", sentence);
            }

            //11
            if (ChkEcoFramePowerCoatingShade.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";
                sentence = ChkEcoFramePowerCoatingShade.Text + ": ";
                if (ddlEcoFramePowerCoatingShadecat1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoFramePowerCoatingShadecat1.SelectedItem.Text;

                    if (ddlEcoFramePowerCoatingShadecat1.SelectedItem.Text == "Specify")
                    {
                        sentence += txtddlEcoFramePowerCoatingShadecat2.Text;
                    }
                    else
                        sentence += second;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFramePowerCoatingShade.Text, second, txtddlEcoFramePowerCoatingShadecat2.Text, "", "", "", sentence);
            }

            //12
            if (ChkEcoFrameLiftingArrangement.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (ddlEcoFrameLiftingArrangementcat1.SelectedItem.Text != "Select")
                {
                    first = ddlEcoFrameLiftingArrangementcat1.SelectedItem.Text;
                    sentence = ChkEcoFrameLiftingArrangement.Text + ": " + first;
                }

                if (ddlEcoFrameLiftingArrangementcat2.Text != "Select Thickness")
                {
                    second = ddlEcoFrameLiftingArrangementcat2.Text;
                    sentence += " in " + second + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameLiftingArrangement.Text, first, second, "", "", "", sentence);
            }

            //13
            if (ChkEcoFrameBase.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlEcoFrameBasecat1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoFrameBasecat1.SelectedItem.Text;
                    sentence = ChkEcoFrameBase.Text + ": " + second;
                }
                if (ddlEcoFrameBasecat2.SelectedItem.Text != "Select Height")
                {
                    third = ddlEcoFrameBasecat2.SelectedItem.Text;
                    sentence += " Height " + third;
                }
                if (ddlecoframebasethickness.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlecoframebasethickness.SelectedItem.Text;
                    sentence += " With Thickness " + third;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameBase.Text, second, third, "", "", "", sentence);
            }

            if (checkfibration.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlfibration.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlfibration.SelectedItem.Text;
                    sentence = "Anti-Fibration Pad" + " in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ddlfibration.Text, second, "", "", "", "", sentence);
            }


            //14
            if (ChkEcoFrameCanopy.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlEcoFrameCanopycat1.SelectedItem.Text != "Select")
                {
                    second = ddlEcoFrameCanopycat1.SelectedItem.Text;
                    sentence = ChkEcoFrameCanopy.Text + ": " + second;
                }
                if (ddlEcoFrameCanopycat2.SelectedItem.Text != "Perforation Hole")
                {
                    third = ddlEcoFrameCanopycat2.SelectedItem.Text;
                    sentence += " " + third;
                }
                if (ddlEcoFrameCanopycat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlEcoFrameCanopycat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameCanopy.Text, second, third, fourth, "", "", sentence);
            }
            //15
            if (ChkEcoFramefan.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlEcoFramefancat1.SelectedItem.Text != "Select Size")
                {
                    first = ddlEcoFramefancat1.SelectedItem.Text;
                    sentence = ChkEcoFramefan.Text + ": In Size " + first + " inch qty: " + txtEcoFramefancat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFramefan.Text, first, txtEcoFramefancat2.Text, "", "", "", sentence);
            }
            //16
            if (ChkEcoFrameJointlesspolyurethanefoamedinplacegasketing.Checked == true)
            {
                validation = 0;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameJointlesspolyurethanefoamedinplacegasketing.Text, "", "", "", "", "", ChkEcoFrameJointlesspolyurethanefoamedinplacegasketing.Text);
            }
            //17
            if (ChkEcoFrameAnyadditionalcomponent.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                first = txtEcoFrameAnyadditionalcomponentcat1.Text;
                sentence = ChkEcoFrameAnyadditionalcomponent.Text + ": " + first;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkEcoFrameAnyadditionalcomponent.Text, txtEcoFrameAnyadditionalcomponentcat1.Text, "", "", "", "", sentence);
            }
        }
        #endregion

        #region 8.PC ENCLOSURE

        if (ddlConstype.Text == "PC ENCLOSURE")
        {
            if (rdlpcenclosure.SelectedItem.Text == "Shop Floor PC Enclosure Standing")
            {
                if (ChkPCEncShopFloorStandingMainframestructure.Checked == false && ChkPCEncShopFloorStandingBottomCover.Checked == false
                    && ChkPCEncShopFloorStandingComponentMtgPlate.Checked == false && ChkPCEncShopFloorStandingSidecPlate.Checked == false
                    && ChkPCEncShopFloorStandingDoorCPlate.Checked == false && ChkPCEncShopFloorStandingFrontDoor.Checked == false
                    && ChkPCEncShopFloorStandingRearDoor.Checked == false && ChkPCEncShopFloorStandingLock.Checked == false
                    && ChkPCEncShopFloorStandingRearCover.Checked == false && ChkPCEncShopFloorStandingSideCover.Checked == false
                    && ChkPCEncShopFloorStandingTopCover.Checked == false && ChkPCEncShopFloorStandingHorizontalPartition.Checked == false
                    && ChkPCEncShopFloorStandingSlidingKeyboarddrawer.Checked == false && ChkPCEncShopFloorStandingPowderCoatingShade.Checked == false
                    && ChkPCEncShopFloorStandingLiftingArrangement.Checked == false && ChkPCEncShopFloorStandingBase.Checked == false
                    && ChkPCEncShopFloorStandingAntivibrationpad.Checked == false && ChkPCEncShopFloorStandingAntivibrationCasterWheel.Checked == false
                    && ChkPCEncShopFloorStandingDrawingPocket.Checked == false && ChkPCEncShopFloorStandingMicroswitchbracket.Checked == false
                    && ChkPCEncShopFloorStandingTubelightBracket.Checked == false && ChkPCEncShopFloorStandingTransparentDoor.Checked == false
                    && ChkPCEncShopFloorStandingFilters.Checked == false && ChkPCEncShopFloorStandingGasspring.Checked == false
                    && ChkPCEncShopFloorStandingAluminiumExtrusion.Checked == false && ChkPCEncShopFloorStandingJointlesspolyurethane.Checked == false
                    && ChkPCEnclosureECOStandingfan.Checked == false && ChkPCEncShopFloorStandingAnyadditional.Checked == false)
                {
                    validation = 1;
                }

                action = "insertShopFloorPCEnclosureStanding";
                //1
                if (ChkPCEncShopFloorStandingMainframestructure.Checked == true)
                {
                    validation = 0;
                    string first = "", sentence = "";

                    if (ddlPCEncShopFloorStandingMainframestructurecat1.SelectedItem.Text != "Select Thickness")
                    {
                        first = ddlPCEncShopFloorStandingMainframestructurecat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingMainframestructure.Text + " in " + first + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingMainframestructure.Text, first, "", "", "", "", sentence);
                }

                //2
                if (ChkPCEncShopFloorStandingBottomCover.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", sentence = "";
                    if (ddlPCEncShopFloorStandingBottomCovercat1.SelectedItem.Text != "Select")
                    {
                        first = ddlPCEncShopFloorStandingBottomCovercat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingBottomCover.Text + ": " + first + " ";
                    }
                    if (ddlPCEncShopFloorStandingBottomCovercat2.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingBottomCovercat2.SelectedItem.Text;
                        sentence += ": in " + second + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingBottomCover.Text, first, second, "", "", "", sentence);
                }

                //3
                if (ChkPCEncShopFloorStandingComponentMtgPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", fifth = "", sixth = "", sentence = "";

                    if (txtPCEncShopFloorStandingComponentMtgPlatecat1.Text != "")
                    {
                        second = txtPCEncShopFloorStandingComponentMtgPlatecat1.Text;
                        sentence = ChkPCEncShopFloorStandingComponentMtgPlate.Text + ": of height " + second + " mm,";
                    }
                    if (txtPCEncShopFloorStandingComponentMtgPlatecat2.Text != "")
                    {
                        third = txtPCEncShopFloorStandingComponentMtgPlatecat2.Text;
                        sentence += " quantity " + third + " Nos";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat3.SelectedItem.Text != "Select")
                    {
                        fourth = ddlPCEncShopFloorStandingComponentMtgPlatecat3.SelectedItem.Text;
                        sentence += " in  " + fourth + " ";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat4.SelectedItem.Text != "Select Colour")
                    {
                        fifth = ddlPCEncShopFloorStandingComponentMtgPlatecat4.SelectedItem.Text;
                        sentence += " colour " + fifth + " ";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat5.SelectedItem.Text != "Select Thickness")
                    {
                        sixth = ddlPCEncShopFloorStandingComponentMtgPlatecat5.SelectedItem.Text;
                        sentence += "in thickness " + sixth;
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingComponentMtgPlate.Text, second, third, fourth, fifth, sixth, sentence);
                }
                //32
                if (ChkPCEncShopFloorStandingComponentMtgPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", fifth = "", sixth = "", sentence = "";

                    if (txtPCEncShopFloorStandingComponentMtgPlatecat21.Text != "")
                    {
                        second = txtPCEncShopFloorStandingComponentMtgPlatecat21.Text;
                        sentence = ChkPCEncShopFloorStandingComponentMtgPlate.Text + ": of height " + second;
                    }
                    if (txtPCEncShopFloorStandingComponentMtgPlatecat22.Text != "")
                    {
                        third = txtPCEncShopFloorStandingComponentMtgPlatecat22.Text;
                        sentence += " quantity " + third + " Nos";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat23.SelectedItem.Text != "Select")
                    {
                        fourth = ddlPCEncShopFloorStandingComponentMtgPlatecat23.SelectedItem.Text;
                        sentence += " in  " + fourth + " ";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat24.SelectedItem.Text != "Select Colour")
                    {
                        fifth = ddlPCEncShopFloorStandingComponentMtgPlatecat24.SelectedItem.Text;
                        sentence += " colour " + fifth + " ";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat25.SelectedItem.Text != "Select Thickness")
                    {
                        sixth = ddlPCEncShopFloorStandingComponentMtgPlatecat25.SelectedItem.Text;
                        sentence += "in thickness " + sixth;
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingComponentMtgPlate.Text + "2", second, third, fourth, fifth, sixth, sentence);
                }

                //33
                if (ChkPCEncShopFloorStandingComponentMtgPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", fifth = "", sixth = "", sentence = "";

                    if (txtPCEncShopFloorStandingComponentMtgPlatecat31.Text != "")
                    {
                        second = txtPCEncShopFloorStandingComponentMtgPlatecat31.Text;
                        sentence = ChkPCEncShopFloorStandingComponentMtgPlate.Text + ": of height " + second;
                    }
                    if (txtPCEncShopFloorStandingComponentMtgPlatecat32.Text != "")
                    {
                        third = txtPCEncShopFloorStandingComponentMtgPlatecat32.Text;
                        sentence += " quantity " + third + " Nos";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat33.SelectedItem.Text != "Select")
                    {
                        fourth = ddlPCEncShopFloorStandingComponentMtgPlatecat33.SelectedItem.Text;
                        sentence += " in  " + fourth + " ";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat34.SelectedItem.Text != "Select Colour")
                    {
                        fifth = ddlPCEncShopFloorStandingComponentMtgPlatecat34.SelectedItem.Text;
                        sentence += " colour " + fifth + " ";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat35.SelectedItem.Text != "Select Thickness")
                    {
                        sixth = ddlPCEncShopFloorStandingComponentMtgPlatecat35.SelectedItem.Text;
                        sentence += "in thickness " + sixth;
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingComponentMtgPlate.Text + "3", second, third, fourth, fifth, sixth, sentence);
                }
                //34
                if (ChkPCEncShopFloorStandingComponentMtgPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", fifth = "", sixth = "", sentence = "";

                    if (txtPCEncShopFloorStandingComponentMtgPlatecat41.Text != "")
                    {
                        second = txtPCEncShopFloorStandingComponentMtgPlatecat41.Text;
                        sentence = ChkPCEncShopFloorStandingComponentMtgPlate.Text + ": of height " + second;
                    }
                    if (txtPCEncShopFloorStandingComponentMtgPlatecat42.Text != "")
                    {
                        third = txtPCEncShopFloorStandingComponentMtgPlatecat42.Text;
                        sentence += " quantity " + third + " Nos";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat43.SelectedItem.Text != "Select")
                    {
                        fourth = ddlPCEncShopFloorStandingComponentMtgPlatecat43.SelectedItem.Text;
                        sentence += " in  " + fourth + " ";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat44.SelectedItem.Text != "Select Colour")
                    {
                        fifth = ddlPCEncShopFloorStandingComponentMtgPlatecat44.SelectedItem.Text;
                        sentence += " colour " + fifth + " ";
                    }
                    if (ddlPCEncShopFloorStandingComponentMtgPlatecat45.SelectedItem.Text != "Select Thickness")
                    {
                        sixth = ddlPCEncShopFloorStandingComponentMtgPlatecat45.SelectedItem.Text;
                        sentence += "in thickness " + sixth;
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingComponentMtgPlate.Text + "4", second, third, fourth, fifth, sixth, sentence);
                }

                //4
                if (ChkPCEncShopFloorStandingSidecPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", sentence = "";

                    if (ddlPCEncShopFloorStandingSidecPlatecat1.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingSidecPlatecat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingSidecPlate.Text + ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingSidecPlatecat2.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEncShopFloorStandingSidecPlatecat2.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    if (ddlPCEncShopFloorStandingSidecPlatecat3.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEncShopFloorStandingSidecPlatecat3.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness ";
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingSidecPlate.Text, second, third, fourth, "", "", sentence);
                }

                //42
                if (ChkPCEncShopFloorStandingSidecPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", sentence = "";

                    if (ddlPCEncShopFloorStandingSidecPlatecat21.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingSidecPlatecat21.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingSidecPlate.Text + ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingSidecPlatecat22.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEncShopFloorStandingSidecPlatecat22.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    if (ddlPCEncShopFloorStandingSidecPlatecat23.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEncShopFloorStandingSidecPlatecat23.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness ";
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingSidecPlate.Text + "2", second, third, fourth, "", "", sentence);
                }

                //43
                if (ChkPCEncShopFloorStandingSidecPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", sentence = "";

                    if (ddlPCEncShopFloorStandingSidecPlatecat31.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingSidecPlatecat31.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingSidecPlate.Text + ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingSidecPlatecat32.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEncShopFloorStandingSidecPlatecat32.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    if (ddlPCEncShopFloorStandingSidecPlatecat33.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEncShopFloorStandingSidecPlatecat33.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness ";
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingSidecPlate.Text + "3", second, third, fourth, "", "", sentence);
                }

                //44
                if (ChkPCEncShopFloorStandingSidecPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", sentence = "";

                    if (ddlPCEncShopFloorStandingSidecPlatecat41.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingSidecPlatecat41.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingSidecPlate.Text + ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingSidecPlatecat42.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEncShopFloorStandingSidecPlatecat42.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    if (ddlPCEncShopFloorStandingSidecPlatecat43.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEncShopFloorStandingSidecPlatecat43.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness ";
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingSidecPlate.Text + "4", second, third, fourth, "", "", sentence);
                }

                //5
                if (ChkPCEncShopFloorStandingDoorCPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", sentence = "";

                    if (ddlPCEncShopFloorStandingDoorCPlatecat1.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingDoorCPlatecat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingDoorCPlate.Text + ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingDoorCPlatecat2.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEncShopFloorStandingDoorCPlatecat2.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    if (ddlPCEncShopFloorStandingDoorCPlatecat3.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEncShopFloorStandingDoorCPlatecat3.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness ";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingDoorCPlate.Text, second, third, fourth, "", "", sentence);
                }

                //52
                if (ChkPCEncShopFloorStandingDoorCPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", sentence = "";

                    if (ddlPCEncShopFloorStandingDoorCPlatecat21.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingDoorCPlatecat21.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingDoorCPlate.Text + ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingDoorCPlatecat22.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEncShopFloorStandingDoorCPlatecat22.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    if (ddlPCEncShopFloorStandingDoorCPlatecat23.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEncShopFloorStandingDoorCPlatecat23.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness ";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingDoorCPlate.Text + "2", second, third, fourth, "", "", sentence);
                }

                //53
                if (ChkPCEncShopFloorStandingDoorCPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", sentence = "";

                    if (ddlPCEncShopFloorStandingDoorCPlatecat31.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingDoorCPlatecat31.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingDoorCPlate.Text + ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingDoorCPlatecat32.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEncShopFloorStandingDoorCPlatecat32.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    if (ddlPCEncShopFloorStandingDoorCPlatecat33.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEncShopFloorStandingDoorCPlatecat33.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness ";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingDoorCPlate.Text + "3", second, third, fourth, "", "", sentence);
                }

                //54
                if (ChkPCEncShopFloorStandingDoorCPlate.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", sentence = "";

                    if (ddlPCEncShopFloorStandingDoorCPlatecat41.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingDoorCPlatecat41.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingDoorCPlate.Text + ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingDoorCPlatecat42.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEncShopFloorStandingDoorCPlatecat42.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    if (ddlPCEncShopFloorStandingDoorCPlatecat43.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEncShopFloorStandingDoorCPlatecat43.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness ";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingDoorCPlate.Text + "4", second, third, fourth, "", "", sentence);
                }
                //6
                if (ChkPCEncShopFloorStandingFrontDoor.Checked == true)
                {
                    validation = 0;
                    string third = "", second = "", sentence = "";
                    sentence = ChkPCEncShopFloorStandingFrontDoor.Text;
                    if (ddlPCEncShopFloorStandingFrontDoorcat1.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingFrontDoorcat1.SelectedItem.Text;
                        sentence += ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingFrontDoorcat2.SelectedItem.Text != "Select Thickness")
                    {
                        third = ddlPCEncShopFloorStandingFrontDoorcat2.SelectedItem.Text;
                        sentence += " in " + third + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingFrontDoor.Text, second, third, "", "", "", sentence);
                }

                //7
                if (ChkPCEncShopFloorStandingRearDoor.Checked == true)
                {
                    validation = 0;
                    string third = "", second = "", sentence = "";
                    sentence = ChkPCEncShopFloorStandingRearDoor.Text;
                    if (ddlPCEncShopFloorStandingRearDoorcat1.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingRearDoorcat1.SelectedItem.Text;
                        sentence += ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingRearDoorcat2.SelectedItem.Text != "Select Thickness")
                    {
                        third = ddlPCEncShopFloorStandingRearDoorcat2.SelectedItem.Text;
                        sentence += " in " + third + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingRearDoor.Text, second, third, "", "", "", sentence);
                }

                //8
                if (ChkPCEncShopFloorStandingLock.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", sentence = "";

                    if (ddlPCEncShopFloorStandingLockcat1.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingLockcat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingLock.Text + ": " + second;
                    }
                    if (txtPCEncShopFloorStandingLockcat2.Text != "")
                    {
                        third = txtPCEncShopFloorStandingLockcat2.Text;
                        sentence += " Qty: " + third;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingLock.Text, second, third, "", "", "", sentence);
                }

                //9
                if (ChkPCEncShopFloorStandingRearCover.Checked == true)
                {
                    validation = 0;
                    string second = "", sentence = "";

                    if (ddlPCEncShopFloorStandingRearCovercat1.SelectedItem.Text != "Select Thickness")
                    {
                        second = ddlPCEncShopFloorStandingRearCovercat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingRearCover.Text + ": in " + second + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingRearCover.Text, second, "", "", "", "", sentence);
                }

                //10
                if (ChkPCEncShopFloorStandingSideCover.Checked == true)
                {
                    validation = 0;
                    string second = "", sentence = "";

                    if (ddlPCEncShopFloorStandingSideCovercat1.SelectedItem.Text != "Select Thickness")
                    {
                        second = ddlPCEncShopFloorStandingSideCovercat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingSideCover.Text + ": in " + second + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingSideCover.Text, second, "", "", "", "", sentence);
                }

                //11
                if (ChkPCEncShopFloorStandingTopCover.Checked == true)
                {
                    validation = 0;
                    string second = "", sentence = "";

                    if (ddlPCEncShopFloorStandingTopCovercat1.SelectedItem.Text != "Select Thickness")
                    {
                        second = ddlPCEncShopFloorStandingTopCovercat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingTopCover.Text + ": in " + second + " thickness";
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingTopCover.Text, second, "", "", "", "", sentence);
                }

                //12
                if (ChkPCEncShopFloorStandingHorizontalPartition.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", sentence = "";

                    if (ddlPCEncShopFloorStandingHorizontalPartitioncat1.SelectedItem.Text != "Select Thickness")
                    {
                        second = ddlPCEncShopFloorStandingTopCovercat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingHorizontalPartition.Text + ": in thickness " + second;
                    }
                    if (ddlPCEncShopFloorStandingHorizontalPartitioncat2.SelectedItem.Text != "Select")
                    {
                        third = ddlPCEncShopFloorStandingHorizontalPartitioncat2.SelectedItem.Text;
                        sentence += " " + third + " in colour";
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingHorizontalPartition.Text, second, third, "", "", "", sentence);
                }

                //13
                if (ChkPCEncShopFloorStandingSlidingKeyboarddrawer.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", sentence = "";

                    if (ddlPCEncShopFloorStandingSlidingKeyboarddrawercat1.SelectedItem.Text != "Select Thickness")
                    {
                        first = ddlPCEncShopFloorStandingSlidingKeyboarddrawercat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingSlidingKeyboarddrawer.Text + ": in " + first + " thickness";
                    }

                    if (ddlPCEncShopFloorStandingSlidingKeyboarddrawercat2.Text != "Select Colour")
                    {
                        second = ddlPCEncShopFloorStandingSlidingKeyboarddrawercat2.Text;
                        sentence += " colour " + second;
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingSlidingKeyboarddrawer.Text, first, second, "", "", "", sentence);
                }

                //14
                if (ChkPCEncShopFloorStandingPowderCoatingShade.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", sentence = "";
                    sentence = ChkPCEncShopFloorStandingPowderCoatingShade.Text + ": ";
                    if (ddlPCEncShopFloorStandingPowderCoatingShadecat1.SelectedItem.Text != "Select")
                    {
                        first = ddlPCEncShopFloorStandingPowderCoatingShadecat1.SelectedItem.Text;
                    }
                    if (ddlPCEncShopFloorStandingPowderCoatingShadecat1.SelectedItem.Text == "Specify")
                    {
                        second = txtPCEncShopFloorStandingPowderCoatingShadecat2.Text;
                        sentence += second;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingPowderCoatingShade.Text, first, second, "", "", "", sentence);
                }
                //15
                if (ChkPCEncShopFloorStandingLiftingArrangement.Checked == true)
                {
                    validation = 0;
                    string second = "", fourth = "", sentence = "";

                    if (ddlPCEncShopFloorStandingLiftingArrangementcat1.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingLiftingArrangementcat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingLiftingArrangement.Text + ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingLiftingArrangementcat2.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEncShopFloorStandingLiftingArrangementcat2.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness";
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingLiftingArrangement.Text, second, fourth, "", "", "", sentence);
                }
                //16
                if (ChkPCEncShopFloorStandingBase.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", sentence = "";

                    if (ddlPCEncShopFloorStandingBasecat1.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingBasecat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingBase.Text + ": " + second;
                    }
                    if (ddlPCEncShopFloorStandingBasecat2.SelectedItem.Text != "Select Thickness")
                    {
                        third = ddlPCEncShopFloorStandingBasecat2.SelectedItem.Text;
                        sentence += " in " + third + " thickness";
                    }
                    if (txtddlPCEncShopFloorStandingBasecat3.Text != "")
                    {
                        fourth = txtddlPCEncShopFloorStandingBasecat3.Text;
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingBase.Text, second, third, fourth, "", "", sentence);
                }

                //17
                if (ChkPCEncShopFloorStandingAntivibrationpad.Checked == true)
                {
                    validation = 0;
                    string second = "", sentence = "";

                    if (ddlPCEncShopFloorStandingAntivibrationpadcat1.SelectedItem.Text != "Select Thickness")
                    {
                        second = ddlPCEncShopFloorStandingAntivibrationpadcat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingAntivibrationpad.Text + ": " + " in " + second + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingAntivibrationpad.Text, second, "", "", "", "", sentence);
                }

                //18
                if (ChkPCEncShopFloorStandingAntivibrationCasterWheel.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", third = "", fourth = "", sentence = "";

                    if (txtPCEncShopFloorStandingAntivibrationCasterWheelcat1.Text != "")
                    {
                        first = txtPCEncShopFloorStandingAntivibrationCasterWheelcat1.Text;
                        sentence = ChkPCEncShopFloorStandingAntivibrationpad.Text + ": " + " size " + first + " ";
                    }
                    if (txtPCEncShopFloorStandingAntivibrationCasterWheelcat2.Text != "")
                    {
                        second = txtPCEncShopFloorStandingAntivibrationCasterWheelcat2.Text;
                        sentence += " resolving qty " + second + " ";
                    }
                    if (txtPCEncShopFloorStandingAntivibrationCasterWheelcat3.Text != "")
                    {
                        third = txtPCEncShopFloorStandingAntivibrationCasterWheelcat3.Text;
                        sentence += " fixed qty " + third + " ";
                    }
                    if (txtPCEncShopFloorStandingAntivibrationCasterWheelcat4.Text != "")
                    {
                        fourth = txtPCEncShopFloorStandingAntivibrationCasterWheelcat4.Text;
                        sentence += " " + fourth + " ";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingAntivibrationCasterWheel.Text, first, second, third, fourth, "", sentence);
                }

                //19
                if (ChkPCEncShopFloorStandingDrawingPocket.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", sentence = "";

                    if (ddlChkPCEncShopFloorStandingDrawingPocketcat1.SelectedItem.Text != "Select")
                    {
                        first = ddlChkPCEncShopFloorStandingDrawingPocketcat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingDrawingPocket.Text + ": " + first;
                    }
                    if (ddlChkPCEncShopFloorStandingDrawingPocketcat2.SelectedItem.Text != "Select Colour")
                    {
                        second = ddlChkPCEncShopFloorStandingDrawingPocketcat2.SelectedItem.Text;
                        sentence += " colour " + second;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingDrawingPocket.Text, first, second, "", "", "", sentence);
                }

                //20
                if (ChkPCEncShopFloorStandingMicroswitchbracket.Checked == true)
                {
                    validation = 0;
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingMicroswitchbracket.Text, "", "", "", "", "", ChkPCEncShopFloorStandingMicroswitchbracket.Text);
                }

                //21
                if (ChkPCEncShopFloorStandingTubelightBracket.Checked == true)
                {
                    validation = 0;
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingTubelightBracket.Text, "", "", "", "", "", ChkPCEncShopFloorStandingTubelightBracket.Text);
                }

                //22
                if (ChkPCEncShopFloorStandingTransparentDoor.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", third = "", fourth = "", sentence = "";

                    if (ddlPCEncShopFloorStandingTransparentDoorcat1.SelectedItem.Text != "Select")
                    {
                        first = ddlPCEncShopFloorStandingTransparentDoorcat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingTransparentDoor.Text + ": " + first + " ";
                    }
                    if (ddlPCEncShopFloorStandingTransparentDoorcat2.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEncShopFloorStandingTransparentDoorcat2.SelectedItem.Text;
                        if (ddlPCEncShopFloorStandingTransparentDoorcat2.SelectedItem.Text != "Specify")
                        {
                            sentence += second + " ";
                        }
                    }
                    if (ddlPCEncShopFloorStandingTransparentDoorcat2.SelectedItem.Text == "Specify")
                    {
                        third = txtPCEncShopFloorStandingTransparentDoorcat3.Text;
                        sentence += third + " ";
                    }
                    if (ddlPCEncShopFloorStandingTransparentDoorcat4.SelectedItem.Text != "Select")
                    {
                        fourth = ddlPCEncShopFloorStandingTransparentDoorcat4.SelectedItem.Text;
                        sentence += fourth;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingTransparentDoor.Text, first, second, third, fourth, "", sentence);
                }
                //23
                if (ChkPCEncShopFloorStandingFilters.Checked == true)
                {
                    validation = 0;
                    string second = "", sentence = "";

                    if (ddlPCEncShopFloorStandingFilterscat1.SelectedItem.Text != "Select Inches")
                    {
                        second = ddlPCEncShopFloorStandingFilterscat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingFilters.Text + ": " + second + " inch";
                    }
                    if (txtqtyenclouser.Text != "")
                    {
                        second = txtqtyenclouser.Text;
                        sentence += " with Qty " + second;
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingFilters.Text, second, "", "", "", "", sentence);
                }

                //24
                if (ChkPCEncShopFloorStandingGasspring.Checked == true)
                {
                    validation = 0;
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingGasspring.Text, "", "", "", "", "", ChkPCEncShopFloorStandingGasspring.Text);
                }

                //25
                if (ChkPCEncShopFloorStandingAluminiumExtrusion.Checked == true)
                {
                    validation = 0;
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingAluminiumExtrusion.Text, "", "", "", "", "", ChkPCEncShopFloorStandingAluminiumExtrusion.Text);
                }

                //26
                if (ChkPCEncShopFloorStandingJointlesspolyurethane.Checked == true)
                {
                    validation = 0;
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingJointlesspolyurethane.Text, "", "", "", "", "", ChkPCEncShopFloorStandingJointlesspolyurethane.Text);
                }
                //27
                if (ChkPCEncShopFloorStandingfan.Checked == true)
                {
                    validation = 0;
                    string first = "", sentence = "";
                    if (ddlPCEncShopFloorStandingfancat1.SelectedItem.Text != "Select Size")
                    {
                        first = ddlPCEncShopFloorStandingfancat1.SelectedItem.Text;
                        sentence = ChkPCEncShopFloorStandingfan.Text + ": In Size " + first + " inch qty: " + txtChkPCEncShopFloorStandingfancat2.Text;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingfan.Text, first, txtChkPCEncShopFloorStandingfancat2.Text, "", "", "", sentence);
                }
                //28
                if (ChkPCEncShopFloorStandingAnyadditional.Checked == true)
                {
                    validation = 0;
                    string first = "", sentence = "";

                    first = txtPCEncShopFloorStandingAnyadditionalcat1.Text;
                    sentence = ChkPCEncShopFloorStandingAnyadditional.Text + ": " + first;

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEncShopFloorStandingAnyadditional.Text, txtPCEncShopFloorStandingAnyadditionalcat1.Text, "", "", "", "", sentence);
                }
            }
        }

        #endregion

        #region 9.PC Enclosure ECO-Standing ==AND== PC Enclosure ECO-Sitting

        if (ddlConstype.Text == "PC ENCLOSURE")
        {
            if (rdlpcenclosure.SelectedItem.Text == "PC Enclosure ECO-Standing" || rdlpcenclosure.SelectedItem.Text == "PC Enclosure ECO-Sitting")
            {
                if (ChkPCEnclosureECOStandingWeldedMainBody.Checked == false && ChkPCEnclosureECOStandingGlandPlate.Checked == false
                && ChkPCEnclosureECOStandingComponentMtgPlate.Checked == false && ChkPCEnclosureECOStandingSideCPlate.Checked == false
                && ChkPCEnclosureECOStandingDoorCPlate.Checked == false && ChkPCEnclosureECOStandingFrontDoorwithstiffeners.Checked == false
                && ChkPCEnclosureECOStandingCableSupportAngle.Checked == false && ChkPCEnclosureECOStandingLock.Checked == false
                && ChkPCEnclosureECOStandingHorizontalPartition.Checked == false && ChkPCEnclosureECOStandingSlidingKeyboarddrawer.Checked == false
                && ChkPCEnclosureECOStandingLiftingIBolt.Checked == false && ChkPCEnclosureECOStandingBase.Checked == false
                && ChkPCEnclosureECOStandingAntivibrationpad.Checked == false && ChkPCEnclosureECOStandingTransparentDoor.Checked == false
                && ChkPCEnclosureECOStandingCasterWheel.Checked == false && ChkPCEnclosureECOStandingFilters.Checked == false
                && ChkPCEnclosureECOStandingTelescopicRail.Checked == false && ChkPCEnclosureECOStandingJointlesspolyurethane.Checked == false
                && ChkPCEnclosureECOStandingPowderCoating.Checked == false && ChkPCEnclosureECOStandingAnyadditional.Checked == false)
                {
                    validation = 1;
                }

                if (rdlpcenclosure.SelectedItem.Text == "PC Enclosure ECO-Standing")
                {
                    action = "insertPcEncStanding";
                }
                if (rdlpcenclosure.SelectedItem.Text == "PC Enclosure ECO-Sitting")
                {
                    action = "insertPcEncSitting";
                }

                //1
                if (ChkPCEnclosureECOStandingWeldedMainBody.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", sentence = "";

                    if (ddlPCEnclosureECOStandingWeldedMainBodycat1.SelectedItem.Text != "Select")
                    {
                        first = ddlPCEnclosureECOStandingWeldedMainBodycat1.SelectedItem.Text;
                        sentence = ChkPCEnclosureECOStandingWeldedMainBody.Text + ": " + first;
                    }
                    if (ddlPCEnclosureECOStandingWeldedMainBodycat2.SelectedItem.Text != "Select Thickness")
                    {
                        second = ddlPCEnclosureECOStandingWeldedMainBodycat2.SelectedItem.Text;
                        sentence += " in " + second + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingWeldedMainBody.Text, first, second, "", "", "", sentence);
                }

                //2
                if (ChkPCEnclosureECOStandingGlandPlate.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", third = "", sentence = "";
                    if (ddlPCEnclosureECOStandingGlandPlatecat1.SelectedItem.Text != "Select")
                    {
                        first = ddlPCEnclosureECOStandingGlandPlatecat1.SelectedItem.Text;
                        sentence = ChkPCEnclosureECOStandingGlandPlate.Text + ": " + first;
                    }
                    if (!string.IsNullOrEmpty(txtPCEnclosureECOStandingGlandPlatecat2.Text))
                    {
                        second = txtPCEnclosureECOStandingGlandPlatecat2.Text;
                        sentence += " in size " + second;
                    }

                    if (ddlPCEnclosureECOStandingGlandPlatecat3.SelectedItem.Text != "Select Thickness")
                    {
                        third = ddlPCEnclosureECOStandingGlandPlatecat3.SelectedItem.Text;
                        sentence += " in " + third + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingGlandPlate.Text, first, second, third, "", "", sentence);
                }

                //3
                if (ChkPCEnclosureECOStandingComponentMtgPlate.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", third = "", fourth = "", sentence = "";

                    if (!string.IsNullOrEmpty(txtPCEnclosureECOStandingComponentMtgPlatecat1.Text))
                    {
                        first = txtPCEnclosureECOStandingComponentMtgPlatecat1.Text;
                        sentence = ChkPCEnclosureECOStandingComponentMtgPlate.Text + " in size " + first;
                    }

                    if (ddlPCEnclosureECOStandingComponentMtgPlatecat2.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEnclosureECOStandingComponentMtgPlatecat2.SelectedItem.Text;
                        sentence += " " + second;
                    }
                    if (ddlPCEnclosureECOStandingComponentMtgPlatecat3.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEnclosureECOStandingComponentMtgPlatecat3.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    if (ddlPCEnclosureECOStandingComponentMtgPlatecat4.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEnclosureECOStandingComponentMtgPlatecat4.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness ";
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingComponentMtgPlate.Text, first, second, third, fourth, "", sentence);
                }

                //4
                if (ChkPCEnclosureECOStandingSideCPlate.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", third = "", fourth = "", sentence = "";

                    if (!string.IsNullOrEmpty(txtPCEnclosureECOStandingSideCPlatecat1.Text))
                    {
                        first = txtPCEnclosureECOStandingSideCPlatecat1.Text;
                        sentence = ChkPCEnclosureECOStandingSideCPlate.Text + " in size " + first + " mm ,";
                    }

                    if (ddlPCEnclosureECOStandingSideCPlatecat2.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEnclosureECOStandingSideCPlatecat2.SelectedItem.Text;
                        sentence += " " + second;
                    }
                    if (ddlPCEnclosureECOStandingSideCPlatecat3.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEnclosureECOStandingSideCPlatecat3.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    if (ddlPCEnclosureECOStandingSideCPlatecat4.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEnclosureECOStandingSideCPlatecat4.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness ";
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingSideCPlate.Text, first, second, third, fourth, "", sentence);
                }

                //5
                if (ChkPCEnclosureECOStandingDoorCPlate.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", third = "", fourth = "", sentence = "";

                    if (!string.IsNullOrEmpty(txtPCEnclosureECOStandingDoorCPlatecat1.Text))
                    {
                        first = txtPCEnclosureECOStandingDoorCPlatecat1.Text;
                        sentence = ChkPCEnclosureECOStandingDoorCPlate.Text + " in size " + first + " mm ,"; ;
                    }

                    if (ddlPCEnclosureECOStandingDoorCPlatecat2.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEnclosureECOStandingDoorCPlatecat2.SelectedItem.Text;
                        sentence += " " + second;
                    }
                    if (ddlPCEnclosureECOStandingDoorCPlatecat3.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEnclosureECOStandingDoorCPlatecat3.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    if (ddlPCEnclosureECOStandingDoorCPlatecat4.SelectedItem.Text != "Select Thickness")
                    {
                        fourth = ddlPCEnclosureECOStandingDoorCPlatecat4.SelectedItem.Text;
                        sentence += " in " + fourth + " thickness ";
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingDoorCPlate.Text, first, second, third, fourth, "", sentence);
                }

                //6
                if (ChkPCEnclosureECOStandingFrontDoorwithstiffeners.Checked == true)
                {
                    validation = 0;
                    string third = "", second = "", sentence = "";
                    sentence = ChkPCEnclosureECOStandingFrontDoorwithstiffeners.Text;
                    if (ddlPCEnclosureECOStandingFrontDoorwithstiffenerscat1.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEnclosureECOStandingFrontDoorwithstiffenerscat1.SelectedItem.Text;
                        sentence += ": " + second;
                    }
                    if (ddlPCEnclosureECOStandingFrontDoorwithstiffenerscat2.SelectedItem.Text != "Select Thickness")
                    {
                        third = ddlPCEnclosureECOStandingFrontDoorwithstiffenerscat2.SelectedItem.Text;
                        sentence += " in " + third + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingFrontDoorwithstiffeners.Text, second, third, "", "", "", sentence);
                }

                //7
                if (ChkPCEnclosureECOStandingCableSupportAngle.Checked == true)
                {
                    validation = 0;
                    string second = "", sentence = "";

                    if (txtPCEnclosureECOStandingCableSupportAnglecat1.Text != "")
                    {
                        second = txtPCEnclosureECOStandingCableSupportAnglecat1.Text;
                        sentence = ChkPCEnclosureECOStandingCableSupportAngle.Text + ": Qty " + second;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingCableSupportAngle.Text, second, "", "", "", "", sentence);
                }

                //8
                if (ChkPCEnclosureECOStandingLock.Checked == true)
                {
                    validation = 0;
                    string second = "", sentence = "";

                    if (ddlPCEnclosureECOStandingLockcat1.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEnclosureECOStandingLockcat1.SelectedItem.Text;
                        sentence = ChkPCEnclosureECOStandingLock.Text + ": " + second + " ";
                    }
                    if (txtPCEnclosureECOStandingLockcat2.Text != "")
                    {
                        sentence += "Qty " + txtPCEnclosureECOStandingLockcat2.Text;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingLock.Text, second, txtPCEnclosureECOStandingLockcat2.Text, "", "", "", sentence);
                }

                //9
                if (ChkPCEnclosureECOStandingHorizontalPartition.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", sentence = "";

                    if (ddlPCEnclosureECOStandingHorizontalPartitioncat1.SelectedItem.Text != "Select Thickness")
                    {
                        first = ddlPCEnclosureECOStandingHorizontalPartitioncat1.SelectedItem.Text;
                        sentence = ChkPCEnclosureECOStandingHorizontalPartition.Text + ": in " + first + " thickness";
                    }
                    if (ddlPCEnclosureECOStandingHorizontalPartitioncat2.SelectedItem.Text != "Select Colour")
                    {
                        second = ddlPCEnclosureECOStandingHorizontalPartitioncat2.SelectedItem.Text;
                        sentence += " colour " + second;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingHorizontalPartition.Text, first, second, "", "", "", sentence);
                }

                //10
                if (ChkPCEnclosureECOStandingSlidingKeyboarddrawer.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", sentence = "";

                    if (ddlPCEnclosureECOStandingSlidingKeyboarddrawercat1.SelectedItem.Text != "Select Thickness")
                    {
                        second = ddlPCEnclosureECOStandingSlidingKeyboarddrawercat1.SelectedItem.Text;
                        sentence = ChkPCEnclosureECOStandingSlidingKeyboarddrawer.Text + ": in " + second + " thickness";
                    }
                    if (ddlPCEnclosureECOStandingSlidingKeyboarddrawercat2.SelectedItem.Text != "Select Colour")
                    {
                        third = ddlPCEnclosureECOStandingSlidingKeyboarddrawercat2.SelectedItem.Text;
                        sentence += " Colour " + third;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingSlidingKeyboarddrawer.Text, second, third, "", "", "", sentence);
                }

                //11
                if (ChkPCEnclosureECOStandingLiftingIBolt.Checked == true)
                {
                    validation = 0;
                    string second = "", sentence = "";
                    sentence = ChkPCEnclosureECOStandingLiftingIBolt.Text + ": ";

                    if (txtPCEnclosureECOStandingLiftingIBoltcat1.Text != "")
                    {
                        second = txtPCEnclosureECOStandingLiftingIBoltcat1.Text;
                        sentence += "Qty " + txtPCEnclosureECOStandingLiftingIBoltcat1.Text;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingLiftingIBolt.Text, second, "", "", "", "", sentence);
                }

                //12
                if (ChkPCEnclosureECOStandingBase.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", third = "", sentence = "";

                    if (ddlPCEnclosureECOStandingBasecat1.SelectedItem.Text != "Select")
                    {
                        first = ddlPCEnclosureECOStandingBasecat1.SelectedItem.Text;
                        sentence = ChkPCEnclosureECOStandingBase.Text + ": " + first;
                    }

                    if (ddlPCEnclosureECOStandingBasecat2.Text != "Select Thickness")
                    {
                        second = ddlPCEnclosureECOStandingBasecat2.Text;
                        sentence += " in " + second + " thickness";
                    }

                    if (ddlPCEnclosureECOStandingBasecat3.Text != "Select Height")
                    {
                        third = ddlPCEnclosureECOStandingBasecat3.Text;

                        if (ddlPCEnclosureECOStandingBasecat3.Text != "Specify")
                        {
                            sentence += " height " + third;
                        }
                    }
                    if (ddlPCEnclosureECOStandingBasecat3.Text == "Specify")
                    {
                        sentence += " height " + txtPCEnclosureECOStandingBasecat4.Text;
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingBase.Text, first, second, third, txtPCEnclosureECOStandingBasecat4.Text, "", sentence);
                }

                //13
                if (ChkPCEnclosureECOStandingAntivibrationpad.Checked == true)
                {
                    validation = 0;
                    string second = "", sentence = "";

                    if (ddlPCEnclosureECOStandingAntivibrationpadcat1.Text != "Select Thickness")
                    {
                        second = ddlPCEnclosureECOStandingAntivibrationpadcat1.Text;
                        sentence = ChkPCEnclosureECOStandingAntivibrationpad.Text + ": in " + second + " thickness";
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingAntivibrationpad.Text, second, "", "", "", "", sentence);
                }
                //14
                if (ChkPCEnclosureECOStandingTransparentDoor.Checked == true)
                {
                    validation = 0;
                    string second = "", third = "", fourth = "", sentence = "";

                    if (ddlPCEnclosureECOStandingTransparentDoorcat1.SelectedItem.Text != "Select")
                    {
                        second = ddlPCEnclosureECOStandingTransparentDoorcat1.SelectedItem.Text;
                        sentence = ChkPCEnclosureECOStandingTransparentDoor.Text + ": " + second;
                    }
                    if (ddlPCEnclosureECOStandingTransparentDoor2.SelectedItem.Text != "Select Thickness")
                    {
                        third = ddlPCEnclosureECOStandingTransparentDoor2.SelectedItem.Text;
                        sentence += " in " + third + " thickness";
                    }
                    if (ddlPCEnclosureECOStandingTransparentDoor3.SelectedItem.Text != "Select")
                    {
                        fourth = ddlPCEnclosureECOStandingTransparentDoor3.SelectedItem.Text;
                        sentence += " " + fourth;
                    }

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingTransparentDoor.Text, second, third, fourth, "", "", sentence);
                }
                //15
                if (ChkPCEnclosureECOStandingCasterWheel.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", third = "", fourth = "", sentence = "";
                    sentence = ChkPCEnclosureECOStandingCasterWheel.Text + ": ";

                    if (txtPCEnclosureECOStandingCasterWheelcat1.Text != "")
                    {
                        first = txtPCEnclosureECOStandingCasterWheelcat1.Text;
                        sentence += ChkPCEnclosureECOStandingCasterWheel.Text + ": size " + first;
                    }
                    if (txtPCEnclosureECOStandingCasterWheelcat2.Text != "")
                    {
                        second = txtPCEnclosureECOStandingCasterWheelcat2.Text;
                        sentence += " with Revolving Qty " + second;
                    }
                    if (txtPCEnclosureECOStandingCasterWheelcat3.Text != "")
                    {
                        third = txtPCEnclosureECOStandingCasterWheelcat3.Text;
                        sentence += " Fixed Qty " + third;
                    }
                    if (txtPCEnclosureECOStandingCasterWheelcat4.Text != "")
                    {
                        fourth = txtPCEnclosureECOStandingCasterWheelcat4.Text;
                        sentence += " " + fourth;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingCasterWheel.Text, first, second, third, fourth, "", sentence);
                }
                //16
                if (ChkPCEnclosureECOStandingFilters.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", sentence = "";

                    if (ddlPCEnclosureECOStandingFilterscat1.Text != "Select Inch")
                    {
                        first = ddlPCEnclosureECOStandingFilterscat1.Text;
                        sentence = ChkPCEnclosureECOStandingFilters.Text + ": " + first + " inch";
                    }
                    if (txtPCEnclosureECOStandingFilterscat2.Text != "")
                    {
                        second = txtPCEnclosureECOStandingFilterscat2.Text;
                        sentence += " Qty " + second;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingFilters.Text, first, second, "", "", "", sentence);
                }

                //17
                if (ChkPCEnclosureECOStandingTelescopicRail.Checked == true)
                {
                    validation = 0;
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingTelescopicRail.Text, "", "", "", "", "", ChkPCEnclosureECOStandingTelescopicRail.Text);
                }

                //18
                if (ChkPCEnclosureECOStandingJointlesspolyurethane.Checked == true)
                {
                    validation = 0;
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingJointlesspolyurethane.Text, "", "", "", "", "", ChkPCEnclosureECOStandingJointlesspolyurethane.Text);
                }

                //19
                if (ChkPCEnclosureECOStandingPowderCoating.Checked == true)
                {
                    validation = 0;
                    string first = "", second = "", sentence = "";
                    sentence = ChkPCEnclosureECOStandingPowderCoating.Text + ": Colour ";
                    if (ddlPCEnclosureECOStandingPowderCoatingcat1.Text != "Select Colour")
                    {
                        first = ddlPCEnclosureECOStandingPowderCoatingcat1.SelectedItem.Text;
                        if (ddlPCEnclosureECOStandingPowderCoatingcat1.Text != "Specify")
                        {
                            sentence += first;
                        }
                        if (ddlPCEnclosureECOStandingPowderCoatingcat1.Text == "Specify")
                        {
                            sentence += txtPCEnclosureECOStandingFilterscat2.Text;
                        }
                    }
                    if (txtPCEnclosureECOStandingFilterscat2.Text != "")
                    {
                        second += txtPCEnclosureECOStandingFilterscat2.Text;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingPowderCoating.Text, first, second, "", "", "", sentence);
                }
                //20
                if (ChkPCEnclosureECOStandingfan.Checked == true)
                {
                    validation = 0;
                    string first = "", sentence = "";
                    if (ddlPCEnclosureECOStandingfancat1.SelectedItem.Text != "Select Size")
                    {
                        first = ddlPCEnclosureECOStandingfancat1.SelectedItem.Text;
                        sentence = ChkPCEnclosureECOStandingfan.Text + ": In Size " + first + " inch qty: " + txtPCEnclosureECOStandingfancat2.Text;
                    }
                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingfan.Text, first, txtPCEnclosureECOStandingfancat2.Text, "", "", "", sentence);
                }
                //21
                if (ChkPCEnclosureECOStandingAnyadditional.Checked == true)
                {
                    validation = 0;
                    string first = "", sentence = "";

                    first = txtPCEnclosureECOStandingAnyadditionalcat1.Text;
                    sentence = ChkPCEnclosureECOStandingAnyadditional.Text + ": " + first;

                    dtConstructionType.Rows.Add(quotationno, quotationid, ChkPCEnclosureECOStandingAnyadditional.Text, txtPCEnclosureECOStandingAnyadditionalcat1.Text, "", "", "", "", sentence);
                }
            }
        }
        #endregion

        #region 10.PC TABLE

        if (ddlConstype.Text == "PC TABLE")
        {
            if (ChkPcTableWeldedMainbody.Checked == false && ChkPcTableGlandPlate.Checked == false
                && ChkPcTableComponentMtgPlate.Checked == false && ChkPcTableSideCPlate.Checked == false
                && ChkPcTableDoorCPlate.Checked == false && ChkPcTableFrontDoor.Checked == false
                && ChkPcTableRearDoor.Checked == false && ChkPcTableLock.Checked == false
                && ChkPcTableCableSupportAngle.Checked == false
                && ChkPcTableHorizontalPartition.Checked == false && ChkPcTableSlidingKeyboarddrawer.Checked == false
                && ChkPcTableSlidingCPUdrawer.Checked == false && ChkPcTableMonitomountingbracket.Checked == false
                && ChkPcTableLiftingIBolt.Checked == false && ChkPcTableBase.Checked == false
                && ChkPcTableAntivibrationpad.Checked == false && ChkPcTableTransparentDoor.Checked == false
                && ChkPcTableCasterWheel.Checked == false && ChkPcTableFilters.Checked == false && ChkPcTablefan.Checked == false
                && ChkPcTablePowderCoatingShade.Checked == false
                && ChkPcTableJointlesspolyurethanefoamed.Checked == false && ChkPcTableAnyadditionalcomponent.Checked == false)
            {
                validation = 1;
            }

            action = "insertPCTABLE";
            //1
            if (ChkPcTableWeldedMainbody.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (ddlPcTableWeldedMainbodycat1.SelectedItem.Text != "Select")
                {
                    first = ddlPcTableWeldedMainbodycat1.SelectedItem.Text;
                    sentence = ChkPcTableWeldedMainbody.Text + " " + first;
                }
                if (ddlPcTableWeldedMainbodycat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlPcTableWeldedMainbodycat2.SelectedItem.Text;
                    sentence += " in " + second + " in thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableWeldedMainbody.Text, first, second, "", "", "", sentence);
            }

            //2
            if (ChkPcTableGlandPlate.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPcTableGlandPlatecat1.SelectedItem.Text != "Select")
                {
                    first = ddlPcTableGlandPlatecat1.SelectedItem.Text;
                    sentence = ChkPcTableGlandPlate.Text + ": " + first;
                }
                if (ddlPcTableGlandPlatecat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlPcTableGlandPlatecat2.SelectedItem.Text;
                    sentence += " in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableGlandPlate.Text, first, second, "", "", "", sentence);
            }

            //3
            if (ChkPcTableComponentMtgPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlPcTableComponentMtgPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlPcTableComponentMtgPlatecat1.SelectedItem.Text;
                    sentence = ChkPcTableComponentMtgPlate.Text + ": " + second;
                }
                if (ddlPcTableComponentMtgPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlPcTableComponentMtgPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlPcTableComponentMtgPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlPcTableComponentMtgPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableComponentMtgPlate.Text, second, third, fourth, "", "", sentence);
            }

            //4
            if (ChkPcTableSideCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlPcTableSideCPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlPcTableSideCPlatecat1.SelectedItem.Text;
                    sentence = ChkPcTableSideCPlate.Text + ": " + second;
                }
                if (ddlPcTableSideCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlPcTableSideCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlPcTableSideCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlPcTableSideCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness " + ",";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableSideCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //5
            if (ChkPcTableDoorCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlPcTableDoorCPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlPcTableDoorCPlatecat1.SelectedItem.Text;
                    sentence = ChkPcTableDoorCPlate.Text + ": " + second;
                }
                if (ddlPcTableDoorCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlPcTableDoorCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlPcTableDoorCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlPcTableDoorCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableDoorCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //6
            if (ChkPcTableFrontDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlPcTableFrontDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlPcTableFrontDoorcat1.SelectedItem.Text;
                    sentence = ChkPcTableFrontDoor.Text + ": " + second;
                }
                if (ddlPcTableFrontDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlPcTableFrontDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableFrontDoor.Text, second, third, "", "", "", sentence);
            }

            //7
            if (ChkPcTableRearDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlPcTableRearDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlPcTableRearDoorcat1.SelectedItem.Text;
                    sentence = ChkPcTableRearDoor.Text + ": " + second;
                }
                if (ddlPcTableRearDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlPcTableRearDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableRearDoor.Text, second, third, "", "", "", sentence);
            }

            //8
            if (ChkPcTableLock.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (ddlPcTableLockcat1.SelectedItem.Text != "Select")
                {
                    first = ddlPcTableLockcat1.SelectedItem.Text;
                    sentence = ChkPcTableLock.Text + ": " + first;
                }
                if (!string.IsNullOrEmpty(txtPcTableLockcat2.Text))
                {
                    second = txtPcTableLockcat2.Text;
                    sentence += " Qty " + second + "Nos";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableLock.Text, first, second, "", "", "", sentence);
            }

            //9
            if (ChkPcTableCableSupportAngle.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (txtPcTableCableSupportAnglecat1.Text != "")
                {
                    second = txtPcTableCableSupportAnglecat1.Text;
                    sentence = ChkPcTableCableSupportAngle.Text + ": Qty " + second + "Nos";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableCableSupportAngle.Text, second, "", "", "", "", sentence);
            }

            //10
            if (ChkPcTableHorizontalPartition.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPcTableHorizontalPartitioncat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlPcTableHorizontalPartitioncat1.SelectedItem.Text;
                    sentence = ChkPcTableHorizontalPartition.Text + ": in " + first + " thickness";
                }
                if (ddlPcTableHorizontalPartitioncat2.SelectedItem.Text != "Select Colour")
                {
                    second = ddlPcTableHorizontalPartitioncat2.SelectedItem.Text;
                    sentence += " Colour " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableHorizontalPartition.Text, first, second, "", "", "", sentence);
            }

            //11
            if (ChkPcTableSlidingKeyboarddrawer.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPcTableSlidingKeyboarddrawercat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlPcTableSlidingKeyboarddrawercat1.SelectedItem.Text;
                    sentence = ChkPcTableSlidingKeyboarddrawer.Text + ": in " + first + " thickness";
                }
                if (ddlPcTableSlidingKeyboarddrawercat2.SelectedItem.Text != "Select Colour")
                {
                    second = ddlPcTableSlidingKeyboarddrawercat2.SelectedItem.Text;
                    sentence += " Colour " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableSlidingKeyboarddrawer.Text, first, second, "", "", "", sentence);
            }

            //12
            if (ChkPcTableSlidingCPUdrawer.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPcTableSlidingCPUdrawercat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlPcTableSlidingCPUdrawercat1.SelectedItem.Text;
                    sentence = ChkPcTableSlidingCPUdrawer.Text + ": in " + first + " thickness";
                }
                if (ddlPcTableSlidingCPUdrawercat2.SelectedItem.Text != "Select Colour")
                {
                    second = ddlPcTableSlidingCPUdrawercat2.SelectedItem.Text;
                    sentence += " Colour " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableSlidingCPUdrawer.Text, first, second, "", "", "", sentence);
            }

            //13
            if (ChkPcTableMonitomountingbracket.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPcTableMonitomountingbracketcat1.SelectedItem.Text != "Select")
                {
                    first = ddlPcTableMonitomountingbracketcat1.SelectedItem.Text;
                    sentence = ChkPcTableMonitomountingbracket.Text + ": " + first;
                }
                if (ddlPcTableMonitomountingbracketcat2.SelectedItem.Text != "Select Colour")
                {
                    second = ddlPcTableMonitomountingbracketcat2.SelectedItem.Text;
                    sentence += " Colour " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableMonitomountingbracket.Text, first, second, "", "", "", sentence);
            }

            //14
            if (ChkPcTableLiftingIBolt.Checked == true)
            {
                validation = 0;
                string sentence = "";

                sentence = ChkPcTableLiftingIBolt.Text + ": Qty " + txtPcTableLiftingIBoltcat1.Text + " Nos";
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableLiftingIBolt.Text, txtPcTableLiftingIBoltcat1.Text, "", "", "", "", sentence);
            }

            //15
            if (ChkPcTableBase.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlPcTableBasecat1.SelectedItem.Text != "Select")
                {
                    second = ddlPcTableBasecat1.SelectedItem.Text;
                    sentence = ChkPcTableBase.Text + ": " + second;
                }
                if (ddlPcTableBasecat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlPcTableBasecat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlPcTableBasecat3.SelectedItem.Text != "Select Height")
                {
                    fourth = ddlPcTableBasecat3.SelectedItem.Text;
                    sentence += "Height " + fourth;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableBase.Text, second, third, fourth, "", "", sentence);
            }

            //16
            if (ChkPcTableAntivibrationpad.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlPcTableAntivibrationpadcat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlPcTableAntivibrationpadcat1.SelectedItem.Text;
                    sentence = ChkPcTableAntivibrationpad.Text + ": in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableAntivibrationpad.Text, second, "", "", "", "", sentence);
            }

            //17
            if (ChkPcTableTransparentDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlPcTableTransparentDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlPcTableTransparentDoorcat1.SelectedItem.Text;
                    sentence = ChkPcTableTransparentDoor.Text + ": " + second;
                }
                if (ddlPcTableTransparentDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlPcTableTransparentDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlPcTableTransparentDoorcat3.SelectedItem.Text != "Select")
                {
                    fourth = ddlPcTableTransparentDoorcat3.SelectedItem.Text;
                    sentence += fourth;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableTransparentDoor.Text, second, third, fourth, "", "", sentence);
            }

            //18
            if (ChkPcTableCasterWheel.Checked == true)
            {
                validation = 0;
                string sentence = "", first = "", second = "", third = "", fourth = "";

                first = txtPcTableCasterWheelcat1.Text;
                second = txtPcTableCasterWheelcat2.Text;
                third = txtPcTableCasterWheelcat3.Text;
                fourth = txtPcTableCasterWheelcat4.Text;

                sentence = ChkPcTableCasterWheel.Text + ": in size " + first + " Resolving Qty " + second + " with Fixed Qty " + third + " " + fourth;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableCasterWheel.Text, first, second, third, fourth, "", sentence);
            }

            //19
            if (ChkPcTableFilters.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPcTableFilterscat1.SelectedItem.Text != "Select")
                {
                    first = ddlPcTableFilterscat1.SelectedItem.Text;
                    //sentence = ChkPcTableTransparentDoor.Text + ": " + first + " inch";
                    sentence = "Flter" + ": " + first + " inch";
                }
                if (txtPcTableFilterscat2.Text != "")
                {
                    second = txtPcTableFilterscat2.Text;
                    sentence += " Qty " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableFilters.Text, first, second, "", "", "", sentence);
            }

            //20
            if (ChkPcTablePowderCoatingShade.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPcTablePowderCoatingShadecat1.SelectedItem.Text != "Select Colour")
                {
                    sentence = ChkPcTablePowderCoatingShade.Text + ": ";
                    first = ddlPcTablePowderCoatingShadecat1.SelectedItem.Text;

                    if (ddlPcTablePowderCoatingShadecat1.SelectedItem.Text != "Specify")
                    {
                        sentence += first;
                    }
                }
                if (txtPcTablePowderCoatingShadecat2.Text != "")
                {
                    second = txtPcTablePowderCoatingShadecat2.Text;

                    if (ddlPcTablePowderCoatingShadecat1.SelectedItem.Text == "Specify")
                    {
                        sentence += second;
                    }
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTablePowderCoatingShade.Text, first, second, "", "", "", sentence);
            }

            if (ChkPcTablefan.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlPcTablefancat1.SelectedItem.Text != "Select Size")
                {
                    first = ddlPcTablefancat1.SelectedItem.Text;
                    sentence = ChkPcTablefan.Text + ": In Size " + first + " inch qty: " + txtPcTablefancat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTablefan.Text, first, txtPcTablefancat2.Text, "", "", "", sentence);
            }

            //21
            if (ChkPcTableJointlesspolyurethanefoamed.Checked == true)
            {
                validation = 0;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableJointlesspolyurethanefoamed.Text, "", "", "", "", "", ChkPcTableJointlesspolyurethanefoamed.Text);
            }

            //22
            if (ChkPcTableAnyadditionalcomponent.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                first = txtPcTableAnyadditionalcomponentcat1.Text;
                sentence = ChkPcTableAnyadditionalcomponent.Text + ": " + first;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPcTableAnyadditionalcomponent.Text, txtPcTableAnyadditionalcomponentcat1.Text, "", "", "", "", sentence);
            }
        }
        #endregion

        #region 10.PRINTER TABLE

        if (ddlConstype.Text == "PRINTER TABLE")
        {
            if (ChkPrinterTableWeldedMainBody.Checked == false && ChkPrinterGlandPlate.Checked == false
                && ChkPrinterTableComponentMtgPlate.Checked == false && ChkPrinterTableSideCPlate.Checked == false
                && ChkPrinterTableDoorCPlate.Checked == false && ChkPrinterTableFrontDoor.Checked == false
                && ChkPrinterTableRearDoor.Checked == false && ChkPrinterTableDoorStiffener.Checked == false
                && ChkPrinterTableLock.Checked == false && ChkPrinterTableCableSupportAngle.Checked == false
                && ChkPrinterTableHorizontalPartition.Checked == false && ChkPrinterTableSlidingdrawer.Checked == false
                && ChkPrinterTableLiftingIBolt.Checked == false && ChkPrinterTableBase.Checked == false
                && ChkPrinterTableAntivibrationpad.Checked == false && ChkPrinterTableTransparentDoor.Checked == false
                && ChkPrinterTableCasterWheel.Checked == false && ChkPrinterTableFilters.Checked == false && ChkPrinterTablefan.Checked == false
                && ChkPrinterTablePowderCoatingShade.Checked == false)
            {
                validation = 1;
            }

            action = "insertPrinterTABLE";
            //1
            if (ChkPrinterTableWeldedMainBody.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (ddlPrinterTableWeldedMainBodycat1.SelectedItem.Text != "Select")
                {
                    first = ddlPrinterTableWeldedMainBodycat1.SelectedItem.Text;
                    sentence = ChkPrinterTableWeldedMainBody.Text + " " + first;
                }
                if (ddlPrinterTableWeldedMainBodycat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlPrinterTableWeldedMainBodycat2.SelectedItem.Text;
                    sentence += " in " + second + " in thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableWeldedMainBody.Text, first, second, "", "", "", sentence);
            }

            //2
            if (ChkPrinterGlandPlate.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPrinterGlandPlatecat1.SelectedItem.Text != "Select")
                {
                    first = ddlPrinterGlandPlatecat1.SelectedItem.Text;
                    sentence = ChkPrinterGlandPlate.Text + ": " + first;
                }
                if (ddlPrinterGlandPlatecat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlPrinterGlandPlatecat2.SelectedItem.Text;
                    sentence += " in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterGlandPlate.Text, first, second, "", "", "", sentence);
            }

            //3
            if (ChkPrinterTableComponentMtgPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlPrinterTableComponentMtgPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlPrinterTableComponentMtgPlatecat1.SelectedItem.Text;
                    sentence = ChkPrinterTableComponentMtgPlate.Text + ": " + second;
                }
                if (ddlPrinterTableComponentMtgPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlPrinterTableComponentMtgPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlPrinterTableComponentMtgPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlPrinterTableComponentMtgPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableComponentMtgPlate.Text, second, third, fourth, "", "", sentence);
            }

            //4
            if (ChkPrinterTableSideCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlPrinterTableSideCPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlPrinterTableSideCPlatecat1.SelectedItem.Text;
                    sentence = ChkPrinterTableSideCPlate.Text + ": " + second;
                }
                if (ddlPrinterTableSideCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlPrinterTableSideCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlPrinterTableSideCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlPrinterTableSideCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableSideCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //5
            if (ChkPrinterTableDoorCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlPrinterTableDoorCPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlPrinterTableDoorCPlatecat1.SelectedItem.Text;
                    sentence = ChkPrinterTableDoorCPlate.Text + ": " + second;
                }
                if (ddlPrinterTableDoorCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlPrinterTableDoorCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlPrinterTableDoorCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlPrinterTableDoorCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableDoorCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //6
            if (ChkPrinterTableFrontDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlPrinterTableFrontDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlPrinterTableFrontDoorcat1.SelectedItem.Text;
                    sentence = ChkPrinterTableFrontDoor.Text + ": " + second;
                }
                if (ddlPrinterTableFrontDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlPrinterTableFrontDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableFrontDoor.Text, second, third, "", "", "", sentence);
            }

            //7
            if (ChkPrinterTableRearDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlPrinterTableRearDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlPrinterTableRearDoorcat1.SelectedItem.Text;
                    sentence = ChkPrinterTableRearDoor.Text + ": " + second;
                }
                if (ddlPrinterTableRearDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlPrinterTableRearDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableRearDoor.Text, second, third, "", "", "", sentence);
            }

            //8
            if (ChkPrinterTableDoorStiffener.Checked == true)
            {
                validation = 0;
                string sentence = "";

                sentence = ChkPrinterTableDoorStiffener.Text;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableDoorStiffener.Text, "", "", "", "", "", sentence);
            }

            //9
            if (ChkPrinterTableLock.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (ddlPrinterTableLockcat1.SelectedItem.Text != "Select")
                {
                    first = ddlPrinterTableLockcat1.SelectedItem.Text;
                    sentence = ChkPrinterTableLock.Text + ": " + first;
                }
                if (!string.IsNullOrEmpty(txtPrinterTableLockcat2.Text))
                {
                    second = txtPrinterTableLockcat2.Text;
                    sentence += " Qty " + second + "Nos";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableLock.Text, first, second, "", "", "", sentence);
            }

            //10
            if (ChkPrinterTableCableSupportAngle.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (txtPrinterTableCableSupportAnglecat1.Text != "")
                {
                    second = txtPrinterTableCableSupportAnglecat1.Text;
                    sentence = ChkPrinterTableCableSupportAngle.Text + ": Qty " + second + " Nos";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableCableSupportAngle.Text, second, "", "", "", "", sentence);
            }

            //11
            if (ChkPrinterTableHorizontalPartition.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPrinterTableHorizontalPartitioncat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlPrinterTableHorizontalPartitioncat1.SelectedItem.Text;
                    sentence = ChkPrinterTableHorizontalPartition.Text + ": in " + first + " thickness";
                }
                if (ddlPrinterTableHorizontalPartitioncat2.SelectedItem.Text != "Select Colour")
                {
                    second = ddlPrinterTableHorizontalPartitioncat2.SelectedItem.Text;
                    sentence += " Colour " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableHorizontalPartition.Text, first, second, "", "", "", sentence);
            }

            //12
            if (ChkPrinterTableSlidingdrawer.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPrinterTableSlidingdrawercat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlPrinterTableSlidingdrawercat1.SelectedItem.Text;
                    sentence = ChkPrinterTableSlidingdrawer.Text + ": in " + first + " thickness";
                }
                if (ddlPrinterTableSlidingdrawercat2.SelectedItem.Text != "Select Colour")
                {
                    second = ddlPrinterTableSlidingdrawercat2.SelectedItem.Text;
                    sentence += " Colour " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableSlidingdrawer.Text, first, second, "", "", "", sentence);
            }

            //13
            if (ChkPrinterTableLiftingIBolt.Checked == true)
            {
                validation = 0;
                string sentence = "";

                sentence = ChkPrinterTableLiftingIBolt.Text + ": Qty " + txtPrinterTableLiftingIBoltcat1.Text + " Nos";
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableLiftingIBolt.Text, txtPrinterTableLiftingIBoltcat1.Text, "", "", "", "", sentence);
            }

            //14
            if (ChkPrinterTableBase.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlPrinterTableBasecat1.SelectedItem.Text != "Select")
                {
                    second = ddlPrinterTableBasecat1.SelectedItem.Text;
                    sentence = ChkPrinterTableBase.Text + ": " + second;
                }
                if (ddlPrinterTableBasecat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlPrinterTableBasecat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlPrinterTableBasecat3.SelectedItem.Text != "Select Height")
                {
                    fourth = ddlPrinterTableBasecat3.SelectedItem.Text;
                    sentence += "Height " + fourth;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableBase.Text, second, third, fourth, "", "", sentence);
            }

            //15
            if (ChkPrinterTableAntivibrationpad.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlPrinterTableAntivibrationpadcat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlPrinterTableAntivibrationpadcat1.SelectedItem.Text;
                    sentence = ChkPrinterTableAntivibrationpad.Text + ": in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableAntivibrationpad.Text, second, "", "", "", "", sentence);
            }

            //16
            if (ChkPrinterTableTransparentDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlPrinterTableTransparentDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlPrinterTableTransparentDoorcat1.SelectedItem.Text;
                    sentence = ChkPrinterTableTransparentDoor.Text + ": " + second;
                }
                if (ddlPrinterTableTransparentDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlPrinterTableTransparentDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlPrinterTableTransparentDoorcat3.SelectedItem.Text != "Select")
                {
                    fourth = ddlPrinterTableTransparentDoorcat3.SelectedItem.Text;
                    sentence += fourth;
                }

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableTransparentDoor.Text, second, third, fourth, "", "", sentence);
            }

            //17
            if (ChkPrinterTableCasterWheel.Checked == true)
            {
                validation = 0;
                string sentence = "", first = "", second = "", third = "", fourth = "";

                first = txtPrinterTableCasterWheelcat1.Text;
                second = txtPrinterTableCasterWheelcat2.Text;
                third = txtPrinterTableCasterWheelcat3.Text;
                fourth = txtPrinterTableCasterWheelcat4.Text;

                sentence = ChkPrinterTableCasterWheel.Text + ": " + first + " Inch Resolving Qty " + second + " with Fixed Qty " + third + " " + fourth;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableCasterWheel.Text, first, second, third, fourth, "", sentence);
            }

            //18
            if (ChkPrinterTableFilters.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPrinterTableFilterscat1.SelectedItem.Text != "Select")
                {
                    first = ddlPrinterTableFilterscat1.SelectedItem.Text;
                    sentence = ChkPrinterTableFilters.Text + ": " + first + " inch";
                }
                if (txtPrinterTableFilterscat2.Text != "")
                {
                    second = txtPrinterTableFilterscat2.Text;
                    sentence += " Qty " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableFilters.Text, first, second, "", "", "", sentence);
            }
            //19
            if (ChkPrinterTablePowderCoatingShade.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlPrinterTablePowderCoatingShadecat1.SelectedItem.Text != "Select Colour")
                {
                    sentence = ChkPrinterTablePowderCoatingShade.Text + ": ";
                    first = ddlPrinterTablePowderCoatingShadecat1.SelectedItem.Text;

                    if (ddlPrinterTablePowderCoatingShadecat1.SelectedItem.Text != "Specify")
                    {
                        sentence += first;
                    }
                }
                if (txtPrinterTablePowderCoatingShadecat2.Text != "")
                {
                    second = txtPrinterTablePowderCoatingShadecat2.Text;

                    if (ddlPrinterTablePowderCoatingShadecat1.SelectedItem.Text == "Specify")
                    {
                        sentence += second;
                    }
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTablePowderCoatingShade.Text, first, second, "", "", "", sentence);
            }
            //20
            if (ChkPrinterTablefan.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlPrinterTablefancat1.SelectedItem.Text != "Select Size")
                {
                    first = ddlPrinterTablefancat1.SelectedItem.Text;
                    sentence = ChkPrinterTablefan.Text + ": In Size " + first + " inch qty: " + txtPrinterTablefancat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTablefan.Text, first, txtPrinterTablefancat2.Text, "", "", "", sentence);
            }
            //21
            if (ChkPrinterTableJointlesspolyurethanefoamed.Checked == true)
            {
                validation = 0;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableJointlesspolyurethanefoamed.Text, "", "", "", "", "", ChkPrinterTableJointlesspolyurethanefoamed.Text);
            }

            //22
            if (ChkPrinterTableAnyadditionalcomponent.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                first = txtPrinterTableAnyadditionalcomponentcat1.Text;
                sentence = ChkPrinterTableAnyadditionalcomponent.Text + ": " + first;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkPrinterTableAnyadditionalcomponent.Text, txtPrinterTableAnyadditionalcomponentcat1.Text, "", "", "", "", sentence);
            }
        }
        #endregion

        #region 11.Single Piece Desk

        if (ddlConstype.Text == "Single Piece Desk")
        {
            if (ChkSinglePieceWeldedMainBody.Checked == false && ChkSinglePieceGlandPlate.Checked == false
                && ChkSinglePieceComponentMtgPlate.Checked == false && ChkSinglePieceSideCPlate.Checked == false
                && ChkSinglePieceDoorCPlate.Checked == false && ChkSinglePieceFrontDoor.Checked == false
                && ChkSinglePieceRearDoor.Checked == false && ChkSinglePieceDoorStiffener.Checked == false
                && ChkSinglePieceLock.Checked == false && ChkSinglePieceCableSupportAngle.Checked == false
                && ChkSinglePieceSlidingKeyboarddrawertelescopicrails.Checked == false && ChkSinglePieceSlidingdrawerwithtelescopicrails.Checked == false
                && ChkSinglePieceMonitormountingarrangement.Checked == false && ChkSinglePieceBase.Checked == false
                && ChkSinglePieceAntivibrationpad.Checked == false && ChkSinglePieceTransparentDoor.Checked == false
                && ChkSinglePieceCasterWheel.Checked == false && ChkSinglePieceFilters.Checked == false && ChkSinglePiecefan.Checked == false
                && ChkSinglePiecePowderCoatingShade.Checked == false && ChkSinglePieceJointlesspolyurethanefoamed.Checked == false
                && ChkSinglePieceAnyadditionalcomponent.Checked == false)
            {
                validation = 1;
            }

            action = "insertSinglePieceDesk";
            //1
            if (ChkSinglePieceWeldedMainBody.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                if (ddlSinglePieceWeldedMainBodycat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlSinglePieceWeldedMainBodycat1.SelectedItem.Text;
                    sentence += ChkSinglePieceWeldedMainBody.Text + " in " + first + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceWeldedMainBody.Text, first, "", "", "", "", sentence);
            }

            //2
            if (ChkSinglePieceGlandPlate.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlSinglePieceGlandPlatecat1.SelectedItem.Text != "Select")
                {
                    first = ddlSinglePieceGlandPlatecat1.SelectedItem.Text;
                    sentence = ChkSinglePieceGlandPlate.Text + ": " + first;
                }
                if (ddlSinglePieceGlandPlatecat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlSinglePieceGlandPlatecat2.SelectedItem.Text;
                    sentence += " in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceGlandPlate.Text, first, second, "", "", "", sentence);
            }

            //3
            if (ChkSinglePieceComponentMtgPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlSinglePieceComponentMtgPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlSinglePieceComponentMtgPlatecat1.SelectedItem.Text;
                    sentence = ChkSinglePieceComponentMtgPlate.Text + ": " + second;
                }
                if (ddlSinglePieceComponentMtgPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlSinglePieceComponentMtgPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlSinglePieceComponentMtgPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlSinglePieceComponentMtgPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceComponentMtgPlate.Text, second, third, fourth, "", "", sentence);
            }

            //4
            if (ChkSinglePieceSideCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlSinglePieceSideCPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlSinglePieceSideCPlatecat1.SelectedItem.Text;
                    sentence = ChkSinglePieceSideCPlate.Text + ": " + second;
                }
                if (ddlSinglePieceSideCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlSinglePieceSideCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlSinglePieceSideCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlSinglePieceSideCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceSideCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //5
            if (ChkSinglePieceDoorCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlSinglePieceDoorCPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlSinglePieceDoorCPlatecat1.SelectedItem.Text;
                    sentence = ChkSinglePieceDoorCPlate.Text + ": " + second;
                }
                if (ddlSinglePieceDoorCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlSinglePieceDoorCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlSinglePieceDoorCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlSinglePieceDoorCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceDoorCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //6
            if (ChkSinglePieceFrontDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlSinglePieceFrontDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlSinglePieceFrontDoorcat1.SelectedItem.Text;
                    sentence = ChkSinglePieceFrontDoor.Text + ": " + second;
                }
                if (ddlSinglePieceFrontDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlSinglePieceFrontDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceFrontDoor.Text, second, third, "", "", "", sentence);
            }

            //7
            if (ChkSinglePieceRearDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlSinglePieceRearDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlSinglePieceRearDoorcat1.SelectedItem.Text;
                    sentence = ChkSinglePieceRearDoor.Text + ": " + second;
                }
                if (ddlSinglePieceRearDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlSinglePieceRearDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceRearDoor.Text, second, third, "", "", "", sentence);
            }

            //8
            if (ChkSinglePieceDoorStiffener.Checked == true)
            {
                validation = 0;
                string sentence = "";

                sentence = ChkSinglePieceDoorStiffener.Text;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceDoorStiffener.Text, "", "", "", "", "", sentence);
            }

            //9
            if (ChkSinglePieceLock.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (ddlSinglePieceLockcat1.SelectedItem.Text != "Select")
                {
                    first = ddlSinglePieceLockcat1.SelectedItem.Text;
                    sentence = ChkSinglePieceLock.Text + ": " + first;
                }
                if (!string.IsNullOrEmpty(txtSinglePieceLockcat2.Text))
                {
                    second = txtSinglePieceLockcat2.Text;
                    sentence += " Qty " + second + "Nos";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceLock.Text, first, second, "", "", "", sentence);
            }

            //10
            if (ChkSinglePieceCableSupportAngle.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (txtSinglePieceCableSupportAnglecat1.Text != "")
                {
                    second = txtSinglePieceCableSupportAnglecat1.Text;
                    sentence = ChkSinglePieceCableSupportAngle.Text + ": Qty " + second + "Nos";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceCableSupportAngle.Text, second, "", "", "", "", sentence);
            }

            //11
            if (ChkSinglePieceSlidingKeyboarddrawertelescopicrails.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlSinglePieceSlidingKeyboarddrawertelescopicrailscat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlSinglePieceSlidingKeyboarddrawertelescopicrailscat1.SelectedItem.Text;
                    sentence = ChkSinglePieceSlidingKeyboarddrawertelescopicrails.Text + ": in " + first + " thickness";
                }
                if (ddlSinglePieceSlidingKeyboarddrawertelescopicrailscat2.SelectedItem.Text != "Select Colour")
                {
                    second = ddlSinglePieceSlidingKeyboarddrawertelescopicrailscat2.SelectedItem.Text;
                    sentence += " Colour " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceSlidingKeyboarddrawertelescopicrails.Text, first, second, "", "", "", sentence);
            }

            //12
            if (ChkSinglePieceSlidingdrawerwithtelescopicrails.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlSinglePieceSlidingdrawerwithtelescopicrailscat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlSinglePieceSlidingdrawerwithtelescopicrailscat1.SelectedItem.Text;
                    sentence = ChkSinglePieceSlidingdrawerwithtelescopicrails.Text + ": in " + first + " thickness";
                }
                if (ddlSinglePieceSlidingdrawerwithtelescopicrailscat2.SelectedItem.Text != "Select Colour")
                {
                    second = ddlSinglePieceSlidingdrawerwithtelescopicrailscat2.SelectedItem.Text;
                    sentence += " Colour " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceSlidingdrawerwithtelescopicrails.Text, first, second, "", "", "", sentence);
            }

            //13
            if (ChkSinglePieceMonitormountingarrangement.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlSinglePieceMonitormountingarrangementcat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlSinglePieceMonitormountingarrangementcat1.SelectedItem.Text;
                    sentence = ChkSinglePieceMonitormountingarrangement.Text + ": in " + first + " thickness";
                }
                if (ddlSinglePieceMonitormountingarrangementcat2.SelectedItem.Text != "Select Colour")
                {
                    second = ddlSinglePieceMonitormountingarrangementcat2.SelectedItem.Text;
                    sentence += " Colour " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceMonitormountingarrangement.Text, first, second, "", "", "", sentence);
            }

            //14
            if (ChkSinglePieceBase.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlSinglePieceBasecat1.SelectedItem.Text != "Select")
                {
                    second = ddlSinglePieceBasecat1.SelectedItem.Text;
                    sentence = ChkSinglePieceBase.Text + ": " + second;
                }
                if (ddlSinglePieceBasecat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlSinglePieceBasecat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlSinglePieceBasecat3.SelectedItem.Text != "Select Height")
                {
                    fourth = ddlSinglePieceBasecat3.SelectedItem.Text;
                    sentence += " Height " + fourth;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceBase.Text, second, third, fourth, "", "", sentence);
            }

            //15
            if (ChkSinglePieceAntivibrationpad.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlSinglePieceAntivibrationpadcat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlSinglePieceAntivibrationpadcat1.SelectedItem.Text;
                    sentence = ChkSinglePieceAntivibrationpad.Text + ": in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceAntivibrationpad.Text, second, "", "", "", "", sentence);
            }

            //16
            if (ChkSinglePieceTransparentDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlSinglePieceTransparentDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlSinglePieceTransparentDoorcat1.SelectedItem.Text;
                    sentence = ChkSinglePieceTransparentDoor.Text + ": " + second;
                }
                if (ddlSinglePieceTransparentDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlSinglePieceTransparentDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlSinglePieceTransparentDoorcat3.SelectedItem.Text != "Select")
                {
                    fourth = ddlSinglePieceTransparentDoorcat3.SelectedItem.Text;
                    sentence += fourth;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceTransparentDoor.Text, second, third, fourth, "", "", sentence);
            }

            //17
            if (ChkSinglePieceCasterWheel.Checked == true)
            {
                validation = 0;
                string sentence = "", first = "", second = "", third = "", fourth = "";

                first = txtSinglePieceCasterWheelcat1.Text;
                second = txtSinglePieceCasterWheelcat2.Text;
                third = txtSinglePieceCasterWheelcat3.Text;
                fourth = txtSinglePieceCasterWheelcat4.Text;

                sentence = ChkSinglePieceCasterWheel.Text + ": in size " + first + " Inch Resolving Qty " + second + " with Fixed Qty " + third + " " + fourth;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceCasterWheel.Text, first, second, third, fourth, "", sentence);
            }

            //18
            if (ChkSinglePieceFilters.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlSinglePieceFilterscat1.SelectedItem.Text != "Select Inch")
                {
                    first = ddlSinglePieceFilterscat1.SelectedItem.Text;
                    sentence = ChkSinglePieceFilters.Text + ": " + first + " inch";
                }
                if (txtSinglePieceFilterscat2.Text != "")
                {
                    second = txtSinglePieceFilterscat2.Text;
                    sentence += " Qty " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceFilters.Text, first, second, "", "", "", sentence);
            }
            //19
            if (ChkSinglePiecefan.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlSinglePiecefancat1.SelectedItem.Text != "Select Size")
                {
                    first = ddlSinglePiecefancat1.SelectedItem.Text;
                    sentence = ChkSinglePiecefan.Text + ": " + first + " inch qty : " + txtSinglePiecefancat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePiecefan.Text, first, txtSinglePiecefancat2.Text, "", "", "", sentence);
            }
            //20
            if (ChkSinglePiecePowderCoatingShade.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlSinglePiecePowderCoatingShadecat1.SelectedItem.Text != "Select Colour")
                {
                    sentence = ChkSinglePiecePowderCoatingShade.Text + ": ";
                    first = ddlSinglePiecePowderCoatingShadecat1.SelectedItem.Text;

                    if (ddlSinglePiecePowderCoatingShadecat1.SelectedItem.Text != "Specify")
                    {
                        sentence += first;
                    }
                }
                if (txtSinglePiecePowderCoatingShadecat2.Text != "")
                {
                    second = txtSinglePiecePowderCoatingShadecat2.Text;

                    if (ddlSinglePiecePowderCoatingShadecat1.SelectedItem.Text == "Specify")
                    {
                        sentence += second;
                    }
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePiecePowderCoatingShade.Text, first, second, "", "", "", sentence);
            }

            //21
            if (ChkSinglePieceJointlesspolyurethanefoamed.Checked == true)
            {
                validation = 0;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceJointlesspolyurethanefoamed.Text, "", "", "", "", "", ChkSinglePieceJointlesspolyurethanefoamed.Text);
            }

            //22
            if (ChkSinglePieceAnyadditionalcomponent.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                first = txtSinglePieceAnyadditionalcomponentcat1.Text;
                sentence = ChkSinglePieceAnyadditionalcomponent.Text + ": " + first;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceAnyadditionalcomponent.Text, txtSinglePieceAnyadditionalcomponentcat1.Text, "", "", "", "", sentence);
            }
        }
        #endregion

        #region 12.Three Piece Desk

        if (ddlConstype.Text == "Three Piece Desk")
        {
            if (ChkThreePieceWeldedMainBody.Checked == false && ChkThreePieceGlandPlate.Checked == false
                && ChkThreePieceComponentMtgPlate.Checked == false && ChkThreePieceSideCPlate.Checked == false
                && ChkThreePieceDoorCPlate.Checked == false && ChkThreePieceFrontDoor.Checked == false
                && ChkThreePieceRearDoor.Checked == false && ChkThreePieceDoorStiffener.Checked == false
                && ChkThreePieceLock.Checked == false && ChkThreePieceCableSupportAngle.Checked == false
                && ChkThreePieceSlidingdrawerwithtelescopic.Checked == false && ChkThreePieceHorizontalPartition.Checked == false
                && ChkSinglePieceBase.Checked == false && ChkThreePieceLiftingIBolt.Checked == false
                && ChkThreePieceAntivibrationpad.Checked == false && ChkThreePieceTransparentDoor.Checked == false
                && ChkThreePieceCasterWheel.Checked == false && ChkThreePieceFilters.Checked == false
                && ChkSinglePiecefan.Checked == false
                && ChkThreePiecePowderCoatingShade.Checked == false && ChkThreePieceJointlesspolyurethanefoamed.Checked == false
                && ChkThreePieceAnyadditionalcomponent.Checked == false)
            {
                validation = 1;
            }

            action = "insertThreePieceDesk";
            //1
            if (ChkThreePieceWeldedMainBody.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                if (ddlThreePieceWeldedMainBodycat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlThreePieceWeldedMainBodycat1.SelectedItem.Text;
                    sentence += ChkThreePieceWeldedMainBody.Text + " in " + first + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceWeldedMainBody.Text, first, "", "", "", "", sentence);
            }

            //2
            if (ChkThreePieceGlandPlate.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlThreePieceGlandPlatecat1.SelectedItem.Text != "Select")
                {
                    first = ddlThreePieceGlandPlatecat1.SelectedItem.Text;
                    sentence = ChkThreePieceGlandPlate.Text + ": " + first;
                }
                if (ddlThreePieceGlandPlatecat2.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlThreePieceGlandPlatecat2.SelectedItem.Text;
                    sentence += " in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceGlandPlate.Text, first, second, "", "", "", sentence);
            }

            //3
            if (ChkThreePieceComponentMtgPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlThreePieceComponentMtgPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlThreePieceComponentMtgPlatecat1.SelectedItem.Text;
                    sentence = ChkThreePieceComponentMtgPlate.Text + ": " + second;
                }
                if (ddlThreePieceComponentMtgPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlThreePieceComponentMtgPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlThreePieceComponentMtgPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlThreePieceComponentMtgPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceComponentMtgPlate.Text, second, third, fourth, "", "", sentence);
            }

            //4
            if (ChkThreePieceSideCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlThreePieceSideCPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlThreePieceSideCPlatecat1.SelectedItem.Text;
                    sentence = ChkThreePieceSideCPlate.Text + ": " + second;
                }
                if (ddlThreePieceSideCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlThreePieceSideCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlThreePieceSideCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlThreePieceSideCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceSideCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //5
            if (ChkThreePieceDoorCPlate.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlThreePieceDoorCPlatecat1.SelectedItem.Text != "Select")
                {
                    second = ddlThreePieceDoorCPlatecat1.SelectedItem.Text;
                    sentence = ChkThreePieceDoorCPlate.Text + ": " + second;
                }
                if (ddlThreePieceDoorCPlatecat2.SelectedItem.Text != "Select Colour")
                {
                    third = ddlThreePieceDoorCPlatecat2.SelectedItem.Text;
                    sentence += " Colour " + third;
                }
                if (ddlThreePieceDoorCPlatecat3.SelectedItem.Text != "Select Thickness")
                {
                    fourth = ddlThreePieceDoorCPlatecat3.SelectedItem.Text;
                    sentence += " in " + fourth + " thickness ";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceDoorCPlate.Text, second, third, fourth, "", "", sentence);
            }

            //6
            if (ChkThreePieceFrontDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlThreePieceFrontDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlThreePieceFrontDoorcat1.SelectedItem.Text;
                    sentence = ChkThreePieceFrontDoor.Text + ": " + second;
                }
                if (ddlThreePieceFrontDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlThreePieceFrontDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceFrontDoor.Text, second, third, "", "", "", sentence);
            }

            //7
            if (ChkThreePieceRearDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", sentence = "";

                if (ddlThreePieceRearDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlThreePieceRearDoorcat1.SelectedItem.Text;
                    sentence = ChkThreePieceRearDoor.Text + ": " + second;
                }
                if (ddlThreePieceRearDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlThreePieceRearDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceRearDoor.Text, second, third, "", "", "", sentence);
            }

            //8
            if (ChkThreePieceDoorStiffener.Checked == true)
            {
                validation = 0;
                string sentence = "";

                sentence = ChkThreePieceDoorStiffener.Text;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceDoorStiffener.Text, "", "", "", "", "", sentence);
            }

            //9
            if (ChkThreePieceLock.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (ddlThreePieceLockcat1.SelectedItem.Text != "Select")
                {
                    first = ddlThreePieceLockcat1.SelectedItem.Text;
                    sentence = ChkThreePieceLock.Text + ": " + first;
                }
                if (!string.IsNullOrEmpty(txtThreePieceLockcat2.Text))
                {
                    second = txtThreePieceLockcat2.Text;
                    sentence += " Qty " + second + "Nos";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceLock.Text, first, second, "", "", "", sentence);
            }

            //10
            if (ChkThreePieceCableSupportAngle.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (txtThreePieceCableSupportAnglecat1.Text != "")
                {
                    second = txtThreePieceCableSupportAnglecat1.Text;
                    sentence = ChkThreePieceCableSupportAngle.Text + ": Qty " + second + "Nos";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceCableSupportAngle.Text, second, "", "", "", "", sentence);
            }

            //11
            if (ChkThreePieceHorizontalPartition.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlThreePieceHorizontalPartitioncat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlThreePieceHorizontalPartitioncat1.SelectedItem.Text;
                    sentence = ChkThreePieceHorizontalPartition.Text + ": in " + first + " thickness";
                }
                if (ddlThreePieceHorizontalPartitioncat2.SelectedItem.Text != "Select Colour")
                {
                    second = ddlThreePieceHorizontalPartitioncat2.SelectedItem.Text;
                    sentence += " Colour " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceHorizontalPartition.Text, first, second, "", "", "", sentence);
            }

            //12
            //if (ChkThreePieceSlidingdrawerwithtelescopic.Checked == true)
            //{
            //    validation = 0;
            //    string first = "", second = "", sentence = "";
            //    if (ddlThreePieceSlidingdrawerwithtelescopiccat1.SelectedItem.Text != "Select Thickness")
            //    {
            //        first = ddlThreePieceSlidingdrawerwithtelescopiccat1.SelectedItem.Text;
            //        sentence = ChkThreePieceSlidingdrawerwithtelescopic.Text + ": in " + first + " thickness";
            //    }
            //    if (ddlThreePieceSlidingdrawerwithtelescopiccat2.SelectedItem.Text != "Select Colour")
            //    {
            //        second = ddlThreePieceSlidingdrawerwithtelescopiccat2.SelectedItem.Text;
            //        sentence += " Colour " + second;
            //    }
            //    dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceSlidingdrawerwithtelescopic.Text, first, second, "", "", "", sentence);
            //}

            //12
            if (ChkSinglePieceSlidingdrawerwithtelescopicrails.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlSinglePieceSlidingdrawerwithtelescopicrailscat1.SelectedItem.Text != "Select Thickness")
                {
                    first = ddlSinglePieceSlidingdrawerwithtelescopicrailscat1.SelectedItem.Text;
                    sentence = ChkSinglePieceSlidingdrawerwithtelescopicrails.Text + ": in " + first + " thickness";
                }
                if (ddlSinglePieceSlidingdrawerwithtelescopicrailscat2.SelectedItem.Text != "Select Colour")
                {
                    second = ddlSinglePieceSlidingdrawerwithtelescopicrailscat2.SelectedItem.Text;
                    sentence += " Colour " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkSinglePieceSlidingdrawerwithtelescopicrails.Text, first, second, "", "", "", sentence);
            }

            //13
            if (ChkThreePieceLiftingIBolt.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                first = txtThreePieceLiftingIBoltcat1.Text;
                sentence = ChkThreePieceLiftingIBolt.Text + " qty " + first + "nos";

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceLiftingIBolt.Text, first, "", "", "", "", sentence);
            }

            //14
            if (ChkThreePieceBase.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlThreePieceBasecat1.SelectedItem.Text != "Select")
                {
                    second = ddlThreePieceBasecat1.SelectedItem.Text;
                    sentence = ChkThreePieceBase.Text + ": " + second;
                }
                if (ddlThreePieceBasecat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlThreePieceBasecat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlThreePieceBasecat3.SelectedItem.Text != "Select Height")
                {
                    fourth = ddlThreePieceBasecat3.SelectedItem.Text;
                    sentence += " Height " + fourth;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceBase.Text, second, third, fourth, "", "", sentence);
            }

            //15
            if (ChkThreePieceAntivibrationpad.Checked == true)
            {
                validation = 0;
                string second = "", sentence = "";

                if (ddlThreePieceAntivibrationpadcat1.SelectedItem.Text != "Select Thickness")
                {
                    second = ddlThreePieceAntivibrationpadcat1.SelectedItem.Text;
                    sentence = ChkThreePieceAntivibrationpad.Text + ": in " + second + " thickness";
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceAntivibrationpad.Text, second, "", "", "", "", sentence);
            }

            //16
            if (ChkThreePieceTransparentDoor.Checked == true)
            {
                validation = 0;
                string second = "", third = "", fourth = "", sentence = "";

                if (ddlThreePieceTransparentDoorcat1.SelectedItem.Text != "Select")
                {
                    second = ddlThreePieceTransparentDoorcat1.SelectedItem.Text;
                    sentence = ChkThreePieceTransparentDoor.Text + ": " + second;
                }
                if (ddlThreePieceTransparentDoorcat2.SelectedItem.Text != "Select Thickness")
                {
                    third = ddlThreePieceTransparentDoorcat2.SelectedItem.Text;
                    sentence += " in " + third + " thickness ";
                }
                if (ddlThreePieceTransparentDoorcat3.SelectedItem.Text != "Select")
                {
                    fourth = ddlThreePieceTransparentDoorcat3.SelectedItem.Text;
                    sentence += fourth;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceTransparentDoor.Text, second, third, fourth, "", "", sentence);
            }

            //17
            if (ChkThreePieceCasterWheel.Checked == true)
            {
                validation = 0;
                string sentence = "", first = "", second = "", third = "", fourth = "";

                first = txtThreePieceCasterWheelcat1.Text;
                second = txtThreePieceCasterWheelcat2.Text;
                third = txtThreePieceCasterWheelcat3.Text;
                fourth = txtThreePieceCasterWheelcat4.Text;

                sentence = ChkThreePieceCasterWheel.Text + ": in size " + first + " Inch Resolving Qty " + second + " with Fixed Qty " + third + " " + fourth;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceCasterWheel.Text, first, second, third, fourth, "", sentence);
            }

            //18
            if (ChkThreePieceFilters.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlThreePieceFilterscat1.SelectedItem.Text != "Select Inch")
                {
                    first = ddlThreePieceFilterscat1.SelectedItem.Text;
                    sentence = ChkThreePieceFilters.Text + ": " + first + " inch";
                }
                if (txtThreePieceFilterscat2.Text != "")
                {
                    second = txtThreePieceFilterscat2.Text;
                    sentence += " Qty " + second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceFilters.Text, first, second, "", "", "", sentence);
            }
            //19
            if (ChkThreePiecefan.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";
                if (ddlThreePiecefancat1.SelectedItem.Text != "Select Size")
                {
                    first = ddlThreePiecefancat1.SelectedItem.Text;
                    sentence = ChkThreePiecefan.Text + ": " + first + " inch qty: " + txtThreePiecefancat2.Text;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePiecefan.Text, first, txtThreePiecefancat2.Text, "", "", "", sentence);
            }
            //20
            if (ChkThreePiecePowderCoatingShade.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";
                if (ddlThreePiecePowderCoatingShadecat1.SelectedItem.Text != "Select Colour")
                {
                    sentence = ChkThreePiecePowderCoatingShade.Text + ": ";
                    first = ddlThreePiecePowderCoatingShadecat1.SelectedItem.Text;

                    if (ddlThreePiecePowderCoatingShadecat1.SelectedItem.Text != "Specify")
                    {
                        sentence += first;
                    }
                }
                if (txtThreePiecePowderCoatingShadecat2.Text != "")
                {
                    second = txtThreePiecePowderCoatingShadecat2.Text;

                    if (ddlThreePiecePowderCoatingShadecat1.SelectedItem.Text == "Specify")
                    {
                        sentence += second;
                    }
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePiecePowderCoatingShade.Text, first, second, "", "", "", sentence);
            }

            //21
            if (ChkThreePieceJointlesspolyurethanefoamed.Checked == true)
            {
                validation = 0;
                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceJointlesspolyurethanefoamed.Text, "", "", "", "", "", ChkThreePieceJointlesspolyurethanefoamed.Text);
            }

            //22
            if (ChkThreePieceAnyadditionalcomponent.Checked == true)
            {
                validation = 0;
                string first = "", sentence = "";

                first = txtThreePieceAnyadditionalcomponentcat1.Text;
                sentence = ChkThreePieceAnyadditionalcomponent.Text + ": " + first;

                dtConstructionType.Rows.Add(quotationno, quotationid, ChkThreePieceAnyadditionalcomponent.Text, txtThreePieceAnyadditionalcomponentcat1.Text, "", "", "", "", sentence);
            }
        }
        #endregion

        #region 13.Specify

        if (ddlConstype.Text == "Specify")
        {
            if (ChkSpecify1.Checked == false && ChkSpecify2.Checked == false
                && ChkSpecify3.Checked == false && ChkSpecify4.Checked == false
                && ChkSpecify5.Checked == false && ChkSpecify6.Checked == false
                && ChkSpecify7.Checked == false && ChkSpecify8.Checked == false
                && ChkSpecify9.Checked == false && ChkSpecify10.Checked == false)
            {
                validation = 1;
            }

            action = "insertSpecify";
            //1
            if (ChkSpecify1.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify1cat2.Text != "")
                {
                    first = txtspecify1cat2.Text;
                    sentence = txtspecify1cat1.Text + ": " + first + " ";
                }
                if (txtspecify1cat3.Text != "")
                {
                    second = txtspecify1cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify1cat1.Text, first, second, "", "", "", sentence);
            }
            //2
            if (ChkSpecify2.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify2cat2.Text != "")
                {
                    first = txtspecify2cat2.Text;
                    sentence = txtspecify2cat1.Text + ": " + first + " ";
                }
                if (txtspecify2cat3.Text != "")
                {
                    second = txtspecify2cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify2cat1.Text, first, second, "", "", "", sentence);
            }
            //3
            if (ChkSpecify3.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify3cat2.Text != "")
                {
                    first = txtspecify3cat2.Text;
                    sentence = txtspecify3cat1.Text + ": " + first + " ";
                }
                if (txtspecify3cat3.Text != "")
                {
                    second = txtspecify3cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify3cat1.Text, first, second, "", "", "", sentence);
            }
            //4
            if (ChkSpecify4.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify4cat2.Text != "")
                {
                    first = txtspecify4cat2.Text;
                    sentence = txtspecify4cat1.Text + ": " + first + " ";
                }
                if (txtspecify4cat3.Text != "")
                {
                    second = txtspecify4cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify4cat1.Text, first, second, "", "", "", sentence);
            }
            //5
            if (ChkSpecify5.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify5cat2.Text != "")
                {
                    first = txtspecify5cat2.Text;
                    sentence = txtspecify5cat1.Text + ": " + first + " ";
                }
                if (txtspecify5cat3.Text != "")
                {
                    second = txtspecify5cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify5cat1.Text, first, second, "", "", "", sentence);
            }
            //6
            if (ChkSpecify6.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify6cat2.Text != "")
                {
                    first = txtspecify6cat2.Text;
                    sentence = txtspecify6cat1.Text + ": " + first + " ";
                }
                if (txtspecify6cat3.Text != "")
                {
                    second = txtspecify6cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify6cat1.Text, first, second, "", "", "", sentence);
            }
            //7
            if (ChkSpecify7.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify7cat2.Text != "")
                {
                    first = txtspecify7cat2.Text;
                    sentence = txtspecify7cat1.Text + ": " + first + " ";
                }
                if (txtspecify7cat3.Text != "")
                {
                    second = txtspecify7cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify7cat1.Text, first, second, "", "", "", sentence);
            }
            //8
            if (ChkSpecify8.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify8cat2.Text != "")
                {
                    first = txtspecify8cat2.Text;
                    sentence = txtspecify8cat1.Text + ": " + first + " ";
                }
                if (txtspecify8cat3.Text != "")
                {
                    second = txtspecify8cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify8cat1.Text, first, second, "", "", "", sentence);
            }
            //9
            if (ChkSpecify9.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify9cat2.Text != "")
                {
                    first = txtspecify9cat2.Text;
                    sentence = txtspecify9cat1.Text + ": " + first + " ";
                }
                if (txtspecify9cat3.Text != "")
                {
                    second = txtspecify9cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify9cat1.Text, first, second, "", "", "", sentence);
            }
            //10
            if (ChkSpecify10.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify10cat2.Text != "")
                {
                    first = txtspecify10cat2.Text;
                    sentence = txtspecify10cat1.Text + ": " + first + " ";
                }
                if (txtspecify10cat3.Text != "")
                {
                    second = txtspecify10cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify10cat1.Text, first, second, "", "", "", sentence);
            }
            //11
            if (ChkSpecify11.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify11cat2.Text != "")
                {
                    first = txtspecify11cat2.Text;
                    sentence = txtspecify11cat1.Text + ": " + first + " ";
                }
                if (txtspecify11cat3.Text != "")
                {
                    second = txtspecify11cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify11cat1.Text, first, second, "", "", "", sentence);
            }
            //12
            if (ChkSpecify12.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify12cat2.Text != "")
                {
                    first = txtspecify12cat2.Text;
                    sentence = txtspecify12cat1.Text + ": " + first + " ";
                }
                if (txtspecify12cat3.Text != "")
                {
                    second = txtspecify12cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify12cat1.Text, first, second, "", "", "", sentence);
            }
            //13
            if (ChkSpecify13.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify13cat2.Text != "")
                {
                    first = txtspecify13cat2.Text;
                    sentence = txtspecify13cat1.Text + ": " + first + " ";
                }
                if (txtspecify13cat3.Text != "")
                {
                    second = txtspecify13cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify13cat1.Text, first, second, "", "", "", sentence);
            }
            //14
            if (ChkSpecify2.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify14cat2.Text != "")
                {
                    first = txtspecify14cat2.Text;
                    sentence = txtspecify14cat1.Text + ": " + first + " ";
                }
                if (txtspecify14cat3.Text != "")
                {
                    second = txtspecify14cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify14cat1.Text, first, second, "", "", "", sentence);
            }
            //15
            if (ChkSpecify15.Checked == true)
            {
                validation = 0;
                string first = "", second = "", sentence = "";

                if (txtspecify15cat2.Text != "")
                {
                    first = txtspecify15cat2.Text;
                    sentence = txtspecify15cat1.Text + ": " + first + " ";
                }
                if (txtspecify15cat3.Text != "")
                {
                    second = txtspecify15cat3.Text;
                    sentence += second;
                }
                dtConstructionType.Rows.Add(quotationno, quotationid, txtspecify15cat1.Text, first, second, "", "", "", sentence);
            }
        }
        #endregion

        StringBuilder sbdescription = new StringBuilder();
        string material = "";
        if (validation == 0)
        {
            if (ddlmaterial.SelectedItem.Text == "Specify")
            {
                material = txtSpecifymaterial.Text;
            }
            else
            {
                material = ddlmaterial.SelectedItem.Text;
            }

            int sno = 3;
            if (ddlConstype.SelectedItem.Text != "PC ENCLOSURE" && ddlConstype.SelectedItem.Text != "Specify")
            {
                sbdescription.Append("Enclosure For Control Panel. <br>");
                sbdescription.Append("1." + ddlConstype.Text + " Construction in " + material + " Sheet. <br>");
            }
            if (ddlConstype.SelectedItem.Text == "PC ENCLOSURE")
            {
                sbdescription.Append("Enclosure For Control Panel. <br>");
                sbdescription.Append("1." + ddlConstype.Text + " in " + rdlpcenclosure.SelectedItem.Text + "" + " Construction in " + material + " Sheet. <br>");
            }
            if (ddlConstype.SelectedItem.Text == "Specify")
            {
                sbdescription.Append("Enclosure For Control Panel. <br>");
                sbdescription.Append("1." + txtconstructiontype.Text + " Construction in " + material + " Sheet. <br>");
            }

            if (txtbase.Text != "" && txtcanopy.Text != "")
            {
                sbdescription.Append("2. Size= " + txtwidth.Text + "W mm x " + txtdepth.Text + "D mm x " + txtheight.Text + "H mm x " + txtbase.Text + "B mm x " + txtcanopy.Text + "C mm. " + "<br>");
            }
            else
            {
                sbdescription.Append("2. Size= " + txtwidth.Text + "W mm x " + txtdepth.Text + "D mm x " + txtheight.Text + "H mm. " + "<br>");
            }

            for (int i = 0; i < dtConstructionType.Rows.Count; i++)
            {
                if (!string.IsNullOrEmpty(dtConstructionType.Rows[i]["category6"].ToString()) || dtConstructionType.Rows[i]["category6"].ToString() != "")
                {
                    sbdescription.Append((sno) + ". " + dtConstructionType.Rows[i]["category6"].ToString() + "." + "<br>");
                    sno++;
                }
            }

            SqlCommand cmd = new SqlCommand("SP_ConstructionType", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@tbljbbox", dtConstructionType);
            cmd.Parameters.AddWithValue("@action", action);
            cmd.Parameters.AddWithValue("@descriptionall", "");
            cmd.Parameters.AddWithValue("@quotationid", quotationid);
            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();

        }
        else
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", " alert('Please Select atleast one field in construction type');", true);
        }

        return sbdescription.ToString();

    }

    protected void dgvQuatationDtl_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string id = e.CommandArgument.ToString();
        if (e.CommandName == "RowDelete")
        {



            //SqlCommand cmd = new SqlCommand("delete from QuotationData where id='" + id + "'", con);
            //con.Open();
            //cmd.ExecuteNonQuery();
            //con.Close();
            //ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", " alert('Selected Row Deleted Successfully...!');", true);
            //getQutationdts();
            //ScriptManager.RegisterStartupScript(this, this.GetType(), "Success", "scrollToElement();", true);
        }
    }

    protected void dgvQuatationDtl_RowEditing(object sender, GridViewEditEventArgs e)
    {
        dgvQuatationDtl.EditIndex = e.NewEditIndex;
        //this.getQutationdts();
        dgvQuatationDtl.DataSource = (DataTable)ViewState["QuatationData"];
        dgvQuatationDtl.DataBind();
        ScriptManager.RegisterStartupScript(this, this.GetType(), "Success", "scrollToElement();", true);
    }

    protected void OnUpdate(object sender, EventArgs e)
    {
        GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;

        Label id = (Label)row.FindControl("lblid");

        //AjaxControlToolkit.HTMLEditor.Editor rName = (AjaxControlToolkit.HTMLEditor.Editor)row.FindControl("txtDescription") as AjaxControlToolkit.HTMLEditor.Editor;
        //string Description = rName.Content;

        TextBox txtDescr = (TextBox)row.FindControl("txtDescription");
        TextBox txtqty = (TextBox)row.FindControl("txtqty");
        TextBox txtprice = (TextBox)row.FindControl("txtprice");
        Label txttottax = (Label)row.FindControl("lbltottax");
        TextBox txtdis = (TextBox)row.FindControl("txtdiscount");
        Label txtamt = (Label)row.FindControl("lbltotalamt");

        dt.Columns.AddRange(new DataColumn[14] { new DataColumn("id"),
                 new DataColumn("description"), new DataColumn("hsncode")
                , new DataColumn("qty"), new DataColumn("rate"), new DataColumn("CGST")
                , new DataColumn("SGST"), new DataColumn("IGST"), new DataColumn("CGSTamt"), new DataColumn("SGSTamt")
                , new DataColumn("IGSTamt"), new DataColumn("totaltax"),new DataColumn("discount"),new DataColumn("amount")
            });

        DataTable Dt = ViewState["QuatationData"] as DataTable;

        Dt.Rows[row.RowIndex]["description"] = txtDescr.Text.Replace("\r\n", "<br>");
        Dt.Rows[row.RowIndex]["qty"] = txtqty.Text;
        Dt.Rows[row.RowIndex]["rate"] = txtprice.Text;
        Dt.Rows[row.RowIndex]["totaltax"] = txttottax.Text;
        Dt.Rows[row.RowIndex]["discount"] = txtdis.Text;
        Dt.Rows[row.RowIndex]["amount"] = txtamt.Text;

        Dt.AcceptChanges();

        ViewState["QuatationData"] = Dt;
        dgvQuatationDtl.EditIndex = -1;

        dgvQuatationDtl.DataSource = (DataTable)ViewState["QuatationData"];
        dgvQuatationDtl.DataBind();

        //SqlCommand cmdupdate = new SqlCommand("update QuotationData set description='" + Description + "', qty='" + txtqty.Text + "',rate='" + txtprice.Text + "',discount='" + txtdis.Text + "',amount='" + txtamt.Text + "' where id='" + id.Text + "'", con);
        //con.Open();
        //cmdupdate.ExecuteNonQuery();
        //con.Close();

        ScriptManager.RegisterStartupScript(this, this.GetType(), "Success", " alert('Product details Updated successfully !!!');", true);

    }

    protected void OnCancel(object sender, EventArgs e)
    {
        dgvQuatationDtl.EditIndex = -1;
        this.getQutationdts();
    }

    protected void txtqty_TextChanged(object sender, EventArgs e)
    {
        GridViewRow row = (sender as TextBox).NamingContainer as GridViewRow;
        calculationQty(row);
        ScriptManager.RegisterStartupScript(this, this.GetType(), "Success", "scrollToElement();", true);
    }

    private void calculationQty(GridViewRow row)
    {
        TextBox txtqty = (TextBox)row.FindControl("txtqty");
        TextBox txtprice = (TextBox)row.FindControl("txtprice");
        Label txttottax = (Label)row.FindControl("lbltottax");
        TextBox txtdis = (TextBox)row.FindControl("txtdiscount");
        Label txtamt = (Label)row.FindControl("lbltotalamt");

        decimal totamt;
        if (txtqty.Text != "" || txtqty.Text != null || txtqty.Text != "0")
        {
            totamt = Convert.ToDecimal(txtprice.Text.Trim()) * Convert.ToInt32(txtqty.Text);
        }
        else
        {
            totamt = 0;
        }

        decimal discount;
        if (txtdis.Text.Trim() != "" || txtdis.Text != null || txtdis.Text != "0")
        {
            if (txtdis.Text == "")
            {
                discount = 0;
            }
            else
            {
                discount = totamt * Convert.ToInt32(txtdis.Text) / 100;
            }

        }
        else
        {
            discount = 0;
        }

        var netamtwithdis = totamt - discount;

        txtamt.Text = netamtwithdis.ToString();
    }

    protected void txtprice_TextChanged(object sender, EventArgs e)
    {
        GridViewRow row = (sender as TextBox).NamingContainer as GridViewRow;
        calculationQty(row);
        ScriptManager.RegisterStartupScript(this, this.GetType(), "Success", "scrollToElement();", true);
    }

    protected void txtdiscount_TextChanged(object sender, EventArgs e)
    {
        GridViewRow row = (sender as TextBox).NamingContainer as GridViewRow;
        calculationDiscount(row);
        ScriptManager.RegisterStartupScript(this, this.GetType(), "Success", "scrollToElement();", true);
    }

    private void calculationDiscount(GridViewRow row)
    {
        TextBox txtqty = (TextBox)row.FindControl("txtqty");
        TextBox txtprice = (TextBox)row.FindControl("txtprice");
        Label txttottax = (Label)row.FindControl("lbltottax");
        TextBox txtdis = (TextBox)row.FindControl("txtdiscount");
        Label txtamt = (Label)row.FindControl("lbltotalamt");

        decimal totamt;
        if (txtqty.Text != "" || txtqty.Text != null || txtqty.Text != "0")
        {
            totamt = Convert.ToDecimal(txtprice.Text.Trim()) * Convert.ToInt32(txtqty.Text);
        }
        else
        {
            totamt = 0;
        }

        decimal AmtWithDiscount;
        if (txtdis.Text != "" || txtdis.Text != null || txtdis.Text != "0")
        {
            var discount = totamt * Convert.ToDecimal(txtdis.Text.Trim()) / 100;
            AmtWithDiscount = discount;
        }
        else
        {
            AmtWithDiscount = 0;
        }
        var nettotamt = totamt - AmtWithDiscount;
        txtamt.Text = nettotamt.ToString();
    }

    protected void LinkButton1_Click(object sender, EventArgs e)
    {
        GridViewRow row = (sender as LinkButton).NamingContainer as GridViewRow;

        DataTable dt = ViewState["QuatationData"] as DataTable;
        dt.Rows.Remove(dt.Rows[row.RowIndex]);
        ViewState["QuatationData"] = dt;
        dgvQuatationDtl.DataSource = (DataTable)ViewState["QuatationData"];
        dgvQuatationDtl.DataBind();
        ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Selected Data Deleted Succesfully !!!');", true);
    }
    //22-03-2022
    protected void ddltaxation_TextChanged(object sender, EventArgs e)
    {
        if (ddltaxation.SelectedValue == "inmah" || ddltaxation.SelectedValue == "outmah")
        {
            ddlCurrency.SelectedItem.Text = "INR";
        }
        else
        {
            ddlCurrency.SelectedItem.Text = "--Select Currency--";
        }
    }
    protected void ddlpaymentterm_TextChanged(object sender, EventArgs e)
    {
        try
        {
            string pt = ddlpaymentterm.Text;
            if (pt == "Specify")
            {
                txtPTSpecify.Visible = true;
                lblSpecify.Visible = true;
            }
            else
            {
                txtPTSpecify.Visible = false;
                lblSpecify.Visible = false;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected string GenerateCode()
    {
        string FinYear = null;
        string FinFullYear = null;
        if (DateTime.Today.Month > 3)
        {
            FinYear = DateTime.Today.AddYears(1).ToString("yy");
            FinFullYear = DateTime.Today.AddYears(1).ToString("yyyy");
        }
        else
        {
            var finYear = DateTime.Today.AddYears(1).ToString("yy");
            FinYear = (Convert.ToInt32(finYear) - 1).ToString();

            var finfYear = DateTime.Today.AddYears(1).ToString("yyyy");
            FinFullYear = (Convert.ToInt32(finfYear) - 1).ToString();
        }

        string previousyear = (Convert.ToDecimal(FinFullYear) - 1).ToString();
        string strQuotationNumber = "";
        string fY = previousyear.ToString() + "-" + FinYear;
        //string strSelect = @"select ISNULL(MAX(quotationno), '') AS maxno from QuotationMain where quotationno like '%" + fY + "%'";
        string strSelect = @"SELECT CASE WHEN ISNULL(MAX(TRY_CAST(SUBSTRING(quotationno, 9, CASE WHEN CHARINDEX('-0', quotationno) > 0 THEN CHARINDEX('-0', quotationno) - 9 ELSE LEN(quotationno) - 8 END) AS INT)), 0) = 0 THEN '2024-25/01000-0' ELSE '2024-25/' + RIGHT('00000' + CAST(ISNULL(MAX(TRY_CAST(SUBSTRING(quotationno, 9, CASE WHEN CHARINDEX('-0', quotationno) > 0 THEN CHARINDEX('-0', quotationno) - 9 ELSE LEN(quotationno) - 8 END) AS INT)), 0) AS VARCHAR(5)), 5) + '-0' END AS maxno FROM QuotationMain where quotationno like '%" + fY + "%'";

        // string strSelect = @"select TOP 1 quotationno AS maxno from QuotationMain where quotationno like '%" + fY + "%' ORDER BY ID DESC";
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = con;
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = strSelect;
        con.Open();
        string result = cmd.ExecuteScalar().ToString();
        con.Close();

        if (result != "")
        {
            // Find the position of the last "/" and the "-" character in the result string
            int startPos = result.LastIndexOf("/") + 1;
            int endPos = result.IndexOf("-", startPos); // Find the "-" after the "/"

            // Check if both positions are valid
            if (startPos > 0 && endPos > startPos)
            {
                // Extract the sequence number between the "/" and "-"
                string stringBeforeChar = result.Substring(startPos, endPos - startPos);

                // Convert it to an integer
                int numbervalue = Convert.ToInt32(stringBeforeChar);

                // Increment the sequence number by 1
                numbervalue = numbervalue + 1;

                // Format the incremented sequence number to ensure it has 5 digits
                strQuotationNumber = fY + "/" + numbervalue.ToString("D5") + "-" + "0"; // "D5" ensures 5 digits

                // Set the formatted quotation number
                txQutno.Text = strQuotationNumber;
            }
            else
            {
                // Handle unexpected result format (e.g., missing "-" or "/")
                Console.WriteLine("Invalid result format");
            }
        }
        else
        {
            // If no result is provided, start with the first number
            strQuotationNumber = fY + "/" + "00001-0"; // The first ID will be "00001"
            txQutno.Text = strQuotationNumber;
        }




        //if (result != "")
        //{
        //    string stringBeforeChar = result.Substring(8, result.IndexOf("-"));
        //    int numbervalue = Convert.ToInt32(stringBeforeChar);
        //    if (numbervalue <= 9)
        //    {
        //        numbervalue = numbervalue + 1;
        //        strQuotationNumber = fY + "/" + "0000" + numbervalue + "-" + "0";  // if no is less than 9
        //        //strQuotationNumber = fY + "/" + "00" + numbervalue + "-" + "0";  // if no  9
        //        txQutno.Text = strQuotationNumber;
        //    }
        //    else if (numbervalue <= 100)
        //    {
        //        numbervalue = numbervalue + 1;
        //        strQuotationNumber = fY + "/" + "0" + numbervalue + "-" + "0";
        //        txQutno.Text = strQuotationNumber;
        //    }
        //    else if (numbervalue > 100)
        //    {
        //        numbervalue = numbervalue + 1;
        //        strQuotationNumber = fY + "/" + "0" + numbervalue + "-" + "0";
        //        txQutno.Text = strQuotationNumber;
        //    }
        //    //if (numbervalue < 9)
        //    //{
        //    //    numbervalue = numbervalue + 1;
        //    //    strQuotationNumber = fY + "/" + "000" + numbervalue + "-" + "0";
        //    //    txQutno.Text = strQuotationNumber;
        //    //}
        //    else if (numbervalue >= 1000)
        //    {
        //        numbervalue = numbervalue + 1;
        //        //numbervalue = 1001;
        //        strQuotationNumber = fY + "/" + numbervalue + "-" + "0";
        //        txQutno.Text = strQuotationNumber;
        //    }
        //}
        //else
        //{
        //    strQuotationNumber = fY + "/" + "0001-0";
        //    txQutno.Text = strQuotationNumber;
        //}
        return strQuotationNumber;
    }


    // code by Nikhil 20-01-2025
    protected void chkJbweldedmainbody_CheckedChanged(object sender, EventArgs e)
    {
        System.Web.UI.WebControls.CheckBox chkBox = sender as System.Web.UI.WebControls.CheckBox;


        switch (chkBox.ID)
        {
            // 1. JB Box 
            case "chkJbweldedmainbody":
                HandleDropDownListVisibility1(ddlJbweldedmainbodycat1, ddlJbweldedmainbodycat2, null, null, null, RequiredFieldValidatorJB1, RequiredFieldValidatorJB2, null, null, chkBox.Checked);
                break;

            case "chkjbGlandplat":
                HandleDropDownListVisibility1(ddljbGlandplatcat1, ddljbGlandplatcat2, null, null, null, RequiredFieldValidatorJB3, RequiredFieldValidatorJB4, null, null, chkBox.Checked);

                break;

            case "Checckboxfor3DD":
                HandleDropDownListVisibility1(ddljbComponetmtgplt, ddljbComponetmtgpltcat1, ddljbComponetmtgpltcat2, null, txtJbweldedmainbodycat, RequiredFieldValidatorJB5, RequiredFieldValidatorJB6, RequiredFieldValidatorJB7, null, chkBox.Checked);
                break;

            case "chkjbfrontscrewcover":
                HandleDropDownListVisibility1(ddljbfrontscrewcovercat1, null, null, null, null, RequiredFieldValidatorJB8, null, null, null, chkBox.Checked);
                break;

            case "Chkjblock":
                HandleDropDownListVisibility1(ddljbLockcat1, null, null, null, txtQty, RequiredFieldValidatorJB9, null, null, null, chkBox.Checked);
                break;

            case "chkjbTransparentdoor":
                HandleDropDownListVisibility1(ddljbTransparentdoorcat1, ddljbTransparentdoorcat2, ddljbTransparentdoorcat3, null, null, RequiredFieldValidatorJB10, RequiredFieldValidatorJB11, RequiredFieldValidatorJB12, null, chkBox.Checked);
                ddljbTransparentdoorcat.Enabled = chkBox.Checked;
                break;

            case "ChkjbPowercoatingshade":
                HandleDropDownListVisibility1(ddljbPowercoatingshadecat1, null, null, null, null, RequiredFieldValidatorJB13, null, null, null, chkBox.Checked);
                break;

            case "ChkjbFan":
                HandleDropDownListVisibility1(ddljbfancat1, null, null, null, txtjbfanqtycat2, RequiredFieldValidatorJB14, null, null, null, chkBox.Checked);
                break;

            case "ChkjbAnyadditionalcomponent":
                HandleDropDownListVisibility(null, null, null, null, txtjbAnyadditionalcomponentcat1, chkBox.Checked);
                break;


            // End Jb Box


            //  2. WMM-23.5 (AE Box)
            case "chkWMM23WeldedMainBody":
                HandleDropDownListVisibility1(ddlWMM23WeldedMainBodycat1, ddlWMM23WeldedMainBodycat2, null, null, null, RequiredFieldValidatorWM1, RequiredFieldValidatorWM2, null, null, chkBox.Checked);
                break;

            case "ChkWMM23GlandPlate":
                HandleDropDownListVisibility1(ddlWMM23GlandPlatecat1, ddlWMM23GlandPlatecat2, null, null, null, RequiredFieldValidatorWM3, RequiredFieldValidatorWM4, null, null, chkBox.Checked);
                break;

            case "ChkWMM23Canopy":
                HandleDropDownListVisibility1(ddlWMM23Canopycat1, ddlWMM23Canopycat2, null, null, null, RequiredFieldValidatorWM5, RequiredFieldValidatorWM6, null, null, chkBox.Checked);
                break;

            case "ChkWMM23ComponentMtgPlate":
                HandleDropDownListVisibility1(ddlWMM23ComponentMtgPlatecat1, ddlWMM23ComponentMtgPlatecat2, ddlWMM23ComponentMtgPlatecat3, null, null, RequiredFieldValidatorWM7, RequiredFieldValidatorWM8, RequiredFieldValidatorWM9, null, chkBox.Checked);
                break;

            case "ChkWMM23SideCPlate":
                HandleDropDownListVisibility1(ddlWMM23SideCPlatecat1, ddlWMM23SideCPlatecat2, ddlWMM23SideCPlatecat3, null, null, RequiredFieldValidatorWM10, RequiredFieldValidatorWM11, RequiredFieldValidatorWM12, null, chkBox.Checked);
                break;

            case "ChkWMM23DoorCPlate":
                HandleDropDownListVisibility1(ddlWMM23DoorCPlatecat1, ddlWMM23DoorCPlatecat2, ddlWMM23DoorCPlatecat3, null, null, RequiredFieldValidatorWM13, RequiredFieldValidatorWM14, RequiredFieldValidatorWM15, null, chkBox.Checked);
                break;

            case "ChkWMM23WallMtgBracket":
                HandleDropDownListVisibility1(ddlWMM23WallMtgBracketcat1, null, null, null, null, RequiredFieldValidatorWM16, null, null, null, chkBox.Checked);
                break;

            case "ChkWMM23FrontDoor":
                HandleDropDownListVisibility1(ddlWMM23FrontDoorcat1, ddlWMM23FrontDoorcat2, null, null, null, RequiredFieldValidatorWM17, RequiredFieldValidatorWM18, null, null, chkBox.Checked);
                break;

            case "ChkWMM23RearDoor":
                HandleDropDownListVisibility1(ddlWMM23RearDoorcat1, ddlWMM23RearDoorcat2, null, null, null, RequiredFieldValidatorWM19, RequiredFieldValidatorWM20, null, null, chkBox.Checked);
                break;

            case "CheckboxinnerDoor":
                HandleDropDownListVisibility1(ddlinnerdoor, ddlthickness, null, null, null, RequiredFieldValidatorWM21, RequiredFieldValidatorWM22, null, null, chkBox.Checked);
                break;

            case "ChkWMM23Lock":
                HandleDropDownListVisibility1(ddlWMM23Lockcat1, null, null, null, txtWMM23Lockcat2, RequiredFieldValidatorWM23, null, null, null, chkBox.Checked);
                break;

            case "ChkWMM23CableSupportAngle":
                HandleDropDownListVisibility(null, null, null, null, txtWMM23CableSupportAnglecat1, chkBox.Checked);
                break;

            case "ChkWMM23PowerCoatingShade":
                HandleDropDownListVisibility1(ddlWMM23PowerCoatingShadecat1, null, null, null, txtddlWMM23PowerCoatingShadecat2, RequiredFieldValidatorWM24, null, null, null, chkBox.Checked);
                break;

            case "ChkWMM23LiftingIBolt":
                HandleDropDownListVisibility(null, null, null, null, txtWMM23LiftingIBoltcat1, chkBox.Checked);
                break;

            case "ChkWMM23Base":
                HandleDropDownListVisibility1(ddlWMM23Basecat1, ddlWMM23Basecat2, ddlheight, null, txtWMM23Basecat3, RequiredFieldValidatorWM25, RequiredFieldValidatorWM26, RequiredFieldValidatorWM27, null, chkBox.Checked);
                break;

            case "ChkWMM23TransparentDoor":
                HandleDropDownListVisibility1(ddlWMM23TransparentDoorcat1, ddlWMM23TransparentDoorcat2, ddlWMM23TransparentDoorcat3, null, null, RequiredFieldValidatorWM28, RequiredFieldValidatorWM29, RequiredFieldValidatorWM30, null, chkBox.Checked);
                break;

            case "chkWMM23fan":
                HandleDropDownListVisibility1(ddlWMM23fancat1, null, null, null, txtWMM23fancat2, RequiredFieldValidatorWM10, RequiredFieldValidatorWM31, null, null, chkBox.Checked);
                break;

            case "ChkWMM23Anyadditionalcomponent":
                HandleDropDownListVisibility(null, null, null, null, txtWMM23Anyadditionalcomponentcat1, chkBox.Checked);
                break;
            //End  WMM - 23.5(AE Box)


            //3.WMM-30 (MCC Box)
            case "ChkWMM30WeldedMainBody":
                HandleDropDownListVisibility1(ddlWMM30WeldedMainBodycat1, ddlWMM30WeldedMainBodycat2, null, null, null, RequiredFieldValidatorWMM1, RequiredFieldValidatorWMM2, null, null, chkBox.Checked);
                break;

            case "ChkWMM30GlandPlat":
                HandleDropDownListVisibility1(ddlWMM30GlandPlatcat1, ddlWMM30GlandPlatcat2, null, null, null, RequiredFieldValidatorWMM3, RequiredFieldValidatorWMM4, null, null, chkBox.Checked);
                break;

            case "ChkWMM30Canopy":
                HandleDropDownListVisibility1(ddlWMM30Canopycat1, ddlWMM30Canopycat2, null, null, null, RequiredFieldValidatorWMM5, RequiredFieldValidatorWMM6, null, null, chkBox.Checked);
                break;

            case "ChkWMM30ComponentMtgPlate":
                HandleDropDownListVisibility1(ddlWMM30ComponentMtgPlatecat1, ddlWMM30ComponentMtgPlatecat2, ddlWMM30ComponentMtgPlatecat3, null, null, RequiredFieldValidatorWMM7, RequiredFieldValidatorWMM8, RequiredFieldValidatorWMM9, null, chkBox.Checked);
                break;

            case "ChkWMM30SideCPlate":
                HandleDropDownListVisibility1(ddlWMM30SideCPlatecat1, ddlWMM30SideCPlatecat2, ddlWMM30SideCPlatecat3, null, null, RequiredFieldValidatorWMM10, RequiredFieldValidatorWMM11, RequiredFieldValidatorWMM12, null, chkBox.Checked);
                break;

            case "ChkWMM30DoorCPlate":
                HandleDropDownListVisibility1(ddlWMM30DoorCPlatecat1, ddlWMM30DoorCPlatecat2, ddlWMM30DoorCPlatecat3, null, null, RequiredFieldValidatorWMM13, RequiredFieldValidatorWMM14, RequiredFieldValidatorWMM15, null, chkBox.Checked);
                break;

            case "ChkWMM30WallMtgBracket":
                HandleDropDownListVisibility1(ddlWMM30WallMtgBracketcat1, null, null, null, null, RequiredFieldValidatorWMM16, null, null, null, chkBox.Checked);
                break;

            case "ChkWMM30FrontDoor":
                HandleDropDownListVisibility1(ddlWMM30FrontDoorcat1, ddlWMM30FrontDoorcat2, null, null, null, RequiredFieldValidatorWMM17, RequiredFieldValidatorWMM18, null, null, chkBox.Checked);
                break;

            case "ChkWMM30RearDoor":
                HandleDropDownListVisibility1(ddlWMM30RearDoorcat1, ddlWMM30RearDoorcat2, null, null, null, RequiredFieldValidatorWMM19, RequiredFieldValidatorWMM20, null, null, chkBox.Checked);
                break;

            case "checkboxInnerdoorforMCBOX30":
                HandleDropDownListVisibility1(ddlInnerdoor1forMCBOX30, ddlInnerdoorThicknessforMCBOX30, null, null, null, RequiredFieldValidatorWMM21, RequiredFieldValidatorWMM22, null, null, chkBox.Checked);
                break;

            case "ChkWMM30Lock":
                HandleDropDownListVisibility1(ddlWMM30Lockcat1, null, null, null, txtWMM30Lockcat2, RequiredFieldValidatorWMM23, null, null, null, chkBox.Checked);
                break;

            case "ChkWMM30CableSupportAngle":
                HandleDropDownListVisibility(null, null, null, null, txtWMM30CableSupportAnglecat1, chkBox.Checked);
                break;

            case "ChkWMM30LiftingIBolt":
                HandleDropDownListVisibility(null, null, null, null, txtWMM30LiftingIBoltcat1, chkBox.Checked);
                break;

            case "ChkWMM30PowerCoatingShade":
                HandleDropDownListVisibility1(ddlWMM30PowerCoatingShadecat1, null, null, null, txtWMM30PowerCoatingShadecat2, RequiredFieldValidatorWMM24, null, null, null, chkBox.Checked);
                break;

            case "ChkWMM30Base":
                HandleDropDownListVisibility1(ddlWMM30Basecat1, ddlWMM30Basecat2, ddlWMM30Basecat3, null, null, RequiredFieldValidatorWMM25, RequiredFieldValidatorWMM26, RequiredFieldValidatorWMM27, null, chkBox.Checked);
                break;

            case "ChkWMM30TransparentDoor":
                HandleDropDownListVisibility1(ddlWMM30TransparentDoorcat1, ddlWMM30TransparentDoorcat2, ddlWMM30TransparentDoorcat3, null, txtWMM30TransparentDoorcat4, RequiredFieldValidatorWMM28, RequiredFieldValidatorWMM29, RequiredFieldValidatorWMM30, null, chkBox.Checked);
                break;

            case "ChkWMM30fan":
                HandleDropDownListVisibility1(ddlWMM30fancat1, null, null, null, txtWMM30fancat2, RequiredFieldValidatorWMM31, null, null, null, chkBox.Checked);
                break;

            case "ChkWMM30Anyadditionalcomponent":
                HandleDropDownListVisibility(null, null, null, null, txtWMM30Anyadditionalcomponentcat1, chkBox.Checked);
                break;
            //End WMM-30 (MCC Box)

            // 4.MFS (Modular Floor Standing Enclosure)
            case "ChkMFSMainframeStructureWelded":
                HandleDropDownListVisibility1(ddlMFSMainframeStructureWeldedcat1, null, null, null, null, RequiredFieldValidatorMFS1, null, null, null, chkBox.Checked);
                break;

            case "ChkMFSBottomCover":
                HandleDropDownListVisibility1(ddlMFSBottomCovercat1, ddlMFSBottomCovercat2, null, null, null, RequiredFieldValidatorMFS2, RequiredFieldValidatorMFS3, null, null, chkBox.Checked);
                break;

            case "ChkMFSGlandPlate":
                HandleDropDownListVisibility1(ddlMFSGlandPlatecat1, ddlMFSGlandPlatecat2, ddlMFSGlandPlatecat3, null, null, RequiredFieldValidatorMFS4, RequiredFieldValidatorMFS5, RequiredFieldValidatorMFS6, null, chkBox.Checked);
                break;

            case "ChkMFSComponentMtgPlate":
                HandleDropDownListVisibility1(ddlMFSComponentMtgPlatecat1, ddlMFSComponentMtgPlatecat2, ddlMFSComponentMtgPlatecat3, null, txtMFSComponentMtgPlatecat4, RequiredFieldValidatorMFS7, RequiredFieldValidatorMFS8, RequiredFieldValidatorMFS9, null, chkBox.Checked);
                break;

            case "ChkMFSSideCPlate":
                HandleDropDownListVisibility(ddlMFSSideCPlatecat1, ddlMFSSideCPlatecat2, ddlMFSSideCPlatecat3, null, txtMFSSideCPlatecat4, chkBox.Checked);
                break;

            case "ChkMFSDoorCPlate":
                HandleDropDownListVisibility(ddlMFSDoorCPlatecat1, ddlMFSDoorCPlatecat2, ddlMFSDoorCPlatecat3, null, txtMFSDoorCPlatecat4, chkBox.Checked);
                break;

            case "ChkMFSPartialMountingPlate":
                HandleDropDownListVisibility(ddlMFSPartialMountingPlatecat1, ddlMFSPartialMountingPlatecat2, ddlMFSPartialMountingPlatecat3, null, txtMFSPartialMountingPlatecat4, chkBox.Checked);
                break;

            case "ChkMFSFillerTray":
                HandleDropDownListVisibility(ddlMFSFillerTraycat1, ddlMFSFillerTraycat2, ddlMFSFillerTraycat3, null, txtMFSFillerTraycat4, chkBox.Checked);
                break;

            case "ChkMFSFrontDoor":
                HandleDropDownListVisibility(ddlMFSFrontDoorcat1, ddlMFSFrontDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkMFSRearDoor":
                HandleDropDownListVisibility(ddlMFSRearDoorcat1, ddlMFSRearDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkMFSLock":
                HandleDropDownListVisibility(ddlMFSLockcat1, null, null, null, txtmsfqty, chkBox.Checked);
                break;

            case "ChkMFSRearCover":
                HandleDropDownListVisibility(ddlMFSRearCovercat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkMFSSideCover":
                HandleDropDownListVisibility(ddlMFSSideCovercat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkMFSTopCover":
                HandleDropDownListVisibility(ddlMFSTopCovercat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkMFSPowerCoatingShade":
                HandleDropDownListVisibility(ddlMFSPowerCoatingShadecat1, null, null, null, txtMFSPowerCoatingShadecat2, chkBox.Checked);
                break;

            case "ChkMFSLiftingArrangement":
                HandleDropDownListVisibility(ddlMFSLiftingArrangementcat1, ddlMFSLiftingArrangementcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkMFSBase":
                HandleDropDownListVisibility(ddlMFSBasecat1, ddlMFSBasecat2, ddlMFSBasecat3, ddllistforshitMfs, null, chkBox.Checked);
                break;

            case "ChkWMM23TransparentDoorMFS":
                HandleDropDownListVisibility(ddlWMM23TransparentDoorcat1MFS1, ddlWMM23TransparentDoorcat2MFS, ddlWMM23TransparentDoorcat3MFS, null, null, chkBox.Checked);
                break;

            case "ChkMFSAntivibrationpad":
                HandleDropDownListVisibility(ddlMFSAntivibrationpadcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkMFSDrawingPocket":
                HandleDropDownListVisibility(ddlMFSDrawingPocketcat1, ddlMFSDrawingPocketcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkMFSCanopy":
                HandleDropDownListVisibility(ddlMFSCanopycat1, ddlMFSCanopycat2, ddlMFSCanopycat3, null, null, chkBox.Checked);
                break;

            case "ChkMFSfan":
                HandleDropDownListVisibility(ddlMFSfancat1, null, null, null, txtMFSfancat2, chkBox.Checked);
                break;

            case "ChkMFSAnyadditionalcomponent":
                HandleDropDownListVisibility(null, null, null, null, txtMFSAnyadditionalcomponentcat1, chkBox.Checked);
                break;
            // End MFS (Modular Floor Standing Enclosure)

            //5.Eco MCC 30mm
            case "ChkEcoMCCMainframeStructureWelded":
                HandleDropDownListVisibility(ddlEcoMCCMainframeStructureWeldedcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCGlandPlate":
                HandleDropDownListVisibility(ddlEcoMCCGlandPlatecat1, ddlEcoMCCGlandPlatecat2, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCComponentMtgPlate":
                HandleDropDownListVisibility(ddlEcoMCCComponentMtgPlatecat1, ddlEcoMCCComponentMtgPlatecat2, ddlEcoMCCComponentMtgPlatecat3, null, txtEcoMCCComponentMtgPlatecat4, chkBox.Checked);
                break;

            case "ChkEcoMCCSideCPlate":
                HandleDropDownListVisibility(ddlEcoMCCSideCPlatecat1, ddlEcoMCCSideCPlatecat2, ddlEcoMCCSideCPlatecat3, null, txtEcoMCCSideCPlatecat4, chkBox.Checked);
                break;

            case "ChkEcoMCCDoorCPlate":
                HandleDropDownListVisibility(ddlEcoMCCDoorCPlatecat1, ddlEcoMCCDoorCPlatecat2, ddlEcoMCCDoorCPlatecat3, null, txtEcoMCCDoorCPlatecat4, chkBox.Checked);
                break;

            case "ChkEcoMCCFrontDoor":
                HandleDropDownListVisibility(ddlEcoMCCFrontDoorca1, ddlEcoMCCFrontDoorca2, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCRearDoor":
                HandleDropDownListVisibility(ddlEcoMCCRearDoorcat1, ddlEcoMCCRearDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "checkboxinnerdoorEcomc":
                HandleDropDownListVisibility(ddlmat, ddlthicknessforinnerdoor, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCRearCover":
                HandleDropDownListVisibility(ddlEcoMCCRearCovercat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCSideCover":
                HandleDropDownListVisibility(ddlEcoMCCSideCovercat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCLock":
                HandleDropDownListVisibility(ddlEcoMCCLockcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCPowerCoatingShade":
                HandleDropDownListVisibility(ddlEcoMCCPowerCoatingShadecat1, null, null, null, txtEcoMCCPowerCoatingShadecat2, chkBox.Checked);
                break;

            case "ChkEcoMCCLiftingArrangement":
                HandleDropDownListVisibility(ddlEcoMCCLiftingArrangementcat1, ddlEcoMCCLiftingArrangementcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCBase":
                HandleDropDownListVisibility(ddlEcoMCCBasecat1, ddlEcoMCCBasecat2, ddlEcoMCCBasecat3, ddlEcoMCCbaseshits, null, chkBox.Checked);
                break;

            case "ChkWMM23TransparentDoorEcoframe":
                HandleDropDownListVisibility(ddlWMM23TransparentDoorcat1Ecoframe, ddlWMM23TransparentDoorcat2Ecoframe, ddlWMM23TransparentDoorcat3Ecoframe, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCAntivibrationpad":
                HandleDropDownListVisibility(ddlEcoMCCAntivibrationpadcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCDrawingPocket":
                HandleDropDownListVisibility(ddlEcoMCCDrawingPocketcat1, ddlEcoMCCDrawingPocketcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCCanopy":
                HandleDropDownListVisibility(ddlEcoMCCCanopycat1, ddlEcoMCCCanopycat2, ddlEcoMCCCanopycat3, null, null, chkBox.Checked);
                break;

            case "ChkEcoMCCfan":
                HandleDropDownListVisibility(ddlEcoMCCfancat1, null, null, null, txtEcoMCCfancat2, chkBox.Checked);
                break;

            case "ChkEcoMCCAnyadditionalcomponent":
                HandleDropDownListVisibility(null, null, null, null, txtEcoMCCAnyadditionalcomponentcat1, chkBox.Checked);
                break;
            // End Eco MCC 30mm

            // 6. Modular W-Big 43 mm
            case "ChkModularWeldedMainBody":
                HandleDropDownListVisibility(ddlModularWeldedMainBodycat1, ddlModularWeldedMainBodycat2, null, null, null, chkBox.Checked);
                break;

            case "ChkModularGlandPlate":
                HandleDropDownListVisibility(ddlModularGlandPlatecat1, ddlModularGlandPlatecat2, null, null, null, chkBox.Checked);
                break;

            case "ChkModularComponentMtgPlate":
                HandleDropDownListVisibility(ddlModularComponentMtgPlatecat1, ddlModularComponentMtgPlatecat2, ddlModularComponentMtgPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkModularSideCPlate":
                HandleDropDownListVisibility(ddlModularSideCPlatecat1, ddlModularSideCPlatecat2, ddlModularSideCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkModularDoorCPlate":
                HandleDropDownListVisibility(ddlModularDoorCPlatecat1, ddlModularDoorCPlatecat2, ddlModularDoorCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkModularWallMtgBracket":
                HandleDropDownListVisibility(ddlModularWallMtgBracketcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkModularFrontDoor":
                HandleDropDownListVisibility(ddlModularFrontDoorcat1, ddlModularFrontDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkModularRearDoor":
                HandleDropDownListVisibility(ddlModularRearDoorcat1, ddlthickmodularecord, null, null, null, chkBox.Checked);
                break;

            case "ChkModularRearCover":
                HandleDropDownListVisibility(ddlModularRearCovercat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkModularCableSupportAngle":
                HandleDropDownListVisibility(null, null, null, null, txtModularCableSupportAnglecat1, chkBox.Checked);
                break;

            case "ChkModularLock":
                HandleDropDownListVisibility(ddlModularLockcat1, null, null, null, txtlockQty, chkBox.Checked);
                break;

            case "ChkModularPowerCoatingShade":
                HandleDropDownListVisibility(ddlModularPowerCoatingShadecat1, ddlModularPowerCoatingShadecat2, null, null, txtModularPowerCoatingShadecat3, chkBox.Checked);
                break;

            case "ChkModularLiftingArrangement":
                HandleDropDownListVisibility(ddlModularLiftingArrangementcat1, ddlModularLiftingArrangementcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkModularBase":
                HandleDropDownListVisibility(ddlModularBasecat1, ddlModularBasecat2, ddltype, null, null, chkBox.Checked);
                break;

            case "ChkWMM23TransparentDoorModular":
                HandleDropDownListVisibility(ddlWMM23TransparentDoorcat1modular, ddlWMM23TransparentDoorcat2Modualr, ddlWMM23TransparentDoorcat3modular, null, null, chkBox.Checked);
                break;

            case "ChkModularAntivibrationpad":
                HandleDropDownListVisibility(ddlModularAntivibrationpadcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkModularCanopy":
                HandleDropDownListVisibility(ddlModularCanopycat1, ddlModularCanopycat2, ddlModularCanopycat3, null, null, chkBox.Checked);
                break;

            case "ChkModularfan":
                HandleDropDownListVisibility(ddlModularfancat1, null, null, null, txtModularfancat2, chkBox.Checked);
                break;

            case "ChkModularAnyadditionalcomponent":
                HandleDropDownListVisibility(null, null, null, null, txtModularAnyadditionalcomponentcat1, chkBox.Checked);
                break;
            //End  Modular W-Big 43 mm

            // 7.Eco Frame 43mm

            case "ChkEcoFrameMainFrameTopBottomWeldedStructure":
                HandleDropDownListVisibility(ddlEcoFrameMainFrameTopBottomWeldedStructurecat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoFrameTopBottomGlandPlate":
                HandleDropDownListVisibility(ddlEcoFrameTopBottomGlandPlatecat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoFrameComponentMtgPlate":
                HandleDropDownListVisibility(ddlEcoFrameComponentMtgPlatecat1, ddlEcoFrameComponentMtgPlatecat2, ddlEcoFrameComponentMtgPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkEcoFrameSideCPlate":
                HandleDropDownListVisibility(ddlEcoFrameSideCPlatecat1, ddlEcoFrameSideCPlatecat2, ddlEcoFrameSideCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkEcoFrameDoorCPlate":
                HandleDropDownListVisibility(ddlEcoFrameDoorCPlatecat1, ddlEcoFrameDoorCPlatecat2, ddlEcoFrameDoorCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkEcoFrameFrontDoor":
                HandleDropDownListVisibility(ddlEcoFrameFrontDoorcat1, ddlEcoFrameFrontDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoFrameRearDoor":
                HandleDropDownListVisibility(ddlEcoFrameRearDoorcat1, ddlthicknessecoframe, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoFrameRearCover":
                HandleDropDownListVisibility(ddlEcoFrameRearCovercat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoFrameSideCover":
                HandleDropDownListVisibility(ddlEcoFrameSideCovercat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoFrameLock":
                HandleDropDownListVisibility(ddlEcoFrameLockcat1, null, null, null, txtecoframelockqty, chkBox.Checked);
                break;

            case "ChkEcoFramePowerCoatingShade":
                HandleDropDownListVisibility(ddlEcoFramePowerCoatingShadecat1, null, null, null, txtddlEcoFramePowerCoatingShadecat2, chkBox.Checked);
                break;

            case "ChkEcoFrameLiftingArrangement":
                HandleDropDownListVisibility(ddlEcoFrameLiftingArrangementcat1, ddlEcoFrameLiftingArrangementcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoFrameBase":
                HandleDropDownListVisibility(ddlEcoFrameBasecat1, ddlEcoFrameBasecat2, ddlecoframebasethickness, null, null, chkBox.Checked);
                break;

            case "checkfibration":
                HandleDropDownListVisibility(ddlfibration, null, null, null, null, chkBox.Checked);
                break;

            case "ChkEcoFrameCanopy":
                HandleDropDownListVisibility(ddlEcoFrameCanopycat1, ddlEcoFrameCanopycat2, ddlEcoFrameCanopycat3, null, null, chkBox.Checked);
                break;

            case "ChkEcoFramefan":
                HandleDropDownListVisibility(ddlEcoFramefancat1, null, null, null, txtEcoFramefancat2, chkBox.Checked);
                break;

            case "ChkEcoFrameAnyadditionalcomponent":
                HandleDropDownListVisibility(null, null, null, null, txtEcoFrameAnyadditionalcomponentcat1, chkBox.Checked);
                break;
            // End Eco Frame 43mm

            // 8.PC ENCLOSURE
            //1st form Shop Floor PC Enclosure Standing
            case "ChkPCEncShopFloorStandingMainframestructure":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingMainframestructurecat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingBottomCover":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingBottomCovercat1, ddlPCEncShopFloorStandingBottomCovercat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingComponentMtgPlate":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingComponentMtgPlatecat3, ddlPCEncShopFloorStandingComponentMtgPlatecat4, ddlPCEncShopFloorStandingComponentMtgPlatecat5, null, null, chkBox.Checked);
                bool check = chkBox.Checked;
                btnAddStandingComponentMtgPlate1.Enabled = check;
                txtPCEncShopFloorStandingComponentMtgPlatecat1.Enabled = check;
                txtPCEncShopFloorStandingComponentMtgPlatecat2.Enabled = check;
                if (check != true)
                {
                    componetmtgplat2.Visible = false;
                    componetmtgplat3.Visible = false;
                    componetmtgplat4.Visible = false;
                }
                break;

            case "ChkPCEncShopFloorStandingSidecPlate":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingSidecPlatecat1, ddlPCEncShopFloorStandingSidecPlatecat2, ddlPCEncShopFloorStandingSidecPlatecat3, null, null, chkBox.Checked);
                bool check2 = chkBox.Checked;
                btnPCEncShopFloorStandingSidecPlate1.Enabled = check2;
                if (check2 != true)
                {
                    SidecPlate2.Visible = false;
                    SidecPlate3.Visible = false;
                    SidecPlate4.Visible = false;
                }
                break;

            case "ChkPCEncShopFloorStandingDoorCPlate":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingDoorCPlatecat1, ddlPCEncShopFloorStandingDoorCPlatecat2, ddlPCEncShopFloorStandingDoorCPlatecat3, null, null, chkBox.Checked);
                bool check3 = chkBox.Checked;
                btnPCEncShopFloorStandingDoorCPlate1.Enabled = check3;
                if (check3 != true)
                {
                    DoorcPlate2.Visible = false;
                    DoorcPlate3.Visible = false;
                    DoorcPlate4.Visible = false;
                }
                break;

            case "ChkPCEncShopFloorStandingFrontDoor":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingFrontDoorcat1, ddlPCEncShopFloorStandingFrontDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingRearDoor":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingRearDoorcat1, ddlPCEncShopFloorStandingRearDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingLock":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingLockcat1, null, null, null, txtPCEncShopFloorStandingLockcat2, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingRearCover":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingRearCovercat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingSideCover":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingSideCovercat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingTopCover":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingTopCovercat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingHorizontalPartition":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingHorizontalPartitioncat1, ddlPCEncShopFloorStandingHorizontalPartitioncat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingSlidingKeyboarddrawer":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingSlidingKeyboarddrawercat1, ddlPCEncShopFloorStandingSlidingKeyboarddrawercat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingPowderCoatingShade":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingPowderCoatingShadecat1, null, null, null, txtPCEncShopFloorStandingPowderCoatingShadecat2, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingLiftingArrangement":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingLiftingArrangementcat1, ddlPCEncShopFloorStandingLiftingArrangementcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingBase":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingBasecat1, ddlPCEncShopFloorStandingBasecat2, null, null, txtddlPCEncShopFloorStandingBasecat3, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingAntivibrationpad":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingAntivibrationpadcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingAntivibrationCasterWheel":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check4 = chkBox.Checked;
                txtPCEncShopFloorStandingAntivibrationCasterWheelcat1.Enabled = check4;
                txtPCEncShopFloorStandingAntivibrationCasterWheelcat2.Enabled = check4;
                txtPCEncShopFloorStandingAntivibrationCasterWheelcat3.Enabled = check4;
                txtPCEncShopFloorStandingAntivibrationCasterWheelcat4.Enabled = check4;

                if (check4 != true)
                {
                    txtPCEncShopFloorStandingAntivibrationCasterWheelcat1.Text = "";
                    txtPCEncShopFloorStandingAntivibrationCasterWheelcat2.Text = "";
                    txtPCEncShopFloorStandingAntivibrationCasterWheelcat3.Text = "";
                    txtPCEncShopFloorStandingAntivibrationCasterWheelcat4.Text = "";
                }
                break;

            case "ChkPCEncShopFloorStandingDrawingPocket":
                HandleDropDownListVisibility(ddlChkPCEncShopFloorStandingDrawingPocketcat1, ddlChkPCEncShopFloorStandingDrawingPocketcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingTransparentDoor":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingTransparentDoorcat1, ddlPCEncShopFloorStandingTransparentDoorcat2, ddlPCEncShopFloorStandingTransparentDoorcat4, null, null, chkBox.Checked);
                bool check5 = chkBox.Checked;
                if (check5 != true)
                {
                    ddlPCEncShopFloorStandingTransparentDoorcat2.SelectedIndex = 0;
                    txtPCEncShopFloorStandingTransparentDoorcat3.Visible = false;
                }
                break;

            case "ChkPCEncShopFloorStandingFilters":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingFilterscat1, null, null, null, txtqtyenclouser, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingfan":
                HandleDropDownListVisibility(ddlPCEncShopFloorStandingfancat1, null, null, null, txtChkPCEncShopFloorStandingfancat2, chkBox.Checked);
                break;

            case "ChkPCEncShopFloorStandingAnyadditional":
                HandleDropDownListVisibility(null, null, null, null, txtPCEncShopFloorStandingAnyadditionalcat1, chkBox.Checked);
                break;
            //End 1st form 

            // 2nd form PC Enclosure ECO-Standing AND PC Enclosure ECO-Sitting

            case "ChkPCEnclosureECOStandingWeldedMainBody":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingWeldedMainBodycat1, ddlPCEnclosureECOStandingWeldedMainBodycat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingGlandPlate":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingGlandPlatecat1, ddlPCEnclosureECOStandingGlandPlatecat3, null, null, txtPCEnclosureECOStandingGlandPlatecat2, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingComponentMtgPlate":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingComponentMtgPlatecat2, ddlPCEnclosureECOStandingComponentMtgPlatecat3, ddlPCEnclosureECOStandingComponentMtgPlatecat4, null, txtPCEnclosureECOStandingComponentMtgPlatecat1, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingSideCPlate":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingSideCPlatecat2, ddlPCEnclosureECOStandingSideCPlatecat3, ddlPCEnclosureECOStandingSideCPlatecat4, null, txtPCEnclosureECOStandingSideCPlatecat1, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingDoorCPlate":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingDoorCPlatecat2, ddlPCEnclosureECOStandingDoorCPlatecat3, ddlPCEnclosureECOStandingDoorCPlatecat4, null, txtPCEnclosureECOStandingDoorCPlatecat1, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingFrontDoorwithstiffeners":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingFrontDoorwithstiffenerscat1, ddlPCEnclosureECOStandingFrontDoorwithstiffenerscat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingCableSupportAngle":
                HandleDropDownListVisibility(null, null, null, null, txtPCEnclosureECOStandingCableSupportAnglecat1, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingLock":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingLockcat1, null, null, null, txtPCEnclosureECOStandingLockcat2, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingHorizontalPartition":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingHorizontalPartitioncat1, ddlPCEnclosureECOStandingHorizontalPartitioncat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingSlidingKeyboarddrawer":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingSlidingKeyboarddrawercat1, ddlPCEnclosureECOStandingSlidingKeyboarddrawercat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingLiftingIBolt":
                HandleDropDownListVisibility(null, null, null, null, txtPCEnclosureECOStandingLiftingIBoltcat1, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingBase":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingBasecat1, ddlPCEnclosureECOStandingBasecat2, ddlPCEnclosureECOStandingBasecat3, null, txtPCEnclosureECOStandingBasecat4, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingAntivibrationpad":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingAntivibrationpadcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingTransparentDoor":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingTransparentDoorcat1, ddlPCEnclosureECOStandingTransparentDoor2, ddlPCEnclosureECOStandingTransparentDoor3, null, null, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingCasterWheel":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check6 = chkBox.Checked;
                txtPCEnclosureECOStandingCasterWheelcat1.Enabled = check6;
                txtPCEnclosureECOStandingCasterWheelcat2.Enabled = check6;
                txtPCEnclosureECOStandingCasterWheelcat3.Enabled = check6;
                txtPCEnclosureECOStandingCasterWheelcat4.Enabled = check6;

                if (check6 != true)
                {
                    txtPCEnclosureECOStandingCasterWheelcat1.Text = "";
                    txtPCEnclosureECOStandingCasterWheelcat2.Text = "";
                    txtPCEnclosureECOStandingCasterWheelcat3.Text = "";
                    txtPCEnclosureECOStandingCasterWheelcat4.Text = "";
                }
                break;

            case "ChkPCEnclosureECOStandingFilters":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingFilterscat1, null, null, null, txtPCEnclosureECOStandingFilterscat2, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingPowderCoating":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingPowderCoatingcat1, null, null, null, txtPCEnclosureECOStandingPowderCoatingcat2, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingfan":
                HandleDropDownListVisibility(ddlPCEnclosureECOStandingfancat1, null, null, null, txtPCEnclosureECOStandingfancat2, chkBox.Checked);
                break;

            case "ChkPCEnclosureECOStandingAnyadditional":
                HandleDropDownListVisibility(null, null, null, null, txtPCEnclosureECOStandingAnyadditionalcat1, chkBox.Checked);
                break;
            // End 2nd form
            //End PC ENCLOSURE

            // 9.PC TABLE

            case "ChkPcTableWeldedMainbody":
                HandleDropDownListVisibility(ddlPcTableWeldedMainbodycat1, ddlPcTableWeldedMainbodycat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPcTableGlandPlate":
                HandleDropDownListVisibility(ddlPcTableGlandPlatecat1, ddlPcTableGlandPlatecat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPcTableComponentMtgPlate":
                HandleDropDownListVisibility(ddlPcTableComponentMtgPlatecat1, ddlPcTableComponentMtgPlatecat2, ddlPcTableComponentMtgPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkPcTableSideCPlate":
                HandleDropDownListVisibility(ddlPcTableSideCPlatecat1, ddlPcTableSideCPlatecat2, ddlPcTableSideCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkPcTableDoorCPlate":
                HandleDropDownListVisibility(ddlPcTableDoorCPlatecat1, ddlPcTableDoorCPlatecat2, ddlPcTableDoorCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkPcTableFrontDoor":
                HandleDropDownListVisibility(ddlPcTableFrontDoorcat1, ddlPcTableFrontDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPcTableRearDoor":
                HandleDropDownListVisibility(ddlPcTableRearDoorcat1, ddlPcTableRearDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPcTableLock":
                HandleDropDownListVisibility(ddlPcTableLockcat1, null, null, null, txtPcTableLockcat2, chkBox.Checked);
                break;

            case "ChkPcTableCableSupportAngle":
                HandleDropDownListVisibility(null, null, null, null, txtPcTableCableSupportAnglecat1, chkBox.Checked);
                break;

            case "ChkPcTableHorizontalPartition":
                HandleDropDownListVisibility(ddlPcTableHorizontalPartitioncat1, ddlPcTableHorizontalPartitioncat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPcTableSlidingKeyboarddrawer":
                HandleDropDownListVisibility(ddlPcTableSlidingKeyboarddrawercat1, ddlPcTableSlidingKeyboarddrawercat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPcTableSlidingCPUdrawer":
                HandleDropDownListVisibility(ddlPcTableSlidingCPUdrawercat1, ddlPcTableSlidingCPUdrawercat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPcTableMonitomountingbracket":
                HandleDropDownListVisibility(ddlPcTableMonitomountingbracketcat1, ddlPcTableMonitomountingbracketcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPcTableLiftingIBolt":
                HandleDropDownListVisibility(null, null, null, null, txtPcTableLiftingIBoltcat1, chkBox.Checked);
                break;

            case "ChkPcTableBase":
                HandleDropDownListVisibility(ddlPcTableBasecat1, ddlPcTableBasecat2, ddlPcTableBasecat3, null, null, chkBox.Checked);
                break;

            case "ChkPcTableAntivibrationpad":
                HandleDropDownListVisibility(ddlPcTableAntivibrationpadcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkPcTableTransparentDoor":
                HandleDropDownListVisibility(ddlPcTableTransparentDoorcat1, ddlPcTableTransparentDoorcat2, ddlPcTableTransparentDoorcat3, null, null, chkBox.Checked);
                break;

            case "ChkPcTableCasterWheel":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check7 = chkBox.Checked;
                txtPcTableCasterWheelcat1.Enabled = check7;
                txtPcTableCasterWheelcat2.Enabled = check7;
                txtPcTableCasterWheelcat3.Enabled = check7;
                txtPcTableCasterWheelcat4.Enabled = check7;

                if (check7 != true)
                {
                    txtPcTableCasterWheelcat1.Text = "";
                    txtPcTableCasterWheelcat2.Text = "";
                    txtPcTableCasterWheelcat3.Text = "";
                    txtPcTableCasterWheelcat4.Text = "";
                }
                break;

            case "ChkPcTableFilters":
                HandleDropDownListVisibility(ddlPcTableFilterscat1, null, null, null, txtPcTableFilterscat2, chkBox.Checked);
                break;

            case "ChkPcTablePowderCoatingShade":
                HandleDropDownListVisibility(ddlPcTablePowderCoatingShadecat1, null, null, null, txtPcTablePowderCoatingShadecat2, chkBox.Checked);
                break;

            case "ChkPcTablefan":
                HandleDropDownListVisibility(ddlPcTablefancat1, null, null, null, txtPcTablefancat2, chkBox.Checked);
                break;

            case "ChkPcTableAnyadditionalcomponent":
                HandleDropDownListVisibility(null, null, null, null, txtPcTableAnyadditionalcomponentcat1, chkBox.Checked);
                break;
            //End PC TABLE

            // 10. PRINTER TABLE 
            case "ChkPrinterTableWeldedMainBody":
                HandleDropDownListVisibility(ddlPrinterTableWeldedMainBodycat1, ddlPrinterTableWeldedMainBodycat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPrinterGlandPlate":
                HandleDropDownListVisibility(ddlPrinterGlandPlatecat1, ddlPrinterGlandPlatecat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPrinterTableComponentMtgPlate":
                HandleDropDownListVisibility(ddlPrinterTableComponentMtgPlatecat1, ddlPrinterTableComponentMtgPlatecat2, ddlPrinterTableComponentMtgPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkPrinterTableSideCPlate":
                HandleDropDownListVisibility(ddlPrinterTableSideCPlatecat1, ddlPrinterTableSideCPlatecat2, ddlPrinterTableSideCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkPrinterTableDoorCPlate":
                HandleDropDownListVisibility(ddlPrinterTableDoorCPlatecat1, ddlPrinterTableDoorCPlatecat2, ddlPrinterTableDoorCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkPrinterTableFrontDoor":
                HandleDropDownListVisibility(ddlPrinterTableFrontDoorcat1, ddlPrinterTableFrontDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPrinterTableRearDoor":
                HandleDropDownListVisibility(ddlPrinterTableRearDoorcat1, ddlPrinterTableRearDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPrinterTableLock":
                HandleDropDownListVisibility(ddlPrinterTableLockcat1, null, null, null, txtPrinterTableLockcat2, chkBox.Checked);
                break;

            case "ChkPrinterTableCableSupportAngle":
                HandleDropDownListVisibility(null, null, null, null, txtPrinterTableCableSupportAnglecat1, chkBox.Checked);
                break;

            case "ChkPrinterTableHorizontalPartition":
                HandleDropDownListVisibility(ddlPrinterTableHorizontalPartitioncat1, ddlPrinterTableHorizontalPartitioncat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPrinterTableSlidingdrawer":
                HandleDropDownListVisibility(ddlPrinterTableSlidingdrawercat1, ddlPrinterTableSlidingdrawercat2, null, null, null, chkBox.Checked);
                break;

            case "ChkPrinterTableLiftingIBolt":
                HandleDropDownListVisibility(null, null, null, null, txtPrinterTableLiftingIBoltcat1, chkBox.Checked);
                break;

            case "ChkPrinterTableBase":
                HandleDropDownListVisibility(ddlPrinterTableBasecat1, ddlPrinterTableBasecat2, ddlPrinterTableBasecat3, null, null, chkBox.Checked);
                break;

            case "ChkPrinterTableAntivibrationpad":
                HandleDropDownListVisibility(ddlPrinterTableAntivibrationpadcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkPrinterTableTransparentDoor":
                HandleDropDownListVisibility(ddlPrinterTableTransparentDoorcat1, ddlPrinterTableTransparentDoorcat2, ddlPrinterTableTransparentDoorcat3, null, null, chkBox.Checked);
                break;

            case "ChkPrinterTableCasterWheel":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check8 = chkBox.Checked;
                txtPrinterTableCasterWheelcat1.Enabled = check8;
                txtPrinterTableCasterWheelcat2.Enabled = check8;
                txtPrinterTableCasterWheelcat3.Enabled = check8;
                txtPrinterTableCasterWheelcat4.Enabled = check8;

                if (check8 != true)
                {
                    txtPrinterTableCasterWheelcat1.Text = "";
                    txtPrinterTableCasterWheelcat2.Text = "";
                    txtPrinterTableCasterWheelcat3.Text = "";
                    txtPrinterTableCasterWheelcat4.Text = "";
                }
                break;

            case "ChkPrinterTableFilters":
                HandleDropDownListVisibility(ddlPrinterTableFilterscat1, null, null, null, txtPrinterTableFilterscat2, chkBox.Checked);
                break;

            case "ChkPrinterTablePowderCoatingShade":
                HandleDropDownListVisibility(ddlPrinterTablePowderCoatingShadecat1, null, null, null, txtPrinterTablePowderCoatingShadecat2, chkBox.Checked);
                break;

            case "ChkPrinterTablefan":
                HandleDropDownListVisibility(ddlPrinterTablefancat1, null, null, null, txtPrinterTablefancat2, chkBox.Checked);
                break;

            case "ChkPrinterTableAnyadditionalcomponent":
                HandleDropDownListVisibility(null, null, null, null, txtPrinterTableAnyadditionalcomponentcat1, chkBox.Checked);
                break;
            // End Printer table

            // 11. Single Piece Desk
            case "ChkSinglePieceWeldedMainBody":
                HandleDropDownListVisibility(ddlSinglePieceWeldedMainBodycat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceGlandPlate":
                HandleDropDownListVisibility(ddlSinglePieceGlandPlatecat1, ddlSinglePieceGlandPlatecat2, null, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceComponentMtgPlate":
                HandleDropDownListVisibility(ddlSinglePieceComponentMtgPlatecat1, ddlSinglePieceComponentMtgPlatecat2, ddlSinglePieceComponentMtgPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceSideCPlate":
                HandleDropDownListVisibility(ddlSinglePieceSideCPlatecat1, ddlSinglePieceSideCPlatecat2, ddlSinglePieceSideCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceDoorCPlate":
                HandleDropDownListVisibility(ddlSinglePieceDoorCPlatecat1, ddlSinglePieceDoorCPlatecat2, ddlSinglePieceDoorCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceFrontDoor":
                HandleDropDownListVisibility(ddlSinglePieceFrontDoorcat1, ddlSinglePieceFrontDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceRearDoor":
                HandleDropDownListVisibility(ddlSinglePieceRearDoorcat1, ddlSinglePieceRearDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceLock":
                HandleDropDownListVisibility(ddlSinglePieceLockcat1, null, null, null, txtSinglePieceLockcat2, chkBox.Checked);
                break;

            case "ChkSinglePieceCableSupportAngle":
                HandleDropDownListVisibility(null, null, null, null, txtSinglePieceCableSupportAnglecat1, chkBox.Checked);
                break;

            case "ChkSinglePieceSlidingKeyboarddrawertelescopicrails":
                HandleDropDownListVisibility(ddlSinglePieceSlidingKeyboarddrawertelescopicrailscat1, ddlSinglePieceSlidingKeyboarddrawertelescopicrailscat2, null, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceSlidingdrawerwithtelescopicrails":
                HandleDropDownListVisibility(ddlSinglePieceSlidingdrawerwithtelescopicrailscat1, ddlSinglePieceSlidingdrawerwithtelescopicrailscat2, null, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceMonitormountingarrangement":
                HandleDropDownListVisibility(ddlSinglePieceMonitormountingarrangementcat1, ddlSinglePieceMonitormountingarrangementcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceBase":
                HandleDropDownListVisibility(ddlSinglePieceBasecat1, ddlSinglePieceBasecat2, ddlSinglePieceBasecat3, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceAntivibrationpad":
                HandleDropDownListVisibility(ddlSinglePieceAntivibrationpadcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceTransparentDoor":
                HandleDropDownListVisibility(ddlSinglePieceTransparentDoorcat1, ddlSinglePieceTransparentDoorcat2, ddlSinglePieceTransparentDoorcat3, null, null, chkBox.Checked);
                break;

            case "ChkSinglePieceCasterWheel":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check9 = chkBox.Checked;
                txtPrinterTableCasterWheelcat1.Enabled = check9;
                txtPrinterTableCasterWheelcat2.Enabled = check9;
                txtPrinterTableCasterWheelcat3.Enabled = check9;
                txtPrinterTableCasterWheelcat4.Enabled = check9;

                if (check9 != true)
                {
                    txtPrinterTableCasterWheelcat1.Text = "";
                    txtPrinterTableCasterWheelcat2.Text = "";
                    txtPrinterTableCasterWheelcat3.Text = "";
                    txtPrinterTableCasterWheelcat4.Text = "";
                }
                break;

            case "ChkSinglePieceFilters":
                HandleDropDownListVisibility(ddlSinglePieceFilterscat1, null, null, null, txtSinglePieceFilterscat2, chkBox.Checked);
                break;

            case "ChkSinglePiecefan":
                HandleDropDownListVisibility(ddlSinglePiecefancat1, null, null, null, txtSinglePiecefancat2, chkBox.Checked);
                break;

            case "ChkSinglePiecePowderCoatingShade":
                HandleDropDownListVisibility(ddlSinglePiecePowderCoatingShadecat1, null, null, null, txtSinglePiecePowderCoatingShadecat2, chkBox.Checked);
                break;

            case "ChkSinglePieceAnyadditionalcomponent":
                HandleDropDownListVisibility(null, null, null, null, txtSinglePieceAnyadditionalcomponentcat1, chkBox.Checked);
                break;
            // End Single Piece Desk

            // 12.Three Piece Desk
            case "ChkThreePieceWeldedMainBody":
                HandleDropDownListVisibility(ddlThreePieceWeldedMainBodycat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceGlandPlate":
                HandleDropDownListVisibility(ddlThreePieceGlandPlatecat1, ddlThreePieceGlandPlatecat2, null, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceComponentMtgPlate":
                HandleDropDownListVisibility(ddlThreePieceComponentMtgPlatecat1, ddlThreePieceComponentMtgPlatecat2, ddlThreePieceComponentMtgPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceSideCPlate":
                HandleDropDownListVisibility(ddlThreePieceSideCPlatecat1, ddlThreePieceSideCPlatecat2, ddlThreePieceSideCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceDoorCPlate":
                HandleDropDownListVisibility(ddlThreePieceDoorCPlatecat1, ddlThreePieceDoorCPlatecat2, ddlThreePieceDoorCPlatecat3, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceFrontDoor":
                HandleDropDownListVisibility(ddlThreePieceFrontDoorcat1, ddlThreePieceFrontDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceRearDoor":
                HandleDropDownListVisibility(ddlThreePieceRearDoorcat1, ddlThreePieceRearDoorcat2, null, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceLock":
                HandleDropDownListVisibility(ddlThreePieceLockcat1, null, null, null, txtThreePieceLockcat2, chkBox.Checked);
                break;

            case "ChkThreePieceCableSupportAngle":
                HandleDropDownListVisibility(null, null, null, null, txtThreePieceCableSupportAnglecat1, chkBox.Checked);
                break;

            case "ChkThreePieceHorizontalPartition":
                HandleDropDownListVisibility(ddlThreePieceHorizontalPartitioncat1, ddlThreePieceHorizontalPartitioncat2, null, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceSlidingdrawerwithtelescopic":
                HandleDropDownListVisibility(ddlThreePieceSlidingdrawerwithtelescopiccat1, ddlThreePieceSlidingdrawerwithtelescopiccat2, null, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceLiftingIBolt":
                HandleDropDownListVisibility(null, null, null, null, txtThreePieceLiftingIBoltcat1, chkBox.Checked);
                break;

            case "ChkThreePieceBase":
                HandleDropDownListVisibility(ddlThreePieceBasecat1, ddlThreePieceBasecat2, ddlThreePieceBasecat3, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceAntivibrationpad":
                HandleDropDownListVisibility(ddlThreePieceAntivibrationpadcat1, null, null, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceTransparentDoor":
                HandleDropDownListVisibility(ddlThreePieceTransparentDoorcat1, ddlThreePieceTransparentDoorcat2, ddlThreePieceTransparentDoorcat3, null, null, chkBox.Checked);
                break;

            case "ChkThreePieceCasterWheel":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check10 = chkBox.Checked;
                txtThreePieceCasterWheelcat1.Enabled = check10;
                txtThreePieceCasterWheelcat2.Enabled = check10;
                txtThreePieceCasterWheelcat3.Enabled = check10;
                txtThreePieceCasterWheelcat4.Enabled = check10;

                if (check10 != true)
                {
                    txtThreePieceCasterWheelcat1.Text = "";
                    txtThreePieceCasterWheelcat2.Text = "";
                    txtThreePieceCasterWheelcat3.Text = "";
                    txtThreePieceCasterWheelcat4.Text = "";
                }
                break;

            case "ChkThreePieceFilters":
                HandleDropDownListVisibility(ddlThreePieceFilterscat1, null, null, null, txtThreePieceFilterscat2, chkBox.Checked);
                break;

            case "ChkThreePiecefan":
                HandleDropDownListVisibility(ddlThreePiecefancat1, null, null, null, txtThreePiecefancat2, chkBox.Checked);
                break;

            case "ChkThreePiecePowderCoatingShade":
                HandleDropDownListVisibility(ddlThreePiecePowderCoatingShadecat1, null, null, null, txtThreePiecePowderCoatingShadecat2, chkBox.Checked);
                break;

            case "ChkThreePieceAnyadditionalcomponent":
                HandleDropDownListVisibility(null, null, null, null, txtThreePieceAnyadditionalcomponentcat1, chkBox.Checked);
                break;
            //End Three Piece Desk

            // 13. Specify
            case "ChkSpecify1":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check11 = chkBox.Checked;
                txtspecify1cat1.Enabled = check11;
                txtspecify1cat2.Enabled = check11;
                txtspecify1cat3.Enabled = check11;

                if (check11 != true)
                {
                    txtspecify1cat1.Text = "";
                    txtspecify1cat2.Text = "";
                    txtspecify1cat3.Text = "";
                }
                break;

            case "ChkSpecify2":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check12 = chkBox.Checked;
                txtspecify2cat1.Enabled = check12;
                txtspecify2cat2.Enabled = check12;
                txtspecify2cat3.Enabled = check12;

                if (check12 != true)
                {
                    txtspecify2cat1.Text = "";
                    txtspecify2cat2.Text = "";
                    txtspecify2cat3.Text = "";
                }
                break;

            case "ChkSpecify3":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check13 = chkBox.Checked;
                txtspecify3cat1.Enabled = check13;
                txtspecify3cat2.Enabled = check13;
                txtspecify3cat3.Enabled = check13;

                if (check13 != true)
                {
                    txtspecify3cat1.Text = "";
                    txtspecify3cat2.Text = "";
                    txtspecify3cat3.Text = "";
                }
                break;

            case "ChkSpecify4":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check14 = chkBox.Checked;
                txtspecify4cat1.Enabled = check14;
                txtspecify4cat2.Enabled = check14;
                txtspecify4cat3.Enabled = check14;

                if (check14 != true)
                {
                    txtspecify4cat1.Text = "";
                    txtspecify4cat2.Text = "";
                    txtspecify4cat3.Text = "";
                }
                break;

            case "ChkSpecify5":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check15 = chkBox.Checked;
                txtspecify5cat1.Enabled = check15;
                txtspecify5cat2.Enabled = check15;
                txtspecify5cat3.Enabled = check15;

                if (check15 != true)
                {
                    txtspecify5cat1.Text = "";
                    txtspecify5cat2.Text = "";
                    txtspecify5cat3.Text = "";
                }
                break;

            case "ChkSpecify6":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check16 = chkBox.Checked;
                txtspecify6cat1.Enabled = check16;
                txtspecify6cat2.Enabled = check16;
                txtspecify6cat3.Enabled = check16;

                if (check16 != true)
                {
                    txtspecify6cat1.Text = "";
                    txtspecify6cat2.Text = "";
                    txtspecify6cat3.Text = "";
                }
                break;

            case "ChkSpecify7":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check17 = chkBox.Checked;
                txtspecify7cat1.Enabled = check17;
                txtspecify7cat2.Enabled = check17;
                txtspecify7cat3.Enabled = check17;

                if (check17 != true)
                {
                    txtspecify7cat1.Text = "";
                    txtspecify7cat2.Text = "";
                    txtspecify7cat3.Text = "";
                }
                break;

            case "ChkSpecify8":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check18 = chkBox.Checked;
                txtspecify8cat1.Enabled = check18;
                txtspecify8cat2.Enabled = check18;
                txtspecify8cat3.Enabled = check18;

                if (check18 != true)
                {
                    txtspecify7cat1.Text = "";
                    txtspecify7cat2.Text = "";
                    txtspecify7cat3.Text = "";
                }
                break;

            case "ChkSpecify9":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check19 = chkBox.Checked;
                txtspecify9cat1.Enabled = check19;
                txtspecify9cat2.Enabled = check19;
                txtspecify9cat3.Enabled = check19;

                if (check19 != true)
                {
                    txtspecify9cat1.Text = "";
                    txtspecify9cat2.Text = "";
                    txtspecify9cat3.Text = "";
                }
                break;

            case "ChkSpecify10":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check20 = chkBox.Checked;
                txtspecify10cat1.Enabled = check20;
                txtspecify10cat2.Enabled = check20;
                txtspecify10cat3.Enabled = check20;

                if (check20 != true)
                {
                    txtspecify10cat1.Text = "";
                    txtspecify10cat2.Text = "";
                    txtspecify10cat3.Text = "";
                }
                break;

            case "ChkSpecify11":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check21 = chkBox.Checked;
                txtspecify11cat1.Enabled = check21;
                txtspecify11cat2.Enabled = check21;
                txtspecify11cat3.Enabled = check21;

                if (check21 != true)
                {
                    txtspecify11cat1.Text = "";
                    txtspecify11cat2.Text = "";
                    txtspecify11cat3.Text = "";
                }
                break;

            case "ChkSpecify12":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check22 = chkBox.Checked;
                txtspecify12cat1.Enabled = check22;
                txtspecify12cat2.Enabled = check22;
                txtspecify12cat3.Enabled = check22;

                if (check22 != true)
                {
                    txtspecify12cat1.Text = "";
                    txtspecify12cat2.Text = "";
                    txtspecify12cat3.Text = "";
                }
                break;

            case "ChkSpecify13":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check23 = chkBox.Checked;
                txtspecify13cat1.Enabled = check23;
                txtspecify13cat2.Enabled = check23;
                txtspecify13cat3.Enabled = check23;

                if (check23 != true)
                {
                    txtspecify13cat1.Text = "";
                    txtspecify13cat2.Text = "";
                    txtspecify13cat3.Text = "";
                }
                break;

            case "ChkSpecify14":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check24 = chkBox.Checked;
                txtspecify14cat1.Enabled = check24;
                txtspecify14cat2.Enabled = check24;
                txtspecify14cat3.Enabled = check24;

                if (check24 != true)
                {
                    txtspecify14cat1.Text = "";
                    txtspecify14cat2.Text = "";
                    txtspecify14cat3.Text = "";
                }
                break;

            case "ChkSpecify15":
                HandleDropDownListVisibility(null, null, null, null, null, chkBox.Checked);
                bool check25 = chkBox.Checked;
                txtspecify15cat1.Enabled = check25;
                txtspecify15cat2.Enabled = check25;
                txtspecify15cat3.Enabled = check25;

                if (check25 != true)
                {
                    txtspecify15cat1.Text = "";
                    txtspecify15cat2.Text = "";
                    txtspecify15cat3.Text = "";
                }
                break;
                // End Specify



        }

        this.modelprofile.Show();

    }


    // Helper method to enable/disable dropdown lists based on checkbox checked state
    private void HandleDropDownListVisibility(DropDownList ddl1, DropDownList ddl2, DropDownList ddl3, DropDownList ddl4, TextBox ttb1, bool isChecked)
    {
        if (ddl1 != null)
        {
            ddl1.Enabled = isChecked;
            ddl1.SelectedIndex = 0;

        }
        if (ddl2 != null)
        {
            ddl2.Enabled = isChecked;
            ddl2.SelectedIndex = 0;
        }
        if (ddl3 != null)
        {
            ddl3.Enabled = isChecked;
            ddl3.SelectedIndex = 0;
        }
        if (ddl4 != null)
        {
            ddl4.Enabled = isChecked;
            ddl4.SelectedIndex = 0;
        }
        if (ttb1 != null)
        {
            ttb1.Enabled = isChecked;
            ttb1.Text = "";
        }

    }

    private void HandleDropDownListVisibility1(DropDownList ddl1, DropDownList ddl2, DropDownList ddl3, DropDownList ddl4, TextBox ttb1, RequiredFieldValidator req1, RequiredFieldValidator req2, RequiredFieldValidator req3, RequiredFieldValidator req4, bool isChecked)
    {
        if (ddl1 != null)
        {
            ddl1.Enabled = isChecked;
            ddl1.SelectedIndex = 0;
        }
        if (ddl2 != null)
        {
            ddl2.Enabled = isChecked;
            ddl2.SelectedIndex = 0;
        }
        if (ddl3 != null)
        {
            ddl3.Enabled = isChecked;
            ddl3.SelectedIndex = 0;
        }
        if (ddl4 != null)
        {
            ddl4.Enabled = isChecked;
            ddl4.SelectedIndex = 0;
        }
        if (ttb1 != null)
        {
            ttb1.Enabled = isChecked;
            ttb1.Text = "";
        }
        if (req1 != null)
        {
            req1.Enabled = isChecked;
        }
        if (req2 != null)
        {
            req2.Enabled = isChecked;
        }
        if (req3 != null)
        {
            req3.Enabled = isChecked;
        }
        if (req4 != null)
        {
            req4.Enabled = isChecked;
        }

    }

}