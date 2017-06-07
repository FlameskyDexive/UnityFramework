using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		[Serializable]
		public sealed class LocalisationMap
		{
			#region Public Data
			public Dictionary<string, Dictionary<string, string>> _strings = new Dictionary<string, Dictionary<string, string>>();
			#endregion

			public string GetString(string key, SystemLanguage language)
			{
				return GetString(key, language, SystemLanguage.Unknown);
			}

			public string GetString(string key, SystemLanguage language, SystemLanguage fallBackLanguage)
			{
				string text;
				Dictionary<string, string> localisedString;

				if (!string.IsNullOrEmpty(key) && _strings.TryGetValue(key, out localisedString))
				{
					if (localisedString.TryGetValue(LanguageCodes.GetLanguageCode(language), out text))
					{
						return text;
					}
					else
					{
						Debug.Log("Can't find localised version of string " + key + " for " + language);
#if _DEBUG
						if (fallBackLanguage != SystemLanguage.Unknown && fallBackLanguage != language && 
							localisedString.TryGetValue(LanguageCodes.GetLanguageCode(fallBackLanguage), out text))
						{
							return text;
						}
						else
						{
							Debug.Log("Can't find localised version of string " + key);
						}
#endif
					}
				}

				Debug.Log(key + " isn't in localisation file.");

				return string.Empty;
			}

			public void UpdateString(string key, SystemLanguage language, string text)
			{
				if (!string.IsNullOrEmpty(key))
				{
					Dictionary<string, string> localisedString;

					if (_strings.TryGetValue(key, out localisedString))
					{
						localisedString[LanguageCodes.GetLanguageCode(language)] = text;
					}
					else
					{
						_strings.Add(key, new Dictionary<string, string>());
						_strings[key].Add(LanguageCodes.GetLanguageCode(language), text);
					}
				}
			}

			public bool IsValidKey(string key)
			{
				return _strings.ContainsKey(key);
			}

			public string[] GetStringKeys()
			{
				return _strings.Keys.ToArray();
			}
		}
	}
}