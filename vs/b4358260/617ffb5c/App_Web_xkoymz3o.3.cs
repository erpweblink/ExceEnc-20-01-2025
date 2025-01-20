﻿#pragma checksum "E:\Ashish\User Autorization Excel Enc 21-11-22\Admin\Drawing.aspx.cs" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "D5D6D23302505A70F1CC3F23CFE70569785B99A6"

#line 1 "E:\Ashish\User Autorization Excel Enc 21-11-22\Admin\Drawing.aspx.cs"
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;


public partial class Admin_Drawing : System.Web.UI.Page
{
    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
    DataTable dt = new DataTable();
    CommonCls objClass = new CommonCls();

    DataTable tempdt = new DataTable();
   
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.IsPostBack)
        {
            //DrawingDDLbind();
            GetDrawingData();
        }
    }

    protected void GetDrawingData()
    {
        try
        {
            string query = string.Empty;
            //            query = @"SELECT [VWID],[id] as mainID, [OAno],[currentdate],[customername],[deliverydatereqbycust],[IsDrawingcomplete],[Description],[Qty],[Price],[Discount]
            //,[TotalAmount],[CGST],[SGST],[IGST],[SubOANumber] FROM vwOrderAccept where IsComplete is null order by deliverydatereqbycust asc";

            query = @"SELECT [pono],[id] as mainID,[OANumber],[Size],[TotalQty],[InwardDtTime],[InwardQty],[deliverydatereqbycust],[customername],SubOA
               FROM vwDrawerCreation where IsComplete is null order by deliverydatereqbycust asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                dgvDrawing.DataSource = dt;
                dgvDrawing.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + dgvDrawing.ClientID + "', 900, 1020 , 40 ,true); </script>", false);
            }
            else
            {
                dgvDrawing.DataSource = null;
                dgvDrawing.DataBind();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Pending Record Not Found..!');", true);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void GetSelectedRecords(object sender, EventArgs e)
    {
        string vwID = "";
        string confirmValue = Request.Form["confirm_value"];
        if (confirmValue == "Yes")
        {

            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[12] { new DataColumn("OAnumber"), new DataColumn("SubOA"), new DataColumn("customername"), new DataColumn("size"), new DataColumn("totalinward"), new DataColumn("inwarddatetime"), new DataColumn("inwardqty"), new DataColumn("outwarddatetime"), new DataColumn("outwardqty"), new DataColumn("deliverydate"), new DataColumn("remark"), new DataColumn("Isapprove") });

            tempdt.Columns.AddRange(new DataColumn[11] { new DataColumn("OAnumber"),
                                new DataColumn("SubOA"),
                                new DataColumn("customername"),
                                new DataColumn("size"),
                                new DataColumn("totalinward"),
                                new DataColumn("inwarddatetime"),
                                new DataColumn("inwardqty"),
                                new DataColumn("outwarddatetime"),
                                new DataColumn("outwardqty"),
                                new DataColumn("deliverydate"),
                                new DataColumn("Isapprove") });
            foreach (GridViewRow row in dgvDrawing.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    CheckBox chkRow = (row.Cells[1].FindControl("chkRow") as CheckBox);
                    int totalCount = dgvDrawing.Rows.Cast<GridViewRow>().Count(r => ((CheckBox)r.FindControl("chkRow")).Checked);
                    if (totalCount <= 0)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Please Select Atleast One Row..!!');", true);
                    }
                    else
                    {
                        if (chkRow.Checked)
                        {
                            //string lblVWID = (row.Cells[1].FindControl("lblVWID") as Label).Text;
                            //vwID = lblVWID;
                            string OANumber = (row.Cells[1].FindControl("lblOANumber") as Label).Text;

                            string SubOA = (row.Cells[1].FindControl("lblSubOANumber") as Label).Text;

                            //string CustName = (row.Cells[1].FindControl("lblCustName") as Label).Text;
                            TextBox Custtb = (TextBox)row.Cells[1].FindControl("lblCustName");
                            string CustName = Custtb.Text;
                            string TotalQty = (row.Cells[1].FindControl("lblTotalQty") as Label).Text;
                            string DeliveryDt = (row.Cells[1].FindControl("lblDeliveryDt") as Label).Text;

                            string InwardDtTime = (row.Cells[1].FindControl("lblInwardDtTime") as Label).Text;
                            TextBox tb = (TextBox)row.Cells[1].FindControl("txtInwardQty");
                            string InwardQty = tb.Text;

                            //string OutwardDtTime = (row.Cells[1].FindControl("lblOutwardDtTime") as Label).Text;

                            TextBox tbOutwardDt = (TextBox)row.Cells[1].FindControl("txtOutwardDtTime");
                            //DateTime OutwardDtT = DateTime.Parse(tbOutwardDt.Text);
                            string time = DateTime.Now.ToString("h:mm tt");
                            string OutwardDtTime = tbOutwardDt.Text + " " + time;

                            TextBox Outwardtb = (TextBox)row.Cells[1].FindControl("txtOutwardQty");

                            string[] strarr = Outwardtb.Text.Split(',');

                            string OutwardQty = strarr[1].ToString();

                            //Label Sizetb = (Label)row.Cells[1].FindControl("txtSize");
                            //string Size = Sizetb.Text;

                            TextBox Sizetb = (TextBox)row.Cells[1].FindControl("txtSize");
                            string Size = Sizetb.Text;

                            TextBox Remarktb = (TextBox)row.Cells[1].FindControl("txtRemark");
                            string Remark = Remarktb.Text;


                            //string Size = strsi.Replace("<br><br><br>", " ");
                            dt.Rows.Add(OANumber, SubOA, CustName, Size, TotalQty, InwardDtTime, InwardQty, OutwardDtTime, OutwardQty, DeliveryDt, Remark, true);
                        }
                    }
                }
            }

            using (con)
            {

                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    bool IsApprove = true, IsPending = false, IsCancel = false, Iscomplete;
                    string CreatedBy = Session["ProductionName"].ToString(), UpdatedBy = "";
                    string UpdatedDate = DateTime.Now.ToShortDateString(), CreatedDate = DateTime.Now.ToShortDateString();
                    bool flgInsert = false;
                    foreach (DataRow row in dt.Rows)
                    {
                        con.Open();
                        if (row["inwardqty"].ToString() == row["outwardqty"].ToString())
                        {
                            Iscomplete = true;
                        }
                        else
                        {
                            Iscomplete = false;
                        }

                        SqlCommand cmdexsist = new SqlCommand("select OANumber,InwardQty,OutwardQty,SubOA from tblLaserPrograming WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                        string OanumberExsists = "", InwardQty = "", OutwardQty = "";
                        using (SqlDataReader dr = cmdexsist.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                OanumberExsists = dr["SubOA"].ToString();
                                InwardQty = dr["InwardQty"].ToString();
                                OutwardQty = dr["OutwardQty"].ToString();
                            }
                        }

                        SqlCommand cmd2 = new SqlCommand("select OutwardQty from tblDrawing WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                        string Outward2Qty = "";
                        using (SqlDataReader dr = cmd2.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                Outward2Qty = dr["OutwardQty"].ToString();
                            }
                        }

                        if (OanumberExsists == "")
                        {
                           
                            tempdt.Rows.Add(row["OAnumber"].ToString(),
                                row["SubOA"].ToString(),
                                row["customername"].ToString(),
                                row["size"].ToString(),
                                row["totalinward"].ToString(),
                                DateTime.Now,
                                row["outwardqty"].ToString(),
                                DateTime.Now,
                                row["outwardqty"].ToString(),
                                row["deliverydate"].ToString(),
                                 true);

                            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                            {
                                //Set the database table name
                                sqlBulkCopy.DestinationTableName = "dbo.tblLaserPrograming";
                                sqlBulkCopy.ColumnMappings.Add("OAnumber", "OANumber");
                                sqlBulkCopy.ColumnMappings.Add("SubOA", "SubOA");
                                sqlBulkCopy.ColumnMappings.Add("customername", "CustomerName");
                                sqlBulkCopy.ColumnMappings.Add("size", "Size");
                                sqlBulkCopy.ColumnMappings.Add("totalinward", "TotalQty");
                                sqlBulkCopy.ColumnMappings.Add("inwarddatetime", "InwardDtTime");
                                sqlBulkCopy.ColumnMappings.Add("inwardqty", "InwardQty");
                                sqlBulkCopy.ColumnMappings.Add("outwarddatetime", "OutwardDtTime");
                                sqlBulkCopy.ColumnMappings.Add("outwardqty", "OutwardQty");
                                sqlBulkCopy.ColumnMappings.Add("deliverydate", "DeliveryDate");
                                sqlBulkCopy.ColumnMappings.Add("Isapprove", "IsApprove");
                                sqlBulkCopy.WriteToServer(tempdt);

                                tempdt.Clear();
                            }


                            //    cmd.CommandText += "INSERT INTO [dbo].[tblLaserPrograming]([OANumber],[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],[DeliveryDate],[IsApprove],[IsPending],[IsCancel],[CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[SubOA])" +
                            //"VALUES('" + row["OAnumber"].ToString() + "','" + row["customername"].ToString() + "'," +
                            //"'" + row["size"].ToString() + "','" + row["totalinward"].ToString() + "'," +
                            //"'" + row["outwarddatetime"].ToString() + "','" + row["outwardqty"].ToString() + "'," +
                            //"'" + row["outwarddatetime"].ToString() + "','0'," +
                            //"'" + row["deliverydate"].ToString() + "','" + IsApprove + "','" + IsPending + "','" + IsCancel + "'," +
                            //"'" + CreatedBy + "','" + CreatedDate + "','" + UpdatedBy + "','','" + row["SubOA"].ToString() + "'); ";
                            //    cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            int totOutwardqnt = Convert.ToInt32(InwardQty) + Convert.ToInt32(row["outwardqty"].ToString());
                            SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblLaserPrograming] SET [InwardQty] = '" + totOutwardqnt.ToString() + "',[InwardDtTime]='" + row["outwarddatetime"].ToString() + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                            cmdupdate.ExecuteNonQuery();
                        }

                        if (row["inwardqty"].ToString() == row["outwardqty"].ToString())
                        {
                            SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblDrawing] SET [OutwardQty] = '" + row["totalinward"].ToString() + "',[InwardQty]='0',[IsComplete]=1,UpdatedDate='" + UpdatedDate + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                            cmdupdate.ExecuteNonQuery();
                        }
                        else
                        {
                            int totoutward;
                            int inwardqy;
                            if (Outward2Qty == "")
                            {
                                Outward2Qty = "0";
                                inwardqy = Convert.ToInt32(row["inwardqty"].ToString()) - Convert.ToInt32(row["outwardqty"].ToString());
                                totoutward = Convert.ToInt32(Outward2Qty) + Convert.ToInt32(row["outwardqty"].ToString());
                            }
                            else
                            {
                                inwardqy = Convert.ToInt32(row["inwardqty"].ToString()) - Convert.ToInt32(row["outwardqty"].ToString());
                                totoutward = Convert.ToInt32(Outward2Qty) + Convert.ToInt32(row["outwardqty"].ToString());
                            }

                            SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblDrawing] SET [InwardQty] = '" + inwardqy.ToString() + "', [OutwardQty] = '" + totoutward.ToString() + "',UpdatedDate='" + UpdatedDate + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                            cmdupdate.ExecuteNonQuery();
                        }
                        con.Close();
                    }

                    if (flgInsert == true)
                    {

                    }


                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully- Approved and send to Laser Programming Department...!');window.location.href='Drawing.aspx';", true);
                   // Response.Redirect("Drawing.aspx");
                }
            }
        }
        else
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Cancelled Successfully..!')", true);
        }
    }

    protected void ddlONumber_SelectedIndexChanged(object sender, EventArgs e)
    {
        GetDrawingData();
        GvOAFileData.DataSource = null;
        GvOAFileData.DataBind();
    }

    protected void txtOANumber_TextChanged(object sender, EventArgs e)
    {
        GetDrawingData();
        //ddlONumber.Enabled = false;
        GvOAFileData.DataSource = null;
        GvOAFileData.DataBind();

    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        Resetdata();
    }

    protected void Resetdata()
    {
        dgvDrawing.DataSource = null;
        dgvDrawing.DataBind(); GvOAFileData.DataSource = null; GvOAFileData.DataBind(); btnGetSelected.Visible = false;
        //Response.Redirect("Drawing.aspx");
    }

    protected void dgvDrawing_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string OAid = e.CommandArgument.ToString();
        if (e.CommandName == "rowDisplay")
        {
            Gvbind(OAid);
        }
        else
        {
            string url = "../Reports/RptDrawing.aspx?OANumber=" + objClass.encrypt(OAid);
            Page.ClientScript.RegisterStartupScript(GetType(), "", "window.open('" + url + "','','width=700px,height=600px');", true);
        }
    }

    protected void GvOAFileData_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            Label file1 = e.Row.FindControl("lblfilepath1") as Label;
            Label file2 = e.Row.FindControl("lblfilepath2") as Label;
            Label file3 = e.Row.FindControl("lblfilepath3") as Label;
            Label file4 = e.Row.FindControl("lblfilepath4") as Label;
            Label file5 = e.Row.FindControl("lblfilepath5") as Label;

            ImageButton ImageButtonfile1 = e.Row.FindControl("ImageButtonfile1") as ImageButton;
            ImageButton ImageButtonfile2 = e.Row.FindControl("ImageButtonfile2") as ImageButton;
            ImageButton ImageButtonfile3 = e.Row.FindControl("ImageButtonfile3") as ImageButton;
            ImageButton ImageButtonfile4 = e.Row.FindControl("ImageButtonfile4") as ImageButton;
            ImageButton ImageButtonfile5 = e.Row.FindControl("ImageButtonfile5") as ImageButton;

            if (string.IsNullOrEmpty(file1.Text))
            {
                ImageButtonfile1.Enabled = false;
                ImageButtonfile1.ToolTip = "File Not Available";
            }
            if (string.IsNullOrEmpty(file2.Text))
            {
                ImageButtonfile2.Enabled = false;
                ImageButtonfile2.ToolTip = "File Not Available";
            }
            if (string.IsNullOrEmpty(file3.Text))
            {
                ImageButtonfile3.Enabled = false;
                ImageButtonfile3.ToolTip = "File Not Available";
            }
            if (string.IsNullOrEmpty(file4.Text))
            {
                ImageButtonfile4.Enabled = false;
                ImageButtonfile4.ToolTip = "File Not Available";
            }
            if (string.IsNullOrEmpty(file5.Text))
            {
                ImageButtonfile5.Enabled = false;
                ImageButtonfile5.ToolTip = "File Not Available";
            }
        }
    }

    private void Gvbind(string OAid)
    {
        string query = string.Empty;
        if (OAid != "")
        {
            query = @"SELECT [DocID],[OAid],[File1],[File2],[File3],[File4],[File5],[CeatedDate],[CreatedBy]FROM tblOAFiledata where OAid='" + OAid + "'";
        }
        else
        {
            lblnodatafoundComp.Text = "OA Id not found...!! ";
        }
        SqlDataAdapter ad = new SqlDataAdapter(query, con);
        DataTable dt = new DataTable();
        ad.Fill(dt);
        if (dt.Rows.Count > 0)
        {
            GvOAFileData.DataSource = dt;
            GvOAFileData.DataBind();
            //lblnodatafoundComp.Visible = false;
            //btnClose.Visible = true;
        }
        else
        {
            GvOAFileData.DataSource = null;
            GvOAFileData.DataBind();
            lblnodatafoundComp.Text = "Attachment Not Found...!! ";
            lblnodatafoundComp.Visible = true;
            lblnodatafoundComp.ForeColor = Color.Red;
            btnClose.Visible = false;
        }
    }

    public void Display(string id, string fileNo)
    {
        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                string CmdText = string.Empty;
                if (fileNo == "1")
                {
                    CmdText = "SELECT '../'+[File1] as Path FROM tblOAFiledata where OAid='" + id + "'";
                }
                if (fileNo == "2")
                {
                    CmdText = "SELECT '../'+[File2] as Path FROM tblOAFiledata where OAid='" + id + "'";
                }
                if (fileNo == "3")
                {
                    CmdText = "SELECT '../'+[File3] as Path FROM tblOAFiledata where OAid='" + id + "'";
                }
                if (fileNo == "4")
                {
                    CmdText = "SELECT '../'+[File4] as Path FROM tblOAFiledata where OAid='" + id + "'";
                }
                if (fileNo == "5")
                {
                    CmdText = "SELECT '../'+[File5] as Path FROM tblOAFiledata where OAid='" + id + "'";
                }

                SqlDataAdapter ad = new SqlDataAdapter(CmdText, con);
                DataTable dt = new DataTable();
                ad.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    //Response.Write(dt.Rows[0]["Path"].ToString());
                    if (!string.IsNullOrEmpty(dt.Rows[0]["Path"].ToString()))
                    {
                        string Patha = dt.Rows[0]["Path"].ToString();

                        string[] msgfilename = Patha.Split('.');
                        string filenameExt = msgfilename[3];
                        if (filenameExt == "msg")
                        {
                        }
                        else
                        {
                            Response.Redirect(Patha);
                        }
                    }
                    else
                    {
                        lblnodatafoundComp.Text = "File Not Found or Not Available !!";
                    }
                }
                else
                {
                    lblnodatafoundComp.Text = "File Not Found or Not Available !!";
                }
            }
        }
    }

    protected void ImageButtonfile1_Click(object sender, ImageClickEventArgs e)
    {
        string id = ((sender as ImageButton).CommandArgument).ToString();
        Display(id, "1");
    }

    protected void ImageButtonfile2_Click(object sender, ImageClickEventArgs e)
    {
        string id = ((sender as ImageButton).CommandArgument).ToString();
        Display(id, "2");
    }

    protected void ImageButtonfile3_Click(object sender, ImageClickEventArgs e)
    {
        string id = ((sender as ImageButton).CommandArgument).ToString();
        Display(id, "3");
    }

    protected void ImageButtonfile4_Click(object sender, ImageClickEventArgs e)
    {
        string id = ((sender as ImageButton).CommandArgument).ToString();
        Display(id, "4");
    }

    protected void ImageButtonfile5_Click(object sender, ImageClickEventArgs e)
    {
        string id = ((sender as ImageButton).CommandArgument).ToString();
        Display(id, "5");
    }

    protected void chkRow_CheckedChanged(object sender, EventArgs e)
    {
        foreach (GridViewRow row in dgvDrawing.Rows)
        {
            if (row.RowType == DataControlRowType.DataRow)
            {
                CheckBox chkRow = (row.Cells[1].FindControl("chkRow") as CheckBox);
                int totalCount = dgvDrawing.Rows.Cast<GridViewRow>().Count(r => ((CheckBox)r.FindControl("chkRow")).Checked);
                TextBox remarktb = (row.Cells[1].FindControl("txtRemark") as TextBox);
                string Remark = remarktb.Text;
                if (chkRow.Checked == true)
                {
                    if (totalCount > 0)
                    {
                        if (Remark == "")
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Please Enter Remarks..!')", true);
                            chkRow.Checked = false;
                            remarktb.Focus();
                        }
                        else
                        {
                            btnGetSelected.Visible = true;
                        }
                    }
                    else
                    {
                        btnGetSelected.Visible = false;
                    }
                }
            }
        }
    }

    protected void btnClose_Click(object sender, EventArgs e)
    {
        GvOAFileData.DataSource = null;
        GvOAFileData.DataBind();
        btnClose.Visible = false;
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

    //public string Between(string STR, string FirstString, string LastString)
    //{
    //    string FinalString;
    //    int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
    //    int Pos2 = STR.IndexOf(LastString);
    //    FinalString = STR.Substring(Pos1, Pos2 - Pos1);
    //    return FinalString;
    //}


    protected void DisplayPop(object sender, EventArgs e)
    {
        int rowIndex = Convert.ToInt32(((sender as LinkButton).NamingContainer as GridViewRow).RowIndex);
        GridViewRow row = dgvDrawing.Rows[rowIndex];

        lblCustNm.Text = (row.FindControl("lblCustName") as Label).Text;
        lblTotalQnty.Text = (row.FindControl("lblTotalQty") as Label).Text;
        lblOAno.Text = (row.FindControl("lblOANumber") as Label).Text;
        lblDescriptionSize.Text = (row.FindControl("txtSize") as TextBox).Text; ;
        lblDeliveryDate.Text = (row.FindControl("lblDeliveryDt") as Label).Text;
        lblInwardDtandTime.Text = (row.FindControl("lblInwardDtTime") as Label).Text;

        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Pop", "openModal();", true);
    }

    protected void dgvDrawing_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            string Id = dgvDrawing.DataKeys[e.Row.RowIndex].Value.ToString();
            GridView gvDetails = e.Row.FindControl("gvDetails") as GridView;
            GridView gvOADetails = e.Row.FindControl("GvOAFileData") as GridView;

            con.Open();
            SqlCommand cmd = new SqlCommand("select id from [vwDrawerCreation] where SubOA='" + Id + "'", con);
            Object Id_data = cmd.ExecuteScalar();
            con.Close();


            gvDetails.DataSource = GetData(string.Format("select * from vwOrderAccept where [SubOANumber]='{0}'", Id));
            gvDetails.DataBind();

            gvOADetails.DataSource = GetData(string.Format("SELECT [DocID],[OAid],[File1],[File2],[File3],[File4],[File5],[CeatedDate],[CreatedBy]FROM tblOAFiledata where OAid='{0}'", Id_data.ToString()));
            gvOADetails.DataBind();

        }
    }
    private static DataTable GetData(string query)
    {
        string strConnString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        using (SqlConnection con = new SqlConnection(strConnString))
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = query;
                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    cmd.Connection = con;
                    sda.SelectCommand = cmd;
                    using (DataSet ds = new DataSet())
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        return dt;
                    }
                }
            }
        }
    }

    protected void txtOutwardQty_TextChanged(object sender, EventArgs e)
    {
        ViewState["Iscomplete"] = "1";
        GridViewRow row = (sender as TextBox).NamingContainer as GridViewRow;
        //calculationA(row);
    }

    [WebMethod]
    public static string OnSubmit(string RowIndex, string InwardQty, string OutwardQty)
    {
        //calculationA(RowIndex,InwardQty,OutwardQty);
        return "it worked";
    }
	protected void btnPrintData_Click(object sender, EventArgs e)
    {
        try
        {
            //string URL = "PDFShow.aspx?Name=Drawing";
            //string modified_URL = "window.open('" + URL + "', '_blank');";
            //ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", modified_URL, true);
			 Response.Redirect("PDFShow.aspx?Name=Drawing");
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

}

#line default
#line hidden
