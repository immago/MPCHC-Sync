using System;
using System.Windows;
using System.Diagnostics;
using System.Net;
using Microsoft.Win32;
using System.IO;

namespace MPCHC_Sync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MPCController player;
        private Client client;
        private Process mpcProceess;

        public MainWindow()
        {
            InitializeComponent();

            if (!Settings.IsConfigured())
            {
                SettingsWindow settingsDialog = new SettingsWindow();
                settingsDialog.ShowDialog();
            }


            client = new Client();
            client.videoStateChanged += clientVideoStateChanged;
            client.connectionStateChanged += clientConnectionStateChanged;
            client.onError += clientOnError;
            player = new MPCController();
            player.stateChanged += playerStateChanged;
            player.initialized += playerInitialized;

            // Not connected
            disconnectGrid.Visibility = Visibility.Hidden;
            this.IsEnabled = false;

            // Run MPC
            mpcProceess = Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "lib/mpc-hc64/mpc-hc64.exe"));
            mpcProceess.EnableRaisingEvents = true;
            mpcProceess.Exited += mpcProceessExited;

            // Window position
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2) + 210;

        }

        // On error in server
        private void clientOnError(object sender, ClientErrorEventArgs e)
        {
            client.Disconnect();
            MessageBox.Show($"{e.description} ({e.code.ToString()})", "Error from server", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // On MPC-HC web-ui connected
        private void playerInitialized(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.IsEnabled = true;
                nameLabel.Content = "";
            });
        }

        // On MPC-HC exit
        private void mpcProceessExited(object sender, EventArgs e)
        {
            client.Disconnect();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }

        // On server connect / disconnect
        private void clientConnectionStateChanged(object sender, ClientConnectionEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                disconnectGrid.Visibility = (e.state == ConnectionState.Disconnected) ? Visibility.Hidden : Visibility.Visible;
            });
        }

        // On change video state by server
        private void clientVideoStateChanged(object sender, ClientVideoEventArgs e)
        {
            Debug.WriteLine("Changed by server, update local...");

            // if new file, try find in same dir
            Info info = player.GetInfo();
            if (info.FileName != e.file)
            {
                // if some file opened
                if (info.FileDir.Length > 0)
                {
                    string newPath = Path.Combine(info.FileDir, e.file);
                    if (File.Exists(newPath))
                    {
                        player.OpenFile(newPath);
                    }
                    else
                    {
                        MessageBox.Show($"Please select {e.file}", "Open file", MessageBoxButton.OK, MessageBoxImage.Information);
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = $"{e.file}|{e.file}|All files (*.*)|*.*";
                        
                        if (openFileDialog.ShowDialog() == true)
                        {
                            string filePath = openFileDialog.FileName;

                            // Open file
                            player.OpenFile(filePath);
                            client.Get(Settings.Token, client.subscribedSessionIdentifer);
                        }
                    }

                }
            }

            player.SetPosition(e.position);
            player.SetState(e.state);
        }

        // On change video state by client
        private void playerStateChanged(object sender, MPCControllerEventArgs e)
        {
            Info info = e.info;
            if (e.chnagedByUser)
            {
                Debug.WriteLine("Changed by user, sending to server...");
                client.Set(Settings.Token, Settings.UUID, info.FileName, info.Position, info.Duration, info.State);
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

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video files (*.mkv;*.webm;*.avi;*.mov;*.wmv;*.mp4;*.mpg;*.mpeg)|*.mkv;*.webm;*.avi;*.mov;*.wmv;*.mp4;*.mpg;*.mpeg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                // Open file
                player.OpenFile(filePath);
                Info info = player.GetInfo();

                // Connect to server
                bool succes = client.Connect(Settings.Host, Settings.Port, true);
                if (succes)
                {
                    client.Subscribe(Settings.Token, Settings.UUID, true);
                    connectedAddressLabel.Content = client.subscribedSessionIdentifer;
                    client.Set(Settings.Token, Settings.UUID, info.FileName, info.Position, info.Duration, info.State);
                }
                else
                {
                    MessageBox.Show("Can't connect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                } 
            }
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            string sessionId = connectAddressTextBox.Text;
            if (sessionId.Length != Settings.UUID.Length)
            {
                MessageBox.Show("Wrong session id size", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Connect to server
            bool succes = client.Connect(Settings.Host, Settings.Port, false);
            if (succes)
            {
                client.Subscribe(Settings.Token, sessionId);
                connectedAddressLabel.Content = client.subscribedSessionIdentifer;
            }
            else
            {
                MessageBox.Show("Can't connect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void disconnectButton_Click(object sender, RoutedEventArgs e)
        {
            client.Disconnect();
        }

        private void copyConnectedAddressButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(connectedAddressLabel.Content.ToString());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            client.videoStateChanged -= clientVideoStateChanged;
            client.connectionStateChanged -= clientConnectionStateChanged;
            client.onError -= clientOnError;
            player.stateChanged -= playerStateChanged;
            player.initialized -= playerInitialized;

            client.Disconnect();
            if (!mpcProceess.HasExited) { 
                mpcProceess.CloseMainWindow();
            }
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsDialog = new SettingsWindow();
            settingsDialog.ShowDialog();
        }
    }
}
