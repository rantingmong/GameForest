<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="update.aspx.cs" Inherits="GameForestFE.update" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>GameForest | Developer - Update game</title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />

    <link href="css/bootstrap.min-plum.css" rel="stylesheet" media="screen" />
    <link href="css/style.css" rel="stylesheet" />
</head>
<body>
    <div class="container">
        <br />
        <nav class="navbar navbar-default" role="navigation">
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
                    <li><a href="#" id="fbLogout" style="display: none">Logout (FB User)</a></li>
                    <li><a href="index.html" id="linkLogout" style="display: none" onclick="onLogout();">Logout</a></li>
                </ul>
            </div>
        </nav>
        <div class="alert alert-danger" id="alertDialog" runat="server" style="display: none"></div>
        <div class="row">
            <div class="col-sm-12">
                <div class="page-header">
                    <h1>Update game <small id="gameName" runat="server">the game</small></h1>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-12">
                <form runat="server">
                    <div class="form-group">
                        <label for="inputGameName">Game name:</label>
                        <asp:TextBox ID="inputGameName" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label for="inputGameDescription">Game description:</label>
                        <asp:TextBox ID="inputGameDescription" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label for="inputMinPlayers">Minimum players:</label>
                        <asp:TextBox ID="inputMinPlayers" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label for="inputMaxPlayers">Maximum players:</label>
                        <asp:TextBox ID="inputMaxPlayers" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <label for="fileUpload">Updated game .zip</label>
                    <asp:FileUpload ID="fileUpload" runat="server" />
                    <br />
                    <asp:Button ID="ButtonSubmit" runat="server" Text="Submit game" CssClass="btn btn-primary" OnClick="ButtonSubmit_Click"/>
                </form>
            </div>
        </div>
    </div>
    <script src="js/jquery.js"></script>
    <script src="js/bootstrap.min.js"></script>
    <script src="js/angular.js"></script>
</body>
</html>
