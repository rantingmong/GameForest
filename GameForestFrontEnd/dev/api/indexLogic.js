/// <reference path="jquery.js" />
/// <reference path="promise.js" />
/// <reference path="guid.js" />
/// <reference path="GameForest.js" />
/// <reference path="GameForestTracking.js" />

var userUpdateInterval = null;

var gf              = new GameForest(new Guid(localStorage.getItem("lobby-game")), new Guid(localStorage.getItem("lobby-session")), new Guid(localStorage.getItem("user-session")));
var chatWebSocket   = new WebSocket('ws://' + GameForestCloudUrl + ':8085/');

$("#gfxButtonLeave").click(function ()
{
    window.location.href = "http://game-forest.cloudapp.net:46069/games.html";
});

$('#buttonSendChat').on('click', function (e)
{
    var mes = new Object();

    mes.Lobby   = localStorage.getItem("lobby-session");
    mes.Message = "chat";
    mes.Value   = $("#chattext").val();

    chatWebSocket.send(JSON.stringify(mes));
    $("#chattext").val("");

    console.log("Sending chat message!");
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

    chatWebSocket.onopen = function ()
    {
        console.log("Connecting to chat...");

        // inform the server for chat connection
        var mes = new Object();

        mes.Lobby   = localStorage.getItem("lobby-session");
        mes.Message = "open";
        mes.Value   = Guid.create();    // we use this to differentiate users in the server.

        chatWebSocket.send(JSON.stringify(mes));
    };

    chatWebSocket.onmessage = function (evt)
    {
        var message = JSON.parse(evt.data);

        if      (message.Message == "open")
        {
            // server replied!
            $("#chatbox").append("Connected\n");
        }
        else if (message.Message == "chat")
        {
            // add chat entry to the list
            $("#chatbox").append(message.Value + "\n");

            var psconsole = $('#chatbox');
                psconsole.scrollTop(psconsole[0].scrollHeight - psconsole.height());
        }
        else
        {
            console.warn("Invalid message!");
        }
    };

    chatWebSocket.onclose      = function ()
    {
        console.error("Connection closed.");
    };
}

function keydownHandler     (evt)
{
    if (evt.keyCode == 13)
    {
        var mes = new Object();

        mes.Lobby   = localStorage.getItem("lobby-session");
        mes.Message = "chat";
        mes.Value   = $("#chattext").val();

        chatWebSocket.send(JSON.stringify(mes));
        $("#chattext").val("");
    }
}
