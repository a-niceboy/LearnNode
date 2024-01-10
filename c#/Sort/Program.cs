using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sort
{
    class Program
    {
        static void Main(string[] args)
        {
            MergeSort mergeSort = new MergeSort();
            int[] n = new int[] { 1, 2, 3, 4, 1, 2, 3, 4 };
            mergeSort.Soft(n);

            Console.ReadLine();
        }
    }
}
