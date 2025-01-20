using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System.Collections;
using System.Net.Mail;
using System.Net;
using System.Net.Mime;

public partial class Admin_OrderAcceptanceList : System.Web.UI.Page
{
    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
    SqlDataAdapter adp;
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
                LinkButton linkcname = new LinkButton();
                linkcname.Attributes.Add("onClick", "return false;");
                FillOAgrid();
            }
        }
    }

    //private void FillOAgrid()
    //{

    //    string query = "";
    //    if (!string.IsNullOrEmpty(txtCustomerName.Text))
    //    {
    //        adp = new SqlDataAdapter(@"SELECT  [id],[quotationid],[oano],[quotationno],[constructiontype],[partyname],[description],
    //                         [status],convert(varchar(20),[createddate],105) as createddate,status,emailstatus,deliverydatereqbycust FROM [ExcelEncLive].[vw_OAList] where IsDispatch is null and partyname like '" + txtCustomerName.Text.Trim() + "%' order by deliverydatereqbycust dsc", con);
    //    }
    //    if (ddlDispatchList.SelectedItem.Text == "Pending")
    //    {
    //        adp = new SqlDataAdapter(@"SELECT  [id],[quotationid],[oano],[quotationno],[constructiontype],[partyname],[description],
    //                         [status],convert(varchar(20),[createddate],105) as createddate,status,emailstatus,deliverydatereqbycust FROM [ExcelEncLive].[vw_OAList] where IsDispatch is null order by deliverydatereqbycust dsc", con);
    //    }
    //    if (ddlDispatchList.SelectedItem.Text == "Dispatch")
    //    {
    //        adp = new SqlDataAdapter(@"SELECT [id],[quotationid],[oano],[quotationno],[constructiontype],[partyname],[description],
    //                         [status],convert(varchar(20),[createddate],105) as createddate,status,emailstatus,deliverydatereqbycust FROM [ExcelEncLive].[vw_OAList] where IsDispatch='1' order by deliverydatereqbycust dsc", con);
    //    }
    //    if (ddlDispatchList.SelectedItem.Text == "--All--")
    //    {
    //        adp = new SqlDataAdapter(@"SELECT [id],[quotationid],[oano],[quotationno],[constructiontype],[partyname],[description],
    //                         [status],convert(varchar(20),[createddate],105) as createddate,status,emailstatus,deliverydatereqbycust FROM [ExcelEncLive].[vw_OAList] order by deliverydatereqbycust dsc", con);
    //    }
    //    if (ddlDispatchList.SelectedItem.Text == "Dispatch" && !string.IsNullOrEmpty(txtCustomerName.Text))
    //    {
    //        adp = new SqlDataAdapter(@"SELECT [id],[quotationid],[oano],[quotationno],[constructiontype],[partyname],[description],
    //                         [status],convert(varchar(20),[createddate],105) as createddate,status,emailstatus,deliverydatereqbycust FROM[ExcelEncLive]. [vw_OAList] where IsDispatch='1' and partyname like '" + txtCustomerName.Text.Trim() + "%' order by deliverydatereqbycust dsc", con);
    //    }
    //    if (ddlDispatchList.SelectedItem.Text == "Pending" && !string.IsNullOrEmpty(txtCustomerName.Text))
    //    {
    //        adp = new SqlDataAdapter(@"SELECT [id],[quotationid],[oano],[quotationno],[constructiontype],[partyname],[description],
    //                         [status],convert(varchar(20),[createddate],105) as createddate,status,emailstatus,deliverydatereqbycust FROM [ExcelEncLive].[vw_OAList] where IsDispatch is null and partyname like '" + txtCustomerName.Text.Trim() + "%' order by deliverydatereqbycust dsc", con);
    //    }
    //    if (ddlDispatchList.SelectedItem.Text == "--All--" && !string.IsNullOrEmpty(txtCustomerName.Text))
    //    {
    //        adp = new SqlDataAdapter(@"SELECT [id],[quotationid],[oano],[quotationno],[constructiontype],[partyname],[description],
    //                         [status],convert(varchar(20),[createddate],105) as createddate,status,emailstatus,deliverydatereqbycust FROM [ExcelEncLive].[vw_OAList] where partyname like '" + txtCustomerName.Text.Trim() + "%' order by deliverydatereqbycust dsc", con);

    //    }
    //    //else
    //    //{
    //    //    adp = new SqlDataAdapter(@"SELECT [id],[quotationid],[oano],[quotationno],[constructiontype],[partyname],[description],
    //    //                     [status],convert(varchar(20),[createddate],105) as createddate,status,emailstatus,deliverydatereqbycust FROM [vw_OAList] order by id asc", con);
    //    //}
    //    DataTable dt = new DataTable();
    //    adp.Fill(dt);

    //    if (dt.Rows.Count > 0)
    //    {
    //        GvOA.DataSource = dt;
    //        GvOA.DataBind();
    //        ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GvOA.ClientID + "', 900, 1020 , 40 ,true); </script>", false);
    //    }
    //    else
    //    {
    //        GvOA.DataSource = null;
    //        GvOA.DataBind();

    //    }
    //}


    private void FillOAgrid()
    {
        string query = "SELECT   [id], [quotationid], [oano], [quotationno], [constructiontype], [partyname], [description], " +
                       "[status], CONVERT(VARCHAR(20), [createddate], 105) AS createddate, status, emailstatus, deliverydatereqbycust " +
                       "FROM [ExcelEncLive].[vw_OAList] ";

        bool isWhereAdded = false;  // Flag to check if the WHERE clause has been added

        // Handle customer name filter
        if (!string.IsNullOrEmpty(txtCustomerName.Text))
        {
            query += "WHERE partyname LIKE '" + txtCustomerName.Text.Trim() + "%' ";
            isWhereAdded = true;  // Mark that WHERE is added
        }

        // Handle dispatch status filter
        if (ddlDispatchList.SelectedItem.Text != "--All--")
        {
            if (isWhereAdded) // If WHERE is already added, use AND
            {
                query += "AND ";
            }
            else // If WHERE is not added yet, use it for the first condition
            {
                query += "WHERE ";
                isWhereAdded = true; // Mark that WHERE is now added
            }

            if (ddlDispatchList.SelectedItem.Text == "Pending")
            {
                query += "IsDispatch IS NULL ";
            }
            else if (ddlDispatchList.SelectedItem.Text == "Dispatch")
            {
                query += "IsDispatch = '1' ";
            }
        }

        // Add ordering to the query
        query += "ORDER BY deliverydatereqbycust ASC";

        // Create the SqlDataAdapter
        adp = new SqlDataAdapter(query, con);

        // Fill the DataTable
        DataTable dt = new DataTable();
        adp.Fill(dt);

        // Bind data to GridView
        if (dt.Rows.Count > 0)
        {
            GvOA.DataSource = dt;
            GvOA.DataBind();
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GvOA.ClientID + "', 900, 1020 , 40 ,true); </script>", false);
        }
        else
        {
            GvOA.DataSource = null;
            GvOA.DataBind();
        }
    }


    protected void GvOA_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string id = e.CommandArgument.ToString();

        if (e.CommandName == "RowCreateOA")
        {
            Response.Redirect("OrderAcceptance.aspx?cmd=" + encrypt(id.ToString()));
        }

        if (e.CommandName == "RowEditOA")
        {
            Control ctrl = e.CommandSource as Control;
            GridViewRow row = ctrl.Parent.NamingContainer as GridViewRow;
            Label ConstType = (Label)row.FindControl("lblConstructiontype");
            Label NO = (Label)row.FindControl("lblquotationno");
            Thread.Sleep(2000);
            if (NO.Text == "")
            {
                Response.Redirect("ManualOrderAcceptance.aspx?cmded=" + encrypt(id.ToString()));
            }
            else
            {
                string Report = "Quation";
                string encryptedId = encrypt(id);
                string url = "ManualOrderAcceptance.aspx?cmded=" + encryptedId + "&name=" + Server.UrlEncode(Report);
                // Response.Redirect("OrderAcceptance.aspx?cmded=" + encrypt(id.ToString()));
                //Response.Redirect("ManualOrderAcceptance.aspx?cmded=" + encrypt(id.ToString()));
                Response.Redirect(url);
            }

        }

        // Before 27-10-23
        //if (e.CommandName == "RowEditOA")
        //{
        //    Control ctrl = e.CommandSource as Control;
        //    GridViewRow row = ctrl.Parent.NamingContainer as GridViewRow;
        //    Label ConstType = (Label)row.FindControl("lblConstructiontype");
        //    Thread.Sleep(2000);
        //    if (ConstType.Text == "")
        //    {
        //        Response.Redirect("ManualOrderAcceptance.aspx?cmded=" + encrypt(id.ToString()));
        //    }
        //    else
        //    {
        //        Response.Redirect("OrderAcceptance.aspx?cmded=" + encrypt(id.ToString()));
        //    }

        //}

        if (e.CommandName == "RowDeleteOA")
        {
            //SqlCommand cmddelete2 = new SqlCommand("delete from OrderAccept where OAno='" + id + "'", con);
            //con.Open();
            //Thread.Sleep(1000);
            //cmddelete2.ExecuteNonQuery();
            //con.Close();

            //SqlCommand cmddelete4 = new SqlCommand("delete from OrderAcceptDtls where OAno='" + id + "'", con);
            //con.Open();
            //Thread.Sleep(1000);
            //cmddelete4.ExecuteNonQuery();
            //con.Close();

            //SqlCommand cmddelete3 = new SqlCommand("delete from OAList where oano='" + id + "'", con);
            //con.Open();
            //Thread.Sleep(1000);
            //cmddelete3.ExecuteNonQuery();
            //con.Close();

            String ID = id;
            DeletedRecord(ID);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('OA has been Deleted Succesfully...!!!');window.location='OrderAcceptanceList.aspx';", true);
        }
        if (e.CommandName == "partyname")
        {
            if (!string.IsNullOrEmpty(e.CommandArgument.ToString()))
            {
                //GetCompanyDataPopup(e.CommandArgument.ToString());
                //GetCompanyDataBDEPopup(e.CommandArgument.ToString());
                GetOAStatusByCustName(e.CommandArgument.ToString());
                this.modelprofile.Show();
            }
        }

        if (e.CommandName == "Sendamil")
        {
            if (!string.IsNullOrEmpty(e.CommandArgument.ToString()))
            {                 

                Control ctrl = e.CommandSource as Control;
                GridViewRow row = ctrl.Parent.NamingContainer as GridViewRow;

                // Assuming "linkcname" is the correct ID of your Label
                Label OA = (Label)row.FindControl("lbloano");
                LinkButton Customer = (LinkButton)row.FindControl("linkcname");

                string ID = OA.Text;
                if (Customer != null)
                {
                    string User = Customer.Text.Replace("<br /><br />", " ").Trim();
                    string customer = Server.UrlEncode(Customer.Text);
                    String Usermail = "";
                    //new
                    SqlDataAdapter ad = new SqlDataAdapter("SELECT TOP 1 email1 AS Usermail FROM Company WHERE cname LIKE '%" + User + "%'", con);
                    DataTable dt = new DataTable();
                    ad.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        Usermail = dt.Rows[0]["Usermail"].ToString();
                       // Usermail = "erp@weblinkservices.net";
                        string url = "DepartmentWiseOAReport.aspx?ID=" + Server.UrlEncode(ID) + "&name=" + Server.UrlEncode(customer);

                        SendMail(url, User, ID, Usermail);
                        Updatemailstaus(ID);
                        //Response.Redirect(url);

                    }
                    else
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Please Register Mail First !!');", true);
                    }

                }
            }
        }


    }

    protected void GvOA_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        GvOA.PageIndex = e.NewPageIndex;
        FillOAgrid();
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

    protected void BindDeprt()
    {
        try
        {
            ArrayList alstNames = new ArrayList();
            alstNames.Add("Drawing Creation");
            alstNames.Add("Laser Programing");
            alstNames.Add("Laser Cutting");
            alstNames.Add("CNC Bending");
            alstNames.Add("Welding");
            alstNames.Add("Powder Coating");
            alstNames.Add("Final Assembly");
            //alstNames.Add("Final Inspection");
            alstNames.Add("Stock");
            //dgvDeprt.DataSource = alstNames;
            //dgvDeprt.DataBind();
        }
        catch (Exception)
        {

            throw;
        }

    }

    private void GetOAStatusByCustName(string OAnum)
    {
        try
        {
            //SqlDataAdapter adp = new SqlDataAdapter(@"select DISTINCT(OAno) from  [ExcelEncLive].[vwOrderAccept] where OAno='" + OAnum + "'", con);
            SqlDataAdapter adp = new SqlDataAdapter(@"select DISTINCT(OAno),pono,CONVERT(VARCHAR(10), podate, 105) as Podate from  [ExcelEncLive].[vwOrderAccept] where OAno='" + OAnum + "'", con);
            DataTable dt = new DataTable();
            adp.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                dgvOANumber.DataSource = dt;
                dgvOANumber.DataBind();
            }

            DataTable dtorder = new DataTable();
            SqlDataAdapter sadorder = new SqlDataAdapter("select * from [ExcelEncLive].[vwOrderAccept] where OAno='" + OAnum + "'", con);
            sadorder.Fill(dtorder);
            ddlSubOaNo.DataValueField = "SubOANumber";
            ddlSubOaNo.DataTextField = "SubOANumber";
            lblcname.Text = dtorder.Rows[0]["customername"].ToString();
            ddlSubOaNo.DataSource = dtorder;
            ddlSubOaNo.DataBind();

            //SqlDataAdapter adp1 = new SqlDataAdapter(@"select SubOANumber,customername from [ExcelEncLive].[vwOrderAccept] where OAno='" + OAnum + "'", con);
            //DataTable dt1 = new DataTable();
            //adp1.Fill(dt1);
            //if (dt1.Rows.Count > 0)
            //{
            //    dgvSubOA.DataSource = dt1;
            //    dgvSubOA.DataBind();

            //    lblcname.Text = dt1.Rows[0]["customername"].ToString();
            //}
            SqlCommand cmdtot = new SqlCommand("select SUM(CAST(Qty as int)) as Tot from [ExcelEncLive].[vwOrderAccept] where OAno='" + OAnum + "'", con);
            con.Open();
            Object Tot = cmdtot.ExecuteScalar();
            //lblQty.Text = Tot.ToString();
            con.Close();

            string query = string.Empty;
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[ExcelEncLive].[SP_CustomerWise]";
            cmd.Parameters.AddWithValue("@OANumber", OAnum);
            cmd.Connection = con;
            con.Open();
            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    //bind the 1st resultset
                    //dgvCustomerWise.EmptyDataText = "No Records Found";
                    //dgvCustomerWise.DataSource = reader;
                    //dgvCustomerWise.DataBind();
                    //dgvCustomerWise.ShowHeader = true;
                    //divdgvCustomerWise.Visible = true;
                    BindDeprt();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
            con.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlCommand com = new SqlCommand())
            {
                com.CommandText = "select DISTINCT cname from Company where " + "cname like @Search + '%'";

                com.Parameters.AddWithValue("@Search", prefixText);
                com.Connection = con;
                con.Open();
                List<string> Qno = new List<string>();
                using (SqlDataReader sdr = com.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        Qno.Add(sdr["cname"].ToString());
                    }
                }
                con.Close();
                return Qno;
            }
        }
    }

    protected void txtCustomerName_TextChanged(object sender, EventArgs e)
    {
        string query = string.Empty;
        FillOAgrid();
    }

    protected void btnresetfilter_Click(object sender, EventArgs e)
    {
        txtCustomerName.Text = string.Empty;
        FillOAgrid();
    }

    protected void ddlDispatchList_TextChanged(object sender, EventArgs e)
    {
        FillOAgrid();
    }

    protected void GvOA_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            Label Label1 = e.Row.FindControl("Label1") as Label;
            LinkButton btnCreate = e.Row.FindControl("btnCreate") as LinkButton;
            LinkButton btnEdit = e.Row.FindControl("btnEdit") as LinkButton;
            LinkButton btnDelete = e.Row.FindControl("btnDelete") as LinkButton;
            //LinkButton btnCloseOa = e.Row.FindControl("btnCloseOa") as LinkButton;

            //changes by shubham wankhade

            Label lblOA = e.Row.FindControl("lbloano") as Label;
            LinkButton btnnmailsend = e.Row.FindControl("lnkmailsend") as LinkButton;

            DataTable Dt1 = new DataTable();
            SqlDataAdapter Sd1 = new SqlDataAdapter("Select IsMail from dbo.OAList  where oano='" + lblOA.Text + "'", con);
            Sd1.Fill(Dt1);
            if (Dt1.Rows.Count > 0)
            {

                object isMailValue = Dt1.Rows[0]["IsMail"];
                if (isMailValue != DBNull.Value && isMailValue != null)
                {

                    btnnmailsend.Enabled = false;
                    btnnmailsend.Text = "Mail Send";
                    btnnmailsend.ForeColor = System.Drawing.Color.Green; // Set text color to green for success
                }
                else
                {

                    btnnmailsend.Visible = true;

                }
            }


            //changes end
            string empcode = Session["empcode"].ToString();
            DataTable Dt = new DataTable();
            SqlDataAdapter Sd = new SqlDataAdapter("Select id from [employees] where [empcode]='" + empcode + "'", con);
            Sd.Fill(Dt);
            if (Dt.Rows.Count > 0)
            {
                string id = Dt.Rows[0]["id"].ToString();
                DataTable Dtt = new DataTable();
                SqlDataAdapter Sdd = new SqlDataAdapter("Select * FROM [ExcelEncLive].tblUserRoleAuthorization where UserID = '" + id + "' AND PageName = 'OrderAcceptanceList.aspx' AND PagesView = '1'", con);
                Sdd.Fill(Dtt);
                if (Dtt.Rows.Count > 0)
                {
                    btnCreate.Visible = false;
                    btnAddOpenOrder.Visible = false;
                    btnQuoList.Visible = false;
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                }
            }
        }
    }

    protected void Bind_SubOaDDl()
    {
        ddlSubOaNo.Items.Clear();
        SqlCommand cmdGetOa = new SqlCommand("select * from[ExcelEncLive]. [vwOrderAccept] where SubOANumber='" + ddlSubOaNo.Text + "'", con);
        con.Open();
        Object Var_OaNumber = cmdGetOa.ExecuteScalar();
        DataTable dtorder = new DataTable();
        SqlDataAdapter sadorder = new SqlDataAdapter("select * from [ExcelEncLive].[vwOrderAccept] where OAno='" + Var_OaNumber + "'", con);
        sadorder.Fill(dtorder);
        //ddlSubOaNo.DataValueField = "SubOANumber";
        ddlSubOaNo.DataTextField = "SubOANumber";
        lblcname.Text = dtorder.Rows[0]["customername"].ToString();
        ddlSubOaNo.DataSource = dtorder;
        ddlSubOaNo.DataBind();
        ddlSubOaNo.Items.Insert(0, new System.Web.UI.WebControls.ListItem("--Select--", "0"));
    }

    protected void ddlSubOaNo_TextChanged(object sender, EventArgs e)
    {
        //divSubOaDetails.Visible = true;
        //divSuboaGrid.Visible = true;
        //con.Close();
        //this.modelprofile.Show();
        //try
        //{
        SqlCommand cmdGetOa = new SqlCommand("select * from [ExcelEncLive].[vwOrderAccept] where SubOANumber='" + ddlSubOaNo.Text + "'", con);
        con.Open();
        Object Var_OaNumber = cmdGetOa.ExecuteScalar();

        //SqlDataAdapter adp = new SqlDataAdapter(@"select DISTINCT(OAno) from [ExcelEncLive].[vwOrderAccept] where OAno='" + Var_OaNumber + "'", con);
        SqlDataAdapter adp = new SqlDataAdapter(@"select DISTINCT(OAno),pono,CONVERT(VARCHAR(10), podate, 105) as Podate from [ExcelEncLive].[vwOrderAccept] where OAno='" + Var_OaNumber + "'", con);
        DataTable dt = new DataTable();
        adp.Fill(dt);
        if (dt.Rows.Count > 0)
        {
            dgvOANumber.DataSource = dt;
            dgvOANumber.DataBind();

        }

        if (!IsPostBack)
        {
            Bind_SubOaDDl();
        }

        SqlDataAdapter adp1 = new SqlDataAdapter(@"select * from [ExcelEncLive].[vwOrderAccept] where SubOANumber='" + ddlSubOaNo.SelectedItem.Text + "'", con);
        DataTable dt1 = new DataTable();
        adp1.Fill(dt1);
        if (dt1.Rows.Count > 0)
        {
            //if (ddlSubOaNo.Text != "--Select--")
            //{
            dgvSubOADetails.DataSource = dt1;
            dgvSubOADetails.DataBind();
            //}
            //else
            //{
            //    dgvSubOADetails.EmptyDataText = "No Records Found";
            //    dgvSubOADetails.DataBind();
            //}
        }

        SqlCommand cmdtot = new SqlCommand("select SUM(CAST(Qty as int)) as Tot from [ExcelEncLive].[vwOrderAccept] where OAno='" + Var_OaNumber + "'", con);
        //con.Open();
        Object Tot = cmdtot.ExecuteScalar();
        //lblQty.Text = Tot.ToString();
        con.Close();

        //    string query = string.Empty;
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.CommandText = "[ExcelEncLive].[SP_SubOawiseDetailsNew]";
        //    cmd.Parameters.AddWithValue("@SubOANumber", ddlSubOaNo.SelectedItem.Text);
        //    cmd.Connection = con;
        //    con.Open();
        //    try
        //    {
        //        using (SqlDataReader reader = cmd.ExecuteReader())
        //        {
        //            //bind the 1st resultset
        //            dgvCustomerWise.EmptyDataText = "No Records Found";
        //            dgvCustomerWise.DataSource = reader;
        //            dgvCustomerWise.DataBind();
        //            dgvCustomerWise.ShowHeader = true;
        //            divdgvCustomerWise.Visible = true;
        //            BindDeprt();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //catch (Exception)
        //{

        //    throw;
        //}

        Getsuboadetails();
    }

    //public void SendMail(string url, string User, string ID, string Usermail)
    //{

    //    try
    //    {

    //        string strMessage = "Hello, This is your quotation PDF.<br /> Please find the attached file.";

    //        MailMessage message = new MailMessage();

    //        // Set up recipient and other details
    //        message.To.Add("erp@weblinkservices.net"); // Add recipient
    //        message.Subject = "Quotation PDF"; // Subject of the email
    //        message.From = new MailAddress("testing@weblinkservices.net"); // From address
    //        message.IsBodyHtml = true; // Mark the body as HTML content
    //        message.Body = strMessage; // Set the body

    //        // Set the "Reply-To" header to indicate the desired reply address
    //        message.ReplyToList.Add(new MailAddress("sales.endeavourautomations@gmail.com"));

    //        // If you want to attach a file (e.g., a PDF):
    //        // Assuming the PDF file is generated and saved to the server
    //        string pdfFilePath = Server.MapPath("~/Files/Quotation.pdf");
    //        if (System.IO.File.Exists(pdfFilePath))
    //        {
    //            Attachment attachment = new Attachment(pdfFilePath);
    //            message.Attachments.Add(attachment); // Add the file as attachment
    //        }

    //        // Set up SMTP client to send the email
    //        SmtpClient smtpMail = new SmtpClient();
    //        smtpMail.Host = "smtpout.secureserver.net"; // SMTP server address (ensure it's correct)
    //        smtpMail.Port = 587; // SMTP Port, usually 587 for TLS
    //        smtpMail.Credentials = new NetworkCredential("testing@weblinkservices.net", "ERP@Testing@123"); // Provide SMTP credentials
    //        smtpMail.EnableSsl = true; // Enable SSL for secure connection
    //        smtpMail.ServicePoint.MaxIdleTime = 0;
    //        smtpMail.ServicePoint.SetTcpKeepAlive(true, 2000, 2000);

    //        // Send the email
    //        smtpMail.Send(message);

    //        // Show success message
    //        ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Mail Sent Successfully!');", true);
    //        Response.Write("Email has been sent");
    //    }
    //    catch (Exception ex)
    //    {
    //        string errorMsg = "An error occurred : " + ex.Message;
    //        ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('" + errorMsg + "') ", true);
    //    }


    //    //try
    //    //{

    //    //    string strMessage = "Hello, " + User + " <br /> Click the link below to track your OA report:<br /><br />";
    //    //    //strMessage += "<a href='http://erp.excelenclosures.net/Admin/" + url + "'>View Report</a>";
    //    //    MailMessage message = new MailMessage();
    //    //    // Add TO recipients
    //    //    //message.To.Add("erp@weblinkservices.net");
    //    //    //message.To.Add("erp@weblinkservices.net");
    //    //    message.To.Add(Usermail);
    //    //    // Add CC recipients
    //    //    string erp = "erp@weblinkservices.net";
    //    //    string Gmail = "Bss@excelenclosures.com";
    //    //    // string Gmail = "erp@weblinkservices.net";
    //    //  //  message.CC.Add(erp);
    //    //   // message.CC.Add(Gmail);

    //    //    message.Subject = "Track Your Order"; // Subject of Email   
    //    //                                          //message.Body = strMessage;
    //    //                                          //message.Body = GetEmailTemplate(User, strMessage, ID);
    //    //    message.Body = GetEmailTemplate(User, strMessage, ID, "http://erp.excelenclosures.net/Admin/" + url);
    //    //    message.From = new System.Net.Mail.MailAddress("testing@weblinkservices.net"); // Email-ID of Sender
    //    //    message.IsBodyHtml = true;

    //    //    SmtpClient SmtpMail = new SmtpClient();
    //    //    SmtpMail.Host = "smtpout.secureserver.net"; // Name or IP-Address of Host used for SMTP transactions  
    //    //    SmtpMail.Port = 587; // Port for sending the mail  
    //    //    SmtpMail.Credentials = new System.Net.NetworkCredential("testing@weblinkservices.net", "ERP@Testing@123"); // Username/password of network, if apply  
    //    //    SmtpMail.DeliveryMethod = SmtpDeliveryMethod.Network;
    //    //    SmtpMail.EnableSsl = false;
    //    //    SmtpMail.ServicePoint.MaxIdleTime = 0;
    //    //    SmtpMail.ServicePoint.SetTcpKeepAlive(true, 2000, 2000);
    //    //    message.BodyEncoding = Encoding.Default;
    //    //    message.Priority = MailPriority.High;
    //    //    SmtpMail.Send(message);
    //    //    ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Mail Send Successfully !!');", true);
    //    //    //Updatemailstaus(ID);

    //    //}
    //    //catch (Exception ex)
    //    //{
    //    //    ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Mail Not send !!');", true);
    //    //    throw ex;
    //    //}


    //}

    //private string GetEmailTemplate(string User, string content, string ID)
    //{
    //    //string template = @"
    //    //        <!DOCTYPE html>
    //    //<html>
    //    //<head>
    //    //    <meta charset=""utf-8"" />
    //    //    <title></title>
    //    //</head>
    //    //<body>
    //    //    <div width=""100%"" style=""min-width:100%!important;margin:0!important;padding:0!important"">
    //    //        <table width=""660"" border=""0"" cellpadding=""0"" cellspacing=""0"" align=""center"">
    //    //            <tbody>
    //    //                <tr>
    //    //                    <td width=""100%"" style=""min-width:100%"">
    //    //                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""display:block"">
    //    //                            <tbody>
    //    //                                <tr>
    //    //                                    <td width=""100%"" align=""center"" style=""display:block;text-align:center;vertical-align:top;font-size:16;min-width:100%;background-color:#edece6"">
    //    //                                        <table align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""display:block;min-width:100%!important"" bgcolor=""#ffffff"">
    //    //                                            <tbody>
    //    //                                                <tr>
    //    //                                                    <td>&nbsp;</td>
    //    //                                                    <td width=""100%"" align=""center"" style=""text-align:center;padding:10px 0px"">
    //    //                                                        <a href=""http://www.weblinkservices.net"" target=""_blank""><img src=""https://www.weblinkservices.net/assets-web/ExcelEncLogo.png"" width=""15%""></a>
    //    //                                                    </td>
    //    //                                                    <td>&nbsp;</td>
    //    //                                                </tr>
    //    //                                            </tbody>
    //    //                                        </table>
    //    //                                        <div style=""overflow:hidden;display:none;font-size:1px;color:#ffffff;line-height:1px;max-height:0px;max-width:0px;opacity:0"">&nbsp;</div>
    //    //                                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""display:block;min-width:100%;background-color:#edece6"">
    //    //                                            <tbody>
    //    //                                                <tr style=""background-color:#1a263a"">
    //    //                                                    <td>&nbsp;</td>
    //    //                                                    <td width=""660"" style=""padding:20px 0px 20px 0px;text-align:center""><a href=""#"" style=""text-decoration:none;font-size:20px;color:#ffffff""> Notification From Excel Enclosures </a></td>
    //    //                                                    <td>&nbsp;</td>
    //    //                                                </tr>
    //    //                                                <tr>
    //    //                                                    <td>&nbsp;</td>
    //    //                                                    <td valign=""middle"" align=""left"" width=""100%"" style=""padding:0px 21px 0px 21px"">&nbsp;</td>
    //    //                                                    <td>&nbsp;</td>
    //    //                                                </tr>
    //    //                                                <tr>
    //    //                                                    <td>&nbsp;</td>
    //    //                                                    <td width=""100%"">
    //    //                                                        <table cellpadding=""0"" cellspacing=""0"" border=""0"" align=""center"" style=""border-bottom:2px solid #b8b8b8; width:90%"" bgcolor=""#ffffff"">
    //    //                                                            <tbody>
    //    //                                                                <tr>
    //    //                                                                    <td width=""100%"" align=""left"" valign=""top"" style=""padding:10px;"">
    //    //                                                                        <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"">
    //    //                                                                            <tbody>
    //    //                                                                                <tr height=""40"">
    //    //                                                                                    <td style=""text-align:center;padding:0px 1px 1px 15px;font-size:14px;color:#333333;line-height:1.4!important;word-wrap:break-word"" valign=""top"">
    //    //                                                                                        <p class=""pdata"" align=""center"" style=""font-size: 16px; text-align: left;"">

    //    //                                                                                            {content}
    //    //                                                                                        </p>

    //    //                                                                                        <br /><br />
    //    //                                                                                        <hr style=""border-top: 1px dashed #333333;"" />
    //    //                                                                                        <p class=""pdata"" align=""center"" style=""font-size: 16px; text-align: left;"">
    //    //                                                                                            NOTE :-  This is a system-generated mail.<br />
    //    //                                                                                            If you find this is not a genuine update then please report to <br /><a href=""mailto:info@weblinkservices.net"">info@weblinkservices.net</a> immediately.
    //    //                                                                                        </p>
    //    //                                                                                    </td>
    //    //                                                                                </tr>
    //    //                                                                            </tbody>
    //    //                                                                        </table>
    //    //                                                                    </td>
    //    //                                                                </tr>
    //    //                                                            </tbody>
    //    //                                                        </table>
    //    //                                                        <br />
    //    //                                                    </td>
    //    //                                                    <td>&nbsp;</td>
    //    //                                                </tr>
    //    //                                            </tbody>
    //    //                                        </table>
    //    //                                        <br />
    //    //                                        <table width=""100%"">
    //    //                                            <tbody>
    //    //                                                <tr>
    //    //                                                    <td width=""100%"" bgcolor=""#1A263A"" style=""padding:0px 30px!important;background-color:#1a263a"">
    //    //                                                        <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""color:#ffffff;text-align:center;font-size:14px"">
    //    //                                                            <tbody>
    //    //                                                                <tr><td height=""24px"">&nbsp;</td></tr>
    //    //                                                                <tr>
    //    //                                                                    <td style=""padding:0px 10px 0px 10px;font-size:14px"">

    //    //                                                                    </td>
    //    //                                                                </tr>
    //    //                                                                <tr>
    //    //                                                                    <td style=""padding:5px 10px 24px 10px;text-decoration:none!important;color:#ffffff!important;font-size:14px"">
    //    //                                                                        <span>Gat No. 1567, Shelar Vasti, Dehu-Alandi Road, Chikhali, Pune - 411062 </span>
    //    //                                                                    </td>
    //    //                                                                </tr>
    //    //                                                            </tbody>
    //    //                                                        </table>
    //    //                                                    </td>
    //    //                                                </tr>
    //    //                                            </tbody>
    //    //                                        </table>
    //    //                                    </td>
    //    //                                </tr>
    //    //                            </tbody>
    //    //                        </table>
    //    //                    </td>
    //    //                </tr>
    //    //            </tbody>
    //    //        </table>
    //    //    </div>
    //    //</body>
    //    //</html>
    //    //    ";



    //    //return template.Replace("{user}", User).Replace("{content}", content);

    //}

    public void SendMail(string url, string User, string ID, string Usermail)
    {
        try
        {

            string strMessage = "Hello, " + User + " <br /> Click the link below to track your OA report:<br /><br />";
            strMessage += "<a href='http://erp.excelenclosures.net/Admin/" + url + "'>View Report</a>";
            MailMessage message = new MailMessage();
            // Add TO recipients
            //message.To.Add("erp@weblinkservices.net");
            //message.To.Add("erp@weblinkservices.net");
            message.To.Add(Usermail);
            // Add CC recipients
            string erp = "erp@weblinkservices.net";
            string Gmail = "Bss@excelenclosures.com";
            // string Gmail = "erp@weblinkservices.net";
            message.CC.Add(erp);
            message.CC.Add(Gmail);

            message.Subject = "Track Your Order"; // Subject of Email   
                                                  //message.Body = strMessage;
                                                  //message.Body = GetEmailTemplate(User, strMessage, ID);
            message.Body = GetEmailTemplate(User, strMessage, ID, "http://erp.excelenclosures.net/Admin/" + url);
            message.From = new System.Net.Mail.MailAddress("erp@weblinkservices.net"); // Email-ID of Sender
            message.IsBodyHtml = true;

            SmtpClient SmtpMail = new SmtpClient();
            SmtpMail.Host = "smtpout.secureserver.net"; // Name or IP-Address of Host used for SMTP transactions  
            SmtpMail.Port = 587; // Port for sending the mail  
            SmtpMail.Credentials = new System.Net.NetworkCredential("testing@weblinkservices.net", "Weblink@Testing#123"); // Username/password of network, if apply  
            SmtpMail.DeliveryMethod = SmtpDeliveryMethod.Network;
            SmtpMail.EnableSsl = true;
            SmtpMail.ServicePoint.MaxIdleTime = 0;
            SmtpMail.ServicePoint.SetTcpKeepAlive(true, 2000, 2000);
            message.BodyEncoding = Encoding.Default;
            message.Priority = MailPriority.High;
            SmtpMail.Send(message);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Mail Send Successfully !!');", true);
            //Updatemailstaus(ID);

        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Mail Not send !!');", true);
            throw ex;
        }


    }

    private string GetEmailTemplate(string user, string content, string id, string link)
    {
        string template = @"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset=""utf-8"" />
            <title></title>
        </head>
        <body>
            <div width=""100%"" style=""min-width:100%!important;margin:0!important;padding:0!important"">
                <table width=""660"" border=""0"" cellpadding=""0"" cellspacing=""0"" align=""center"">
                    <tbody>
                        <tr>
                            <td width=""100%"" style=""min-width:100%"">
                                <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""display:block"">
                                    <tbody>
                                        <tr>
                                            <td width=""100%"" align=""center"" style=""display:block;text-align:center;vertical-align:top;font-size:16;min-width:100%;background-color:#edece6"">
                                                <table align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""display:block;min-width:100%!important"" bgcolor=""#ffffff"">
                                                    <tbody>
                                                        <tr>
                                                            <td>&nbsp;</td>
                                                            <td width=""100%"" align=""center"" style=""text-align:center;padding:10px 0px"">
                                                                <a href=""http://excelenclosures.net/"" target=""_blank""><img src=""https://www.weblinkservices.net/assets-web/ExcelEncLogo.png"" width=""20%""></a>
                                                            </td>
                                                            <td>&nbsp;</td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                                <div style=""overflow:hidden;display:none;font-size:1px;color:#ffffff;line-height:1px;max-height:0px;max-width:0px;opacity:0"">&nbsp;</div>
                                                <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""display:block;min-width:100%;background-color:#edece6"">
                                                    <tbody>
                                                        <tr style=""background-color:#1a263a"">
                                                            <td>&nbsp;</td>
                                                            <td width=""660"" style=""padding:20px 0px 20px 0px;text-align:center""><a href=""#"" style=""text-decoration:none;font-size:20px;color:#ffffff""> Notification From Excel Enclosures </a></td>
                                                            <td>&nbsp;</td>
                                                        </tr>
                                                        <tr>
                                                            <td>&nbsp;</td>
                                                            <td valign=""middle"" align=""left"" width=""100%"" style=""padding:0px 21px 0px 21px"">&nbsp;</td>
                                                            <td>&nbsp;</td>
                                                        </tr>
                                                        <tr>
                                                            <td>&nbsp;</td>
                                                            <td width=""100%"">
                                                                <table cellpadding=""0"" cellspacing=""0"" border=""0"" align=""center"" style=""border-bottom:2px solid #b8b8b8; width:90%"" bgcolor=""#ffffff"">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td width=""100%"" align=""left"" valign=""top"" style=""padding:10px;"">
                                                                                <table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"">
                                                                                    <tbody>
                                                                                        <tr height=""40"">
                                                                                            <td style=""text-align:center;padding:0px 1px 1px 15px;font-size:14px;color:#333333;line-height:1.4!important;word-wrap:break-word"" valign=""top"">
                                                                                                <p class=""pdata"" align=""center"" style=""font-size: 16px; text-align: left; color:black;"">
                                                                                                    Dear: <strong>{user}</strong>,
                                                                                                </p>
                                                                                                <p class=""pdata"" align=""center"" style=""font-size: 16px; text-align: left;"">
                                                                                                   We hope this message finds you well. Thank you for choosing Excel Enclosures for your recent purchase. We're happy to provide you with an update.
                                                                                                </p>
                                                                                                <p class=""pdata"" align=""center"" style=""font-size: 16px; text-align: left;"">
                                                                                                    Your order, with the reference number <strong>{ID}</strong>, is currently in the production stage.
                                                                                                </p>
                                                                                                <p class=""pdata"" align=""center"" style=""font-size: 16px; text-align: left;"">
                                                                                                    To help you track the progress of your order, <a href=""{link}"" style=""color: #1a263a; text-decoration: underline;""><strong>Click Here</strong></a>.
                                                                                                </p>
                                                                                                   <p class=""pdata"" align=""center"" style=""font-size: 16px; text-align: left;"">
                                                                                                    We appreciate your patience and trust in Excel Enclosures. Thank you for being a valued customer.
                                                                                                </p>
                                                                                                <p class=""pdata"" align=""center"" style=""font-size: 16px; text-align: left;"">
                                                                                                    Best regards, <br />
                                                                                                    Excel Enclosures.
                                                                                                </p>
                                                                                            </td>
                                                                                        </tr>
                                                                                    </tbody>
                                                                                </table>
                                                                            </td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                                <br />
                                                            </td>
                                                            <td>&nbsp;</td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                                <br />
                                                <table width=""100%"">
                                                    <tbody>
                                                        <tr>
                                                            <td width=""100%"" bgcolor=""#1A263A"" style=""padding:0px 30px!important;background-color:#1a263a"">
                                                                <table width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""color:#ffffff;text-align:center;font-size:14px"">
                                                                    <tbody>
                                                                        <tr><td height=""24px"">&nbsp;</td></tr>
                                                                        <tr>
                                                                            <td style=""padding:0px 10px 0px 10px;font-size:14px"">
                                                                                <!-- Add any additional content or links here -->
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td style=""padding:5px 10px 24px 10px;text-decoration:none!important;color:#ffffff!important;font-size:14px"">
                                                                                <span>Gat No. 1567, Shelar Vasti, Dehu-Alandi Road, Chikhali, Pune - 411062 </span>
                                                                            </td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </body>
        </html>
    ";

        return template.Replace("{user}", user).Replace("{content}", content).Replace("{ID}", id).Replace("{link}", link);
    }

    public void Updatemailstaus(string ID)
    {

        SqlCommand cmd = new SqlCommand("Update dbo.OAList set IsMail = 1  where oano ='" + ID + "'", con);
        con.Open();
        Thread.Sleep(1000);
        cmd.ExecuteNonQuery();
        con.Close();
    }

    public void Getsuboadetails()
    {
        try
        {
            string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("[ExcelEncLive].[SP_SubOawiseDetailsNew]", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SubOANumber", ddlSubOaNo.SelectedItem.Text);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable Dt = new DataTable();
                    adapter.Fill(Dt);
                    if (Dt.Rows.Count > 0)
                    {
                        //divdgvCustomerWise.Visible = true;
                        this.modelprofile.Show();
                        div1.Visible = true;
                        divSubOaDetails.Visible = true;
                        divSuboaGrid.Visible = true;
                        GVSuboadetails.DataSource = Dt;
                        GVSuboadetails.DataBind();

                    }
                    else
                    {
                        GVSuboadetails.EmptyDataText = "<span class='empty-data-message'>No records found.</span>";
                        GVSuboadetails.DataBind();
                    }

                }
            }
        }
        catch (Exception ex)
        {

            //throw;
            string errorMsg = "An error occurred : " + ex.Message;
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('" + errorMsg + "');", true);
        }
    }


    public void DeletedRecord(string ID)

    {
        try
        {
            string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("[InsertAllDeletedData]", connection))
                {
                    
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OAno", ID);
                    cmd.Parameters.AddWithValue("@DeletedBy", Session["name"].ToString());
                    cmd.ExecuteNonQuery();
                    con.Close();

                }
            }
        }
        catch (Exception ex)
        {

            //throw;
            string errorMsg = "An error occurred : " + ex.Message;
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('" + errorMsg + "');", true);
        }
    }
	
	  protected void GVSuboadetails_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            // Find the Label control in the GridView row
            Label lblDate = e.Row.FindControl("txtOutwardDt") as Label;

            if (lblDate != null)
            {
                // Retrieve the value
                object dataValue = DataBinder.Eval(e.Row.DataItem, "InwardDttime");

                DateTime inwardDate;

                if (dataValue != null && dataValue is DateTime)
                {
                    // If the value is already a DateTime
                    inwardDate = (DateTime)dataValue;
                    lblDate.Text = inwardDate.ToString("dd-MM-yyyy");
                }
                else if (dataValue != null && dataValue is string)
                {
                    // Handle string values
                    string dateString = (string)dataValue;

                    // Try parsing the string into DateTime with multiple formats
                    //string[] formats = { "yyyy-MM-dd HH:mm:ss", "MM/dd/yyyy h:mm:ss tt", "yyyy-MM-dd H:mm:ss", "yyyy-MM-dd HH:mm:ss.FFF" };

					string[] formats = { "yyyy-MM-dd HH:mm:ss", "MM/dd/yyyy h:mm:ss tt", "yyyy-MM-dd H:mm:ss", "yyyy-MM-dd HH:mm:ss.FFF", "dd-MM-yyyy hh:mm tt", };
					
                    if (DateTime.TryParseExact(dateString, formats,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out inwardDate))
                    {
                        lblDate.Text = inwardDate.ToString("dd-MM-yyyy HH:mm:ss");
                    }
                    else
                    {
                        // Handle invalid formats
                        lblDate.Text = "Invalid Date";
                    }
                }
                else
                {
                    // Handle null or other unexpected types
                    lblDate.Text = "Invalid Date";
                }
            }
        }



    }

}
