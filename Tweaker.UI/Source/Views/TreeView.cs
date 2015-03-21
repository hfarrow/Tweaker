using Ghostbit.Tweaker.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghostbit.Tweaker.UI
{
	public class BaseNode : TreeNode<BaseNode>
	{
		public enum NodeType
		{
			Unknown,
			Root,
			Group,
			Invokable,
			Tweakable,
			Watchable
		}

		public virtual NodeType Type { get { return NodeType.Unknown; } }

		public BaseNode()
		{
			if (Type == NodeType.Unknown)
			{
				throw new Exception("NodeType must be overriden in parent and must not be Unknown.");
			}
		}
	}

	public class RootNode : BaseNode
	{
		public RootNode()
		{
		}

		public override NodeType Type { get { return NodeType.Root; } }

		public void Init()
		{

		}
	}

	public class GroupNode : BaseNode
	{
		public string ShortName { get; private set; }
		public string FullName { get; private set; }
		public override NodeType Type { get { return NodeType.Group; } }

		public GroupNode(string fullName, string shortName)
		{
			FullName = fullName;
			ShortName = shortName;
		}
	}

	public class InvokableNode : BaseNode
	{
		public IInvokable Invokable { get; private set; }
		public override NodeType Type { get { return NodeType.Invokable; } }

		public InvokableNode(IInvokable invokable)
		{
			Invokable = invokable;
		}
	}

	public class TweakableNode : BaseNode
	{
		public ITweakable Tweakable { get; private set; }
		public override NodeType Type { get { return NodeType.Tweakable; } }

		public TweakableNode(ITweakable tweakable)
		{
			Tweakable = tweakable;
		}
	}

	public class WatchableNode : BaseNode
	{
		public IWatchable Watchable { get; private set; }
		public override NodeType Type { get { return NodeType.Invokable; } }

		public WatchableNode(IWatchable watchable)
		{
			Watchable = watchable;
		}
	}

	public class TreeView
	{
		public Tree<BaseNode> Tree { get; private set; }
		public Tweaker Tweaker { get; private set; }
		private Dictionary<string, GroupNode> GroupNodes { get; set; }

		public TreeView(Tweaker tweaker)
		{
			Tweaker = tweaker;
		}

		public void BuildTree(SearchOptions searchOptions = null)
		{
			Tree = new Tree<BaseNode>(new RootNode());
			GroupNodes = new Dictionary<string, GroupNode>();

			var invokables = Tweaker.Invokables.GetInvokables(searchOptions);
			var tweakables = Tweaker.Tweakables.GetTweakables(searchOptions);
			//var watchables = Tweaker.Watchables.GetWatchables(searchOptions);

			// Merge objects into a single temporary list
			List<ITweakerObject> objects = new List<ITweakerObject>();
			objects.AddRange(invokables.Values);
			objects.AddRange(tweakables.Values);
			//objects.AddRange(watchables.Values);

			foreach (ITweakerObject tweakerObj in objects)
			{
				string fullName = tweakerObj.Name;
				string groupPath = "";
				string nodeName;
				int indexOfNodeName = fullName.LastIndexOf('.');
				if (indexOfNodeName < 0)
				{
					nodeName = fullName;
				}
				else
				{
					groupPath = fullName.Substring(0, indexOfNodeName);
					nodeName = fullName.Substring(indexOfNodeName + 1);
				}

				TreeNode<BaseNode> parent = Tree.Root;
				if (!string.IsNullOrEmpty(groupPath))
				{
					parent = EnsureGroupExists(groupPath);
				}
				CreateTweakerNode(parent, tweakerObj);
			}
		}

		private TreeNode<BaseNode> EnsureGroupExists(string groupPath)
		{
			string[] groups = groupPath.Split('.');
			string currentGroupPath = "";
			TreeNode<BaseNode> currentNode = Tree.Root;
			for (var i = 0; i < groups.Length; ++i)
			{
				if (i > 0)
				{
					currentGroupPath += "." + groups[i];
				}
				else
				{
					currentGroupPath += groups[i];
				}

				GroupNode node = GetGroupNode(currentGroupPath);
				if (node == null)
				{
					currentNode = CreateGroupNode(currentGroupPath, groups[i], currentNode);
				}
				else
				{
					currentNode = node;
				}
			}

			return currentNode;
		}

		public GroupNode GetGroupNode(string groupPath)
		{
			GroupNode node = null;
			GroupNodes.TryGetValue(groupPath, out node);
			return node;
		}

		private GroupNode CreateGroupNode(string fullName, string shortName, TreeNode<BaseNode> parent)
		{
			var newNode = new GroupNode(fullName, shortName);
			GroupNodes.Add(fullName, newNode);
			parent.Children.Add(newNode);
			return newNode;
		}

		private TreeNode<BaseNode> CreateTweakerNode(TreeNode<BaseNode> parent, ITweakerObject obj)
		{
			BaseNode newNode = null;
			if (obj is IInvokable)
			{
				newNode = new InvokableNode(obj as IInvokable);
			}
			else if (obj is ITweakable)
			{
				newNode = new TweakableNode(obj as ITweakable);
			}
			else if (obj is IWatchable)
			{
				newNode = new WatchableNode(obj as IWatchable);
			}
			parent.Children.Add(newNode);
			return newNode;
		}
	}
}
