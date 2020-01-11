using System.Collections.Generic;
using System.Net;

namespace SimpleNetStatusPolling
{
    /// <summary>
    /// 网络轮询服务
    /// </summary>
    public class NetStatusPollingService : PollingService<IPEndPoint, bool>
    {
        public NetStatusPollingService(List<IPEndPoint> eps, int interval = 60 * 1000, int timeout = 1000) : base(eps, interval, timeout)
        {
        }

        protected override bool PollingDetailWork(IPEndPoint obj)
        {
            return NetworkHelper.IsOnline(obj, _timeout);
        }
    }

}
