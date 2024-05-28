using NATS.Client;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Data;
using System.Text;

namespace EventsLogger
{

    public class MessageModel
    {
        public string Id { get; set; }
        public List<int> ProcessesTimes { get; set; }
    }

    class EventsLogger
    {
        private static readonly ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        private static readonly IDatabase db = ConnectionMultiplexer.Connect("127.0.0.1:6379").GetDatabase();
        private static readonly IConnection natsConnection = new ConnectionFactory().CreateConnection("127.0.0.1:4222");
        private static Dictionary<string,List<int>> _processesTimes = [];

        static void Main(string[] args)
        {

            natsConnection.SubscribeAsync("event", (sender, args) =>
            {
                var messageBytes = args.Message.Data;
                var messageObject = JsonConvert.DeserializeObject<MessageModel>(Encoding.UTF8.GetString(messageBytes));
                string id = messageObject.Id;
                List<int> times = messageObject.ProcessesTimes;

                _processesTimes.Add(id, times);

            });



            while (true)
            {
                try
                {
                    Console.WriteLine("enter \"true\" for save");
                    bool saveToDB = bool.Parse(Console.ReadLine());
                    if (saveToDB)
                    {
                        string id = connection.GetServer("127.0.0.1:6379").Keys(pattern: "*").Count().ToString();

                        string timesStr = JsonConvert.SerializeObject(_processesTimes);
                        db.StringSetAsync(id, timesStr);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

        }
    }
}