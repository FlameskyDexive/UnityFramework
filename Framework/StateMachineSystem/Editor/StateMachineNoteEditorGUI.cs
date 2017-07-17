using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		namespace Editor
		{
			[StateCustomEditorGUI(typeof(StateMachineNote))]
			public class StateMachineNoteEditorGUI : StateEditorGUI
			{
				public override bool RenderObjectProperties(GUIContent label)
				{
					EditorGUI.BeginChangeCheck();

					State state = GetEditableObject();

					state._editorDescription = EditorGUILayout.TextArea(state._editorDescription);

					return EditorGUI.EndChangeCheck();
				}

				protected override string GetStateIdLabel()
				{
					return "Note";
				}

				public override Color GetColor(StateMachineEditorStyle style)
				{
					return style._noteColor;
				}

				public override float GetBorderSize(bool selected)
				{
					return selected ? 1.0f : 0.0f;
				}

				public override GUIStyle GetTextStyle(StateMachineEditorStyle style)
				{
					return style._noteTextStyle;
				}
			}
		}
	}
}