using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;

public partial class Admin_TaxInvoiceList : System.Web.UI.Page
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
                GVBinddata();
            }
        }
    }

    protected void GVBinddata()
    {
        try
        {
            DataTable dt = new DataTable();
                        SqlDataAdapter sad = new SqlDataAdapter("SELECT TOP 100 TIH.ID, TIH.BillingCustomer, CASE WHEN TIH.InvoiceNo IS NULL THEN TIH.finalbasic ELSE TIH.InvoiceNo END AS InvoiceNo, TIH.Invoicedate, TIH.CustomerPONo, SUM(CONVERT(decimal(18, 2), TD.GrandTotal)) + CONVERT(decimal(18, 2), TIH.Cost) AS GrandTotalFinal, TIH.CreatedBy, TIH.CreatedOn, TIH.finalbasic AS finalbasic, TIH.Status, TIH.IsPaid FROM tblTaxInvoiceHdr AS TIH INNER JOIN tblTaxInvoiceDtls AS TD ON TD.HeaderID = TIH.ID WHERE TIH.isdeleted = '0' GROUP BY TIH.ID, TIH.BillingCustomer, CASE WHEN TIH.InvoiceNo IS NULL THEN TIH.finalbasic ELSE TIH.InvoiceNo END, TIH.Invoicedate, TIH.CustomerPONo, TIH.CreatedBy, TIH.CreatedOn, TIH.Cost, TIH.finalbasic, TIH.Status, TIH.IsPaid ORDER BY TIH.CreatedOn DESC;", con);

            sad.Fill(dt);
            GvInvoiceList.DataSource = dt;
            GvInvoiceList.DataBind();
            GvInvoiceList.EmptyDataText = "Record Not Found";
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GvInvoiceList.ClientID + "', 400, 1020 , 40 ,true); </script>", false);
        }
        catch (Exception)
        {
            throw;
        }
    }

    [System.Web.Script.Services.ScriptMethod()]
    [System.Web.Services.WebMethod]
    public static List<string> GetCustomerList(string prefixText, int count)
    {
        return AutoFillCustomerlist(prefixText);
    }

    public static List<string> AutoFillCustomerlist(string prefixText)
    {
        using (SqlConnection con = new SqlConnection())
        {
            con.ConnectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

            using (SqlCommand com = new SqlCommand())
            {
                com.CommandText = "select DISTINCT BillingCustomer from tblTaxInvoiceHdr where " + "BillingCustomer like @Search + '%' AND isdeleted='0'  ";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> BillingCustomer = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        BillingCustomer.Add(sdr["BillingCustomer"].ToString());
                    }
                }
                con.Close();
                return BillingCustomer;
            }

        }
    }

    protected void txtCustomerName_TextChanged(object sender, EventArgs e)
    {
        try
        {
            DataTable dtt = new DataTable();
                        SqlDataAdapter sad = new SqlDataAdapter("SELECT TIH.ID, TIH.BillingCustomer, CASE WHEN TIH.InvoiceNo IS NULL THEN TIH.finalbasic ELSE TIH.InvoiceNo END AS InvoiceNo, TIH.Invoicedate, TIH.CustomerPONo, SUM(CONVERT(decimal(18, 2), TD.GrandTotal)) + CONVERT(decimal(18, 2), TIH.Cost) AS GrandTotalFinal, TIH.CreatedBy, TIH.CreatedOn, TIH.finalbasic AS finalbasic, TIH.Status, TIH.IsPaid FROM tblTaxInvoiceHdr AS TIH INNER JOIN tblTaxInvoiceDtls AS TD ON TD.HeaderID = TIH.ID WHERE TIH.BillingCustomer='" + txtCustomerName.Text + "' AND TIH.isdeleted = '0' GROUP BY TIH.ID, TIH.BillingCustomer, CASE WHEN TIH.InvoiceNo IS NULL THEN TIH.finalbasic ELSE TIH.InvoiceNo END, TIH.Invoicedate, TIH.CustomerPONo, TIH.CreatedBy, TIH.CreatedOn, TIH.Cost, TIH.finalbasic, TIH.Status, TIH.IsPaid ORDER BY TIH.CreatedOn DESC;", con);

            sad.Fill(dtt);
            GvInvoiceList.DataSource = dtt;
            GvInvoiceList.DataBind();
            GvInvoiceList.EmptyDataText = "Record Not Found";
        }
        catch (Exception)
        {

            throw;
        }
    }

    protected void btnresetfilter_Click(object sender, EventArgs e)
    {
        Response.Redirect("TaxInvoiceList.aspx");
    }

    protected void GvInvoiceList_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "RowEdit")
        {
            Response.Redirect("TaxInvoice.aspx?Id=" + encrypt(e.CommandArgument.ToString()));
        }
        if (e.CommandName == "DownloadPDF")
        {
            if (!string.IsNullOrEmpty(e.CommandArgument.ToString()))
            {
                Session["PDFID"] = e.CommandArgument.ToString();
                // Response.Write("<script>window.open('PurchaseBillPDF.aspx','_blank');</script>");
                Response.Write("<script>window.open ('TaxInvoicePDF.aspx?Id=" + encrypt(e.CommandArgument.ToString()) + "','_blank');</script>");


            }
        }
        if (e.CommandName == "RowDelete")
        {
            con.Open();
            SqlCommand cmdget = new SqlCommand("select AgainstNumber from tblTaxInvoiceHdr WHERE Id=@Id", con);
            cmdget.Parameters.AddWithValue("@Id", Convert.ToInt32(e.CommandArgument.ToString()));
            string pono = cmdget.ExecuteScalar().ToString();

            SqlCommand CmduptDtl = new SqlCommand("update OrderAccept set status=null where pono=@pono", con);
            CmduptDtl.Parameters.AddWithValue("@pono", pono);
            CmduptDtl.ExecuteNonQuery();

            SqlCommand Cmd = new SqlCommand("delete from tblTaxInvoiceHdr WHERE Id=@Id", con);
            Cmd.Parameters.AddWithValue("@Id", Convert.ToInt32(e.CommandArgument.ToString()));
            Cmd.ExecuteNonQuery();

            SqlCommand CmddeleteDtl = new SqlCommand("delete from tblTaxInvoiceDtls where HeaderID=@Id", con);
            CmddeleteDtl.Parameters.AddWithValue("@Id", Convert.ToInt32(e.CommandArgument.ToString()));
            CmddeleteDtl.ExecuteNonQuery();


            con.Close();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Data Deleted Sucessfully');window.location.href='TaxInvoiceList.aspx';", true);
        }
    }

    public string encrypt(string encryptString)
    {
        string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                encryptString = Convert.ToBase64String(ms.ToArray());
            }
        }
        return encryptString;
    }

    protected void GvInvoiceList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        try
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                con.Open();
                int id = Convert.ToInt32(GvInvoiceList.DataKeys[e.Row.RowIndex].Values[0]);
                SqlCommand cmd = new SqlCommand("select COUNT(HeaderID) from tblTaxInvoiceDtls where HeaderID='" + id + "'", con);
                Object Procnt = cmd.ExecuteScalar();
                Label grandtotal = (Label)e.Row.FindControl("lblProduct");
                grandtotal.Text = Procnt == null ? "0" : Procnt.ToString();
                con.Close();

                Label lblGrandTotal = (Label)e.Row.FindControl("lblGrandTotal");
                var gtot = Math.Round(Convert.ToDouble(lblGrandTotal.Text));

                lblGrandTotal.Text = gtot.ToString("#0.00");
                Label lblStatus = (Label)e.Row.FindControl("lblStatus");
                LinkButton lnkEdit = (LinkButton)e.Row.FindControl("lnkEdit");
                LinkButton lnkPDF = (LinkButton)e.Row.FindControl("lnkPDF");
                LinkButton lnkDelete = (LinkButton)e.Row.FindControl("lnkDelete");
                Label lblInvoiceNo = (Label)e.Row.FindControl("lblInvoiceNo");
                Label lblFinalBasic = (Label)e.Row.FindControl("lblFinalBasic");
                if (lblStatus.Text == "True")
                {
                    lblStatus.Text = "Paid";
                    lnkEdit.Visible = false;
                    lnkDelete.Visible = false;
                }
                else
                {
                    lblStatus.Text = "Pending";
                    lnkEdit.Visible = true;
                    lnkDelete.Visible = true;
                }
                if (lblInvoiceNo.Text != "")
                {
                    lblInvoiceNo.Text = lblInvoiceNo.Text;
                }
                else
                {
                    lblInvoiceNo.Text = lblFinalBasic.Text;
                }

                string empcode = Session["empcode"].ToString();
                DataTable Dt = new DataTable();
                SqlDataAdapter Sd = new SqlDataAdapter("Select id from [employees] where [empcode]='" + empcode + "'", con);
                Sd.Fill(Dt);
                if (Dt.Rows.Count > 0)
                {
                    string idd = Dt.Rows[0]["id"].ToString();
                    DataTable Dtt = new DataTable();
                    SqlDataAdapter Sdd = new SqlDataAdapter("Select * FROM tblUserRoleAuthorization where UserID = '" + idd + "' AND PageName = 'TaxInvoiceList.aspx' AND PagesView = '1'", con);
                    Sdd.Fill(Dtt);
                    if (Dtt.Rows.Count > 0)
                    {
                        btnAddTaxInvoice.Visible = false;
                        lnkEdit.Visible = false;
                        lnkDelete.Visible = false;
                        lnkPDF.Visible = true;
                    }
                }
                //Method for finding Mismatched Values
                //DataTable Dt_MismatchedValues = GetData("SP_FindingMismatchedAmt_New", id);
                //if (Dt_MismatchedValues.Rows.Count > 0)
                //{
                //    e.Row.BackColor = System.Drawing.Color.LightPink;
                //}

                // Check whether the E-Invoice is Created or not
                con.Open();
                SqlCommand cmdIgstVal = new SqlCommand("select Irn from tblTaxInvoiceHdr where Id='" + id + "'", con);
                Object F_IgstVal = cmdIgstVal.ExecuteScalar();
                string IsCreatedIRN = F_IgstVal.ToString();
                if (IsCreatedIRN == "")
                {
                    lnkEdit.Visible = true;
                    lnkDelete.Visible = true;

                }
                else
                {
                    lnkEdit.Visible = false;
                    lnkDelete.Visible = false;
                }
                con.Close();

                DataTable Dttt = new DataTable();
                SqlDataAdapter Sddd = new SqlDataAdapter("Select * FROM tbltaxinvoicehdr where Id = '" + id + "'", con);
                Sddd.Fill(Dttt);
                if (Dttt.Rows.Count > 0)
                {
                    string e_invoice_status = Dttt.Rows[0]["e_invoice_status"].ToString();
                    string e_invoice_cancel_status = Dttt.Rows[0]["e_invoice_cancel_status"].ToString();

                    if (e_invoice_status == true.ToString() && e_invoice_cancel_status == true.ToString())
                    {

                        e.Row.BackColor = System.Drawing.Color.LightPink;
                    }
                    if (lblInvoiceNo.Text == null || lblInvoiceNo.Text == "")
                    {
                        lblInvoiceNo.Text = Dttt.Rows[0]["FinalBasic"].ToString();
                    }
                }
                con.Close();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private static DataTable GetData(string SP, int id)
    {
        string strConnString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        using (SqlConnection con = new SqlConnection(strConnString))
        {
            using (SqlCommand cmd = new SqlCommand(SP, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);

                using (SqlDataAdapter sda_MismatchedValues = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    sda_MismatchedValues.SelectCommand = cmd;
                    using (DataSet ds_mis = new DataSet())
                    {
                        DataTable Dt_MismatchedValues = new DataTable();
                        sda_MismatchedValues.Fill(Dt_MismatchedValues);
                        return Dt_MismatchedValues;
                    }
                }
            }
        }
    }
}