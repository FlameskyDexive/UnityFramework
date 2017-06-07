using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Float")]
		[Serializable]
		public class SquareWaveNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<float> _frequency = 0.5f;
			#endregion

			#region Private Data 
			private float _time;
			private float _value;
			#endregion

			#region Node
			public override void Init()
			{
				_time = 0.0f;
				_value = 0.0f;
			}

			public override void Update(float deltaTime)
			{
				_time += deltaTime * _frequency;
				_value = (_time % 2.0f) > 1.0f ? 1.0f : 0.0f;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return FloatNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<float>
			public float GetValue()
			{
				return _value;
			}
			#endregion
		}
	}
}