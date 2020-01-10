using SimpleNetStatusPolling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SimpleNetStatusPollingDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            List<IPEndPoint> ipes = new List<IPEndPoint>();
            for (int i = 1; i < 200; i++)
            {
                var ipe = new IPEndPoint(IPAddress.Parse($"192.168.1.{i}"), i);
                ipes.Add(ipe);
            }

            PollingService service = new PollingService(ipes);
            service.PollingProcessing += Service_PollingProcessing;
            service.PollingFinished += Service_PollingFinished;

            service.Start();
            string input;
            do
            {
                input = Console.ReadLine();
                if (input == "s")
                {
                    service.Start();
                }
                else if (input == "e")
                {
                    service.Stop();
                }
            } while (input != "1");





        }

        private static void Service_PollingFinished(Dictionary<IPEndPoint, bool> obj)
        {
            Console.WriteLine("Finished!");
            foreach (var item in obj)
            {
                Console.WriteLine($"{item.Key}, {item.Value}");
            }
        }

        private static void Service_PollingProcessing(IPEndPoint arg1, bool arg2)
        {
            Console.WriteLine("Processing!");
            Console.WriteLine($"{arg1}, {arg2}");
        }
    }
}
