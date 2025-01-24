    cmd.Parameters.AddWithValue("@isdeleted", '0');
                    cmd.Parameters.AddWithValue("@ProductFault", txtproductfaulty.Text);
                    cmd.Parameters.AddWithValue("@RepeatedNo", txtrepeatedno.Text);
                    cmd.Parameters.AddWithValue("@RepeatedDate", txtrepeateddate.Text);
                    cmd.Parameters.AddWithValue("@RepeatedJobNo", txtrepetedjob.Text);
                    cmd.Parameters.AddWithValue("@Date", txtdate.Text);
                    cmd.Parameters.AddWithValue("@Branch", txtbranch.Text);
                    cmd.Parameters.AddWithValue("@otherinfo", txtotherinfo.Text);
                    cmd.Parameters.AddWithValue("@ServiceType", ddlservicetype.SelectedItem.Text);
                    cmd.Parameters.AddWithValue("@Services", txtservices.Text);
                    cmd.Parameters.AddWithValue("@CustChallnno", txtcustomerno.Text);
                    if (FileUpload.HasFile)
                    {
                        var Filenamenew = FileUpload.FileName;
                        string codenew = Guid.NewGuid().ToString();
                        Path = Server.MapPath("~/ProductImg/") + codenew + "_" + Filenamenew;
                        FileUpload.SaveAs(Path);
                        cmd.Parameters.AddWithValue("@Imagepath", "~/ProductImg/" + codenew + "_" + Filenamenew);
                    }
                   // cmd.Parameters.AddWithValue("@Imagepath", "~/ProductImg/" + FileUpload.FileName);
                    cmd.Parameters.AddWithValue("@Action", "Insert");
                    cmd.ExecuteNonQuery();
                    con.Close();
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "HideLabel('Data Saved Successfully');", true);
                }
                else
                {
                    QuatationNo = string.Empty;
                }
            }
        }
        else if (btnSubmit.Text == "Update")
        {
            DateTime Date = DateTime.Now;
            DateTime dat = Convert.ToDateTime(txtDateIn.Text);
            con.Open();
            SqlCommand cmd1 = new SqlCommand("SP_InwardEntry", con);
            cmd1.CommandType = CommandType.StoredProcedure;

            cmd1.Parameters.AddWithValue("@JobNo", txtJobNo.Text);
            cmd1.Parameters.AddWithValue("@DateIn", dat);
            cmd1.Parameters.AddWithValue("@CustName", txtcustomername.Text);
            cmd1.Parameters.AddWithValue("@MateName", txtproductname.Text);
            cmd1.Parameters.AddWithValue("@SrNo", txtSrNo.Text);
            cmd1.Parameters.AddWithValue("@Subcustomer", txtsubcust.Text);
            cmd1.Parameters.AddWithValue("@MateStatus", txtMateriStatus.Text);
            cmd1.Parameters.AddWithValue("@FinalStatus", txtfinalstatus.Text);
            //cmd1.Parameters.AddWithValue("@TestBy", DropDownListtest.SelectedItem.Text);
            cmd1.Parameters.AddWithValue("@ModelNo", txtModelNo.Text);

            cmd1.Parameters.AddWithValue("@CreatedBy", Session["adminname"].ToString());
            cmd1.Parameters.AddWithValue("@CreatedDate", Date);
            cmd1.Parameters.AddWithValue("@UpdateBy", Session["adminname"].ToString());
            cmd1.Parameters.AddWithValue("@UpdateDate", Date);
            cmd1.Parameters.AddWithValue("@isdeleted", '0');
            cmd1.Parameters.AddWithValue("@ProductFault", txtproductfaulty.Text);
            cmd1.Parameters.AddWithValue("@RepeatedNo", txtrepeatedno.Text);
            cmd1.Parameters.AddWithValue("@RepeatedDate", txtrepeateddate.Text);
            cmd1.Parameters.AddWithValue("@RepeatedJobNo", txtrepetedjob.Text);
            cmd1.Parameters.AddWithValue("@Date", txtdate.Text);
            cmd1.Parameters.AddWithValue("@Branch", txtbranch.Text);
            cmd1.Parameters.AddWithValue("@otherinfo", txtotherinfo.Text);
            cmd1.Parameters.AddWithValue("@ServiceType", ddlservicetype.SelectedItem.Text);
            cmd1.Parameters.AddWithValue("@Services", txtservices.Text);
            cmd1.Parameters.AddWithValue("@CustChallnno", txtcustomerno.Text);
            if (FileUpload.HasFile)
            {
                var Filenamenew = FileUpload.FileName;
                string codenew = Guid.NewGuid().ToString();
                Path = Server.MapPath("~/ProductImg/") + codenew + "_" + Filenamenew;
                FileUpload.SaveAs(Path);
                cmd1.Parameters