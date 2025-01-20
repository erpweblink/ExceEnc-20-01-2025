using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Admin_OutstandingReport_List : System.Web.UI.Page
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

            }
        }
    }

    protected void btnresetfilter_Click(object sender, EventArgs e)
    {
        Response.Redirect("QuotationF_Report.aspx");
    }

    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
           server control at run time. */
    }

    protected void btnexcel_Click(object sender, EventArgs e)
    {
        //Response.Clear();
        //DateTime now = DateTime.Today;
        //string filename = "QuotationReport" + now.ToString("dd/MM/yyyy");
        //Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
        //Response.ContentType = "application/vnd.xls";
        //System.IO.StringWriter stringWrite = new System.IO.StringWriter();
        //System.Web.UI.HtmlTextWriter htmlWrite =
        //new HtmlTextWriter(stringWrite);
        //GV_QuotationF_Report.RenderControl(htmlWrite);
        //Response.Write(stringWrite.ToString());
        //Response.End();

        PurchaseOrderExport_Excel();

    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        //bindOutstandingReportData();
        BindQuotRpt();
    }

    private void BindQuotRpt()
    {
        try
        {
            if (!string.IsNullOrEmpty(txtfromdate.Text) && !string.IsNullOrEmpty(txttodate.Text))
            {
                if (ddltype.SelectedItem.Value == "0")
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "Alert", "alert('Kindly Select Type First !!!');", true);
                }
                else if (ddltype.SelectedItem.Value == "1")
                {
                    DataTable dt = new DataTable();
                    //SqlDataAdapter sad = new SqlDataAdapter("SELECT * FROM QuotationMain QM  WHERE NOT EXISTS (SELECT 1 FROM OAList OA WHERE OA.quotationid = QM.id) and (convert(date,createddate) BETWEEN CONVERT(DATETIME,'" + txtfromdate.Text + "',103) AND CONVERT(DATETIME,'" + txttodate.Text + "',103))", con);

                    SqlDataAdapter sad = new SqlDataAdapter("SELECT * FROM QuotationMain QM " +
               "WHERE NOT EXISTS (SELECT 1 FROM OAList OA WHERE OA.quotationid = QM.id) " +
               "AND CONVERT(DATE, createddate) BETWEEN " +
               "CONVERT(DATETIME, '" + txtfromdate.Text + "', 103) AND " +
               "CONVERT(DATETIME, '" + txttodate.Text + "', 103)", con);



                    sad.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        GV_QuotationF_Report.EmptyDataText = "Record Not Found";
                        GV_QuotationF_Report.DataSource = dt;
                        GV_QuotationF_Report.DataBind();
                        btnexcel.Visible = true;
                    }
                    else
                    {
                        GV_QuotationF_Report.EmptyDataText = "Record Not Found";
                    }
                }
                else if (ddltype.SelectedItem.Value == "2")
                {
                    DataTable dt = new DataTable();
                    SqlDataAdapter sad = new SqlDataAdapter("SELECT * FROM QuotationMain QM WHERE EXISTS (SELECT 1 FROM OAList OA WHERE OA.quotationid = QM.id) and (convert(date,createddate) BETWEEN CONVERT(DATETIME,'" + txtfromdate.Text + "',103) AND CONVERT(DATETIME,'" + txttodate.Text + "',103))", con);
                    sad.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        GV_QuotationF_Report.EmptyDataText = "Record Not Found";
                        GV_QuotationF_Report.DataSource = dt;
                        GV_QuotationF_Report.DataBind();
                        btnexcel.Visible = true;
                    }
                    else
                    {
                        GV_QuotationF_Report.EmptyDataText = "Record Not Found";
                    }
                }


                else if (ddltype.SelectedItem.Value == "3")
                {
                    DataTable dt = new DataTable();
                    SqlDataAdapter sad = new SqlDataAdapter("SELECT * FROM QuotationMain QM WHERE (convert(date,createddate) BETWEEN CONVERT(DATETIME,'" + txtfromdate.Text + "',103) AND CONVERT(DATETIME,'" + txttodate.Text + "',103))", con);
                    sad.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        GV_QuotationF_Report.EmptyDataText = "Record Not Found";
                        GV_QuotationF_Report.DataSource = dt;
                        GV_QuotationF_Report.DataBind();
                        btnexcel.Visible = true;
                    }
                    else
                    {
                        GV_QuotationF_Report.EmptyDataText = "Record Not Found";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

   protected void GV_QuotationF_Report_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        try
        {
            //if (ddltype.SelectedValue.ToString() == "1")
            //{
            //    DataTable dt = new DataTable();
            //    SqlDataAdapter sad = new SqlDataAdapter("SELECT * FROM QuotationMain QM WHERE EXISTS (SELECT 1 FROM OAList OA WHERE OA.quotationid = QM.id) and (convert(date,createddate) BETWEEN CONVERT(DATETIME,'" + txtfromdate.Text + "',103) AND CONVERT(DATETIME,'" + txttodate.Text + "',103))", con);
            //    sad.Fill(dt);
            //    if (dt.Rows.Count > 0)
            //    {
            //        List<string> quotationList = dt.AsEnumerable().Select(r => r.Field<string>("quotationno").TrimEnd('-').TrimEnd('0').TrimEnd('1').TrimEnd('2').TrimEnd('3').TrimEnd('4').TrimEnd('5')).ToList();
            //        ViewState["QuotationList"] = quotationList;
            //    }

            //    if (e.Row.RowType == DataControlRowType.DataRow)
            //    {
            //        DataRowView rowView = (DataRowView)e.Row.DataItem;
            //        if (rowView != null)
            //        {
            //            string quotationNo = rowView["quotationno"].ToString().TrimEnd('-').TrimEnd('0').TrimEnd('1').TrimEnd('2').TrimEnd('3').TrimEnd('4').TrimEnd('5');
            //            if (ViewState["QuotationList"] != null)
            //            {
            //                List<string> quotationList = (List<string>)ViewState["QuotationList"];
            //                bool visible = true;
            //                if (quotationList.Contains(quotationNo))
            //                {
            //                    visible = false;
            //                }
            //                e.Row.Visible = visible;
            //            }
            //        }
            //    }
            //}     

            if (ddltype.SelectedValue.ToString() == "1")
            {
                DataTable dt = new DataTable();
                //SqlDataAdapter sad = new SqlDataAdapter("SELECT * FROM QuotationMain QM WHERE EXISTS (SELECT 1 FROM OAList OA WHERE OA.quotationid = QM.id) and (convert(date,createddate) BETWEEN CONVERT(DATETIME,'" + txtfromdate.Text + "',103) AND CONVERT(DATETIME,'" + txttodate.Text + "',103))", con);
                SqlDataAdapter sad = new SqlDataAdapter("SELECT * FROM QuotationMain QM  Inner Join QuotationData As QT On  QM.quotationno = QT.quotationno WHERE EXISTS (SELECT 1 FROM OAList OA WHERE OA.quotationid = QM.id) and (convert(date,createddate) BETWEEN CONVERT(DATETIME,'" + txtfromdate.Text + "',103) AND CONVERT(DATETIME,'" + txttodate.Text + "',103))", con);
                sad.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    List<string> quotationList = dt.AsEnumerable().Select(r => r.Field<string>("quotationno").TrimEnd('-').TrimEnd('0').TrimEnd('1').TrimEnd('2').TrimEnd('3').TrimEnd('4').TrimEnd('5')).ToList();

                    ViewState["QuotationList"] = quotationList;
                }
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    DataRowView rowView = (DataRowView)e.Row.DataItem;
                    if (rowView != null)
                    {
                        string quotationNo = rowView["quotationno"].ToString().TrimEnd('-').TrimEnd('0').TrimEnd('1').TrimEnd('2').TrimEnd('3').TrimEnd('4').TrimEnd('5');

                        if (ViewState["QuotationList"] != null)
                        {
                            List<string> quotationList = (List<string>)ViewState["QuotationList"];

                            bool visible = true;

                            if (quotationList.Contains(quotationNo))
                            {
                                visible = false;
                            }

                            e.Row.Visible = visible;
                        }
                    }
                }

                // The second condition
                DataTable dt2 = new DataTable();
                SqlDataAdapter sad2 = new SqlDataAdapter("SELECT QM.* FROM QuotationMain QM LEFT JOIN OAList OA ON QM.quotationno = OA.quotationno WHERE(OA.quotationid IS  NULL OR QM.quotationno IS NULL) AND(CONVERT(date, QM.createddate) BETWEEN CONVERT(DATETIME, '" + txtfromdate.Text + "', 103) AND CONVERT(DATETIME, '" + txttodate.Text + "', 103))", con);
                sad2.Fill(dt2);
                if (dt2.Rows.Count > 0)
                {
                    List<string> quotationList2 = dt2.AsEnumerable().Select(r => r.Field<string>("quotationno")).ToList();
                    ViewState["QuotationList2"] = quotationList2;
                }
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    DataRowView rowView = (DataRowView)e.Row.DataItem;
                    if (rowView != null)
                    {
                        string quotationNo = rowView["quotationno"].ToString();
                        string[] parts = quotationNo.Split('/');
                        string series = parts[0];
                        List<string> quotationList2 = (List<string>)ViewState["QuotationList2"];
                        if (quotationList2 != null)
                        {
                            foreach (string item in quotationList2)
                            {
                                string[] parts1 = item.Split('/');
                                if (parts1.Length == 2)
                                {
                                    string listSeries = parts1[0];
                                    string listSeries1 = parts1[1];

                                    if (series == listSeries)
                                    {
                                        string modifiedString = parts[1].Replace("-", "").TrimStart('0');
                                        int currentQuotationNumber = int.Parse(modifiedString);
                                        string modifiedString1 = listSeries1.Replace("-", "").TrimStart('0');
                                        int listQuotationNumber = int.Parse(modifiedString1);
                                        string ModifiedcurrentQuotationNumber = parts[1].ToString().TrimEnd('-').TrimEnd('0').TrimEnd('1').TrimEnd('2').TrimEnd('3').TrimEnd('4').TrimEnd('5');
                                        string ModifedlistQuotationNumber = listSeries1.ToString().TrimEnd('-').TrimEnd('0').TrimEnd('1').TrimEnd('2').TrimEnd('3').TrimEnd('4').TrimEnd('5');

                                        if (ModifiedcurrentQuotationNumber == ModifedlistQuotationNumber && currentQuotationNumber < listQuotationNumber)
                                        {
                                            e.Row.Visible = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            string Id = GV_QuotationF_Report.DataKeys[e.Row.RowIndex].Value.ToString();
            GridView gvPODetails = e.Row.FindControl("gvPODetails") as GridView;
            //gvPODetails.DataSource = GetData(string.Format("SELECT * FROM QuotationData WHERE quotationno =  '" + Id + "'"));
            //gvPODetails.DataBind();


            DataTable dtt = GetData(string.Format("SELECT qty, Description,rate, amount FROM QuotationData WHERE quotationno = '{0}'", Id));
            if (dtt != null && dtt.Rows.Count > 0)
            {
                gvPODetails.DataSource = dtt;
                gvPODetails.DataBind();
            }




        }
        catch (Exception ex)
        {
            //throw ex;
            string errorMsg = "An error occurred : " + ex.Message;
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('" + errorMsg + "');", true);
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


    //private void PurchaseOrderExport_Excel()
    //{
    //    // Step 1: Initialization
    //    DataTable dt = new DataTable("PurchaseOrder");

    //    // Step 2: Set Up Columns
    //    dt.Columns.Add("SNo.");
    //    dt.Columns.Add("Customer Name");
    //    dt.Columns.Add("Quotation No.");
    //    dt.Columns.Add("Quotation Date");
    //    dt.Columns.Add("Created On");
    //    dt.Columns.Add("Created By");

    //    // Add columns for gvOrders
    //    dt.Columns.Add("Qty");
    //    dt.Columns.Add("Description");

    //    // Step 3: Populate Data
    //    int SN = 0;
    //    foreach (GridViewRow row in GV_QuotationF_Report.Rows)
    //    {
    //        string CustomerName = ((Label)row.FindControl("linksname")).Text;
    //        string QuotationNo = ((Label)row.FindControl("lblInvoiceNo")).Text;
    //        string QuotationDate = ((Label)row.FindControl("lblBillDate")).Text;
    //        string CreatedOn = ((Label)row.FindControl("lblCreatedOn")).Text;
    //        string CreatedBy = ((Label)row.FindControl("lblPDCAmt")).Text;

    //        GridView gvOrderscell = (GridView)row.FindControl("gvPODetails");

    //        for (int j = 0; j < gvOrderscell.Rows.Count; j++)
    //        {
    //            // Ensure the row has enough cells
    //            if (gvOrderscell.Rows[j].Cells.Count > 2) // Adjust based on the number of columns in gvPODetails
    //            {
    //                string qty = gvOrderscell.Rows[j].Cells[0].Text == "&nbsp;" ? "" : gvOrderscell.Rows[j].Cells[0].Text;
    //                string description = gvOrderscell.Rows[j].Cells[1].Text == "&nbsp;" ? "" : gvOrderscell.Rows[j].Cells[1].Text;
    //                string Rate = gvOrderscell.Rows[j].Cells[1].Text == "&nbsp;" ? "" : gvOrderscell.Rows[j].Cells[2].Text;
    //                string Ammount = gvOrderscell.Rows[j].Cells[1].Text == "&nbsp;" ? "" : gvOrderscell.Rows[j].Cells[3].Text;
    //                dt.Rows.Add(SN++, CustomerName, QuotationNo, QuotationDate, CreatedOn, CreatedBy, qty, description, Rate,Ammount);
    //            }

    //        }
    //    }

    //    // Step 4: Export to Excel
    //    using (XLWorkbook wb = new XLWorkbook())
    //    {
    //        wb.Worksheets.Add(dt);

    //        Response.Clear();
    //        Response.Buffer = true;
    //        Response.Charset = "";
    //        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    //        Response.AddHeader("content-disposition", "attachment;filename=PurchaseOrder.xlsx");

    //        using (MemoryStream MyMemoryStream = new MemoryStream())
    //        {
    //            string path = Server.MapPath("~") + "/files/PurchaseOrder.xlsx";
    //            wb.SaveAs(MyMemoryStream);
    //            wb.SaveAs(path);
    //            MyMemoryStream.WriteTo(Response.OutputStream);
    //            Response.Flush();
    //            Response.End();
    //        }
    //    }
    //}

    private void PurchaseOrderExport_Excel()
    {
        // Step 1: Initialization
        DataTable dt = new DataTable("PurchaseOrder");

        // Step 2: Set Up Columns
        dt.Columns.Add("SNo.");
        dt.Columns.Add("Customer Name");
        dt.Columns.Add("Quotation No.");
        dt.Columns.Add("Quotation Date");
        dt.Columns.Add("Created On");
        dt.Columns.Add("Created By");
        dt.Columns.Add("Qty");
        dt.Columns.Add("Description");
        dt.Columns.Add("Rate"); // New column for Rate
        dt.Columns.Add("Amount"); // New column for Amount

        // Step 3: Populate Data
        int SN = 0; foreach (GridViewRow row in GV_QuotationF_Report.Rows)
        {
            // Retrieve parent record details
            string CustomerName = ((Label)row.FindControl("linksname")).Text;
            string QuotationNo = ((Label)row.FindControl("lblInvoiceNo")).Text;
            string QuotationDate = ((Label)row.FindControl("lblBillDate")).Text;
            string CreatedOn = ((Label)row.FindControl("lblCreatedOn")).Text;
            string CreatedBy = ((Label)row.FindControl("lblPDCAmt")).Text;

            // Access the nested GridView for child records
            GridView gvOrderscell = (GridView)row.FindControl("gvPODetails");

            // Add a parent row for the Purchase Order
            dt.Rows.Add(SN++, CustomerName, QuotationNo, QuotationDate, CreatedOn, CreatedBy, "", "", "", ""); // Empty fields for parent row

            // Loop through the child GridView to get item details
            for (int j = 0; j < gvOrderscell.Rows.Count; j++)
            {
                // Ensure the row has enough cells
                if (gvOrderscell.Rows[j].Cells.Count >= 4) // Adjust based on the number of columns in gvPODetails
                {
                    string qty = gvOrderscell.Rows[j].Cells[0].Text == "&nbsp;" ? "" : gvOrderscell.Rows[j].Cells[0].Text;
                    string description = gvOrderscell.Rows[j].Cells[1].Text == "&nbsp;" ? "" : gvOrderscell.Rows[j].Cells[1].Text;
                    string rate = gvOrderscell.Rows[j].Cells[2].Text == "&nbsp;" ? "" : gvOrderscell.Rows[j].Cells[2].Text; // Rate from the 3rd column
                    string amount = gvOrderscell.Rows[j].Cells[3].Text == "&nbsp;" ? "" : gvOrderscell.Rows[j].Cells[3].Text; // Amount from the 4th column

                    // Add a child row for each item
                    dt.Rows.Add(SN++, "", "", "", "", "", qty, description, rate, amount); // Add new fields for child rows
                }
            }
        }

        // Step 4: Export to Excel
        using (XLWorkbook wb = new XLWorkbook())
        {
            wb.Worksheets.Add(dt);

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment;filename=PurchaseOrder.xlsx");

            using (MemoryStream MyMemoryStream = new MemoryStream())
            {
                string path = Server.MapPath(" &#x7E;") + "/files/PurchaseOrder.xlsx";
                wb.SaveAs(MyMemoryStream);
                wb.SaveAs(path);
                MyMemoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }
        }
    }


}