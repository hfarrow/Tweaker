using Tweaker.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweaker.UI
{
	public class BaseNode : TreeNode<BaseNode>
	{
		public enum NodeType
		{
			Unknown,
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
		private ITweakable[] argTweakables;

		public InvokableNode(IInvokable invokable)
		{
			Invokable = invokable;

			if (invokable != null)
			{
				BindArgsToTweakables();
			}
		}

		private void BindArgsToTweakables()
		{
			// Add an empy invokable node that will execute the invokable.
			Children.Add(new InvokableNode(null));

			Type[] argTypes = Invokable.ParameterTypes;
			argTweakables = new ITweakable[argTypes.Length];

			for(int i = 0; i < argTypes.Length; ++i)
			{
				Type argType = argTypes[i];
				object virtualFieldRef;
				ITweakable tweakable = TweakableFactory.MakeTweakable(argType, 
					Invokable.Parameters[i].Name, 
					Invokable.InvokableInfo.ArgDescriptions[i],
					out virtualFieldRef);
				argTweakables[i] = tweakable;

				Children.Add(new TweakableNode(tweakable, virtualFieldRef));
			}
		}

		public void Invoke()
		{
			object[] args = new object[argTweakables.Length];
			for (int i = 0; i < args.Length; ++i)
			{
				args[i] = argTweakables[i].GetValue();
			}
			Invokable.Invoke(args);
		}
	}

	public class TweakableNode : BaseNode
	{
		public ITweakable Tweakable { get; private set; }
		public override NodeType Type { get { return NodeType.Tweakable; } }

#pragma warning disable 0414 // is assigned but its value is never used
		// Keep a reference to the virtual field (if there is one) because the only other reference
		// to this object is a WeakReference owned by BaseTweakable.
		private object virtualFieldRef;
#pragma warning restore 0414

		public TweakableNode(ITweakable tweakable)
		{
			Tweakable = tweakable;
		}

		public TweakableNode(ITweakable tweakable, object virtualFieldRef)
			: this(tweakable)
		{
			this.virtualFieldRef = virtualFieldRef;
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

	public class TweakerTree
	{
		public ITweakerLogger logger = LogManager.GetCurrentClassLogger();
		public Tree<BaseNode> Tree { get; private set; }
		public Tweaker Tweaker { get; private set; }
		private Dictionary<string, GroupNode> GroupNodes { get; set; }

		public TweakerTree(Tweaker tweaker)
		{
			Tweaker = tweaker;
		}

		public void BuildTree(SearchOptions searchOptions = null)
		{
			logger.Debug("BuildTree({0})", searchOptions);

			Tree = new Tree<BaseNode>(new GroupNode("Root", "Root"));
			GroupNodes = new Dictionary<string, GroupNode>();

			var invokables = Tweaker.Invokables.GetInvokables(searchOptions);
			var tweakables = Tweaker.Tweakables.GetTweakables(searchOptions);
			//var watchables = Tweaker.Watchables.GetWatchables(searchOptions);

			// Merge objects into a single temporary list
			List<ITweakerObject> objects = new List<ITweakerObject>();
			objects.AddRange(invokables.Values.ToArray());
			objects.AddRange(tweakables.Values.ToArray());
			//objects.AddRange(watchables.Values);

			foreach (ITweakerObject tweakerObj in objects)
			{
				string fullName = tweakerObj.Name;
				string groupPath = "";
				int indexOfNodeName = fullName.LastIndexOf('.');
				if (indexOfNodeName >= 0)
				{
					groupPath = fullName.Substring(0, indexOfNodeName);
				}

				TreeNode<BaseNode> parent = Tree.Root;
				if (!string.IsNullOrEmpty(groupPath))
				{
					parent = EnsureGroupExists(groupPath);
				}
				CreateTweakerNode(parent, tweakerObj);
			}

			SortGroupChildren();
		}

		private void SortGroupChildren()
		{
			logger.Debug("SortGroupChildren");

			// GetBranchNodes is an enumerator so we need to cache all nodes before we start
			// moving children around.
			List<BaseNode> branchNodes = new List<BaseNode>() { Tree.Root.Value };
			foreach(var node in Tree.Root.GetBranchNodes())
			{
				logger.Debug("Found branchNode: {0}", node);
				branchNodes.Add(node.Value);
			}

			foreach(var node in branchNodes)
			{
				// Create a list for each type of node
				// TODO: decide on a more generic way to do this.
				// What if the number of node types explodes?
				// Dictionary<Type, List<Type>> or add sorting mechanism to Tree data structure
				List<GroupNode> groups = new List<GroupNode>();
				List<InvokableNode> invokables = new List<InvokableNode>();
				List<TweakableNode> tweakables = new List<TweakableNode>();
				List<WatchableNode> watchables = new List<WatchableNode>();
				List<BaseNode> other = new List<BaseNode>();

				foreach (var childNode in node.Children)
				{
					switch (childNode.Value.Type)
					{
						case BaseNode.NodeType.Group:
							groups.Add(childNode.Value as GroupNode);
							break;
						case BaseNode.NodeType.Invokable:
							invokables.Add(childNode.Value as InvokableNode);
							break;
						case BaseNode.NodeType.Tweakable:
							tweakables.Add(childNode.Value as TweakableNode);
							break;
						case BaseNode.NodeType.Watchable:
							watchables.Add(childNode.Value as WatchableNode);
							break;
						default:
							other.Add(childNode.Value);
							break;
					}
				}

				List<BaseNode> sortedNodes = new List<BaseNode>(node.Children.Count);
				groups.ForEach(n => sortedNodes.Add(n));
				invokables.ForEach(n => sortedNodes.Add(n));
				tweakables.ForEach(n => sortedNodes.Add(n));
				watchables.ForEach(n => sortedNodes.Add(n));
				other.ForEach(n => sortedNodes.Add(n));
				for (int i = 0; i < node.Children.Count; ++i)
				{
					node.Children[i] = sortedNodes[i];
				}
			}
		}

		private TreeNode<BaseNode> EnsureGroupExists(string groupPath)
		{
			//logger.Trace("EnsureGroupExists({0})", groupPath);

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
			logger.Trace("CreateGroupNode({0}, {1}, {2})", fullName, shortName, parent);

			var newNode = new GroupNode(fullName, shortName);
			GroupNodes.Add(fullName, newNode);
			parent.Children.Add(newNode);
			return newNode;
		}

		private TreeNode<BaseNode> CreateTweakerNode(TreeNode<BaseNode> parent, ITweakerObject obj)
		{
			logger.Trace("CreateTweakerNode({0}, {1})", parent, obj.Name);

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
