 new DataColumn("outwarddatetime"),
                new DataColumn("outwardqty"),
                new DataColumn("deliverydate"),
                new DataColumn("Isapprove") });

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
            foreach (GridViewRow row in dgvLaserprogram.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    chkRow = (row.Cells[1].FindControl("chkRow") as CheckBox);
                    int totalCount = dgvLaserprogram.Rows.Cast<GridViewRow>().Count(r => ((CheckBox)r.FindControl("chkRow")).Checked);
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
                            LaserID = (row.Cells[1].FindControl("lblLaserID") as Label).Text;
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

                            //DateTime OutwardDtT = DateTime.Now;//DateTime.Parse(tbOutwardDt.Text);
                            //string time = DateTime.Now.ToString();
                            string OutwardDtTime = DateTime.Now.ToString();

                            TextBox Outwardtb = (TextBox)row.Cells[1].FindControl("txtOutwardQty");
                            string[] strarr = Outwardtb.Text.Split(',');

                            string OutwardQty = strarr[1].ToString();



                            TextBox Sizetb = (TextBox)row.Cells[1].FindControl("lblSize");
                            string Size = Sizetb.Text;

                            dt.Rows.Add(OANumber, SubOA, CustName, Size, TotalQty, InwardDtTime, InwardQty, OutwardDtTime, OutwardQty, DeliveryDt);
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

                    //DateTime UpdatedDate = DateTime.Parse(UDate);
                    //DateTime CreatedDate = DateTime.Parse(CDate);
                    bool Insertdata = false;
                    foreach (DataRow row in dt.Rows)
                    {
                        con.Open();
                        if (row["inwardqty"].ToString() == row["outwardqty"].ToString())
                        {
                      