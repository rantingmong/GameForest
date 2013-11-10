/// <reference path="index.html" />
/// <reference path="guid.js" />
/// <reference path="jquery.js" />
/// <reference path="promise.js" />
/// <reference path="../../js/gameforestip.js" />

'use strict';

// Set this to true when debugging to let Game Forest show alert messages when something went wrong.
var GameForestVerboseMessaging              = false;

// Set this to localhost when debugging and to game-forest.cloudapp.net when submitting it to Game Forest.
var GameForestCloudUrl                      = gameForestIP;

var GFX_STATUS_LOBBY                        = 0,
    GFX_STATUS_CHOOSE                       = 1,
    GFX_STATUS_PLAYING                      = 2,
    GFX_STATUS_FINISHED                     = 4;

// Game Forest client API
var GameForest                              = function (gameId, lobbyId, sessionId)
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
    
    // critical variables

    this.lobbyStatus            = 0;

    this.gameId                 = gameId;                   // the unique identifier of the game
    this.lobbyId                = lobbyId;                  // the unique identifier of the lobby the player is in
    this.sessionId              = sessionId;                // the unique identifier of the current session of the player

    this.connectionId           = "";                       // the unique identifier for the websocket connection

    this.wsConnection           = new WebSocket("ws://" + GameForestCloudUrl +":8084");     // websocket object

    // ------------------------------------------------------------------------------------------------

    // helper methods

    function sendRequest        (url, type, onSuccess, onError)
    {
        var httpURL = "http://" + cloudURL + ":" + cloudPRT + "/service" + url;

        console.log("Doing " + type + " request on URL " + httpURL);

        var response = $.ajax({
            url:    httpURL,
            async:  true,
            type:   type,
            cache:  false
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

    // URL of the game forest server and its port. DO NOT EVER EVER EVER CHANGE THIS IF YOU DON'T WANT YOUR GAME TO EXPLODE. >:)

    var cloudURL                = GameForestCloudUrl,
        cloudPRT                = 1193,
        wsPromise               = new promise.Promise(),        // promise instance for websocket messages
        wsRCPromise             = new promise.Promise();

    // internal game forest messages

    var GFX_INIT_CONNECTION     = "GFX_INIT_CONNECTION",        // message to initiate connection with a game forest server
        GFX_STOP_CONNECTION     = "GFX_STOP_CONNECTION",        // message to terminate connection with a game forest server

    // messages the client will send to the server

        GFX_ASK_DATA            = "GFX_ASK_DATA",               // message to ask for the game's data
        GFX_ASK_USER_DATA       = "GFX_ASK_USER_DATA",          // message to ask for the user's game data

        GFX_SEND_DATA           = "GFX_SEND_DATA",              // message to send game data
        GFX_SEND_USER_DATA      = "GFX_SEND_USER_DATA",         // message to send user game data

        GFX_FINISH              = "GFX_GAME_FINISH",            // message to inform the server the game is finished

        GFX_GAME_START          = "GFX_GAME_START",             // message to inform the server the game should start
        GFX_GAME_START_CONFIRM  = "GFX_GAME_START_CONFIRM",     // message to inform the server the client has acknowledged the GFX_GAME_START message

        GFX_NEXT_TURN           = "GFX_NEXT_TURN",              // message to inform the server that its the next player's turn
        GFX_CONFIRM_TURN        = "GFX_CONFIRM_TURN",           // message to change the player's order

    // messages the client will receive from the server

        GFX_START_GAME          = "GFX_START_GAME",             // message to inform the client the game has started
        GFX_START_CHOICE        = "GFX_START_CHOICE",           // message to inform the client the game should display the order choose screen
        GFX_GAME_FINISHED       = "GFX_GAME_FINISHED",          // message to inform the client the game has finished
        GFX_TURN_START          = "GFX_TURN_START",             // message to inform the client its the player's turn
        GFX_TURN_CHANGED        = "GFX_TURN_CHANGED",           // message to inform the client the player's turn has changed
        GFX_TURN_RESOLVE        = "GFX_TURN_RESOLVE",           // message to inform the client the player's orders are updated
        GFX_DATA_CHANGED        = "GFX_DATA_CHANGED",           // message to inform the client the game's data is changed
        GFX_USER_DATA_CHANGED   = "GFX_USER_DATA_CHANGED",      // message to inform the client someone changed their user's data

        GFX_CONNECTION_PING     = "GFX_CONNECTION_PING",        // message the server will send to the client to ping back
        GFX_PING_RESPOND        = "GFX_PING_RESPOND",           // message to inform the server the client received the ping message

    // messages for client disconnect and reconnect
        
        GFX_PLAYER_DISCONNECTED = "GFX_PLAYER_DISCONNECTED",    // message to inform the client someone was disconnected by the server
        GFX_PLAYER_RECONNECTED  = "GFX_PLAYER_RECONNECTED",     // message to inform the client someone was reconnected by the server

        GFX_INFORM_RECONNECT    = "GFX_INFORM_RECONNECT";       // message sent by the client to let the server know the client is reconnected

    // ------------------------------------------------------------------------------------------------

    // accessor variables

    var _sCurrentPlayer         = null;
    var _sPlayerOrderIndex      = null;
    var _sPlayerInfo            = null;
    var _sUserList              = null;

    this.currentPlayer          = function ()
    {

        return _sCurrentPlayer;
    };

    this.playerOrderIndex       = function () {

        return _sPlayerOrderIndex;
    };

    this.playerInfo             = function () {

        return _sPlayerInfo;
    };

    this.userList               = function () {

        return _sUserList;
    };

    this.nextPlayer             = function (steps)
    {
        if (_sUserList == null || _sPlayerOrderIndex == null)
        {
            return null;
        }

        var preStep = parseInt(_sPlayerOrderIndex) + steps - 1;
        var modStep = preStep % _sUserList.length;
        var posStep = modStep;

        return _sUserList[posStep];
    };

    // ------------------------------------------------------------------------------------------------

    // function to start the game forest client
    this.start                  = function ()
    {
        var pGfxObject = this;

        setInterval(function ()
        {
            pGfxObject.heartbeat();

        }, 15000);

        this.wsConnection.onopen    = function ()
        {
            pGfxObject.wsOnOpen     (pGfxObject);
        };
        this.wsConnection.onclose   = function ()
        {
            pGfxObject.wsOnClose    (pGfxObject);
        };
        this.wsConnection.onmessage = function (message)
        {
            pGfxObject.wsOnMessage  (pGfxObject, message);
        };
    };
    // function to stop the game forest client
    this.stop                   = function ()
    {
        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_STOP_CONNECTION, "");
    };

    // function to navigate back to the games page.
    this.navigateToGame         = function ()
    {
        window.location.href = "../../../games.html";
    };

    // lol method
    this.thenStarter            = function ()
    {
        var p = new promise.Promise();

        p.done(null, "");

        return p;
    };

    // websocket messages -----------------------------------------------------------------------------

    this.wsOnOpen               = function (gfxObject)
    {
        if (GameForestVerboseMessaging)
        {
            alert("Connection is opened with the server! Expecting the server to give connection ID...");
        }
    };

    this.wsOnClose              = function (gfxObject)
    {
        if (GameForestVerboseMessaging)
        {
            alert("Connection is closed with the server.");
        }

        // if the user is playing and this suddenly went up. we know the user is disconnected from the internet.
        if (gfxObject.lobbyStatus == GFX_STATUS_PLAYING)
        {
            // call
            GameForest.prototype.onGameDisconnected();
        }
    };

    this.wsOnMessage            = function (gfxObject, message)
    {
        var parse = JSON.parse(message.data);

        console.log(message.data);

        if (message.data == GFX_CONNECTION_PING)
        {
            console.log("Sending ping reply!");

            constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_PING_RESPOND);
            return;
        }

        switch (parse.Subject)
        {
            case GFX_PLAYER_DISCONNECTED:

                GameForest.prototype.onPlayerDisconnected(JSON.parse(parse.Message));
                break;
            case GFX_PLAYER_RECONNECTED:

                GameForest.prototype.onPlayerReconnected();
                break;
            case GFX_INFORM_RECONNECT:

                if (parse.ResponseCode === 0)
                {
                    wsRCPromise.done(null, "Reconnected!");
                } else
                {
                    wsRCPromise.done(parse.ResponseCode + ": " + parse.Message, null);
                }
                break;
            case GFX_ASK_DATA:
            case GFX_ASK_USER_DATA:
            case GFX_SEND_DATA:
            case GFX_SEND_USER_DATA:
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
            case GFX_FINISH:

                this.lobbyStatus = GFX_STATUS_FINISHED;

                console.log("Game is finished!");

                if (parse.ResponseCode === 0)
                {
                    wsPromise.done(null, parse.Message);
                } else
                {
                    wsPromise.done(parse.ResponseCode + ":" + parse.Message, null);
                }

                break;
            case GFX_INIT_CONNECTION:

                console.log("Server acknowledges connection, yay!!");

                gfxObject.connectionId = parse.Message;
                constructWSRequest(gfxObject.wsConnection, gfxObject.connectionId, gfxObject.sessionId, GFX_INIT_CONNECTION, "");
                break;
            case GFX_STOP_CONNECTION:

                gfxObject.wsConnection.close();

                break;
            case GFX_GAME_START:

                console.log("Server sent a start game signal.");

                GameForest.prototype.onGameStartSignal();
                break;
            case GFX_START_GAME:

                console.log("Server sent a start game event! Time to really start the game.");

                $("#gameForestLobbyWindow").hide();
                $("#gameForestGameWindow").show();

                gfxObject.lobbyStatus = GFX_STATUS_PLAYING;

                gfxObject.getUserList()
                    .then(function (error, result) {

                        _sUserList = result;

                        return gfxObject.getUserInfo();
                    })
                    .then(function (error, result) {

                        _sPlayerInfo = result;

                        GameForest.prototype.onGameStart();
                    });

                break;
            case GFX_START_CHOICE:

                console.log("Server is asking the players to choose! Show the choose screen.");

                $("#gameForestLobbyWindow").hide();
                $("#gameForestGameWindow").show();

                gfxObject.lobbyStatus = GFX_STATUS_CHOOSE;

                GameForest.prototype.onGameChoose();
                break;
            case GFX_GAME_FINISHED:

                console.log("Server is informing players that the game is finished!");

                GameForest.prototype.onGameFinish(parse.Message);
                break;
            case GFX_TURN_START:

                console.log("Server is saying my turn has started.");

                gfxObject.getCurrentPlayer().then(function (error, result)
                {
                    _sCurrentPlayer = result;
                    GameForest.prototype.onTurnStart();
                });
                break;
            case GFX_TURN_CHANGED:

                console.log("Server is informing other players the game's turn have changed.");

                gfxObject.getCurrentPlayer().then(function (error, result)
                {
                    _sCurrentPlayer = result;
                    GameForest.prototype.onTurnChange();
                });
                break;
            case GFX_TURN_RESOLVE:

                console.log("Server is asking the player to choose his/her order.");

                GameForest.prototype.onTurnSelect(parse.Message);
                break;
            case GFX_DATA_CHANGED:

                console.log("Server is informing players the game's data has changed.");

                var packagedData = JSON.parse(parse.Message);
                GameForest.prototype.onUpdateData(packagedData.Key, JSON.parse(packagedData.Data));
                break;
            case GFX_USER_DATA_CHANGED:

                console.log("Server is informing players someone's data has changed.");

                var packagedData = JSON.parse(parse.Message);
                GameForest.prototype.onUpdateUserData(JSON.parse(packagedData.User), JSON.parse(packagedData.Data));
                break;
        }
    };

    // ------------------------------------------------------------------------------------------------

    // function to inform the server its the next player's turn
    this.nextTurn               = function (gameData)
    {
        if (typeof gameData != undefined || gameData != null)
        {
            var closureObject = this;

            this.sendGameData("default", gameData).then(function (error, result)
            {
                constructWSRequest(
                    closureObject.wsConnection,
                    closureObject.connectionId,
                    closureObject.sessionId,
                    GFX_NEXT_TURN,
                    "");
            });
        }
        else
        {
            wsPromise = new promise.Promise();

            constructWSRequest(
                this.wsConnection,
                this.connectionId,
                this.sessionId,
                GFX_NEXT_TURN,
                "");

            return wsPromise;
        }
    };

    // function to inform the server the player wants its turn to change order
    this.confirmTurn            = function (changedTurn, gameData)
    {
        if (typeof gameData != undefined && gameData != null)
        {
            var closure = this;

            this.sendGameData("choose", gameData).then(function (error, result)
            {
                _sPlayerOrderIndex = changedTurn;

                constructWSRequest(closure.wsConnection, closure.connectionId, closure.sessionId, GFX_CONFIRM_TURN, changedTurn);
            });
        }
        else
        {
            wsPromise           = new promise.Promise();
            _sPlayerOrderIndex  = changedTurn;

            constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_CONFIRM_TURN, changedTurn);

            return wsPromise;
        }
    };
    
    // function to get the lobby's current state (lobby, choose, playing)
    this.getLobbyState          = function ()
    {
        return this.lobbyStatus;
    };

    // function to get the current active player
    this.getCurrentPlayer       = function ()
    {
        var p = new promise.Promise();

        sendRequest("/lobby/currentplayer?lobbyid=" + this.lobbyId + "&usersessionid=" + this.sessionId, "GET",
            function (result)
            {
                console.log("Result is: " + result.ResponseType + " with payload: " + result.AdditionalData);

                if (result.ResponseType == 0)
                    p.done(null, JSON.parse(result.AdditionalData));
                else
                    p.done(result.ResponseType + " " + result.AdditionalData);
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

        sendRequest("/lobby/nextplayer?lobbyid=" + this.lobbyId + "&usersessionid=" + this.sessionId + "&steps=" + steps, "GET",
            function (result)
            {
                if (result.ResponseType == 0)
                    p.done(null, JSON.parse(result.AdditionalData));
                else
                    p.done(result.ResponseType + " " + result.AdditionalData);
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

        sendRequest("/lobby/turn?lobbyid=" + this.lobbyId + "&usersessionid=" + this.sessionId, "GET",
                    function (result)
                    {
                        if (result.ResponseType == 0)
                            p.done(null, parseInt(result.AdditionalData));
                        else
                            p.done(result.ResponseType + " " + result.AdditionalData);
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
            sendRequest("/user/session/" + this.sessionId, "GET",
                        function (result)
                        {
                            if (result.ResponseType == 0)
                                p.done(null, JSON.parse(result.AdditionalData));
                            else
                                p.done(result.ResponseType + " " + result.AdditionalData);
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
            sendRequest("/user/" + username, "GET",
                        function (result)
                        {
                            if (result.ResponseType == 0)
                                p.done(null, JSON.parse(result.AdditionalData));
                            else
                                p.done(result.ResponseType + " " + result.AdditionalData);
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

        sendRequest("/lobby/users?lobbyid=" + this.lobbyId + "&usersessionid=" + this.sessionId, "GET",
            function (result)
            {
                if (result.ResponseType == 0)
                    p.done(null, JSON.parse(result.AdditionalData));
                else
                    p.done(result.ResponseType + " " + result.AdditionalData);
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

        sendRequest("/game/" + this.gameId, "GET",
            function (result)
            {
                if (result.ResponseType == 0)
                    p.done(null, JSON.parse(result.AdditionalData));
                else
                    p.done(result.ResponseType + " " + result.AdditionalData);
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
    this.finishGame             = function (tallyResults)
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_FINISH, tallyResults);

        return wsPromise;
    };
    // function to send game data
    this.sendGameData           = function (key, payload)
    {
        wsPromise = new promise.Promise();

        var packagedData =
            {
                Key:    key,
                Data:   JSON.stringify(payload)
            };

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_SEND_DATA, JSON.stringify(packagedData));

        return wsPromise;
    };
    // function to send user data
    this.sendUserGameData       = function (payload)
    {
        wsPromise = new promise.Promise();

        constructWSRequest(this.wsConnection, this.connectionId, this.sessionId, GFX_SEND_USER_DATA, JSON.string(payload));

        return wsPromise;
    };

    // function to inform the server the user is active
    this.heartbeat              = function ()
    {
        var thus = this;

        sendRequest("/user/login?usersessionid=" + this.sessionId, "PUT",
            function(result) {

            },
            function (status, why)
            {
                // if the user is playing and this suddenly went up. we know the user is disconnected from the internet.
                if (thus.lobbyStatus == GFX_STATUS_PLAYING)
                {
                    // call
                    GameForest.prototype.onGameDisconnected();
                }
            });
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

    // function to inform the server the client is now back online and ready to reconnect
    this.askForReconnection     = function ()
    {
        wsRCPromise = new promise.Promise();

        var pGfxObject = this;

        // if request is successful, we remake the websocket and ask the server for an updated game data via a message
        pGfxObject.wsConnection = new WebSocket("ws://" + GameForestCloudUrl + ":8084");

        pGfxObject.wsConnection.onopen      = pGfxObject.wsOnOpen;
        pGfxObject.wsConnection.onclose     = pGfxObject.wsOnClose;
        pGfxObject.wsConnection.onmessage   = pGfxObject.wsOnMessage;

        constructWSRequest(pGfxObject.wsConnection, pGfxObject.connectionId, GFX_INFORM_RECONNECT, "");

        return wsRCPromise;
    };
};

// override these methods in your game

// REQUIRED METHODS --------------------------------------------------------------------

// method to override when the game has started
GameForest.prototype.onGameStart            = function ()
{

};

// method to override when its the player's turn
GameForest.prototype.onTurnStart            = function ()
{

};

// method to override when its the other player's turn
GameForest.prototype.onTurnChange           = function ()
{

};

// method to override when the game's data has changed
GameForest.prototype.onUpdateData           = function (key, data)
{

};

// method to override when someone's user data has changed
GameForest.prototype.onUpdateUserData       = function (user, data)
{

};

// method to override when the game is finished
GameForest.prototype.onGameFinish           = function (tallyList)
{

};

// OPTIONAL ADVANCED METHODS -----------------------------------------------------------

// method called when the player clicked the start button
GameForest.prototype.onGameStartSignal      = function ()
{
    console.log("Game start signal invoked!");

    $("#gfxButtonStart").removeAttr("disabled");
};

// method to override when the game should display the choose screen
GameForest.prototype.onGameChoose           = function ()
{

};

// method to override when its the player's time to change his/her order
GameForest.prototype.onTurnSelect           = function (originalTurn)
{
    gf.confirmTurn(originalTurn);
};

// method to override when a player (not this player) is disconnected
GameForest.prototype.onPlayerDisconnected   = function (who)
{
    $("#gameForestClientDisconnected").show();
    $("#gameForestPlayerDisconnected").hide();

    $("#gameForestDisconnectedPlayer").html(who.Username + " was disconnected from the game.");

    $("#gameForestDisconnectWindow").fadeIn("fast");
};

// method to override when a player is reconnected
GameForest.prototype.onPlayerReconnected    = function ()
{
    $("#gameForestDisconnectWindow").fadeOut("fast");
};

// method to override when this player is disconnected
GameForest.prototype.onGameDisconnected     = function ()
{
    $("#gameForestClientDisconnected").hide();
    $("#gameForestPlayerDisconnected").show();

    $("#gameForestDisconnectWindow").fadeIn("fast");

    var connected       = false;
    var countdownTimer  = 30;
    var interval        = setInterval(function()
    {
        $("#gameForestCountdownKill").html(countdownTimer);

        countdownTimer -= 1;

        gf.askForReconnection().then(function (error, result)
        {
            if (error != null)
            {
                // show the error prompt
                $("#gameForestPlayerKilled").show();
            }
            else
            {
                connected = true;
                // we are connected!
            }
        });

        if (countdownTimer <= 0 && connected == false)
        {
            // we give up...
            clearInterval(interval);

            // inform the user he is disconnected and cannot be brought back to the game
        }

    }, 1000);
};
