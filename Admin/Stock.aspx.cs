﻿
using ClosedXML.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization; 


public partial class Admin_Stock : System.Web.UI.Page
{
    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
    DataTable dt = new DataTable();
    string InwardQty = "";

    DataTable tempdt = new DataTable();
    protected void Page_Load(object sender, EventArgs e)
    {
        string name = Session["name"].ToString();
        if (Session["name"] == null)
        {
            Response.Redirect("../Login.aspx");
        }
        else
        {
            if (!this.IsPostBack)
            {
                GetStockData();
            }
        }
    }

    private void StockDDLbind()
    {
        SqlDataAdapter ad = new SqlDataAdapter("select StockId, OANumber as OANumber from tblStock where IsApprove=1 and IsComplete is null order by StockId desc", con);
        DataTable dt = new DataTable();
        ad.Fill(dt);
        if (dt.Rows.Count > 0)
        {
            ddlONumber.DataSource = dt;
            ddlONumber.DataTextField = "OANumber";
            ddlONumber.DataValueField = "StockId";
            ddlONumber.DataBind();
            ddlONumber.Items.Insert(0, "All");
            ddlONumber.Items.Insert(0, "--Select--");
        }
    }

    protected void GetStockData()
    {
        try
        {
            string query = string.Empty;

            query = @"SELECT [StockId],[OANumber],[SubOA],[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],
            [OutwardDtTime],[OutwardQty],[DeliveryDate],[IsApprove],[IsPending],[IsCancel],[CreatedBy],[CreatedDate],[UpdatedBy],
            [UpdatedDate],[IsComplete] FROM tblStock where IsComplete is null order by CONVERT(DateTime, DeliveryDate,103) asc";

            DataTable dt = GetData(query);
            if (dt.Rows.Count > 0)
            {

                dgvStock.DataSource = dt;
                dgvStock.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + dgvStock.ClientID + "', 900, 1020 , 40 ,true); </script>", false);
            }
            else
            {
                dgvStock.DataSource = null;
                dgvStock.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + dgvStock.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Record Not Found..!');", true);
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
            CheckBox chkRow;
            //string confirmValue = Request.Form["confirm_value"];
            string confirmValue = "Yes";
            if (confirmValue == "Yes")
            {
                DataTable dt = new DataTable();
                bool flag = false;
                dt.Columns.AddRange(new DataColumn[10]
                { new DataColumn("OAnumber"),
            new DataColumn("SubOA"),
                new DataColumn("customername"),
                new DataColumn("size"),
                new DataColumn("totalinward"),
                new DataColumn("inwarddatetime"),
                new DataColumn("inwardqty"),
                new DataColumn("outwarddatetime"),
                new DataColumn("outwardqty"),
                new DataColumn("deliverydate") });

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
                                new DataColumn("Isapprove") });

                foreach (GridViewRow row in dgvStock.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        chkRow = (row.Cells[1].FindControl("chkRow") as CheckBox);
                        int totalCount = dgvStock.Rows.Cast<GridViewRow>().Count(r => ((CheckBox)r.FindControl("chkRow")).Checked);
                        if (totalCount <= 0)
                        {
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Please Select Atleast One Row..!!');", true);
                            flag = true;
                        }
                        else
                        {
                            if (chkRow.Checked)
                            {
                                string OANumber = (row.Cells[1].FindControl("lblOANumber") as Label).Text;
                                string SubOA = (row.Cells[1].FindControl("lblSubOANumber") as Label).Text;
                                //string CustName = (row.Cells[1].FindControl("lblCustName") as Label).Text;
                                TextBox Custtb = (TextBox)row.Cells[1].FindControl("lblCustName");
                                string CustName = Custtb.Text;
                                string TotalQty = (row.Cells[1].FindControl("lblTotalQty") as Label).Text;
                                string DeliveryDt = (row.Cells[1].FindControl("lblDeliveryDt") as Label).Text;
                                string InwardDtTime = (row.Cells[1].FindControl("lblInwardDtTime") as Label).Text;
                                TextBox tb = (TextBox)row.Cells[1].FindControl("txtInwardQty");
                                //string InwardQty = tb.Text;
                                string[] strarr1 = tb.Text.Split(',');
                                InwardQty = strarr1[0].ToString();

                                //Get Date and time gridview row
                                TextBox tbOutwardDt = (TextBox)row.Cells[1].FindControl("txtOutwardDtTime");
                                // DateTime OutwardDtT = DateTime.Parse(tbOutwardDt.Text);
                                string time = DateTime.Now.ToString("h:mm tt");
                                string OutwardDtTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");

                                TextBox Outwardtb = (TextBox)row.Cells[1].FindControl("txtOutwardQty");
                                //string OutwardQty = Outwardtb.Text;
                                string[] strarr = Outwardtb.Text.Split(',');
                                string OutwardQty = strarr[1].ToString();

                                TextBox Sizetb = (TextBox)row.Cells[1].FindControl("lblSize");
                                string Size = Sizetb.Text;


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
                                    //string Size = strsi.Replace("<br><br><br>", " ")
                                    dt.Rows.Add(OANumber, SubOA, CustName, Size, TotalQty, InwardDtTime, InwardQty, OutwardDtTime, OutwardQty, DeliveryDt);
                                }
                                else
                                {
                                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Alert!!!!!- Outward Quantity Is Greater Than Inward Quantity...!');window.location.href='Stock.aspx';", true);
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
                        string CreatedBy = Session["name"].ToString(), UpdatedBy = "";
                        //string UpdatedDate = DateTime.Now.ToShortDateString(), 
                        //    CreatedDate = DateTime.Now.ToShortDateString();
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


                            SqlCommand cmdexsist = new SqlCommand("select OANumber,InwardQty,OutwardQty,SubOA from [tblDispatch] WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
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


                            SqlCommand cmd2 = new SqlCommand("select OutwardQty from tblStock WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
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
                                   //DateTime.Now,
                                   //row["outwardqty"].ToString(),
                                   row["deliverydate"].ToString(),
                                    true);

                                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                                {
                                    //Set the database table name
                                    sqlBulkCopy.DestinationTableName = "dbo.tblDispatch";
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
                                }
                            }
                            else
                            {
                                int totOutwardqnt = Convert.ToInt32(InwardQty) + Convert.ToInt32(row["outwardqty"].ToString());
                                SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblDispatch] SET [InwardQty] = '" + totOutwardqnt.ToString() + "',[InwardDtTime]='" + row["outwarddatetime"].ToString() + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                                cmdupdate.ExecuteNonQuery();
                            }

                            //New changes for Forword Qty on 28-11-2024 by shubham wankhade
                            if (row["inwardqty"].ToString() == row["outwardqty"].ToString())
                            {
                                string OutwardDtTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

                                // SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblStock] SET [OutwardQty] = '" + row["totalinward"].ToString() + "',[InwardQty]='0',[IsComplete]=1,OutwardDtTime= '" + OutwardDtTime.ToString() + "',UpdatedDate='" + OutwardDtTime + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                                SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblStock] SET [OutwardQty] = '" + row["inwardqty"].ToString() + "',[InwardQty]='0',[IsComplete]=1,OutwardDtTime= '" + OutwardDtTime.ToString() + "',UpdatedDate='" + OutwardDtTime + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                                cmdupdate.ExecuteNonQuery();
                            }
                            //End
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

                                SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblStock] SET [InwardQty] = '" + inwardqy.ToString() + "', [OutwardQty] = '" + totoutward.ToString() + "',OutwardDtTime= '" + OutwardDtTime.ToString() + "',UpdatedDate='" + OutwardDtTime + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                                cmdupdate.ExecuteNonQuery();
                            }

                            string OAnum = row["OAnumber"].ToString();

                            SqlCommand cmdUpdateOA = new SqlCommand("update OAList set IsDispatch=1 where oano='" + OAnum + "'", con);
                            cmdUpdateOA.ExecuteNonQuery();

                            con.Close();
                        }
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully- Disptached...!');window.location.href='Stock.aspx';", true);
                        //Response.Redirect("Stock.aspx");
                    }
                }

            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Cancelled Successfully..!')", true);
                Response.Redirect("Stock.aspx");
            }

        }
        else
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully- Approved and send to TPP Programming Department...!');window.location.href='Drawing.aspx';", true);
        }








    }

    protected void ddlONumber_SelectedIndexChanged(object sender, EventArgs e)
    {
        GetStockData();
        txtcustomerName.Enabled = false;
    }

    protected void txtOANumber_TextChanged(object sender, EventArgs e)
    {
        GetStockData();
        ddlONumber.Enabled = false;
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        Resetdata();
    }

    protected void Resetdata()
    {
        //txtcustomerName.Text = ""; ddlONumber.Text = "--Select--"; ddlONumber.Enabled = true;
        //dgvStock.DataSource = null;
        //dgvStock.DataBind();
        // btnGetSelected.Visible = false;
        //txtcustomerName.Enabled = true;
        //Response.Redirect("LaserProgramming.aspx");
        GetStockData();
    }

    //Checkbox checked
    protected void chkRow_CheckedChanged(object sender, EventArgs e)
    {
        foreach (GridViewRow row in dgvStock.Rows)
        {
            if (row.RowType == DataControlRowType.DataRow)
            {

                CheckBox chkRow = (row.Cells[1].FindControl("chkRow") as CheckBox);
                int totalCount = dgvStock.Rows.Cast<GridViewRow>().Count(r => ((CheckBox)r.FindControl("chkRow")).Checked);
                TextBox Inwardtb = (TextBox)row.Cells[1].FindControl("txtInwardQty");
                int InwardQty = Convert.ToInt32(Inwardtb.Text);
                if (chkRow.Checked == true)
                {
                    if (totalCount > 0)
                    {
                        if (InwardQty == 0)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Less Quantity- You have Dispatch Quantity...!')", true);
                        }
                        else
                        {
                            //btnGetSelected.Visible = true;
                        }
                    }
                    else
                    {
                        //btnGetSelected.Visible = false;
                    }
                }
                else
                {
                    //btnGetSelected.Visible = false;
                }
            }
        }
    }

    private void calculationA(GridViewRow row)
    {
        TextBox txt_Inward = (TextBox)row.FindControl("txtInwardQty");
        TextBox txt_Outward = (TextBox)row.FindControl("txtOutwardQty");
        txt_Inward.Text = (Convert.ToDecimal(txt_Inward.Text.Trim()) - Convert.ToDecimal(txt_Outward.Text.Trim())).ToString();
    }

    protected void txtOutwardQty_TextChanged(object sender, EventArgs e)
    {
        ViewState["Iscomplete"] = "1";
        GridViewRow row = (sender as TextBox).NamingContainer as GridViewRow;
        calculationA(row);
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

    protected void dgvStock_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            TextBox txtOutwardQty = e.Row.FindControl("txtOutwardQty") as TextBox;
            CheckBox chkRow = (e.Row.Cells[1].FindControl("chkRow") as CheckBox);
            string empcode = Session["empcode"].ToString();
            DataTable Dt = new DataTable();
            SqlDataAdapter Sd = new SqlDataAdapter("Select id from [employees] where [empcode]='" + empcode + "'", con);
            Sd.Fill(Dt);
            if (Dt.Rows.Count > 0)
            {
                string idd = Dt.Rows[0]["id"].ToString();
                DataTable Dtt = new DataTable();
                SqlDataAdapter Sdd = new SqlDataAdapter("Select * FROM [ExcelEncLive].[tblUserRoleAuthorization] where UserID = '" + idd + "' AND PageName = 'Stock.aspx' AND PagesView = '1'", con);
                Sdd.Fill(Dtt);
                if (Dtt.Rows.Count > 0)
                {
                    dgvStock.Columns[1].Visible = false;
                    dgvStock.Columns[13].Visible = false;
                    txtOutwardQty.ReadOnly = true;
                    btnPrintData.Visible = false;
                }
            }
            ViewState["Id"] = dgvStock.DataKeys[e.Row.RowIndex].Value.ToString();
            string Id = dgvStock.DataKeys[e.Row.RowIndex].Value.ToString();
            GridView gvDetails = e.Row.FindControl("gvDetails") as GridView;
            gvDetails.DataSource = GetData(string.Format("select * from  [ExcelEncLive].[vw_FinalAssembly] where SubOA='{0}'", Id));
            gvDetails.DataBind();
            string id = ViewState["Id"].ToString();

            string query = string.Empty;
            string Role = Session["RoleName"].ToString();
            query = @"SELECT * FROM tblStock WHERE  EXISTS ( SELECT 1 FROM [ExcelEncLive].OrderAcceptDtls AS OA INNER JOIN [ExcelEncLive].tblTaxInvoiceDtls AS TI ON OA.id = TI.OAId  INNER JOIN tblStock AS TS ON TS.SubOA = OA.SubOANumber  WHERE TS.IsComplete IS NULL  AND tblStock.SubOA = TS.SubOA 	AND tblStock.SubOA = '" + Id + @"' ) ORDER BY CONVERT(DateTime, DeliveryDate, 103) ASC;";

            DataTable dt = GetData(query);
            if (Role == "Admin")
            {
                chkRow.Visible = true;
            }
            else if (dt.Rows.Count > 0 && Role != "Admin")
            {
                chkRow.Visible = true;

            }
            else
            {
                chkRow.Visible = false;


            }

        }
    }

    protected void lnkbtnReturn_Click(object sender, EventArgs e)
    {
        try
        {
            if (Convert.ToInt32(txtReturnInward.Text) > Convert.ToInt32(hdnInwardQty.Value))
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Inward Qauntity Should be Smaller than or equal to Outward Quantity..!')", true);
                txtReturnInward.Focus();
            }
            else
            {
                con.Open();

                //Get Exsiting Record
                SqlCommand cmdselect = new SqlCommand("select InwardQty from tblFinalAssembly WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                //Object Inwardqty = cmdselect.ExecuteScalar();
                int Inwardqty = Convert.ToInt32(cmdselect.ExecuteScalar() ?? 0);

                SqlCommand cmdselectt = new SqlCommand("select OutwardQty from tblFinalAssembly WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                // Object Inwardqty = cmdselect.ExecuteScalar();
                int OutwardQty = Convert.ToInt32(cmdselectt.ExecuteScalar() ?? 0);


                if (OutwardQty == 0)
                {
                   // GetRecords();
					String SubOa = hdnSubOANo.Value;
                    GetRecords(SubOa);
                }


                if (Convert.ToInt32(txtReturnInward.Text) == Convert.ToInt32(hdnInwardQty.Value))
                {
                    // If all record return
                    SqlCommand cmdDelete = new SqlCommand("Delete from [tblStock] WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                    cmdDelete.ExecuteNonQuery();

                    int TotalReturnInward = Convert.ToInt32(Inwardqty.ToString()) + Convert.ToInt32(txtReturnInward.Text);
                    SqlCommand cmdupdate1 = new SqlCommand("UPDATE [dbo].[tblFinalAssembly] SET [InwardQty] = '" + TotalReturnInward + "',[IsComplete] = NULL, [OutwardQty] = '0' WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                    cmdupdate1.ExecuteNonQuery();
                    Getpreviousqty();
                }
                else
                {


                    int TotalReturn_Outward = Convert.ToInt32(hdnInwardQty.Value) - Convert.ToInt32(txtReturnInward.Text);
                    int TotalReturnInward = Convert.ToInt32(Inwardqty.ToString()) + Convert.ToInt32(txtReturnInward.Text);

                    //Updated current stage
                    //SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblStock] SET [InwardQty] = '" + TotalReturn_Outward + "',[OutwardQty] = '" + TotalReturn_Outward + "' WHERE SubOA='" + hdnSubOANo.Value + "'", con);

                    SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblStock] SET [InwardQty] = '" + TotalReturn_Outward + "',[OutwardQty] = '0' WHERE SubOA='" + hdnSubOANo.Value + "'", con);

                    cmdupdate.ExecuteNonQuery();

                    //Updated Prev stage 
                    SqlCommand cmdupdate1 = new SqlCommand("UPDATE [dbo].[tblFinalAssembly] SET [InwardQty] = '" + TotalReturnInward + "' ,[IsComplete] = NULL  WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                    cmdupdate1.ExecuteNonQuery();
                    GetUpdateqty();
                }
                con.Close();
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Quantity has been Return Successfully..!');window.location.href='Stock.aspx';", true);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    protected void btnPrintData_Click(object sender, EventArgs e)
    {
        try
        {
            //string URL = "PDFShow.aspx?Name=Drawing";
            //string modified_URL = "window.open('" + URL + "', '_blank');";
            //ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", modified_URL, true);
            Response.Redirect("PDFShow.aspx?Name=Stock");
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void btnexcel_Click(object sender, EventArgs e)
    {
        //string Report = "STOCK";
        //string url = "ProductionExcel.aspx?Dep=" + Server.UrlEncode(Report);
        //Response.Redirect(url);

        if (txtCustomerNameNew.Text == "")
        {
            //Response.Redirect("ProductionExcel.aspx");
            string Report = "STOCK";
            string url = "ProductionExcel.aspx?Dep=" + Server.UrlEncode(Report);
            Response.Redirect(url);
        }

        else
        {
            string Report = "STOCK";
            string Customer = Server.UrlEncode(txtCustomerNameNew.Text);
            string url = "ProductionExcel.aspx?Dep=" + Server.UrlEncode(Report) + "&Customer=" + Customer;
            Response.Redirect(url);
        }



    }

    protected void txtCustomerNameNew_TextChanged(object sender, EventArgs e)
    {
        FillGrid();
    }

    public void FillGrid()
    {
        try
        {
            string query = string.Empty;

            query = @"SELECT [StockId],[OANumber],[SubOA],[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],
            [OutwardDtTime],[OutwardQty],[DeliveryDate],[IsApprove],[IsPending],[IsCancel],[CreatedBy],[CreatedDate],[UpdatedBy],
            [UpdatedDate],[IsComplete] FROM tblStock where IsComplete is null and CustomerName like '" + txtCustomerNameNew.Text.Trim() + "%' order by CONVERT(DateTime, DeliveryDate,103) asc";

            DataTable dt = GetData(query);
            if (dt.Rows.Count > 0)
            {

                dgvStock.DataSource = dt;
                dgvStock.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + dgvStock.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

            }
            else
            {
                dgvStock.DataSource = null;
                dgvStock.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + dgvStock.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Record Not Found..!');", true);
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

    public void GetUpdateqty()
    {
        SqlCommand cmdselect1 = new SqlCommand("select InwardQty from tblStock WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        Object LCInwardqty = cmdselect1.ExecuteScalar();

        // Check the Prevoius stage
        if (LCInwardqty == DBNull.Value || LCInwardqty == null)
        {
            LCInwardqty = 0;
        }

        SqlCommand cmdupdatee = new SqlCommand("UPDATE [dbo].[tblFinalAssembly] SET [OutwardQty] = '" + LCInwardqty + "' WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        cmdupdatee.ExecuteNonQuery();
        //END


    }


    //protected void GetSelectedRecords(object sender, EventArgs e)
    //{
    //    string confirmValue = "Yes";  // Assume this comes from a confirmation dialog or user action
    //    if (confirmValue == "Yes")
    //    {
    //        DataTable dt = new DataTable();
    //        bool flag = false;

    //        // Define columns for DataTable
    //        dt.Columns.AddRange(new DataColumn[10]
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
    //        new DataColumn("deliverydate")
    //        });

    //        // Loop through selected rows in the GridView
    //        foreach (GridViewRow row in dgvStock.Rows)
    //        {
    //            if (row.RowType == DataControlRowType.DataRow)
    //            {
    //                CheckBox chkRow = (CheckBox)row.Cells[1].FindControl("chkRow");
    //                if (chkRow.Checked)
    //                {
    //                    string OANumber = ((Label)row.Cells[1].FindControl("lblOANumber")).Text;
    //                    string SubOA = ((Label)row.Cells[1].FindControl("lblSubOANumber")).Text;
    //                    string CustName = ((TextBox)row.Cells[1].FindControl("lblCustName")).Text;
    //                    string TotalQty = ((Label)row.Cells[1].FindControl("lblTotalQty")).Text;
    //                    string DeliveryDt = ((Label)row.Cells[1].FindControl("lblDeliveryDt")).Text;
    //                    string InwardDtTime = ((Label)row.Cells[1].FindControl("lblInwardDtTime")).Text;
    //                    string InwardQty = ((TextBox)row.Cells[1].FindControl("txtInwardQty")).Text.Split(',')[0];
    //                    string OutwardDtTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");
    //                    string OutwardQty = ((TextBox)row.Cells[1].FindControl("txtOutwardQty")).Text.Split(',')[1];
    //                    string Size = ((TextBox)row.Cells[1].FindControl("lblSize")).Text;

    //                    // Add row data to the DataTable
    //                    dt.Rows.Add(OANumber, SubOA, CustName, Size, TotalQty, InwardDtTime, InwardQty, OutwardDtTime, OutwardQty, DeliveryDt);
    //                }
    //            }
    //        }

    //        // If no rows are selected, show alert and return
    //        if (dt.Rows.Count == 0)
    //        {
    //            ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Please Select Atleast One Row..!!');", true);
    //            return;
    //        }

    //        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString); // Assuming connection string is set elsewhere
    //        {
    //            con.Open();
    //            foreach (DataRow row in dt.Rows)
    //            {
    //                // Retrieve existing data for checking if SubOA exists
    //                SqlCommand cmdExsist = new SqlCommand("SELECT OANumber, InwardQty, OutwardQty, SubOA FROM tblDispatch WHERE SubOA = @SubOA", con);
    //                cmdExsist.Parameters.AddWithValue("@SubOA", row["SubOA"].ToString());
    //                SqlDataReader dr = cmdExsist.ExecuteReader();

    //                // Initialize variables for existing values
    //                string OanumberExsists = "", InwardQty = "", OutwardQty = "";
    //                while (dr.Read())
    //                {
    //                    OanumberExsists = dr["SubOA"].ToString();
    //                    InwardQty = dr["InwardQty"].ToString();
    //                    OutwardQty = dr["OutwardQty"].ToString();
    //                }
    //                dr.Close();

    //                // Check if SubOA exists in tblDispatch
    //                if (string.IsNullOrEmpty(OanumberExsists))
    //                {
    //                    // Insert new record into tblDispatch
    //                    SqlCommand cmdInsert = new SqlCommand("INSERT INTO tblDispatch (OANumber, SubOA, CustomerName, Size, TotalQty, InwardDtTime, InwardQty, DeliveryDate, IsApprove) " +
    //                                                          "VALUES (@OANumber, @SubOA, @CustomerName, @Size, @TotalQty, @InwardDtTime, @InwardQty, @DeliveryDate, @IsApprove)", con);
    //                    cmdInsert.Parameters.AddWithValue("@OANumber", row["OAnumber"].ToString());
    //                    cmdInsert.Parameters.AddWithValue("@SubOA", row["SubOA"].ToString());
    //                    cmdInsert.Parameters.AddWithValue("@CustomerName", row["customername"].ToString());
    //                    cmdInsert.Parameters.AddWithValue("@Size", row["size"].ToString());
    //                    cmdInsert.Parameters.AddWithValue("@TotalQty", row["totalinward"].ToString());
    //                    cmdInsert.Parameters.AddWithValue("@InwardDtTime", DateTime.Now);  // Set to current date/time
    //                    cmdInsert.Parameters.AddWithValue("@InwardQty", row["inwardqty"].ToString());
    //                    cmdInsert.Parameters.AddWithValue("@DeliveryDate", row["deliverydate"].ToString());
    //                    cmdInsert.Parameters.AddWithValue("@IsApprove", true);  // Set to true by default
    //                    cmdInsert.ExecuteNonQuery();
    //                }
    //                else
    //                {
    //                    // If SubOA exists, update record in tblDispatch
    //                    int totalOutwardQty = Convert.ToInt32(InwardQty) + Convert.ToInt32(row["outwardqty"].ToString());
    //                    SqlCommand cmdUpdateDispatch = new SqlCommand("UPDATE tblDispatch SET InwardQty = @InwardQty, InwardDtTime = @InwardDtTime WHERE SubOA = @SubOA", con);
    //                    cmdUpdateDispatch.Parameters.AddWithValue("@InwardQty", totalOutwardQty);
    //                    cmdUpdateDispatch.Parameters.AddWithValue("@InwardDtTime", row["outwarddatetime"].ToString());
    //                    cmdUpdateDispatch.Parameters.AddWithValue("@SubOA", row["SubOA"].ToString());
    //                    cmdUpdateDispatch.ExecuteNonQuery();
    //                }

    //                // Update tblStock based on condition
    //                string OutwardDtTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
    //                SqlCommand cmdUpdateStock;
    //                if (row["inwardqty"].ToString() == row["outwardqty"].ToString())
    //                {
    //                    cmdUpdateStock = new SqlCommand("UPDATE tblStock SET OutwardQty = @TotalInward, InwardQty = 0, IsComplete = 1, OutwardDtTime = @OutwardDtTime, UpdatedDate = @OutwardDtTime WHERE SubOA = @SubOA", con);
    //                    cmdUpdateStock.Parameters.AddWithValue("@TotalInward", row["totalinward"].ToString());
    //                }
    //                else
    //                {
    //                    int inwardQty = Convert.ToInt32(row["inwardqty"].ToString()) - Convert.ToInt32(row["outwardqty"].ToString());
    //                    int totalOutwardQty = Convert.ToInt32(OutwardQty) + Convert.ToInt32(row["outwardqty"].ToString());
    //                    cmdUpdateStock = new SqlCommand("UPDATE tblStock SET InwardQty = @InwardQty, OutwardQty = @OutwardQty, OutwardDtTime = @OutwardDtTime, UpdatedDate = @OutwardDtTime WHERE SubOA = @SubOA", con);
    //                    cmdUpdateStock.Parameters.AddWithValue("@InwardQty", inwardQty);
    //                    cmdUpdateStock.Parameters.AddWithValue("@OutwardQty", totalOutwardQty);
    //                }

    //                cmdUpdateStock.Parameters.AddWithValue("@OutwardDtTime", OutwardDtTime);
    //                cmdUpdateStock.Parameters.AddWithValue("@SubOA", row["SubOA"].ToString());
    //                cmdUpdateStock.ExecuteNonQuery();

    //                // Update OAList status
    //                string OAnum = row["OAnumber"].ToString();
    //                SqlCommand cmdUpdateOA = new SqlCommand("UPDATE OAList SET IsDispatch = 1 WHERE OANumber = @OANumber", con);
    //                cmdUpdateOA.Parameters.AddWithValue("@OANumber", OAnum);
    //                cmdUpdateOA.ExecuteNonQuery();
    //            }

    //            con.Close();
    //        }

    //        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully Dispatched!'); window.location.href='Stock.aspx';", true);
    //    }
    //    else
    //    {
    //        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Cancelled Successfully!');", true);
    //        Response.Redirect("Stock.aspx");
    //    }
    //}


    public void GetRecords(string SubOa)
    {

        try
        {
            SqlCommand cmd = new SqlCommand("[SP_GetandInsertdata]", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "Insert");
            cmd.Parameters.AddWithValue("@Currentstages", "tblStock");
            cmd.Parameters.AddWithValue("@prevousStage", "tblFinalAssembly");
			cmd.Parameters.AddWithValue("@SubOa", SubOa);
            cmd.ExecuteNonQuery();
        }
        catch (Exception Ex)
        {

            throw;
        }


    }



    public void Getpreviousqty()
    {
        object CNCInwardQty = 0;


        // Check tblStock
        SqlCommand cmdselect7 = new SqlCommand("select InwardQty from tblstock WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        object result = cmdselect7.ExecuteScalar();
        if (result != DBNull.Value && result != null)
        {
            CNCInwardQty = result;
        }
        else
        {
            CNCInwardQty = 0;
        }

        // Now update the table with the determined CNCInwardQty
        SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblFinalAssembly] SET [OutwardQty] = @OutwardQty WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        cmdupdate.Parameters.AddWithValue("@OutwardQty", CNCInwardQty);
        cmdupdate.ExecuteNonQuery();
        //END



        SqlCommand cmdselect1 = new SqlCommand("select InwardQty from tblFinalAssembly WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        Object LCInwardqty = cmdselect1.ExecuteScalar();

        // Check the Prevoius stage
        if (LCInwardqty == DBNull.Value || LCInwardqty == null)
        {
            LCInwardqty = 0;
        }

        SqlCommand cmdupdatee = new SqlCommand("UPDATE [dbo].[tblPowderCoating] SET [OutwardQty] = '" + LCInwardqty + "' WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        cmdupdatee.ExecuteNonQuery();
        //END


    }

    [WebMethod]
    public static void MakeSessionNull()
    {
        // Clear the session value
        HttpContext.Current.Session["OneTimeFlag"] = null;
    }


}


