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
        
        if (BillNumber == "" || BillNumber == "--Select Invoice Number--")
        {
            BillNumber = "";
        }
        else
        {
           BillNumber = Dt.Rows[0]["BillNumber"].ToString();
        }

        Document doc = new Document(PageSize.A4, 30f, 10f, -25f, 0f);
        //Document doc = new Document(PageSize.A4, 10f,);
        //string Path = ;

        if (NoteType == "Credit_Sale")
        {
            NoteType = "Credit";
        }
        if (NoteType == "Debit_Sale")
        {
            NoteType = "Debit";
        }
      
        string Docname = billingsupplier + "_" + NoteType + "Note.pdf";
  
        iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, new FileStream(Server.MapPath("~/files/") + Docname, FileMode.Create));
        //PdfWriter writer = PdfWriter.GetInstance(doc, Response.OutputStream);
        iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, sr);

        doc.Open();
        string imageURL = Server.MapPath("~") + "/img/ExcelEncLogo.png";

        //Price Format
        System.Globalization.CultureInfo info = System.Globalization.CultureInfo.GetCultureInfo("en-IN");

        iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(imageURL);


        //Resize image depend upon your need
        png.ScaleToFit(100, 100);

        //For Image Position
        png.SetAbsolutePosition(40, 779);
        //var document = new Document();

        //Give space before image
        //png.ScaleToFit(document.PageSize.Width - (document.RightMargin * 100), 50);
        png.SpacingBefore = 50f;

        //Give some space after the image

        png.SpacingAfter = 1f;

        png.Alignment = Element.ALIGN_LEFT;


        doc.Add(png);


        if (Dt.Rows.Count > 0)
        {
            var CreateDate = DateTime.Now.ToString("yyyy-MM-dd");

            string BillDate = Dt.Rows[0]["BillDate"].ToString().TrimEnd("0:0".ToCharArray());

            string CategoryName = Dt.Rows[0]["CategoryName"].ToString();
            string DocNo = Dt.Rows[0]["DocNo"].ToString();
            string DocDate = Dt.Rows[0]["DocDate"].ToString().TrimEnd("0:0".ToCharArray());

            //string ShippingCustomer = Dt.Rows[0]["CustomerName"].ToString();
            //string ShippingAddress = Dt.Rows[0]["ShippingAddress"].ToString();
            //string BillingAddress = Dt.Rows[0]["ShippingAddress"].ToString();
            string grandtotal = Dt.Rows[0]["Grandtotal"].ToString();

            string GSTNo = "";
            string ShippingAddress = "";
            string BillingAddress = "";

            DataTable dtgstno = new DataTable();
            //SqlDataAdapter sadgst = new SqlDataAdapter("select * from tblSupplierMaster where SupplierName='" + billingsupplier + "'", con);
            SqlDataAdapter sadgst = new SqlDataAdapter("select * from Company where cname='" + billingsupplier + "'", con);
            sadgst.Fill(dtgstno);
            if (dtgstno.Rows.Count > 0)
            {
                GSTNo = dtgstno.Rows[0]["GSTNo"].ToString();
                ShippingAddress = dtgstno.Rows[0]["shippingaddress"].ToString();
                BillingAddress = dtgstno.Rows[0]["billingaddress"].ToString();
            }

            string PanNo = "";
            if (GSTNo == "")
            {
                PanNo = "NA";
                GSTNo = "NA";
            }
            else
            {
                string MyString = GSTNo;
                string res = MyString.Substring(0, 2);
                string word1 = res;
                string word2 = "1Z";
                PanNo = stringBetween(MyString, word1, word2);
            }

            PdfContentByte cb = writer.DirectContent;
            cb.Rectangle(28f, 756f, 560f, 80f);

            cb.Stroke();
            // Header 
            cb.BeginText();

            cb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 25);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Excel Enclosures", 250, 815, 0);
            cb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 11);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Gat No. 1567, Shelar Vasti, Dehu-Alandi Road, Chikhali, Pune - 411062", 150, 800, 0);
            cb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 11);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "EMAIL : purchase@excelenclosures.com", 250, 785, 0);
            cb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 11);
            cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "", 227, 740, 0);
            cb.EndText();


            PdfContentByte cbbb = writer.DirectContent;
            cbbb.Rectangle(28f, 756f, 560f, 25f);
            cbbb.Stroke();
            //Header
            cbbb.BeginText();
            cbbb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 10);
            cbbb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "GSTIN :27ATFPS1959J1Z4" + "", 48, 765, 0);
            cbbb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 10);
            cbbb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "PAN NO: ATFPS1959J" + "", 170, 765, 0);
            cbbb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 10);
            cbbb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "MAHARASHTRA STATE GST CODE : 27" + "", 280, 765, 0);
            cbbb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 10);
            cbbb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "CONTACT : 9225658662", 455, 765, 0);
            cbbb.EndText();

            PdfContentByte cd = writer.DirectContent;
            cd.Rectangle(28f, 731f, 560f, 25f);
            cd.Stroke();
            // Header 
            cd.BeginText();

            if (flg == "Original")
            {
                //cb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 10);
                //cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "ORIGINAL FOR BUYER", 480, 739, 0);
            }
            else if (flg == "Duplicate")
            {
                cb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 10);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "DUPLICATE FOR TRANSPORTER", 450, 739, 0);
            }
            else if (flg == "Triplicate")
            {
                cb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 10);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "TRIPLICATE FOR SUPPLIER", 470, 739, 0);
            }
            else if (flg == "Extra")
            {
                cb.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 10);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "EXTRA COPY", 480, 739, 0);
            }
            cd.SetFontAndSize(BaseFont.CreateFont(@"C:\Windows\Fonts\Calibrib.ttf", "Identity-H", BaseFont.EMBEDDED), 14);
            cd.ShowTextAligned(PdfContentByte.ALIGN_LEFT, NoteType + " Note", 270, 739, 0);
            cd.EndText();


            //DetailCustomer

            Paragraph paragraphTable1 = new Paragraph();
            paragraphTable1.SpacingBefore = 120f;
            paragraphTable1.SpacingAfter = 0f;

            PdfPTable table = new PdfPTable(6);

            float[] widths2 = new float[] { 100, 100, 100, 100, 0, 0 };
            table.SetWidths(widths2);
            table.TotalWidth = 560f;
