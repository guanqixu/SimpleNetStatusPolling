using SimpleNetStatusPolling;
using System;
using System.Collections.Generic;
using System.Net;

namespace SimpleNetStatusPollingDemo
{
    class Program
    {
        /// <summary>
        /// 主程序
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            List<IPEndPoint> ipes = new List<IPEndPoint>();
            for (int i = 1; i < 200; i++)
            {
                var ipe = new IPEndPoint(IPAddress.Parse($"14.215.177.{i}"), 80);
                ipes.Add(ipe);
            }

            NetStatusPollingService service = new NetStatusPollingService(ipes);
            service.PollingProgressing += Service_PollingProgressing;
            service.PollingFinished += Service_PollingFinished;
            service.PollingPeriodResultNotifyEvent += Service_PollingPeriodResultNotifyEvent;

            service.Start();
            string input;
            do
            {
                input = Console.ReadLine();
                if (input == "s")
                {
                    service.Start(2000, 10);
                }
                else if (input == "e")
                {
                    service.Stop();
                }
                else if (string.IsNullOrWhiteSpace(input))
                {
                    service.Stop();
                    break;
                }

            } while (input != "");
        }

        private static void Service_PollingPeriodResultNotifyEvent(Dictionary<IPEndPoint, bool> obj)
        {
            Console.WriteLine("Period! " + obj.Count);
            foreach (var item in obj)
            {
                Console.WriteLine($"{item.Key}, {item.Value}");
            }
        }

        /// <summary>
        /// 轮询完成，返回所有结果
        /// </summary>
        /// <param name="obj"></param>
        private static void Service_PollingFinished(Dictionary<IPEndPoint, bool> obj)
        {
            Console.WriteLine("Finished! " + obj.Count);
            foreach (var item in obj)
            {
                Console.WriteLine($"{item.Key}, {item.Value}");
            }
        }

        /// <summary>
        /// 单次轮询，返回每次的结果
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void Service_PollingProgressing(IPEndPoint arg1, bool arg2)
        {
            Console.WriteLine($"Processing! {arg1}, {arg2}");
        }
    }
}
