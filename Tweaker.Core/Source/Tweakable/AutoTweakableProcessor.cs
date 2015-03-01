using Ghostbit.Tweaker.AssemblyScanner;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ghostbit.Tweaker.Core
{
    public class AutoTweakableResult
    {
        public ITweakable tweakble;
        public AutoTweakable autoTweakable;
    }

    public class AutoTweakableProcessor : IAttributeScanProcessor<Tweakable, AutoTweakableResult>
    {
        public void ProcessAttribute(Tweakable input, Type type, IBoundInstance instance = null)
        {
            
        }

        public void ProcessAttribute(Tweakable input, MemberInfo memberInfo, IBoundInstance instance = null)
        {
            ITweakable tweakable = null;
            AutoTweakableResult result = null;
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                var fieldInfo = (FieldInfo)memberInfo;
                if (fieldInfo.FieldType.IsSubclassOf(typeof(AutoTweakable)))
                {
                    AutoTweakable autoInstance = fieldInfo.GetValue(instance.Instance) as AutoTweakable;
                    FieldInfo valueFieldInfo = fieldInfo.FieldType.GetField("value", BindingFlags.Public | BindingFlags.Instance);
                    IBoundInstance boundInstance = BoundInstanceFactory.Create(autoInstance, instance.UniqueId);
                    tweakable = TweakableFactory.MakeTweakable(input, valueFieldInfo, boundInstance, memberInfo);
                    result = new AutoTweakableResult();
                    result.autoTweakable = boundInstance.Instance as AutoTweakable;
                    result.tweakble = tweakable;
                }
            }
            else
            {
                // Processing properties is possible but encourages poor design.
                // Auto tweakables should not be exposed in public properties as it
                // will begin to invade your public API. If you want to use auto tweakables
                // make fields instead of properties. You can create a property to
                // wrap the value instead. ie: 
                //      public int Value {
                //          get { return autoTweakable.value; }
                //          set { autoTweakable.value = value; } }
                throw new ProcessorException("AutoTweakableProcessor cannot process non FieldInfo types");
            }

            if(result != null)
            {
                ProvideResult(result);
            }
        }

        public event EventHandler<ScanResultArgs<AutoTweakableResult>> ResultProvided;

        private void ProvideResult(AutoTweakableResult result)
        {
            if (ResultProvided != null)
                ResultProvided(this, new ScanResultArgs<AutoTweakableResult>(result));
        }
    }
}
