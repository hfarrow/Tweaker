using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghostbit.Tweaker.Core
{
    public class ToggleTweakable<T> : IToggleTweakable
    {
        private readonly BaseTweakable<T> baseTweakable;
        public BaseTweakable<T> BaseTweakable { get { return baseTweakable; } }

        private int currentIndex = -1;
        private TweakableInfo<T> tweakableInfo;

        public ToggleTweakable(BaseTweakable<T> baseTweakable)
        {
            this.baseTweakable = baseTweakable;
            tweakableInfo = baseTweakable.TweakableInfo;
        }

        public object StepSize
        {
            get { return 1; }
        }

        public int CurrentIndex
        {
            get { return currentIndex; }
        }

        public int GetIndexOfValue(object value)
        {
            for (int i = 0; i < tweakableInfo.ToggleValues.Length; ++i)
            {
                var toggleValue = tweakableInfo.ToggleValues[i];
                if (toggleValue.Value.Equals(value))
                {
                    return i;
                }
            }
            return -1;
        }

        public string GetNameByIndex(int index)
        {
            if (index >= 0 && tweakableInfo.ToggleValues.Length > index)
                return tweakableInfo.ToggleValues[index].Name;
            else
                return null;
        }

        public string GetNameByValue(object value)
        {
            return GetNameByIndex(GetIndexOfValue(value));
        }

        public object SetValueByName(string valueName)
        {
            for (int i = 0; i < tweakableInfo.ToggleValues.Length; ++i)
            {
                if (tweakableInfo.ToggleValues[i].Name == valueName)
                {
                    currentIndex = i;
                    baseTweakable.SetValue(tweakableInfo.ToggleValues[i].Value);
                    return baseTweakable.GetValue();
                }
            }

            throw new TweakableSetException(baseTweakable.Name, "Invalid toggle value name: '" + valueName + "'");
        }

        public string GetValueName()
        {
            if (currentIndex >= 0 && currentIndex < tweakableInfo.ToggleValues.Length)
            {
                return tweakableInfo.ToggleValues[currentIndex].Name;
            }
            return null;
        }

        public object StepNext()
        {
            currentIndex++;
            if (currentIndex >= tweakableInfo.ToggleValues.Length)
                currentIndex = 0;

            var nextValue = tweakableInfo.ToggleValues[currentIndex].Value;
            baseTweakable.SetValue(nextValue);
            return baseTweakable.GetValue();
        }

        public object StepPrevious()
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = tweakableInfo.ToggleValues.Length - 1;

            var nextValue = tweakableInfo.ToggleValues[currentIndex].Value;
            baseTweakable.SetValue(nextValue);
            return baseTweakable.GetValue();
        }

        public Type TweakableType { get { return typeof(T); } }
    }
}
