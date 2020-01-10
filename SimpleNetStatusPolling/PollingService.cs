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
        private List<IPEndPoint> _pollingEndPoints;

        public PollingService(List<IPEndPoint> eps)
        {
            _pollingEndPoints = eps;
        }

        private int _pollingIndex;

        public event Action<IPEndPoint, bool> PollingProcessing;

        public event Action<Dictionary<IPEndPoint, bool>> PollingFinished;

        private Dictionary<IPEndPoint, bool> _pollingResult = new Dictionary<IPEndPoint, bool>();

        private CancellationTokenSource _tokenSource;

        public int _maxTaskCount = 5;

        private bool _isRunning = false;


        public void Start()
        {
            if (_isRunning)
                return;
            _isRunning = true;
            _finishTaskCount = 0;
            _pollingIndex = -1;
            _pollingResult.Clear();
            List<Task> tasks = new List<Task>();

            _tokenSource = new CancellationTokenSource();

            for (int i = 0; i < _maxTaskCount; i++)
            {
                CreateTaskAndStart();
            }

        }

        public void Stop()
        {
            if (_isRunning)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _isRunning = false;
            }
        }

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
