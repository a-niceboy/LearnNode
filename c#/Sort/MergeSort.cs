using System;

namespace sort
{
    class MergeSort
    {
        int[] temp;
        public void Soft(int [] n)
        {
            Log("public Soft " + ToString(n));
            temp = n;
            Soft(n, 0, (n.Length - 1) / 2, n.Length - 1);
        }

        private void Soft(int[] n, int begin, int mid, int end)
        {
            if (begin == mid && mid == end)
                return;
            Log("private Soft " + " begin: " + begin + " mid " + mid + " end " + end);
            Soft(n, begin, (mid + begin) / 2, mid);
            Soft(n, mid + 1, (end + mid + 1) / 2, end);
            Merge(n, begin, mid, end);
        }

        private void Merge(int[] n, int begin, int mid, int end)
        {
            int i = begin;
            int j = mid;
            int index = i;
            while (i != mid && j != end)
            {


                //temp[index] = 
            }



            Log("Merge " + " begin: " + begin + " mid " + mid + " end " + end);
            //if(n[])
        }

        private string ToString(int[] n)
        {
            string str = "";
            for (int i = 0; i < n.Length; i++)
            {
                str += " " + n[i];
            }
            return str;
        }
        private void Log(string str)
        {
            Console.WriteLine(str);
        }
    }
}
