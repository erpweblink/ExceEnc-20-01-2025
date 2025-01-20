                                   </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Size" ItemStyle-Width="200" ItemStyle-HorizontalAlign="Center" Visible="true">
                                                        <ItemTemplate>
                                                            <asp:TextBox runat="server" ID="lblSize" ReadOnly="true" TextMode="MultiLine" Rows="5" Width="100%" CssClass="form-control" Text='<%# Eval("Size").ToString().Replace("<br>", " ") %>'></asp:TextBox>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Total Quantity" ItemStyle-Width="150" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblTotalQty" runat="server" Text='<%# Eval("TotalQty") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Delivery Date" ItemStyle-Width="150" ItemStyle-HorizontalAlign="Center" Visible="true">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblDeliveryDt" runat="server" Text='<%# Eval("DeliveryDate","{0:dd/MM/yyyy}") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Inward Date and Time" ItemStyle-Width="150" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblInwardDtTime" runat="server" Text='<%# Eval("InwardDtTime") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="InQty" ItemStyle-Width="150" ItemStyle-HorizontalAlign="Center">
                                                        <ItemTemplate>
                                                            <asp:TextBox runat="server" ID="txtInwardQty" Width="100%" ReadOnly="true" CssClass="form-control" Text='<%# Eval("InwardQty") %>'></asp:TextBox>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>

                                                    <asp:TemplateField HeaderText="OutDtTime" ItemStyle-Width="150" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="txtOutwardDtTime" runat="server" ReadOnly="false" CssClass="form-control myDate"></asp:TextBox>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="OutQty" ItemStyle-Width="150" ItemStyle-HorizontalAlign="Center">
                                                        <ItemTemplate>
                                                            <asp:TextBox runat="server" Width="100%" ID="txtOutwardQty" onkeypress="return ValidNumeric()" onblur="Keydown(this);" AutoPostBack="false" CssClass="form-control" Text='<%# Eval("InwardQty") %>'></asp:TextBox>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                    <asp:TemplateField HeaderText="Action" ItemStyle-Width="100" ItemStyle-HorizontalAlign="Center" Visible="false">
                                                        <ItemTemplate>
                                                            <a href="<%#string.Format("../Reports/ReportPDF.aspx?Dept="+encrypt("Laser Programing")+"&OANumberWise={0}", encrypt(Eval("OANumber").ToString())) %>" target="_blank">
                                                                <asp:Image ID="ImgPrint" ru