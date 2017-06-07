using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(bool), "PropertyField")]
		public static class BoolEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.Toggle(label, (bool)obj);
				dataChanged = EditorGUI.EndChangeCheck();
				return obj;
			}
			#endregion
		}
	}
}