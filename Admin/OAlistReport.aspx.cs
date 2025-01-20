using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Admin_OAlistReport : System.Web.UI.Page
{
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
                GVBinddata();
            }
        }

    }


    protected void GVBinddata()
    {
        try
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand("[ExcelEncLive].[SP_OAListReport]", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Action", "GetList"));
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable Dt = new DataTable();
                    adapter.Fill(Dt);
                    if (Dt.Rows.Count > 0)
                    {
                        GVReports.DataSource = Dt;
                        GVReports.DataBind();

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

    protected void BtnExcell_Click(object sender, EventArgs e)
    {
        try
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("[ExcelEncLive].[SP_OAListReport]", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "GetList");
                    connection.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        GVReports.DataSource = dt;
                        GVReports.DataBind();
                        connection.Close();
                        Response.Clear();
                        DateTime now = DateTime.Today;
                        string filename = " OA List Report " + now.ToString("dd/MM/yyyy");
                        Response.AddHeader("content-disposition", "attachment; filename = '" + filename + "'.xls");
                        Response.ContentType = "application/vnd.xls";
                        System.IO.StringWriter stringWrite = new System.IO.StringWriter();
                        System.Web.UI.HtmlTextWriter htmlWrite =
                        new HtmlTextWriter(stringWrite);
                        GVReports.RenderControl(htmlWrite);
                        Response.Write(stringWrite.ToString());
                        Response.End();
                    }
                }
            }
        }

        catch (Exception ex)
        {
            throw ex;
        }
    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
           server control at run time. */
    }
}