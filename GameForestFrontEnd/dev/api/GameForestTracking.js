/// <reference path="GameForest.js" />
/// <reference path="guid.js" />
/// <reference path="jquery.js" />
/// <reference path="index.html" />

var GameForestTracker = function (gameId, sessionId)
{
    // checker to see if gameforest can initialize
    if (!Guid.isGuid(gameId) ||
        !Guid.isGuid(sessionId))
    {
        console.error("Parameters required to start GameForest is either missing or in an invalid state.");

        if (GameForestVerboseMessaging)
        {
            alert("Parameters required to start GameForest is either missing or in an invalid state.");
        }

        return;
    }

    // ------------------------------------------------

    var cloudURL = GameForestCloudURL;
    var cloudPRT = 1193;

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

    // ------------------------------------

    // function to add a NUMERICAL stat to track for a DEVELOPER
    // Best time to call this is at the very start of the game.
    // IMPORTANT: THIS ONLY WORKS WITH STATS TRACKING INT VALUES (not float nor time)
    this.trackStat = function (statName)
    {
        var p = null;

        sendRequest("/stats/stats?addStat=" + statName + "&gameid=" + gameId, "POST",
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

    // function to add a NUMERICAL stat to track for a given user
    // Best time to call this command is when users initially enter a game
    // Since it uses the user's session id. 
    this.trackUserStat = function (statName)
    {
        var p = null;

        sendRequest("/stats/users?addstat=" + statName + "&gameid=" + gameId + "&session=" + sessionId, "POST",
            function (result)
            {
                if (result.ResponseType == 0)
                {
                    p = JSON.parse(result.AdditionalData);
                    return p;
                }
                else
                {
                    p = "Err: " + JSON.parse(result.AdditionalData);
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
        )
    
    statValList and statNameList are designed this way
    because the calls to fetch statistics are asynchronous 
    and thus you won't always get the stats in the same
    order. 
    
    This will be fixed ASAP
    ******/

    this.getStatVal = function (statName, statVals, statNames)
    {
        var p = {};
        var appendStat = {};

        // DO NOT CHANGE false 
        sendRequest("/stats/?getstat=" + statName + "&gameid=" + gameId + "&all=false", "GET",
            function (result)
            {
                if (result.ResponseType == 0)
                {
                    p = JSON.parse(result.AdditionalData);
                    console.log("name... " + p.stat_name);
                    console.log("value... " + p.stat_value);

                    // callback 
                    console.log(p); // extract stat object
                    var statName = p.stat_name;
                    statNames.push(statName); // push stat's name to list of stat names
                    var statVal = p.stat_value;
                    statVals.push(statVal); // push the stat's value to list of values, same index

                    // How to match names to value below, DEBUG
                    for (var i = 0; i < statNames.length; i++)
                        console.log(statNames[i] + ": " + statVals[i]);

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
                alert("Error in getStatVal function: [status=" + status + "] [reason=" + why + "]");
            });
    }

    /****************
    getStatValR -
    statName = name of stat to fetch
    statObj = statistic container passed as  { stat_name: "name of stat" }
    
    Needs to be tested
    ****************/

    this.getStatValR = function (statName, statObj)
    {
        var p = {};

        // DO NOT CHANGE false 
        sendRequest("/stats/?getstat=" + statName + "&gameid=" + gameId + "&all=false", "GET",
            function (result)
            {
                if (result.ResponseType == 0)
                {
                    p = JSON.parse(result.AdditionalData);
                    console.log("name... " + p.stat_name);
                    console.log("value... " + p.stat_value);

                    // callback 
                    console.log(p); // extract stat object

                    statObj.stat_value = p.stat_value;

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
                alert("Error in getStatVal function: [status=" + status + "] [reason=" + why + "]");
            });
    }

    /****************
    getUserStatVal - parameters - 
    statName = name of the stat (string)
    valueObj = value container passed as { stat_value: 0 }
    ****************/
    this.getUserStatVal = function (statName, valueObj)
    {
        var p = {};
        var val = valueObj;

        // DO NOT CHANGE false
        sendRequest("/stats/user?getstat=" + statName + "&gameid=" + gameId + "&session=" + sessionId +
                    "&all=false", "GET",
                    function (result)
                    {
                        if (result.ResponseType == 0)
                        {
                            p = JSON.parse(result.AdditionalData);
                            console.log(p);
                            console.log("session..." + sessionId);
                            console.log("name... " + p.stat_name);
                            console.log("value... " + p.stat_value);

                            val.stat_value = p.stat_value;
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
                        alert("Error in getUserStatVal function: [status=" + status + "] [reason=" + why + "]");
                    });
    }

    // function to update a tracked stat
    // IMPORTANT: THIS ONLY WORKS WITH statValue BEING AN INT
    this.updateStat = function (statName, statValue)
    {
        sendRequest("/stats/?updatestat=" + statName + "&gameid=" + gameId + "&statvalue=" + statValue, "POST",
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

    // function to update a tracked stat for a particular user. 
    // username is used for consistency with the database
    // username can be retrieved via getUserInfo from gameforest API (.Username property)
    // if using sessionid, the wrong user's stats may be updated
    // IMPORTANT: THIS ONLY WORKS WITH statValue BEING AN INT
    this.updateUserStat = function (statName, statValue, username)
    {
        sendRequest("/stats/user?updatestat=" + statName + "&gameid=" + gameId + "&user=" + username +
                    "&statvalue=" + statValue, "POST",
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
                        alert("Error in updateUserStat function: [status=" + status + "] [reason=" + why + "]");
                    });
    }
}