using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Tweaker.Core
{
	/// <summary>
	/// All Tweaker objects implement this interface.
	/// </summary>
	public interface ITweakerObject
	{
		/// <summary>
		/// Get the description for this tweakable object.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// The name that this tweaker object registers with.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The name of this tweaker without groups included.
		/// </summary>
		string ShortName { get; }

		/// <summary>
		/// Does this tweaker object bind a public member or type?
		/// </summary>
		bool IsPublic { get; }

		/// <summary>
		/// The assembly of the type or member that this tweaker objects binds to.
		/// </summary>
		Assembly Assembly { get; }

		/// <summary>
		/// The weak reference to the instance this tweaker object is bound to.
		/// Null if bound to a static tweaker object.
		/// </summary>
		WeakReference WeakInstance { get; }

		/// <summary>
		/// The strong reference to the instance this tweaker object is bound to.
		/// Null if bound to a static tweaker object.
		/// </summary>
		object StrongInstance { get; }

		/// <summary>
		/// Indicates that the weak reference is still bound to a non-destroyed object and
		/// is in a valid state. All invalid tweaker object references should be nulled
		/// by objects holding a reference.
		/// </summary>
		bool IsValid { get; }

		/// <summary>
		/// Custom attributes that this tweaker object was annotated with. Attribute types must
		/// extend ICustomTweakerAttribute.
		/// </summary>
		ICustomTweakerAttribute[] CustomAttributes { get; }

		/// <summary>
		/// Get the custom attribute of the provided generic type.
		/// </summary>
		/// <typeparam name="TAttribute">The type of attribute to get.</typeparam>
		/// <returns>The custom attribute or null if there is no attribute of that type.</returns>
		TAttribute GetCustomAttribute<TAttribute>()
			where TAttribute : Attribute, ICustomTweakerAttribute;
	}
}
