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
    }

    public class GFXLogger
    {
        private string                              applicationGroup    = "";
        private List<GFXLoggerEntry>                entries             = new List<GFXLoggerEntry>();

        public ReadOnlyCollection<GFXLoggerEntry>   Entries
        {
            get { return entries.AsReadOnly(); }
        }

        public event Action<GFXLoggerEntry>         OnLogged;

        public GFXLogger                            (string group)
        {
            applicationGroup = group;
        }

        public void                                 Log                 (GFXLoggerLevel level, string category, string message)
        {
            var entry   = new GFXLoggerEntry
            {
                LoggerLevel = level,
                Category    = category,
                Message     = message
            };

            var msg     = string.Format("[{0} | {1}] ({2}) {3}", applicationGroup, category, level, message);
            
            entries.Add(entry);

            if (OnLogged != null)
                OnLogged(entry);

            Debug.WriteLine(msg);
        }

        public void                                 Clear               ()
        {
            entries.Clear();
        }
    }
}
