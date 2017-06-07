using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(Quaternion), "PropertyField")]
		public static class QuaternionEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
				Quaternion quaternion = (Quaternion)obj;
				EditorGUI.BeginChangeCheck();
				Vector3 euler = EditorGUILayout.Vector3Field(label, quaternion.eulerAngles);
				dataChanged = EditorGUI.EndChangeCheck();
				if (dataChanged)
				{
					return Quaternion.Euler(euler);
				}

				return obj;
			}
			#endregion
		}
	}
}