using UnityEngine;
using System;

namespace Framework
{
	using Utils;

	namespace NodeGraphSystem
	{
		[Serializable]
		public class ComponentNodeInputField<T> : NodeInputFieldBase<Component> where T : Component
		{
			public ComponentRef<T> _value;

			public static implicit operator T(ComponentNodeInputField<T> value)
			{
				return value.GetValue() as T;
			}

			protected override Component GetStaticValue()
			{
				return _value.GetComponent();
			}

#if UNITY_EDITOR
			protected override void ClearStaticValue()
			{
				_value = new ComponentRef<T>();
			}
#endif
		}
	}
}