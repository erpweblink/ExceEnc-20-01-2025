asp:TemplateField>
                                        <asp:TemplateField HeaderText="Discount Amount" ItemStyle-Width="120" HeaderStyle-CssClass="gvhead">
                                            <EditItemTemplate>
                                                <asp:TextBox Text='<%# Eval("DiscountAmount") %>' CssClass="form-control" ReadOnly="true" ID="txtDiscountAmount" AutoPostBack="true" Width="100px" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblDiscountAmount" runat="Server" Text='<%# Eval("DiscountAmount") %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Grand Total" ItemStyle-Width="120" HeaderStyle-CssClass="gvhead">
                                            <EditItemTemplate>
                                                <asp:TextBox Text='<%# Eval("Alltotal") %>' CssClass="form-control" ID="txtAlltotal" Width="100px" runat="server"></asp:TextBox>
                                            </EditItemTemplate>
                                            <ItemTemplate>
                                                <asp:Label ID="lblAlltotal" runat="Server" Text='<%# Eval("Alltotal") %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Action" ItemStyle-Width="120" HeaderStyle-CssClass="gvhead">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="btn_edit" CausesValidation="false" Text="Edit" runat="server" CommandName="Edit"><i class='fas fa-edit' style='font-size:24px;color: #212529;'></i></asp:LinkButton>
                                                <asp:LinkButton runat="server" ID="lnkbtnDelete" OnClick="lnkbtnDelete_Click" ToolTip="Delete" OnClientClick="Javascript:return confirm('Are you sure to Delete?')" CausesValidation="false"><i class="fa fa-trash" style="font-size:24px"></i></asp:LinkButton>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <asp:LinkButton ID="gv_update" OnClick="gv_update_Click" Text="Update" CssClass="btn btn-primary btn-sm" runat="server"></asp:LinkButton>&nbsp;
                                                        <asp:LinkButton ID="gv_cancel" OnClick="gv_cancel_Click" CausesValidation="false" Text="Cancel" CssClass="btn btn-primary btn-sm " runat="server"></asp:LinkButton>
                                            </EditItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </div>
                        <br />
                    </div>
                </div>
                <%--Grid View End--%>

                <%--last total show--%>

                <div id="divTotalPart" runat="server" visible="false">
                    <div class="row">
                        <div class="col-md-12">
                            <div class="row">
                                <div class="col-md-6">
                                    <br />

                                    <center>
                                        <div class="col-md-12">
                                            <asp:Label ID="lbl_total_amt" runat="server" class="control-label col-sm-6">Total Amount (In Words) :<span class="spncls"></span></asp:Label><br />
                                            <asp:Label ID="lbl_total_amt_Value" ForeColor="red" class="control-label col-sm-6 font-weight-bold" runat="server" Text=""></asp:Label>
                                             <asp:HiddenField ID="hfTotal" runat="server" />
                                        </div>
                                            </center>
                                </div>
                                <div class="col-md-6" style="text-align: right">
                                    <div class="row">
                                        <div class="col-md-6">
                                            <asp:Label ID="lbl_Subtotal" runat="server" class="control-label col-sm-6">SubTotal :<span class="spncls"></span></asp:Label>
                                        </div>
                                        <div class="col-md-4">
                                            <asp:Label runat="server" class="control-label col-sm-6" ReadOnly="true" ID="txt_Subtotal" Text="0.00"></asp:Label><br />
                                        </div>
                                    </div>
                                    <asp:Panel ID="taxPanel1" runat="server">
                                        <div class="row">
                                            <div class="col-md-6">
                                                <asp:Label ID="lbl_cgst9" runat="server" class="control-label col-sm-6">CGST  Amount :<span class="spncls"></span></asp:Label>
                                            </div>
                                            <div class="col-md-4">
                                                <asp:Label runat="server" class="control-label col-sm-6" ReadOnly="true" ID="txt_cgstamt" Text="0.00"></asp:Label><br />
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-md-6">
                                                <asp:Label ID="lbl_sgst9" runat="server" class="control-label col-sm-6">SGST  Amount :<span class="spncls"></span></asp:Label>
                                            </div>
                                            <div class="col-md-4">
                                                <asp:Label runat="server" class="control-label col-sm-6" ReadOnly="true" ID="txt_sgstamt" Text="0.00"></asp:Label><br />
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-md-6">
                                                <asp:Label ID="lbligst" runat="server" class="control-label col-sm-6">IGST  Amount :<span class="spncls"></span></asp:Label>
                                            </div>
                                            <div class="col-md-4">
                                                <asp:Label runat="server" class="control-label col-sm-6" ReadOnly="true" ID="txt_igstamt" Text="0.00"></asp:Label><br />
                                            </div>
                                        </div>
                                    </asp:Panel>
                                    <div class="row">
                                        <div class="col-md-6">
                                            <asp:Label ID="lbl_grandTotal" runat="server" class="control-label col-sm-6">Grand Total :<span class="spncls"></span></asp:Label>
                                        </div>
                                        <div class="col-md-4">
                                            <asp:Label runat="server" class="control-label col-sm-6" ReadOnly="true" ID="txt_grandTotal" Text="0.00"></asp:Label><br />
                                        </div>
                                    </div>
                                </div>

                            </div>
                        </div>
                    </div>
                </div>
                <%--  last total show--%>



                <br />
                <div class="row">
                    <div class="col-md-4"></div>
                    <div class="col-6 col-md-2">
                        <asp:Button ID="btnsave" OnClick="btnsave_Click" CssClass="form-control btn btn-outline-primary m-2" runat="server" Text="Save" />
                    </div>
                    <div class="col-6 c