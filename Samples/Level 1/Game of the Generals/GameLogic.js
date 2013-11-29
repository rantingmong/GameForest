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

var GAME_STATE_CHOOSE_PIECE         = 0;
var GAME_STATE_PLAYING_GAME         = 1;
var GAME_STATE_FLAG_ON_OTHER_BASE   = 2;

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

    // TODO: give this a better name
    flagAlmostThere:
    {
        who:            0,
        status:         false
    },

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

function                            clickGamePlace          ()
{
    if (allowInteraction == false)
        return;

    if (pickedUp)
    {
        targetCell = event.target;

        if      (targetCell.dataset.piece == '')
        {
            console.log("Normal move");

            // show the avaiable tiles the player can move along
            var tx = Number(sourceCell.id.split("_")[2]);
            var ty = Number(sourceCell.id.split("_")[1]);

            var ct = document.getElementById("game_" + (ty - 1) + "_" + tx);
            var cd = document.getElementById("game_" + (ty + 1) + "_" + tx);
            var cl = document.getElementById("game_" + ty + "_" + (tx - 1));
            var cr = document.getElementById("game_" + ty + "_" + (tx + 1));

            if (targetCell == ct || targetCell == cd || targetCell == cl || targetCell == cr)
            {
                console.log("Moving!");
                
                var sx = Number(sourceCell.id.split("_")[2]) - 1;
                var sy = Number(sourceCell.id.split("_")[1]) - 1;

                var tx = Number(targetCell.id.split("_")[2]) - 1;
                var ty = Number(targetCell.id.split("_")[1]) - 1;

                gameData.boardData[ty][tx] = gameData.boardData[sy][sx];
                gameData.boardData[sy][sx] = "";

                gf.nextTurn(gameData);
            }
            else
            {
                console.warn("Invalid move.");
            }
        }
        else if (Number(targetCell.dataset.piece.split("_")[0]) != gf.playerOrderIndex())
        {
            console.log("Enemy move");

            var sourceData = sourceCell.dataset.piece;
            var targetData = targetCell.dataset.piece;

            var sDataPlayer = Number(sourceData[0]);
            var sDataPiece  = sourceData[2];
            
            var tDataPlayer = Number(targetData[0]);
            var tDataPiece  = targetData[2];

            var player1Piece = 0;
            var player2Piece = 0;

            // flag is being eaten! the other player wins.
            if (sDataPiece == 'e')
            {
                if(sDataPlayer == 1)
                {
                    gf.finishGame(2);
                }
                else
                {
                    gf.finishGame(1);
                }

                return;
            }

            // determine whose player 1 and player 2
            if (sDataPlayer == 1)
            {
                gameData.conflictData.player1Piece = sDataPiece;
                gameData.conflictData.player2Piece = tDataPiece;

                player1Piece = stringToInt(sDataPiece);
                player2Piece = stringToInt(tDataPiece);
            }
            else
            {
                gameData.conflictData.player1Piece = tDataPiece;
                gameData.conflictData.player2Piece = sDataPiece;

                player1Piece = stringToInt(tDataPiece);
                player2Piece = stringToInt(sDataPiece);
            }

            gameData.conflictData.status = true;
            gameData.conflictData.winner = winLossMatrix[player1Piece][player2Piece];

            var sx = Number(sourceCell.id.split("_")[2]) - 1;
            var sy = Number(sourceCell.id.split("_")[1]) - 1;

            var tx = Number(targetCell.id.split("_")[2]) - 1;
            var ty = Number(targetCell.id.split("_")[1]) - 1;

            var dn = false;

            // update the board based from who won
            switch(gameData.conflictData.winner)
            {
                case 0:     // draw!
                    {
                        // REMOVE BOTH SOURCE AND DESTINATION

                        gameData.boardData[sy][sx] = "";
                        gameData.boardData[tx][ty] = "";
                    }
                    break;
                case 1:     // player 1 wins
                    {
                        // determine who's player 1
                        if (sDataPlayer == 1)
                        {
                            // if source is player 1, source -> target
                            var sourceData = gameData.boardData[sy][sx];

                            gameData.boardData[ty][tx] = sourceData;
                            gameData.boardData[sy][sx] = "";
                        }
                        else
                        {
                            // if target is player 1, source is deleted
                            gameData.boardData[sy][sx] = "";
                        }
                    }
                    break;
                case 2:     // player 2 wins
                    {
                        // determine who's player 2
                        if (sDataPlayer == 2)
                        {
                            // if source is player 2, source -> target
                            var sourceData = gameData.boardData[sy][sx];

                            gameData.boardData[ty][tx] = sourceData;
                            gameData.boardData[sy][sx] = "";
                        }
                        else
                        {
                            // if target is player 2, source is deleted
                            gameData.boardData[sy][sx] = "";
                        }
                    }
                    break;
                case 3:     // flag case!

                    // this will send a gf.finishGame() command!

                    // source -> target (source wins!)

                    if (sDataPlayer == 1)
                    {
                        gf.finishGame(1);
                    }
                    else
                    {
                        gf.finishGame(2);
                    }

                    dn = true;

                    break;
            }

            if (dn)
            {
                pickedUp = true;
                updateBoard();

                return;
            }

            // check if a player has any pieces left
            var p1Count = 0;
            var p2Count = 0;

            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 9; x++)
                {
                    var player = gameData.boardData[y][x][0];

                    if (player == "1")
                    {
                        p1Count++;
                    }
                    else
                    {
                        p2Count++;
                    }
                }
            }

            if      (p1Count == 0)
            {
                gf.finishGame(2);
            }
            else if(p2Count == 0)
            {
                gf.finishGame(1);
            }
            else
            {
                gf.nextTurn(gameData);
            }
        }

        pickedUp = false;

        updateBoard();
    }
    else
    {
        pickedUp    = true;
        sourceCell  = event.target;

        // check the selected tile
        if (sourceCell.dataset.piece == '' || Number(sourceCell.dataset.piece.split("_")[0]) != gf.playerOrderIndex())
        {
            pickedUp = false;

            console.warn("Ehh...");
        }
        else
        {
            console.log("Accepted!");

            // show the avaiable tiles the player can move along
            var tx = Number(sourceCell.id.split("_")[2]);
            var ty = Number(sourceCell.id.split("_")[1]);

            var ct = document.getElementById("game_" + (ty - 1) + "_" + tx);
            var cd = document.getElementById("game_" + (ty + 1) + "_" + tx);
            var cl = document.getElementById("game_" + ty + "_" + (tx - 1));
            var cr = document.getElementById("game_" + ty + "_" + (tx + 1));

            if (ct != null || ct != undefined)
            {
                var ctp = ct.dataset.piece;

                if      (ctp == '')
                {
                    ct.style.backgroundColor = "green";
                }
                else if (ctp[0] != gf.playerOrderIndex())
                {
                    ct.style.backgroundColor = "orange";
                }
                else if (ctp[0] == gf.playerOrderIndex())
                {
                    ct.style.backgroundColor = "red";
                }
            }

            if (cd != null || cd != undefined)
            {
                var ctd = cd.dataset.piece;

                if      (ctd == '')
                {
                    cd.style.backgroundColor = "green";
                }
                else if (ctd[0] != gf.playerOrderIndex())
                {
                    cd.style.backgroundColor = "orange";
                }
                else if (ctd[0] == gf.playerOrderIndex())
                {
                    cd.style.backgroundColor = "red";
                }
            }

            if (cl != null || cl != undefined)
            {
                var ctl = cl.dataset.piece;

                if      (ctl == '')
                {
                    cl.style.backgroundColor = "green";
                }
                else if (ctl[0] != gf.playerOrderIndex())
                {
                    cl.style.backgroundColor = "orange";
                }
                else if (ctl[0] == gf.playerOrderIndex())
                {
                    cl.style.backgroundColor = "red";
                }
            }

            if (cr != null || cr != undefined)
            {
                var ctr = cr.dataset.piece;

                if      (ctr == '')
                {
                    cr.style.backgroundColor = "green";
                }
                else if (ctr[0] != gf.playerOrderIndex())
                {
                    cr.style.backgroundColor = "orange";
                }
                else if (ctr[0] == gf.playerOrderIndex())
                {
                    cr.style.backgroundColor = "red";
                }
            }
        }
    }
}

function                            updateBoard             ()
{
    for (var y = 0; y < 8; y++)
    {
        for (var x = 0; x < 9; x++)
        {
            var elem                = document.getElementById("game_" + (y + 1) + "_" + (x + 1));
                elem.dataset.piece  = gameData.boardData[y][x];

            if (gameData.boardData[y][x] == '')
            {
                elem.style.backgroundColor = "";
                elem.style.backgroundImage = "";
            }
            else
            {
                var p = gameData.boardData[y][x][0];
                var e = gameData.boardData[y][x][2];

                if      (p == '1')
                {
                    elem.style.backgroundColor = "#f59393";

                    if (gf.playerOrderIndex() == 1)
                    {
                        elem.style.backgroundImage = "url(img/" + imageUrls[stringToInt(e)] + ")";
                    }
                    else
                    {
                        elem.style.backgroundImage = "";
                    }
                }
                else if (p == '2')
                {
                    elem.style.backgroundColor = "#4b87f3";

                    if (gf.playerOrderIndex() == 2)
                    {
                        elem.style.backgroundImage = "url(img/" + imageUrls[stringToInt(e)] + ")";
                    }
                    else
                    {
                        elem.style.backgroundImage = "";
                    }
                }
            }
        }
    }

    var row0 = gameData.boardData[0]; // player 1
    var row7 = gameData.boardData[7]; // player 2

    // check if there's a enemy flag in either row0 or row 7
    for (var x = 0; x < 9; x++)
    {
        if (row0[x] == '2_e')
        {
            gameData.flagAlmostThere.who    = 2;
            gameData.flagAlmostThere.status = true;

            // check adjacent cells
            var l = gameData.boardData[0][x - 1];
            var r = gameData.boardData[0][x + 1];
            var b = gameData.boardData[1][x];

            if (l == "" && r == "" && b == "")
            {
                gf.finishGame(2);
            }
        }
    }

    for (var x = 0; x < 9; x++)
    {
        if (row7[x] == '1_e')
        {
            gameData.flagAlmostThere.who    = 1;
            gameData.flagAlmostThere.status = true;

            // check adjacent cells
            var l = gameData.boardData[7][x - 1];
            var r = gameData.boardData[7][x + 1];
            var t = gameData.boardData[6][x];

            if (l == "" && r == "" && t == "")
            {
                gf.finishGame(1);
            }
        }
    }
}

function                            refreshBoard            ()
{
    for (var y = 1; y <= 8; y++)
    {
        for (var x = 1; x <= 9; x++)
        {
            var elem = document.getElementById("game_" + y + "_" + x);

            gameData.boardData[y - 1][x - 1] = elem.dataset.piece;
        }
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

            if (tp != '')
                gameData.boardData[!flipped ? (4 + y) : (3 - y)][x - 1] = gf.playerOrderIndex() + "_" + tp;
        }
    }

    gf.nextTurn(gameData);

    $("#gamePickTable").hide();
    $("#gameWaitPanel").show();
}

function                            stringToInt             (int)
{
    switch (int.toLowerCase())
    {
        case "0":
            return 0;
        case "1":
            return 1;
        case "2":
            return 2;
        case "3":
            return 3;
        case "4":
            return 4;
        case "5":
            return 5;
        case "6":
            return 6;
        case "7":
            return 7;
        case "8":
            return 8;
        case "9":
            return 9;
        case "a":
            return 10;
        case "b":
            return 11;
        case "c":
            return 12;
        case "d":
            return 13;
        case "e":
            return 14;
    }

    return -1;
}

function                            returnToGameList        ()
{
    gf.navigateToGame();
}

// gameforest methods

// method to override when the game has started
GameForest.prototype.onGameStart    = function ()
{
    $("#gamePickTable").hide();
    $("#gameWaitPanel").show();

    player1 = gf.userList()[0];
    player2 = gf.userList()[1];

    var un1 = $("#usernameOne");
        un1.html('<b>Player 1: </b>' + player1.Username);

    var un2 = $("#usernameTwo");
        un2.html('<b>Player 2: </b>' + player2.Username);

    if (gf.playerOrderIndex() == 1)
    {
        $("#usernameWait").text(player2.Username);
    }
    else
    {
        $("#usernameWait").text(player1.Username);
    }
};

// method to override when its the player's turn
GameForest.prototype.onTurnStart    = function ()
{
    alert("Your turn!");

    if (gameState == GAME_STATE_CHOOSE_PIECE)
    {
        $("#gamePickTable").show();
        $("#gameWaitPanel").hide();
    }
    else
    {
        allowInteraction = true;
    }
};

// method to override when its the other player's turn
GameForest.prototype.onTurnChange   = function ()
{

};

// method to override when the game's data has changed
GameForest.prototype.onUpdateData   = function (key, data)
{
    // gameData = data;

    if (gameData.player1PiecesOk && gameData.player2PiecesOk)
    {
        gameState = GAME_STATE_PLAYING_GAME;

        $("#gamePickTable").hide();
        $("#gameWaitPanel").hide();

        $("#gameGameTable").show();

        updateBoard();

        if (gameData.conflictData.status)
        {
            gameData.conflictData.status = false;

            switch (gameData.conflictData.winner)
            {
                case 0:
                    alert("Draw! Both of your pieces are removed.");
                    break;
                case 1:

                    if (gf.playerOrderIndex() == 1)
                        alert("Your piece won the match!");
                    else
                        alert(player1.Username + "'s piece won the match!");

                    break;
                case 2:

                    if (gf.playerOrderIndex() == 2)
                        alert("Your piece won the match!");
                    else
                        alert(player2.Username + "'s piece won the match!");

                    break;
                case 3:
                    break;
            }
        }
    }
};

// method to override when the game is finished
GameForest.prototype.onGameFinish   = function (tallyList)
{
    $("#gameDonePanel").show();
    $("#gameGameTable").hide();

    $("#winningPlayer").text(tallyList == 1 ? player1.Username : player2.Username);
};
