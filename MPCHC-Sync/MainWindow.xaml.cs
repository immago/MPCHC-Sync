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
        //private MPCHomeCinema player;
        private MPCController player;
        private Settings settings;
        private Client client;

        public MainWindow()
        {
            InitializeComponent();

            settings = new Settings();
            client = new Client();
            player = new MPCController();
            player.dataChanged += dataChanged;


            //Debug.WriteLine(settings.UUID);
            //Debug.WriteLine(Dns.GetHostName());

            //client.Connect(Dns.GetHostName(), 5000);
            //client.Subscribe("86de0ff4-3115-4385-b485-b5e83ae6b890", "1234");
            //client.Set("86de0ff4-3115-4385-b485-b5e83ae6b890", "1234", "test.mp4", new TimeSpan(0, 0, 10), new TimeSpan(1, 10, 0), State.Playing);
            
        }

        private void dataChanged(object sender, MPCControllerEventArgs e)
        {

            if (e.chnagedByUser)
            {
                Debug.WriteLine("Changed by user");
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Info info = e.info;
                nameLabel.Content = info.FileName;
                positionLabel.Content = $"{info.Position:hh\\:mm\\:ss}/{info.Duration:hh\\:mm\\:ss}";
                statusLabel.Content = info.State;
            });
        }


        // Update methods
        //private void RunUpdate()
        //{
        //    CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        //    CancellationToken token = cancelTokenSource.Token;

        //    Task.Run(async () =>
        //    {
        //        await RunUpdateTask(new TimeSpan(0, 0, 1), token);
        //    }).GetAwaiter();
        //}

        //private async Task RunUpdateTask(TimeSpan interval, CancellationToken cancellationToken)
        //{
        //    while (!cancellationToken.IsCancellationRequested)
        //    {
        //        var task = await player.GetInfo();
        //        Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            nameLabel.Content = task.FileName;
        //            positionLabel.Content = $"{task.Position:hh\\:mm\\:ss}/{task.Duration:hh\\:mm\\:ss}";
        //            statusLabel.Content = task.State;
        //        });
        //        await Task.Delay(interval, cancellationToken);
        //    }
        //}



        // Buttons
        private void hostButton_Click(object sender, RoutedEventArgs e)
        {
            bool succes = client.Connect(Dns.GetHostName(), 5000);
            if (succes)
            {
                client.Subscribe(settings.Token, settings.UUID);
                client.Set(settings.Token, settings.UUID, "test.mp4", new TimeSpan(0, 0, 10), new TimeSpan(1, 10, 0), State.Playing);
            }
            else
            {
                MessageBox.Show("Can't connect", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
