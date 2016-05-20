using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models
{
    /// <summary>
    /// Элемент дерева
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public class TreeNode<TNode, TKey, TData>
     where TKey : struct
    {
        public TData Data { get; set; }
        public TKey Key { get; set; }
        public Nullable<TKey> ParentId { get; set; }
        public TNode Node { get; set; }
        public TreeNode<TNode, TKey, TData> Parent { get; set; }
        public List<TreeNode<TNode, TKey, TData>> Children { get; set; }

        public TreeNode()
        {
            Children = new List<TreeNode<TNode, TKey, TData>>();
            Data = Activator.CreateInstance<TData>();
        }

        public TreeNode<TNode, TKey, TData> AppendChild(TreeNode<TNode, TKey, TData> child)
        {
            Children.Add(child);
            child.Parent = this;
            return this;
        }

        public TreeNode<TNode, TKey, TData> SetParent(TreeNode<TNode, TKey, TData> parent)
        {
            parent.AppendChild(this);
            return this;
        }
        public IEnumerable<TNode> EnumerateParents()
        {
            var c = this;
            while (c != null)
            {
                yield return c.Node;
                c = c.Parent;
            }
        }
    }
}
