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
    public class GFXServerCore
    {
        public static readonly string                   INIT_CONNECTION     = "GFX_INIT_CONNECTION";
        public static readonly string                   STOP_CONNECTION     = "GFX_STOP_CONNECTION";

        private double                                  disconnectThreshold = 30D;  // 30 seconds
        private double                                  logoutThreshold     = 1.5D; // 1 hour and 30 minutes

        private Timer                                   loginCheckTimer     = null;
        private Timer                                   sessionCheckTimer   = null;

        private WebSocketServer                         server;

        private List<Guid>                              verifyList          = new List<Guid>();

        private Dictionary<Guid, Guid>                  connectionList      = new Dictionary<Guid, Guid>();  // key is session id
        private Dictionary<Guid, IWebSocketConnection>  webSocketList       = new Dictionary<Guid, IWebSocketConnection>();

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

        public  Dictionary<Guid, IWebSocketConnection>  WebSocketList
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

<<<<<<< HEAD
        public GFXDatabaseTable<GFXUserRow>             UserList
        {
            get { return userList; }
        }

        public GFXDatabaseTable<GFXLoginRow>            LoginList
        {
            get { return sessionList; }
        }

=======
>>>>>>> origin/analytics
        public  GFXServerCore                           (string ipAddress = "localhost")
        {
            listenerList.Add(new GFXConfirmTurn());
            listenerList.Add(new GFXGameAskUserData());
            listenerList.Add(new GFXGameAskData());
            listenerList.Add(new GFXGameFinish());
            listenerList.Add(new GFXGameNextTurn());
            listenerList.Add(new GFXGameSendData());
            listenerList.Add(new GFXGameSendUserData());
            listenerList.Add(new GFXGameStart());
            listenerList.Add(new GFXGameStartConfirm());
            
            // TODO: create a timer that checks users if they are still online (interval: 30,000ms)
            loginCheckTimer     = new Timer(new TimerCallback((o) =>
                {
                    CheckUsernameTick();
                }));

            sessionCheckTimer   = new Timer(new TimerCallback((o) =>
                {
                    // CheckUserConnectedTick();
                }));

            loginCheckTimer.Change  (TimeSpan.FromHours(1),     TimeSpan.FromHours(1));
            sessionCheckTimer.Change(TimeSpan.FromSeconds(30),  TimeSpan.FromSeconds(30));

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

                            Debug.WriteLine("Connection opened with " + socket.ConnectionInfo);
                        };
                    socket.OnClose      = () =>
                        {
                            Debug.WriteLine("Connection closed with " + socket.ConnectionInfo);
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
                                    // connection id verified! we can add the session id to the connection list
                                    webSocketList   [info.SessionId] = socket;
                                    connectionList  [info.SessionId] = info.ConnectionId;

                                    // and remove the key from verify list (for safety purposes :) )
                                    verifyList.Remove(info.ConnectionId);
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
                                webSocketList.Remove(info.SessionId);
                                connectionList.Remove(info.SessionId);
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

            loginCheckTimer.Dispose();
            sessionCheckTimer.Dispose();
        }

        // This method checks if the user is still logged in to the system
        private void                                    CheckUsernameTick       ()
        {
            var sessionList = new List<GFXLoginRow>(this.sessionList.Select(""));

            var dtNow       = DateTime.Now;

            foreach (var item in sessionList)
            {
                if ((dtNow - item.LastHeartbeat).TotalHours > logoutThreshold)
                {
                    // remove this user from the login list
                    this.sessionList.Remove(string.Format("SessionId = '{0}'", item.SessionId));
                }
            }
        }

        // This method checks if the user is online and connected to a game
        private void                                    CheckUserConnectedTick  ()
        {
            var sessionList         = new List<GFXLoginRow>(this.sessionList.Select(""));
            var lobbySessionList    = new List<GFXLobbySessionRow>(this.lobbySessionList.Select(""));

            var dtNow               = DateTime.Now;

            foreach (var item in lobbySessionList)
            {
                try
                {
                    var result = sessionList.Find(new Predicate<GFXLoginRow>((x) =>
                    {
                        foreach (var user in sessionList)
                        {
                            if (user.SessionId.Equals(item.SessionID))
                            {
                                return true;
                            }
                        }

                        return false;
                    }));

                    if ((dtNow - result.LastHeartbeat).TotalSeconds > disconnectThreshold)
                    {
                        // get the lobby the player is in
                        var getLobbySessionResult = new List<GFXLobbySessionRow>(this.lobbySessionList.Select(string.Format("SessionId = '{0}'", result.SessionId)));

                        if (getLobbySessionResult.Count > 0)
                        {
                            // this client has disconnected! send a message to other players that has this player
                            var playersWithTheDisconnectedPlayer = new List<GFXLobbySessionRow>(this.lobbySessionList.Select(string.Format("LobbyId = '{0}' AND SessionId <> '{0}'", getLobbySessionResult[0].LobbyID, getLobbySessionResult[0].SessionID)));

                            var disconnectedPlayerInfo = new List<GFXUserRow>(this.userList.Select(string.Format("UserId = '{0}'", result.UserId)));
                            var disconnectedPlayerData = new Dictionary<string, object>()
                                    {
                                        { "Name",       disconnectedPlayerInfo[0].FirstName + " " + disconnectedPlayerInfo[0].LastName },
                                        { "UserId",     disconnectedPlayerInfo[0].UserId },
                                        { "Username",   disconnectedPlayerInfo[0].Username }
                                    };

                            foreach (var player in playersWithTheDisconnectedPlayer)
                            {
                                webSocketList[player.SessionID].Send(JsonConvert.SerializeObject(new GFXSocketResponse
                                {
                                    Subject         = "GFX_PLAYER_DISCONNECTED",
                                    Message         = JsonConvert.SerializeObject(disconnectedPlayerData),
                                    ResponseCode    = GFXResponseType.Normal
                                }));
                            }

                            // make a new one-shot timer to wait for the player to reconnect (90 seconds) before we totally remove that player from the game.
                            // NOTE: the clients will wait for 120 seconds (additional 30 seconds) to compensate network lag.
                            new Timer(new TimerCallback((o) =>
                                {
                                    var sessionRow                  = ((GFXLobbySessionRow)((object[])o)[0]);
                                    var guidOfDisconnectedPlayer    = sessionRow.SessionID;

                                    // remove user from session list
                                    this.lobbySessionList.Remove(string.Format("SessionId = '{0}'", guidOfDisconnectedPlayer));

                                    webSocketList.Remove(guidOfDisconnectedPlayer);
                                    connectionList.Remove(guidOfDisconnectedPlayer);

                                    disconnectedList.Add(new GFXDisconnectInfo
                                        {
                                            SessionId   = sessionRow.SessionID,
                                            UserId      = ((GFXUserRow)((object[])o)[1]).UserId,
                                            LobbyId     = sessionRow.LobbyID
                                        });
                                }), new object[] { getLobbySessionResult, disconnectedPlayerInfo[0] }, 90000, Timeout.Infinite);

                        }
                    }
                }
                catch (ArgumentNullException exp)
                {
                    // I swallow thee.
                    Console.Write(exp + "\r\n");
                }
            }
        }
    }
}
