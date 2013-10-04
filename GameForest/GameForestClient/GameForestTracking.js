/// <reference path="GameForest.js" />

var GameForestTracker   = function(gameId)
{
    // checker to see if gameforest can initialize
    if (!Guid.isGuid(gameId))
    {
        console.error("Parameters required to start GameForest is either missing or in an invalid state.");

        if (GameForestVerboseMessaging)
        {
            alert("Parameters required to start GameForest is either missing or in an invalid state.");
        }

        return;
    }

    // helper method for sending request
    function sendRequest(url, type, onSuccess, onError)
    {
        var httpURL = "http://" + cloudURL + ":" + cloudPRT + "/service" + url;

        console.log("Doing " + type + " request on URL " + httpURL);

        var response = $.ajax({
            url: httpURL,
            async: true,
            type: type,
            cache: false
        });

        response.success(onSuccess);
        response.fail(function (a, b, c)
        {
            onError(b, c);
        });
    }

    // function to add a NUMERICAL stat to track
    // IMPORTANT: THIS ONLY WORKS WITH STATS TRACKING INT VALUES (not float nor time)
    this.trackStat = function (statName)
    {
        var p = null;

        sendRequest("/game/stat?addStat=" + statName + "&gameid=" + gameId, "POST",
            function (result)
            {
                if (result.ResponseType == 0)
                {
                    p = JSON.parse(result.AdditionalData);
                    return p;
                }
                else
                {
                    var p = "Err: " + JSON.parse(result.AdditionalData);
                    console.log("returned: " + p);
                    return p;
                }
            },
            function (status, why)
            {
                alert("Error in trackStat function: [status=" + status + "] [reason=" + why + "]");
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

    this.cbStat = function(statVals, statNames, data)
    {
        console.log(data); // extract stat object
        var statName = data.stat_name;
        statNames.push(statName); // push stat's name to list of stat names
        var statVal = data.stat_value;
        statVals.push(statVal); // push the stat's value to list of values, same index

        // How to match names to value below, DEBUG
        for (var i = 0; i < statNames.length; i++)
            console.log(statNames[i] + ": " + statVals[i]);
    }

    this.getStatVal = function(statName, statValList, statNameList, callback)
    {
        var p = {};
        var statOut = null;
        
        // DO NOT CHANGE false 
        sendRequest("/game/stats?getstat=" + statName + "&gameid=" + gameId + "&all=false", "GET",
            function (result)
            {
                if (result.ResponseType == 0)
                {
                    p = JSON.parse(result.AdditionalData);
                    console.log("name... " + p.stat_name);
                    console.log("value... " + p.stat_value);

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
            function (status, why)
            {
                alert("Error in getStat function: [status=" + status + "] [reason=" + why + "]");
            });
    }

    // function to update a tracked stat
    // IMPORTANT: THIS ONLY WORKS WITH statValue BEING AN INT
    this.updateStat = function(statName, statValue)
    {
        sendRequest("/game/stats?updatestat=" + statName + "&gameid=" + gameId + "&statvalue=" + statValue, "POST",
            function (result)
            {
                if (result.ResponseType == 0)
                {
                    var p = JSON.parse(result.AdditionalData);
                    return p;
                }
                else
                {
                    var p = result.ResponseType + ": " + result.AdditionalData;
                }
            },
            function (status, why)
            {
                alert("Error in updateStat function: [status=" + status + "] [reason=" + why + "]");
            });
    }

}