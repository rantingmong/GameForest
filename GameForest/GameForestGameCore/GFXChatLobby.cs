using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameForestCoreWebSocket
{
    public class GFXChatLobby
    {
        private Boolean isNew = true;
        private String name;
        private Dictionary<IWebSocketConnection, String> connections;

        class ChatName
        {
            public String Message = "Names";
            public List<String> names;
            public ChatName(List<String> temp)
            {
                names = temp;
            }
        }

        class ChatMessage
        {
            public String Message = "Chat";
            public String Chat = "";
            public ChatMessage(String mes)
            {
                Chat = mes;
            }
        }

        public GFXChatLobby(String _name)
        {
            name = _name;
            connections = new Dictionary<IWebSocketConnection, String>();
        }

        public void Connection(IWebSocketConnection con, String name)
        {
            if (isNew) isNew = false;

            connections.Add(con, name);
            sendToAllNames();
        }

        public Boolean ConnectionExists()
        {
            if (isNew == false) return true;
            return false;
        }

        public void sendToAllNames()
        {
            List<String> getNames = new List<String>();

            foreach (String names in connections.Values)
            {
                getNames.Add(names);
            }

            ChatName temp = new ChatName(getNames);

            foreach (IWebSocketConnection connection in connections.Keys)
            {
                connection.Send(JsonConvert.SerializeObject(temp));
            }
        }

        public Boolean findIfInList(IWebSocketConnection con)
        {
            foreach (IWebSocketConnection cons in connections.Keys)
                if (cons == con) return true;
            return false;
        }

        public void Remove(IWebSocketConnection con)
        {
            if (findIfInList(con))
            {
                connections.Remove(con);
                sendToAllNames();
            }
        }

        public String getName()
        {
            return name;
        }

        public Boolean isToBeKilled()
        {
            if (!isNew)
            {
                if (connections.Count == 0) return true;
                else return false;
            }
            else
            {
                return false;
            }
        }

        public void SendToAll(String message, IWebSocketConnection con)
        {
            message = connections[con] + ":" + message;

            ChatMessage temp = new ChatMessage(message);

            foreach (IWebSocketConnection connection in connections.Keys)
            {
                connection.Send(JsonConvert.SerializeObject(temp));
            }
        }

    }
}
