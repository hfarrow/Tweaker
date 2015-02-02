# About Tweaker
The intention of Tweaker is to provide a system that discovers and processes custom attributes (annotations) into a collection of intermediate objects. Tweaker was created with the intention of being used as the "back end" of a debug console/gui for games. Tweaker itself does not include a console or gui but simply provides a data model exposing Invokables, Tweakables, and Watchables for consumption by a debug console. It is up to the consuming console to present and manipulate the Invokables, Tweakables, and Watchables provided by Tweaker.

# Description
Tweaker can be broken down into 2 main components. A Core collection of classes and an AssemblyScanner (referred to as Scanner)

## Core
The meat and potatoes of Tweaker. Users of Tweaker interact with the Core and need not know about the Scanner.
There are 3 types of objects that Tweaker can expose:

1. **Invokables**
  * AKA: Command
  * Invokables are defined by annotating a static Method or C# event.
  * Invokables are given a unique name. To group multiple invokables into groups and sub groups you could use dots or slashes in the name. It is up to the consuming console to display these as groups if it wants to.
    * For Example: GameplayTuning.Player.Speed or GameplayTuning/Player/Speed 
  * Can have any number of arguments. Currently it is the responsibility of the consuming console to inspect the argument types and ensure it passes the correct number and type of arguments when invoking.
    *Exceptions thrown if arguments mismatch. Invoking is wrapped in try catch.
  * Instance member methods or events can be added to Tweaker manually through Tweaker's interface.
    * Don't think there is much practical use for this. Maybe if you had a several player objects and want to tune or control them separately through the console.
  * You can annotate a type as Invokable and all public methods will be registered. Useful if you wanted to group many debug commands into a single class. Easy to then exclude the entire class from a production build.
2. **Tweakables**
  * Tweakables are defined by annotating a static property or field
  * Additional annotations can optionally be added to specify a min and max value or a list of valid named toggle values.
    * Values outside min and max are automatically clamped.
    * API is exposed that allows you to go forward or backwards through toggle values. Will wrap at the end values.
    * Define a step for values to be incremented or decremented by. (The annotation is defined but currently it is not processed/implemented.)
  * Grouped the same way as Invokables
  * You can annotate a type as Tweakable and all public methods and fields will be registered.
3. **Watchable**
  * Not implemented currently and purely conceptual.
    * The idea here it to annotate properties or fields that you want to monitor overtime
  * A console could simply display the value of a watchable in a special readonly manner (Tweakables by definition are read/write).
  * A console could show a graph of the value over time
  * A console could show a graph of the change of the value over time (delta).
  * A console could group multiple Watchables into the same graph.
  * Bar graphs, line graphs, piechart, etc
  * For custom types (non integral) a plugin system could be devised that allows other objects to be graphed or allow textures / render targets to be displayed. Basically add plugins for rendering/watching custom types

The best description of the Core would simply be an example demonstrating how to use Tweaker which you can see below. The unit test source can be found here: https://github.com/Ghostbit/Tweaker/blob/master/Tweaker.Core.Tests/src/Tests/TweakableTest.cs. The tests also demonstrate how to use Tweaker.

First, lets define a class that represents something in our game and we are going to annotate some static properties, method, and fields. These annotated members are what become Invokables and Tweakables.

```C#
public class Player
{
	// Expose the player's speed to to Tweaker via a Tweakable.
	// Note that the property can be public or private.
    [Tweakable("Gameplay.Player.Speed")]
    public static int Speed { get; set; }

	// Expose the player's max health and provide constraints via TweakableAttributes.Range attribute.
    [Tweakable("Gameplay.Player.MaxHealth"), 
	Range(1, 100)]
    public static int MaxHealth { get; set; }

	// Expose the default state for a player and constrain it to 3 possible states: idle, patrol, attack
	// Use NamedToggleValue(string displayName, object value, int index) to define the possible states.
	// index is used for ordering the values. The order that each value is declared in may not match the order
	// that they get returned by reflection.
    [Tweakable("Gameplay.Player.DefaultState"),
    NamedToggleValue("idle", 0, 0),
    NamedToggleValue("patrol", 1, 1),
    NamedToggleValue("attack", 2, 2)]
    public static int DefaultState { get; set; }

	// Expose a field instead of a property
    [Tweakable("Gameplay.Player.SomeRandomField")]
    public static int SomeRandomField;

	// Expose a field with a range instead of a property
    [Tweakable("Gameplay.Player.SomeRandomFieldWithRange"),
	Range(0, 100)]
    public static int SomeRandomFieldWithRange;

	// Expose a field with toggle values instead of a property
    [Tweakable("Gameplay.Player.SomeRandomFieldWithToggles"),
    NamedToggleValue("zero", 0, 0),
    NamedToggleValue("eleven", 11, 1),
    NamedToggleValue("hundred", 100, 2)]
    public static int SomeRandomFieldWithToggles;
 
	// Expose a command that adds adds 10 to MaxHealth
	[Invokable("Gameplay.Player.AddTenToMaxHealth")]
	public static void AddTenToMaxHealth()
	{
		MaxHealth += 10;
	}
 
	// Expose an event that can be invoked by tweaker
	// When invoked, the underlying multidelegate will be dispatched.
	[Invokable("Gameplayer.Player.EventWithListeners")]
	public static event Action EventWithListeners;
}
```
Second, lets see how a console can use the Tweaker api to invoke or modify the annotated members above.
```C#
public void Main()
{
// First we must scan assemblies for the tweaker annotations (Invokable, Tweakables, Watchables)
// We won't dive into the details of Scanner but know that it can filter what assemblies and types you scan.
// You should only have to scan once at the start of your application. If new assemblies are loaded you may
// want to scan the newly loaded assemblies
Scanner scanner = new Scanner();
ScanOptions options = new ScanOptions();
options.Assemblies.ScannableRefs = new Assembly[] { Assembly.GetExecutingAssembly() };

// Create a manager for all Tweakables that are scanned.
ITweakableManager tweakableManager = new TweakableManager(scanner);


// Create a manager for all Invokables that are scanned.
IInvokableManager invokableManager = new InvokabkeManager(scanner);
 
// Once the managers are created, scan! The managers will have added processors to the scanner that will convert
// our annotated members into intermediate objects that can be accessed and manipulated through the managers.
scanner.Scan(options); // WARNING: expensive because it enumerates all types and members allowed by the ScanOptions
 
// Optionally, all the above can be done using the Tweaker convenience class
/*
	Tweaker tweaker = new Tweaker();
	tweaker.Init(null);
	// Init takes TweakerOptions which contains flags controlling how tweaker initializes.
	// Passing null will result in using default options. By default all non "system" assemblies will
	// be scanned which is probably undesirable. Using the options you can limit the scan to specific
	// assemblies (recommended) in order to speed up the scan duration.
 
	invokableManager = tweaker.Invokables;
	tweakableManager = tweaker.Tweakables;
*/
 
// Lets get some Tweakable objects and set their values to something new...
 
// The line below would get all tweakables so that they can be displayed in a console or gui.
// For the example we will fetch individual objects.
//var tweakables = tweakableManager.GetTweakables(null);
 
// Lets get and set the player Speed property that we annotated.
ITweakable tweakable = tweakableManager.GetTweakable("Gameplay.Player.Speed");
Log("Value = " + tweakable.GetValue());
tweakable.SetValue(10000);
 
// Lets get and set the player DefaultState property toggle that we annotated.
TweakableToggle tweakableToggle = tweakableManager.GetTweakable("Gameplay.Player.DefaultState") as TweakableToggle;
Log("Value = " + tweakableToggle.GetValueName());
tweakable.NextValue();
tweakable.SetValueByName("patrol"); // Player.DefaultState will equal 1;
// There are several other TweakableToggle methods to make working with toggles in a console easy.
 
// Lets invoke the AddTenToMaxHealth method that we annotated.
IInvokable invokable = invokableManager.GetInvokable("Gameplay.Player.AddTenToMaxHealth");
invokable.Invoke(); // Overloaded to take an Object[] argument but our invokable takes no arguments.
}
```
Hopefully this paints a clear picture about how a console would interface with Tweaker.
The library is a working proof of concept at this point and is deserving a of a little polish. It would also be nice to flesh out and implement Watchables.

## Assembly Scanner
The Scanner is supplemental to the Core and is not integral to the concept of Tweaker. The Scanner's responsibility is to locate and process the attributes that Invokables, Tweakables, and Watchables are annotated with. Once processed, the Tweaker API exposes methods for retrieving intermediate objects that represent the data collected and processed by the Scanner. This is demonstrated in the examples in the Core section of this page.

The source for the Scanner can be found here: https://github.com/Ghostbit/Tweaker/tree/master/Tweaker.Core/src/AssemblyScanner 

Please note that the Processors that convert annotated types and members into Invokables, Tweakables, and Watchables are located in the Core. They extend a base processor interface defined by the Scanner. The Scanner API has no dependencies on Core.

The scanner is a generic way of searching for attributes, types, methods, or fields and processing them into intermediate objects defined by users of the scanner. The Core is one such user of the Scanner. The Scanner itself has no external dependenies and could be re-used in other projects. Infact, the Scanner could be it's own library/repository.
