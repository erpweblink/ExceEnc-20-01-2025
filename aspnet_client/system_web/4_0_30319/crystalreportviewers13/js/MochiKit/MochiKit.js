using ERPTMT;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Admin_QuotationList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            FillGrid(); FillddlQuotationno(); FillddlCompany();
            if (Session["Username"] == null)
            {
                Response.Redirect("../Login.aspx");
            }
            else
            {
                //string curdate = DateTime.Now.ToString("dd/MM/yyyy");
                //DateTime ffff1 = Convert.ToDateTime(curdate);
                //txtquotationdate.Text = ffff1.ToString("yyyy-MM-dd");

                //string Validtilldate = DateTime.Now.ToString("dd/MM/yyyy");
                //DateTime ffff2 = Convert.ToDateTime(curdate).AddDays(30); ;
                //txtvalidtill.Text = ffff2.ToString("yyyy-MM-dd");
            }
        }
    }

    //Fill GridView
    private void FillGrid()
    {
        DataTable Dt = Cls_Main.Read_Table("SELECT * FROM [tbl_QuotationHdr] WHERE IsDeleted = 0 ORDER BY [CreatedOn] Desc");
        GVQuotation.DataSource = Dt;
        GVQuotation.DataBind();
    }

    private void FillddlQuotationno()
    {
        SqlDataAdapter ad = new SqlDataAdapter("SELECT [ID],[Quotationno] FROM [tbl_QuotationHdr] WHERE IsDeleted = 0", Cls_Main.Conn);
        DataTable dt = new DataTable();
        ad.Fill(dt);
        if (dt.Rows.Count > 0)
        {
            ddlquotationno.DataSource = dt;
            ddlquotationno.DataValueField = "ID";
            ddlquotationno.DataTextField = "Quotationno";
            ddlquotationno.DataBind();
            ddlquotationno.Items.Insert(0, " --  Select Quotation No. -- ");
        }
    }

    private void FillddlCompany()
    {
        SqlDataAdapter ad = new SqlDataAdapter("SELECT DISTINCT [Companyname] FROM [tbl_QuotationHdr] WHERE IsDeleted = 0", Cls_Main.Conn);
        DataTable dt = new DataTable();
        ad.Fill(dt);
        if (dt.Rows.Count > 0)
        {
            ddlcompnay.DataSource = dt;
           // ddlcompnay.DataValueField = "ID";
            ddlcompnay.DataTextField = "Companyname";
            ddlcompnay.DataBind();
            ddlcompnay.Items.Insert(0, " --  Select Company -- ");
        }
    }

    //Encrypt
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

    protected void btnCreate_Click(object sender, EventArgs e)
    {
        Response.Redirect("Quotation_Master.aspx");
    }

    protected void GVQuotation_RowCommand(object sender, GridViewCommandEventArgs e)
    {

        if (e.CommandName == "RowEdit")
        {
            Response.Redirect("Quotation_Master.aspx?Id=" + encrypt(e.CommandArgument.ToString()) + "");
        }

        if (e.CommandName == "RowDelete")
        {
            Cls_Main.Conn_Open();
            SqlCommand Cmd = new SqlCommand("UPDATE [tbl_QuotationHdr] SET IsDeleted=@IsDeleted,DeletedBy=@DeletedBy,DeletedOn=@DeletedOn WHERE ID=@ID", Cls_Main.Conn);
            Cmd.Parameters.AddWithValue("@ID", Convert.ToInt32(e.CommandArgument.ToString()));
            Cmd.Parameters.AddWithValue("@IsDeleted", '1');
            Cmd.Parameters.AddWithValue("@DeletedBy", Session["Username"].ToString());
            Cmd.Parameters.AddWithValue("@DeletedOn", DateTime.Now);
            Cmd.ExecuteNonQuery();
            Cls_Main.Conn_Close();
            Scri