using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Eruru.Command {

	public class CommandSystem<Tag> {

		public event Action<Tag> NoPermission;
		public bool IgnoreCase { get; set; } = true;

		readonly object CommandsLock = new object ();
		readonly List<Command> Commands = new List<Command> ();

		public void Register<T> () where T : class {
			lock (CommandsLock) {
				foreach (MethodInfo methodInfo in typeof (T).GetMethods (BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
					Command command = CommandAPI.GetCustomAttribute<Command> (methodInfo);
					if (command is null) {
						continue;
					}
					command.MethodInfo = methodInfo;
					Commands.Add (command);
				}
			}
		}

		public bool Execute (Tag tag, int permissionLevel, string name, params object[] args) {
			lock (CommandsLock) {
				foreach (Command command in Commands) {
					if (!CommandAPI.NameEquals (name, command.Name, IgnoreCase)) {
						continue;
					}
					int argsLength = command.Parameters.Length - 1;
					if (args.Length < argsLength) {
						continue;
					}
					if (argsLength == 0 && args.Length > argsLength) {
						continue;
					}
					try {
						for (int i = 0; i < command.Parameters.Length; i++) {
							if (i == 0) {
								command.Parameters[i] = tag;
								continue;
							}
							Type parameterType = command.ParameterInfos[i].ParameterType;
							if (i == command.Parameters.Length - 1 && args.Length >= argsLength && parameterType.IsArray) {
								Type elementType = parameterType.GetElementType ();
								Array array = Array.CreateInstance (elementType, args.Length - argsLength + 1);
								for (int n = 0; n < array.Length; n++) {
									array.SetValue (Convert.ChangeType (args[argsLength + n - 1], elementType), n);
								}
								command.Parameters[i] = array;
								continue;
							}
							command.Parameters[i] = Convert.ChangeType (args[i - 1], parameterType);
						}
					} catch {
						continue;
					}
					if (permissionLevel < command.PermissionLevel) {
						NoPermission?.Invoke (tag);
						continue;
					}
					command.Invoke ();
					return true;
				}
				return false;
			}
		}
		public bool Execute (Tag tag, string name, params object[] args) {
			return Execute (tag, default, name, args);
		}
		public bool Execute (int permissionLevel, string name, params object[] args) {
			return Execute (default, permissionLevel, name, args);
		}
		public bool Execute (string name, params object[] args) {
			return Execute (default, default, name, args);
		}
		public bool Execute (int permissionLevel, string name) {
			return Execute (default, permissionLevel, name);
		}
		public bool Execute (Tag tag, string name) {
			return Execute (tag, default, name);
		}
		public bool Execute (string name) {
			return Execute (default, default, name);
		}
		public bool ExecuteText (string text, Tag tag, int permissionLevel) {
			string name = null;
			List<string> args = new List<string> ();
			for (int i = 0; i < text.Length; i++) {
				if (char.IsWhiteSpace (text[i])) {
					continue;
				}
				string value = Regex.Unescape (ReadString (text, ref i));
				if (name is null) {
					name = value;
				} else {
					args.Add (value);
				}
			}
			if (name is null) {
				return false;
			}
			return Execute (tag, permissionLevel, name, args.ToArray ());
		}
		public bool ExecuteText (string text, Tag tag) {
			return ExecuteText (text, tag, default);
		}
		public bool ExecuteText (string text, int permissionLevel) {
			return ExecuteText (text, default, permissionLevel);
		}
		public bool ExecuteText (string text) {
			return ExecuteText (text, default, default);
		}

		public void ForEach (Action<Command> action) {
			lock (CommandsLock) {
				foreach (Command command in Commands) {
					action (command);
				}
			}
		}

		public void Rename (string oldName, string newName, bool isMethodName = true) {
			lock (CommandsLock) {
				foreach (Command command in Commands) {
					if (CommandAPI.NameEquals (isMethodName ? command.MethodInfo.Name : command.Name, oldName, IgnoreCase)) {
						command.Name = newName;
					}
				}
			}
		}

		public void Clear () {
			lock (CommandsLock) {
				Commands.Clear ();
			}
		}

		string ReadString (string text, ref int i) {
			char end;
			switch (text[i]) {
				case '"':
					end = '"';
					i++;
					break;
				case '\'':
					end = '\'';
					i++;
					break;
				default:
					end = ' ';
					break;
			}
			StringBuilder stringBuilder = new StringBuilder ();
			for (; i < text.Length; i++) {
				if (text[i] == end) {
					break;
				}
				switch (text[i]) {
					case '\\':
						stringBuilder.Append (text[i++]);
						if (i < text.Length) {
							stringBuilder.Append (text[i]);
						}
						break;
					default:
						stringBuilder.Append (text[i]);
						break;
				}
			}
			return stringBuilder.ToString ();
		}

	}

}