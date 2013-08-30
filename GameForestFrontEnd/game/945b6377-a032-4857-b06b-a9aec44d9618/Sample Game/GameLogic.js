/// <reference path="GameForest.js" />
/// <reference path="guid.js" />
/// <reference path="jquery.js" />
/// <reference path="promise.js" />
/// <reference path="index.html" />

'use strict';

GameForestCloudUrl          = "localhost";
GameForestVerboseMessaging  = true;

GameForest.prototype.onGameStart    = function ()
{
    console.log("OnGameStart is invoked!");
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
