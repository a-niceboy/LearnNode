
using System;

namespace rbtree
{
    class Program
    {
        static void Main(string[] args)
        {
            RBTree rBTree = new RBTree();
            rBTree.log = true;

            int testCount = 10;
            int[] keys = new int[testCount];

            Random rd = new Random();
            for (int i = 0; i < testCount; i++)
            {
                int key = rd.Next(0, 100);
                rBTree.Insert(key);
                rBTree.Print();
                keys[i] = key;
            }

            for (int i = 0; i < testCount; i++)
            {
                rBTree.Delete(keys[i]);
                rBTree.Print();
            }


            Console.WriteLine("finish");
            Console.ReadLine();
        }
    }
}
