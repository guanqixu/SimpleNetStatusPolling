using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleNetStatusPolling
{
    /// <summary>
    /// 轮询服务
    /// </summary>
    /// <typeparam name="T_Object"></typeparam>
    /// <typeparam name="T_Result"></typeparam>
    public abstract class PollingService<T_Object, T_Result> : IPollingService<T_Object, T_Result>
    {
        /// <summary>
        /// 两个轮询间的间隔时间
        /// </summary>
        protected int _pollingInterval;

        /// <summary>
        /// 超时时间
        /// </summary>
        protected int _timeout;

        /// <summary>
        /// 进行通知的数量
        /// </summary>
        protected int _notifyCount;

        /// <summary>
        /// 最大线程数
        /// </summary>
        public int _maxThreadCount = 5;

        /// <summary>
        /// 轮询开关
        /// </summary>
        private CancellationTokenSource _pollingCancel;

        /// <summary>
        /// 要轮询的对象
        /// </summary>
        private List<T_Object> _pollingObjects;

        /// <summary>
        /// 轮询过程中的事件
        /// </summary>
        public event Action<T_Object, T_Result> PollingProgressing;

        /// <summary>
        /// 轮询完成后的事件
        /// </summary>
        public event Action<Dictionary<T_Object, T_Result>> PollingFinished;

        /// <summary>
        /// 在指定时间后进行结果返回
        /// </summary>
        public event Action<Dictionary<T_Object, T_Result>> PollingNotifyEvent;

        /// <summary>
        /// 轮询的详细工作
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected abstract T_Result PollingDetailWork(T_Object obj);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="interval"></param>
        /// <param name="timeout"></param>
        public PollingService(List<T_Object> objects, int interval = 1000 * 60, int timeout = 1000, int notifyCount = 10)
        {
            _pollingObjects = objects;
            _pollingInterval = interval;
            _notifyCount = notifyCount;
            _timeout = timeout;
        }

        /// <summary>
        /// 更改轮询对象
        /// </summary>
        /// <param name="objects"></param>
        public void ChangePollingObjects(List<T_Object> objects)
        {
            if (_pollingCancel != null)
            {
                Stop();

                _pollingObjects = objects;

                Start();
            }

            else
            {
                _pollingObjects = objects;
            }

        }

        /// <summary>
        /// 开始
        /// </summary>
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
                    var semaphoreSlim = new SemaphoreSlim(_maxThreadCount);
                    var countdown = new CountdownEvent(_pollingObjects.Count);
                    Dictionary<T_Object, T_Result> pollingResult = new Dictionary<T_Object, T_Result>();

                    List<Tuple<T_Object, T_Result>> notifyResult = new List<Tuple<T_Object, T_Result>>();

                    foreach (var ipe in _pollingObjects)
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
                                var result = PollingDetailWork(ep);
                                PollingProgressing?.Invoke(ep, result);
                                lock (((ICollection)pollingResult).SyncRoot)
                                {
                                    pollingResult.Add(ep, result);
                                    notifyResult.Add(new Tuple<T_Object, T_Result>(ep, result));
                                    if (notifyResult.Count >= _notifyCount)
                                    {
                                        PollingNotifyEvent.Invoke(notifyResult.ToDictionary(t => t.Item1, t => t.Item2));
                                        notifyResult.Clear();
                                    }
                                }
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
                        if (notifyResult.Count > 0)
                        {
                            PollingNotifyEvent.Invoke(notifyResult.ToDictionary(t => t.Item1, t => t.Item2));
                            notifyResult.Clear();
                        }
                        PollingFinished?.Invoke(pollingResult);
                    }
                    Thread.Sleep(_pollingInterval);
                }
            })
            { IsBackground = true }.Start(_pollingCancel);
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            if (_pollingCancel != null)
            {
                _pollingCancel.Cancel();
                _pollingCancel.Dispose();
                _pollingCancel = null;
            }
        }

        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="timeout"></param>
        public void Start(int interval, int timeout)
        {
            if (_pollingCancel == null)
            {
                _pollingInterval = interval;
                _timeout = timeout;
                Start();
            }
        }

        /// <summary>
        /// 更改时间
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="timeout"></param>
        public void ChangeTime(int interval, int timeout)
        {
            _pollingInterval = interval;
            _timeout = timeout;
        }

    }

    /// <summary>
    /// 轮询服务接口
    /// </summary>
    public interface IPollingService
    {
        void Start();

        void Stop();

        void Start(int interval, int timeout);

        void ChangeTime(int interval, int timeout);
    }

    /// <summary>
    /// 轮询服务接口
    /// </summary>
    /// <typeparam name="T_Object"></typeparam>
    /// <typeparam name="T_Result"></typeparam>
    public interface IPollingService<T_Object, T_Result> : IPollingService
    {
        void ChangePollingObjects(List<T_Object> objects);

        event Action<T_Object, T_Result> PollingProgressing;

        event Action<Dictionary<T_Object, T_Result>> PollingFinished;

        event Action<Dictionary<T_Object, T_Result>> PollingNotifyEvent;

    }




}
