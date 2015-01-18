using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Ghostbit.Tweaker.Core
{
    public class SearchOptions
    {
        public enum ScopeType
        {
            NonPublic,
            Public,
            All
        }

        public Regex NameRegex { get; set; }
        public Regex AssemblyRegex { get; set; }
        public ScopeType Scope { get; set; }

        public SearchOptions(string nameRegex = null, string assemblyRegex = null, ScopeType scope = ScopeType.All)
        {
            NameRegex = nameRegex == null ? null : new Regex(nameRegex);
            AssemblyRegex = assemblyRegex == null ? null : new Regex(assemblyRegex);
            Scope = scope;
        }

        public bool CheckMatch(string name, MethodInfo info)
        {
            return CheckMatch(name, info.ReflectedType.Assembly, info.IsPublic ? ScopeType.Public : ScopeType.NonPublic);
        }

        public bool CheckMatch(string name, FieldInfo info)
        {
            return CheckMatch(name, info.ReflectedType.Assembly, info.IsPublic ? ScopeType.Public : ScopeType.NonPublic);
        }

        public bool CheckMatch(string name, Assembly assembly, ScopeType scope)
        {
            if (Scope != ScopeType.All && Scope != scope)
            {
                return false;
            }

            if (NameRegex != null && name != null && !NameRegex.Match(name).Success)
            {
                return false;
            }

            if (AssemblyRegex != null && assembly != null && !AssemblyRegex.Match(assembly.FullName).Success)
            {
                return false;
            }

            return true;
        }
    }
}