using GameForestCore.Database;
using GameForestDatabaseConnector.Logger;
using System;
using System.Collections.Generic;

namespace GameForest
{
    class Program
    {
        static void Main(string[] args)
        {
            GFXLogger.GetInstance().OnLogged += (entry) =>
            {
                string message = string.Format("[{0}] ({1}) {2}", 
                                               entry.LoggerLevel,
                                               entry.Category,
                                               entry.Message);

                switch (entry.LoggerLevel)
                {
                    case GFXLoggerLevel.ERROR:
                    case GFXLoggerLevel.FATAL:

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(message);
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
            };

            string username = "";
            string password = "";

            string ipAddress = "";

            Console.WriteLine("Please enter your MySql username and password:");

            Console.Write("Username: ");
            username = Console.ReadLine();

            Console.Write("Password: ");
            password = Console.ReadLine();

            Console.WriteLine();
            Console.WriteLine("Enter address for websocket: ");
            ipAddress = Console.ReadLine();

            GFXDatabaseCore.Initialize("Server=localhost;Database=GameForest;Uid=" + username + ";Pwd=" + password + ";");

            GFXRestServerCore   restServer = new GFXRestServerCore();
            GFXWebsocketCore    wbSxServer = new GFXWebsocketCore(string.IsNullOrEmpty(ipAddress) ? "localhost" : ipAddress);

            restServer.Start();
            wbSxServer.Start();

            Console.WriteLine("Server is started! Type 'stop' to exit the sever.");

            string          command     = "";
            List<string>    commandList = new List<string>();

            commandList.Add("stop");
            commandList.Add("clear");
            commandList.Add("save-log");

            do
            {
                command = Console.ReadLine();

                if (!commandList.Contains(command.ToLower()))
                {
                    Console.WriteLine("Command is invalid! Please try again.");
                    continue;
                }

                switch (command.ToLower())
                {
                    case "stop":
                        Console.WriteLine("Server is shutting down...");
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case "save-log":
                        break;
                }

            } while (command.ToLower() != "stop");

            restServer.Stop();
            wbSxServer.Stop();
        }
    }
}
