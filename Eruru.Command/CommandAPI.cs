using System;
using System.Reflection;

namespace Eruru.Command {

	public delegate void CommandAction ();

	static class CommandAPI {

		public static T GetCustomAttribute<T> (MethodInfo methodInfo) where T : Attribute {
			object[] attributes = methodInfo.GetCustomAttributes (typeof (T), false);
			return attributes.Length > 0 ? (T)attributes[0] : null;
		}

		public static bool NameEquals (string a, string b, bool ignoreCase) {
			return a.Equals (b, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
		}

	}

}