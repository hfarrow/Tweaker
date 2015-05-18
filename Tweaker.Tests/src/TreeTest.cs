using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tweaker.UI.Tests
{
	[TestFixture]
	public class TreeTest
	{
		public class TestNode : TreeNode<TestNode>
		{
			public string Name { get; set; }
			public TestNode(string name)
			{
				Name = name;
			}
		}

		[SetUp]
		public void Init()
		{

		}

		[Test]
		public void ValidateEmptyTree()
		{
			Tree<TestNode> tree = new Tree<TestNode>(new TestNode("Root"));
			TestNode root = tree.Root;
			Assert.IsNotNull(root.Children);
			Assert.AreEqual(0, root.Children.Count);
			Assert.IsNotNull(root.Root);
			Assert.IsNull(root.Parent);
			Assert.AreEqual(root, root.Root);
			Assert.AreEqual(0, root.Depth);
			Assert.AreEqual("Root", root.Name);
			Assert.AreEqual("Root", root.Root.Value.Name);

			int counter = 0;
			foreach (var node in root.TraverseBreadthFirst())
			{
				Assert.NotNull(node);
				counter++;
			}
			Assert.AreEqual(0, counter);

			counter = 0;
			foreach (var node in root.TraverseDepthFirst())
			{
				Assert.NotNull(node);
				counter++;
			}
			Assert.AreEqual(0, counter);
		}

		[Test]
		public void AddNodeToRoot()
		{
			Tree<TestNode> tree = new Tree<TestNode>(new TestNode("Root"));
			TestNode root = tree.Root;

			TestNode child1 = root.Children.Add(new TestNode("child1"));
			Assert.IsNotNull(child1);
			Assert.AreEqual(1, root.Children.Count);
			Assert.AreSame(child1, root.Children[0]);
			Assert.AreSame("child1", child1.Name);
			Assert.AreEqual(0, child1.Children.Count);
			Assert.AreEqual(1, child1.Depth);

			TestNode child2 = root.Children.Add(new TestNode("child2"));
			Assert.IsNotNull(child2);
			Assert.AreEqual(2, root.Children.Count);
			Assert.AreSame(child2, root.Children[1]);
			Assert.AreSame("child2", child2.Name);
			Assert.AreEqual(0, child2.Children.Count);
			Assert.AreEqual(1, child2.Depth);
		}

		[Test]
		public void TraverseSingleDepthTree()
		{
			Tree<TestNode> tree = new Tree<TestNode>(new TestNode("Root"));
			TestNode root = tree.Root;

			TestNode child1 = root.Children.Add(new TestNode("child1"));
			TestNode child2 = root.Children.Add(new TestNode("child2"));

			TestNode[] nodes = new TestNode[] { child1, child2 };
			int index = 0;
			foreach (var node in root.TraverseBreadthFirst())
			{
				Assert.AreSame(nodes[index], node);
				index++;
			}

			index = 0;
			foreach (var node in root.TraverseDepthFirst())
			{
				Assert.AreSame(nodes[index], node);
				index++;
			}
		}

		[Test]
		public void ValidateTraverseDepthAndBreadth()
		{
			Tree<TestNode> tree = new Tree<TestNode>(new TestNode("Root"));
			TestNode root = tree.Root;

			TestNode child_0 = root.Children.Add(new TestNode("child_0"));
			TestNode child_1 = root.Children.Add(new TestNode("child_1"));

			TestNode child_0_0 = child_0.Children.Add(new TestNode("child_0_0"));
			TestNode child_0_1 = child_0.Children.Add(new TestNode("child_0_1"));

			TestNode child_1_0 = child_1.Children.Add(new TestNode("child_1_0"));
			TestNode child_1_1 = child_1.Children.Add(new TestNode("child_1_1"));

			TestNode[] breadthFirst = new TestNode[] { child_0, child_1, child_0_0, child_0_1, child_1_0, child_1_1 };
			TestNode[] depthFirst = new TestNode[] { child_0, child_0_0, child_0_1, child_1, child_1_0, child_1_1 };

			int index = 0;
			foreach (var node in root.TraverseBreadthFirst())
			{
				Assert.AreSame(breadthFirst[index], node);
				index++;
			}

			index = 0;
			foreach (var node in root.TraverseDepthFirst())
			{
				Assert.AreSame(depthFirst[index], node);
				index++;
			}
		}

		[Test]
		public void ValidateMultiDepths()
		{
			Tree<TestNode> tree = new Tree<TestNode>(new TestNode("Root"));
			TestNode root = tree.Root;
			TestNode child_0 = root.Children.Add(new TestNode("child_0"));
			TestNode child_0_0 = child_0.Children.Add(new TestNode("child_0_0"));

			Assert.AreEqual(0, root.Depth);
			Assert.AreEqual(1, child_0.Depth);
			Assert.AreEqual(2, child_0_0.Depth);

			Assert.AreSame(root, child_0.Parent);
			Assert.AreSame(root, child_0.Root);

			Assert.AreSame(child_0, child_0_0.Parent);
			Assert.AreSame(root, child_0_0.Root);
		}
	}
}
