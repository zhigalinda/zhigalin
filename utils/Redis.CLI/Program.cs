using System;
using System.IO;
using StackExchange.Redis;

namespace Redis.CLI
{
    static class Program
    {
        private static IDatabase _db;
        private static IConnectionMultiplexer _redis;

        private enum Actions
        {
            Read,
            Write
        }

        //0 action
        //1 redis connection string
        //2 service name
        //3 local service json file path
        static int Main(string[] args)
        {
            if (!ValidateArgs(args))
                return -1;

            var action = Enum.Parse<Actions>(args[0], true);

            try
            {
                _redis = ConnectionMultiplexer.Connect(args[1]);
                _db = _redis.GetDatabase(0);
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection error: {0}", e);
                return -1;
            }

            var serviceName = args[2];

            var serviceConfigFile = Path.GetFullPath(args[3]);

            switch (action)
            {
                case Actions.Read:
                    Read(serviceName, serviceConfigFile);
                    break;
                case Actions.Write:
                    Write(serviceName, serviceConfigFile);
                    break;
                default:
                    throw new NotSupportedException("Incorrect action value");
            }

            return 0;
        }

        private static void Read(string serviceName, string serviceConfigFile)
        {
            using var streamWriter = new StreamWriter(serviceConfigFile);
            streamWriter.WriteLine(_db.StringGet(serviceName));
            Console.WriteLine("OK");
        }

        private static void Write(string serviceName, string serviceConfigFile)
        {
            try
            {
                using var streamReader = new StreamReader(serviceConfigFile);
                var text = streamReader.ReadToEnd();
                _db.StringSet(serviceName, text);
                TriggerSubscribers(serviceName);
                Console.WriteLine("OK");
            }
            catch (Exception e)
            {
                Console.WriteLine("Writing error: {0}", e);
            }
        }

        private static bool ValidateArgs(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Invalid arguments number");
                return false;
            }

            return true;
        }

        private static void TriggerSubscribers(string serviceId)
        {
            var subscriber = _redis.GetSubscriber();
            subscriber.Publish(GetChannelName(serviceId), "Data has been changed");
        }

        private static string GetChannelName(string serviceId)
        {
            return "__keyspace@0__:" + serviceId;
        }
    }
}