using System;
using MySql.Data.MySqlClient;

namespace GameForestCore.Database
{
    public static class GFXDatabaseCore
    {
        private static string           connectionString    = "";

        public static void              Initialize          (string connectionString)
        {
            GFXDatabaseCore.connectionString = connectionString;
        }

        public static MySqlConnection   GetConnection       ()
        {
            var msc = new MySqlConnection(connectionString);
            msc.Open();

            return msc;
        }

        public static string            ToSQLString         (string value)
        {
            return string.Format("'{0}'", value);
        }
    }
}
