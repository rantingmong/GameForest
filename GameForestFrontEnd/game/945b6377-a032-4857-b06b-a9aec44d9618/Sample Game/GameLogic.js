/// <reference path="GameForest.js" />
/// <reference path="guid.js" />
/// <reference path="jquery.js" />
/// <reference path="promise.js" />
/// <reference path="index.html" />

'use strict';

GameForestCloudUrl          = "localhost";
GameForestVerboseMessaging  = true;

var canvasObject            = document.getElementById("sampleGameCanvas");
var canvasContex            = canvasObject.getContext("2d");

var gameData                = {

    gameTile:   [ [ ' ', ' ', ' ' ],
                  [ ' ', ' ', ' ' ],
                  [ ' ', ' ', ' ' ] ]
};

var myTurn                  = false;

var showUpdateText          = false;

var player1Name             = "";
var player2Name             = "";

var selectedTile            = { x: 0, y: 0 };

var mousePositionMsg        = "";
var mousePosInTileMsg       = "";
var selectedTileMsg         = "";

var highlightBox            = "lightGray";

Math.clamp = function (value, min, max)
{
    if      (value <= min)
        return min;
    else if (value >= max)
        return max;
    else
        return value;
};

function drawTurns  ()
{
    canvasContex.fillStyle  = "black";
    canvasContex.font       = "20px arial";

    canvasContex.fillText("Players:", 20, 20);
}

function drawTile   ()
{
    canvasContex.save();

    canvasContex.translate(190.5, 30.5);

    canvasContex.strokeRect(0, 0, 420, 420);

    canvasContex.moveTo(0, 140);
    canvasContex.lineTo(420, 140);

    canvasContex.moveTo(  0, 280);
    canvasContex.lineTo(420, 280);

    canvasContex.moveTo(140, 0);
    canvasContex.lineTo(140, 420);

    canvasContex.moveTo(280, 0);
    canvasContex.lineTo(280, 420);

    canvasContex.stroke();

    canvasContex.restore();
}

function drawTokens ()
{
    canvasContex.save();

    canvasContex.translate(140 * selectedTile.x + 190, 140 * selectedTile.y + 30);

    canvasContex.fillStyle = highlightBox;
    canvasContex.fillRect(0, 0, 140, 140);

    canvasContex.restore();
}

function mouseMove  (evt)
{
    var rect = canvasObject.getBoundingClientRect();

    var px = evt.pageX - rect.left;
    var py = evt.pageY - rect.top;

    var tx = Math.clamp(px - 190, 0, 420);
    var ty = Math.clamp(py - 30, 0, 420);

    selectedTile.x = Math.clamp(Math.floor(tx / 140), 0, 2);
    selectedTile.y = Math.clamp(Math.floor(ty / 140), 0, 2);

    // do mouse handling stuff here
    mousePositionMsg    = "Mouse position: {" + px + ", " + py + "}";
    mousePosInTileMsg   = "Mouse position relative to tile: {" + tx + ", " + ty + "}";
    selectedTileMsg     = "Selected tile position: {" + selectedTile.x + ", " + selectedTile.y + "}";
}

function mouseClick (evt)
{
    if (myTurn == false)
        return;

    // get the mouse position and place token, in addition to checking if the tile is empty or not
    if (gameData.gameTile[selectedTile.y][selectedTile.x] != ' ')
    {
        gameData.gameTile[selectedTile.y][selectedTile.x] = 'O'; // place the letter that i chose

        // remove the click event of the canvas object while we process gameforest things
        canvasObject.removeEventListener("click", mouseClick);

        // send the game data asynchonously
        gf.sendGameData(gameData).then(function (a, b)
        {
            // if not empty, inform next player
            gf.nextTurn().then(function (ca, cb)
            {
                myTurn = false;
                canvasObject.addEventListener("click", mouseClick);
            });
        });
        
    }
}

function mouseDown  (evt)
{
    highlightBox = "gray";
}

function mouseUp    (evt)
{
    highlightBox = "lightGray";
}

var timer = setInterval(function()
{
    canvasContex.clearRect  (0, 0, 800, 480);

    canvasContex.fillStyle  = "white";
    canvasContex.fillRect   (0, 0, 800, 480);

    drawTokens  ();
    drawTile    ();
    drawTurns   ();

    canvasContex.fillStyle  = "black";
    canvasContex.font       = "12px consolas";

    canvasContex.fillText(mousePositionMsg, 10, 30);
    canvasContex.fillText(mousePosInTileMsg, 10, 50);
    canvasContex.fillText(selectedTileMsg, 10, 70);

}, 1000 / 60);

GameForest.prototype.onGameStart    = function ()
{
    console.log("OnGameStart is invoked!");

    $("#sampleGameChooseScreen").hide();
    $("#sampleGameActualGame").show();

    // get this user's information
    gf.getUserInfo().then(function (error, result)
    {
        var data = JSON.parse(result);
        
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
        gf.confirmTurn(1);
    });

    $("#sampleGameButtonChooseX").click(function ()
    {
        gf.confirmTurn(2);
    });
};

GameForest.prototype.onTurnStart    = function ()
{
    myTurn = true;
};

GameForest.prototype.onTurnChange   = function ()
{

};

GameForest.prototype.onUpdateData   = function (updatedData)
{

};

GameForest.prototype.onGameFinish   = function ()
{

};

GameForest.prototype.onGameTally    = function (tallyResults)
{

};
