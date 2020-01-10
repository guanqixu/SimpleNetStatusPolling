using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleNetStatusPolling
{
    public class PollingService
    {
        private int _pollingInterval;

        private int _timeout;

        private List<IPEndPoint> _pollingEndPoints;

        public PollingService(List<IPEndPoint> eps, int interval = 60 * 1000, int timeout = 1000)
        {
            _pollingEndPoints = eps;
            _pollingInterval = interval;
            _timeout = timeout;
        }

        private int _pollingIndex;

        public event Action<IPEndPoint, bool> PollingProcessing;

        public event Action<Dictionary<IPEndPoint, bool>> PollingFinished;

        private Dictionary<IPEndPoint, bool> _pollingResult = new Dictionary<IPEndPoint, bool>();

        private CancellationTokenSource _tokenSource;

        public int _maxTaskCount = 5;

        private CancellationTokenSource _pollingCancel;

        public void Start()
        {
            if (_pollingCancel != null)
                return;
            _pollingCancel = new CancellationTokenSource();

            new Thread(obj =>
            {
                var cancel = obj as CancellationTokenSource;
                while (!cancel.IsCancellationRequested)
                {
                    var semaphoreSlim = new SemaphoreSlim(_maxTaskCount);
                    var countdown = new CountdownEvent(_pollingEndPoints.Count);
                    Dictionary<IPEndPoint, bool> pollingResult = new Dictionary<IPEndPoint, bool>();

                    foreach (var ipe in _pollingEndPoints)
                    {
                        semaphoreSlim.Wait();
                        if (!cancel.IsCancellationRequested)
                        {
                            var task = new Task(obj1 =>
                            {
                                var cancel1 = obj1 as CancellationTokenSource;
                                if (cancel1.IsCancellationRequested)
                                    return;
                                var ep = ipe;
                                var result = NetworkHelper.IsOnline(ep, _timeout);
                                PollingProcessing?.Invoke(ep, result);
                                Console.WriteLine(ep);
                                pollingResult.Add(ep, result);
                                countdown.Signal();
                                semaphoreSlim.Release();
                            }, cancel, cancel.Token);
                            task.Start();
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (!cancel.IsCancellationRequested)
                    {
                        countdown.Wait();
                        PollingFinished?.Invoke(pollingResult);
                    }

                    Thread.Sleep(_pollingInterval);
                }

            }).Start(_pollingCancel);


        }

        public void Start(int interval, int timeout)
        {
            _pollingInterval = interval;
            _timeout = timeout;
            Start();


        }

        public void Stop()
        {
            if (_pollingCancel != null)
            {
                _pollingCancel.Cancel();
                _pollingCancel.Dispose();
                _pollingCancel = null;
            }
        }

        //public void Stop()
        //{
        //    if (_isRunning)
        //    {
        //        _tokenSource.Cancel();
        //        _tokenSource.Dispose();
        //        _isRunning = false;
        //    }
        //}

        public void CreateTaskAndStart()
        {
            int index = Interlocked.Increment(ref _pollingIndex);
            var cancel = _tokenSource;
            if (index < _pollingEndPoints.Count && !cancel.IsCancellationRequested)
            {
                var task = new Task((obj) =>
                {
                    var tokenSource = obj as CancellationTokenSource;
                    if (tokenSource.IsCancellationRequested)
                        return;
                    var ep = _pollingEndPoints[index];
                    var result = NetworkHelper.IsOnline(ep, 1);
                    PollingProcessing?.Invoke(ep, result);
                    _pollingResult.Add(ep, result);
                }, cancel, cancel.Token);
                task.ContinueWith(t => CreateTaskAndStart());
                task.Start();
            }
            else
            {
                TaskFinished();
            }
        }

        private int _finishTaskCount;

        private void TaskFinished()
        {
            int count = Interlocked.Increment(ref _finishTaskCount);
            if (count == _maxTaskCount)
            {
                PollingFinished(_pollingResult);
            }
        }
    }

}
