#if UNITY_EDITOR

using System;
using System.Collections;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public class StateMachineExternalState : State
		{
			[NonSerialized]
			public StateMachineEditorLink _externalStateRef;

			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				throw new NotImplementedException();
			}

			public override string GetAutoDescription()
			{
				StateRef stateRef = _externalStateRef.GetStateRef();
				return (stateRef._file._editorAsset != null ? stateRef._file._editorAsset.name : null);
			}

			public override string GetStateIdLabel()
			{
				return "External State";
			}
		}
	}
}

#endif