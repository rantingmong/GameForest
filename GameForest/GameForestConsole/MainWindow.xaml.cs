using GameForestDatabaseConnector.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace GameForestConsole
{
    public partial class MainWindow : Window
    {
        private GFXRestServerCore serverCore;
        private GFXWebsocketCore websocketCore;

        public MainWindow()
        {
            InitializeComponent();

            textConsole.Document.Blocks.Clear();

            serverCore      = new GFXRestServerCore();
            websocketCore   = new GFXWebsocketCore();

            serverCore.OnServerRestStart += (o, e) =>
                {
                    Dispatcher.Invoke(new Action(() =>
                        {
                            ellipseREST.Fill = Brushes.Green;
                        }));
                };
            serverCore.OnServerRestStop += (o, e) =>
                {
                    Dispatcher.Invoke(new Action(() =>
                        {
                            ellipseREST.Fill = Brushes.Gray;
                        }));
                };

            GFXLogger.GetInstance().OnLogged += (entry) =>
                {
                    Dispatcher.Invoke(new Action(() =>
                        {
                            var par = new Paragraph();
                            var run = new Run(string.Format("[{0}] ({1}) {2}", entry.Category, entry.LoggerLevel, entry.Message));

                            switch(entry.LoggerLevel)
                            {
                                case GFXLoggerLevel.ERROR:
                                case GFXLoggerLevel.FATAL:
                                    run.Foreground = Brushes.DarkRed;
                                    break;
                                case GFXLoggerLevel.INFO:
                                    run.Foreground = Brushes.DarkBlue;
                                    break;
                                case GFXLoggerLevel.WARN:
                                    run.Foreground = Brushes.Orange;
                                    break;
                            }

                            par.Inlines.Add(run);

                            textConsole.Document.Blocks.Add(par);
                            textConsole.ScrollToEnd();
                        }));
                };
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (serverCore.IsStarted)
            {
                serverCore.Stop();
            }

            if (websocketCore.IsRunning)
            {
                websocketCore.Stop();
            }
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            ellipseREST.Fill = Brushes.Orange;
            ellipseWSSV.Fill = Brushes.Orange;

            serverCore.Start();
            websocketCore.Start();

            ellipseWSSV.Fill = Brushes.Green;

            buttonStart.IsEnabled = false;
            buttonStop.IsEnabled = true;
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            serverCore.Stop();
            websocketCore.Stop();

            buttonStart.IsEnabled = true;
            buttonStop.IsEnabled = false;

            ellipseWSSV.Fill = Brushes.Gray;
        }

        private void buttonClearLog_Click(object sender, RoutedEventArgs e)
        {
            textConsole.Document.Blocks.Clear();
        }

        private void buttonSaveLog_Click(object sender, RoutedEventArgs e)
        {
            Thread saveThread = new Thread(new ThreadStart(() =>
                {
                    var saveList = new List<GFXLoggerEntry>(GFXLogger.GetInstance().Entries);

                    using (TextWriter tw = new StreamWriter(File.OpenWrite("log-" + DateTime.Now.ToFileTime() + ".txt")))
                    {
                        foreach (var item in saveList)
                        {
                            tw.WriteLine(item.ToString());
                        }

                        tw.Flush();
                    }
                }));

            saveThread.Start();
        }
    }
}
