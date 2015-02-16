# About Tweaker
The intention of Tweaker is to provide a system that discovers and processes custom attributes (annotations) into a collection of intermediate objects. Tweaker was created with the intention of being used as the "back end" of a debug console/gui for games. Tweaker itself does not include a console or gui but simply provides a data model exposing **Invokables**, **Tweakables**, and **Watchables** for use by a debug console. It is up to the console to present and manipulate the Invokables, Tweakables, and Watchables provided by Tweaker.

# Description
Tweaker can be broken down into 2 main components. The **Core** and **AssemblyScanner** (referred to as Scanner)

## Core
The meat and potatoes of Tweaker. Users of Tweaker interact with the Core and need not know about the Scanner.
There are 3 types of objects that Tweaker can expose:

1. **Invokables**
  * AKA: Command
  * Are defined by annotating Method or C# event.
  * Methods and events can be static or instance members.
  * Are given a unique name. To group multiple invokables into groups and sub groups you could use dots or slashes in the name. It is up to the consuming console to parse and display these as groups if desired.
    * For Example: GameplayTuning.Player.Speed or GameplayTuning/Player/Speed 
  * Can have any number of arguments. Descriptive exceptions will be thrown if arguments are of the incorrect type.
    * The invocation of the method or event is wrapped in Try/Catch.
  * You can annotate a type as Invokable and all public methods will be registered. Useful if you wanted to group many debug commands into a single class. It is then easy to then exclude the entire class from a production build.
2. **Tweakables**
  * Are defined by annotating a property or field
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

There are 4 ways to get annotated members registered into their respective managers. Each provides a different tradeoff between performance and convenience.

1. The most convenient and performant way of using Tweaker is to annotate static members. When the Scanner scans your assemblies at startup, the static Invokables, Tweakables, and Watchables will automatically be created an registered within the corrosponding managers. 
2. There are also cases where you need to register instance members. These cannot be automatically detected and registered. It is recommended that you use TweakerFactory (or your own factory) to create objects. The factory calls Scanner.ScanInstance(object instance) to get all annoted members registered. 
3. You can also manually create instances of IInvokable, ITweakable, and IWatchable via the provided factory classes, InvokableFactory, TweakableFactory, and WatchableFactory. Those instance can then be manually registered into the managers without using the scanner and heavy reflection.
4. (not implemented yet) A generic container type can wrap a member and automatically register and unregister itself with the managers. The downside to this approach is that it requires more than just annotated members. Even with functionality stripped out in release builds it still has a small perfomance hit each time the members is accessed. It also makes your code more cumbersome. For example: `TweakableContainer<int> myIntTweakable; myIntTweakable.SetValue(999); int myInt = myIntTweakable.GetValue()`. At release, you could consider converting these containers back to regular ints.

The unit test source can be found here: https://github.com/Ghostbit/Tweaker/blob/master/Tweaker.Core.Tests/src/Tests/TweakableTest.cs. The tests demonstrate how to use Tweaker. However, below you will see an example usage of Tweaker that is better used as an example

Firstly, assume the code below is within a class called Player. We will define a static tweakable int that represents the default speed for a player. The tweakable will be constrained within a range of 0 to 100 when the value is set via Tweaker. Other parts of the game could still set DefaultSpeed to any value.
```C#
[Tweakable("Gameplay.Player.DefaultSpeed"), Range(0, 100)]
public static int DefaultSpeed { get; set; }
```

Next we define a static method to print the current DefaultSpeed. Invokables can have a return type and any number of arguments if desired. In this case there is no return type and no arguments.
```C#
[Invokable("Gameplay.Player.PrintDefaultSpeed")]
public static void PrintDefaultSpeed()
{
    Log("DefaultSpeed = " + Player.DefaultSpeed);
}
```

Next we create an instance of Tweaker to automatically scan and register the tweakable and invokable above. Once tweaker is initialized we can retrieve instances of ITweakable and IInvokable. With those instances we can change the tweakable values or execute the invokables.
```C#
public static void Main()
{
    Tweaker tweaker = new Tweaker();

    // tweaker.Init will scan and register all static tweakables, invokables, and watchables.
    // Passing null to Init will initialize with reasonable default settings.
    Tweaker.Init(null);

    ITweakable defaultSpeed = tweaker.Tweakables.GetTweakable("Gameplay.Player.DefaultSpeed");
    defaultSpeed.SetValue(50);
    Log("defaultSpeed.GetValue() = " + defaultSpeed.GetValue());

    IInvokable printDefaultSpeed = tweaker.Invokables.GetInvokable("Gameplay.Player.PrintDefaultSpeed");
    printDefaultSpeed.Invoke();
}
```

Static tweakables and invokable can be convenient but there are also times you will want to register non static members. Tweaker cannot automatically scan and register non static objects. It is up to the user to call Scanner.ScanInstance(object). Usually, it would be best to wrap object creation in a factory that calls ScanInstance for you. There is an example factory included in Tweaker called TweakerFactory. Below you will see how to define non static tweakables. Please note that the name of non static tweaker objects will have an instance id appended to the name. For example: Gameplay.Player.Nickname#1 and Gameplay.Player.Nickname#2.

First define a tweakable string for the player nickname. Note how even private members can be exposed.
```C#
[Tweakable("Gameplay.Player.Nickname")]
private string Nickname = "No-Name";
```

Now we create an instance of player and scan it in order to have the tweakable registered.
```C#
Tweaker tweaker = new Tweaker();
tweaker.Init(null);

Player player = new Player();
tweaker.Scanner.ScanInstance(player);

// Alternatively, if you use the example factory:
ITweakerFactory factory = new TweakerFactory(tweaker.Scanner);
Player player2 = factory.Create<Player>();

// We registered 2 instances of Player. By using search options we can return the first tweakable found.
ITweakable nickname = tweaker.Tweakables.GetTweakable(new SearchOptions("Gameplay.Player.Nickname"));
nickname.SetValue("Rooster");
Log(nickname.Name + " = " + nickname.GetValue());
```

Internally, all references to the scanned object are stored as weak references. Tweaker will try to prune dead
weak references at certain points. You can also manually unregister a dead tweakable.

## Assembly Scanner
The Scanner is supplemental to the Core and is not integral to the concept of Tweaker. The Scanner's responsibility is to locate and process the attributes that Invokables, Tweakables, and Watchables are annotated with. Once processed, the Tweaker API exposes methods for retrieving intermediate objects that represent the data collected and processed by the Scanner. This is demonstrated in the examples in the Core section of this page.

The source for the Scanner can be found here: https://github.com/Ghostbit/Tweaker/tree/master/Tweaker.AssemblyScanner/Source/AssemblyScanner 

Please note that the Processors that convert annotated types and members into Invokables, Tweakables, and Watchables are located in the Core. They extend a base processor interface defined by the Scanner. The Scanner API has no dependencies on Core.

The scanner is a generic way of searching for attributes, types, methods, or fields and processing them into intermediate objects defined by users of the scanner. The Core is one such user of the Scanner. The Scanner itself has no external dependenies and could be re-used in other projects. Infact, the Scanner could be it's own library/repository.
