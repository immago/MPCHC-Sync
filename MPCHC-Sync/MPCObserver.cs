using MPC_HC.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MPCHC_Sync
{


    public class MPCControllerEventArgs : EventArgs
    {
        public Info info { get; set; }
        public bool chnagedByUser { get; set; }
    }

    class MPCController
    {

        public event EventHandler<MPCControllerEventArgs> dataChanged;
        private MPCHomeCinema player;
        private Info previousInfo;
        private TimeSpan updateInterval;

        public MPCController()
        {
            player = new MPCHomeCinema("http://localhost:13579");
            updateInterval = new TimeSpan(0, 0, 1);
            RunUpdate();
        }

        private void RunUpdate()
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            Task.Run(async () =>
            {
                await RunUpdateTask(updateInterval, token);
            }).GetAwaiter();
        }

        private async Task RunUpdateTask(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var task = await player.GetInfo();

                // Send event
                if(!IsInfoEqual(previousInfo, task))
                {
                    EventHandler<MPCControllerEventArgs> handler = dataChanged;
                    if (handler != null)
                    {
                        MPCControllerEventArgs args = new MPCControllerEventArgs();
                        args.info = task;
                        args.chnagedByUser = IsChangedByUser(previousInfo, task, updateInterval);
                        handler(this, args);
                    }

                }
                previousInfo = task;
                await Task.Delay(interval, cancellationToken);
            }
        }

        private bool IsInfoEqual(Info info1, Info info2)
        {
            if (info1 == null || info2 == null)
                return false;

            return   (info1.State == info2.State)       &&
                     (info1.Position == info2.Position) &&
                     (info1.Duration == info2.Duration) &&
                     (info1.FileName == info2.FileName);
        }

        private bool IsChangedByUser(Info previous, Info current, TimeSpan timePassed)
        {

            if (previous == null || current == null)
                return false;

            // state changed
            if (previous.State != current.State)
            {
                return true;
            }

            // file changed
            if (previous.FileName != current.FileName)
            {
                return true;
            }

            // position changed
            if (current.State == State.Playing)
            {
                TimeSpan difference = current.Position - previous.Position;
                double maxError = 500; // ms 
                if(Math.Abs((difference - timePassed).TotalMilliseconds) > maxError) // user move position
                {
                    return true;
                }
            }

            return false;
        }

    }
}
