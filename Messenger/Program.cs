using NATS.Client;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Text;

namespace Messenger
{

    public class MessageModel
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<int> ProcessesTimes { get; set; }
    }

    public class Action
    {
        public string Id { get; set; }
        public List<int> ProcessesTimes { get; set; }
    }

    class Messanger
    {
        private static readonly IDatabase db = ConnectionMultiplexer.Connect("127.0.0.1:6379").GetDatabase();
        private static readonly IConnection natsConnection = new ConnectionFactory().CreateConnection("127.0.0.1:4222");
        private static List<int> _processesTimes = [0,0,0];

        private static int _id;

        static void Main(string[] args)
        {
            _id = int.Parse(args[0]);
            natsConnection.SubscribeAsync("message" + args[0], (sender, args) =>
            {
                var messageBytes = args.Message.Data;
                var messageObject = JsonConvert.DeserializeObject<MessageModel>(Encoding.UTF8.GetString(messageBytes));
                string id = messageObject.Id;
                List<int> times = messageObject.ProcessesTimes;
                Console.WriteLine($"Получено сообщение {messageObject.Text} от процесса с id {id}");

                for (int i = 0; i < _processesTimes.Count; i++)
                {
                    _processesTimes[i] = int.Max(_processesTimes[i], times[i]);
                }
                _processesTimes[_id]++;
                Action action = new()
                {
                    Id = _id.ToString(),
                    ProcessesTimes = _processesTimes
                };
                string actionMessage = JsonConvert.SerializeObject(action);
                messageBytes = Encoding.UTF8.GetBytes(actionMessage);
                db.StringSetAsync("e"+ _id.ToString() + "_" + _processesTimes[_id].ToString(), actionMessage);
            });

            Console.WriteLine($"Процесс с id {_id} готов");

            while (true)
            {
                Console.WriteLine("Введите id процесса, куда отправить и само сообщение в следующем формате: <номер процесса> <сообщение>");
                string input = Console.ReadLine();

                string[] parts = input.Split(' ');

                if(parts.Length < 2 || !int.TryParse(parts[0], out int tempId)) 
                {
                    Console.WriteLine("не правльный формат ввода");
                    continue;
                }

                if(tempId == _id)
                {
                    Console.WriteLine("нельзя отправить сообщение себе");
                    continue;
                }

                string id = parts[0];

                string restOfString = string.Join(" ", parts, 1, parts.Length - 1);

                _processesTimes[_id]++;
                MessageModel mess = new()
                {
                    Id = _id.ToString(),
                    Text = restOfString,
                    ProcessesTimes = _processesTimes

                };
                string messMessage = JsonConvert.SerializeObject(mess);

                var messageBytes = Encoding.UTF8.GetBytes(messMessage);
                natsConnection.Publish("message" + id, messageBytes);

                Action action = new()
                {
                    Id = _id.ToString(),
                    ProcessesTimes = _processesTimes
                };
                string actionMessage = JsonConvert.SerializeObject(action);
                db.StringSetAsync("e" + _id.ToString() + "_" + _processesTimes[_id].ToString(), actionMessage);

            };
        }
    }
}