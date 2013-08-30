using GameForestDatabaseConnector.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameForestConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        private void buttonClearLog_Click(object sender, RoutedEventArgs e)
        {
            textConsole.Document.Blocks.Clear();
        }
    }
}
