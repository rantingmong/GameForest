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
        private GFXLogger logger = new GFXLogger("GameForest");
        private GFXServerCore serverCore;

        public MainWindow()
        {
            InitializeComponent();

            serverCore = new GFXServerCore(logger);

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

            logger.OnLogged += (entry) =>
                {
                    Dispatcher.Invoke(new Action(() =>
                        {
                            var par = new Paragraph();
                            par.Inlines.Add(new Run(string.Format("[{0}] ({1}) {2}", entry.Category, entry.LoggerLevel, entry.Message)));

                            textConsole.Document.Blocks.Add(par);
                        }));
                };
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            ellipseREST.Fill = Brushes.Orange;

            serverCore.Start();

            buttonStart.IsEnabled = false;
            buttonStop.IsEnabled = true;
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            serverCore.Start();

            buttonStart.IsEnabled = true;
            buttonStop.IsEnabled = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (serverCore.IsStarted)
            {
                serverCore.Stop();
            }
        }
    }
}
