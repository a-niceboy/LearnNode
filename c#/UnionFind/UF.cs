using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace union_find
{
    interface UF
    {
        int Find(int p);
        bool IsConnect(int p, int q);
        void Union(int p, int q);
    }
    class UFQuickFind : UF
    {
        public UFQuickFind(int N)
        {
            count = N;
            id = new int[N];
            for (int i = 0; i < N; i++)
            {
                id[i] = i;
            }
        }

        public int Find(int p)
        {
            return id[p];
        }

        public bool IsConnect(int p, int q)
        {
            return id[p] == id[q];
        }

        public void Union(int p, int q)
        {
            if (IsConnect(p, q))
            {
                return;
            }

            int pId = Find(p);
            int qId = Find(q);

            for (int i = 0; i < id.Length; i++)
            {
                if (id[i] == pId)
                {
                    id[i] = qId;
                }
            }
            count--;
        }

        public int count;
        public int[] id;
    }

    class UFQuickUnion : UF
    {
        public UFQuickUnion(int N)
        {
            count = N;
            id = new int[N];
            for (int i = 0; i < N; i++)
            {
                id[i] = i;
            }
        }

        public int Find(int p)
        {
            while (p != id[p])
                p = id[p];

            return p;
        }

        public bool IsConnect(int p, int q)
        {
            return Find(p) == Find(q);
        }

        public void Union(int p, int q)
        {
            if (IsConnect(p, q))
            {
                return;
            }

            int pRoot = Find(p);
            int qRoot = Find(q);

            id[pRoot] = qRoot;
            count--;
        }

        public void PrintId()
        {
            for (int i = 0; i < id.Length; i++)
            {
                Console.WriteLine(id[i]);
            }
        }

        public int count;
        public int[] id;
    }

    class UFWeightQuickUnion : UF
    {
        public UFWeightQuickUnion(int N)
        {
            count = N;
            id = new int[N];
            size = new int[N];
            for (int i = 0; i < N; i++)
            {
                id[i] = i;
                size[i] = 1;
            }
        }

        public int Find(int p)
        {
            int start = p;
            while (p != id[p])
            {
                p = id[p];
            }

            //while (p != id[start])
            //{
            //    int next = id[start];
            //    id[start] = p;
            //    size[start] = 2;
            //    start = id[next];
            //}

            return p;
        }

        public bool IsConnect(int p, int q)
        {
            return Find(p) == Find(q);
        }

        public void Union(int p, int q)
        {
            if (IsConnect(p, q))
            {
                return;
            }

            int pRoot = Find(p);
            int qRoot = Find(q);

            if (size[pRoot] < size[qRoot])
            {
                id[pRoot] = qRoot;
                size[qRoot] += size[pRoot];
            }
            else
            {
                id[qRoot] = pRoot;
                size[pRoot] += size[qRoot];
            }
            count--;
        }

        public void PrintId()
        {
            for (int i = 0; i < id.Length; i++)
            {
                Console.WriteLine(id[i]);
            }
        }

        public int count;
        public int[] id;
        public int[] size;
    }
}
