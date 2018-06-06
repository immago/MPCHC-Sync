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
using System.Threading;

namespace MPCHC_Sync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MPCHomeCinema player;

        public MainWindow()
        {
            InitializeComponent();

            player = new MPCHomeCinema("http://localhost:13579");
            RunUpdate();
        }


        // Update methods
        private void RunUpdate()
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            Task.Run(async () =>
            {
                await RunUpdateTask(new TimeSpan(0, 0, 1), token);
            }).GetAwaiter();
        }

        private async Task RunUpdateTask(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var task = await player.GetInfo();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    nameLabel.Content = task.FileName;
                    positionLabel.Content = $"{task.Position:hh\\:mm\\:ss}/{task.Duration:hh\\:mm\\:ss}";
                    statusLabel.Content = task.State;
                });
                await Task.Delay(interval, cancellationToken);
            }
        }
    }
}
