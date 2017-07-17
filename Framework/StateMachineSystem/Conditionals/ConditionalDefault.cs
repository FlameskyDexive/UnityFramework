using System;

namespace Framework
{ 
	using StateMachineSystem;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[ConditionalCategory("")]
		public class ConditionalDefault : IConditional
		{
			#region IConditional
#if UNITY_EDITOR
			public string GetEditorDescription()
			{
				return "<b>By Default</b>";
			}

			public bool AllowInverseVariant()
			{
				return false;
			}
#endif

			public void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
			}

			public bool IsConditionMet(StateMachineComponent stateMachine)
			{
				return true;
			}

			public void OnEndConditionChecking(StateMachineComponent stateMachine)
			{
			}
			#endregion
		}
	}
}