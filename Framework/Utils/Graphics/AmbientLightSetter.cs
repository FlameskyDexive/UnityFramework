using UnityEngine;
using UnityEngine.Rendering;

namespace Framework
{
	namespace Utils
	{
		public class AmbientLightSetter : MonoBehaviour
		{
			public AmbientMode _ambientMode = AmbientMode.Flat;
			public Color _ambientLight = Color.grey;
			public Color _ambientEquatorColor = Color.grey;
			public Color _ambientGroundColor = Color.grey;
			public Material _skyBox;

#if UNITY_EDITOR
			void OnEnable()
			{
				if (!Application.isPlaying)
					SetAmbientLight();
			}
#endif

			public void SetColor(Color color)
			{
				_ambientLight = color;

#if UNITY_EDITOR
				if (!Application.isPlaying)
					SetAmbientLight();
#endif
			}

			public void SetAmbientLight()
			{
				RenderSettings.ambientMode = _ambientMode;
				
				switch (_ambientMode)
				{
					case AmbientMode.Flat:
						RenderSettings.ambientLight = _ambientLight;
						RenderSettings.ambientSkyColor = _ambientLight;
						RenderSettings.ambientEquatorColor = _ambientLight;
						RenderSettings.ambientGroundColor = _ambientLight;
						break;
					case AmbientMode.Trilight:
						RenderSettings.ambientSkyColor = _ambientLight;
						RenderSettings.ambientEquatorColor = _ambientEquatorColor;
						RenderSettings.ambientGroundColor = _ambientGroundColor;
						break;
					case AmbientMode.Skybox:
						RenderSettings.ambientLight = _ambientLight;
						RenderSettings.ambientSkyColor = _ambientLight;
						RenderSettings.ambientEquatorColor = _ambientLight;
						RenderSettings.ambientGroundColor = _ambientLight;
						break;
				}

				if (_skyBox != null)
					RenderSettings.skybox = _skyBox;
				else
					RenderSettings.skybox = null;
			}

			public static void ClearAmbientLight()
			{
				RenderSettings.ambientMode = AmbientMode.Flat;
				RenderSettings.ambientLight = Color.black;
				RenderSettings.ambientSkyColor = Color.black;
				RenderSettings.ambientEquatorColor = Color.black;
				RenderSettings.ambientGroundColor = Color.black;
				RenderSettings.skybox = null;
			}
		}
	}
}