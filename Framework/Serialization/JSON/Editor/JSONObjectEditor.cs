using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

namespace Engine
{
	using Utils.Editor;
	
	namespace JSON
	{
		public abstract class JSONObjectEditor<T> : ScriptableObject where T : class
		{
			public interface IEditorWindow
			{
				void DoRepaint();
				void OnSelectObject(ScriptableObject obj);
				void OnDeselectObject(ScriptableObject obj);
			}

			#region Protected Data
			protected enum eDragType
			{
				NotDragging,
				LeftClick,
				MiddleMouseClick,
				Custom,
			}
			protected eDragType _dragMode = eDragType.NotDragging;
			protected JSONObjectEditorGUI<T> _draggedObject;
			protected Vector2 _dragPos = Vector2.zero;
			protected Rect _dragAreaRect;

			//Note these should all be JSONObjectEditorGUI<T> but unity won't serialize them (and thus make them usable in Undo actions) if they are a template class
			[SerializeField]
			protected List<ScriptableObject> _editableObjects = new List<ScriptableObject>();
			[SerializeField]
			protected List<ScriptableObject> _selectedObjects = new List<ScriptableObject>();
			[SerializeField]
			protected List<ScriptableObject> _copiedObjects = new List<ScriptableObject>();
			#endregion

			#region Private Data
			private enum eRightClickOperation
			{
				Copy,
				Paste,
				Cut,
				Remove,
			}

			private class RightClickData
			{
				public eRightClickOperation _operation;
				public JSONObjectEditorGUI<T> _editableObject;
				public Type _editableObjectType;
			}
			private IEditorWindow _editorWindow;
			private int _controlID;
			private bool _needsRepaint;
			private bool _isDirty;
			private List<ScriptableObject> _cachedEditableObjects = new List<ScriptableObject>();
			#endregion

			#region Public Methods
			public JSONObjectEditor(IEditorWindow editorWindow)
			{
				_editorWindow = editorWindow;
				Undo.undoRedoPerformed += UndoRedoCallback;
				_controlID = GUIUtility.GetControlID(FocusType.Passive);
			}

			~JSONObjectEditor()
			{
				Undo.undoRedoPerformed -= UndoRedoCallback;
			}

			public bool NeedsRepaint()
			{
				return _needsRepaint;
			}

			public bool HasChanges()
			{
				if (_isDirty)
					return true;

				foreach (JSONObjectEditorGUI<T> editorGUI in _editableObjects)
				{
					if (editorGUI.IsDirty())
						return true;
				}

				return false;
			}

			public void ClearDirtyFlag()
			{
				_isDirty = false;

				foreach (JSONObjectEditorGUI<T> editorGUI in _editableObjects)
				{
					editorGUI.MarkAsDirty(false);
				}
			}

			public void SetNeedsRepaint(bool needsRepaint = true)
			{
				_needsRepaint = needsRepaint;
			}

			public IEditorWindow GetEditorWindow()
			{
				return _editorWindow;
			}
			#endregion

			#region Protected Functions
			protected void ClearObjects()
			{
				foreach (JSONObjectEditorGUI<T> editorGUI in _editableObjects)
				{
					Undo.ClearUndo(editorGUI);
					GetEditorWindow().OnDeselectObject(editorGUI);
				}

				_editableObjects.Clear();
				_selectedObjects.Clear();
				_copiedObjects.Clear();
				_draggedObject = null;

				Undo.ClearUndo(this);

				ClearDirtyFlag();
			}

			protected JSONObjectEditorGUI<T> AddNewObject(T obj)
			{
				JSONObjectEditorGUI<T> editorGUI = CreateObjectEditorGUI(obj);
				editorGUI.SetEditableObject(obj);

				_editableObjects.Add(editorGUI);
				SortObjects();

				UpdateCachedObjectList();

				return editorGUI;
			}

			protected void RemoveObject(JSONObjectEditorGUI<T> editorGUI)
			{
				GetEditorWindow().OnDeselectObject(editorGUI);
				_editableObjects.Remove(editorGUI);
				_selectedObjects.Remove(editorGUI);
				UpdateCachedObjectList();
			}

			protected void MarkAsDirty()
			{
				_isDirty = true;
				EditorUtility.SetDirty(this);
			}

			protected void SortObjects()
			{
				_editableObjects.Sort((a, b) => (((JSONObjectEditorGUI<T>)a).CompareTo((JSONObjectEditorGUI<T>)b)));
			}
			#endregion

			#region Abstract Interface
			protected abstract void OnCreatedNewObject(T obj);

			protected abstract JSONObjectEditorGUI<T> CreateObjectEditorGUI(T obj);

			protected abstract T CreateCopyFrom(JSONObjectEditorGUI<T> editorGUI);

			protected abstract void ZoomEditorView(float amount);

			protected abstract void ScrollEditorView(Vector2 delta);

			protected abstract void DragObjects(Vector2 delta);

			protected abstract void AddContextMenu(GenericMenu menu);

			protected abstract void SetObjectPosition(JSONObjectEditorGUI<T> obj, Vector2 position);
			#endregion

			#region Virtual Interface
			protected virtual void OnLeftMouseDown(Event inputEvent)
			{
				JSONObjectEditorGUI<T> clickedOnObject = null;

				Vector2 gridPosition = GetEditorPosition(inputEvent.mousePosition);

				for (int i = 0; i < _editableObjects.Count; i++)
				{
					JSONObjectEditorGUI<T> evnt = (JSONObjectEditorGUI<T>)_editableObjects[i];
					if (evnt.GetBounds().Contains(gridPosition))
					{
						clickedOnObject = evnt;
						break;
					}
				}

				if (inputEvent.shift || inputEvent.control)
				{
					if (clickedOnObject != null)
					{
						if (_selectedObjects.Contains(clickedOnObject))
						{
							_selectedObjects.Remove(clickedOnObject);
							clickedOnObject = null;
						}
						else
						{
							_selectedObjects.Add(clickedOnObject);
						}
					}
				}
				else if (clickedOnObject == null)
				{
					_selectedObjects.Clear();
				}
				else if (!_selectedObjects.Contains(clickedOnObject))
				{
					_selectedObjects = new List<ScriptableObject>() { clickedOnObject };
				}

				//Dragging
				{
					GetEditorWindow().OnSelectObject(clickedOnObject);

					_draggedObject = clickedOnObject;
					_dragMode = eDragType.LeftClick;
					_dragPos = inputEvent.mousePosition;
					_dragAreaRect = new Rect(-1.0f, -1.0f, 0.0f, 0.0f);

					//Save state before dragging
					foreach (JSONObjectEditorGUI<T> evnt in _selectedObjects)
					{
						evnt.SaveUndoState();
					}
				}
			}

			protected virtual void OnMiddleMouseDown(Event inputEvent)
			{
				_dragMode = eDragType.MiddleMouseClick;
				_dragPos = inputEvent.mousePosition;
				_dragAreaRect = new Rect(-1.0f, -1.0f, 0.0f, 0.0f);
			}

			protected virtual void OnRightMouseDown(Event inputEvent)
			{
				JSONObjectEditorGUI<T> clickedNode = null;
				_dragPos = GetEditorPosition(inputEvent.mousePosition);
				
				//Check clicked on event
				Vector2 gridPosition = GetEditorPosition(inputEvent.mousePosition);

				foreach (JSONObjectEditorGUI<T> node in _editableObjects)
				{
					if (node.GetBounds().Contains(gridPosition))
					{
						clickedNode = node;
						break;
					}
				}

				if (clickedNode != null)
				{
					if (!_selectedObjects.Contains(clickedNode))
						_selectedObjects = new List<ScriptableObject>() { clickedNode };

					GetEditorWindow().DoRepaint();
				}

				// Now create the menu, add items and show it
				GenericMenu menu = GetRightMouseMenu(clickedNode);
				menu.ShowAsContext();
			}

			protected virtual Vector2 GetEditorPosition(Vector2 screenPosition)
			{
				return screenPosition;
			}

			protected virtual Rect GetEditorRect(Rect screenRect)
			{
				return screenRect;
			}

			protected virtual Rect GetScreenRect(Rect editorRect)
			{
				return editorRect;
			}

			protected virtual void OnStopDragging(Event inputEvent)
			{
				if (_dragMode != eDragType.NotDragging)
				{
					if (_dragMode == eDragType.LeftClick)
					{
						Undo.RegisterCompleteObjectUndo(_selectedObjects.ToArray(), "Move Objects(s)");

						foreach (JSONObjectEditorGUI<T> evnt in _selectedObjects)
						{
							evnt.SaveUndoState();
						}
					}

					SortObjects();
					inputEvent.Use();
					_dragMode = eDragType.NotDragging;
					_needsRepaint = true;
				}
			}

			protected virtual void OnDragging(Event inputEvent)
			{
				Vector2 currentPos = inputEvent.mousePosition;

				if (_dragMode == eDragType.LeftClick)
				{
					_needsRepaint = true;

					inputEvent.Use();

					if (_draggedObject != null)
					{
						Vector2 delta = currentPos - _dragPos;
						_dragPos = currentPos;

						DragObjects(delta);
					}
					else
					{
						_dragAreaRect.x = Math.Min(currentPos.x, _dragPos.x);
						_dragAreaRect.y = Math.Min(currentPos.y, _dragPos.y);
						_dragAreaRect.height = Math.Abs(currentPos.y - _dragPos.y);
						_dragAreaRect.width = Math.Abs(currentPos.x - _dragPos.x);

						_selectedObjects.Clear();

						Rect gridDragRect = GetEditorRect(_dragAreaRect);

						for (int i = 0; i < _editableObjects.Count; i++)
						{
							JSONObjectEditorGUI<T> editorGUI = (JSONObjectEditorGUI<T>)_editableObjects[i];
							if (editorGUI.GetBounds().Overlaps(gridDragRect))
							{
								_selectedObjects.Add(editorGUI);
							}
						}
					}
				}
				else if (_dragMode == eDragType.MiddleMouseClick)
				{
					Vector2 delta = currentPos - _dragPos;
					_dragPos = currentPos;

					ScrollEditorView(delta);

					SetNeedsRepaint();
				}
			}
			#endregion

			#region Private Functions
			protected void HandleInput()
			{
				Event inputEvent = Event.current;

				if (inputEvent == null)
					return;
				
				EventType controlEventType = inputEvent.GetTypeForControl(_controlID);
				Vector2 mousePosition = GetEditorPosition(inputEvent.mousePosition);

				if (_dragMode != eDragType.NotDragging && inputEvent.rawType == EventType.MouseUp)
				{
					OnStopDragging(inputEvent);
					_needsRepaint = true;
				}

				switch (controlEventType)
				{
					case EventType.MouseDown:
						{
							OnMouseDown(inputEvent);
						}
						break;

					case EventType.MouseUp:
						{
							OnStopDragging(inputEvent);
							_needsRepaint = true;
						}
						break;

					case EventType.ContextClick:
						{
							inputEvent.Use();
							OnRightMouseDown(inputEvent);
						}
						break;

					case EventType.mouseDrag:
						{
							OnDragging(inputEvent);
						}
						break;

					case EventType.ScrollWheel:
						{
							#region Zooming via Mouse Wheel
							float zoomDelta = -inputEvent.delta.y;
							ZoomEditorView(zoomDelta);
							_needsRepaint = true;
							#endregion
						}
						break;

					case EventType.KeyDown:
						{
							#region Scrolling via Keys (To do)						
							#endregion
						}
						break;

					case EventType.ValidateCommand:
						{
							if (inputEvent.commandName == "SoftDelete")
							{
								DeleteSelected();
							}
							else if (inputEvent.commandName == "SelectAll")
							{
								SelectAll();
							}
							else if (inputEvent.commandName == "Cut")
							{
								CutSelected();
							}
							else if (inputEvent.commandName == "Copy")
							{
								CopySelected();
							}
							else if (inputEvent.commandName == "Paste")
							{
								Paste();
							}
							else if (inputEvent.commandName == "Duplicate")
							{
								CopySelected();
								Paste();
							}
						}
						break;
					case EventType.Repaint:
						{
							#region Dragging Rect
							if (_dragMode != eDragType.NotDragging && _draggedObject == null)
							{
								EditorUtils.DrawSelectionRect(_dragAreaRect);
							}
							#endregion
						}
						break;
				}
			}

			private void OnMouseDown(Event inputEvent)
			{
				if (inputEvent.button == 0)
				{
					inputEvent.Use();
					_needsRepaint = true;

					OnLeftMouseDown(inputEvent);
				}
				else if (inputEvent.button == 1)
				{
					inputEvent.Use();
					OnRightMouseDown(inputEvent);
				}
				else if (inputEvent.button == 2)
				{
					inputEvent.Use();
					_needsRepaint = true;

					OnMiddleMouseDown(inputEvent);
				}
				else
				{
					_dragMode = eDragType.NotDragging;
				}
			}

			private GenericMenu GetRightMouseMenu(JSONObjectEditorGUI<T> clickedObject)
			{
				GenericMenu menu = new GenericMenu();

				if (clickedObject != null)
				{
					RightClickData copyData = new RightClickData();
					copyData._operation = eRightClickOperation.Copy;
					copyData._editableObject = clickedObject;
					menu.AddItem(new GUIContent("Copy"), false, ContextMenuCallback, copyData);
					RightClickData cutData = new RightClickData();
					cutData._operation = eRightClickOperation.Cut;
					cutData._editableObject = clickedObject;
					menu.AddItem(new GUIContent("Cut"), false, ContextMenuCallback, cutData);
					RightClickData removeData = new RightClickData();
					removeData._operation = eRightClickOperation.Remove;
					removeData._editableObject = clickedObject;
					menu.AddItem(new GUIContent("Remove"), false, ContextMenuCallback, removeData);
				}
				else
				{
					AddContextMenu(menu);

					RightClickData pasteData = null;
					if (_copiedObjects.Count > 0)
					{
						pasteData = new RightClickData();
						pasteData._operation = eRightClickOperation.Paste;
						pasteData._editableObject = clickedObject;
						menu.AddItem(new GUIContent(_copiedObjects.Count == 1 ? "Paste" : "Paste"), false, ContextMenuCallback, pasteData);
					}
				}

				return menu;
			}

			private void ContextMenuCallback(object obj)
			{
				RightClickData data = obj as RightClickData;

				if (data == null)
				{
					return;
				}

				switch (data._operation)
				{
					case eRightClickOperation.Copy:
						{
							CopySelected();
						}
						break;
					case eRightClickOperation.Paste:
						{
							Paste();
						}
						break;
					case eRightClickOperation.Cut:
						{
							CutSelected();
						}
						break;
					case eRightClickOperation.Remove:
						{
							DeleteSelected();
						}
						break;
				}
			}

			protected void CreateAndAddNewObject(Type type)
			{
				Undo.RegisterCompleteObjectUndo(this, "Create Object");
				T newObject = Activator.CreateInstance(type) as T;
				OnCreatedNewObject(newObject);
				JSONObjectEditorGUI<T> editorGUI = AddNewObject(newObject);
				GetEditorWindow().OnSelectObject(editorGUI);
				_selectedObjects.Clear();
				_selectedObjects.Add(editorGUI);
				SetObjectPosition(editorGUI, _dragPos);
			}

			private void SelectAll()
			{
				_selectedObjects.Clear();
				foreach (JSONObjectEditorGUI<T> editorGUI in _editableObjects)
				{
					_selectedObjects.Add(editorGUI);
				}
				_needsRepaint = true;
			}

			private void CopySelected()
			{
				_copiedObjects.Clear();
				foreach (JSONObjectEditorGUI<T> editorGUI in _selectedObjects)
				{
					_copiedObjects.Add(editorGUI);
				}
			}

			private void Paste()
			{
				if (_copiedObjects.Count > 0)
				{
					Undo.RegisterCompleteObjectUndo(this, "Paste Object(s)");

					Vector2 pos = ((JSONObjectEditorGUI<T>)_copiedObjects[0]).GetPosition();


					foreach (JSONObjectEditorGUI<T> editorGUI in _copiedObjects)
					{
						T copyObject = CreateCopyFrom(editorGUI);
						JSONObjectEditorGUI<T> copyEditorGUI = AddNewObject(copyObject);
						SetObjectPosition(copyEditorGUI, _dragPos + editorGUI.GetPosition() - pos);
					}

					MarkAsDirty();

					_needsRepaint = true;
				}
			}

			private void CutSelected()
			{
				if (_selectedObjects.Count > 0)
				{
					Undo.RegisterCompleteObjectUndo(this, "Cut Object(s)");

					_copiedObjects.Clear();

					List<ScriptableObject> selectedObjects = new List<ScriptableObject>(_selectedObjects);

					foreach (JSONObjectEditorGUI<T> editorGUI in selectedObjects)
					{
						_copiedObjects.Add(editorGUI);
						RemoveObject(editorGUI);
					}
					_selectedObjects.Clear();

					MarkAsDirty();

					_needsRepaint = true;
				}
			}

			private void DeleteSelected()
			{
				if (_selectedObjects.Count > 0)
				{
					Undo.RegisterCompleteObjectUndo(this, "Remove Object(s)");

					foreach (JSONObjectEditorGUI<T> editorGUI in new List<ScriptableObject>(_selectedObjects))
					{
						RemoveObject(editorGUI);
					}
					_selectedObjects.Clear();

					MarkAsDirty();

					_needsRepaint = true;
				}
			}

			private void UpdateCachedObjectList()
			{
				_cachedEditableObjects = new List<ScriptableObject>(_editableObjects);
			}

			private void UndoRedoCallback()
			{
				GetEditorWindow().DoRepaint();

				//Need way of knowing when a objects been restored from undo history
				foreach (JSONObjectEditorGUI<T> editorGUI in _editableObjects)
				{
					if (!_cachedEditableObjects.Contains(editorGUI))
					{
						OnCreatedNewObject(editorGUI.GetEditableObject());
						editorGUI.ClearUndoState();
					}
				}
			}
			#endregion
		}
	}
}