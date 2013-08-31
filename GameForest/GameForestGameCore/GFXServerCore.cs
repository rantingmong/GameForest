using System;
using System.Collections.Generic;
using System.Diagnostics;
using Fleck;
using Newtonsoft.Json;
using GameForestCore.Database;
using GameForestCoreWebSocket.Messages;
using GameForestDatabaseConnector.Logger;

namespace GameForestCoreWebSocket
{
    public class GFXServerCore
    {
        public static readonly string                   INIT_CONNECTION     = "GFX_INIT_CONNECTION";
        public static readonly string                   STOP_CONNECTION     = "GFX_STOP_CONNECTION";

        private WebSocketServer                         server;

        private List<Guid>                              verifyList          = new List<Guid>();

        private Dictionary<Guid, Guid>                  connectionList      = new Dictionary<Guid, Guid>();  // key is session id
        private Dictionary<Guid, IWebSocketConnection>  webSocketList       = new Dictionary<Guid, IWebSocketConnection>();

        private Dictionary<Guid, GFXGameData>           gameDatalist        = new Dictionary<Guid, GFXGameData>();

        private List<GFXSocketListener>                 listenerList        = new List<GFXSocketListener>();

        private GFXDatabaseTable<GFXLobbyRow>           lobbyList           = new GFXDatabaseTable<GFXLobbyRow>(new GFXLobbyRowTranslator());
        private GFXDatabaseTable<GFXLobbySessionRow>    lobbySessionList    = new GFXDatabaseTable<GFXLobbySessionRow>(new GFXLobbySessionRowTranslator());

        public Dictionary<Guid, GFXGameData>            GameDataList
        {
            get { return gameDatalist; }
        }

        public Dictionary<Guid, Guid>                   ConnectionList
        {
            get { return connectionList; }
        }

        public Dictionary<Guid, IWebSocketConnection>   WebSocketList
        {
            get { return webSocketList; }
        }

        public GFXDatabaseTable<GFXLobbyRow>            LobbyList
        {
            get { return lobbyList; }
        }

        public GFXDatabaseTable<GFXLobbySessionRow>     LobbySessionList
        {
            get { return lobbySessionList; }
        }

        public GFXServerCore                            ()
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
            
            server = new WebSocketServer("ws://localhost:8084");

            server.Start(socket =>
                {
                    socket.OnOpen       = () =>
                        {
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
                                    webSocketList.Add(info.SessionId, socket);
                                    connectionList.Add(info.SessionId, info.ConnectionId);

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

        public void Stop()
        {
            server.Dispose();
        }
    }
}
