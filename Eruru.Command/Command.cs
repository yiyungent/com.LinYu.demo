using System;
using System.Reflection;

namespace Eruru.Command {

	[AttributeUsage (AttributeTargets.Method)]
	public class Command : Attribute {

		public string Name { get; set; }
		public object Tag { get; set; }
		public int PermissionLevel { get; set; }
		public MethodInfo MethodInfo {

			get => _MethodInfo;

			set {
				_MethodInfo = value;
				Name = Name ?? value.Name;
				ParameterInfos = value.GetParameters ();
				Parameters = new object[ParameterInfos.Length];
			}

		}
		public ParameterInfo[] ParameterInfos { get; private set; }
		public object[] Parameters { get; private set; }

		MethodInfo _MethodInfo;

		public Command () {

		}
		public Command (object tag) {
			Tag = tag;
		}
		public Command (int permissionLevel) {
			PermissionLevel = permissionLevel;
		}
		public Command (object tag, int permissionLevel) {
			Tag = tag;
			PermissionLevel = permissionLevel;
		}
		public Command (string name) {
			Name = name;
		}
		public Command (string name, object tag) {
			Name = name;
			Tag = tag;
		}
		public Command (string name, int permissionLevel) {
			Name = name;
			PermissionLevel = permissionLevel;
		}
		public Command (string name, object tag, int permissionLevel) {
			Name = name;
			Tag = tag;
			PermissionLevel = permissionLevel;
		}

		public T GetTag<T> () {
			return (T)Tag;
		}

		public void Invoke () {
			MethodInfo.Invoke (null, Parameters);
		}

	}

}