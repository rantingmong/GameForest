/// <reference path="guid.js" />
/// <reference path="jquery.js" />
/// <reference path="promise.js" />
/// <reference path="index.html" />

'use strict';

// Set this to true when debugging to let Game Forest show alert messages when something went wrong.
var GameForestVerboseMessaging  = false;

// Set this to localhost when debugging and to game-forest.cloudapp.net when submitting it to Game Forest.
var GameForestCloudUrl          = "game-forest.cloudapp.net";

var GFX_STATUS_LOBBY            = 0,
    GFX_STATUS_CHOOSE           = 1,
    GFX_STATUS_PLAYING          = 2,
    GFX_STATUS_FINISHED         = 4;

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
    
    // critical variables

    this.lobbyStatus            = 0;

    this.gameId                 = gameId;                   // the unique identifier of the game
    this.lobbyId                = lobbyId;                  // the unique identifier of the lobby the player is in
    this.sessionId              = sessionId;                // the unique identifier of the current session of the player

    this.connectionId           = "";                       // the unique identifier for the websocket connection

    this.wsConnection           = new WebSocket("ws://localhost:8084");     // websocket object

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
        wsPromise               = new promise.Promise();    // promise instance for websocket messages

    // internal game forest messages

    var GFX_INIT_CONNECTION     = "GFX_INIT_CONNECTION",    // message to initiate connection with a game forest server
        GFX_STOP_CONNECTION     = "GFX_STOP_CONNECTION",    // message to terminate connection with a game forest server

    // messages the client will send to the server

        GFX_ASK_DATA            = "GFX_ASK_DATA",           // message to ask for the game's data
        GFX_ASK_USER_DATA       = "GFX_ASK_USER_DATA",      // message to ask for the user's game data

        GFX_SEND_DATA           = "GFX_SEND_DATA",          // message to send game data
        GFX_SEND_USER_DATA      = "GFX_SEND_USER_DATA",     // message to send user game data

        GFX_FINISH              = "GFX_GAME_FINISH",        // message to inform the server the game is finished

        GFX_GAME_START          = "GFX_GAME_START",         // message to inform the server the game should start
        GFX_GAME_START_CONFIRM  = "GFX_GAME_START_CONFIRM", // message to inform the server the client has acknowledged the GFX_GAME_START message

        GFX_NEXT_TURN           = "GFX_NEXT_TURN",          // message to inform the server that its the next player's turn
        GFX_CONFIRM_TURN        = "GFX_CONFIRM_TURN",       // message to change the player's order

    // messages the client will receive from the server

        GFX_START_GAME          = "GFX_START_GAME",         // message to inform the client the game has started
        GFX_START_CHOICE        = "GFX_START_CHOICE",       // message to inform the client the game should display the order choose screen
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

            console.log(message.data);

            switch (parse.Subject)
            {
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

                    pGfxObject.lobbyStatus = GFX_STATUS_FINISHED;

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

                    pGfxObject.lobbyStatus = GFX_STATUS_PLAYING;

                    GameForest.prototype.onGameStart();
                    break;
                case GFX_START_CHOICE:

                    console.log("Server is asking the players to choose! Show the choose screen.");

                    $("#gameForestLobbyWindow").hide();
                    $("#gameForestGameWindow").show();

                    pGfxObject.lobbyStatus = GFX_STATUS_CHOOSE;

                    GameForest.prototype.onGameChoose();
                    break;
                case GFX_GAME_FINISHED:

                    console.log("Server is informing players that the game is finished!");

                    GameForest.prototype.onGameFinish(parse.Message);
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

                    var packagedData = JSON.parse(parse.Message);

                    GameForest.prototype.onUpdateData(packagedData.Key, JSON.parse(packagedData.Data));
                    break;
            }
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
	
	// function to add a NUMERICAL stat to track
	// IMPORTANT: THIS ONLY WORKS WITH STATS TRACKING INT VALUES (not float nor time)
	this.trackStat				= function (statName)
	{
		var p = null;
		
		sendRequest("/game/stat?addStat=" + statName + "&gameid=" + this.gameId, "POST",
			function(result)
			{
				if(result.ResponseType == 0) {
					p = JSON.parse(result.AdditionalData);
					return p;
					}
				else {
					var p = "Err: " + JSON.parse(result.AdditionalData);
					console.log("returned: " + p);
					return p;
				}
			},
			function(status, why)
			{
				if(GameForestVerboseMessaging)
				{
					alert("Error in trackStat function: [status=" + status + "] [reason=" + why + "]");
				}
			});
	}
	
	/******
	this.getStatVal = function (
		statName = the name of the statistic you want to fetch, should be same as
				   in trackStat
		statValList = array of stat value, each value you fetch for your game
					  will be pushed to this array
		statNameList= array of stat names, this is to identify which statistic gets added
		              to the array first
		callback = this.cbStat, callback function to push stat value into array
		)
		
	statValList and statNameList are designed this way
	because the calls to fetch statistics are asynchronous 
	and thus you won't always get the stats in the same
	order. 
	
	this.cbStat	    = function (
		statList = array of stats
		data	 = statistic fetched from the database
		)
		
	THIS ONLY WORKS FOR INTEGER VALUE STATS
	
	example:
		gf.getStatVal(statName, statValList, statNameList, gf.cbStat);
		will push statName's name to statNameList,
		statName's value to statValList
	
	******/
	
	this.cbStat					= function (statVals, statNames, data) 
	{
		console.log(data); // extract stat object
		var statName = data.stat_name;
		statNames.push(statName); // push stat's name to list of stat names
		var statVal = data.stat_value;
		statVals.push(statVal); // push the stat's value to list of values, same index
		
		// How to match names to value below, DEBUG
		for(var i = 0; i < statNames.length; i++)
			console.log(statNames[i] + ": " + statVals[i]);
	}
	
	this.getStatVal				= function (statName, statValList, statNameList, callback)
	{
		var p = { };
		var statOut = null;
		
		// DO NOT CHANGE false 
		sendRequest("/game/stats?getstat=" + statName + "&gameid=" + this.gameId + "&all=false", "GET",
			function(result)
			{
				if(result.ResponseType == 0) 
				{
					p = JSON.parse(result.AdditionalData);
					console.log("name... "+p.stat_name);
					console.log("value... "+p.stat_value);
					
					// callback is "this.cbStat" 
					callback(statValList, statNameList, p);
				}
				else
				{
					p = JSON.parse(result.AdditionalData);
					
					console.log("Err");
					console.log(p)
				}
			},
			function(status, why)
			{
				if(GameForestVerboseMessaging)
				{
					alert("Error in getStat function: [status=" + status + "] [reason=" + why + "]");
				}
			});
	}
	
	// function to update a tracked stat
	// IMPORTANT: THIS ONLY WORKS WITH statValue BEING AN INT
	this.updateStat				= function (statName, statValue)
	{
		sendRequest("/game/stats?updatestat=" + statName + "&gameid=" + this.gameId + "&statvalue=" + statValue, "POST",
			function(result)
			{
				if(result.ResponseType == 0)
				{
					var p = JSON.parse(result.AdditionalData);
					return p;
				}
				else
				{
					var p = result.ResponseType + ": " + result.AdditionalData;
				}
			},
			function(status, why)
			{
				if(GameForestVerboseMessaging)
				{
					alert("Error in updateStat function: [status=" + status + "] [reason=" + why + "]");
				}
			});
	}
	
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
    console.log("Game start signal invoked!");

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
GameForest.prototype.onUpdateData       = function (key, data)
{

};

// method to override when the game is finished
GameForest.prototype.onGameFinish       = function (tallyList)
{

};
