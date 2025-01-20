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

public partial class Admin_ProformaInvoiceList : System.Web.UI.Page
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
            SqlDataAdapter sad = new SqlDataAdapter("select * from tblProformaInvoiceHdr order by CreatedOn DESC", con);
            sad.Fill(dt);
            GvInvoiceList.DataSource = dt;
            GvInvoiceList.DataBind();
            GvInvoiceList.EmptyDataText = "Record Not Found";

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
                com.CommandText = "Select DISTINCT [cname] from Company where " + "cname like @Search + '%'";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> BillingCustomer = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        BillingCustomer.Add(sdr["cname"].ToString());
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
            if (txtCustomerName.Text != "")
            {
                DataTable dtt = new DataTable();
                SqlDataAdapter sad = new SqlDataAdapter("select * from tblProformaInvoiceHdr where BillingCustomer='" + txtCustomerName.Text + "'", con);
                sad.Fill(dtt);
                GvInvoiceList.DataSource = dtt;
                GvInvoiceList.DataBind();
                GvInvoiceList.EmptyDataText = "Record Not Found";
            }
            else
            {
                DataTable dt = new DataTable();
                SqlDataAdapter sad = new SqlDataAdapter("select * from tblProformaInvoiceHdr order by CreatedOn DESC", con);
                sad.Fill(dt);
                GvInvoiceList.DataSource = dt;
                GvInvoiceList.DataBind();
                GvInvoiceList.EmptyDataText = "Record Not Found";
            }

        }
        catch (Exception)
        {

            throw;
        }
    }

    protected void btnresetfilter_Click(object sender, EventArgs e)
    {
        Response.Redirect("ProformaInvoiceList.aspx");
    }

    protected void GvInvoiceList_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "RowEdit")
        {
            Response.Redirect("ProformaInvoice.aspx?Id=" + encrypt(e.CommandArgument.ToString()));
        }
        if (e.CommandName == "DownloadPDF")
        {
            if (!string.IsNullOrEmpty(e.CommandArgument.ToString()))
            {
                Session["PDFID"] = e.CommandArgument.ToString();
                // Response.Write("<script>window.open('PurchaseBillPDF.aspx','_blank');</script>");
                Response.Write("<script>window.open('ProformaInvoicePDF.aspx?Id=" + encrypt(e.CommandArgument.ToString()) + "','_blank');</script>");


            }
        }
        if (e.CommandName == "RowDelete")
        {
            con.Open();
            SqlCommand Cmd = new SqlCommand("delete from tblProformaInvoiceHdr WHERE Id=@Id", con);
            Cmd.Parameters.AddWithValue("@Id", Convert.ToInt32(e.CommandArgument.ToString()));
            Cmd.ExecuteNonQuery();

            SqlCommand CmddeleteDtl = new SqlCommand("delete from tblProformaInvoiceDtls where HeaderID=@Id", con);
            CmddeleteDtl.Parameters.AddWithValue("@Id", Convert.ToInt32(e.CommandArgument.ToString()));
            CmddeleteDtl.ExecuteNonQuery();

            con.Close();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Data Deleted Sucessfully');window.location.href='ProformaInvoiceList.aspx';", true);
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
                LinkButton btnEdit = e.Row.FindControl("btnEdit") as LinkButton;
                LinkButton btnDelete = e.Row.FindControl("btnDelete") as LinkButton;
                LinkButton btnPDF = e.Row.FindControl("btnPDF") as LinkButton;

                string empcode = Session["empcode"].ToString();
                DataTable Dt = new DataTable();
                SqlDataAdapter Sd = new SqlDataAdapter("Select id from [employees] where [empcode]='" + empcode + "'", con);
                Sd.Fill(Dt);
                if (Dt.Rows.Count > 0)
                {
                    string idd = Dt.Rows[0]["id"].ToString();
                    DataTable Dtt = new DataTable();
                    SqlDataAdapter Sdd = new SqlDataAdapter("Select * FROM tblUserRoleAuthorization where UserID = '" + idd + "' AND PageName = 'ProformaInvoiceList.aspx' AND PagesView = '1'", con);
                    Sdd.Fill(Dtt);
                    if (Dtt.Rows.Count > 0)
                    {
                        btnAddProfInv.Visible = false;
                        btnEdit.Visible = false;
                        btnDelete.Visible = false;
                        btnPDF.Visible = true;
                    }
                }


                con.Open();
                int id = Convert.ToInt32(GvInvoiceList.DataKeys[e.Row.RowIndex].Values[0]);
                SqlCommand cmd = new SqlCommand("select COUNT(HeaderID) from tblProformaInvoiceDtls where HeaderID='" + id + "'", con);
                Object Procnt = cmd.ExecuteScalar();
                Label grandtotal = (Label)e.Row.FindControl("lblProduct");
                grandtotal.Text = Procnt == null ? "0" : Procnt.ToString();
                con.Close();


                Label lblGrandTotal = (Label)e.Row.FindControl("lblGrandTotal");

                var gtot = Math.Round(Convert.ToDouble(lblGrandTotal.Text));

                lblGrandTotal.Text = gtot.ToString("#0.00");

            }
        }
        catch (Exception)
        {

            throw;
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                