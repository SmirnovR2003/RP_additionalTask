
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Messenger
{

    public class Action
    {
        public string Id { get; set; }
        public List<int> ProcessesTimes { get; set; }

        public static bool operator==(Action a, Action b)
        {
            if (a.ProcessesTimes.Count != b.ProcessesTimes.Count) 
                return false;

            for(int i = 0; i < a.ProcessesTimes.Count;i++)
            {
                if (a.ProcessesTimes[i] != b.ProcessesTimes[i]) return false;
            }
            return true;
        }
        public static bool operator!=(Action a, Action b)
        {
            return !(a == b);
        }

        public static bool operator<=(Action a, Action b)
        {
            for (int i = 0; i < a.ProcessesTimes.Count; i++)
            {
                if (a.ProcessesTimes[i] > b.ProcessesTimes[i])
                    return false;
            }
            return true;
        }

        public static bool operator<(Action a, Action b)
        {
            bool haveLess = false;

            for (int i = 0; i < a.ProcessesTimes.Count; i++)
            {
                if (a.ProcessesTimes[i] < b.ProcessesTimes[i])
                {
                    haveLess = true;
                }
            }
            return a <= b && haveLess;
        }

        public static bool operator>=(Action a, Action b)
        {
            return !(a < b);
        }

        public static bool operator>(Action a, Action b)
        {
            return !(a <= b);

        }
    }

    class Messanger
    {
        private static readonly ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        private static readonly IDatabase db = ConnectionMultiplexer.Connect("127.0.0.1:6379").GetDatabase();

        static void Main(string[] args)
        {
           

            while (true)
            {
                List<string> past = [];
                List<string> future = [];
                List<string> parallel = [];
                Console.WriteLine("Введите id процесса и номер события: <номер процесса> <номер события>");
                string input = Console.ReadLine();

                string[] parts = input.Split(' ');

                if (parts.Length != 2 || !int.TryParse(parts[0], out int tempProcessId) || !int.TryParse(parts[1], out int tempActionId))
                {
                    Console.WriteLine("не правльный формат ввода");
                    continue;
                }
                var currAction = JsonConvert.DeserializeObject<Action>(db.StringGet("e" + tempProcessId.ToString() + "_" + tempActionId.ToString()));

                foreach (string? key in connection.GetServer("127.0.0.1:6379").Keys(pattern:"*"))
                {
                    string? tesxtByDB = db.StringGet(key);
                    var action = JsonConvert.DeserializeObject<Action>(tesxtByDB);

                    if (action == currAction) continue;

                    if (action < currAction) 
                    {
                        past.Add(key);
                        continue;
                    }

                    if(currAction <  action)
                    {
                        future.Add(key);
                        continue;
                    }
                    parallel.Add(key);
                }

                Console.WriteLine("past: " + string.Join(", ", past));
                Console.WriteLine("future: " + string.Join(", ", future));
                Console.WriteLine("parallel: " + string.Join(", ", parallel));



            };
        }
    }
}