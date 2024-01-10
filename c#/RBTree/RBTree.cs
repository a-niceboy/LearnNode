/// <summary>
/// 红黑树
/// </summary>
using System;
namespace rbtree
{
    /// <summary>
    /// 颜色枚举
    /// </summary>
    public enum Color
    {
        Red,    // 红
        Black   // 黑
    }

    /// <summary>
    /// 红黑节点类
    /// </summary>
    class RBNode
    {
        public RBNode(int key, Color color = Color.Red)
        {
            this.key = key;
            this.color = color;
            weight = 1;
        }
        public int key;         // 用于判断位置逻辑主键 object 类型？
        //public object value;    // 存储 值
        public RBNode left;     // 左子节点
        public RBNode right;    // 右子节点
        public Color color;     // 颜色
        public int weight;      // 节点权重 越往上权重越大
        public int printFormat; // 仅用于打印格式
    }

    /// <summary>
    /// 红黑树类
    /// </summary>
    internal partial class RBTree
    {
        RBNode root;            // 根节点
        public int size;        // 树总节点数
        public int height;      // 树高
        public bool log = false;// 内部打印平衡过程

        /// <summary>
        /// 打印树
        /// </summary>
        public void Print()
        {
            Log("\n");
            Print(root, null, true);
            Log("\n");
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="key"></param>
        public void Insert(int key)
        {
            Log("public Insert " + key);
            root = Insert(root, key);
            root.color = Color.Black;
            Log("public Insert end root " + root.key);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        public void Delete(int key)
        {
            bool temp = log;
            log = false;
            if (!Find(key))
            {
                return;
            }
            log = temp;
            Log("public Delete " + key);
            root = Delete2(root, key);
            if(root != null)
                root.color = Color.Black;
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Find(int key)
        {
            bool isFind = Find(root, key, 0);
            Log("public Find " + key + " " + isFind);
            return isFind;
        }
    }

    internal partial class RBTree
    {
        private void Print(RBNode node, RBNode parnetNode = null, bool publicLog = false)
        {
            if (node == null)
                return;

            if (!publicLog && !log)
                return;

            // printFormat
            if (parnetNode != null)
            {
                node.printFormat = node.key > parnetNode.key ? parnetNode.printFormat + GetWeight(node) + 1 + GetWeight(parnetNode.left) : parnetNode.printFormat - node.weight - 1 - GetWeight(parnetNode.right);
            }
            else
            {
                int leftHeight = GetLeftAllWeight(node);
                node.printFormat = leftHeight;
            }
            if (node.printFormat < 0 && publicLog)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Log("#### node.printFormat " + node.printFormat + " node.key: " + node.key + " parnetNode.key: " + parnetNode.key + " parnetNode.printFormat: " + parnetNode.printFormat + " node.weight：" + node.weight);
                Console.ReadLine();
                node.printFormat = 0;
            }

            string sign = " ";
            string s = "";
            for (int i = 0; i < node.printFormat; i++)
            {
                s += sign;
            }
            Console.ForegroundColor = node.color == Color.Red ? ConsoleColor.Red : ConsoleColor.White;
            Console.WriteLine(s + node.key);
            Console.ResetColor();
            Print(node.left, node, publicLog);
            Print(node.right, node, publicLog);
        }

        private RBNode Insert(RBNode node, int key)
        {
            if (node == null)
                return new RBNode(key);

            // 判断大小
            if (key < node.key)
                node.left = Insert(node.left, key);
            else
                node.right = Insert(node.right, key);

            UpdateWeight(node);

            node = FixNode(node);
            return node;
        }

        private RBNode Delete(RBNode node, int key)
        {
            Log("private Delete " + key);
            if(key < node.key)
            {
                // 最小节点为2节点 
                if (!IsRed(node.left) && !IsRed(node.left.left))
                    node = MoveRedLeft(node);

                node.left = Delete(node.left, key);
            }
            else
            {
                if (IsRed(node.left))
                    node = RightRotate(node);

                if (node.key == key && node.right == null)
                {
                    Log("real Delete " + node.key);
                    return null;
                }

                // 最大节点为2节点 
                if (!IsRed(node.right.left) && !IsRed(node.right))
                    node = MoveRedRight(node);

                if (node.key == key)
                {
                    RBNode minNode = Min(node.right);
                    node.key = minNode.key;
                    node.right = DeleteMin(node.right);
                    Log("replace Delete " + key + " minNode " + minNode.key);
                }
                else
                {
                    node.right = Delete(node.right, key);
                }
            }
            return FixNode(node);
        }

        private RBNode Delete2(RBNode node, int key)
        {
            Log("private Delete2 " + key);
            if (key > node.key)
            {
                if (IsRed(node.left))
                    node = RightRotate(node);

                // 最大节点为2节点 
                if (!IsRed(node.right.left) && !IsRed(node.right))
                    node = MoveRedRight(node);

                node.right = Delete(node.right, key);
            }
            else
            {
                if (node.key == key && node.left == null)
                {
                    Log("real Delete " + node.key);
                    return null;
                }

                // 最小节点为2节点 
                if (!IsRed(node.left) && !IsRed(node.left.left))
                    node = MoveRedLeft(node);

                if (node.key == key)
                {
                    RBNode maxNode = Max(node.left);
                    node.key = maxNode.key;
                    node.left = DeleteMax(node.left);
                    Log("replace Delete " + key + " minNode " + maxNode.key);
                }
                else
                {
                    node.left = Delete(node.left, key);
                }
            }
            return FixNode(node);
        }

        private bool Find(RBNode node, int key, int depth)
        {
            if (node == null)
                return false;

            depth++;
            Log("private Find node " + node.key + " key " + key + " depth " + depth);

            if (key < node.key)
                return Find(node.left, key, depth);
            else if (key > node.key)
                return Find(node.right, key, depth);
            else
                return true;
        }

        /// <summary>
        /// 删除该节点的最大节点 并修复该节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private RBNode DeleteMax(RBNode node)
        {
            Log("private DeleteMax " + node.key);
            if (IsRed(node.left))
                node = RightRotate(node);

            // 删除？
            if (node.right == null)
                return null;

            // 最大节点为2节点 
            if (!IsRed(node.right.left) && !IsRed(node.right))
                node = MoveRedRight(node);

            node.right = DeleteMax(node.right);
            return FixNode(node);
        }

        /// <summary>
        /// 删除该节点的最小节点 并修复该节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private RBNode DeleteMin(RBNode node)
        {
            Log("private DeleteMin " + node.key);
            // 删除？
            if (node.left == null)
                return null;

            // 最小节点为2节点 
            if (!IsRed(node.left) && !IsRed(node.left.left))
                node = MoveRedLeft(node);

            node.left = DeleteMin(node.left);
            return FixNode(node);
        }

        /// <summary>
        /// 将红移动给右节点
        /// </summary>
        /// <param name="node"></param>
        private RBNode MoveRedRight(RBNode node)
        {
            Log("## MoveRedRight " + node.key);
            if (IsRed(node.right))
                return node;

            Print(node);
            Log("↓");
            // 先从父节点借红
            ColorFilp(node);
            //Print(node);
            //Log("↓");
            // 左兄弟有红节点 从左兄弟借
            if (node.left != null && IsRed(node.left.left))
            {
                node = RightRotate(node);
                ColorFilp(node);
            }
            Print(node);
            return node;
        }

        /// <summary>
        /// 将红移动给左节点
        /// </summary>
        /// <param name="node"></param>
        private RBNode MoveRedLeft(RBNode node)
        {
            Log("## MoveRedLeft " + node.key);
            if (IsRed(node.left))
                return node;

            Print(node);
            Log("↓");
            // 先从父节点借红
            ColorFilp(node);
            // 右兄弟有红节点 从右兄弟借
            if (node.right != null && IsRed(node.right.left))
            {
                node.right = RightRotate(node.right);
                node = LiftRotate(node);
                ColorFilp(node);
            }
            Print(node);
            return node;
        }

        /// <summary>
        /// 左旋节点 a
        ///     a           c
        ///    / \         / \
        ///   b   c       a   e
        ///      / \     / \ 
        ///     d   e   b   d  
        /// </summary>
        /// <param name="待旋转节点"></param>
        /// <returns>旋转后节点</returns>
        private RBNode LiftRotate(RBNode node)
        {
            Log("## LiftRotate " + node.key + " " + node.right.key);
            Print(node);
            Log("↓");
            RBNode rightNode = node.right;
            node.right = rightNode.left;
            rightNode.left = node;

            Color color = rightNode.color;
            rightNode.color = node.color;
            node.color = color;

            UpdateWeight(node);
            UpdateWeight(rightNode);
            Print(rightNode);
            return rightNode;
        }

        /// <summary>
        /// 右旋节点 a
        ///      a      b     
        ///     / \    / \    
        ///    b   c  d   a   
        ///   / \        / \  
        ///  d   e      e   c 
        /// </summary>
        /// <param name="待旋转节点"></param>
        /// <returns>旋转后节点</returns>
        private RBNode RightRotate(RBNode node)
        {
            Log("## RightRotate " + node.key + " " + node.left.key);
            Print(node);
            Log("↓");
            RBNode leftNode = node.left;
            node.left = leftNode.right;
            leftNode.right = node;

            Color color = leftNode.color;
            leftNode.color = node.color;
            node.color = color;

            UpdateWeight(node);
            UpdateWeight(leftNode);
            Print(leftNode);
            return leftNode;
        }

        /// <summary>
        /// 翻色
        /// </summary>
        /// <param name="node"></param>
        private void ColorFilp(RBNode node)
        {
            Log("## ColorFilp " + node.key);
            Print(node);
            Log("↓");
            if (node == null)
                return;
            node.color = IsRed(node) ? Color.Black : Color.Red;
            if (node.left != null)
                node.left.color = IsRed(node.left) ? Color.Black : Color.Red;
            if (node.right != null)
                node.right.color = IsRed(node.right) ? Color.Black : Color.Red;

            Print(node);
        }

        /// <summary>
        /// 修改节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private RBNode FixNode(RBNode node)
        {
            // 如果是为右红 左旋
            if (IsRed(node.right) && !IsRed(node.left))
                node = LiftRotate(node);

            // 如果是连续两个左红 右旋
            if (IsRed(node.left) && IsRed(node.left.left))
                node = RightRotate(node);

            // 左右红 反色
            if (IsRed(node.right) && IsRed(node.left))
                ColorFilp(node);

            return node;
        }

        /// <summary>
        /// 该节点的最小节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private RBNode Min(RBNode node)
        {
            if (node.left != null)
            {
                return Min(node.left);
            }
            return node;
        }

        /// <summary>
        /// 该节点的最大节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private RBNode Max(RBNode node)
        {
            if (node.right != null)
            {
                return Max(node.right);
            }
            return node;
        }

        /// <summary>
        /// 是否为红
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool IsRed(RBNode node)
        {
            if (node == null)
                return false;
            return node.color == Color.Red;
        }

        /// <summary>
        /// 获取左整个权重
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private int GetLeftAllWeight(RBNode node)
        {
            if (node == null || node.left == null)
                return GetWeight(node);

            return GetWeight(node) + GetLeftAllWeight(node.left);
        }

        /// <summary>
        /// 获取右整个权重
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private int GetRightAllWeight(RBNode node)
        {
            if (node == null || node.right == null)
                return GetWeight(node);

            return GetWeight(node) + GetRightAllWeight(node.right);
        }
        /// <summary>
        /// 获取权重
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private int GetWeight(RBNode node)
        {
            if (node == null)
                return 0;
            return node.weight;
        }

        /// <summary>
        /// 更新权重
        /// </summary>
        /// <param name="node"></param>
        private void UpdateWeight(RBNode node)
        {
            if (node == null)
                return;
            node.weight = GetWeight(node.right) + GetWeight(node.left) + 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        private void Log(string s)
        {
            if (log)
            {
                Console.WriteLine(s);
            }
        }
    }
}