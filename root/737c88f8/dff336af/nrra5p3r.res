        ��  ��                  /       �����e                 

    <div class="page-wrapper">
        <div class="page-body">
            <div class="row">
                <div class="col-md-7">
                    <div class="page-header-breadcrumb">
                        <div style="float: left; font-size: 15px;">
                            <span><i class="feather icon-home"></i>&nbsp;Add User</span>
                        </div>
                    </div>
                </div>

                <div class="col-md-5">
                    
                </div>
            </div>

            <div class="container py-3">
                <div class="card">
                    <div class="card-header bg-primary text-uppercase text-white">
                        <h5><i class="fa fa-user-plus"></i>Add User</h5>
                    </div>

                    <div class="row">
                        <div class="col-xl-12 col-md-12">
                            <div class="card">
                                <div class="card-header">
                                    <div class="row">
                                        <div class="col-md-12">
                                            <br />
                                            <br />
                                            <div class="row">
                                                <div class="col-md-2 spancls">Name<i class="reqcls">*&nbsp;</i> : </div>
                                                <div class="col-md-4">
                                                    
                                                </div>
                                                <div class="col-md-2 spancls">Email<i class="reqcls">*&nbsp;</i> : </div>
                                                <div class="col-md-4">
                                                    
                                                </div>
                                            </div>
                                            <br />

                                            <div class="row">
                                                <div class="col-md-2 spancls">Email Password<i class="reqcls">*&nbsp;</i> : </div>
                                                <div class="col-md-4">
                                                    
                                                </div>
                                                <div class="col-md-2 spancls">Panel Password<i class="reqcls">*&nbsp;</i> : </div>
                                                <div class="col-md-4">
                                                    
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-md-2"></div>
                                                <div class="col-md-4">
                                                    &nbsp;<input type="checkbox" onclick="ShowPsw1()">&nbsp;&nbsp;<p style="font-size: 12px; display: inline;">Show Password</p>
                                                </div>
                                                <div class="col-md-2"></div>
                                                <div class="col-md-4">
                                                    &nbsp;<input type="checkbox" onclick="ShowPsw2()">&nbsp;&nbsp;<p style="font-size: 12px; display: inline;">Show Password</p>
                                                </div>
                                            </div>
                                            <br />
                                            <script>
                                                function ShowPsw1() {
                                                    var x = document.getElementById("ContentPlaceHolder1_txtemailpsw");
                                                    if (x.type === "password") {
                                                        x.type = "text";
                                                    } else {
                                                        x.type = "password";
                                                    }
                                                }

                                                function ShowPsw2() {
                                                    var x = document.getElementById("ContentPlaceHolder1_txtpanelpsw");
                                                    if (x.type === "password") {
                                                        x.type = "text";
                                                    } else {
                                                        x.type = "password";
                                                    }
                                                }
                                            </script>
                                            <div class="row">
                                                <div class="col-md-2 spancls">Mobile : </div>
                                                <div class="col-md-4">
                                                    
                                                </div>
                                                <div class="col-md-2 spancls">Department<i class="reqcls">*&nbsp;</i> : </div>
                                                <div class="col-md-4">
                                                    
                                                </div>
                                            </div>
                                            <br />

                                            <div class="row">
                                                <div class="col-md-2 spancls">Status<i class="reqcls">*&nbsp;</i> : </div>
                                                <div class="col-md-4">
                                                    
                                                </div>
                                              <div class="col-md-2 spancls">Role</div>
                                                <div class="col-md-4">
                                                    
                                            <br />
                                            <div class="row">
                                                <div class="col-md-2"></div>
                                                <div class="col-md-2">
                                                    <center> </center>
                                                </div>
                                                <div class="col-md-6"></div>

                                            </div>
                                            <br />

                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <h3 class="container"><span class="starcls"><i class="feather icon-list"></i>&nbsp;Users List</span></h3>

                    <div class="row">
                        <div class="col-xl-12 col-md-12">
                            <div class="card">
                                <div class="card-header">
                                    <div class="row">
                                        <div class="col-md-12">
                                            <br />
                                            <div class="dt-responsive table-responsive">
                                                
                                            </div>
                                            <br />

                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <br />
                </div>
            </div>
        </div>
    </div>



    