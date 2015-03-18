# About Tweaker
The intention of Tweaker is to provide a system that discovers and processes custom attributes (annotations) into a collection of intermediate objects. Tweaker was created with the intention of being used as the "back end" of a debug console/gui for games. Tweaker itself does not include a console or gui but simply provides a data model exposing **Invokables**, **Tweakables**, and **Watchables** for use by a debug console. It is up to the console to present and manipulate the Invokables, Tweakables, and Watchables provided by Tweaker.

# Description
Tweaker can be broken down into 2 main components. The **Core** and **AssemblyScanner** (referred to as Scanner)

## Core
The meat and potatoes of Tweaker. Users of Tweaker interact with the Core. The Core has dependencies on the Scanner but for the most part that dependency is hidden from users.
There are 3 types of objects that Tweaker can expose:

1. **Invokables**
  * AKA: Command
  * Are defined by annotating methods or events.
  * Methods and events can be static or instance members.
  * Are given a unique name. To group multiple invokables into groups and sub groups you use dots in the name. It is up to the consuming console to parse and display these as groups if desired.
    * For Example: GameplayTuning.Player.Speed or GameplayTuning/Player/Speed 
  * Can have any number of arguments. Descriptive exceptions will be thrown if arguments are of the incorrect type.
    * The invocation of the method or event is wrapped in Try/Catch.
  * You can annotate a type as Invokable and all public methods will be registered. Useful if you wanted to group many debug commands into a single class. It is then easy to then exclude the entire class from a production build.
  * Optional description annotations can be added to the invokable, the return value, and each argument. A console can then display those descriptions as help strings.
2. **Tweakables**
  * Are defined by annotating a property or field
  * Additional annotations can optionally be added to specify a min and max value or a list of valid named toggle values.
    * Values outside min and max are automatically clamped.
    * API is exposed that allows you to go forward or backwards through toggle values. Will wrap at the end values.
    * Define a step for values to be incremented or decremented by.
  * Grouped the same way as Invokables
  * You can annotate a type as Tweakable and all public methods and fields will be registered.
3. **Watchable**
  * **Not implemented currently and purely conceptual at this time.**
    * The idea here it to annotate properties or fields that you want to monitor overtime
  * A console could simply display the value of a watchable in a special readonly manner (Tweakables by definition are read/write).
  * A console could show a graph of the value over time
  * A console could show a graph of the change of the value over time (delta).
  * A console could group multiple Watchables into the same graph.
  * Bar graphs, line graphs, piechart, etc
  * For custom types (non integral) a plugin system could be devised that allows other objects to be graphed or allow textures / render targets to be displayed. Basically add plugins for rendering/watching custom types

There are 4 ways to get annotated members registered into their respective managers. Each provides a different tradeoff between performance and convenience. Further down, you will ready about how to use each of these methods.

1. The most convenient and performant way of using Tweaker is to annotate static members. When the Scanner scans your assemblies at startup, the static Invokables, Tweakables, and Watchables will automatically be created an registered within the corrosponding managers. 
2. There are also cases where you need to register instance members. These cannot be automatically detected and registered. It is recommended that you use TweakerFactory (or your own factory) to create objects. The factory calls Scanner.ScanInstance(object instance) to get all annoted members registered. Alternatively, you can use the generic container method described below.
3. You can also manually create instances of IInvokable, ITweakable, and IWatchable via the provided factory classes, InvokableFactory, TweakableFactory, and WatchableFactory. Those instance can then be manually registered into the managers without using the scanner and heavy reflection.
4. A generic container type can wrap a member and automatically register and unregister itself with the managers. The downside to this approach is that it requires more than just annotated members. Even with functionality stripped out in release builds it still has a small perfomance hit each time the members is accessed. It also makes your code more cumbersome. For example: `Tweakable<int> myIntTweakable; myIntTweakable.SetTweakableValue(999); int myInt = myIntTweakable.value`. At release, you could consider converting these containers back to regular integers.

The unit test source can be found here: https://github.com/Ghostbit/Tweaker/blob/master/Tweaker.Core.Tests/src/Tests/TweakableTest.cs.

## Assembly Scanner
The Scanner is supplemental to the Core and is not integral to the concept of Tweaker. The Scanner's responsibility is to locate and process the attributes that Invokables, Tweakables, and Watchables are annotated with. Once processed, the Tweaker API exposes methods for retrieving intermediate objects that represent the data collected and processed by the Scanner. This is demonstrated in the examples in the Core section of this page.

The source for the Scanner can be found here: https://github.com/Ghostbit/Tweaker/blob/master/Tweaker.AssemblyScanner/Source/Scanner.cs 

Please note that the Processors that convert annotated types and members into Invokables, Tweakables, and Watchables are located in the Core. They extend a base processor interface defined by the Scanner. The Scanner API has no dependencies on Core.

The scanner is a generic way of searching for attributes, types, methods, or fields and processing them into intermediate objects defined by users of the scanner. The Core is one such user of the Scanner. The Scanner itself has no external dependencies and could be re-used in other projects. In fact, the Scanner could be it's own library/repository.

# Examples
This section contains examples of several ways to use tweaker as described early in this document.

## Static Invokables and Tweakables
This is the simplest and most straight forward method of registering Invokables, Tweakables, and Watchables. The only requirement is that the methods, events, properties, and fields must be static. This is useful console commands that do not need to be bound to object instances. For this example, we will define a class called "DebugCommands" that annotates mothods and properties with various Tweaker attributes required to define and register Invokables and Tweakables.
```C#
public static class DebugCommands
{
    [Invokable("DebugCommands.LoadLevel", Description="Loads the specified level name.")]
    [return: ReturnDescription("true if the level will be loaded.")]
    public static bool LoadLevel([ArgDescription("The name of the level to load."] string levelName)
    {
        Debug.Log("Loading Level: " + levelName);
        // Logic to load a new level here...
    }

    [Tweakable("DebugCommand.TargetEnvironment")]
    public static string TargetEnvironment { get; set; }
}
```
After annotating the static members you want to register with tweaker, it is time to initialize tweaker. Tweaker will scan your loaded assemblies and automatically bind your annotated members to IInvokable or ITweakable instances and register them to their respective managers.
```C#
static void Main()
{
    Tweaker tweaker = new Tweaker();

    // tweaker.Init will scan and register all static tweakables, invokables..
    // Passing null to Init will initialize with reasonable default settings.
    Tweaker.Init(null);

    // Once the tweakable has been automatically registered by the scanner, we can retrieve the objects 
    // that are bound to your annotated members in the DebugCommands class.
    IInvokable loadLevelInvokable = tweaker.Invokables.GetInvokable("DebugCommands.LoadLevel");

    // Invoking the invokable will run the method or event that it is bound to.
    loadLevelInvokable.Invoke("LevelNameHere");

    // Alternatively, you can invoke without retrieving the invokable first
    tweaker.Invokables.Invoke("DebugCommands.LoadLevel", "LevelNameHere");

    // The same pattern as above exists for tweakables.
    ITweakable targetEnvironmentTweakable = tweaker.Tweakables.GetTweakable("DebugCommands.TargetEnvironment");
    string currentTarget = targetEnvironmentTweakable.GetValue() as string;
    targetEnvironmentTweakable.SetValue("QA");

    // or...
    tweaker.Tweakables.SetValue("DebugCommands.TargetEnvironment", "QA");

    // To enumerate all bound objects:
    var invokables = tweaker.Invokables.GetInvokables(null);
}
```

## Instance Tweakables - Factory Method
Because tweaker only scans and registers static members, there are several ways to register annotated instance members. The first is to use a factory to create all objects that contain annotated members. Tweaker provides an example factory that will create an object and call `Tweaker.Scanner.ScanInstance(obj)`. Internally tweaker only holds weak a weak reference to the scanned object. When tweaker detects that the weak reference has been garbage collected, the registered object will automatically be unregistered. To be safe, you should always check that the tweakable is valid before using it. `if(ITweakable.IsValid){...}`. One last important detail is that the name of instance tweakables will have an instance id appended. For example if the tweakable name is "Player.Nickname", the name that actually gets registered might be "Player.Nickname#1".
```C#
Tweaker tweaker = new Tweaker();
Tweaker.Init(null);

ITweakerFactory factory = new TweakerFactory(Tweaker.Scanner);
MyClass obj = factory.Create<MyClass>("arg1", "arg2", "etc");

// Any Invokables or Tweakables defined in 'MyClass' can now be retrieved through Tweaker.Invokables or Tweaker.Tweakables
```

## Instance Tweakables - Generic Container Method
If you dislike the factory method above, you can create container objects that automatically register with tweaker. When the container is finalized (garbage collected), it will automatically unregister. This method has the downside that you must litter 'Tweakable<T>' types in your code. However, it does not require using a factory and the tweaker objects are unregistered immediately unlike the factory method where unregistering is deferred to later. Another downside to this method is that accessing the container values adds abstraction when reading or writing the value. It is up to the user to decide what method meets their needs best. When using the generic container, you must call AutoTweakable.Bind(this) to have the tweakables bound to the correct instance.
```C#
public class Player
{
    [Tweakable("Player.Nickname", Description="The Debug name assigned to the player.")]
    private Tweakable<string> nickname = new Tweakable<string>("not-set");

    public Player()
    {
        // IMPORTANT - must have or the tweakable will not be registered.
        // Consider moving to a base class and only call if not a production build.
        AutoTweakable.Bind(this);
    }
}

public void Main()
{
    Tweaker tweaker = new Tweaker();
    tweaker.Init();

    Player player = new Player();
    // Since this is the first player instance created, we can assume the appended instance id is 1.
    tweaker.Tweakables.SetTweakableValue("Player.Nickname#1", "Player One");
}
```

## Snippets
### Range Tweakable
```C#
[Tweakable("TweakableWithRange", Description="This tweakable in clamped within a min and max value."),
Range(0, 100)]
public int playerHealth = 100;
// Note: Setting playerHealth directly in code will not enforce the range. However, when set via ITweakable, the range will be enforced.
```
### Step Size Tweakable
```C#
[Tweakable("TweakableWithStep", Description="This tweakable is incremented in fixed steps."),
StepSize(10)]
public int scoreMultiplier = 1;
// Note: Use ITweakable.Step.StepNext/StepPrevious to use the StepSize. If there is no StepSize attribute, ITweakable.Step will be null.
// The step size can be ignored if you set the value directly via ITweakable.SetValue(...)
```
### Named Toggle Tweakable
```C#
[Tweakable("TweakableWithNamedToggleValues", Description="This tweakable has 3 possible values with names assigned to them."),
NamedToggleValue("easy", 0, 0),
NamedToggleValue("medium", 1, 1)
NamedToggleValue("hard", 2, 2)]
public int difficulty = 0;
// Note: The 2nd parameter is the value to assign to the field.
// The 3rd parameter is the order index of the toggle value. The order define the attributes may not be the order
// they are processed in. Because of this you must explicitly index of each value.
// Use ITweakable.Toggle.StepNext/StepPrevious to cycle through toggle values. See IToggleTweakable for other
// useful methods such as GetValueName() or SetValueByName(string valueName).
```
### Search Tweakbles By Group
```C#
Tweaker tweaker = new Tweaker();
tweaker.Init(null);

var tweakables = tweaker.Tweakables.GetTweakables(new SearchOptions(@"^DebugCommands\.Player"));
// Note: The string passed into SearchOptions is a regex.
````
### Search For Static Tweakables
```C#
Tweaker tweaker = new Tweaker();
tweaker.Init(null);

var tweakables = tweaker.Tweakables.GetTweakables(new SearchOptions(null, null, SearchOptions.ScopeType.All, SearchOptions.BindingType.Static));
```
### Search For Tweakables Belonging To Instance
```C#
Tweaker tweaker = new Tweaker();
tweaker.Init(null);

var obj = new ClassWithInstanceTweakables();

var tweakables = tweaker.Tweakables.GetTweakables(new SearchOptions(null, null, SearchOptions.ScopeType.All, SearchOptions.BindingType.Instance, obj));
```
### Search For Tweakables Belonging To Assembly
```C#
Tweaker tweaker = new Tweaker();
tweaker.Init(null);

var tweakables = tweaker.Tweakables.GetTweakables(new SearchOptions(null, "^MyAssemblyNameHere"));
```
