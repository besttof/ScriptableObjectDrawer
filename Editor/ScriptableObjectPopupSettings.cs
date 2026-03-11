using UnityEditor;
using UnityEngine;

namespace Besttof.ScriptableObjectDrawer.Editor
{
	[FilePath("ScriptableObjectDrawerSettings.asset", FilePathAttribute.Location.PreferencesFolder)]
	public class ScriptableObjectPopupSettings : ScriptableSingleton<ScriptableObjectPopupSettings>
	{
		[Tooltip("When enabled the object will be opened with a properties window instead of the in-place popups")]
		[SerializeField] private bool _skipInPlacePopup = false;

		public bool SkipInPlacePopup => _skipInPlacePopup;

		internal void Save()
		{
			Save(true);
		}
	}
}