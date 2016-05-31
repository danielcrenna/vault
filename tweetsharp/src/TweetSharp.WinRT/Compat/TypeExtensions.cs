using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TweetSharp.WinRT.Compat
{
	public static class TypeExtensions
	{

		public static bool IsAssignableFrom(this Type sourceType, Type type)
		{
			return sourceType.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
		}

		public static Type GetInterface(this Type sourceType, string name, bool ignoreCase)
		{
			return (from i in sourceType.GetTypeInfo().ImplementedInterfaces
							where String.Equals(name, i.FullName, (ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
							select i).FirstOrDefault();
		}

	}
}
