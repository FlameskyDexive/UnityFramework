using System;

namespace Framework
{
	using StateMachineSystem;
	
	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[ConditionalCategory("")]
		public class ConditionalAND : IConditional
		{
			#region Public Data
			public Condition[] _conditions = new Condition[0];
			#endregion

#if UNITY_EDITOR
			private bool[] _editorFoldout;
#endif

			#region IConditional
#if UNITY_EDITOR
			public string GetEditorDescription()
			{
				string description;

				if (_conditions != null && _conditions.Length > 0 && _conditions[0]._condition != null)
				{
					description = "(" + _conditions[0]._condition.GetEditorDescription();

					for (int i = 1; i < _conditions.Length; i++)
					{
						if (_conditions[i]._condition != null)
						{
							description += ") <b>&&</b> (";
							description += _conditions[i]._condition.GetEditorDescription();
						}
					}

					description += ")";
				}
				else
				{
					description = "(condition) && (condition)";
				}

				return description;
			}

			public bool AllowInverseVariant()
			{
				return false;
			}
#endif

			public void OnStartConditionChecking(StateMachine stateMachine)
			{
				foreach (Condition condition in _conditions)
				{
					condition._condition.OnStartConditionChecking(stateMachine);
				}
			}

			public bool IsConditionMet(StateMachine stateMachine)
			{
				bool allConditionsMet = true;

				foreach (Condition condition in _conditions)
				{
					if (!condition._condition.IsConditionMet(stateMachine))
					{
						allConditionsMet = false;
					}
				}

				return allConditionsMet;
			}

			public void OnEndConditionChecking(StateMachine stateMachine)
			{
				foreach (Condition condition in _conditions)
				{
					condition._condition.OnEndConditionChecking(stateMachine);
				}
			}
			#endregion
		}
	}
}