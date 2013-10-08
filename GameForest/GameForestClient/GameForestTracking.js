/// <reference path="GameForest.js" />

var GameForestTracker   = function(gameId, sessionId)
{
    // checker to see if gameforest can initialize
    if (!Guid.isGuid(gameId) 	||
		!Guid.isGuid(sessionId))
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

        sendRequest("stats/stat?addStat=" + statName + "&gameid=" + gameId, "POST",
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
	
	this.trackUserStat = function(statName)
	{
		var p = null;
		var x = null;
		var id = null;
		
		sendRequest("/user/session" + this.sessionId, "GET",
			function(result)
			{
				if(result.ResponseType == 0)
				{
					p = JSON.parse(result.AdditionalData);
					id = p.UserId;
					console.log(id);
					
					sendRequest("/stats/stat?adduserstat=" + statName + "&gameid=" this.gameId + "&userid=" id, "POST",
						function(result)
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
						function(status,why)
						{
							alert("Error in trackStat function: [status=" + status + "] [reason=" + why + "]");
						});
				}
				else
				{
					p = "Err: " + JSON.parse(result.AdditionalData);
                    console.log("returned: " + p); 
				}
			},
			function(status, why)
			{
				alert("Error in trackUserStat function: [status=" + status + "] [reason=" + why + "]");
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
    ******/

    this.getStatVal = function(statName, statValList, statNameList)
    {
        var p = {};
        var statOut = null;
        
        // DO NOT CHANGE false 
        sendRequest("/stats/stats?getstat=" + statName + "&gameid=" + gameId + "&all=false", "GET",
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
                alert("Error in getStat function: [status=" + status + "] [reason=" + why + "]");
            });
    }

    // function to update a tracked stat
    // IMPORTANT: THIS ONLY WORKS WITH statValue BEING AN INT
    this.updateStat = function(statName, statValue)
    {
        sendRequest("/stats/stats?updatestat=" + statName + "&gameid=" + gameId + "&statvalue=" + statValue, "POST",
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
	
	this.updateUserStat = function(statName, statValue)
	{
		var p = null;
		var x = null;
		var id = null;
		
		sendRequest("/user/session" + this.sessionId, "GET",
			function(result)
			{
				if(result.ResponseType == 0)
				{
					p = JSON.parse(result.AdditionalData);
					id = p.UserId;
					console.log(id);
					
					sendRequest("/stats/stats?updateuserstat=" + statName + "&gameid=" this.gameId + "&userid=" id + 
								"&statvalue=" + statValue, "POST",
						function(result)
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
						function(status,why)
						{
							alert("Error in trackStat function: [status=" + status + "] [reason=" + why + "]");
						});
				}
				else
				{
					p = "Err: " + JSON.parse(result.AdditionalData);
                    console.log("returned: " + p); 
				}
			},
			function(status, why)
			{
				alert("Error in updateUserStat function: [status=" + status + "] [reason=" + why + "]");
			});
	}
}