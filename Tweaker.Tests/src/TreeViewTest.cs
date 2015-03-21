using Ghostbit.Tweaker.AssemblyScanner;
using Ghostbit.Tweaker.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ghostbit.Tweaker.UI.Tests
{
	[TestFixture]
	public class TreeViewTest
	{
#pragma warning disable 0067,0649,0219
		public class TestClass
		{
			[Tweakable("Node-1")]
			public static int RootLevelNode1 = 1;

			[Tweakable("Node-2")]
			public static int RootLevelNode2 = 2;

			[Tweakable("Group-1.Node-1")]
			public static int Group1_Node1 = 11;

			[Tweakable("Group-1.Node-2")]
			public static int Group1_Node2 = 12;

			[Tweakable("Group-2.Node-1")]
			public static int Group2_Node1 = 21;

			[Tweakable("Group-2.Node-2")]
			public static int Group2_Node2 = 22;

			[Tweakable("Group-1.Group-1.Node-1")]
			public static int Group1_Group1_Node1 = 111;

			[Tweakable("Group-1.Group-1.Node-2")]
			public static int Group1_Group1_Node2 = 112;
		}
#pragma warning restore 0067,0649,0219

		private TestClass testClass;
		private Tweaker tweaker;
		[SetUp]
		public void Init()
		{
			testClass = new TestClass();
			tweaker = new Tweaker();

			Scanner scanner = new Scanner();
			ScanOptions scanOptions = new ScanOptions();
			scanOptions.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetExecutingAssembly() };
			scanOptions.Types.ScannableRefs = new Type[] { typeof(TreeViewTest.TestClass) };

			TweakerOptions tweakerOptions = new TweakerOptions();
			tweakerOptions.Flags = 
				TweakerOptionFlags.DoNotAutoScan |
				TweakerOptionFlags.ScanForTweakables |
				TweakerOptionFlags.ScanForInvokables |
				TweakerOptionFlags.ScanForWatchables;
			tweaker.Init(tweakerOptions, scanner);
			tweaker.Scanner.Scan(scanOptions);
		}

		[Test]
		public void BuildTreeAndVerify()
		{
			TreeView view = new TreeView(tweaker);
			view.BuildTree();

			Tree<BaseNode> tree = view.Tree;
			Assert.IsNotNull(tree);
			Assert.IsNotNull(tree.Root);

			TreeNodeList<BaseNode> rootChildren = tree.Root.Children;
			Assert.IsNotNull(rootChildren);
			Assert.AreEqual(4, rootChildren.Count);

			bool found_Node1 = false;
			bool found_Node2 = false;
			bool found_Group1 = false;
			bool found_Group2 = false;

			foreach (BaseNode node in rootChildren)
			{
				if (node is TweakableNode)
				{
					TweakableNode tweakableNode = node as TweakableNode;
					if (tweakableNode.Tweakable.Name == "Node-1")
					{
						found_Node1 = true;
					}
					else if (tweakableNode.Tweakable.Name == "Node-2")
					{
						found_Node2 = true;
					}
				}
				else if (node is GroupNode)
				{
					GroupNode groupNode = node as GroupNode;
					if (groupNode.FullName == "Group-1" && groupNode.ShortName == "Group-1")
					{
						found_Group1 = true;
						Assert.AreEqual(3, groupNode.Children.Count);
					}
					else if (groupNode.FullName == "Group-2" && groupNode.ShortName == "Group-2")
					{
						found_Group2 = true;
						Assert.AreEqual(2, groupNode.Children.Count);
					}
				}
			}

			Assert.IsTrue(found_Node1);
			Assert.IsTrue(found_Node2);
			Assert.IsTrue(found_Group1);
			Assert.IsTrue(found_Group2);
		}
	}
}
