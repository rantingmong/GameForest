using System;
using System.Collections.Generic;
using System.Diagnostics;
using Fleck;
using Newtonsoft.Json;
using GameForestCore.Database;
using GameForestCoreWebSocket.Messages;
using GameForestDatabaseConnector.Logger;
using System.Threading;

namespace GameForestCoreWebSocket
{
    public class WebSocketEntry
    {
        public int                      checkCount;
        public bool                     disconnected;

        public bool                     pingSent;
        public DateTime                 lastPing;

        public IWebSocketConnection     webSocket;

        public WebSocketEntry()
        {
            checkCount      = 0;
            disconnected    = false;
            pingSent        = false;
            lastPing        = DateTime.Now;
            webSocket       = null;
        }
    }

    public class GFXServerCore
    {
        public static readonly string                   INIT_CONNECTION     = "GFX_INIT_CONNECTION";
        public static readonly string                   STOP_CONNECTION     = "GFX_STOP_CONNECTION";

        private int                                     modChecker          = 0;

        private double                                  pingThreshold       = 10D;
        private double                                  disconnectThreshold = 30D;
        private double                                  logoutThreshold     = 300D;

        private Timer                                   loginCheckTimer     = null; // timer for users logged in on game-forest
        private Timer                                   sessionCheckTimer   = null; // timer for users playing
        private Timer                                   pingTimer           = null; // timer for checking if the player is still alive
        
        private WebSocketServer                         server;

        private List<Guid>                              verifyList          = new List<Guid>();

        private Dictionary<Guid, Guid>                  connectionList      = new Dictionary<Guid, Guid>();  // key is session id
        private Dictionary<Guid, WebSocketEntry>        webSocketList       = new Dictionary<Guid, WebSocketEntry>();

        private Dictionary<Guid, GFXGameData>           gameDatalist        = new Dictionary<Guid, GFXGameData>();

        private List<GFXSocketListener>                 listenerList        = new List<GFXSocketListener>();

        private List<GFXDisconnectInfo>                 disconnectedList    = new List<GFXDisconnectInfo>();

        private GFXDatabaseTable<GFXUserRow>            userList            = new GFXDatabaseTable<GFXUserRow>(new GFXUserRowTranslator());
        private GFXDatabaseTable<GFXLoginRow>           sessionList         = new GFXDatabaseTable<GFXLoginRow>(new GFXLoginRowTranslator());

        private GFXDatabaseTable<GFXLobbyRow>           lobbyList           = new GFXDatabaseTable<GFXLobbyRow>(new GFXLobbyRowTranslator());
        private GFXDatabaseTable<GFXLobbySessionRow>    lobbySessionList    = new GFXDatabaseTable<GFXLobbySessionRow>(new GFXLobbySessionRowTranslator());

        public  Dictionary<Guid, GFXGameData>           GameDataList
        {
            get { return gameDatalist; }
        }

        public  Dictionary<Guid, Guid>                  ConnectionList
        {
            get { return connectionList; }
        }

        public  Dictionary<Guid, WebSocketEntry>        WebSocketList
        {
            get { return webSocketList; }
        }

        public  GFXDatabaseTable<GFXLobbyRow>           LobbyList
        {
            get { return lobbyList; }
        }

        public  GFXDatabaseTable<GFXLobbySessionRow>    LobbySessionList
        {
            get { return lobbySessionList; }
        }

        public GFXDatabaseTable<GFXUserRow>             UserList
        {
            get { return userList; }
        }

        public GFXDatabaseTable<GFXLoginRow>            LoginList
        {
            get { return sessionList; }
        }

        public  GFXServerCore                           (string ipAddress = "localhost")
        {
            listenerList.Add(new GFXInformReconnect());
            listenerList.Add(new GFXPingRespond());
            listenerList.Add(new GFXConfirmTurn());
            listenerList.Add(new GFXGameAskUserData());
            listenerList.Add(new GFXGameAskData());
            listenerList.Add(new GFXGameFinish());
            listenerList.Add(new GFXGameNextTurn());
            listenerList.Add(new GFXGameSendData());
            listenerList.Add(new GFXGameSendUserData());
            listenerList.Add(new GFXGameStart());
            listenerList.Add(new GFXGameStartConfirm());

            pingTimer           = new Timer(new TimerCallback((o) =>
                {
                    SendPing();
                }));

            loginCheckTimer     = new Timer(new TimerCallback((o) =>
                {
                    CheckUsernameTick();

                }));

            sessionCheckTimer   = new Timer(new TimerCallback((o) =>
                {
                    CheckUserConnectedTick();
                }));

            pingTimer           .Change(TimeSpan.FromSeconds(pingThreshold),        TimeSpan.FromSeconds(pingThreshold));
            loginCheckTimer     .Change(TimeSpan.FromSeconds(logoutThreshold),      TimeSpan.FromSeconds(logoutThreshold));
            sessionCheckTimer   .Change(TimeSpan.FromSeconds(disconnectThreshold),  TimeSpan.FromSeconds(disconnectThreshold));

            server = new WebSocketServer("ws://" + ipAddress + ":8084");

            server.Start(socket =>
                {
                    socket.OnOpen       = () =>
                        {
                            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "WebSocket", "A client wants to connect!");

                            // a client wants to connect! give it a new connection id
                            Guid guid = Guid.NewGuid();

                            verifyList.Add(guid);
                            socket.Send(JsonConvert.SerializeObject(new GFXSocketResponse
                                {
                                    Message         = guid.ToString(),
                                    ResponseCode    = GFXResponseType.Normal,
                                    Subject         = INIT_CONNECTION
                                }));

                            Debug.WriteLine("Connection opened with " + socket.ConnectionInfo.ClientIpAddress);
                        };
                    socket.OnClose      = () =>
                        {
                            Debug.WriteLine("Connection closed with " + socket.ConnectionInfo.ClientIpAddress);

                            lock (webSocketList)
                            {
                                var key     = Guid.Empty;
                                var entry   = new WebSocketEntry();

                                foreach(var kvp in webSocketList)
                                {
                                    if (kvp.Value.webSocket == socket)
                                    {
                                        key     = kvp.Key;
                                        entry   = kvp.Value;
                                    }
                                }

                                if (key != Guid.Empty)
                                {
                                    webSocketList   .Remove(key);
                                    connectionList  .Remove(key);

                                    // remove the user from session database.
                                    try
                                    {
                                        List<GFXLobbySessionRow> result = new List<GFXLobbySessionRow>(lobbySessionList.Select(string.Format("SessionId = '{0}'", key)));
                                        lobbySessionList.Remove(string.Format("SessionId = '{0}'", key));

                                        if (lobbySessionList.Count(string.Format("LobbyId = '{0}'", result[0].LobbyID)) == 0)
                                        {
                                            lobbyList.Remove(string.Format("LobbyId = '{0}'", result[0].LobbyID));
                                        }

                                        GFXLoginRow loginRow            = new List<GFXLoginRow>(sessionList.Select(string.Format("SessionId = '{0}'", key), 1))[0];
                                                    loginRow.UserStatus = GFXLoginStatus.MENU;

                                        sessionList.Update(string.Format("SessionId = '{0}'", key), loginRow);

                                    }
                                    catch (Exception exp)
                                    {
                                        GFXLogger.GetInstance().Log(GFXLoggerLevel.FATAL, "Force Remove User", exp.Message);
                                    }
                                }
                            }
                        };

                    socket.OnMessage    = message =>
                        {
                            Debug.WriteLine("Message received: " + message);

                            // parse message
                            GFXSocketInfo info = JsonConvert.DeserializeObject<GFXSocketInfo>(message);

                            if (info.Subject == INIT_CONNECTION)
                            {
                                if (verifyList.Contains(info.ConnectionId))
                                {
                                    lock(webSocketList)
                                    {
                                        // connection id verified! we can add the session id to the connection list
                                        connectionList  [info.SessionId] = info.ConnectionId;
                                        webSocketList   [info.SessionId] = new WebSocketEntry
                                            {
                                                webSocket   = socket,
                                                lastPing    = DateTime.Now,
                                                pingSent    = false
                                            };
                                    
                                        // and remove the key from verify list (for safety purposes :) )
                                        verifyList.Remove(info.ConnectionId);
                                    }
                                }
                                else
                                {
                                    socket.Send(JsonConvert.SerializeObject(new GFXSocketResponse
                                        {
                                            Message         = INIT_CONNECTION,
                                            Subject         = "Connection ID is not in the verification list! Denying connection.",
                                            ResponseCode    = GFXResponseType.DoesNotExist
                                        }));
                                }
                            }
                            else if (info.Subject == STOP_CONNECTION)
                            {
                                lock(webSocketList)
                                {
                                    webSocketList.Remove(info.SessionId);
                                    connectionList.Remove(info.SessionId);
                                }
                            }
                            else
                            {
                                if (!connectionList.ContainsKey(info.SessionId))
                                {
                                    socket.Send(JsonConvert.SerializeObject(new GFXSocketResponse
                                        {
                                            Message         = INIT_CONNECTION,
                                            Subject         = "Unknown client trying to message the server! Denying request.",
                                            ResponseCode    = GFXResponseType.DoesNotExist
                                        }));

                                    return;
                                }

                                bool processed = false;

                                foreach (var listener in listenerList)
                                {
                                    if (info.Subject == listener.Subject)
                                    {
                                        GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, info.Subject, "Message received: " + info.Message);

                                        processed = true;

                                        socket.Send(JsonConvert.SerializeObject(listener.DoMessage(this, info, socket)));
                                        break;
                                    }
                                }

                                if (processed == false)
                                {
                                    socket.Send(JsonConvert.SerializeObject(new GFXSocketResponse
                                        {
                                            Subject         = info.Subject,
                                            Message         = "Message is not implemented on this server.",
                                            ResponseCode    = GFXResponseType.DoesNotExist
                                        }));
                                }
                            }
                        };
                });
        }

        public  void                                    Stop                    ()
        {
            server.Dispose();

            pingTimer.Dispose();
            loginCheckTimer.Dispose();
            sessionCheckTimer.Dispose();
        }

        private void                                    SendPing                ()
        {
            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "Ping", "Sending ping...");

            lock(webSocketList)
            // after checking for disconnected users, we refresh their pings
                foreach (var item in webSocketList)
                {
                    var websocketEntry = item.Value;

                    if (websocketEntry.pingSent == false)
                    {
                        // we don't want to spam the client
                        websocketEntry.pingSent = true;
                        websocketEntry.webSocket.Send(JsonConvert.SerializeObject("GFX_CONNECTION_PING"));
                    }
                }

            GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "Ping", "Ping requests sent!");
        }

        // This method checks if the user is still logged in to the system
        private void                                    CheckUsernameTick       ()
        {
            var sessionList = new List<GFXLoginRow>(this.sessionList.Select(""));

            var dtNow       = DateTime.Now;

            Console.WriteLine("Removing inactive users...");

            foreach (var item in sessionList)
            {
                if ((dtNow - item.LastHeartbeat).TotalSeconds > logoutThreshold)
                {
                    // remove this user from the login list
                    this.sessionList.Remove(string.Format("SessionId = '{0}'", item.SessionId));

                    Console.WriteLine("Logged out user with session " + item.SessionId);
                }
            }
        }

        // This method checks if the user is online and connected to a game
        private void                                    CheckUserConnectedTick  ()
        {
            lock(this.webSocketList)
                try
                {
                    GFXLogger.GetInstance().Log(GFXLoggerLevel.WARN, "Remove users", "Removing invalid users...");

                    var removeList = new List<Guid>();

                    foreach (var item in webSocketList)
                    {
                        var websocketEntry = item.Value;

                        // we check if the client replied by checking
                        var secondsSinceLastPing = (DateTime.Now - websocketEntry.lastPing).TotalSeconds;

                        GFXLogger.GetInstance().Log(GFXLoggerLevel.WARN, "info", string.Format("{0} >= {1} of {2}",
                                                    secondsSinceLastPing,
                                                    disconnectThreshold,
                                                    item.Key));

                        if (secondsSinceLastPing >= disconnectThreshold)
                        {
                            websocketEntry.disconnected = true;

                            // The user is not responding! It could be the user has been disconnected or its not responding. Nonetheless we inform other players.

                            // We can get the session ID of the user by
                            var sessionId = item.Key;

                            // We get the lobby the user is in
                            var lobbyList = new List<GFXLobbySessionRow>(lobbySessionList.Select(string.Format("SessionId = '{0}'", sessionId)));
                            var usessList = new List<GFXLoginRow>(sessionList.Select(string.Format("SessionId = '{0}'", sessionId)));

                            var userId = usessList[0].UserId;

                            if (lobbyList.Count > 0)
                            {
                                var lobbyId = lobbyList[0].LobbyID;

                                // We get the users playing in this lobby
                                var userList = new List<GFXLobbySessionRow>(lobbySessionList.Select(string.Format("LobbyId = '{0}'", lobbyId)));

                                var disconnectedPlayerInfo = new List<GFXUserRow>(this.userList.Select(string.Format("UserId = '{0}'", userId)));
                                var disconnectedPlayerData = new Dictionary<string, object>()
                                {
                                    { "Name",       disconnectedPlayerInfo[0].FirstName + " " + disconnectedPlayerInfo[0].LastName },
                                    { "UserId",     disconnectedPlayerInfo[0].UserId },
                                    { "Username",   disconnectedPlayerInfo[0].Username }
                                };

                                var serializedInfo = JsonConvert.SerializeObject(disconnectedPlayerData);

                                lock(webSocketList)
                                    foreach (var player in userList)
                                    {
                                        if (player.SessionID != sessionId)
                                            webSocketList[player.SessionID].webSocket.Send(JsonConvert.SerializeObject(new GFXSocketResponse
                                            {
                                                Subject         = "GFX_PLAYER_DISCONNECTED",
                                                Message         = serializedInfo,
                                                ResponseCode    = GFXResponseType.Normal
                                            }));
                                    }
                            }
                        }
                        else if (websocketEntry.disconnected)
                        {
                            websocketEntry.checkCount += 1;

                            if (websocketEntry.checkCount > 3)
                            {
                                // if after 60 seconds the user hasn't reconnected, we remove that user from the list and lobby session and inform other players this user is dead.
                                removeList.Add(item.Key);
                            }
                        }
                    }

                    // remove dead connections
                    foreach (var item in removeList)
                    {
                        webSocketList.Remove(item);

                        // remove the user from the lobby
                        List<GFXLobbySessionRow> result = new List<GFXLobbySessionRow>(lobbySessionList.Select(string.Format("SessionId = '{0}'", item)));
                        lobbySessionList.Remove(string.Format("SessionId = '{0}'", item));

                        if (lobbySessionList.Count(string.Format("LobbyId = '{0}'", result[0].LobbyID)) == 0)
                        {
                            lobbyList.Remove(string.Format("LobbyId = '{0}'", result[0].LobbyID));
                        }

                        GFXLoginRow loginRow = new List<GFXLoginRow>(sessionList.Select(string.Format("SessionId = '{0}'", item), 1))[0];

                        loginRow.UserStatus = GFXLoginStatus.MENU;

                        sessionList.Update(string.Format("SessionId = '{0}'", item), loginRow);
                    }

                    removeList.Clear();

                    GFXLogger.GetInstance().Log(GFXLoggerLevel.INFO, "Remove users", "Invalid users removed!");
                }
                catch (Exception exp)
                {
                    GFXLogger.GetInstance().Log(GFXLoggerLevel.WARN, "CheckUserConnectedTick", "Error! " + exp.Message + "\n" + exp.StackTrace);
                }
        }
    }
}
