using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Admin_RegisterReportPurchase : System.Web.UI.Page
{
    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
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

                btn.Visible = false;
            }
        }
    }


    protected void btnresetfilter_Click(object sender, EventArgs e)
    {
        Response.Redirect("RegisterReportPurchase.aspx");
    }

    [System.Web.Script.Services.ScriptMethod()]
    [System.Web.Services.WebMethod]
    public static List<string> GetSupplierList(string prefixText, int count)
    {
        return AutoFillSupplierlist(prefixText);
    }

    public static List<string> AutoFillSupplierlist(string prefixText)
    {
        using (SqlConnection con = new SqlConnection())
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

            using (SqlCommand com = new SqlCommand())
            {

                com.CommandText = "select DISTINCT SupplierName from excelenclive.tblSupplierMaster where " + "SupplierName like @Search + '%' and IsActive=1";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> sname = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        sname.Add(sdr["SupplierName"].ToString());
                    }
                }
                con.Close();
                return sname;
            }
        }
    }

    int count = 0;
    public override void VerifyRenderingInServerForm(Control control)
    {
        //required to avoid the runtime error "  
        //Control 'GridView1' of type 'GridView' must be placed inside a form tag with runat=server."  
    }
    
    protected void btnsearch_Click(object sender, EventArgs e)
    {
        try
        {
            bindPurchaseRegisterReportData();

        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private static DataTable GetData(string SP, string PartyName, string FromDate, string ToDate, string Type)
    {
        string strConnString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        using (SqlConnection con = new SqlConnection(strConnString))
        {
            using (SqlCommand cmd = new SqlCommand(SP, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Type", Type);
                // if (PartyName == "")
                // cmd.Parameters.AddWithValue("@PartyName", DBNull.Value);
                // else
                cmd.Parameters.AddWithValue("@PartyName", PartyName);

                if (FromDate == "")
                    cmd.Parameters.AddWithValue("@FromDate", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@FromDate", FromDate);
                if (ToDate == "")
                    cmd.Parameters.AddWithValue("@ToDate", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@ToDate", ToDate);
                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    sda.SelectCommand = cmd;
                    using (DataSet ds = new DataSet())
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);

                        //sda.Fill(ds);
                        return dt;
                    }
                }
            }
        }
    }

    private static DataTable GetHSNSummaryData(string SP, string PartyName, string FromDate, string ToDate, string Type)
    {
        string strConnString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        using (SqlConnection con = new SqlConnection(strConnString))
        {
            using (SqlCommand cmd = new SqlCommand(SP, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Type", Type);
                if (PartyName == "")
                    cmd.Parameters.AddWithValue("@PartyName", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@PartyName", PartyName);

                if (FromDate == "")
                    cmd.Parameters.AddWithValue("@FromDate", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@FromDate", FromDate);
                if (ToDate == "")
                    cmd.Parameters.AddWithValue("@ToDate", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@ToDate", ToDate);
                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    sda.SelectCommand = cmd;
                    using (DataSet ds = new DataSet())
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        //sda.Fill(ds);
                        return dt;
                    }
                }
            }
        }
    }

    protected void ExportToExcel(object sender, EventArgs e)
    {
       
        string fileneme = "PURCHASE_Register " + DateTime.Now.ToShortDateString();
        DataTable dt = new DataTable("GridView_Data");
        DataTable dthsnsummary = new DataTable("GridView_DataHSN");
        //DataTable dtHSN = GetHSNSummaryData("SP_HSNRegisterReport", txtPartyName.Text, txtfromdate.Text, txttodate.Text, ddltype.Text);
        DataTable dtHSN = GetHSNSummaryData("ExcelEncLive.SP_RegisterReportPurchase", txtPartyName.Text, txtfromdate.Text, txttodate.Text, "HSNPurchase");
        double BasicAmount = 0;
        double GSTTotAmount = 0;
        double GrandAmount = 0;

        double HSNBasicAmt = 0;
        double HSNCGST = 0;
        double HSNSGST = 0;
        double HSNIGST = 0;
        double HSNGtot = 0;
        using (SqlDataAdapter sda = new SqlDataAdapter())
        {
            foreach (TableCell cell in dgvPurchaseRegisterReport.HeaderRow.Cells)
            {
                dt.Columns.Add(cell.Text);
            }
            foreach (GridViewRow row in dgvPurchaseRegisterReport.Rows)
            {
                Label lblGSTNumber = (Label)row.FindControl("lblGSTNumber");
                Label lblStatus = (Label)row.FindControl("lblStatus");
                Label lblType = (Label)row.FindControl("lblType");

                Label lblCGST = (Label)row.FindControl("lblCGST");
                Label lblSGST = (Label)row.FindControl("lblSGST");
                Label lblIGST = (Label)row.FindControl("lblIGST");
                Label lblGSTAmount = (Label)row.FindControl("lblGSTAmount");
                Label lblGrandTotal = (Label)row.FindControl("lblGrandTotal");
                Label lblBasicTotal = (Label)row.FindControl("lblBasicTotal");

                Label lblInvoiceNo = (Label)row.FindControl("lblVchNo");
                Label lblQty = (Label)row.FindControl("lblQty");

                string Party = row.Cells[0].Text.Replace("&amp;", "&");
                string Type = lblType.Text;
                string GSTNumber = lblGSTNumber.Text;
                string VoucherType = row.Cells[3].Text;
                string Date = row.Cells[4].Text;
                string RefNo = row.Cells[5].Text;
                string DocNo = lblInvoiceNo.Text;//row.Cells[6].Text;
                                                 //string RefDate = row.Cells[7].Text;
                string TCS = row.Cells[7].Text;
                string Qty = lblQty.Text; //row.Cells[9].Text;
                string BasicTotal = lblBasicTotal.Text;
                string CGST = lblCGST.Text;
                string SGST = lblSGST.Text;
                string IGST = lblIGST.Text;
                string GSTAmount = lblGSTAmount.Text;
                string GrandTotal = lblGrandTotal.Text;
                string Status = lblStatus.Text;
                dt.Rows.Add(Party, Type, GSTNumber, VoucherType, Date, RefNo, DocNo, TCS, Qty, BasicTotal, CGST, SGST, IGST, GSTAmount, GrandTotal, Status);

                BasicAmount += Convert.ToDouble(BasicTotal);
                GSTTotAmount += Convert.ToDouble(GSTAmount);
                GrandAmount += Convert.ToDouble(GrandTotal);
            }
            dt.Rows.Add("", "", "", "", "", "", "", "", "TOTAL", Math.Round(BasicAmount).ToString(), "", "", "", GSTTotAmount, Math.Round(GrandAmount).ToString(), "");


            // Add the columns dynamically to the DataTable
            foreach (TableCell cell in dgvHSNSummaryPurchase.HeaderRow.Cells)
            {
                dthsnsummary.Columns.Add(cell.Text);
            }

            // Add a 'Grand Total' column
           // dthsnsummary.Columns.Add("Grand Total");

            // Loop through each row in the GridView
            foreach (GridViewRow row in dgvHSNSummaryPurchase.Rows)
            {
                // Find the controls for CGST, SGST, and IGST
                Label qty = (Label)row.FindControl("lblQty");
                Label hsn = (Label)row.FindControl("lblHSN");
                Label uom = (Label)row.FindControl("lblUOM");
                Label lblCGSTAmt = (Label)row.FindControl("lblCGSTAmt");
                Label lblSGSTAmt = (Label)row.FindControl("lblSGSTAmt");
                Label lblIGSTAmt = (Label)row.FindControl("lblIGSTAmt");
                Label basictotal1 = (Label)row.FindControl("lblBasicTotal");

                // Ensure values are parsed safely, in case any label is empty
                double cgstAmt = string.IsNullOrEmpty(lblCGSTAmt.Text) ? 0 : Convert.ToDouble(lblCGSTAmt.Text);
                double sgstAmt = string.IsNullOrEmpty(lblSGSTAmt.Text) ? 0 : Convert.ToDouble(lblSGSTAmt.Text);
                double igstAmt = string.IsNullOrEmpty(lblIGSTAmt.Text) ? 0 : Convert.ToDouble(lblIGSTAmt.Text);
                double basictotal = string.IsNullOrEmpty(basictotal1.Text) ? 0 : Convert.ToDouble(basictotal1.Text);
                double qtyt = string.IsNullOrEmpty(qty.Text) ? 0 : Convert.ToDouble(qty.Text);               
                double hsnt = string.IsNullOrEmpty(hsn.Text) ? 0 : Convert.ToDouble(hsn.Text);
              
                

                // Add the row to the DataTable
                dthsnsummary.Rows.Add(qtyt, basictotal, hsnt, uom.Text, cgstAmt.ToString(), sgstAmt.ToString(), igstAmt.ToString());

                // Update the summary totals
                HSNBasicAmt += basictotal;
                HSNCGST += cgstAmt;
                HSNSGST += sgstAmt;
                HSNIGST += igstAmt;
             
            }

            // Add the summary row for totals
            dthsnsummary.Rows.Add("TOTAL", Math.Round(HSNBasicAmt).ToString(), "", "", Math.Round(HSNCGST).ToString(), Math.Round(HSNSGST).ToString(), Math.Round(HSNIGST).ToString());

        }
        using (DataSet ds = new DataSet())
        {
            ds.Tables.Add(dt);
            ds.Tables.Add(dthsnsummary);

            ds.Tables[0].TableName = "Purchase_Summary";
            ds.Tables[1].TableName = "HSN_Summary";

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(ds);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename= " + fileneme + ".xlsx");
                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

    }

    #region Purchase 

    protected void bindPurchaseRegisterReportData()
    {
        DataTable dt = GetData("ExcelEncLive.SP_RegisterReportPurchase", txtPartyName.Text, txtfromdate.Text, txttodate.Text, "PURCHASE");
        DataTable dtHSN = GetHSNSummaryData("ExcelEncLive.SP_RegisterReportPurchase", txtPartyName.Text, txtfromdate.Text, txttodate.Text, "HSNPurchase");

        if (dt.Rows.Count > 0)
        {
            btn.Visible = true;
        }
        else
        {
            btn.Visible = false;
        }
        dgvPurchaseRegisterReport.DataSource = dt;
        dgvPurchaseRegisterReport.DataBind();
        dgvPurchaseRegisterReport.EmptyDataText = "Record Not Found";

        dgvHSNSummaryPurchase.DataSource = dtHSN;
        dgvHSNSummaryPurchase.DataBind();
        dgvHSNSummaryPurchase.EmptyDataText = "Record Not Found";
        ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeaderPurchase('" + dgvPurchaseRegisterReport.ClientID + "',500, 1020 , 40 ,true); </script>", false);
    }

    protected void dgvPurchaseRegisterReport_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            string ID = dgvPurchaseRegisterReport.DataKeys[e.Row.RowIndex].Values[0].ToString();
            con.Open();

            string sname = e.Row.Cells[0].Text.Replace("&amp;", "&");

            SqlCommand cmd = new SqlCommand("select GSTNo from ExcelEncLive.tblSupplierMaster where SupplierName='" + sname + "' and IsActive=1", con);
            string gstno = cmd.ExecuteScalar() == null ? "" : cmd.ExecuteScalar().ToString();

            SqlCommand cmdbasic = new SqlCommand("SELECT SUM(CAST(Amount as float)) FROM ExcelEncLive.tblPurchaseBillDtls where HeaderID='" + ID + "'", con);
            string basicamt = cmdbasic.ExecuteScalar() == null ? "0" : cmdbasic.ExecuteScalar().ToString();

            SqlCommand cmdcgst = new SqlCommand(" SELECT top 1 CGSTPer FROM ExcelEncLive.tblPurchaseBillDtls where HeaderID='" + ID + "'", con);
            string cgst = cmdcgst.ExecuteScalar() == null ? "0" : cmdcgst.ExecuteScalar().ToString();

            SqlCommand cmdsgst = new SqlCommand(" SELECT top 1 SGSTPer FROM ExcelEncLive.tblPurchaseBillDtls where HeaderID='" + ID + "'", con);
            string sgst = cmdsgst.ExecuteScalar() == null ? "0" : cmdsgst.ExecuteScalar().ToString();

            SqlCommand cmdigst = new SqlCommand(" SELECT top 1 IGSTPer FROM ExcelEncLive.tblPurchaseBillDtls where HeaderID='" + ID + "'", con);
            string igst = cmdigst.ExecuteScalar() == null ? "0" : cmdigst.ExecuteScalar().ToString();

            SqlCommand cmdqty = new SqlCommand("SELECT SUM(CAST(Qty as float)) as Qty FROM ExcelEncLive.tblPurchaseBillDtls where HeaderID='" + ID + "'", con);
            string Qty = cmdqty.ExecuteScalar().ToString();

            var Cgstamt = Convert.ToDouble(basicamt) * Convert.ToDouble(cgst == "" ? "0" : cgst) / 100;
            var Sgstamt = Convert.ToDouble(basicamt) * Convert.ToDouble(sgst == "" ? "0" : sgst) / 100;
            var Igstamt = Convert.ToDouble(basicamt) * Convert.ToDouble(igst == "" ? "0" : igst) / 100;

            string GSTAmount = (Cgstamt + Sgstamt + Igstamt).ToString();

            con.Close();
            Label lblCGST = (Label)e.Row.FindControl("lblCGST");
            lblCGST.Text = cgst;

            Label lblSGST = (Label)e.Row.FindControl("lblSGST");
            lblSGST.Text = sgst;

            Label lblIGST = (Label)e.Row.FindControl("lblIGST");
            lblIGST.Text = igst;

            Label lblQty = (Label)e.Row.FindControl("lblQty");
            lblQty.Text = Qty;

            Label lblBasicTotal = (Label)e.Row.FindControl("lblBasicTotal");
            lblBasicTotal.Text = Math.Round(Convert.ToDouble(basicamt)).ToString();

            Label lblGSTAmount = (Label)e.Row.FindControl("lblGSTAmount");
            lblGSTAmount.Text = Math.Round(Convert.ToDouble(GSTAmount)).ToString();

            Label lblGSTNumber = (Label)e.Row.FindControl("lblGSTNumber");
            lblGSTNumber.Text = gstno;

            Label lblStatus = (Label)e.Row.FindControl("lblStatus");
            if (lblStatus.Text != "")
            {
                lblStatus.Text = "Paid";
            }
            else
            {
                lblStatus.Text = "Raised";
            }

            Label lblType = (Label)e.Row.FindControl("lblType");
            if (lblGSTNumber.Text != "")
            {
                lblType.Text = "Register";
            }
            else
            {
                lblType.Text = "Unregister";
            }

        }
    }

    decimal BasicTotalAmt;
    decimal CGSTTotalAmt;
    decimal SGSTTotalAmt;
    decimal IGSTTotalAmt;
    protected void dgvHSNSummaryPurchase_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            con.Open();
            Label lblamount = e.Row.FindControl("lblBasicTotal") as Label;
            Label lblHSN = e.Row.FindControl("lblHSN") as Label;
            string basicamt = lblamount.Text;
            string hsn = lblHSN.Text;
            if(hsn!="")
            {
                SqlCommand cmdcgst = new SqlCommand("select  SUM(TRY_CAST(A.CGSTPer AS FLOAT))  FROM ExcelEncLive.tblPurchaseBillDtls A INNER JOIN ExcelEncLive.tblPurchaseBillHdr B ON A.HeaderID = B.Id where HSN='" + hsn + "' AND SupplierName='"+txtPartyName.Text+ "'GRoup BY HSN", con);
                string cgst = cmdcgst.ExecuteScalar().ToString();

                SqlCommand cmdsgst = new SqlCommand("select  SUM(TRY_CAST(A.SGSTPer AS FLOAT))  FROM ExcelEncLive.tblPurchaseBillDtls A INNER JOIN ExcelEncLive.tblPurchaseBillHdr B ON A.HeaderID = B.Id where HSN='" + hsn + "' AND SupplierName='" + txtPartyName.Text + "'GRoup BY HSN", con);
                string sgst = cmdsgst.ExecuteScalar().ToString();

                SqlCommand cmdigst = new SqlCommand("select  SUM(TRY_CAST(A.IGSTPer AS FLOAT))  FROM ExcelEncLive.tblPurchaseBillDtls A INNER JOIN ExcelEncLive.tblPurchaseBillHdr B ON A.HeaderID = B.Id where HSN='" + hsn + "' AND SupplierName='" + txtPartyName.Text + "'GRoup BY HSN", con);
                string igst = cmdigst.ExecuteScalar().ToString();

                var Cgstamt = Convert.ToDouble(basicamt) * Convert.ToDouble(cgst) / 100;
                var Sgstamt = Convert.ToDouble(basicamt) * Convert.ToDouble(sgst) / 100;
                var Igstamt = Convert.ToDouble(basicamt) * Convert.ToDouble(igst) / 100;

                string GSTAmount = (Cgstamt + Sgstamt + Igstamt).ToString();

                con.Close();
                Label lblCGST = (Label)e.Row.FindControl("lblCGSTAmt");
                lblCGST.Text = Math.Round(Cgstamt).ToString();

                Label lblSGST = (Label)e.Row.FindControl("lblSGSTAmt");
                lblSGST.Text = Math.Round(Sgstamt).ToString();

                Label lblIGST = (Label)e.Row.FindControl("lblIGSTAmt");
                lblIGST.Text = Math.Round(Igstamt).ToString();

                BasicTotalAmt += Convert.ToDecimal(lblamount.Text);
                CGSTTotalAmt += Convert.ToDecimal(Cgstamt);
                SGSTTotalAmt += Convert.ToDecimal(Sgstamt);
                IGSTTotalAmt += Convert.ToDecimal(Igstamt);
            }
           
        }
        if (e.Row.RowType == DataControlRowType.Footer)
        {
         
            (e.Row.FindControl("lblBasicTotalamt") as Label).Text = Math.Round(BasicTotalAmt).ToString("##.00");
            (e.Row.FindControl("lblCGSTTotalamt") as Label).Text = Math.Round(CGSTTotalAmt).ToString("##.00");
            (e.Row.FindControl("lblSGSTTotalamt") as Label).Text = Math.Round(SGSTTotalAmt).ToString("##.00");
            (e.Row.FindControl("lblIGSTTotalamt") as Label).Text = Math.Round(IGSTTotalAmt).ToString("##.00");

            
        }
    }

    #endregion
    
    protected void txtPartyName_TextChanged(object sender, EventArgs e)
    {
        bindPurchaseRegisterReportData();
    }
}