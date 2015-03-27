using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ghostbit.Tweaker.AssemblyScanner;
using Ghostbit.Tweaker.Core;
using Ghostbit.Tweaker.UI;

namespace Ghostbit.Tweaker.Core.Testbed
{
	class TreeViewExample
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

		static void Main(string[] args)
		{
			Tweaker tweaker = new Tweaker();
			Scanner scanner = new Scanner();
			ScanOptions scanOptions = new ScanOptions();
			scanOptions.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetExecutingAssembly() };
			scanOptions.Types.ScannableRefs = new Type[] { typeof(TreeViewExample.TestClass) };

			TweakerOptions tweakerOptions = new TweakerOptions();
			tweakerOptions.Flags =
				TweakerOptionFlags.DoNotAutoScan |
				TweakerOptionFlags.ScanForTweakables |
				TweakerOptionFlags.ScanForInvokables |
				TweakerOptionFlags.ScanForWatchables;
			tweaker.Init(tweakerOptions, scanner);
			tweaker.Scanner.Scan(scanOptions);

			TweakerTree view = new TweakerTree(tweaker);
			view.BuildTree();
		}
	}	
}
