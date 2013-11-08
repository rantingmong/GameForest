/// <reference path="index.html" />

/// <reference path="guid.js" />
/// <reference path="jquery.js" />
/// <reference path="promise.js" />

/// <reference path="GameForest.js" />

'use strict';

// gameforest-specific data

// Set this to localhost when debugging and to game-forest.cloudapp.net when submitting it to Game Forest.
GameForestCloudUrl          = "10.2.180.26";

// Set this to true when debugging to let Game Forest show alert messages when something went wrong.
GameForestVerboseMessaging  = false;

// game specific data

var GAME_STATE_CHOOSE_PIECE = 0;
var GAME_STATE_PLAYING_GAME = 1;

var gameState               = GAME_STATE_CHOOSE_PIECE;

var allowInteraction        = false;

var player1                 = null;
var player2                 = null;

var cellSource              = null;

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
    "general5star.png",
    "general4star.png",
    "general3star.png",
    "general2star.png",
    "general1star.png",
    "colonel.png",
    "ltcolonel.png",
    "major.png",
    "captain.png",
    "lieu1st.png",
    "lieu2nd.png",
    "sergeant.png",
    "private.png",
    "spy.png",
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

// gameforest callback methods

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

// pick element events

function                            pickDragStart           (e)
{
    if (e.target.nodeName == 'TD')
    {
        cellSource = e.target;
    }
    else
    {
        e.preventDefault();
    }
}

function                            pickDragDrop            (e)
{
    e.preventDefault();

    var des = document.getElementById(e.target.id);
    var src = document.getElementById(cellSource.id);
    
    var prv = des.innerHTML;
    var dpv = des.dataset.info;

    des.innerHTML       = src.innerHTML;
    des.dataset.info    = src.dataset.info;

    src.innerHTML       = prv;
    src.dataset.info    = dpv;

    cellSource = null;
}

function                            pickDragOver            (e)
{
    e.preventDefault();
}

// confirm button event

function                            buttonConfirmPlaceClick (e)
{
    var allPiecesPlaced = true;
    var shouldBreak     = false;

    for (var y = 1; y < 4; y++)
    {
        for (var x = 1; x < 8; x++)
        {
            if (document.getElementById("pickcellsrc_" + y + "_" + x).dataset.info != 0)
            {
                allPiecesPlaced = false;
                shouldBreak     = true;

                break;
            }
        }

        if (shouldBreak)
            break;
    }

    if (allPiecesPlaced == false)
    {
        alert("Please place all pieces on the board before you hit this button again.");
        return;
    }

    var sI = 0;
    var eI = 0;

    var ng = false;
    var pI = 0;

    if      (gf.playerOrderIndex() == 1)
    {
        sI = 2;
        eI = 0;

        ng = true;
    }
    else if (gf.playerOrderIndex() == 2)
    {
        sI = 5;
        eI = 7;

        ng = false;
    }

    for (var i = sI; ng ? (eI <= i) : (eI >= i); ng ? i-- : i++)
    {
        pI++;

        for (var x = 0; x < 9; x++)
        {
            var info = document.getElementById("pickcelldes_" + pI + "_" + (x + 1)).dataset.info;

            if (info == 0)
                continue;

            gameData.boardData[i][x] = info + "_" + gf.playerOrderIndex();
        }
    }

    if      (gf.playerOrderIndex() == 1)
    {
        gameData.player1PiecesOk = true;
    }
    else if (gf.playerOrderIndex() == 2)
    {
        gameData.player2PiecesOk = true;
    }

    $("#pickPanel").hide();
    $("#waitPanel").show();

    alert("NEXT TURN!");

    gf.nextTurn(gameData);
}

// main board element events

function                            mainDragStart           ()
{
    if (allowInteraction == false)
    {
        event.preventDefault();
        return;
    }

    // highlight where the player can move the piece
    var tId = event.target.id.split("_");

    var y = parseInt(tId[1]);
    var x = parseInt(tId[2]);
    
    var elem = event.target;

    console.log("Element to be dragged is: " + event.target.id);

    if (parseInt(elem.dataset.player) != gf.playerOrderIndex())
    {
        cellSource = null;
        event.preventDefault();
        
        console.log("Cannot drag this element. This is not yours.");

        return;
    }

    console.log("Source is: " + event.target.id);

    cellSource = event.target.id;

    // set allowable drop locations

    var t = "maincell_" + (y - 1) + "_" + x;
    var b = "maincell_" + (y + 1) + "_" + x;
    var l = "maincell_" + y + "_" + (x - 1);
    var r = "maincell_" + y + "_" + (x + 1);

    updateTilesToDrop(t);
    updateTilesToDrop(b);
    updateTilesToDrop(l);
    updateTilesToDrop(r);
}

function                            mainDragDrop            ()
{
    // check if the cell to be dropped has a piece and do conflict resolution, else place the piece as is
    if (cellSource != null)
    {
        var elem        = event.target;

        var splitSrc    = cellSource.split("_");

        var sy          = parseInt(splitSrc[1]);
        var sx          = parseInt(splitSrc[2]);

        var t           = "maincell_" + (sy - 1) + "_" + sx;
        var b           = "maincell_" + (sy + 1) + "_" + sx;
        var l           = "maincell_" + sy + "_" + (sx - 1);
        var r           = "maincell_" + sy + "_" + (sx + 1);

        if      (parseInt(elem.dataset.player) == 0 && (elem.id === t || elem.id === b || elem.id === l || elem.id === r))
        {
            console.log("Normal as possible...");

            event.preventDefault();

            var src = document.getElementById(cellSource);
            var des = document.getElementById(elem.id);

            var spt = des.id.split("_");

            var dy   = parseInt(spt[1]);
            var dx   = parseInt(spt[2]);

            gameData.boardData[sy - 1][sx - 1] = "";
            gameData.boardData[dy - 1][dx - 1] = src.dataset.info + "_" + src.dataset.player;

            allowInteraction = false;
            gf.nextTurn(gameData);
        }
        else if (parseInt(elem.dataset.player) == gf.playerOrderIndex())
        {
            console.log("You cannot replace your pieces.");
        }
        else if (parseInt(elem.dataset.player) != gf.playerOrderIndex())
        {
            console.log("CONFLICT!! :O");

            event.preventDefault();

            var src         = document.getElementById(cellSource);
            var des         = document.getElementById(elem.id);

            var srcIdSplit  = src.id.split("_");
            var desIdSplit  = des.id.split("_");

            var sy          = parseInt(srcIdSplit[1]) - 1,
                sx          = parseInt(srcIdSplit[2]) - 1,
                dy          = parseInt(desIdSplit[1]) - 1,
                dx          = parseInt(desIdSplit[2]) - 1;

            // source is always player(current) and destination is always player(current + 1)
            if      (gf.playerOrderIndex() == 1)
            {
                gameData.conflictData.player1Piece = hexToInt(src.dataset.info);
                gameData.conflictData.player2Piece = hexToInt(des.dataset.info);
            }
            else if (gf.playerOrderIndex() == 2)
            {
                gameData.conflictData.player2Piece = hexToInt(src.dataset.info);
                gameData.conflictData.player1Piece = hexToInt(des.dataset.info);
            }

            // dataset.info is 1 based, we have to decrease it for the win loss matrix
            var fp1p = gameData.conflictData.player1Piece - 1;
            var fp2p = gameData.conflictData.player2Piece - 1;

            gameData.conflictData.winner = winLossMatrix[fp1p][fp2p];
            gameData.conflictData.status = true;

            // update board data
            switch (gameData.conflictData.winner)
            {
                case 0:

                    gameData.boardData[sy][sx] = "";
                    gameData.boardData[dy][dx] = "";

                    break;
                case 1:

                    if (gf.playerOrderIndex() == 1)
                    {
                        gameData.boardData[dy][dx] = gameData.boardData[sy][sx];
                        gameData.boardData[sy][sx] = "";
                    }
                    else
                    {
                        gameData.boardData[sy][sx] = gameData.boardData[dy][dx];
                        gameData.boardData[dy][dx] = "";
                    }

                    break;
                case 2:

                    if (gf.playerOrderIndex() == 2)
                    {
                        gameData.boardData[dy][dx] = gameData.boardData[sy][sx];
                        gameData.boardData[sy][sx] = "";
                    }
                    else
                    {
                        gameData.boardData[sy][sx] = gameData.boardData[dy][dx];
                        gameData.boardData[dy][dx] = "";
                    }

                    break;
                case 3:

                    // we also have to fill in information which player won the flag case
                    gameData.conflictData.winnerFlag = gf.playerOrderIndex();

                    gameData.boardData[dy][dx] = gameData.boardData[sy][sx];
                    gameData.boardData[sy][sx] = "";

                    break;
            }

            allowInteraction = false;
            gf.nextTurn(gameData);
        }

        refreshBoard();
        cellSource = null;
    }
}

function                            mainDragOver            ()
{
    event.preventDefault();
}

// util functions

function                            updateTilesToDrop       (id)
{
    var elem = document.getElementById(id);

    if (elem != null)
    {
        var thisPlayer = gf.playerOrderIndex();

        var dataPPiece = elem.dataset.info;
        var dataPlayer = elem.dataset.player;

        if (dataPPiece != 0)
        {
            if (dataPlayer != thisPlayer)
            {
                elem.style.backgroundColor = "orange";
            }
            else
            {
                elem.style.backgroundColor = "red";
            }
        }
        else
        {
            elem.style.backgroundColor = "green";
        }
    }
}

function                            refreshBoard            ()
{
    for (var y = 0; y < 8; y++)
    {
        for (var x = 0; x < 9; x++)
        {
            var splitTheInfo = gameData.boardData[y][x];

            if (splitTheInfo == "")
            {
                var elem = document.getElementById("maincell_" + (y + 1) + "_" + (x + 1));

                elem.style.backgroundColor  = "white";
                    
                elem.innerHTML              = "";
                    
                elem.dataset.info           = "0";
                elem.dataset.player         = "0";

                continue;
            }

            var info    = splitTheInfo[0];
            var plyr    = splitTheInfo[2];

            var id      = "maincell_" + (y + 1) + "_" + (x + 1);
            var text    = "";

            switch(info)
            {
                case "1": text = "5 star general";
                    break;
                case "2": text = "4 star general";
                    break;
                case "3": text = "Lieutentant general";
                    break;
                case "4": text = "Major general";
                    break;
                case "5": text = "Brigadier general";
                    break;
                case "6": text = "Colonel";
                    break;
                case "7": text = "Lieutentant colonel";
                    break;
                case "8": text = "Major";
                    break;
                case "9": text = "Captain";
                    break;
                case "a": text = "1st lieutentant";
                    break;
                case "b": text = "2nd lieutentant";
                    break;
                case "c": text = "Sergeant";
                    break;
                case "d": text = "Private";
                    break;
                case "e": text = "Spy";
                    break;
                case "f": text = "Flag";
                    break;
            }

            var elem    = document.getElementById(id);

            if      (plyr == "1")
            {
                elem.style.backgroundColor = "#f59393";

                if (gf.playerOrderIndex() == 2)
                {
                    text = "";
                }
            }
            else if (plyr == "2")
            {
                elem.style.backgroundColor = "#4b87f3";

                if (gf.playerOrderIndex() == 1)
                {
                    text = "";
                }
            }

            elem.innerHTML      = text;
                
            elem.dataset.info   = info;
            elem.dataset.player = plyr;
        }
    }
}

function                            checkForConflict        ()
{
    if (gameData.conflictData.status)
    {
        // update board data
        switch (gameData.conflictData.winner)
        {
            case 0:
                alert("Draw! You both have your pieces killed.");
                break;
            case 1:
                if (gf.playerOrderIndex() == 1)
                {
                    alert("You won the battle!");
                }
                else
                {
                    alert(player1.Username + " wins the battle! The piece was " + intToString(gameData.conflictData.player1Piece));
                }
                break;
            case 2:
                if (gf.playerOrderIndex() == 2)
                {
                    alert("You won the battle!");
                }
                else
                {
                    alert(player2.Username + " wins the battle! The piece was " + intToString(gameData.conflictData.player2Piece));
                }
                break;
            case 3:
                alert("Special case for flags! In this case " + gameData.conflictData.winnerFlag == 1 ? player1.Username : player2.Username + " wins the game!");
                break;
        }

        gameData.conflictData.status = false;
    }
}

function                            hexToInt                (input)
{
    if (parseInt(input) <= 9)
    {
        return parseInt(input);
    }
    else
    {
        switch (input)
        {
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
            case "f":
                return 15;
            default:
                return -1;
        }
    }
}

function                            intToString             (input)
{
    switch(input)
    {
        case 1:
            return "5 star general";
        case 2:
            return "4 star general";
        case 3:
            return "Lieutentant general";
        case 4:
            return "Major general";
        case 5:
            return "Brigadier general";
        case 6:
            return "Colonel";
        case 7:
            return "Lieutentant colonel";
        case 8:
            return "Major";
        case 9:
            return "Captain";
        case 10:
            return "1st lieutentant";
        case 11:
            return "2nd lieutentant";
        case 12:
            return "Sergeant";
        case 13:
            return "Private";
        case 14:
            return "Spy";
        case 15:
            return "Flag";
    }
}

// game forest methods

// method to override when the game has started
GameForest.prototype.onGameStart    = function ()
{
    player1 = gf.playerAtIndex(0);
    player2 = gf.playerAtIndex(1);

    if      (gf.playerOrderIndex() == 1)
    {
        $("#waitPanelUsername").html(player2.Username);
    }
    else if (gf.playerOrderIndex() == 2)
    {
        $("#waitPanelUsername").html(player1.Username);
    }

    $("#username1").html("<span id='username1pic' class='glyphicon glyphicon-play' style='display: none'></span> " + player1.Username);
    $("#username2").html("<span id='username2pic' class='glyphicon glyphicon-play' style='display: none'></span> " + player2.Username);
};

// method to override when its the player's turn
GameForest.prototype.onTurnStart    = function ()
{
    if      (gameState == GAME_STATE_CHOOSE_PIECE)
    {
        alert("YAY! SHOW IT!!");

        $("#pickPanel").show();
        $("#waitPanel").hide();
    }
    else if (gameState == GAME_STATE_PLAYING_GAME)
    {
        alert("YAY! GAME NA!!");

        allowInteraction = true;
    }
};

// method to override when its the other player's turn
GameForest.prototype.onTurnChange   = function ()
{
    $("#username1pic").hide();
    $("#username2pic").hide();

    if (gf.currentPlayer() == player1)
    {
        $("#username1pic").show();
    }
    else
    {
        $("#username2pic").show();
    }
};

// method to override when the game's data has changed
GameForest.prototype.onUpdateData   = function (key, data)
{
    gameData = data;

    if (gameData.player1PiecesOk && gameData.player2PiecesOk && gameState == GAME_STATE_CHOOSE_PIECE)
    {
        gameState = GAME_STATE_PLAYING_GAME;

        $("#waitPanel").hide();
        $("#mainPanel").show();
    }

    // update game board and check conflict data
    if (gameState == GAME_STATE_PLAYING_GAME)
    {
        // STEP 1: update game board
        refreshBoard();

        // STEP 2: check for conflict data
        checkForConflict();
    }
};

// method to override when the game is finished
GameForest.prototype.onGameFinish   = function (tallyList)
{

};
