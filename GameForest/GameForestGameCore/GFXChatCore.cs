using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GameForestCoreWebSocket
{
    public class GFXChatCore
    {
        public class MessageSocket
        {
            public IWebSocketConnection    socketConnection;
            public String                  message;

            public MessageSocket(IWebSocketConnection _socket, String _message)
            {
                socketConnection = _socket;
                message = _message;
            }
        }

        Dictionary<IWebSocketConnection, String> connections;
        List<GFXChatLobby> lobbies;
        Boolean lobbyConnection = false;

        WebSocketServer         websocketServer;
        Queue<MessageSocket>    messageQueue = new Queue<MessageSocket>();

        public GFXChatCore(String address, String portnumber)
        {
            connections = new Dictionary<IWebSocketConnection, String>();
            lobbies = new List<GFXChatLobby>();
            websocketServer = new WebSocketServer("ws://" + address + ":" + portnumber);
        }

        public void start()
        {
            websocketServer.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine(socket.ConnectionInfo.ClientIpAddress);
                    Console.WriteLine("Opened socket!");
                };

                socket.OnMessage = message =>
                {
                    messageQueue.Enqueue(new MessageSocket(socket, message));

                    // put processMessage in another thread
                    // processMessage(socket, message);
                };

                socket.OnClose = () =>
                {
                    for (int i = 0; i < lobbies.Count; i++)
                    {
                        //lobbies[i].Remove(websocketServer);
                    }

                    List<GFXChatLobby> deadLobbies = killLobbies(lobbies);

                    for (int i = 0; i < deadLobbies.Count; i++)
                    {
                        //lobbies[i].Remove(deadLobbies[i]);
                    }
                };


            });

            var thread = new Thread(new ThreadStart(() =>
                {
                    while (true)
                    {
                        // process commands here
                        MessageSocket thingToDequeue = null;

                        lock (messageQueue)
                        {
                            if (messageQueue.Count != 0)
                            {
                                thingToDequeue = messageQueue.Dequeue();

                                if (thingToDequeue != null)
                                {
                                    processMessage(thingToDequeue.socketConnection, thingToDequeue.message);
                                }
                            }
                                
                        }

                        Thread.Sleep(100);
                    }
                }));

            thread.Start();

            Console.Read();
            websocketServer.Dispose();
        }

        public void processMessage(IWebSocketConnection socket, String message)
        {
            var jsonSerializer = new JsonSerializer();

            Console.Write(message);

            dynamic packet = jsonSerializer.Deserialize(new JsonTextReader(new StringReader(message)));

            String name = packet.Name + "";
            String mess = packet.Mess + "";
            String lobbyName = packet.Lobby + "";
            int index = getLobbyIndex(lobbyName, lobbies);

            if (packet.Message == "Name")
            {
                
                Console.WriteLine("Sent name!");

            } else if (packet.Message == "Mess")
            {
                Console.WriteLine("Sent message!");

                if (index != -1)
                {
                    if (lobbies[index].ConnectionExists() == false)
                    {
                        lobbies[index].Connection(socket, name);
                        lobbyConnection = true;
                    }

                    lobbies[index].SendToAll(mess, socket);
                }
                else
                {
                    lobbies.Add(new GFXChatLobby(lobbyName));
                    index = getLobbyIndex(lobbyName, lobbies);

                    if (lobbies[index].ConnectionExists() == false)
                    {
                        lobbies[index].Connection(socket, name);
                        lobbyConnection = true;
                    }

                    lobbies[index].SendToAll(mess, socket);
                }
            }
            else
            {
                Console.WriteLine("Unknown packet message!");
            }
        }

        public void processMessage2(IWebSocketConnection socket, String message)
        {
            var jsonSerializer = new JsonSerializer();

            Console.Write(message);
            dynamic packet = jsonSerializer.Deserialize(new JsonTextReader(new StringReader(message)));

            if (packet.Message == "Name")
            {
                String lobbyName = packet.Lobby + "";
                String name = packet.Name + "";
                int index = getLobbyIndex(lobbyName, lobbies);

                if (index != -1)
                {
                    lobbies[index].Connection(socket, name);
                }
                else
                {
                    lobbies.Add(new GFXChatLobby(lobbyName));
                    index = getLobbyIndex(lobbyName, lobbies);

                    lobbies[index].Connection(socket, name);
                    Console.Write("Lobby: " + index);
                }
            }
            else
            {
                String lobbyName = packet.Lobby + "";
                String mess = packet.Mess + "";
                String name = packet.Name + "";

                int index = getLobbyIndex(lobbyName, lobbies);

                if (index != -1)
                {
                    if (lobbies[index].ConnectionExists() == false)
                    {
                        lobbies[index].Connection(socket, name);
                        lobbyConnection = true;
                    }
                    
                    lobbies[index].SendToAll(mess, socket);
                }
                else
                {
                    lobbies.Add(new GFXChatLobby(lobbyName));
                    index = getLobbyIndex(lobbyName, lobbies);

                    if (lobbies[index].ConnectionExists() == false)
                    {
                        lobbies[index].Connection(socket, name);
                        lobbyConnection = true;
                    }

                    lobbies[index].SendToAll(mess, socket);
                }
            }
        }

        public List<GFXChatLobby> killLobbies(List<GFXChatLobby> currentLobbies)
        {
            List<GFXChatLobby> list = new List<GFXChatLobby>();

            for (int i = 0; i < currentLobbies.Count; i++)
                if (currentLobbies[i].isToBeKilled() == true)
                    list.Add(currentLobbies[i]);
            
            return list;
        }

        public int getLobbyIndex(String lobbyName, List<GFXChatLobby> list)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].getName() == lobbyName) return i;
            return -1;
        }
    }
}
