using Fleck;
using GameForestDatabaseConnector.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GameForestCoreWebSocket
{
    public struct MessageSocket
    {
        public IWebSocketConnection webSocket;
        public string               message;

        public MessageSocket        (IWebSocketConnection websocket, string message) : this()
        {
            this.message    = message;
            this.webSocket  = websocket;
        }
    }

    public struct ChatEntry
    {
        public IWebSocketConnection webSocket;
        public Guid                 lobbyId;
    }

    public class GFXChatCore
    {
        WebSocketServer             websocketServer = null;
        Queue<MessageSocket>        messageQueue    = new Queue<MessageSocket>();

        Dictionary<Guid, ChatEntry> clientList      = new Dictionary<Guid, ChatEntry>();

        Thread                      messageThread   = null;

        public                      GFXChatCore     (String address, String portnumber)
        {
            websocketServer = new WebSocketServer("ws://" + address + ":" + portnumber);
        }

        public void                 start           ()
        {
            websocketServer.Start(socket =>
            {
                socket.OnOpen = () =>
                {

                };

                socket.OnMessage = message =>
                {
                    messageQueue.Enqueue(new MessageSocket(socket, message));
                };

                socket.OnClose = () =>
                {
                    // we remove the websocket entry from clientList
                    Guid keyToRemove = Guid.Empty;

                    foreach (var item in clientList)
                    {
                        if (item.Value.webSocket == socket)
                        {
                            keyToRemove = item.Key;
                            break;
                        }
                    }

                    if (keyToRemove != Guid.Empty)
                    {
                        clientList.Remove(keyToRemove);
                    }
                };
            });

            messageThread = new Thread(new ThreadStart(() =>
                {
                    while (true)
                    {
                        // process commands here
                        MessageSocket thingToDequeue;

                        lock (messageQueue)
                        {
                            if (messageQueue.Count > 0)
                            {
                                thingToDequeue = messageQueue.Dequeue();
                                processMessage(thingToDequeue.webSocket, thingToDequeue.message);
                            }
                        }

                        Thread.Sleep(50);
                    }
                }));

            messageThread.Start();
        }

        public void                 stop            ()
        {
            websocketServer.Dispose();
            messageThread.Abort();
        }

        public void                 processMessage  (IWebSocketConnection socket, String msg)
        {
            Console.Write(msg);

            var     packet  = JsonConvert.DeserializeObject(msg) as Dictionary<string, object>;

            Guid    lobby   = Guid.Parse(packet["Lobby"] as string);
            string  message = packet["Message"] as string;
            string  value   = packet["Value"]   as string;

            switch (message)
            {
                case "open":
                    {
                        ChatEntry newEntry = new ChatEntry
                        {
                            lobbyId     = lobby,
                            webSocket   = socket
                        };

                        clientList.Add(Guid.Parse(value), newEntry);

                        Dictionary<string, object> sendMessage = new Dictionary<string, object>();

                        sendMessage["Messsage"] = "open";
                        sendMessage["Value"]    = "okay";

                        // we inform the client the server accepted the connection
                        socket.Send(JsonConvert.SerializeObject(sendMessage));
                    }
                    break;
                case "chat":
                    {
                        // we get other players in the lobby specified by this message
                        List<ChatEntry> otherPlayers = new List<ChatEntry>();

                        foreach (var item in clientList)
                        {
                            if (item.Value.lobbyId == lobby)
                            {
                                otherPlayers.Add(item.Value);
                            }
                        }

                        // after that, we send the 'value' to other players
                        foreach (var item in otherPlayers)
                        {
                            if (item.webSocket == socket)
                                continue;

                            Dictionary<string, object> sendMessage = new Dictionary<string, object>();

                            sendMessage["Messsage"] = "chat";
                            sendMessage["Value"]    = value;

                            // fly fly away!
                            item.webSocket.Send(JsonConvert.SerializeObject(sendMessage));
                        }
                    }
                    break;
            }
        }
    }
}
