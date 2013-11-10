/// <reference path="jquery.js" />
/// <reference path="promise.js" />
/// <reference path="guid.js" />
/// <reference path="GameForest.js" />
/// <reference path="GameForestTracking.js" />

var userUpdateInterval = null;

var gf = new GameForest(new Guid(localStorage.getItem("lobby-game")), new Guid(localStorage.getItem("lobby-session")), new Guid(localStorage.getItem("user-session")));

$("#gfxButtonLeave").click(function ()
{
    window.location.href = "../../../games.html";
});

$(document).ready(function ()
{
    var getLobbyRequest = $.ajax({
        url: "http://" + GameForestCloudUrl + ":1193/service/lobby/" + localStorage.getItem("lobby-session"),
        type: "GET",
        async: true
    });

    getLobbyRequest.success(function (result)
    {
        if (result.ResponseType == 0)
        {
            var lobbyData = JSON.parse(result.AdditionalData);
            $("#gameForestLobbyName").html(lobbyData.Name);
        }
    });

    userUpdateInterval = setInterval(updateUserList, 1000);

    // start game forest
    gf.start();

    // start chat services
    connect();

    var isowner = localStorage.getItem("lobby-owner");

    if (isowner == "true")
    {
        $("#gfxButtonStart").removeAttr("disabled");
        $("#gfxButtonStart").html("Start game");

        $("#gfxButtonStart").click(function ()
        {
            $("#gfxButtonStart").prop("disabled", true);
            $("#gfxButtonStart").html("Waiting for other players...");

            gf.startGame();
        });
    }
    else
    {
        $("#gfxButtonStart").html("Ready");

        $("#gfxButtonStart").click(function ()
        {
            gf.readyGame();
        });
    }

    onDocumentReady();
});

function unload             ()
{
    $.ajax({
        url: "http://" + GameForestCloudUrl + ":1193/service/lobby/join?usersessionid=" + localStorage.getItem("user-session"),
        type: "DELETE",
        async: false
    });

    gf.stop();
}

function updateUserList     ()
{
    var response = $.ajax({
        url: "http://" + GameForestCloudUrl + ":1193/service/lobby/users?lobbyid=" + localStorage.getItem("lobby-session") + "&usersessionid=" + localStorage.getItem("user-session"),
        type: "GET",
        async: true
    });

    response.success(function (data)
    {
        if (data.ResponseType == 0)
        {
            $("#gfxUserList").html("");

            var userList = JSON.parse(data.AdditionalData);

            for (var i = 0; i < userList.length; i++)
            {
                $("#gfxUserList").append("<div class='panel-body'><span class='glyphicon glyphicon-user' style='margin-right: 10px'></span>" + userList[i].Username + "</div>");
            }
        }
    });
}

function connect            ()
{
    document.getElementById("chattext").onkeydown = keydownHandler;

    var socket = new WebSocket('ws://' + GameForestCloudUrl + ':8085/');

    socket.onopen       = function ()
    {
        $("#chatbox").append("Connected\n");

        var mess = new Object();

        mess.Lobby      = localStorage.getItem("lobby-session");
        mess.Message    = "Mess";
        mess.Mess       = $("#chattext").val();

        // socket.send(JSON.stringify(mess));
    };

    socket.onmessage    = function (evt)
    {
        var message = JSON.parse(evt.data);

        if (message.Message == "Chat")
        {
            $("#chatbox").append(message.Chat + "\n");
            var psconsole = $('#chatbox');
            psconsole.scrollTop(psconsole[0].scrollHeight - psconsole.height());
        }
        else if (message.Message == "Names")
        {
            var list = message.names;
            var code = "";

            for (var i = 0; i < list.length; i++)
            {
                console.log(list[i]);
                code += list[i] + "\n";
            }
        }
    };

    socket.onclose      = function ()
    {
        $("#chatbox").append("Connection Lost\n");
    };

    $('#buttonSendChat').on('click', function (e)
    {
        var mes = new Object();

        mes.Lobby   = localStorage.getItem("lobby-session");
        mes.Message = "Mess";
        mes.Mess    = $("#chattext").val();

        socket.send(JSON.stringify(mes));
        $("#chattext").val("");
    });
}

function keydownHandler     (evt)
{
    if (evt.keyCode == 13)
    {
        var mes = new Object();

        mes.Lobby       = localStorage.getItem("lobby-session");
        mes.Message     = "Mess";
        mes.Mess        = $("#chattext").val();

        socket.send(JSON.stringify(mes));
        $("#chattext").val("");
    }
}
