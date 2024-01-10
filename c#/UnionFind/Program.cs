using System;
using System.IO;

namespace union_find
{
    class Tick
    {
        public Tick()
        {
            time = DateTime.Now;
        }
        public void tick()
        {
            last = time;
            time = DateTime.Now;
            TimeSpan ts = time.Subtract(last);
            Console.WriteLine("time: " + ts.TotalMilliseconds);
        }
        DateTime time;
        DateTime last;
    }

    class Program
    {
        const string mediumUFPath = "C:/Users/15720/Documents/c#/union-find/mediumUF.txt";
        const string largeUFPath = "C:/Users/15720/Documents/c#/union-find/largeUF.txt";
        static void Main(string[] args)
        {
            Tick tick = new Tick();
            string path = largeUFPath;
            string[] lines = File.ReadAllLines(path);
            tick.tick();
            int n = int.Parse(lines[0]);
            UFWeightQuickUnion uF = new UFWeightQuickUnion(n);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] splits = line.Split(' ');
                int p = int.Parse(splits[0]);
                int q = int.Parse(splits[1]);
                uF.Union(p, q);
            }

            Console.WriteLine("uF: " + uF.count);
            tick.tick();
            Console.ReadLine();
        }
    }
}
