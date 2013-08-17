/// <reference path="promise.js" />
/// <reference path="jquery.js" />
"use strict";

var INIT_CONNECTION = "GFX_INIT_CONNECTION";
var STOP_CONNECTION = "GFX_STOP_CONNECTION";

var cloudURL        = "game-forest.cloudapp.net";
var cloudPRT        = 1193;

var GameForest      = function (gameId, lobbyId, sessionId)
{
    this.gameId             = gameId;
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

    // ------------------------------------------------------------------------------------------------

    this.confirmTurn        = function (originalTurn, changedTurn)
    {

    };

    this.getUserInfo        = function (username)
    {
        var p = new promise.Promise();

        sendRequest("/user/" + username,
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

        sendRequest("/lobby/users?lobbyid=" + this.lobbyId + "&usersessionid=" + this.sessionId,
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
        var p = new promise.Promise();

        sendRequest("/game/" + this.gameId,
            function (result)
            {
                p.done(null, result);
            },
            function (status, why)
            {
                p.done(status + ": " + why, null);
            });
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
            url:        "http://" + cloudURL + ":" + cloudPRT + url,
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

GameForest.prototype.onGameStart    = function ()
{

};
GameForest.prototype.onTurnStart    = function ()
{

};
GameForest.prototype.onTurnSelect   = function (originalTurn)
{

};
GameForest.prototype.onUpdateData   = function (data)
{

};
GameForest.prototype.onGameFinish   = function (tallyList)
{

};
GameForest.prototype.onGameTally    = function (tallyList)
{

};
