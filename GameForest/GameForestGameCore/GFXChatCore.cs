using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameForestCoreWebSocket
{
    public class GFXChatCore
    {
        Dictionary <IWebSocketConnection, String> connections;
		List<GFXChatLobby> lobbies;

		WebSocketServer websocketServer;

		public GFXChatCore (String address, String portnumber)
		{
			connections 	= new Dictionary <IWebSocketConnection, String>();
			lobbies			= new List <GFXChatLobby>();
			websocketServer = new WebSocketServer("ws://" + address + ":" + portnumber);
		}

		public void start()
		{
			var jsonSerializer = new JsonSerializer();

			websocketServer.Start (socket =>
			{
				socket.OnOpen = () =>
				{
					Console.WriteLine(socket.ConnectionInfo.ClientIpAddress);
					Console.WriteLine("Opened socket!");
				};

				socket.OnMessage = message =>
				{
					Console.Write(message);
					dynamic packet = jsonSerializer.Deserialize(new JsonTextReader(new StringReader(message)));

					if (packet.Message == "Name")
					{
						String lobbyName 	= packet.Lobby + "";
						String name 		= packet.Name + "";
						int index 			= getLobbyIndex(lobbyName, lobbies);

						if (index != -1)
						{
							lobbies[index].Connection(socket, name);
						} else {
							lobbies.Add(new GFXChatLobby(lobbyName));
							index = getLobbyIndex(lobbyName, lobbies);

							lobbies[index].Connection(socket, name);
							Console.Write("Lobby: " + index);
						}
					} else {
						String lobbyName = packet.Lobby + "";
						String mess 	 = packet.Mess + "";

                        int index = getLobbyIndex(lobbyName, lobbies);

						if (index != -1)
						{
							lobbies[index].SendToAll(mess, socket);
						} else {
							lobbies.Add(new GFXChatLobby(lobbyName));
							index = getLobbyIndex(lobbyName, lobbies);
							lobbies[index].SendToAll(mess, socket);
						}
					}
				};

				socket.OnClose = () =>
				{
					for (int i = 0; i < lobbies.Count; i++)
					{
						//lobbies[i].Remove(websocketServer);
					}

					List<GFXChatLobby> deadLobbies = killLobbies(lobbies);

					for (int i = 0; i< deadLobbies.Count; i++)
					{
						//lobbies[i].Remove(deadLobbies[i]);
					}
				};
			});
			
			Console.Read();
			websocketServer.Dispose();
		}

		public List<GFXChatLobby> killLobbies(List<GFXChatLobby> currentLobbies) 
		{
			List<GFXChatLobby> list = new List<GFXChatLobby>();

			for(int i = 0; i < currentLobbies.Count; i++)
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
