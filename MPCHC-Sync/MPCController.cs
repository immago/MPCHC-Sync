using MPC_HC.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.IO;


namespace MPCHC_Sync
{


    public class MPCControllerEventArgs : EventArgs
    {
        public Info info { get; set; }
        public bool chnagedByUser { get; set; }
    }

    class MPCController
    {

        public event EventHandler<MPCControllerEventArgs> stateChanged;
        public event EventHandler<EventArgs> initialized;

        private MPCHomeCinema player;
        private Info previousInfo;
        private TimeSpan updateInterval;
        CancellationTokenSource cancelTokenSource;

        public MPCController()
        {
            player = new MPCHomeCinema(Settings.MPCWebUIAddress);
            updateInterval = new TimeSpan(0, 0, 1);

            // Wait connection
            Task.Run(async () =>
            {
                var task = await player.GetInfo();
                initialized?.Invoke(this, new EventArgs());

            }).GetAwaiter();

            // Update normaly
            RunUpdate();
        }

        // Observing

        public Info GetLastInfo()
        {
            return previousInfo;

        }

        public Info GetInfo()
        {
            Info info = null;
            Task.Run(async () =>
            {
                info = await player.GetInfo();
            }).GetAwaiter().GetResult();
            return info;
        }

        private void RunUpdate()
        {
            if(cancelTokenSource != null && !cancelTokenSource.IsCancellationRequested)
            {
                Debug.WriteLine("alrady runned");
                return;
            }

            cancelTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancelTokenSource.Token;

            Task.Run(async () =>
            {
                await RunUpdateTask(updateInterval, cancellationToken);
            }).GetAwaiter();
        }

        private void StopUpdate()
        {
            previousInfo = null;
            cancelTokenSource.Cancel();
        }

        private async Task RunUpdateTask(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var task = await player.GetInfo();

                // Send event
                if(!IsInfoEqual(previousInfo, task))
                {
                    EventHandler<MPCControllerEventArgs> handler = stateChanged;
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
            if (current.State == State.Playing || current.State == State.Paused)
            {
                TimeSpan difference = current.Position - previous.Position;
                if(current.State == State.Playing)
                {
                    difference -= timePassed;
                }
                double maxError = 500; // ms 
                if(Math.Abs(difference.TotalMilliseconds) > maxError) // user move position
                {
                    return true;
                }
            }

            return false;
        }

        // Control

        public void SetState(State state)
        {

            if(previousInfo != null && previousInfo.State == state)
            {
                return;
            }

            StopUpdate();
            Task.Run(async () =>
            {
                Result task;
                switch (state)
                {
                    case State.Playing:
                        task = await player.PlayAsync();
                        break;
                    case State.Paused:
                        task = await player.PauseAsync();
                        break;
                    case State.Stoped:
                        task = await player.StopAsync();
                        break;
                    default:
                        task = await player.StopAsync();
                        break;
                }
            }).GetAwaiter().GetResult();
            RunUpdate();
        }

        public void SetPosition(TimeSpan position)
        {

            if (previousInfo != null)
            {
                TimeSpan difference = position - previousInfo.Position;
                double maxError = 500 + updateInterval.TotalMilliseconds; // ms 
                if (Math.Abs(difference.TotalMilliseconds) < maxError) // not needed
                {
                    return;
                }
            }

            StopUpdate();
            Task.Run(async () =>
            {
                // Fix: MPCHomeCinema class dont support steps lower then 1 sec
                TimeSpan ts = TimeSpan.FromSeconds(Math.Round(position.TotalSeconds));
                Result task = await player.SetPosition(ts);
                
            }).GetAwaiter().GetResult();
            RunUpdate();
        }

        public void OpenFile(string path)
        {
            if (previousInfo != null && previousInfo.FileName == Path.GetFileName(path))
            {
                return;
            }

            StopUpdate();
            Task.Run(async () =>
            {
                // TODO: check file open
                Result openResult = await player.OpenFileAsync(path);
                Thread.Sleep(500);
                Result pauseResult = await player.PauseAsync();
                Thread.Sleep(500);

            }).GetAwaiter().GetResult();
            RunUpdate();
        }

    }
}
