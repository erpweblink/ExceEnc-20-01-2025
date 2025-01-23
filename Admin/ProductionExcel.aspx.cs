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

public partial class Admin_ProductionExcel : System.Web.UI.Page
{
    SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString);

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

                if (Request.QueryString["Dep"] != null)
                {
                    string Dep = Server.UrlDecode(Request.QueryString["Dep"]);
                    string Customer = Server.UrlDecode(Request.QueryString["Customer"]);
                    if (Customer == null && Dep == "Drawaing")
                    {

                        GetDrawingData();
                    }
                    else if (Customer != null && Dep == "Drawaing")
                    {
                        ViewState["Customer"] = Customer.ToString();
                        GetDrawingDataCustomerwise();

                    }
                    else if (Customer == null && Dep == "Laser")
                    {

                        GetLaserData();
                    }
                    else if (Customer != null && Dep == "Laser")
                    {
                        ViewState["Customer"] = Customer.ToString();
                        GetLaserDataCustomerwise();

                    }

                    else if (Customer == null && Dep == "Cutting")
                    {

                        GetCuttingData();
                    }
                    else if (Customer != null && Dep == "Cutting")
                    {
                        ViewState["Customer"] = Customer.ToString();
                        GetCuttingDataCustomerwise();

                    }



                    else if (Customer == null && Dep == "CNCBEN")
                    {

                        GetCNCBendingData();
                    }
                    else if (Customer != null && Dep == "CNCBEN")
                    {
                        ViewState["Customer"] = Customer.ToString();
                        GetCNCBendingDataCustomerwiese();

                    }


                    else if (Customer == null && Dep == "Welding")
                    {

                        GetweldingData();
                    }
                    else if (Customer != null && Dep == "Welding")
                    {
                        ViewState["Customer"] = Customer.ToString();
                        GetweldingDataCustomerwise();
                    }

                    else if (Customer == null && Dep == "Powder")
                    {

                        GetPowderData();
                    }
                    else if (Customer != null && Dep == "Powder")
                    {
                        ViewState["Customer"] = Customer.ToString();
                        GetPowderDataCustomerwise();

                    }


                    else if (Customer == null && Dep == "Finalassembly")
                    {
                        GetAssemblyData();
                    }
                    else if (Customer != null && Dep == "Finalassembly")
                    {
                        ViewState["Customer"] = Customer.ToString();
                        GetAssemblyDataCustomerwise();
                    }


                    else if (Customer == null && Dep == "STOCK")
                    {
                        GetStockData();
                    }
                    else if (Customer != null && Dep == "STOCK")
                    {
                        ViewState["Customer"] = Customer.ToString();
                        GetStockDataCustomerwise();
                    }
                }
                else
                {

                }
            }
        }
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
           server control at run time. */
    }
    protected void GetDrawingData()
    {
        try
        {
            string query = string.Empty;
            //            query = @"SELECT [VWID],[id] as mainID, [OAno],[currentdate],[customername],[deliverydatereqbycust],[IsDrawingcomplete],[Description],[Qty],[Price],[Discount]
            //,[TotalAmount],[CGST],[SGST],[IGST],[SubOANumber] FROM vwOrderAccept where IsComplete is null order by deliverydatereqbycust asc";

            query = @"SELECT [pono],[id] as mainID,[OANumber],[Size],Basic,OACreationDate,[TotalQty],[InwardDtTime],[InwardQty], [InwardQty] As OutwardQty ,[deliverydatereqbycust],[customername],SubOA
               FROM [ExcelEncLive].[vwDrawerCreation] where IsComplete is null order by deliverydatereqbycust asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = "DrawingReport" + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    protected void GetLaserData()
    {
        try
        {
            string query = string.Empty;
            query = @"

SELECT [LaserProgId],
       [OANumber],
       [SubOA],
        LP.[CustomerName],
       [Size],
       [TotalQty],
       [InwardDtTime],
       [InwardQty],
       [OutwardDtTime],
       [OutwardQty],
       [DeliveryDate]  As deliverydatereqbycust,
       [IsApprove],
       [IsPending],
       [IsCancel],
       LP.[CreatedBy],
       [CreatedDate],
       LP.[UpdatedBy],
       [UpdatedDate],
	   o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].[tblLaserPrograming] AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
where IsApprove = 1
      and LP.IsComplete is null
order by CONVERT(DateTime, DeliveryDate, 103) asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = "Laser Programing Report" + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    protected void GetCuttingData()
    {
        try
        {
            string query = string.Empty;
            query = @"SELECT LaserCutId,
       [OANumber],
       [SubOA],
        LP.[CustomerName],
       [Size],
       [TotalQty],
       [InwardDtTime],
       [InwardQty],
       [OutwardDtTime],
       [OutwardQty],
       [DeliveryDate]  As deliverydatereqbycust,
       [IsApprove],
       [IsPending],
       [IsCancel],
       LP.[CreatedBy],
       [CreatedDate],
       LP.[UpdatedBy],
       [UpdatedDate],
	   o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].tblLaserCutting AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
where IsApprove = 1
      and LP.IsComplete is null 
order by CONVERT(DateTime, DeliveryDate, 103) asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = "Laser Cutting Report" + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    protected void GetCNCBendingData()
    {
        try
        {
            string query = string.Empty;
            query = @"SELECT [CNCBendingId],[OANumber],[SubOA],LP.[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate] As deliverydatereqbycust,[IsApprove],[IsPending],[IsCancel],LP.[CreatedBy],[CreatedDate],LP.[UpdatedBy],[UpdatedDate] ,
             
                 o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].tblCNCBending AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
where IsApprove = 1
      and LP.IsComplete is null 
order by CONVERT(DateTime, DeliveryDate, 103) asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = "CNCBending Report" + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    protected void GetweldingData()
    {
        try
        {
            string query = string.Empty;
            query = @"SELECT [WeldingId],[OANumber],[SubOA], LP.[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate] As deliverydatereqbycust,[IsApprove],[IsPending],[IsCancel], LP.[CreatedBy],[CreatedDate], LP.[UpdatedBy],[UpdatedDate] ,
                  o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].tblWelding AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
where IsApprove = 1
      and LP.IsComplete is null 
order by CONVERT(DateTime, DeliveryDate, 103) asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = "Welding Report" + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    protected void GetPowderData()
    {
        try
        {
            string query = string.Empty;

            query = @"SELECT PowdercoatId,[OANumber],[SubOA],LP.[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate] As deliverydatereqbycust,[IsApprove],[IsPending],[IsCancel],LP.[CreatedBy],[CreatedDate],LP.[UpdatedBy],[UpdatedDate] ,
             
                 o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].tblPowderCoating AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
where IsApprove = 1
      and LP.IsComplete is null 
order by CONVERT(DateTime, DeliveryDate, 103) asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = "Powder Cutting Report" + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    protected void GetAssemblyData()
    {
        try
        {
            string query = string.Empty;

            query = @"SELECT FinalAssemblyId,[OANumber],[SubOA],LP.[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate] As deliverydatereqbycust,[IsApprove],[IsPending],[IsCancel],LP.[CreatedBy],[CreatedDate],LP.[UpdatedBy],[UpdatedDate] ,
             
                 o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].tblfinalassembly AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
where IsApprove = 1
      and LP.IsComplete is null 
order by CONVERT(DateTime, DeliveryDate, 103) asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = "Final Assembly Report" + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    protected void GetStockData()
    {
        try
        {
            string query = string.Empty;

            query = @"SELECT StockId,[OANumber],[SubOA],LP.[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate] As deliverydatereqbycust,[IsApprove],[IsPending],[IsCancel],LP.[CreatedBy],[CreatedDate],LP.[UpdatedBy],[UpdatedDate] ,
             
                 o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].tblStock AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
where IsApprove = 1
      and LP.IsComplete is null 
order by CONVERT(DateTime, DeliveryDate, 103) asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = "STOCK Report" + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    protected void GetDrawingDataCustomerwise()
    {
        try
        {
            string query = string.Empty;
            String Customer = ViewState["Customer"].ToString();
            //            query = @"SELECT [VWID],[id] as mainID, [OAno],[currentdate],[customername],[deliverydatereqbycust],[IsDrawingcomplete],[Description],[Qty],[Price],[Discount]
            //,[TotalAmount],[CGST],[SGST],[IGST],[SubOANumber] FROM vwOrderAccept where IsComplete is null order by deliverydatereqbycust asc";

            query = @"SELECT [pono],[id] as mainID,[OANumber],[Size],[TotalQty],[InwardDtTime],Basic,OACreationDate,[InwardQty],[deliverydatereqbycust],[customername],SubOA
               FROM [ExcelEncLive].[vwDrawerCreation] where IsComplete is null and customername like '" + Customer + "%'   order by deliverydatereqbycust asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = Customer + " DrawingReport " + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void GetLaserDataCustomerwise()
    {
        try
        {
            string query = string.Empty;
            String Customer = ViewState["Customer"].ToString();
            query = @"SELECT [LaserProgId],
       [OANumber],
       [SubOA],
        LP.[CustomerName],
       [Size],
       [TotalQty],
       [InwardDtTime],
       [InwardQty],
       [OutwardDtTime],
       [OutwardQty],
       [DeliveryDate]  As deliverydatereqbycust,
       [IsApprove],
       [IsPending],
       [IsCancel],
       LP.[CreatedBy],
       [CreatedDate],
       LP.[UpdatedBy],
       [UpdatedDate],
	   o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].[tblLaserPrograming] AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
                WHERE LP.IsApprove = 1
                AND LP.CustomerName LIKE '" + Customer + @"%'
                AND LP.IsComplete IS NULL 
                ORDER BY CONVERT(DateTime, LP.DeliveryDate, 103) ASC";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = Customer + " Laser Programing Report " + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void GetCuttingDataCustomerwise()
    {
        try
        {
            string query = string.Empty;
            String Customer = ViewState["Customer"].ToString();
            query = @"SELECT LaserCutId,
       [OANumber],
       [SubOA],
        LP.[CustomerName],
       [Size],
       [TotalQty],
       [InwardDtTime],
       [InwardQty],
       [OutwardDtTime],
       [OutwardQty],
       [DeliveryDate]  As deliverydatereqbycust,
       [IsApprove],
       [IsPending],
       [IsCancel],
       LP.[CreatedBy],
       [CreatedDate],
       LP.[UpdatedBy],
       [UpdatedDate],
	   o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].tblLaserCutting AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
                WHERE LP.IsComplete IS NULL
                AND LP.CustomerName LIKE '" + Customer + @"%'
                ORDER BY CONVERT(DateTime, DeliveryDate, 103) ASC";


            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = Customer + " Laser Cutting Report " + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }


    protected void GetCNCBendingDataCustomerwiese()
    {
        try
        {
            string query = string.Empty;
            String Customer = ViewState["Customer"].ToString();
            query = @"SELECT [CNCBendingId],[OANumber],[SubOA],LP.[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate] As deliverydatereqbycust,[IsApprove],[IsPending],[IsCancel],LP.[CreatedBy],[CreatedDate],LP.[UpdatedBy],[UpdatedDate] ,
             
                 o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].tblCNCBending AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
                WHERE LP.IsComplete IS NULL 
                AND LP.CustomerName LIKE '" + Customer + @"%'
                ORDER BY CONVERT(DateTime, DeliveryDate, 103) ASC";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = Customer + " CNCBending Report " + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void GetweldingDataCustomerwise()
    {
        try
        {
            string query = string.Empty;
            String Customer = ViewState["Customer"].ToString();
            query = @"SELECT [WeldingId],[OANumber],[SubOA], LP.[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate] As deliverydatereqbycust,[IsApprove],[IsPending],[IsCancel], LP.[CreatedBy],[CreatedDate], LP.[UpdatedBy],[UpdatedDate] ,
                  o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].tblWelding AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
                WHERE LP.IsComplete IS NULL 
                AND LP.CustomerName LIKE '" + Customer + @"%'
                ORDER BY CONVERT(DateTime, DeliveryDate, 103) ASC";


            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = Customer + " Welding Report " + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void GetPowderDataCustomerwise()
    {
        try
        {
            string query = string.Empty;
            String Customer = ViewState["Customer"].ToString();
            query = @"SELECT PowdercoatId,[OANumber],[SubOA],LP.[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate] As deliverydatereqbycust,[IsApprove],[IsPending],[IsCancel],LP.[CreatedBy],[CreatedDate],LP.[UpdatedBy],[UpdatedDate] ,
             
                 o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].tblPowderCoating AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
where IsApprove = 1
      and LP.IsComplete is null 
 AND LP.[CustomerName] like '" + Customer + "%'  order by CONVERT(DateTime, DeliveryDate,103) asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = Customer + " Powder Cutting Report " + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void GetAssemblyDataCustomerwise()
    {
        try
        {
            string query = string.Empty;
            String Customer = ViewState["Customer"].ToString();
            query = @"SELECT FinalAssemblyId,[OANumber],[SubOA],LP.[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate] As deliverydatereqbycust,[IsApprove],[IsPending],[IsCancel], LP.[CreatedBy],[CreatedDate],LP.[UpdatedBy],[UpdatedDate] ,
             
                 o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM[dbo].tblfinalassembly AS LP
left join orderaccept AS O ON LP.oanumber = o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA = OA.SubOANumber
where IsApprove = 1
      and LP.IsComplete is null AND LP.Customername like '" + Customer + "%'order by CONVERT(DateTime, DeliveryDate, 103) asc";



            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = Customer + " Final Assembly Report " + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void GetStockDataCustomerwise()
    {
        try
        {
            string query = string.Empty;
            String Customer = ViewState["Customer"].ToString();
            query = @"SELECT StockId,[OANumber],[SubOA],LP.[CustomerName],[Size],[TotalQty],[InwardDtTime],[InwardQty],[OutwardDtTime],[OutwardQty],
                [DeliveryDate] As deliverydatereqbycust,[IsApprove],[IsPending],[IsCancel],LP.[CreatedBy],[CreatedDate],LP.[UpdatedBy],[UpdatedDate] ,
             
                 o.CreatedOn AS OACreationDate,
	    (Try_CAST(OA.Qty AS DECIMAL(10, 2)) * Try_CAST(OA.Price AS DECIMAL(10, 2))) AS Basic

FROM [dbo].tblStock AS LP
left join orderaccept AS O ON LP.oanumber=o.oano
left join ExcelEncLive.OrderAcceptDtls AS OA ON  LP.SubOA=OA.SubOANumber
where IsApprove = 1
      and LP.IsComplete is null AND LP.Customername like '"+Customer+"%'order by CONVERT(DateTime, DeliveryDate, 103) asc";

            SqlDataAdapter ad = new SqlDataAdapter(query, con);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GV_QuotationF_Report.DataSource = dt;
                GV_QuotationF_Report.DataBind();
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "Key", "<script>MakeStaticHeader('" + GV_QuotationF_Report.ClientID + "', 900, 1020 , 40 ,true); </script>", false);

                Response.Clear();
                DateTime now = DateTime.Today;
                string filename = Customer + " STOCK Report " + now.ToString("dd/MM/yyyy");
                Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                Response.ContentType = "application/vnd.xls";
                System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                System.Web.UI.HtmlTextWriter htmlWrite =
                new HtmlTextWriter(stringWrite);
                GV_QuotationF_Report.RenderControl(htmlWrite);
                Response.Write(stringWrite.ToString());
                Response.End();
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
}
