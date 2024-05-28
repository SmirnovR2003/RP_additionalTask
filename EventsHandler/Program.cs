
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
            return b <= a;
        }

        public static bool operator>(Action a, Action b)
        {
            return b < a;

        }
    }

    class Messanger
    {
        private static readonly ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("127.0.0.1:6379");
        private static readonly IDatabase db = ConnectionMultiplexer.Connect("127.0.0.1:6379").GetDatabase();

        static void Main(string[] args)
        {
           
            //сделать без очищения базы(доп)
            //сделать внутренние события
            while (true)
            {
                List<string> past = [];
                List<string> future = [];
                List<string> parallel = [];
                Console.WriteLine("Введите id процесса и номер события: <номер сессии> <номер процесса> <номер события>");
                string input = Console.ReadLine();

                string[] parts = input.Split(' ');

                if (parts.Length != 3 
                    || !int.TryParse(parts[0], out int tempSessionId) 
                    || !int.TryParse(parts[1], out int tempProcessId) 
                    || !int.TryParse(parts[2], out int tempActionId))
                {
                    Console.WriteLine("не правльный формат ввода");
                    continue;
                }

                Dictionary<string, List<int>> times = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(db.StringGet(tempSessionId.ToString()));

                Action currAction = new()
                {
                    ProcessesTimes = times["e" + tempProcessId.ToString() + "_" + tempActionId.ToString()]
                };

                foreach (var key in times)
                {
                    Action action = new()
                    {
                        ProcessesTimes = key.Value
                    };

                    if (action == currAction) continue;

                    if (action < currAction) 
                    {
                        past.Add(key.Key);
                        continue;
                    }

                    if(currAction <  action)
                    {
                        future.Add(key.Key);
                        continue;
                    }
                    parallel.Add(key.Key);
                }

                Console.WriteLine("past: " + string.Join(", ", past));
                Console.WriteLine("future: " + string.Join(", ", future));
                Console.WriteLine("parallel: " + string.Join(", ", parallel));



            };
        }
    }
}