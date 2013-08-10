/// <reference path="promise.js" />
/// <reference path="jquery.js" />
"use strict";

var INIT_CONNECTION = "GFX_INIT_CONNECTION";
var STOP_CONNECTION = "GFX_STOP_CONNECTION";

var GameForest = function (lobbyId, sessionId)
{
    this.lobbyId            = lobbyId;
    this.sessionId          = sessionId;

    this.connectionId       = "";

    this.wsConnection       = new WebSocket("ws://localhost:1193");

    // ------------------------------------------------------------------------------------------------
    
    this.start              = function ()
    {
        this.wsConnection.onopen    = function ()
        {

        };
        this.wsConnection.onclose   = function ()
        {

        };
        this.wsConnection.onmessage = function (message)
        {
            var parse = JSON.parse(message.data);

            switch(parse.Subject)
            {
                case INIT_CONNECTION:
                    break;
            }
        };
    };
    this.stop               = function ()
    {
        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, STOP_CONNECTION, "");
    };

    this.onGameStart        = function ()
    {

    };
    this.onTurnStart        = function ()
    {

    };
    this.onUpdateData       = function (data)
    {

    };
    this.onGameFinish       = function (tallyList)
    {

    };
    this.onGameTally        = function (tallyList)
    {

    };

    // ------------------------------------------------------------------------------------------------

    this.getUserInfo        = function (username)
    {
        var p = new promise.Promise();

        sendRequest("http://localhost:1193/user/" + username,
            function (result)
            {
                p.done(null, result);
            },
            function (status, why)
            {
                p.done(status + ": " + why, null);
            });

        return p;
    };
    this.getUserList        = function ()
    {
        var p = new promise.Promise();

        sendRequest("http://localhost:1193/lobby/users?lobbyid=" + this.lobbyId + "&usersessionid=" + this.sessionId,
            function (result)
            {
                p.done(null, result);
            },
            function (status, why)
            {
                p.done(status + ": " + why, null);
            });

        return p;
    };
    this.getGameInfo        = function ()
    {

    };

    this.askGameData        = function ()
    {

    };
    this.askUserGameData    = function ()
    {

    };

    this.finishGame         = function ()
    {

    };
    this.sendGameData       = function (payload)
    {

    };
    this.sendUserGameData   = function (payload)
    {

    };

    // ------------------------------------------------------------------------------------------------

    function sendRequest        (url, onSuccess, onError)
    {
        $.ajax({
            url:        url,
            async:      false,
            success:    onSuccess,
            error:      function (a, b, c) { onError(b, c);}
        });
    }

    function constructWSRequest (ws, cid, sid, subject, payload)
    {
        ws.send(JSON.stringify(
            {
                "ConnectionId": cid,
                "SessionId":    sid,
                "Subject":      subject,
                "Message":      payload
            }));
    }
};
