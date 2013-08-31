﻿/// <reference path="guid.js" />
/// <reference path="jquery.js" />
/// <reference path="promise.js" />
/// <reference path="index.html" />

'use strict';

// Set this to true when debugging to let Game Forest show alert messages when something went wrong.
var GameForestVerboseMessaging  = false;

// Set this to localhost when debugging and to game-forest.cloudapp.net when submitting it to Game Forest.
var GameForestCloudUrl          = "game-forest.cloudapp.net";

// Game Forest client API
var GameForest                  = function (gameId, lobbyId, sessionId)
{
    // checker to see if gameforest can initialize
    if (!Guid.isGuid(gameId)    ||
        !Guid.isGuid(lobbyId)   ||
        !Guid.isGuid(sessionId))
    {
        console.error("Parameters required to start GameForest is either missing or in an invalid state.");

        if (GameForestVerboseMessaging)
        {
            alert("Parameters required to start GameForest is either missing or in an invalid state.");
        }

        return;
    }

    // ------------------------------------------------------------------------------------------------
    
    // URL of the game forest server and its port. DO NOT EVER EVER EVER CHANGE THIS IF YOU DON'T WANT YOUR GAME TO EXPLODE. >:)

    var cloudURL                = GameForestCloudUrl,
        cloudPRT                = 1193,
        wsPromise               = new promise.Promise();    // promise instance for websocket messages

    this.gameId                 = gameId;                   // the unique identifier of the game
    this.lobbyId                = lobbyId;                  // the unique identifier of the lobby the player is in
    this.sessionId              = sessionId;                // the unique identifier of the current session of the player

    this.connectionId           = "";                       // the unique identifier for the websocket connection

    this.wsConnection           = new WebSocket("ws://localhost:8084");     // websocket object

    // ------------------------------------------------------------------------------------------------

    // helper methods

    function sendRequest        (url, onSuccess, onError)
    {
        var response = $.ajax({
            url:    "http://" + cloudURL + ":" + cloudPRT + "/service" + url,
            async:  true
        });

        response.success(onSuccess);
        response.fail(function (a, b, c)
        {

            onError(b, c);
        });
    }

    function constructWSRequest (ws, cid, sid, subject, payload)
    {
        if (payload.length > 0)
            console.log("Sending message with subject " + subject + " and payload " + payload);
        else
            console.log("Sending message with subject " + subject);

        ws.send(JSON.stringify(
            {
                "ConnectionId": cid,
                "SessionId":    sid,
                "Subject":      subject,
                "Message":      payload
            }));
    }

    // ------------------------------------------------------------------------------------------------

    // internal game forest messages

    var GFX_INIT_CONNECTION     = "GFX_INIT_CONNECTION",    // message to initiate connection with a game forest server
        GFX_STOP_CONNECTION     = "GFX_STOP_CONNECTION",    // message to terminate connection with a game forest server

    // messages the client will send to the server

        GFX_ASK_DATA            = "GFX_ASK_DATA",           // message to ask for the game's data
        GFX_ASK_USER_DATA       = "GFX_ASK_USER_DATA",      // message to ask for the user's game data

        GFX_SEND_DATA           = "GFX_SEND_DATA",          // message to send game data
        GFX_SEND_USER_DATA      = "GFX_SEND_USER_DATA",     // message to send user game data

        GFX_FINISH              = "GFX_GAME_FINISH",        // message to inform the server the game is finished
        GFX_TALLY               = "GFX_GAME_TALLY",         // message to inform the server the game's scores are tallied and ready to be shown to players

        GFX_GAME_START          = "GFX_GAME_START",         // message to inform the server the game should start
        GFX_GAME_START_CONFIRM  = "GFX_GAME_START_CONFIRM", // message to inform the server the client has acknowledged the GFX_GAME_START message

        GFX_NEXT_TURN           = "GFX_NEXT_TURN",          // message to inform the server that its the next player's turn
        GFX_CONFIRM_TURN        = "GFX_CONFIRM_TURN",       // message to change the player's order

    // messages the client will receive from the server

        GFX_START_GAME          = "GFX_START_GAME",         // message to inform the client the game has started
        GFX_START_CHOICE        = "GFX_START_CHOICE",       // message to inform the client the game should display the order choose screen
        GFX_GAME_TALLIED        = "GFX_GAME_TALLIED",       // message to inform the client the game's scores are tallied
        GFX_GAME_FINISHED       = "GFX_GAME_FINISHED",      // message to inform the client the game has finished
        GFX_TURN_START          = "GFX_TURN_START",         // message to inform the client its the player's turn
        GFX_TURN_CHANGED        = "GFX_TURN_CHANGED",       // message to inform the client the player's turn has changed
        GFX_TURN_RESOLVE        = "GFX_TURN_RESOLVE",       // message to inform the client the player's orders are updated
        GFX_DATA_CHANGED        = "GFX_DATA_CHANGED";       // message to inform the client the game's data is changed

    // ------------------------------------------------------------------------------------------------

    // function to start the game forest client
    this.start                  = function ()
    {
        var pGfxObject = this;

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

            switch (parse.Subject)
            {
                case GFX_ASK_DATA:
                case GFX_ASK_USER_DATA:
                case GFX_SEND_DATA:
                case GFX_SEND_USER_DATA:
                case GFX_FINISH:
                case GFX_TALLY:
                case GFX_NEXT_TURN:
                case GFX_CONFIRM_TURN:
                case GFX_GAME_START_CONFIRM:

                    console.log("Reply recevied! " + message.data);

                    if (parse.ResponseCode === 0)
                    {
                        wsPromise.done(null, parse.Message);
                    } else
                    {
                        wsPromise.done(parse.ResponseCode + ":" + parse.Message, null);
                    }
                    break;
                case GFX_INIT_CONNECTION:

                    console.log("Server acknowledges connection! Yay!!");

                    pGfxObject.connectionId = parse.Message;
                    constructWSRequest(pGfxObject.wsConnection, pGfxObject.connectionId, pGfxObject.sessionId, GFX_INIT_CONNECTION, "");
                    break;
                case GFX_GAME_START:

                    console.log("Server sent a start game signal.");

                    GameForest.prototype.onGameStartSignal();
                    break;
                case GFX_START_GAME:

                    console.log("Server sent a start game event! Time to really start the game.");

                    clearInterval(updateUserList);

                    $("#gameForestLobbyWindow").hide();
                    $("#gameForestGameWindow").show();

                    GameForest.prototype.onGameStart();
                    break;
                case GFX_START_CHOICE:

                    console.log("Server is asking the players to choose! Show the choose screen.");

                    $("#gameForestLobbyWindow").hide();
                    $("#gameForestGameWindow").show();

                    GameForest.prototype.onGameChoose();
                    break;
                case GFX_GAME_TALLIED:

                    console.log("Server is asking the game to tally the player's scores. Show the tally screen.");

                    GameForest.prototype.onGameTally(parse.Message);
                    break;
                case GFX_GAME_FINISHED:

                    console.log("Server is informing players that the game is finished!");

                    GameForest.prototype.onGameFinish();
                    break;
                case GFX_TURN_START:

                    console.log("Server is saying my turn has started.");

                    GameForest.prototype.onTurnStart();
                    break;
                case GFX_TURN_CHANGED:

                    console.log("Server is informing other players the game's turn have changed.");

                    GameForest.prototype.onTurnChange();
                    break;
                case GFX_TURN_RESOLVE:

                    console.log("Server is asking the player to choose his/her order.");

                    GameForest.prototype.onTurnSelect(parse.Message);
                    break;
                case GFX_DATA_CHANGED:

                    console.log("Server is informing players the game's data has changed.");

                    GameForest.prototype.onUpdateData(JSON.parse(parse.Message));
                    break;
            }
        };
    };
    // function to stop the game forest client
    this.stop                   = function ()
    {
        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_STOP_CONNECTION, "");
    };

    // ------------------------------------------------------------------------------------------------

    // function to inform the server its the next player's turn
    this.nextTurn               = function ()
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_NEXT_TURN, "");

        return wsPromise;
    };
    // function to inform the server the player wants its turn to change order
    this.confirmTurn            = function (changedTurn)
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_CONFIRM_TURN, changedTurn);

        return wsPromise;
    };
    // function to inform the server the game's scores are tallied
    this.sendTallyResult        = function (tallyResult)
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_TALLY, tallyResult);

        return wsPromise;
    };

    // function to get the current active player
    this.getCurrentPlayer       = function ()
    {
        var p = new promise.Promise();

        sendRequest("/user/currentplayer?lobbyid=" + this.lobbyId + "&usersessionid=" + this.sessionId,
            function (result)
            {
                p.done(null, result);
            },
            function (status, why)
            {
                p.done(status + ": " + why, null);

                if (GameForestVerboseMessaging)
                {
                    alert("Error in getCurrentPlayer function: [status=" + status + "] [reason=" + why + "]");
                }
            });

        return p;
    };
    // function to get the next player "steps" from the player calling this function
    this.getNextPlayer          = function (steps)
    {
        var p = new promise.Promise();

        sendRequest("/user/nextplayer?lobbyid=" + this.lobbyId + "&usersessionid=" + this.sessionId + "&steps=" + this.steps,
            function (result)
            {
                p.done(null, result);
            },
            function (status, why)
            {
                p.done(status + ": " + why, null);

                if (GameForestVerboseMessaging)
                {
                    alert("Error in getCurrentPlayer function: [status=" + status + "] [reason=" + why + "]");
                }
            });

        return p;
    };

    // function to get the user's current order
    this.getUserOrder           = function ()
    {
        var p = new promise.Promise();

        sendRequest("/lobby/turn?lobbyid" + this.lobbyId + "?usersessionid=" + this.sessionId,
                    function (result)
                    {
                        p.done(null, result);
                    },
                    function (status, why)
                    {
                        p.done(status + ": " + why, null);

                        if (GameForestVerboseMessaging)
                        {
                            alert("Error in getUserOrder function: [status=" + status + "] [reason=" + why + "]");
                        }
                    });

        return p;
    };
    // function to get the user's information
    this.getUserInfo            = function (username)
    {
        var p = new promise.Promise();

        if (username == undefined || username.length == 0 || username == " ")
        {
            sendRequest("/user/session/" + this.sessionId,
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
        }
        else
        {
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

        }
        
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

    this.startGame              = function ()
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_GAME_START, "");

        return wsPromise;
    };
    this.readyGame              = function ()
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_GAME_START_CONFIRM, "");

        return wsPromise;
    };
};

GameForest.prototype.onGameStartSignal  = function ()
{
    $("#gfxButtonStart").removeAttr("disabled");
};

// override these methods in your game

// method to override when the game has started
GameForest.prototype.onGameStart        = function ()
{

};

// method to override when the game should display the choose screen
GameForest.prototype.onGameChoose       = function ()
{

};

// method to override when its the player's turn
GameForest.prototype.onTurnStart        = function ()
{

};

// method to override when its the other player's turn
GameForest.prototype.onTurnChange       = function ()
{

};

// method to override when its the player's time to change his/her order
GameForest.prototype.onTurnSelect       = function (originalTurn)
{

};

// method to override when the game's data has changed
GameForest.prototype.onUpdateData       = function (data)
{

};

// method to override when the game is finished
GameForest.prototype.onGameFinish       = function ()
{

};

// method to override when the game's scores are tallied
GameForest.prototype.onGameTally        = function (tallyList)
{

};
