using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ClosedXML.Excel;
using System.Globalization;
using Spire.Pdf;
using Spire.Pdf.Texts;
using iTextSharp.text.pdf.parser;
using iTextSharp.tool.xml.html.table;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Font = iTextSharp.text.Font;
using DocumentFormat.OpenXml;
using iTextSharp.tool.xml;
using Spire.Pdf.Conversion;
using Microsoft.Reporting.WebForms;

public partial class Admin_PurchaseOutstandingReport : System.Web.UI.Page
{
    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
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
                DivRoot1.Visible = true;
                btn.Visible = true;
               
            }
        }
    }

    protected void btnresetfilter_Click(object sender, EventArgs e)
    {
        Response.Redirect("PurchaseOutstandingReport.aspx");
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

                com.CommandText = "select DISTINCT SupplierName from excelenclive.tblSupplierMaster where " + "SupplierName like @Search + '%'  ";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> SupplierName = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        SupplierName.Add(sdr["SupplierName"].ToString());
                    }
                }
                con.Close();
                return SupplierName;
            }

        }
    }


    decimal Balance = 0;
    protected void OnRowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            string invoiceno = e.Row.Cells[1].Text;
            string payble = e.Row.Cells[4].Text;
            string Recevd = e.Row.Cells[5].Text;
            Label lblbalance = (Label)e.Row.FindControl("lblbalance");
            Label lblCum_Balance = (Label)e.Row.FindControl("lblCum_Balance");
        }

    }

    protected void btnpdf_Click(object sender, EventArgs e)
    {
        string flg = "PDF";
        Report(flg);
    }


    protected void ExportExcel(object sender, EventArgs e)
    {
        string flg = "Excel";
        Report(flg);     
    }

    bool Show = false;

    protected void Report(string flg)
    {
        DataSet Dtt = new DataSet();
        string strConnString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
        using (SqlConnection con = new SqlConnection(strConnString))
        {
            using (SqlCommand cmd = new SqlCommand("[ExcelEncLive].[SP_OutstandingRForRDLReports]", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Type", "GetPurchaseOutstandingData");
                cmd.Parameters.AddWithValue("@PartyName", txtPartyName.Text);
                if (txtfromdate.Text != "")
                {
                    DateTime ftdate = Convert.ToDateTime(txtfromdate.Text, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat);
                    string fdate = ftdate.ToString("yyyy-MM-dd");
                    cmd.Parameters.AddWithValue("@FromDate", fdate);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FromDate", DBNull.Value);
                }
                if (txttodate.Text != "")
                {
                    DateTime date = Convert.ToDateTime(txttodate.Text, System.Globalization.CultureInfo.GetCultureInfo("ur-PK").DateTimeFormat);
                    string Todate = date.ToString("yyyy-MM-dd");
                    cmd.Parameters.AddWithValue("@ToDate", Todate);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ToDate", DBNull.Value);
                }

                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    sda.SelectCommand = cmd;
                    using (DataSet ds = new DataSet())
                    {
                        sda.Fill(Dtt);


                    }
                }
            }
        }

        if (Dtt.Tables.Count > 0)
        {

            if (Dtt.Tables[0].Rows.Count > 0)
            {
                ReportDataSource obj1 = new ReportDataSource("DataSet1", Dtt.Tables[0]);
                ReportViewer1.LocalReport.DataSources.Add(obj1);
                ReportViewer1.LocalReport.ReportPath = "RdlcReports\\Outstandingrdlc.rdlc";
                ReportViewer1.LocalReport.Refresh();
                //-------- Print PDF directly without showing ReportViewer ----
                Warning[] warnings;
                string[] streamids;
                string mimeType;
                string encoding;
                string extension;
                byte[] bytePdfRep = ReportViewer1.LocalReport.Render(flg, null, out mimeType, out encoding, out extension, out streamids, out warnings);
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                if (flg == "Excel")
                {
                    Response.ContentType = "application/vnd.xls";
                    Response.AddHeader("content-disposition", "attachment;filename=\"" + txtPartyName.Text + ".xls"); //Give file name here

                    Response.BinaryWrite(bytePdfRep);
                    ReportViewer1.LocalReport.DataSources.Clear();

                }
                else
                {
                    string filePath = Server.MapPath("~") + "/files/OutstandingReport.pdf";
                    System.IO.File.WriteAllBytes(filePath, bytePdfRep);
                    ifrRight6.Attributes["src"] = @"../files/" + "OutstandingReport.pdf";

                }
                this.ReportViewer1.Reset();


            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Record Not Found...........!')", true);
            }
        }
    }
}
































