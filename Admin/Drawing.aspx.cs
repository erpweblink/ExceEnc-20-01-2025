﻿using System;
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
    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);

    DataTable dt = new DataTable();
    CommonCls objClass = new CommonCls();

    DataTable tempdt = new DataTable();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["name"] == null)
        {
            Response.Redirect("../Login.aspx");
        }
        else
        {
            if (!this.IsPostBack)
            {
                //DrawingDDLbind();
                GetDrawingData();
                ViewAuthorization();

            }
        }
    }

    private void ViewAuthorization()
    {
        string empcode = Session["empcode"].ToString();
        DataTable Dt = new DataTable();
        SqlDataAdapter Sd = new SqlDataAdapter("Select id from [employees] where [empcode]='" + empcode + "'", con);
        Sd.Fill(Dt);
        if (Dt.Rows.Count > 0)
        {
            string id = Dt.Rows[0]["id"].ToString();
            DataTable Dtt = new DataTable();
            SqlDataAdapter Sdd = new SqlDataAdapter("Select * FROM  [ExcelEncLive].tblUserRoleAuthorization where UserID = '" + id + "' AND PageName = 'Drawing.aspx' AND PagesView = '1'", con);
            Sdd.Fill(Dtt);
            if (Dtt.Rows.Count > 0)
            {
                dgvDrawing.Columns[1].Visible = false;
                btnPrintData.Visible = false;
            }
        }
    }

    protected void GetDrawingData()
    {
        try
        {
            string query = string.Empty;
            //            query = @"SELECT [VWID],[id] as mainID, [OAno],[currentdate],[customername],[deliverydatereqbycust],[IsDrawingcomplete],[Description],[Qty],[Price],[Discount]
            //,[TotalAmount],[CGST],[SGST],[IGST],[SubOANumber] FROM vwOrderAccept where IsComplete is null order by deliverydatereqbycust asc";

            query = @"SELECT [pono],[id] as mainID,[OANumber],[Size],[TotalQty],OACreationDate,[InwardDtTime],[InwardQty],[deliverydatereqbycust],[customername],SubOA
               FROM [ExcelEncLive].[vwDrawerCreation] where IsComplete is null order by deliverydatereqbycust asc";

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
        if (Session["OneTimeFlag"] == null || Session["OneTimeFlag"].ToString() == "")
        {
            Session["OneTimeFlag"] = "Inserted";




            string vwID = "";
            //string confirmValue = Request.Form["confirm_value"];
            string confirmValue = "Yes";
            if (confirmValue == "Yes")
            {

                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[12] { new DataColumn("OAnumber"), new DataColumn("SubOA"), new DataColumn("customername"), new DataColumn("size"), new DataColumn("totalinward"), new DataColumn("inwarddatetime"), new DataColumn("inwardqty"), new DataColumn("outwarddatetime"), new DataColumn("outwardqty"), new DataColumn("deliverydate"), new DataColumn("remark"), new DataColumn("Isapprove") });

                tempdt.Columns.AddRange(new DataColumn[9] { new DataColumn("OAnumber"),
                                new DataColumn("SubOA"),
                                new DataColumn("customername"),
                                new DataColumn("size"),
                                new DataColumn("totalinward"),
                                new DataColumn("inwarddatetime"),
                                new DataColumn("inwardqty"),
                                //new DataColumn("outwarddatetime"),
                                //new DataColumn("outwardqty"),
                                new DataColumn("deliverydate"),
                                new DataColumn("Isapprove")
            });
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
                                TextBox tbOutwardDt = (TextBox)row.Cells[1].FindControl("txtOutwardDtTimel");
                                //DateTime OutwardDtT = DateTime.Parse(tbOutwardDt.Text);

                                string OutwardDtTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");
                                //string OutwardDtTime = tbOutwardDt.Text + " " + time;
                                TextBox Outwardtb = (TextBox)row.Cells[1].FindControl("txtOutwardQty");
                                string[] strarr = Outwardtb.Text.Split(',');
                                string OutwardQty = strarr[1].ToString();

                                //  Label Sizetb = (Label)row.Cells[1].FindControl("txtSize");
                                //  string Size = Sizetb.Text;

                                TextBox Sizetb = (TextBox)row.Cells[1].FindControl("txtSize");
                                string Size = Sizetb.Text;

                                TextBox Remarktb = (TextBox)row.Cells[1].FindControl("txtRemark");
                                string Remark = Remarktb.Text;

                                TextBox txtQty1 = (TextBox)row.Cells[1].FindControl("txtInwardQty");
                                TextBox txtQty2 = (TextBox)row.Cells[1].FindControl("txtOutwardQty");
                                string outwardQtyText = txtQty2.Text;
                                string[] qty2Values = outwardQtyText.Split(',');
                                string lastQty2Value = qty2Values[qty2Values.Length - 1];
                                int qty1 = Convert.ToInt32(txtQty1.Text);
                                int qty2 = Convert.ToInt32(lastQty2Value);
                                //  if (qty1 < qty2)  original
                                if (qty1 >= qty2)
                                {
                                    //string Size = strsi.Replace("<br><br><br>", " ");
                                    dt.Rows.Add(OANumber, SubOA, CustName, Size, TotalQty, InwardDtTime, InwardQty, OutwardDtTime, OutwardQty, DeliveryDt, Remark, true);
                                }
                                else
                                {
                                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Alert!!!!!- Outward Quantity Is Greater Than Inward Quantity...!');window.location.href='Drawing.aspx';", true);
                                    return;
                                }


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
                        string CreatedBy = Session["name"].ToString(), UpdatedBy = "", SubOA = "";
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
                                     DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
												//DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                                    row["outwardqty"].ToString(),
                                    //DateTime.Now,                           // no need to insert (Wrong Input)
                                    //row["outwardqty"].ToString(),           // no need to insert (Wrong Input)
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
                                    //sqlBulkCopy.ColumnMappings.Add("outwarddatetime", "OutwardDtTime");
                                    //sqlBulkCopy.ColumnMappings.Add("outwardqty", "OutwardQty");
                                    sqlBulkCopy.ColumnMappings.Add("deliverydate", "DeliveryDate");
                                    sqlBulkCopy.ColumnMappings.Add("Isapprove", "IsApprove");
                                    sqlBulkCopy.WriteToServer(tempdt);

                                    tempdt.Clear();
                                    SubOA = row["SubOA"] != DBNull.Value ? row["SubOA"].ToString() : string.Empty;
                                    CheckInwardqtystages(SubOA);
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
                                SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblLaserPrograming] SET [InwardQty] = '" + totOutwardqnt.ToString() + "',[IsComplete]=NULL,[InwardDtTime]='" + row["outwarddatetime"].ToString() + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                                cmdupdate.ExecuteNonQuery();
                            }

                            if (row["inwardqty"].ToString() == row["outwardqty"].ToString())
                            {
                                string OutwardDtTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                                SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblDrawing] SET [OutwardQty] = '" + row["totalinward"].ToString() + "',[InwardQty]='0',[IsComplete]=1,OutwardDtTime= '" + OutwardDtTime.ToString() + "',UpdatedDate='" + OutwardDtTime.ToString() + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                                cmdupdate.ExecuteNonQuery();
                            }
                            else
                            {
                                string OutwardDtTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
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

                                //DateTime conversion issue here
                                SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblDrawing] SET [InwardQty] = '" + inwardqy.ToString() + "', [OutwardQty] = '" + totoutward.ToString() + "',OutwardDtTime= '" + OutwardDtTime.ToString() + "',UpdatedDate='" + OutwardDtTime.ToString() + "'  WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                                cmdupdate.ExecuteNonQuery();


                                //new  method add
                                Checkoutwardqtnyofcuurentstage(SubOA);

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
        else
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully- Approved and send to TPP Programming Department...!');window.location.href='Drawing.aspx';", true);
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
            SqlCommand cmd = new SqlCommand("select id from [ExcelEncLive].[vwDrawerCreation] where SubOA='" + Id + "'", con);
            Object Id_data = cmd.ExecuteScalar();
            con.Close();


            gvDetails.DataSource = GetData(string.Format("select * from  [ExcelEncLive].vwOrderAccept where [SubOANumber]='{0}'", Id));
            gvDetails.DataBind();

            gvOADetails.DataSource = GetData(string.Format("SELECT [DocID],[OAid],[File1],[File2],[File3],[File4],[File5],[CeatedDate],[CreatedBy] FROM [ExcelEncLive].[tblOAFiledata] where OAid='{0}'", Id_data.ToString()));
            gvOADetails.DataBind();

        }
    }
    private static DataTable GetData(string query)
    {
        string strConnString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
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


    protected void btnexcel_Click(object sender, EventArgs e)
    {


        if (txtCustomerName.Text == "")
        {
            //Response.Redirect("ProductionExcel.aspx");
            string Report = "Drawaing";
            string url = "ProductionExcel.aspx?Dep=" + Server.UrlEncode(Report);
            Response.Redirect(url);
        }

        else
        {
            string Report = "Drawaing";
            string Customer = Server.UrlEncode(txtCustomerName.Text);
            string url = "ProductionExcel.aspx?Dep=" + Server.UrlEncode(Report) + "&Customer=" + Customer;
            Response.Redirect(url);
        }

        ////Response.Redirect("ProductionExcel.aspx");
        //string Report = "Drawaing";
        //string url = "ProductionExcel.aspx?Dep=" + Server.UrlEncode(Report);
        //Response.Redirect(url);
    }

    protected void txtCustomerName_TextChanged(object sender, EventArgs e)
    {
        FillDrawinggrid();
    }

    private void FillDrawinggrid()
    {


        try
        {
            string query = string.Empty;
            //            query = @"SELECT [VWID],[id] as mainID, [OAno],[currentdate],[customername],[deliverydatereqbycust],[IsDrawingcomplete],[Description],[Qty],[Price],[Discount]
            //,[TotalAmount],[CGST],[SGST],[IGST],[SubOANumber] FROM vwOrderAccept where IsComplete is null order by deliverydatereqbycust asc";

            query = @"SELECT [pono],[id] as mainID,[OANumber],[Size],[TotalQty],OACreationDate,[InwardDtTime],[InwardQty],[deliverydatereqbycust],[customername],SubOA
               FROM [ExcelEncLive].[vwDrawerCreation] where IsComplete is null and customername like '" + txtCustomerName.Text.Trim() + "%'   order by deliverydatereqbycust asc";

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

    public void CheckInwardqtystages(String SubOA)
    {
        // Initialize CNCInwardQty with a default value (0)
        object CNCInwardQty = 0;

        SqlCommand cmdselect2 = new SqlCommand("select OutwardQty from tblLaserPrograming WHERE SubOA=@SubOA", con);
        cmdselect2.Parameters.AddWithValue("@SubOA", SubOA);
        object result = cmdselect2.ExecuteScalar();
        if (result != DBNull.Value && result != null)
        {
            CNCInwardQty = result;
        }
        else
        {
            // Check tblLaserCutting
            SqlCommand cmdselect8 = new SqlCommand("select InwardQty from tblLaserCutting WHERE SubOA=@SubOA", con);
            cmdselect8.Parameters.AddWithValue("@SubOA", SubOA);
            result = cmdselect8.ExecuteScalar();
            if (result != DBNull.Value && result != null)
            {
                CNCInwardQty = result;
            }
            else
            {
                // Check tblCNCBending
                SqlCommand cmdselect3 = new SqlCommand("select InwardQty from tblCNCBending WHERE SubOA=@SubOA", con);
                cmdselect3.Parameters.AddWithValue("@SubOA", SubOA);
                result = cmdselect3.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    CNCInwardQty = result;
                }
                else
                {
                    // Check tblWelding
                    SqlCommand cmdselect4 = new SqlCommand("select InwardQty from tblWelding WHERE SubOA=@SubOA", con);
                    cmdselect4.Parameters.AddWithValue("@SubOA", SubOA);
                    result = cmdselect4.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        CNCInwardQty = result;
                    }
                    else
                    {
                        // Check tblPowderCoating
                        SqlCommand cmdselect5 = new SqlCommand("select InwardQty from tblPowderCoating WHERE SubOA=@SubOA", con);
                        cmdselect5.Parameters.AddWithValue("@SubOA", SubOA);
                        result = cmdselect5.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            CNCInwardQty = result;
                        }
                        else
                        {
                            // Check tblFinalAssembly
                            SqlCommand cmdselect6 = new SqlCommand("select InwardQty from tblFinalAssembly WHERE SubOA=@SubOA", con);
                            cmdselect6.Parameters.AddWithValue("@SubOA", SubOA);
                            result = cmdselect6.ExecuteScalar();
                            if (result != DBNull.Value && result != null)
                            {
                                CNCInwardQty = result;
                            }
                            else
                            {
                                // Check tblStock
                                SqlCommand cmdselect7 = new SqlCommand("select InwardQty from tblStock WHERE SubOA=@SubOA", con);
                                cmdselect7.Parameters.AddWithValue("@SubOA", SubOA);
                                result = cmdselect7.ExecuteScalar();
                                if (result != DBNull.Value && result != null)
                                {
                                    CNCInwardQty = result;
                                }
                                else
                                {
                                    CNCInwardQty = 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Now update the table with the determined CNCInwardQty
        SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblLaserPrograming] SET [OutwardQty] = @OutwardQty WHERE SubOA=@SubOA", con);
        cmdupdate.Parameters.AddWithValue("@OutwardQty", CNCInwardQty);
        cmdupdate.Parameters.AddWithValue("@SubOA", SubOA); // Use parameterized query for security
        cmdupdate.ExecuteNonQuery();
    }
    ///25-11-2024
    //protected void GetSelectedRecords(object sender, EventArgs e)
    //{
    //    string confirmValue = "Yes";

    //    if (confirmValue == "Yes")
    //    {
    //        DataTable dt = new DataTable();
    //        dt.Columns.AddRange(new DataColumn[12]
    //        {
    //        new DataColumn("OAnumber"),
    //        new DataColumn("SubOA"),
    //        new DataColumn("customername"),
    //        new DataColumn("size"),
    //        new DataColumn("totalinward"),
    //        new DataColumn("inwarddatetime"),
    //        new DataColumn("inwardqty"),
    //        new DataColumn("outwarddatetime"),
    //        new DataColumn("outwardqty"),
    //        new DataColumn("deliverydate"),
    //        new DataColumn("remark"),
    //        new DataColumn("Isapprove")
    //        });

    //        foreach (GridViewRow row in dgvDrawing.Rows)
    //        {
    //            if (row.RowType == DataControlRowType.DataRow)
    //            {
    //                CheckBox chkRow = (row.Cells[1].FindControl("chkRow") as CheckBox);
    //                if (chkRow.Checked)
    //                {
    //                    string OANumber = (row.Cells[1].FindControl("lblOANumber") as Label).Text;
    //                    string SubOA = (row.Cells[1].FindControl("lblSubOANumber") as Label).Text;
    //                    TextBox Custtb = (TextBox)row.Cells[1].FindControl("lblCustName");
    //                    string CustName = Custtb.Text;
    //                    string TotalQty = (row.Cells[1].FindControl("lblTotalQty") as Label).Text;
    //                    string DeliveryDt = (row.Cells[1].FindControl("lblDeliveryDt") as Label).Text;
    //                    string InwardDtTime = (row.Cells[1].FindControl("lblInwardDtTime") as Label).Text;
    //                    TextBox tbInwardQty = (TextBox)row.Cells[1].FindControl("txtInwardQty");
    //                    string InwardQty = tbInwardQty.Text;
    //                    string OutwardDtTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");
    //                    TextBox Outwardtb = (TextBox)row.Cells[1].FindControl("txtOutwardQty");
    //                    string[] strarr = Outwardtb.Text.Split(',');
    //                    string OutwardQty = strarr[1].ToString();

    //                    TextBox Sizetb = (TextBox)row.Cells[1].FindControl("txtSize");
    //                    string Size = Sizetb.Text;

    //                    TextBox Remarktb = (TextBox)row.Cells[1].FindControl("txtRemark");
    //                    string Remark = Remarktb.Text;

    //                    dt.Rows.Add(OANumber, SubOA, CustName, Size, TotalQty, InwardDtTime, InwardQty, OutwardDtTime, OutwardQty, DeliveryDt, Remark, true);
    //                }
    //            }
    //        }

    //        using (SqlConnection con = new SqlConnection("your_connection_string"))
    //        {
    //            con.Open();
    //            foreach (DataRow row in dt.Rows)
    //            {
    //                bool IsComplete = row["inwardqty"].ToString() == row["outwardqty"].ToString();
    //                string OANumber = row["OAnumber"].ToString();
    //                string SubOA = row["SubOA"].ToString();

    //                SqlCommand cmdCheckExist = new SqlCommand("SELECT SubOA FROM tblLaserPrograming WHERE SubOA = @SubOA", con);
    //                cmdCheckExist.Parameters.AddWithValue("@SubOA", SubOA);
    //                var existingSubOA = cmdCheckExist.ExecuteScalar();

    //                if (existingSubOA == null)
    //                {
    //                    // Insert new record into tblLaserPrograming
    //                    string insertQuery = @"
    //                    INSERT INTO dbo.tblLaserPrograming
    //                    (OANumber, SubOA, CustomerName, Size, TotalQty, InwardDtTime, InwardQty, DeliveryDate, IsApprove)
    //                    VALUES (@OANumber, @SubOA, @CustomerName, @Size, @TotalQty, @InwardDtTime, @InwardQty, @DeliveryDate, @IsApprove)";

    //                    SqlCommand cmdInsert = new SqlCommand(insertQuery, con);
    //                    cmdInsert.Parameters.AddWithValue("@OANumber", OANumber);
    //                    cmdInsert.Parameters.AddWithValue("@SubOA", SubOA);
    //                    cmdInsert.Parameters.AddWithValue("@CustomerName", row["customername"]);
    //                    cmdInsert.Parameters.AddWithValue("@Size", row["size"]);
    //                    cmdInsert.Parameters.AddWithValue("@TotalQty", row["totalinward"]);
    //                    cmdInsert.Parameters.AddWithValue("@InwardDtTime", row["inwarddatetime"]);
    //                    cmdInsert.Parameters.AddWithValue("@InwardQty", row["inwardqty"]);
    //                    cmdInsert.Parameters.AddWithValue("@DeliveryDate", row["deliverydate"]);
    //                    cmdInsert.Parameters.AddWithValue("@IsApprove", true);

    //                    cmdInsert.ExecuteNonQuery();
    //                }
    //                else
    //                {
    //                    // Update existing record in tblLaserPrograming
    //                    string updateQuery = @"
    //                    UPDATE dbo.tblLaserPrograming
    //                    SET InwardQty = InwardQty + @OutwardQty, 
    //                        IsComplete = @IsComplete, 
    //                        InwardDtTime = @InwardDtTime
    //                    WHERE SubOA = @SubOA";

    //                    SqlCommand cmdUpdate = new SqlCommand(updateQuery, con);
    //                    cmdUpdate.Parameters.AddWithValue("@OutwardQty", row["outwardqty"]);
    //                    cmdUpdate.Parameters.AddWithValue("@IsComplete", IsComplete);
    //                    cmdUpdate.Parameters.AddWithValue("@InwardDtTime", row["outwarddatetime"]);
    //                    cmdUpdate.Parameters.AddWithValue("@SubOA", SubOA);

    //                    cmdUpdate.ExecuteNonQuery();
    //                }

    //                // Update tblDrawing record
    //                string updateDrawingQuery = @"
    //                UPDATE dbo.tblDrawing
    //                SET OutwardQty = OutwardQty + @OutwardQty, 
    //                    InwardQty = @InwardQty, 
    //                    OutwardDtTime = @OutwardDtTime
    //                WHERE SubOA = @SubOA";

    //                SqlCommand cmdUpdateDrawing = new SqlCommand(updateDrawingQuery, con);
    //                cmdUpdateDrawing.Parameters.AddWithValue("@OutwardQty", row["outwardqty"]);
    //                cmdUpdateDrawing.Parameters.AddWithValue("@InwardQty", row["inwardqty"]);
    //                cmdUpdateDrawing.Parameters.AddWithValue("@OutwardDtTime", DateTime.Now);
    //                cmdUpdateDrawing.Parameters.AddWithValue("@SubOA", SubOA);

    //                cmdUpdateDrawing.ExecuteNonQuery();
    //            }

    //            con.Close();
    //        }

    //        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully- Approved and sent to Laser Programming Department!'); window.location.href='Drawing.aspx';", true);
    //    }
    //    else
    //    {
    //        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Cancelled Successfully!')", true);
    //    }
    //}


    //protected void GetSelectedRecords(object sender, EventArgs e)
    //{
    //    string confirmValue = "Yes";

    //    if (confirmValue == "Yes")
    //    {
    //        DataTable dt = new DataTable();
    //        dt.Columns.AddRange(new DataColumn[12]
    //        {
    //        new DataColumn("OAnumber"),
    //        new DataColumn("SubOA"),
    //        new DataColumn("customername"),
    //        new DataColumn("size"),
    //        new DataColumn("totalinward"),
    //        new DataColumn("inwarddatetime"),
    //        new DataColumn("inwardqty"),
    //        new DataColumn("outwarddatetime"),
    //        new DataColumn("outwardqty"),
    //        new DataColumn("deliverydate"),
    //        new DataColumn("remark"),
    //        new DataColumn("Isapprove")
    //        });

    //        foreach (GridViewRow row in dgvDrawing.Rows)
    //        {
    //            if (row.RowType == DataControlRowType.DataRow)
    //            {
    //                CheckBox chkRow = (row.Cells[1].FindControl("chkRow") as CheckBox);
    //                if (chkRow.Checked)
    //                {
    //                    string OANumber = (row.Cells[1].FindControl("lblOANumber") as Label).Text;
    //                    string SubOA = (row.Cells[1].FindControl("lblSubOANumber") as Label).Text;
    //                    TextBox Custtb = (TextBox)row.Cells[1].FindControl("lblCustName");
    //                    string CustName = Custtb.Text;
    //                    string TotalQty = (row.Cells[1].FindControl("lblTotalQty") as Label).Text;
    //                    string DeliveryDt = (row.Cells[1].FindControl("lblDeliveryDt") as Label).Text;
    //                    string InwardDtTime = (row.Cells[1].FindControl("lblInwardDtTime") as Label).Text;
    //                    TextBox tbInwardQty = (TextBox)row.Cells[1].FindControl("txtInwardQty");
    //                    string InwardQty = tbInwardQty.Text;
    //                    string OutwardDtTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");
    //                    TextBox Outwardtb = (TextBox)row.Cells[1].FindControl("txtOutwardQty");
    //                    string[] strarr = Outwardtb.Text.Split(',');
    //                    string OutwardQty = strarr[1].ToString();

    //                    TextBox Sizetb = (TextBox)row.Cells[1].FindControl("txtSize");
    //                    string Size = Sizetb.Text;

    //                    TextBox Remarktb = (TextBox)row.Cells[1].FindControl("txtRemark");
    //                    string Remark = Remarktb.Text;

    //                    dt.Rows.Add(OANumber, SubOA, CustName, Size, TotalQty, InwardDtTime, InwardQty, OutwardDtTime, OutwardQty, DeliveryDt, Remark, true);
    //                }
    //            }
    //        }

    //        using (SqlConnection con = new SqlConnection("your_connection_string"))
    //        {
    //            con.Open();
    //            foreach (DataRow row in dt.Rows)
    //            {
    //                bool IsComplete = row["inwardqty"].ToString() == row["outwardqty"].ToString();
    //                string OANumber = row["OAnumber"].ToString();
    //                string SubOA = row["SubOA"].ToString();

    //                SqlCommand cmdCheckExist = new SqlCommand("SELECT SubOA FROM tblLaserPrograming WHERE SubOA = @SubOA", con);
    //                cmdCheckExist.Parameters.AddWithValue("@SubOA", SubOA);
    //                var existingSubOA = cmdCheckExist.ExecuteScalar();

    //                if (existingSubOA == null)
    //                {
    //                    // Insert new record into tblLaserPrograming
    //                    string insertQuery = @"
    //                INSERT INTO dbo.tblLaserPrograming
    //                (OANumber, SubOA, CustomerName, Size, TotalQty, InwardDtTime, InwardQty, DeliveryDate, IsApprove)
    //                VALUES (@OANumber, @SubOA, @CustomerName, @Size, @TotalQty, @InwardDtTime, @InwardQty, @DeliveryDate, @IsApprove)";

    //                    SqlCommand cmdInsert = new SqlCommand(insertQuery, con);
    //                    cmdInsert.Parameters.AddWithValue("@OANumber", OANumber);
    //                    cmdInsert.Parameters.AddWithValue("@SubOA", SubOA);
    //                    cmdInsert.Parameters.AddWithValue("@CustomerName", row["customername"]);
    //                    cmdInsert.Parameters.AddWithValue("@Size", row["size"]);
    //                    cmdInsert.Parameters.AddWithValue("@TotalQty", row["totalinward"]);
    //                    cmdInsert.Parameters.AddWithValue("@InwardDtTime", row["inwarddatetime"]);
    //                    cmdInsert.Parameters.AddWithValue("@InwardQty", row["inwardqty"]);
    //                    cmdInsert.Parameters.AddWithValue("@DeliveryDate", row["deliverydate"]);
    //                    cmdInsert.Parameters.AddWithValue("@IsApprove", true);

    //                    cmdInsert.ExecuteNonQuery();
    //                }
    //                else
    //                {
    //                    // Update existing record in tblLaserPrograming
    //                    string updateQuery = @"
    //                UPDATE dbo.tblLaserPrograming
    //                SET InwardQty = InwardQty + @OutwardQty, 
    //                    IsComplete = @IsComplete, 
    //                    InwardDtTime = @InwardDtTime
    //                WHERE SubOA = @SubOA";

    //                    SqlCommand cmdUpdate = new SqlCommand(updateQuery, con);
    //                    cmdUpdate.Parameters.AddWithValue("@OutwardQty", row["outwardqty"]);
    //                    cmdUpdate.Parameters.AddWithValue("@IsComplete", IsComplete);
    //                    cmdUpdate.Parameters.AddWithValue("@InwardDtTime", row["outwarddatetime"]);
    //                    cmdUpdate.Parameters.AddWithValue("@SubOA", SubOA);

    //                    cmdUpdate.ExecuteNonQuery();
    //                }

    //                // Update tblDrawing record
    //                string updateDrawingQuery = @"
    //            UPDATE dbo.tblDrawing
    //            SET OutwardQty = OutwardQty + @OutwardQty, 
    //                InwardQty = @InwardQty, 
    //                OutwardDtTime = @OutwardDtTime
    //            WHERE SubOA = @SubOA";

    //                SqlCommand cmdUpdateDrawing = new SqlCommand(updateDrawingQuery, con);
    //                cmdUpdateDrawing.Parameters.AddWithValue("@OutwardQty", row["outwardqty"]);
    //                cmdUpdateDrawing.Parameters.AddWithValue("@InwardQty", row["inwardqty"]);
    //                cmdUpdateDrawing.Parameters.AddWithValue("@OutwardDtTime", DateTime.Now);
    //                cmdUpdateDrawing.Parameters.AddWithValue("@SubOA", SubOA);

    //                cmdUpdateDrawing.ExecuteNonQuery();
    //            }

    //            con.Close();
    //        }

    //        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully- Approved and sent to Laser Programming Department!'); window.location.href='Drawing.aspx';", true);
    //    }
    //    else
    //    {
    //        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Cancelled Successfully!')", true);
    //    }
    //}



    public void Checkoutwardqtnyofcuurentstage(String SubOA)
    {


        object CNCInwardQty1 = 0;
        // Check tblPowderCoating
        SqlCommand cmdselect5 = new SqlCommand("select InwardQty from tblLaserPrograming WHERE SubOA=@SubOA", con);
        cmdselect5.Parameters.AddWithValue("@SubOA", SubOA);
        object result = cmdselect5.ExecuteScalar();
        if (result != DBNull.Value && result != null && result.ToString() != "0")
        {
            CNCInwardQty1 = result;

            SqlCommand cmdupdate1 = new SqlCommand("UPDATE [dbo].[tblDrawing] SET [OutwardQty] = @OutwardQty WHERE SubOA=@SubOA", con);
            cmdupdate1.Parameters.AddWithValue("@OutwardQty", CNCInwardQty1);
            cmdupdate1.Parameters.AddWithValue("@SubOA", SubOA);
            cmdupdate1.ExecuteNonQuery();

        }


    }


    [WebMethod]
    public static void MakeSessionNull()
    {
        // Clear the session value
        HttpContext.Current.Session["OneTimeFlag"] = null;
    }

}
