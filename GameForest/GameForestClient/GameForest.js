/// <reference path="promise.js" />
/// <reference path="guid.js" />
/// <reference path="jquery.js" />
"use strict";

// Set this to true when debugging to let Game Forest show alert messages when something went wrong.
var GameForestVerboseMessaging = false;

// Set this to localhost when debugging and to game-forest.cloudapp.net when submitting it to Game Forest.
var GameForestCloudUrl = "game-forest.cloudapp.net";

// Game Forest client API
var GameForest = function (gameId, lobbyId, sessionId)
{
    if (!Guid.isGuid(gameId) ||
        !Guid.isGuid(lobbyId) ||
        !Guid.isGuid(sessionId))
    {
        console.error("Parameters required to start GameForest is either missing or in an invalid state.");

        if(GameForestVerboseMessaging)
        {
            alert("Parameters required to start GameForest is either missing or in an invalid state.");
        }

        return;
    }

    // important shizzocrap, DO NOT FUCKING CHANGE!! D:<

    // internal game forest messages

    var GFX_INIT_CONNECTION     = "GFX_INIT_CONNECTION";    // message to initiate connection with a game forest server
    var GFX_STOP_CONNECTION     = "GFX_STOP_CONNECTION";    // message to terminate connection with a game forest server

    // messages the client will send to the server

    var GFX_ASK_DATA            = "GFX_ASK_DATA";           // message to ask for the game's data
    var GFX_ASK_USER_DATA       = "GFX_ASK_USER_DATA";      // message to ask for the user's game data
    var GFX_FINISH              = "GFX_GAME_FINISH";        // message to inform the server the game is finished
    var GFX_TALLY               = "GFX_GAME_TALLY";         // message to inform the server the game's scores are tallied and ready to be shown to players
    var GFX_NEXT_TURN           = "GFX_NEXT_TURN";          // message to inform the server that its the next player's turn
    var GFX_SEND_DATA           = "GFX_SEND_DATA";          // message to send game data
    var GFX_SEND_USER_DATA      = "GFX_SEND_USER_DATA";     // message to send user game data
    var GFX_GAME_START          = "GFX_GAME_START";         // message to inform the server the game should start
    var GFX_GAME_START_CONFIRM  = "GFX_GAME_START_CONFIRM"; // message to inform the server the client has acknowledged the GFX_GAME_START message
    var GFX_CONFIRM_TURN        = "GFX_CONFIRM_TURN";       // message to change the player's order

    // message the client will receive from the server

    var GFX_START               = "GFX_START";              // message to inform the client the game has started
    var GFX_GAME_TALLIED        = "GFX_GAME_TALLIED";       // message to inform the client the game's scores are tallied
    var GFX_GAME_FINISHED       = "GFX_GAME_FINISHED";      // message to inform the client the game has finished
    var GFX_TURN_START          = "GFX_TURN_START";         // message to inform the client its the player's turn
    var GFX_TURN_CHANGED        = "GFX_TURN_CHANGED";       // message to inform the client the player's turn has changed
    var GFX_TURN_RESOLVE        = "GFX_TURN_RESOLVE";       // message to inform the client the player's orders are updated
    var GFX_DATA_CHANGED        = "GFX_DATA_CHANGED";       // message to inform the client the game's data is changed

    // URL of the game forest server and its port. DO NOT EVER EVER EVER CHANGE THIS IF YOU DON'T WANT YOUR GAME TO EXPLODE. >:)

    var cloudURL                = GameForestCloudUrl;
    var cloudPRT                = 1193;

    // we use this to make websocket messages synchonized. since only one websocket command can run at a time (via the promise's .then() function),
    // we don't have to make a promise for each websocket message.
    var wsPromise               = new promise.Promise();

    // end of important shizzocrap

    this.gameId                 = gameId;                                       // the unique identifier of the game
    this.lobbyId                = lobbyId;                                      // the unique identifier of the lobby the player is in
    this.sessionId              = sessionId;                                    // the unique identifier of the current session of the player

    this.connectionId           = "";                                           // the unique identifier for the websocket connection

    this.wsConnection           = new WebSocket("ws://localhost:1193");         // websocket object

    // ------------------------------------------------------------------------------------------------

    // function to start the game forest client
    this.start                  = function ()
    {
        this.wsConnection.onopen    = function ()
        {
            if (GameForestVerboseMessaging)
            {
                alert("Connection is opened with the server! Expecting the server to give connection ID...");
            }
        };
        this.wsConnection.onclose   = function ()
        {
            if (GameForestVerboseMessaging)
            {
                alert("Connection is closed with the server.");
            }
        };
        this.wsConnection.onmessage = function (message)
        {
            var parse = JSON.parse(message.data);

            switch(parse.Subject)
            {

            }
        };
    };
    // function to stop the game forest client
    this.stop                   = function ()
    {
        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, STOP_CONNECTION, "");
    };

    // ------------------------------------------------------------------------------------------------

    // function to inform the server its the next player's turn
    this.nextTurn               = function ()
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_NEXT_TURN, "");

        return wsPromise;
    };
    // function to inform the server this players turn should be changed
    this.confirmTurn            = function (changedTurn)
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_CONFIRM_TURN, changedTurn);

        return wsPromise;
    };

    // function to get the user's information
    this.getUserInfo            = function (username)
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

                if (GameForestVerboseMessaging)
                {
                    alert("Error in getUserInfo function: [status=" + status + "] [reason=" + why + "]");
                }
            });

        return p;
    };
    // function to get the lobby's user list
    this.getUserList            = function ()
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

                if (GameForestVerboseMessaging)
                {
                    alert("Error in getUserList function: [status=" + status + "] [reason=" + why + "]");
                }
            });

        return p;
    };
    // function to get the game's information
    this.getGameInfo            = function ()
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

                if (GameForestVerboseMessaging)
                {
                    alert("Error in getGameInfo function: [status=" + status + "] [reason=" + why + "]");
                }
            });

        return p;
    };

    // function to retrieve the game data
    this.askGameData            = function ()
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_ASK_DATA, "");

        return wsPromise;
    };
    // function to retrieve the user data
    this.askUserGameData        = function ()
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_ASK_USER_DATA, "");

        return wsPromise;
    };

    // function to finish the game
    this.finishGame             = function ()
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_FINISH, "");

        return wsPromise;
    };
    // function to send game data
    this.sendGameData           = function (payload)
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_SEND_DATA, payload);

        return wsPromise;
    };
    // function to send user data
    this.sendUserGameData       = function (payload)
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_SEND_USER_DATA, payload);

        return wsPromise;
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

// method to override when the game has started
GameForest.prototype.onGameStart    = function ()
{

};

// method to override when its the player's turn
GameForest.prototype.onTurnStart    = function ()
{

};

// method to override when its the player's time to change his/her order
GameForest.prototype.onTurnSelect   = function (originalTurn)
{

};

// method to override when the game's data has changed
GameForest.prototype.onUpdateData   = function (data)
{

};

// method to override when the game is finished
GameForest.prototype.onGameFinish   = function ()
{

};

// method to override when the game's scores are tallied
GameForest.prototype.onGameTally    = function (tallyList)
{

};
