using System;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public static class GFXDatabaseCore
    {
        static GFXDatabaseCore()
        {
            Instance = null;
        }

        public static MySqlConnection   Instance    { get; private set; }

        public static void              Initialize  (string connectionString)
        {
            if (Instance != null)
                return;

            Instance = new MySqlConnection(connectionString);

            try
            {
                Console.WriteLine("Initializing MySql...");
                Instance.Open();
            }
            catch (MySqlException exception)
            {
                Console.Error.WriteLine("MySql initialization failed: " + exception.Message);
            }
        }
        public static void              Destroy     ()
        {
            if (Instance == null)
                return;

            Instance.Close();
            Instance.Dispose();
        }

        public static string            ToSQLString (string value)
        {
            return string.Format("'{0}'", value);
        }
    }
}
