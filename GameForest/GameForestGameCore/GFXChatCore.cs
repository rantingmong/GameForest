using Fleck;
using GameForestCore.Database;
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

        public string               userName;
    }

    public class GFXChatCore
    {
        WebSocketServer                 websocketServer = null;
        Queue<MessageSocket>            messageQueue    = new Queue<MessageSocket>();

        Dictionary<Guid, ChatEntry>     clientList      = new Dictionary<Guid, ChatEntry>();

        Thread                          messageThread   = null;

        GFXDatabaseTable<GFXLoginRow>   loginTable      = new GFXDatabaseTable<GFXLoginRow>(new GFXLoginRowTranslator());
        GFXDatabaseTable<GFXUserRow>    userTable       = new GFXDatabaseTable<GFXUserRow>(new GFXUserRowTranslator());

        public                          GFXChatCore     (String address, String portnumber)
        {
            websocketServer = new WebSocketServer("ws://" + address + ":" + portnumber);
        }

        public void                     start           ()
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

        public void                     stop            ()
        {
            websocketServer.Dispose();
            messageThread.Abort();
        }

        public void                     processMessage  (IWebSocketConnection socket, String msg)
        {
            Console.Write(msg);

            var     packet  = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg);

            Guid    lobby   = Guid.Parse(packet["Lobby"] as string);
            string  name    = packet["Name"]    as string;
            string  message = packet["Message"] as string;
            string  value   = packet["Value"]   as string;

            var userSession = new List<GFXLoginRow>(loginTable.Select(string.Format("SessionId = '{0}'", name)))[0];
            var userInfo    = new List<GFXUserRow>(userTable.Select(string.Format("UserId = '{0}'", userSession.UserId)))[0];

            switch (message)
            {
                case "open":
                    {
                        ChatEntry newEntry = new ChatEntry
                        {
                            lobbyId     = lobby,
                            webSocket   = socket,
                            userName    = userInfo.Username
                        };

                        clientList.Add(Guid.Parse(name), newEntry);

                        Dictionary<string, object> sendMessage = new Dictionary<string, object>();

                        sendMessage["Message"]  = "open";
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
                            Dictionary<string, object> sendMessage = new Dictionary<string, object>();

                            sendMessage["Message"]  = "chat";
                            sendMessage["Value"]    = value;
                            sendMessage["Name"]     = userInfo.Username;

                            // fly fly away!
                            item.webSocket.Send(JsonConvert.SerializeObject(sendMessage));
                        }
                    }
                    break;
            }
        }
    }
}
