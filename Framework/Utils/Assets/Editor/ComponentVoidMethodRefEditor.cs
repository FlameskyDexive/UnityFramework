using UnityEngine;

namespace Framework
{
	using Serialization;

	namespace Utils
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(ComponentVoidMethodRef), "PropertyField")]
			public static class ComponentVoidMethodRefEditor
			{
				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					ComponentVoidMethodRef componentMethodRef = (ComponentVoidMethodRef)obj;
					return ComponentMethodRefEditor.ComponentMethodRefField(componentMethodRef._methodRef, typeof(void), label, ref dataChanged);
				}
				#endregion
			}
		}
	}
}