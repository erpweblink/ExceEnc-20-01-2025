#pragma checksum "E:\Ashish\User Autorization Excel Enc 21-11-22\Admin\CDNotePDF.aspx.cs" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "AD85D3B4B77C20F32E90C3D6698F7937AC7DC798"

#line 1 "E:\Ashish\User Autorization Excel Enc 21-11-22\Admin\CDNotePDF.aspx.cs"
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
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Net.Mail;
using iTextSharp.text.pdf;
using System.Globalization;
using System.Threading.Tasks;
using iTextSharp.text;
using System.Collections;
using System.Net;
using iTextSharp.text.html.simpleparser;
using Image = iTextSharp.text.Image;
using iTextSharp.text.pdf.parser;

public partial class Admin_TaxInvoicePDF : System.Web.UI.Page
{
    //string id;
    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["Id"] != null)
            {
                //id = Session["PDFID"].ToString();// Decrypt(Request.QueryString["Id"].ToString());
                Pdf("Original");
            }
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

    public static string stringBetween(string Source, string Start, string End)
    {
        string result = "";
        if (Source.Contains(Start) && Source.Contains(End))
        {
            int StartIndex = Source.IndexOf(Start, 0) + Start.Length;
            int EndIndex = Source.IndexOf(End, StartIndex);
            result = Source.Substring(StartIndex, EndIndex - StartIndex);
            return result;
        }
        return result;
    }

    protected void Pdf(string flg)
    {
        string id = Session["PDFID"].ToString();

        DataTable Dt = new DataTable();
        SqlDataAdapter Da = new SqlDataAdapter("select * from vw_CreditDebitNote where Id = '" + id + "'", con);

        Da.Fill(Dt);

        StringWriter sw = new StringWriter();
        StringReader sr = new StringReader(sw.ToString());
        string billingsupplier = Dt.Rows[0]["SupplierName"].ToString();
        string BillNumber = Dt.Rows[0]["BillNumber"].ToString();
        string NoteType = Dt.Rows[0]["NoteType"].ToString();
        string Remark = Dt.Rows[0]["Remarks"].ToString();

        Document doc = new Document(PageSize.A4, 30f, 10f, -25f, 0f);
        //Document doc = new Document(PageSize.A4, 10f,);
        //string Path = ;
        string Docname = billingsupplier + "_" + NoteType + "Note.pdf";
        iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~/files/") + Docname, FileMode.Create));
        //PdfWriter writer = PdfWriter.GetInstance(doc, Response.OutputStream);
        iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, sr);

        doc.Open();
        string imageURL = Server.MapPath("~") + "/img/ExcelEncLogo.png";

        //Price Format
        System.Globalization.CultureInfo info = System.Globalization.CultureInfo.GetCultureInfo("en-IN");

        iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(imageURL);

        //Resi