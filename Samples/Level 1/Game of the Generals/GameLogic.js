/// <reference path="http://game-forest.cloudapp.net:46069/dev/api/guid.js" />
/// <reference path="http://game-forest.cloudapp.net:46069/dev/api/jquery.js" />
/// <reference path="http://game-forest.cloudapp.net:46069/dev/api/promise.js" />

/// <reference path="http://game-forest.cloudapp.net:46069/dev/api/gameforest.js" />
/// <reference path="http://game-forest.cloudapp.net:46069/dev/api/gameforesttracking.js" />

/// <reference path="index.html" />

'use strict';

// gameforest-specific data

// Set this to localhost when debugging and to game-forest.cloudapp.net when submitting it to Game Forest.
GameForestCloudUrl          = "game-forest.cloudapp.net";

// Set this to true when debugging to let Game Forest show alert messages when something went wrong.
GameForestVerboseMessaging  = false;

// game specific data

var GAME_STATE_CHOOSE_PIECE = 0;
var GAME_STATE_PLAYING_GAME = 1;

var gameState               = GAME_STATE_CHOOSE_PIECE;

var allowInteraction        = false;

var player1                 = null;
var player2                 = null;

var targetCell              = null;
var sourceCell              = null;

var pickedUp                = false;

var winLossMatrix           =
[
    [0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1],
    [2, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1],
    [2, 2, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1],
    [2, 2, 2, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1],
    [2, 2, 2, 2, 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 1],
    [2, 2, 2, 2, 2, 0, 1, 1, 1, 1, 1, 1, 1, 2, 1],
    [2, 2, 2, 2, 2, 2, 0, 1, 1, 1, 1, 1, 1, 2, 1],
    [2, 2, 2, 2, 2, 2, 2, 0, 1, 1, 1, 1, 1, 2, 1],
    [2, 2, 2, 2, 2, 2, 2, 2, 0, 1, 1, 1, 1, 2, 1],
    [2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 1, 1, 1, 2, 1],
    [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 1, 1, 2, 1],
    [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 1, 2, 1],
    [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 1, 1],
    [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 0, 1],
    [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3],
];

var imageUrls               =
[
    "general5star.png"  ,
    "general4star.png"  ,
    "general3star.png"  ,
    "general2star.png"  ,
    "general1star.png"  ,
    "colonel.png"       ,
    "ltcolonel.png"     ,
    "major.png"         ,
    "captain.png"       ,
    "lieu1st.png"       ,
    "lieu2nd.png"       ,
    "sergeant.png"      ,
    "private.png"       ,
    "spy.png"           ,
    "flag.png"
];

var imageObj                =
[

];

var gameData                =
{
    player1PiecesOk:        false,
    player2PiecesOk:        false,

    boardData:
    [
        ["", "", "", "", "", "", "", "", ""],
        ["", "", "", "", "", "", "", "", ""],
        ["", "", "", "", "", "", "", "", ""],

        ["", "", "", "", "", "", "", "", ""],
        ["", "", "", "", "", "", "", "", ""],

        ["", "", "", "", "", "", "", "", ""],
        ["", "", "", "", "", "", "", "", ""],
        ["", "", "", "", "", "", "", "", ""]
    ],

    conflictData:
    {
        winner:         "", // 0 if draw, 1 if player 1 wins, 2 if player 2 wins, 3 if this is a flag conflict
        winnerFlag:     "", // additional data for the flag conflict, 1 if player 1 wins, or 2 if player 2 wins

        player1Piece:   "",
        player2Piece:   "",
        status:         false
    }
};

function                            onDocumentReady         ()
{
    // load images
    for (var i = 0; i < imageUrls.length; i++)
    {
        var img     = new Image();
            img.src = "img/" + imageUrls[i];

        imageObj.push(img);
    }
};

function                            clickChoosePlace        ()
{
    if (pickedUp)
    {
        pickedUp    = false;
        targetCell  = event.target;

        sourceCell.style.backgroundColor = "";

        var sourcePiece = sourceCell.dataset.piece;
        var targetPiece = targetCell.dataset.piece;

        var sourceImage = sourceCell.style.backgroundImage;
        var targetImage = targetCell.style.backgroundImage;

        targetCell.dataset.piece            = sourcePiece;
        sourceCell.dataset.piece            = targetPiece;

        targetCell.style.backgroundImage    = sourceImage;
        sourceCell.style.backgroundImage    = targetImage;

        targetCell = null;
        sourceCell = null;
    }
    else
    {
        pickedUp    = true;
        sourceCell  = event.target;

        sourceCell.style.backgroundColor = "gold";
    }
}

function                            buttonAcceptClick       ()
{
    var stopp = false;

    for (var y = 1; y <= 3; y++)
    {
        for (var x = 1; x <= 7; x++)
        {
            var ce = document.getElementById("source_" + y + "_" + x);
            var cp = ce.dataset.piece;

            console.log(cp);

            if (cp != "")
            {
                alert("Please fill up the board above.");

                stopp = true;
                break;
            }
        }

        if (stopp)
        {
            break;
        }
    }

    if (stopp == false)
    {
        alert("Placement accepted!");

        if      (gf.playerOrderIndex() == 1)
        {
            gameData.player1PiecesOk = true;
        }
        else if (gf.playerOrderIndex() == 2)
        {
            gameData.player2PiecesOk = true;
        }

        transferToGameBoard(gf.playerOrderIndex() == 1);
    }
}

function                            transferToGameBoard     (flipped)
{
    for (var y = 1; y <= 3; y++)
    {
        for (var x = 1; x <= 9; x++)
        {
            var te = document.getElementById("target_" + y + "_" + x);
            var tp = te.dataset.piece;

            gameData.boardData[flipped ? (8 - y) : (y - 1)][x - 1] = tp;
        }
    }

    gf.nextTurn(gameData);

    $("#gamePickTable").hide();
    $("#gameWaitPanel").show();
}

// gameforest methods

// method to override when the game has started
GameForest.prototype.onGameStart    = function ()
{
    $("#gamePickTable").hide();
    $("#gameWaitPanel").show();
};

// method to override when its the player's turn
GameForest.prototype.onTurnStart    = function ()
{
    if (gameState == GAME_STATE_CHOOSE_PIECE)
    {
        $("#gamePickTable").show();
    }
};

// method to override when its the other player's turn
GameForest.prototype.onTurnChange   = function ()
{

};

// method to override when the game's data has changed
GameForest.prototype.onUpdateData   = function (key, data)
{
    gameData = data;

    console.log(JSON.stringify(gameData));
};

// method to override when the game is finished
GameForest.prototype.onGameFinish   = function (tallyList)
{

};
