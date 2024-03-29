﻿/// <reference path="GameForest.js" />
/// <reference path="guid.js" />
/// <reference path="jquery.js" />
/// <reference path="promise.js" />
/// <reference path="index.html" />

'use strict';

// gameforest-specific data

GameForestCloudUrl                  = "localhost";
GameForestVerboseMessaging          = false;

// DOM elements

var canvasObject                    = document.getElementById("sampleGameCanvas");
var canvasContex                    = canvasObject.getContext("2d");

// data that is shared between players

var gameData                        = {

    gameTile:   [ [ ' ', ' ', ' ' ],
                  [ ' ', ' ', ' ' ],
                  [ ' ', ' ', ' ' ] ]
};

var chooseData                      = {

    oTaken: false,
    xTaken: false
};

// user-specific data

var finished                        = false;

var myTurn                          = false;
var myToken                         = ' ';

var xPlayer                         = null; // we know that X player is always the second one
var oPlayer                         = null; // we know that O player is always the first one

var currentPlayer                   = null;

var xPlayerName                     = "";
var oPlayerName                     = "";

var showUpdateText                  = false;

var selectedTile                    = { x: 0, y: 0 };

// debug data

var mousePositionMsg                = "";
var mousePosInTileMsg               = "";
var selectedTileMsg                 = "";

var highlightBox                    = "lightGray";

// clams "value" from "min" to "max"
Math.clamp                          = function (value, min, max)
{
    if (value <= min)
    {
        return min;
    }
    else if (value >= max)
    {
        return max;
    }

    return value;
};

// game loop

var timer                           = setInterval(function()
{
    canvasContex.clearRect  (0, 0, 800, 480);

    canvasContex.fillStyle  = "white";
    canvasContex.fillRect   (0, 0, 800, 480);

    drawTokens  ();
    drawTile    ();
    drawTurns   ();

}, 1000 / 60);

// game specific functions

function drawTile                   ()
{
    canvasContex.save();

    canvasContex.translate(190.5, 30.5);

    canvasContex.strokeRect(0, 0, 420, 420);

    canvasContex.moveTo(0, 140);
    canvasContex.lineTo(420, 140);

    canvasContex.moveTo(0, 280);
    canvasContex.lineTo(420, 280);

    canvasContex.moveTo(140, 0);
    canvasContex.lineTo(140, 420);

    canvasContex.moveTo(280, 0);
    canvasContex.lineTo(280, 420);

    canvasContex.stroke();

    canvasContex.restore();
};

function drawTurns                  ()
{
    canvasContex.fillStyle = "black";
    canvasContex.font = "20px arial";

    canvasContex.fillText("Players:", 20, 20);

    if (currentPlayer != null && oPlayer != null)
    {
        if (currentPlayer.UserId == oPlayer.UserId)
            canvasContex.fillStyle = "green";
        else
            canvasContex.fillStyle = "black";
    }

    canvasContex.fillText("O: " + oPlayerName, 20, 40);

    if (currentPlayer != null && xPlayer != null)
    {
        if (currentPlayer.UserId == xPlayer.UserId)
            canvasContex.fillStyle = "green";
        else
            canvasContex.fillStyle = "black";
    }

    canvasContex.fillText("X: " + xPlayerName, 20, 60);
};

function drawTokens                 ()
{
    canvasContex.save();

    canvasContex.translate(140 * selectedTile.x + 190, 140 * selectedTile.y + 30);

    canvasContex.fillStyle = highlightBox;
    canvasContex.fillRect(0, 0, 140, 140);

    canvasContex.restore();

    canvasContex.save();

    canvasContex.textBaseline   = "middle";
    canvasContex.fillStyle      = "black";
    canvasContex.font           = "30px arial";

    for (var y = 0; y < 3; y++)
    {
        for (var x = 0; x < 3; x++)
        {
            var tx = gameData.gameTile[y][x];
            var tm = canvasContex.measureText(tx);

            canvasContex.fillText(tx, x * 140 + 190 + ((140 - tm.width) / 2), y * 140 + 85);
        }
    }

    canvasContex.restore();
};

function mouseMove                  (evt)
{
    var rect = canvasObject.getBoundingClientRect();

    var px = evt.pageX - rect.left;
    var py = evt.pageY - rect.top;

    var tx = Math.clamp(px - 190, 0, 420);
    var ty = Math.clamp(py - 30, 0, 420);

    selectedTile.x = Math.clamp(Math.floor(tx / 140), 0, 2);
    selectedTile.y = Math.clamp(Math.floor(ty / 140), 0, 2);

    // do mouse handling stuff here
    mousePositionMsg = "Mouse position: {" + px + ", " + py + "}";
    mousePosInTileMsg = "Mouse position relative to tile: {" + tx + ", " + ty + "}";
    selectedTileMsg = "Selected tile position: {" + selectedTile.x + ", " + selectedTile.y + "}";
};

function mouseClick                 (evt)
{
    if (myTurn == false)
        return;

    // get the mouse position and place token, in addition to checking if the tile is empty or not
    if (gameData.gameTile[selectedTile.y][selectedTile.x] == ' ')
    {
        gameData.gameTile[selectedTile.y][selectedTile.x] = myToken; // place the letter that i chose

        // remove the click event of the canvas object while we process gameforest things
        canvasObject.removeEventListener("click", mouseClick);

        // send the game data asynchonously
        gf.thenStarter()
            .then(function (error, result)
            {
                return gf.sendGameData("GameData", gameData);
            })
            .then(function (error, result)
            {
                return gf.nextTurn();
            })
            .then(function (error, result)
            {
                myTurn = false;
                canvasObject.addEventListener("click", mouseClick);
            });
    }
};

function mouseDown                  (evt)
{
    highlightBox = "gray";
};

function mouseUp                    (evt)
{
    highlightBox = "lightGray";
};

function onDocumentReady            ()
{
    // we register events for the game

    canvasObject.addEventListener("mousemove", mouseMove, false);
    canvasObject.addEventListener("mouseup", mouseUp, false);
    canvasObject.addEventListener("click", mouseClick, false);
    canvasObject.addEventListener("mousedown", mouseDown, false);
};

// gameforest callback methods

GameForest.prototype.onGameStart    = function ()
{
    console.log("OnGameStart is invoked!");

    $("#sampleGameChooseScreen").hide();
    $("#sampleGameActualGame").show();

    var userInfo = null;

    // lol chain starter
    gf.thenStarter()
        .then(function (error, result)
        {
            // get this user's information
            return gf.getUserInfo();
        })
        .then(function (error, result)
        {
            /* returns: GFXUserRow
                {
                    UserId,
                    Password,
                    Username,
                    FirstName,
                    LastName,
                    Description
                }
             */

            console.log("UserId: " + result.UserId + "\nPassword: " + result.Password + "\nUsername: " + result.Username);

            userInfo = result;

            // get player's order
            return gf.getUserOrder();
        })
        .then(function (error, result)
        {
            // we have the user's order number!
            console.log("Player order: " + result);

            if     (result == 1)
            {
                myToken = 'O';

                oPlayer = userInfo;
            }
            else if (result == 2)
            {
                myToken = 'X';

                xPlayer = userInfo;
            }

            return gf.getNextPlayer(1);
        })
        .then(function (error, result)
        {
            /* returns: GFXUserRow
                {
                    UserId,
                    Password,
                    Username,
                    FirstName,
                    LastName,
                    Description
                }
             */

            console.log("Get next player: " + JSON.stringify(result));

            if      (myToken == 'O')
                xPlayer = result;
            else if (myToken == 'X')
                oPlayer = result;

            xPlayerName = xPlayer.Username;
            oPlayerName = oPlayer.Username;
        });
};

GameForest.prototype.onGameChoose   = function ()
{
    console.log("OnGameChoose is invoked!");

    $("#sampleGameChooseScreen").show();
};

GameForest.prototype.onTurnSelect   = function (originalTurn)
{
    console.log("My turn is " + originalTurn);

    $("#sampleGameButtonChooseO").click(function ()
    {
        console.log("o token taken!");

        chooseData.oTaken = true;

        gf.sendGameData("chooseData", chooseData)
            .then(function (error, result)
            {
                gf.confirmTurn(1);
            });
    });

    $("#sampleGameButtonChooseX").click(function ()
    {
        console.log("x token taken!");

        chooseData.xTaken = true;

        gf.sendGameData("chooseData", chooseData)
            .then(function (error, result)
            {
                gf.confirmTurn(2);
            });
    });
};

GameForest.prototype.onTurnStart    = function ()
{
    myTurn      = true;
    finished    = false;

    // check if game is finished

    // x = 4 points
    // o = 5 points

    var scoreTile = [

        [ 0, 0, 0 ],
        [ 0, 0, 0 ],
        [ 0, 0, 0 ]
    ];

    // convert gameTile to scoreTile
    for (var y = 0; y < 3; y++)
    {
        for (var x = 0; x < 3; x++)
        {
            var score = 0;

            switch (gameData.gameTile[y][x])
            {
                case 'O': score = 5;
                    break;
                case 'X': score = 4;
                    break;
            }

            scoreTile[y][x] = score;
        }
    }

    console.log("Score tile data: ");

    console.log("[" + scoreTile[0][0] + ", " + scoreTile[0][1] + ", " + scoreTile[0][2] + "]");
    console.log("[" + scoreTile[1][0] + ", " + scoreTile[1][1] + ", " + scoreTile[1][2] + "]");
    console.log("[" + scoreTile[2][0] + ", " + scoreTile[2][1] + ", " + scoreTile[2][2] + "]");

    var c1 = scoreTile[0][0] + scoreTile[1][0] + scoreTile[2][0],
        c2 = scoreTile[0][1] + scoreTile[1][1] + scoreTile[2][1],
        c3 = scoreTile[0][2] + scoreTile[1][2] + scoreTile[2][2],

        c4 = scoreTile[0][2] + scoreTile[0][1] + scoreTile[0][0],
        c5 = scoreTile[1][2] + scoreTile[1][1] + scoreTile[1][0],
        c6 = scoreTile[2][2] + scoreTile[2][1] + scoreTile[2][0],

        c7 = scoreTile[0][2] + scoreTile[1][1] + scoreTile[2][0],
        c8 = scoreTile[0][0] + scoreTile[1][1] + scoreTile[2][2];

    if (c1 == 15 || c2 == 15 || c3 == 15 || c4 == 15 || c5 == 15 || c6 == 15 || c7 == 15 || c8 == 15)
    {
        finished = true;

        // O wins
        gf.finishGame("O");
    }

    if (c1 == 12 || c2 == 12 || c3 == 12 || c4 == 12 || c5 == 12 || c6 == 12 || c7 == 12 || c8 == 12)
    {
        finished = true;

        // X wins
        gf.finishGame("X");
    }

    if (finished == false)
    {
        gf.thenStarter()
            .then(function (error, result)
            {
                return gf.getCurrentPlayer();
            })
            .then(function (error, result)
            {
                console.log(JSON.stringify(result));

                currentPlayer = result;
            });
    }
};

GameForest.prototype.onTurnChange   = function ()
{
    gf.thenStarter()
        .then(function (error, result)
        {
            return gf.getCurrentPlayer();
        })
        .then(function (error, result)
        {
            console.log(JSON.stringify(result));

            currentPlayer = result;
        });
};

GameForest.prototype.onUpdateData   = function (key, updatedData)
{
    console.log(gf.getLobbyState());

    if      (gf.getLobbyState() == GFX_STATUS_CHOOSE)
    {
        console.log(updatedData.oTaken);
        console.log(updatedData.xTaken);

        if (updatedData.oTaken)
        {
            $("#sampleGameButtonChooseO").prop("disabled", true);
        }
        
        if (updatedData.xTaken)
        {
            $("#sampleGameButtonChooseX").prop("disabled", true);
        }

        chooseData = updatedData;
    }
    else if (gf.getLobbyState() == GFX_STATUS_PLAYING)
    {
        gameData = updatedData;
    }
};

GameForest.prototype.onGameFinish   = function (tallyList)
{
    alert(tallyList);

    gf.navigateToGame();
};
