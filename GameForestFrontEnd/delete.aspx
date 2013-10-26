<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="delete.aspx.cs" Inherits="GameForestFE.delete" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>GameForest | Developer - Delete game</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />

    <link href="css/bootstrap.min-plum.css" rel="stylesheet" media="screen" />
    <link href="css/style.css" rel="stylesheet" />
</head>
<body>
    <div class="container">
        s
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
                    <li><a href="index.html" id="linkLogout" style="display: none" onclick="logOut(event)">Logout</a></li>
                </ul>
            </div>
        </nav>
        <div class="row" style="margin-top: 40px">
            <div class="col-sm-12">
                <div class="page-header">
                    <h1>Delete game <small id="gameName" runat="server">the game</small></h1>
                </div>
            </div>
        </div>
        <div class="row" id="rowDelete" runat="server">
            <div class="col-sm-6">
                <h3>Are you sure you want to delete this game? You will not be able to recover your game once you pressed &quot;Delete game&quot;.</h3>
                <div class="row">
                    <br />
                    <div class="col-sm-6">
                        <form runat="server">
                            <asp:Button ID="buttonDelete" runat="server" Text="Delete game" CssClass="btn btn-danger" OnClick="buttonDelete_Click" />
                            <a class="btn btn-primary" href="dev.html">Cancel</a>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <div class="row" id="rowError" runat="server" style="display: none">
            <div class="col-sm-6">
                <h3 id="errorHeader" runat="server"></h3>
                <br />
                <a class="btn btn-primary" href="dev.html">Go back</a>
            </div>
        </div>
    </div>
    <!--TODO: update this not to make sira sira-->
    <script src="js/jquery.js"></script>
    <script src="js/bootstrap.min.js"></script>
    <script src="js/angular.js"></script>
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

        function logOut(e)
        {
            console.log("Logging you out...");

            var request = $.ajax(
                {
                    url: "http://" + gameForestIP + ":1193/service/user/login?usersessionid=" + localStorage.getItem("user-session"),
                    type: "DELETE",
                    async: false
                });

            request.success(function (data)
            {
                if (data.ResponseType == 0)
                {
                    localStorage.setItem("user-session", null);
                    window.location.reload(true);
                }
            });
        }

    </script>
</body>
</html>
