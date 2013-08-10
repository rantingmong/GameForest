using System;
using System.Collections.Generic;

using Fleck;
using Newtonsoft.Json;

namespace GameForestCoreWebSocket
{
    public class GFXServerCore
    {
        public static readonly string   INIT_CONNECTION = "GFX_INIT_CONNECTION";
        public static readonly string   STOP_CONNECTION = "GFX_STOP_CONNECTION";

        private WebSocketServer         server;

        private List<Guid>              verifyList      = new List<Guid>();

        private Dictionary<Guid, Guid>  connectionList  = new Dictionary<Guid,Guid>();      // key is session id
        private List<GFXSocketListener> listenerList    = new List<GFXSocketListener>();

        public GFXServerCore            ()
        {
            server = new WebSocketServer(1193, "ws://localhost");

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
                        };
                    socket.OnClose      = () =>
                        {

                        };

                    socket.OnMessage    = message =>
                        {
                            // parse message
                            GFXSocketInfo info = JsonConvert.DeserializeObject<GFXSocketInfo>(message);

                            if (info.Message == INIT_CONNECTION)
                            {
                                if (verifyList.Contains(info.ConnectionId))
                                {
                                    // connection id verified! we can add the session id to the connection list
                                    connectionList.Add(info.SessionId, info.ConnectionId);
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
                            else if (info.Message == STOP_CONNECTION)
                            {
                                if (verifyList.Contains(info.ConnectionId))
                                {
                                    connectionList.Remove(info.SessionId);
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

                                foreach (var listener in listenerList)
                                {
                                    if (info.Message == listener.Subject)
                                    {
                                        socket.Send(JsonConvert.SerializeObject(listener.DoMessage(info)));
                                        break;
                                    }
                                }
                            }
                        };
                });
        }
    }
}
