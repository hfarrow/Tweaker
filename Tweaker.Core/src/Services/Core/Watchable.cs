using System.Collections;

namespace Ghostbit.Tweaker.Core
{
    public interface IWatchable : ITweakerObject
    {
    }

    public abstract class WatchableInfo
    {
        public enum DisplayMode
        {
            Value, // Show the current value. use ToString()
            ValueGraph, // Show a graph of the value over time. May require a plugin for user defined types.
            Delta, // Show the change of value over time. May require a plugin for user defined types.
            DeltaGraph // Show a graph of the value change over time. May require a plugin for user defined types.
        }

        public string Name { get; private set; }
        public DisplayMode Mode { get; private set; }

        public WatchableInfo(string name, DisplayMode mode = DisplayMode.Value)
        {
            Name = name;
            Mode = mode;
        }
    }
}