using GameForestCore.Database;
using GameForestDatabaseConnector.Logger;
using System;

namespace GameForest
{
    class Program
    {
        static void Main(string[] args)
        {
            GFXLogger.GetInstance().OnLogged += (entry) =>
            {
                string message = string.Format("[{0}] ({1}) {2}", entry.Category, entry.LoggerLevel, entry.Message);

                switch (entry.LoggerLevel)
                {
                    case GFXLoggerLevel.ERROR:
                    case GFXLoggerLevel.FATAL:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case GFXLoggerLevel.INFO:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case GFXLoggerLevel.WARN:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                }

                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.White;
            };

            string username = "";
            string password = "";

            Console.WriteLine("Please enter your MySql username and password:");

            Console.Write("Username: ");
            username = Console.ReadLine();

            Console.Write("Password: ");
            password = Console.ReadLine();

            GFXDatabaseCore.Initialize("Server=localhost;Database=GameForest;Uid=" + username + ";Pwd=" + password + ";");

            GFXRestServerCore   restServer = new GFXRestServerCore();
            GFXWebsocketCore    wbSxServer = new GFXWebsocketCore();

            restServer.Start();
            wbSxServer.Start();

            Console.WriteLine("Server is started! Type 'stop' to exit the sever.");

            string command = "";

            do
            {
                command = Console.ReadLine();

                if (command != "stop")
                {
                    Console.WriteLine("Incorrect command entered. Try again.");
                }

            } while (command.ToLower() != "stop");

            restServer.Stop();
            wbSxServer.Stop();
        }
    }
}
