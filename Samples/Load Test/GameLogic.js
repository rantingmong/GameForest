/// <reference path="http://game-forest.cloudapp.net:46069/dev/api/guid.js" />
/// <reference path="http://game-forest.cloudapp.net:46069/dev/api/jquery.js" />
/// <reference path="http://game-forest.cloudapp.net:46069/dev/api/promise.js" />

/// <reference path="http://game-forest.cloudapp.net:46069/dev/api/gameforest.js" />
/// <reference path="http://game-forest.cloudapp.net:46069/dev/api/gameforesttracking.js" />

/// <reference path="index.html" />


'use strict';

// gameforest-specific data

// Set this to localhost when debugging and to game-forest.cloudapp.net when submitting it to Game Forest.
GameForestCloudUrl                  = "game-forest.cloudapp.net";

// Set this to true when debugging to let Game Forest show alert messages when something went wrong.
GameForestVerboseMessaging          = false;

// gameforest callback methods
var prevDateTime = new Date();
var currDateTime = new Date();

function onDocumentReady()
{

};

// method to override when the game has started
GameForest.prototype.onGameStart            = function ()
{
    var playerList          = gf.userList();
    var playerListObject    = document.getElementById("playerList");

    for (var i = 0; i < playerList.length; i++)
    {
        var li              = document.createElement("li");
            li.innerText    = playerList[i].Username;

        playerListObject.appendChild(li);
    }

    document.getElementById("gameStatus").innerHTML = "Game started!";
};

// method to override when its the player's turn
GameForest.prototype.onTurnStart            = function ()
{
    prevDateTime = currDateTime;
    currDateTime = new Date();

    var offset = currDateTime.getTime() - prevDateTime.getTime();

    document.getElementById("gameStatus")       .innerHTML = "Turn started!";
    document.getElementById("roundAboutTime")   .innerHTML = offset + " milliseconds.";

    gf.nextTurn().then(function (error, result)
    {
        document.getElementById("gameStatus")   .innerHTML = "Waiting for turn...";
    });
};

// method to override when its the other player's turn
GameForest.prototype.onTurnChange           = function ()
{

};

// method to override when the game's data has changed
GameForest.prototype.onUpdateData           = function (key, data)
{

};

// method to override when the game is finished
GameForest.prototype.onGameFinish           = function (tallyList)
{

};

// OPTIONAL ADVANCED METHODS -----------------------------------------------------------

/*

// method called when the player clicked the start button
GameForest.prototype.onGameStartSignal = function ()
{
    console.log("Game start signal invoked!");

    $("#gfxButtonStart").removeAttr("disabled");
};

// method to override when the game should display the choose screen
GameForest.prototype.onGameChoose = function ()
{

};

// method to override when its the player's time to change his/her order
GameForest.prototype.onTurnSelect = function (originalTurn)
{
    this.confirmTurn(originalTurn);
};

// method to override when a player is suddenly disconnected
GameForest.prototype.onPlayerDisconnected = function (who)
{
    $("#gameForestClientDisconnected").show();
    $("#gameForestPlayerDisconnected").hide();

    $("#gameForestDisconnectedPlayer").html(who.Name + " was disconnected from the game.");

    $("#gameForestDisconnectWindow").fadeIn("fast");

    var countdownTimer = 30;

    setInterval(function ()
    {
        $("#gameForestCountdownWait").html(countdownTimer);

        countdownTimer -= 1;

        if (countdownTimer <= 0)
        {

        }
    }, 1000);
};

// method to override when a player is reconnected
GameForest.prototype.onPlayerReconnected = function ()
{
    $("#gameForestDisconnectWindow").fadeOut("fast");
};

// method to override when the player is disconnected
GameForest.prototype.onGameDisconnected = function ()
{
    $("#gameForestClientDisconnected").hide();
    $("#gameForestPlayerDisconnected").show();

    $("#gameForestDisconnectWindow").fadeIn("fast");

    var countdownTimer = 30;

    setInterval(function ()
    {
        $("#gameForestCountdownKill").html(countdownTimer);

        countdownTimer -= 1;

        this.askForReconnection();

    }, 1000);
};

*/