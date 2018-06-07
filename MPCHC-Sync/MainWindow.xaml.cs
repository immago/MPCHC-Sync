using MPC_HC.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Timers;
using System.Net;
using System.Threading;

namespace MPCHC_Sync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MPCController player;
        private Settings settings;
        private Client client;

        public MainWindow()
        {
            InitializeComponent();

            settings = new Settings();
            client = new Client();
            client.stateChanged += clientStateChanged;
            player = new MPCController();
            player.stateChanged += playerStateChanged;


            //Debug.WriteLine(settings.UUID);
            //Debug.WriteLine(Dns.GetHostName());

            //client.Connect(Dns.GetHostName(), 5000);
            //client.Subscribe("86de0ff4-3115-4385-b485-b5e83ae6b890", "1234");
            //client.Set("86de0ff4-3115-4385-b485-b5e83ae6b890", "1234", "test.mp4", new TimeSpan(0, 0, 10), new TimeSpan(1, 10, 0), State.Playing);
            
        }

        private void clientStateChanged(object sender, ClientEventArgs e)
        {
            Debug.WriteLine("Changed by server, update local...");
            player.SetPosition(e.position);
            player.SetState(e.state);
        }

        private void playerStateChanged(object sender, MPCControllerEventArgs e)
        {
            Info info = e.info;
            if (e.chnagedByUser)
            {
                Debug.WriteLine("Changed by user, sending to server...");
                client.Set(settings.Token, settings.UUID, info.FileName, info.Position, info.Duration, info.State);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                nameLabel.Content = info.FileName;
                positionLabel.Content = $"{info.Position:hh\\:mm\\:ss}/{info.Duration:hh\\:mm\\:ss}";
                statusLabel.Content = info.State;
            });
        }

        // Buttons
        private void hostButton_Click(object sender, RoutedEventArgs e)
        {
            bool succes = client.Connect(Dns.GetHostName(), 5000);
            if (succes)
            {
                client.Subscribe(settings.Token, settings.UUID);

                Info info = player.GetLastInfo();
                if(info != null)
                {
                    client.Set(settings.Token, settings.UUID, info.FileName, info.Position, info.Duration, info.State);
                }
                
            }
            else
            {
                MessageBox.Show("Can't connect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            //player.SetState(State.Paused);
            //player.SetPosition(new TimeSpan(0, 0, 20));
            Info info = player.GetLastInfo();
            client.Set(settings.Token, settings.UUID, info.FileName, info.Position +  TimeSpan.FromSeconds(10.5), info.Duration, State.Paused);
        }
    }
}
