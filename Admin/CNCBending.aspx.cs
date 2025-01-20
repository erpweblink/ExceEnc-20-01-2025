﻿
using System;
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


public partial class Admin_CNCBending : System.Web.UI.Page
{
    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);
    DataTable dt = new DataTable();
    string InwardQty = "";
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
                GetCNCBendingData();
                //GetLaserProgamData();
            }
        }
    }

    private void CNCBendingDDLbind()
    {
        SqlDataAdapter ad = new SqlDataAdapter("select CNCBendingId, OANumber as OANumber from tblCNCBending where IsApprove=1 and IsComplete is null order by CNCBendingId desc", con);
        DataTable dt = new DataTable();
        ad.Fill(dt);
        if (dt.Rows.Count > 0)
        {
            ddlONumber.DataSource = dt;
            ddlONumber.DataTextField = "OANumber";
            ddlONumber.DataValueField = "CNCBendingId";
            ddlONumber.DataBind();
            ddlONumber.Items.Insert(0, "All");
            ddlONumber.Items.Insert(0, "--Select--");
        }
    }

    protected void GetCNCBendingData()
    {
        try
        {
            string query = string.Empty;

            query = @"SELECT [CNCBendingId],[OANumber],[SubOA],[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate],[IsApprove],[IsPending],[IsCancel],[CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate] 
                FROM tblCNCBending
                where IsComplete is null order by CONVERT(DateTime, DeliveryDate,103) asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                dgvCNCBending.DataSource = dt;
                dgvCNCBending.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + dgvCNCBending.ClientID + "', 900, 1020 , 40 ,true); </script>", false);
            }
            else
            {
                dgvCNCBending.DataSource = null;
                dgvCNCBending.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + dgvCNCBending.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Pending Record Not Found..!');", true);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    #region Save Data
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
                bool flag = false;
                dt.Columns.AddRange(new DataColumn[10] { new DataColumn("OAnumber"),
                new DataColumn("SubOA"), new DataColumn("customername"),
                new DataColumn("size"), new DataColumn("totalinward"),
                new DataColumn("inwarddatetime"), new DataColumn("inwardqty"),
                new DataColumn("outwarddatetime"), new DataColumn("outwardqty"),
                new DataColumn("deliverydate")
            });

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

                foreach (GridViewRow row in dgvCNCBending.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        CheckBox chkRow = (row.Cells[1].FindControl("chkRow") as CheckBox);
                        int totalCount = dgvCNCBending.Rows.Cast<GridViewRow>().Count(r => ((CheckBox)r.FindControl("chkRow")).Checked);
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
                                string InwardQty = tb.Text;
                                //Get Date and time gridview row
                                TextBox tbOutwardDt = (TextBox)row.Cells[1].FindControl("txtOutwardDtTime");
                                //DateTime OutwardDtT = DateTime.Parse(tbOutwardDt.Text);
                                string time = DateTime.Now.ToString("h:mm tt");
                                //string OutwardDtTime = tbOutwardDt.Text + " " + time;
                                string OutwardDtTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");


                                TextBox Outwardtb = (TextBox)row.Cells[1].FindControl("txtOutwardQty");
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
                                    ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Alert!!!!!- Outward Quantity Is Greater Than Inward Quantity...!');window.location.href='LaserCutting.aspx';", true);
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

                            SqlCommand cmdexsist = new SqlCommand("select OANumber,InwardQty,OutwardQty,SubOA from tblWelding WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                            string OanumberExsists = "", InwardQty = "", OutwardQty = "", SubOA = "";
                            using (SqlDataReader dr = cmdexsist.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    OanumberExsists = dr["SubOA"].ToString();
                                    InwardQty = dr["InwardQty"].ToString();
                                    OutwardQty = dr["OutwardQty"].ToString();
                                }
                            }


                            SqlCommand cmd2 = new SqlCommand("select OutwardQty from tblCNCBending WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
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
                                    sqlBulkCopy.DestinationTableName = "dbo.tblWelding";
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
                            }
                            else
                            {
                                int totOutwardqnt = Convert.ToInt32(InwardQty) + Convert.ToInt32(row["outwardqty"].ToString());
                                SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblWelding] SET [InwardQty] = '" + totOutwardqnt.ToString() + "',[IsComplete]=NULL,[InwardDtTime]='" + row["outwarddatetime"].ToString() + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                                cmdupdate.ExecuteNonQuery();
                            }
                            //New changes for Forword Qty on 28-11-2024 by shubham wankhade
                            if (row["inwardqty"].ToString() == row["outwardqty"].ToString())
                            {
                                string OutwardDtTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

                                SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblCNCBending] SET [OutwardQty] = '" + row["inwardqty"].ToString() + "',[InwardQty]='0',[IsComplete]=1,OutwardDtTime= '" + OutwardDtTime.ToString() + "',UpdatedDate='" + OutwardDtTime + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                                cmdupdate.ExecuteNonQuery();
                            }
                            //END
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

                                SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblCNCBending] SET [InwardQty] = '" + inwardqy.ToString() + "', [OutwardQty] = '" + totoutward.ToString() + "',OutwardDtTime= '" + OutwardDtTime.ToString() + "',UpdatedDate='" + OutwardDtTime + "' WHERE SubOA='" + row["SubOA"].ToString() + "'", con);
                                cmdupdate.ExecuteNonQuery();

                                //new method add
                                Checkoutwardqtnyofcuurentstage(SubOA);

                            }
                            con.Close();
                        }
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Successfully- Approved and send to Welding Department...!');window.location.href='CNCBending.aspx';", true);
                        //Response.Redirect("CNCBending.aspx");
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
    #endregion


    protected void ddlONumber_SelectedIndexChanged(object sender, EventArgs e)
    {
        GetCNCBendingData();
        txtcustomerName.Enabled = false;
    }

    protected void txtOANumber_TextChanged(object sender, EventArgs e)
    {
        GetCNCBendingData();
        ddlONumber.Enabled = false;
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        Resetdata();
    }

    protected void Resetdata()
    {
        //txtcustomerName.Text = ""; ddlONumber.Text = "--Select--"; ddlONumber.Enabled = true;
        //dgvCNCBending.DataSource = null;
        //dgvCNCBending.DataBind();
        btnGetSelected.Visible = false;
        // txtcustomerName.Enabled = true;
        //Response.Redirect("LaserProgramming.aspx");
        GetCNCBendingData();
    }

    //Checkbox All checked
    protected void chkRow_CheckedChanged(object sender, EventArgs e)
    {
        foreach (GridViewRow row in dgvCNCBending.Rows)
        {
            if (row.RowType == DataControlRowType.DataRow)
            {
                CheckBox chkRow = (row.Cells[1].FindControl("chkRow") as CheckBox);
                int totalCount = dgvCNCBending.Rows.Cast<GridViewRow>().Count(r => ((CheckBox)r.FindControl("chkRow")).Checked);
                TextBox Inwardtb = (TextBox)row.Cells[1].FindControl("txtInwardQty");
                int InwardQty = Convert.ToInt32(Inwardtb.Text);
                if (chkRow.Checked == true)
                {
                    if (totalCount > 0)
                    {
                        if (InwardQty == 0)
                        {
                            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Less Quantity- You have sent Quantity to Welding department...!')", true);
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

    protected void checkAll_CheckedChanged(object sender, EventArgs e)
    {
        foreach (GridViewRow row in dgvCNCBending.Rows)
        {
            if (row.RowType == DataControlRowType.DataRow)
            {
                CheckBox chkRow = (row.Cells[1].FindControl("chkRow") as CheckBox);
                int totalCount = dgvCNCBending.Rows.Cast<GridViewRow>().Count(r => ((CheckBox)r.FindControl("chkRow")).Checked);
                TextBox Inwardtb = (TextBox)row.Cells[1].FindControl("txtInwardQty");
                int InwardQty = Convert.ToInt32(Inwardtb.Text);
                if (totalCount > 0)
                {
                    if (InwardQty == 0)
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Less Quantity- You have sent Quantity to Welding department...!')", true);
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

    public string Between(string STR, string FirstString, string LastString)
    {
        string FinalString;
        int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
        int Pos2 = STR.IndexOf(LastString);
        FinalString = STR.Substring(Pos1, Pos2 - Pos1);
        return FinalString;
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
                SqlCommand cmdselect = new SqlCommand("select InwardQty from tblLaserCutting WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                //Object Inwardqty = cmdselect.ExecuteScalar();
                int Inwardqty = Convert.ToInt32(cmdselect.ExecuteScalar() ?? 0);


                SqlCommand cmdselectt = new SqlCommand("select OutwardQty from tblLaserCutting WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                //Object Inwardqty =t cmdselect.ExecuteScalar();
                int OutwardQty = Convert.ToInt32(cmdselectt.ExecuteScalar() ?? 0);
                if (OutwardQty == 0)
                {
                    //GetRecords();
					String SubOa = hdnSubOANo.Value;
                    GetRecords(SubOa);
                }

                if (Convert.ToInt32(txtReturnInward.Text) == Convert.ToInt32(hdnInwardQty.Value))
                {
                    // If all record return

                    int TotalReturnInward = Convert.ToInt32(Inwardqty.ToString()) + Convert.ToInt32(txtReturnInward.Text);
                    SqlCommand cmdupdate1 = new SqlCommand("UPDATE [dbo].[tblLaserCutting] SET [InwardQty] = '" + TotalReturnInward + "',[IsComplete] = NULL,[OutwardQty] = '0' WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                    cmdupdate1.ExecuteNonQuery();

                    SqlCommand cmdDelete = new SqlCommand("Delete from [tblCNCBending] WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                    cmdDelete.ExecuteNonQuery();

                    GetUpdatePrevoiusqty();
                }
                else
                {


                    int TotalReturn_Outward = Convert.ToInt32(hdnInwardQty.Value) - Convert.ToInt32(txtReturnInward.Text);
                    int TotalReturnInward = Convert.ToInt32(Inwardqty.ToString()) + Convert.ToInt32(txtReturnInward.Text);

                    //Updated current stage
                    SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblCNCBending] SET [InwardQty] = '" + TotalReturn_Outward + "',[OutwardQty] = '" + TotalReturn_Outward + "' WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                    cmdupdate.ExecuteNonQuery();

                    //Updated Prev stage 
                    SqlCommand cmdupdate1 = new SqlCommand("UPDATE [dbo].[tblLaserCutting] SET [InwardQty] = '" + TotalReturnInward + "' ,[IsComplete] = NULL  WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                    cmdupdate1.ExecuteNonQuery();

                    GetUpdateqty();
                }

                con.Close();

                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Quantity has been Return Successfully..!');window.location.href='CNCBending.aspx';", true);
            }
        }
        catch (Exception Ex)
        {
            throw;
        }
    }

    protected void dgvCNCBending_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "selectOAnumber")
        {
            string oaNumber = Convert.ToString(e.CommandArgument.ToString());
            ViewState["OANumber"] = oaNumber;
            if (oaNumber != "")
            {
                SqlCommand cmd = new SqlCommand("select OutwardQty from tblRptCNCBending WHERE SubOA='" + oaNumber + "'", con);
                string InwardQty = "";
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            //txtReturnOutward.Text = dr["OutwardQty"].ToString();
                            //divReturn.Visible = true;
                        }
                    }
                    else
                    {
                        ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('Outward Quantity Not Found..!')", true);
                    }
                }
                con.Close();
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('OA number Does not exsist..!')", true);
            }
        }
    }

    protected void dgvCNCBending_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            TextBox txtOutwardQty = e.Row.FindControl("txtOutwardQty") as TextBox;

            string empcode = Session["empcode"].ToString();
            DataTable Dt = new DataTable();
            SqlDataAdapter Sd = new SqlDataAdapter("Select id from [employees] where [empcode]='" + empcode + "'", con);
            Sd.Fill(Dt);
            if (Dt.Rows.Count > 0)
            {
                string idd = Dt.Rows[0]["id"].ToString();
                DataTable Dtt = new DataTable();
                SqlDataAdapter Sdd = new SqlDataAdapter("Select * FROM [ExcelEncLive].[tblUserRoleAuthorization] where UserID = '" + idd + "' AND PageName = 'CNCBending.aspx' AND PagesView = '1'", con);
                Sdd.Fill(Dtt);
                if (Dtt.Rows.Count > 0)
                {
                    dgvCNCBending.Columns[1].Visible = false;
                    dgvCNCBending.Columns[13].Visible = false;
                    txtOutwardQty.ReadOnly = true;
                    btnPrintData.Visible = false;
                }
            }



            string Id = dgvCNCBending.DataKeys[e.Row.RowIndex].Value.ToString();
            GridView gvDetails = e.Row.FindControl("gvDetails") as GridView;
            gvDetails.DataSource = GetData(string.Format("select * from [ExcelEncLive].[vwCNCBending] where SubOA='{0}'", Id));
            gvDetails.DataBind();
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
    protected void btnPrintData_Click(object sender, EventArgs e)
    {
        try
        {
            //string URL = "PDFShow.aspx?Name=Drawing";
            //string modified_URL = "window.open('" + URL + "', '_blank');";
            //ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", modified_URL, true);
            Response.Redirect("PDFShow.aspx?Name=CNC Bending");
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void btnexcel_Click(object sender, EventArgs e)
    {
        //string Report = "CNCBEN";
        //string url = "ProductionExcel.aspx?Dep=" + Server.UrlEncode(Report);
        //Response.Redirect(url);



        if (txtCustomerNameNew.Text == "")
        {
            string Report = "CNCBEN";
            string url = "ProductionExcel.aspx?Dep=" + Server.UrlEncode(Report);
            Response.Redirect(url);
        }

        else
        {
            string Report = "CNCBEN";
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
            query = @"SELECT [CNCBendingId],[OANumber],[SubOA],[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate],[IsApprove],[IsPending],[IsCancel],[CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate] 
                FROM tblCNCBending
                WHERE IsComplete IS NULL 
                AND CustomerName LIKE '" + txtCustomerNameNew.Text.Trim() + @"%'
                ORDER BY CONVERT(DateTime, DeliveryDate, 103) ASC";


            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                dgvCNCBending.DataSource = dt;
                dgvCNCBending.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + dgvCNCBending.ClientID + "', 900, 1020 , 40 ,true); </script>", false);
            }
            else
            {
                dgvCNCBending.DataSource = null;
                dgvCNCBending.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + dgvCNCBending.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

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

    public void GetUpdateqty()
    {
        SqlCommand cmdselect1 = new SqlCommand("select InwardQty from tblCNCBending WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        Object LCInwardqty = cmdselect1.ExecuteScalar();

        SqlCommand cmdselect2 = new SqlCommand("select InwardQty from tblWelding WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        Object CNCInwardQty = cmdselect2.ExecuteScalar();

        // Check the Next stage
        if (CNCInwardQty == DBNull.Value || CNCInwardQty == null)
        {
            CNCInwardQty = 0;
        }

        SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblCNCBending] SET [OutwardQty] = '" + CNCInwardQty + "' WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        cmdupdate.ExecuteNonQuery();
        //END


        // Check the Prevoius stage
        if (LCInwardqty == DBNull.Value || LCInwardqty == null)
        {
            LCInwardqty = 0;
        }

        SqlCommand cmdupdatee = new SqlCommand("UPDATE [dbo].[tblLaserCutting] SET [OutwardQty] = '" + LCInwardqty + "' WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        cmdupdatee.ExecuteNonQuery();
        //END


    }

    public void GetUpdatePrevoiusqty()
    {

        object CNCInwardQty = 0;
        // Check tblWelding
        SqlCommand cmdselect4 = new SqlCommand("select InwardQty from tblWelding WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        object result = cmdselect4.ExecuteScalar();
        if (result != DBNull.Value && result != null)
        {
            CNCInwardQty = result;
        }
        else
        {
            // Check tblPowderCoating
            SqlCommand cmdselect5 = new SqlCommand("select InwardQty from tblPowderCoating WHERE SubOA='" + hdnSubOANo.Value + "'", con);
            result = cmdselect5.ExecuteScalar();
            if (result != DBNull.Value && result != null)
            {
                CNCInwardQty = result;
            }
            else
            {
                // Check tblFinalAssembly
                SqlCommand cmdselect6 = new SqlCommand("select InwardQty from tblFinalAssembly WHERE SubOA='" + hdnSubOANo.Value + "'", con);
                result = cmdselect6.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    CNCInwardQty = result;
                }
                else
                {
                    // Check tblStock
                    SqlCommand cmdselect7 = new SqlCommand("select InwardQty from tblstock WHERE SubOA='" + hdnSubOANo.Value + "'", con);
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

        // Now update the table with the determined CNCInwardQty
        SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblCNCBending] SET [OutwardQty] = @OutwardQty WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        cmdupdate.Parameters.AddWithValue("@OutwardQty", CNCInwardQty);
        cmdupdate.ExecuteNonQuery();
        //END


        // Check the Prevoius stage

        SqlCommand cmdselect1 = new SqlCommand("select InwardQty from tblCNCBending WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        Object LCInwardqty = cmdselect1.ExecuteScalar();
        if (LCInwardqty == DBNull.Value || LCInwardqty == null)
        {
            LCInwardqty = 0;
        }

        SqlCommand cmdupdatee = new SqlCommand("UPDATE [dbo].[tblLaserCutting] SET [OutwardQty] = '" + CNCInwardQty + "' WHERE SubOA='" + hdnSubOANo.Value + "'", con);
        cmdupdatee.ExecuteNonQuery();
        //END


    }

    public void GetRecords(string SubOa)
    {

        try
        {
            SqlCommand cmd = new SqlCommand("[SP_GetandInsertdata]", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Action", "Insert");
            cmd.Parameters.AddWithValue("@Currentstages", "tblCNCBending");
            cmd.Parameters.AddWithValue("@prevousStage", "tblLaserCutting");
			cmd.Parameters.AddWithValue("@SubOa", SubOa);
            cmd.ExecuteNonQuery();
        }
        catch (Exception Ex)
        {

            throw;
        }


    }



    public void CheckInwardqtystages(String SubOA)
    {
        // Initialize CNCInwardQty with a default value (0)
        object CNCInwardQty = 0;

        // Check tblCNCBending

        // Check tblWelding
        SqlCommand cmdselect4 = new SqlCommand("select OutwardQty from tblWelding WHERE SubOA=@SubOA", con);
        cmdselect4.Parameters.AddWithValue("@SubOA", SubOA);
        object result = cmdselect4.ExecuteScalar();
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


        // Now update the table with the determined CNCInwardQty
        SqlCommand cmdupdate = new SqlCommand("UPDATE [dbo].[tblWelding] SET [OutwardQty] = @OutwardQty WHERE SubOA=@SubOA", con);
        cmdupdate.Parameters.AddWithValue("@OutwardQty", CNCInwardQty);
        cmdupdate.Parameters.AddWithValue("@SubOA", SubOA); // Use parameterized query for security
        cmdupdate.ExecuteNonQuery();
    }


    public void Checkoutwardqtnyofcuurentstage(String SubOA)
    {


        object CNCInwardQty1 = 0;
        // Check tblPowderCoating
        SqlCommand cmdselect5 = new SqlCommand("select InwardQty from tblWelding WHERE SubOA=@SubOA", con);
        cmdselect5.Parameters.AddWithValue("@SubOA", SubOA);
        object result = cmdselect5.ExecuteScalar();
        if (result != DBNull.Value && result != null && result.ToString() != "0")
        {
            CNCInwardQty1 = result;

            SqlCommand cmdupdate1 = new SqlCommand("UPDATE [dbo].[tblCNCBending] SET [OutwardQty] = @OutwardQty WHERE SubOA=@SubOA", con);
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