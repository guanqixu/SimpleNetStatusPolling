using System.Collections.Generic;
using System.Net;

namespace SimpleNetStatusPolling
{
    /// <summary>
    /// 网络轮询服务
    /// </summary>
    public class NetStatusPollingService : PollingService<IPEndPoint, bool>
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eps">网络终结点集合</param>
        /// <param name="interval">两轮轮询的时间间隔</param>
        /// <param name="timeout">每个终结点的尝试连接的超时时间，超出时间则认为网络不通</param>
        /// <param name="notifyCount">满足指定数量后，调用 PollingNotifyEvent 返回最新结果</param>
        public NetStatusPollingService(List<IPEndPoint> eps, int interval = 60 * 1000, int timeout = 1000, int notifyCount = 10) : base(eps, interval, timeout, notifyCount)
        {
        }

        /// <summary>
        /// 每个轮询的具体工作内容
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected override bool PollingDetailWork(IPEndPoint obj)
        {
            return NetworkHelper.IsOnline(obj, _timeout);
        }
    }

}
