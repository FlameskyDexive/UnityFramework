﻿using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils;
	using Serialization;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			public sealed class TimelineStateMachineEditorWindow : EditorWindow, TimelineStateMachineEditor.IEditorWindow
			{
				private static readonly string kWindowTag = "TimelineStateMachineEditor";
				private static readonly string kWindowWindowName = "State Machine";
				private static readonly string kWindowTitle = "Timeline State Machine Editor";

				private TimelineStateMachineEditor _stateMachineEditor;
				
				#region Menu Stuff
				private static TimelineStateMachineEditorWindow _instance = null;

				[MenuItem("Window/Timeline State Machine Editor")]
				private static void CreateWindow()
				{
					// Get existing open window or if none, make a new one:
					_instance = (TimelineStateMachineEditorWindow)GetWindow(typeof(TimelineStateMachineEditorWindow), false, kWindowWindowName);
				}

				[MenuItem("Assets/Load Timeline State Machine")]
				private static void MenuLoadTimeline()
				{
					TextAsset asset = Selection.activeObject as TextAsset;
					if (asset != null)
					{
						Load(asset);
					}
				}

				[MenuItem("Assets/Load Timeline State Machine", true)]
				private static bool ValidateMenuLoadTimeline()
				{
					TextAsset asset = Selection.activeObject as TextAsset;

					if (asset != null && !asset.name.StartsWith("TextConv"))
					{
						return SerializeConverter.DoesAssetContainObject<TimelineStateMachine>(asset);
					}

					return false;
				}
				#endregion

				public static void Load(TextAsset textAsset)
				{
					if (_instance == null)
						CreateWindow();

					string fileName = AssetDatabase.GetAssetPath(textAsset);
					_instance.Load(fileName);
				}

				public void Load(string fileName)
				{
					if (_instance == null)
						CreateWindow();

					CreateEditor();

					_stateMachineEditor.Load(fileName);
				}

				private void CreateEditor()
				{
					if (_stateMachineEditor == null || _stateMachineEditor.GetEditorWindow() == null)
					{
						TimelineStateMachineEditorStyle style = new TimelineStateMachineEditorStyle();
						style._defaultStateColor = new Color(61f / 255f, 154f / 255f, 92f / 255f);
						style._linkColor = Color.white;

						_stateMachineEditor = TimelineStateMachineEditor.CreateInstance<TimelineStateMachineEditor>();
						_stateMachineEditor.Init(kWindowTitle, this, kWindowTag, SystemUtils.GetAllSubTypes(typeof(IStateMachineEvent)), style);
					}
				}

				#region EditorWindow
				void Update()
				{
					if (_stateMachineEditor != null)
						_stateMachineEditor.UpdateEditor();
				}

				void OnGUI()
				{
					if (_instance == null)
						CreateWindow();

					CreateEditor();

					Vector2 windowSize = new Vector2(this.position.width, this.position.height);
					_stateMachineEditor.Render(windowSize);
				}

				void OnDestroy()
				{
					if (_stateMachineEditor != null)
						_stateMachineEditor.OnQuit();
				}
				#endregion

				#region IEditorWindow
				public void DoRepaint()
				{
					Repaint();
				}

				public void OnSelectObject(ScriptableObject obj)
				{
					Selection.activeObject = obj;
				}

				public void OnDeselectObject(ScriptableObject obj)
				{
					if (Selection.activeObject == obj)
						Selection.activeObject = null;
				}
				#endregion
			}
		}
	}
}