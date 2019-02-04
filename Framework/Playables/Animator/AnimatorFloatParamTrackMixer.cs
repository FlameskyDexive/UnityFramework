using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		public class AnimatorFloatParamTrackMixer : AnimatorParamTrackMixer<float>
		{		
			protected override object GetValue()
			{
				return _trackBinding.GetFloat(_parameterHash);
			}

			protected override object ApplyValue(object value, float inputWeight, Playable playable)
			{
				ScriptPlayable<AnimatorFloatParamPlayableBehaviour> scriptPlayable = (ScriptPlayable<AnimatorFloatParamPlayableBehaviour>)playable;
				AnimatorFloatParamPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

				value = Mathf.Lerp((float)value, inputBehaviour._value, inputWeight);
				return value;
			}

			protected override void SetValue(object value)
			{
				_trackBinding.SetFloat(_parameterHash, (float)value);
			}
		}
	}
}
