<%@ Page Language="C#" AutoEventWireup="true" Inherits="GameForestFE.upload" CodeBehind="upload.aspx.cs" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>GameForest | Developer - Upload game</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />

    <link href="css/bootstrap.min-plum.css" rel="stylesheet" media="screen" />
    <link href="css/style.css" rel="stylesheet" />
</head>
<body>
    <div class="container">
        <br />
        <nav class="navbar navbar-default navbar-fixed-top" role="navigation">
            <div class="navbar-header">
                <button type="button" class="navbar-toggle" data-toggle="collapse" data-target=".navbar-ex1-collapse">
                    <span class="sr-only">Toggle navigation</span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                </button>
                <a class="navbar-brand" href="index.html">GameForest</a>
            </div>
            <div class="collapse navbar-collapse navbar-ex1-collapse">
                <ul class="nav navbar-nav">
                    <li><a href="index.html">Home</a></li>
                    <li><a href="games.html">Games</a></li>
                    <li><a href="dev.html">Developer</a></li>
                </ul>
                <ul class="nav navbar-nav navbar-right">
                    <li><a href="profile.html" id="linkProfile" style="display: none">user's name goes here</a></li>
                    <li><a href="index.html" id="linkLogout" style="display: none" onclick="onLogout();">Logout</a></li>
                </ul>
            </div>
        </nav>
        <div class="alert alert-danger" id="alertDialog" runat="server" style="display: none"></div>
        <div class="row" style="margin-top: 40px">
            <div class="col-sm-12">
                <div class="page-header">
                    <h1>Upload game</h1>
                </div>
                <form runat="server">
                    <div class="row">
                        <div class="col-sm-6">
                            <div class="form-group">
                                <label for="inputGameName">Game name:</label>
                                <asp:TextBox ID="inputGameName" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label for="inputMaxPlayers">Maximum allowable players:</label>
                                <asp:TextBox ID="inputMaxPlayers" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label for="inputMinPlayers">Minimum allowable players:</label>
                                <asp:TextBox ID="inputMinPlayers" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label for="inputDescription">Description of the game:</label>
                                <asp:TextBox ID="inputDescription" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="form-group" style="display: none">
                                <label for="inputUserId">User ID:</label>
                                <asp:TextBox ID="inputUserId" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group" style="display: none">
                                <label for="inputSessionId">Session ID:</label>
                                <asp:TextBox ID="inputSessionId" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <label for="fileUpload">Game .zip</label>
                            <asp:FileUpload ID="fileUpload" runat="server" />
                            <br />
                            <asp:Button ID="ButtonSubmit" runat="server" Text="Submit game" CssClass="btn btn-primary" OnClick="ButtonSubmit_Click" />
                        </div>
                    </div>
                </form>
            </div>
        </div>
    </div>
    <script src="http://code.jquery.com/jquery.js"></script>
    <script src="js/bootstrap.min.js"></script>
    <script src="js/gameforestip.js"></script>
    <script type="text/javascript">

        $(document).ready(function ()
        {
            // do a heartbeat to confirm user's session is still valid
            var showLogin = true;

            var response = $.ajax({

                url: "http://" + gameForestIP + ":1193/service/user/login?usersessionid=" + localStorage.getItem("user-session"),
                type: "PUT",
                async: true,
            });

            response.success(function (data)
            {
                if (data.ResponseType != 0)
                {
                    showLogin = true;
                    localStorage.setItem('user-session', null);
                }
                else
                {
                    showLogin = false;
                }

                if (showLogin == false)
                {
                    // then async get user's information

                    var userResponse = $.ajax({
                        url: "http://" + gameForestIP + ":1193/service/user/session/" + localStorage.getItem("user-session"),
                        type: "GET",
                        async: true,
                    });

                    userResponse.success(function (data)
                    {
                        // set user name to linkProfile
                        var adata = JSON.parse(data.AdditionalData);

                        $("#linkLogout").show();
                        $("#linkProfile").show();

                        $("#linkProfile").html(adata.Username);

                        $("#inputUserId").val(adata.UserId);
                        $("#inputSessionId").val(localStorage.getItem("user-session"));
                    });
                }
                else
                {
                    window.location.href = "index.html";
                }
            });

            response.fail(function (data)
            {
                localStorage.setItem('user-session', null);

                window.location.href = "index.html";
            });
        });

    </script>
</body>
</html>
