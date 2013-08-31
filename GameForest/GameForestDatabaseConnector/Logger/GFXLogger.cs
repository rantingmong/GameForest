using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GameForestDatabaseConnector.Logger
{
    public enum GFXLoggerLevel
    {
        INFO,
        WARN,
        ERROR,
        FATAL
    }
    public struct GFXLoggerEntry
    {
        public GFXLoggerLevel LoggerLevel { get; set; }

        public string Category { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] ({1}) {2}", Category, LoggerLevel, Message);
        }
    }

    public class GFXLogger
    {
        private static GFXLogger                    instance            = new GFXLogger();

        public static GFXLogger                     GetInstance         ()
        {
            return instance;
        }

        private List<GFXLoggerEntry>                entries             = new List<GFXLoggerEntry>();

        public ReadOnlyCollection<GFXLoggerEntry>   Entries
        {
            get { return entries.AsReadOnly(); }
        }

        public event Action<GFXLoggerEntry>         OnLogged;

        private                                     GFXLogger           ()
        {
            Debug.WriteLine("Logger initialized!");
        }

        public void                                 Log                 (GFXLoggerLevel level, string category, string message)
        {
            var entry   = new GFXLoggerEntry
            {
                LoggerLevel = level,
                Category    = category,
                Message     = message
            };

            entries.Add(entry);

            if (OnLogged != null)
                OnLogged(entry);

            Debug.WriteLine(string.Format("[{0}] ({1}) {2}", category, level, message));
        }

        public void                                 Clear               ()
        {
            entries.Clear();
        }
    }
}
