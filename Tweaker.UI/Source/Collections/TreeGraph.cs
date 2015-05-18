using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweaker.UI
{
	public class TreeGraph<TValue>
	{
		public TValue Root { get; private set; }

		public TreeGraph(TValue root)
		{
			Root = root;
		}
	}

	public class TreeNode<TValue>
		where TValue : TreeNode<TValue>
	{
		public TValue Root { get; private set; }
		public TreeNodeList<TValue> Children { get; private set; }

		private TValue parent;
		public TValue Parent
		{
			get
			{
				return parent == null ? null : parent.Value;
			}
			set
			{
				if (parent == value)
				{
					return;
				}

				if (parent != null)
				{
					parent.Children.Remove(Value);
				}

				if (value != null && !value.Children.Contains(this.Value))
				{
					value.Children.Add(Value);
				}

				parent = value;
				UpdateRoot();
			}
		}

		public TValue Value { get { return (TValue)this; } }

		public uint Depth { get; private set; }

		public TreeNode() : 
			this(null)
		{
			
		}

		public TreeNode(TValue parent)
		{
			// Root will be updated when Parent is set. However, Root will not be 
			// set if parent is null so default to self. This ensures Root != null. 
			Root = Value;
			Parent = parent;
			Children = new TreeNodeList<TValue>(Value);
		}

		private void UpdateRoot()
		{
			uint depth = 0;
			TValue node = Value;
			while (node.Parent != null)
			{
				depth++;
				node = node.Parent;
			}
			Root = node;
			Depth = depth;
		}

		public IEnumerable<TreeNode<TValue>> TraverseBreadthFirst()
		{
			foreach (var child in Children)
			{
				yield return child;
			}

			foreach (var child in Children)
			{
				foreach (var _child in child.TraverseBreadthFirst())
				{
					yield return _child;
				}
			}
		}

		public IEnumerable<TreeNode<TValue>> TraverseDepthFirst()
		{
			foreach (var child in Children)
			{
				yield return child;
				foreach (var _child in child.TraverseDepthFirst())
				{
					yield return _child;
				}
			}
		}

		// Enumerate all node (in undefined order) and return nodes that have at
		// least one child.
		public IEnumerable<TreeNode<TValue>> GetBranchNodes()
		{
			foreach(var node in TraverseBreadthFirst())
			{
				if(node.Children.Count > 0)
				{
					yield return node;
				}
			}
		}

		// Enumerate all node (in undefined order) and return nodes that have no
		// children
		public IEnumerable<TreeNode<TValue>> GetLeafNodes()
		{
			foreach (var node in TraverseBreadthFirst())
			{
				if (node.Children.Count == 0)
				{
					yield return node;
				}
			}
		}
	}

	public class TreeNodeList<TValue> : List<TValue>
		where TValue : TreeNode<TValue>
	{
		public TValue Parent {get; private set;}

		public TreeNodeList(TValue parent)
		{
			Parent = parent;
		}

		public new TValue Add(TValue node)
		{
			base.Add(node);
			node.Parent = Parent;
			return node;
		}
	}
}
