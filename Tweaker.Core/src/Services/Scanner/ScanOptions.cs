using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace Ghostbit.Tweaker.Core
{
    public class ScanOption<T>
    {
        public T[] ScannableRefs;
        private Regex regex;
        public string NameRegex
        {
            get { return regex != null ? regex.ToString() : string.Empty; }
            set { regex = new Regex(value); }
        }
        public bool ScanNonPublic { get; set; }
        public bool ScanPublic { get; set; }

        public ScanOption()
        {
            ScanNonPublic = true;
            ScanPublic = true;
        }

        public bool CheckRefMatch(T reference)
        {
            if (ScannableRefs != null && ScannableRefs.Length > 0)
            {
                if (!ScannableRefs.Contains(reference))
                    return false;
            }

            return true;
        }

        public bool CheckNameMatch(string name)
        {
            if (regex == null)
                return true;

            return regex.Match(name).Success;
        }
    }

    public class ScanOptions
    {
        public ScanOption<Assembly> Assemblies = new ScanOption<Assembly>();
        public ScanOption<Type> Types = new ScanOption<Type>();
        public ScanOption<Type> Attributes = new ScanOption<Type>();
        public ScanOption<MemberInfo> Members = new ScanOption<MemberInfo>();

        public bool CheckMatch(Assembly assembly)
        {
            if (!Assemblies.CheckRefMatch(assembly))
                return false;

            if (!Assemblies.CheckNameMatch(assembly.GetName().Name))
                return false;

            return true;
        }

        public bool CheckMatch(Type type)
        {
            if (!Types.CheckRefMatch(type))
                return false;

            if (!Types.CheckNameMatch(type.FullName))
                return false;

            bool isPublic = type.IsNested ? type.IsNestedPublic : type.IsPublic;

            if (Types.ScanPublic && isPublic)
                return true;

            if (Types.ScanNonPublic && !isPublic)
                return true;

            return false;
        }

        public bool CheckMatch(Attribute attribute)
        {
            var type = attribute.GetType();
            if (!Attributes.CheckRefMatch(type))
                return false;

            if (!Attributes.CheckNameMatch(type.FullName))
                return false;

            if (Attributes.ScanPublic && type.IsPublic)
                return true;

            if (Attributes.ScanNonPublic && type.IsNotPublic)
                return true;

            return false;
        }

        public bool CheckMatch(MemberInfo member)
        {
            if (!Members.CheckRefMatch(member))
                return false;

            if (!Members.CheckNameMatch(member.Name))
                return false;

            bool isPublic = true;
            var type = member.MemberType;
            switch (type)
            {
                case MemberTypes.Constructor:
                    isPublic = ((ConstructorInfo)member).IsPublic;
                    break;
                case MemberTypes.Event:
                    var reflectedType = member.ReflectedType;
                    var field = reflectedType.GetField(member.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    isPublic = field != null ? field.IsPublic : true;
                    break;
                case MemberTypes.Field:
                    isPublic = ((FieldInfo)member).IsPublic;
                    break;
                case MemberTypes.Method:
                    isPublic = ((MethodInfo)member).IsPublic;
                    break;
                case MemberTypes.Property:
                    var property = ((PropertyInfo)member);
                    isPublic = property.CanRead || property.CanWrite;
                    break;
            }

            if (Members.ScanPublic && isPublic)
                return true;

            if (Members.ScanNonPublic && !isPublic)
                return true;

            return false;
        }
    }
}