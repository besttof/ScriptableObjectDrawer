using System.Collections.Generic;
using UnityEditor;

namespace Besttof.ScriptableObjectDrawer.Editor
{
	[CustomEditor(typeof(ScriptableObjectPopupSettings))]
	public class ScriptableObjectPopupSettingsEditor : UnityEditor.Editor
	{
		[SettingsProvider]
		public static SettingsProvider CreateSettingsProvider()
		{
			return new SettingsProvider("Preferences/besttof/ScriptableObjectPopupSettings", SettingsScope.User)
			       {
				       label = "ScriptableObject Popup Drawer",
				       keywords = new HashSet<string>(new[] { "scriptable", "object", "popup" }),
				       guiHandler = _ =>
				       {
					       EditorGUIUtility.labelWidth = 200f;
					       var serializedObject = new SerializedObject(ScriptableObjectPopupSettings.instance);

					       var skipPopup = serializedObject.FindProperty("_skipInPlacePopup");

					       using var changeCheck = new EditorGUI.ChangeCheckScope();

					       EditorGUILayout.PropertyField(skipPopup);

					       if (changeCheck.changed && serializedObject.ApplyModifiedProperties())
					       {
						       ScriptableObjectPopupSettings.instance.Save();
					       }
				       },
			       };
		}
	}
}